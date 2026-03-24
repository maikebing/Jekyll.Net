namespace JekyllNet.Core.Models;

public sealed class JekyllStaticFile
{
    public string SourcePath { get; init; } = string.Empty;

    public string RelativePath { get; init; } = string.Empty;

    public string OutputRelativePath { get; init; } = string.Empty;

    public string Url { get; init; } = string.Empty;

    public string Content { get; init; } = string.Empty;

    public Dictionary<string, object?> FrontMatter { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public bool HasFrontMatter { get; init; }
}
