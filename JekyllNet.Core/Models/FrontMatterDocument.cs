using System.Collections.Generic;

namespace JekyllNet.Core.Models;

public sealed class FrontMatterDocument
{
    public bool HasFrontMatter { get; init; }

    public Dictionary<string, object?> FrontMatter { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public string Content { get; init; } = string.Empty;
}