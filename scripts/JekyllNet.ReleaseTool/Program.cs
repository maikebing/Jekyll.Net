using System.CommandLine;
using JekyllNet.ReleaseTool;

var resolveVersionCommand = CreateResolveVersionCommand();
var writeSha256Command = CreateWriteSha256Command();
var exportWingetManifestCommand = CreateExportWingetManifestCommand();

var rootCommand = new RootCommand("JekyllNet release automation helpers")
{
    resolveVersionCommand,
    writeSha256Command,
    exportWingetManifestCommand
};

return await rootCommand.Parse(args).InvokeAsync();

static Command CreateResolveVersionCommand()
{
    var projectOption = new Option<FileInfo?>("--project")
    {
        Description = "Project file used for fallback version resolution."
    };

    var githubRefTypeOption = new Option<string?>("--github-ref-type")
    {
        Description = "GitHub ref type such as 'tag' or 'branch'."
    };

    var githubRefNameOption = new Option<string?>("--github-ref-name")
    {
        Description = "GitHub ref name such as 'v0.1.0'."
    };

    var inputVersionOption = new Option<string?>("--input-version")
    {
        Description = "Optional manual version override."
    };

    var githubOutputOption = new Option<FileInfo?>("--github-output")
    {
        Description = "Optional GITHUB_OUTPUT file path."
    };

    var command = new Command("resolve-version", "Resolve package version and release tag")
    {
        projectOption,
        githubRefTypeOption,
        githubRefNameOption,
        inputVersionOption,
        githubOutputOption
    };

    command.SetAction(async parseResult =>
    {
        var settings = new ResolveVersionSettings(
            parseResult.GetValue(projectOption)?.FullName,
            parseResult.GetValue(githubRefTypeOption),
            parseResult.GetValue(githubRefNameOption),
            parseResult.GetValue(inputVersionOption),
            parseResult.GetValue(githubOutputOption)?.FullName);

        await ReleaseToolRuntime.ResolveVersionAsync(settings, parseResult.InvocationConfiguration.Output, CancellationToken.None);
    });

    return command;
}

static Command CreateWriteSha256Command()
{
    var fileOption = new Option<FileInfo>("--file")
    {
        Description = "File to hash."
    };
    fileOption.Required = true;

    var assetNameOption = new Option<string>("--asset-name")
    {
        Description = "Display name written to the checksum file."
    };
    assetNameOption.Required = true;

    var outputOption = new Option<FileInfo>("--output")
    {
        Description = "Output checksum file."
    };
    outputOption.Required = true;

    var githubOutputOption = new Option<FileInfo?>("--github-output")
    {
        Description = "Optional GITHUB_OUTPUT file path."
    };

    var githubOutputKeyOption = new Option<string?>("--github-output-key")
    {
        Description = "Optional GITHUB_OUTPUT key for the computed hash."
    };

    var command = new Command("write-sha256", "Write an uppercase SHA256 checksum file")
    {
        fileOption,
        assetNameOption,
        outputOption,
        githubOutputOption,
        githubOutputKeyOption
    };

    command.SetAction(async parseResult =>
    {
        var settings = new WriteSha256Settings(
            parseResult.GetValue(fileOption)!.FullName,
            parseResult.GetValue(assetNameOption)!,
            parseResult.GetValue(outputOption)!.FullName,
            parseResult.GetValue(githubOutputOption)?.FullName,
            parseResult.GetValue(githubOutputKeyOption));

        await ReleaseToolRuntime.WriteSha256Async(settings, parseResult.InvocationConfiguration.Output, CancellationToken.None);
    });

    return command;
}

static Command CreateExportWingetManifestCommand()
{
    var versionOption = new Option<string>("--version")
    {
        Description = "Package version."
    };
    versionOption.Required = true;

    var installerUrlOption = new Option<string>("--installer-url")
    {
        Description = "Installer URL for the generated manifest."
    };
    installerUrlOption.Required = true;

    var zipPathOption = new Option<FileInfo>("--zip-path")
    {
        Description = "Portable zip used for SHA256 generation."
    };
    zipPathOption.Required = true;

    var outputDirectoryOption = new Option<DirectoryInfo?>("--output-directory")
    {
        Description = "Output directory for generated manifests."
    };

    var command = new Command("export-winget-manifest", "Generate filled winget manifest files from templates")
    {
        versionOption,
        installerUrlOption,
        zipPathOption,
        outputDirectoryOption
    };

    command.SetAction(async parseResult =>
    {
        var settings = new ExportWingetManifestSettings(
            parseResult.GetValue(versionOption)!,
            parseResult.GetValue(installerUrlOption)!,
            parseResult.GetValue(zipPathOption)!.FullName,
            parseResult.GetValue(outputDirectoryOption)?.FullName);

        await ReleaseToolRuntime.ExportWingetManifestAsync(settings, parseResult.InvocationConfiguration.Output, CancellationToken.None);
    });

    return command;
}
