using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace JekyllNet.ReleaseTool;

internal sealed record ResolveVersionSettings(
    string? ProjectPath,
    string? GitHubRefType,
    string? GitHubRefName,
    string? InputVersion,
    string? GitHubOutputPath);

internal sealed record WriteSha256Settings(
    string FilePath,
    string AssetName,
    string OutputPath,
    string? GitHubOutputPath,
    string? GitHubOutputKey);

internal sealed record ExportWingetManifestSettings(
    string Version,
    string InstallerUrl,
    string ZipPath,
    string? OutputDirectory);

internal sealed record ResolvedVersionInfo(string PackageVersion, string ReleaseTag);

internal static class ReleaseToolRuntime
{
    private static readonly Regex SemVerRegex = new("^[0-9]+\\.[0-9]+\\.[0-9]+([-.][0-9A-Za-z.-]+)?$", RegexOptions.Compiled);

    public static async Task<ResolvedVersionInfo> ResolveVersionAsync(
        ResolveVersionSettings settings,
        TextWriter output,
        CancellationToken cancellationToken)
    {
        var version = ResolveVersion(settings);
        var releaseTag = ResolveReleaseTag(settings, version);
        ValidateVersion(version);

        if (!string.IsNullOrWhiteSpace(settings.GitHubOutputPath))
        {
            await AppendGitHubOutputAsync(settings.GitHubOutputPath, new Dictionary<string, string>
            {
                ["package_version"] = version,
                ["release_tag"] = releaseTag
            }, cancellationToken);
        }

        await output.WriteLineAsync($"Resolved package version: {version}");
        await output.WriteLineAsync($"Resolved release tag: {releaseTag}");
        return new ResolvedVersionInfo(version, releaseTag);
    }

    public static async Task<string> WriteSha256Async(
        WriteSha256Settings settings,
        TextWriter output,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.FilePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.AssetName);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.OutputPath);

        var hash = await ComputeSha256Async(settings.FilePath, cancellationToken);
        var outputDirectory = Path.GetDirectoryName(settings.OutputPath);
        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        await File.WriteAllTextAsync(settings.OutputPath, $"{hash}  {settings.AssetName}{Environment.NewLine}", cancellationToken);

        if (!string.IsNullOrWhiteSpace(settings.GitHubOutputPath) && !string.IsNullOrWhiteSpace(settings.GitHubOutputKey))
        {
            await AppendGitHubOutputAsync(settings.GitHubOutputPath, new Dictionary<string, string>
            {
                [settings.GitHubOutputKey] = hash
            }, cancellationToken);
        }

        await output.WriteLineAsync($"SHA256 ({settings.AssetName}): {hash}");
        return hash;
    }

    public static async Task<string> ExportWingetManifestAsync(
        ExportWingetManifestSettings settings,
        TextWriter output,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.Version);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.InstallerUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.ZipPath);
        ValidateVersion(settings.Version);

        var repoRoot = FindRepoRoot();
        var templateDirectory = Path.Combine(repoRoot, "packaging", "winget", "templates");
        if (!Directory.Exists(templateDirectory))
        {
            throw new DirectoryNotFoundException($"Winget template directory not found: {templateDirectory}");
        }

        var outputDirectory = settings.OutputDirectory;
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            outputDirectory = Path.Combine(repoRoot, "artifacts", "winget");
        }

        var manifestDirectory = Path.Combine(outputDirectory, "JekyllNet.JekyllNet", settings.Version);
        Directory.CreateDirectory(manifestDirectory);

        var zipSha256 = await ComputeSha256Async(settings.ZipPath, cancellationToken);
        var replacements = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["__VERSION__"] = settings.Version,
            ["__WINDOWS_X64_ZIP_URL__"] = settings.InstallerUrl,
            ["__WINDOWS_X64_ZIP_SHA256__"] = zipSha256
        };

        foreach (var templatePath in Directory.EnumerateFiles(templateDirectory, "*.yaml", SearchOption.TopDirectoryOnly))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var content = await File.ReadAllTextAsync(templatePath, cancellationToken);
            foreach (var replacement in replacements)
            {
                content = content.Replace(replacement.Key, replacement.Value, StringComparison.Ordinal);
            }

            var targetPath = Path.Combine(manifestDirectory, Path.GetFileName(templatePath));
            await File.WriteAllTextAsync(targetPath, content, cancellationToken);
        }

        var resolvedManifestDirectory = Path.GetFullPath(manifestDirectory);
        await output.WriteLineAsync($"Generated manifests: {resolvedManifestDirectory}");
        await output.WriteLineAsync($"Installer URL: {settings.InstallerUrl}");
        await output.WriteLineAsync($"Installer SHA256: {zipSha256}");
        return resolvedManifestDirectory;
    }

    private static string ResolveVersion(ResolveVersionSettings settings)
    {
        if (string.Equals(settings.GitHubRefType, "tag", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(settings.GitHubRefName))
        {
            return settings.GitHubRefName.StartsWith('v')
                ? settings.GitHubRefName[1..]
                : settings.GitHubRefName;
        }

        if (!string.IsNullOrWhiteSpace(settings.InputVersion))
        {
            return settings.InputVersion;
        }

        if (string.IsNullOrWhiteSpace(settings.ProjectPath))
        {
            throw new InvalidOperationException("Could not resolve a package version because no project path was provided.");
        }

        var project = XDocument.Load(settings.ProjectPath);
        var version = project.Descendants("Version").Select(static element => element.Value).FirstOrDefault();
        if (string.IsNullOrWhiteSpace(version))
        {
            throw new InvalidOperationException($"Could not resolve a package version from project file '{settings.ProjectPath}'.");
        }

        return version;
    }

    private static string ResolveReleaseTag(ResolveVersionSettings settings, string version)
    {
        if (string.Equals(settings.GitHubRefType, "tag", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(settings.GitHubRefName))
        {
            return settings.GitHubRefName;
        }

        return $"v{version}";
    }

    private static void ValidateVersion(string version)
    {
        if (!SemVerRegex.IsMatch(version))
        {
            throw new InvalidOperationException($"Version '{version}' is not a valid SemVer-style release version.");
        }
    }

    private static async Task<string> ComputeSha256Async(string filePath, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(stream, cancellationToken);
        return Convert.ToHexString(hash);
    }

    private static async Task AppendGitHubOutputAsync(
        string githubOutputPath,
        IReadOnlyDictionary<string, string> values,
        CancellationToken cancellationToken)
    {
        var outputDirectory = Path.GetDirectoryName(githubOutputPath);
        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        await using var stream = new FileStream(githubOutputPath, FileMode.Append, FileAccess.Write, FileShare.Read);
        await using var writer = new StreamWriter(stream);
        foreach (var pair in values)
        {
            await writer.WriteLineAsync($"{pair.Key}={pair.Value}".AsMemory(), cancellationToken);
        }
    }

    private static string FindRepoRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "JekyllNet.slnx")))
            {
                return current;
            }

            var parent = Directory.GetParent(current);
            if (parent is null)
            {
                break;
            }

            current = parent.FullName;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from the current application base directory.");
    }
}
