using System.Globalization;
using JekyllNet.Core.Models;

namespace JekyllNet.Core.Rendering;

public sealed partial class TemplateRenderer
{
    public string Render(string template, IReadOnlyDictionary<string, object?> variables, IReadOnlyDictionary<string, string>? includes = null)
    {
        if (string.IsNullOrEmpty(template))
        {
            return string.Empty;
        }

        var scope = new Dictionary<string, object?>(variables, StringComparer.OrdinalIgnoreCase);
        return RenderSegment(template, scope, includes);
    }

    private string RenderSegment(string template, Dictionary<string, object?> scope, IReadOnlyDictionary<string, string>? includes)
    {
        var output = new System.Text.StringBuilder();
        var index = 0;

        while (index < template.Length)
        {
            var variableStart = template.IndexOf("{{", index, StringComparison.Ordinal);
            var tagStart = template.IndexOf("{%", index, StringComparison.Ordinal);
            var nextStart = NextTokenStart(variableStart, tagStart);

            if (nextStart < 0)
            {
                output.Append(template[index..]);
                break;
            }

            output.Append(template[index..nextStart]);

            if (nextStart == variableStart)
            {
                var variableEnd = template.IndexOf("}}", variableStart + 2, StringComparison.Ordinal);
                if (variableEnd < 0)
                {
                    output.Append(template[variableStart..]);
                    break;
                }

                var expression = template[(variableStart + 2)..variableEnd].Trim();
                output.Append(ResolveExpression(expression, scope)?.ToString() ?? string.Empty);
                index = variableEnd + 2;
                continue;
            }

            var tagEnd = template.IndexOf("%}", tagStart + 2, StringComparison.Ordinal);
            if (tagEnd < 0)
            {
                output.Append(template[tagStart..]);
                break;
            }

            var tagContent = template[(tagStart + 2)..tagEnd].Trim();
            var tagName = GetTagName(tagContent);
            index = tagEnd + 2;

            switch (tagName)
            {
                case "assign":
                    ExecuteAssign(tagContent, scope);
                    break;

                case "include":
                    output.Append(RenderInclude(tagContent, scope, includes));
                    break;

                case "if":
                    output.Append(RenderIfBlock(template, tagContent, scope, includes, ref index));
                    break;

                case "for":
                    output.Append(RenderForBlock(template, tagContent, scope, includes, ref index));
                    break;
            }
        }

        return output.ToString();
    }

    private string RenderInclude(string tagContent, Dictionary<string, object?> scope, IReadOnlyDictionary<string, string>? includes)
    {
        if (includes is null || includes.Count == 0)
        {
            return string.Empty;
        }

        var includeExpression = tagContent["include".Length..].Trim();
        if (string.IsNullOrWhiteSpace(includeExpression))
        {
            return string.Empty;
        }

        var parts = includeExpression.Split(' ', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var includeName = parts[0].Trim('"', '\'');
        if (!includes.TryGetValue(includeName, out var includeTemplate))
        {
            return string.Empty;
        }

        var includeScope = new Dictionary<string, object?>(scope, StringComparer.OrdinalIgnoreCase)
        {
            ["include"] = parts.Length > 1
                ? ParseNamedArguments(parts[1], scope)
                : new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        };

        return RenderSegment(includeTemplate, includeScope, includes);
    }

    private string RenderIfBlock(
        string template,
        string tagContent,
        Dictionary<string, object?> scope,
        IReadOnlyDictionary<string, string>? includes,
        ref int index)
    {
        var condition = tagContent["if".Length..].Trim();
        var bodyStart = index;
        index = ExtractIfBranches(template, bodyStart, out var trueBranch, out var falseBranch);

        return EvaluateCondition(condition, scope)
            ? RenderSegment(trueBranch, scope, includes)
            : RenderSegment(falseBranch, scope, includes);
    }

    private string RenderForBlock(
        string template,
        string tagContent,
        Dictionary<string, object?> scope,
        IReadOnlyDictionary<string, string>? includes,
        ref int index)
    {
        var expression = tagContent["for".Length..].Trim();
        var tokens = expression.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length < 3 || !string.Equals(tokens[1], "in", StringComparison.OrdinalIgnoreCase))
        {
            index = ExtractForBody(template, index, out _);
            return string.Empty;
        }

        var itemName = tokens[0];
        var collectionPath = string.Join(' ', tokens.Skip(2));
        index = ExtractForBody(template, index, out var body);

        if (!TryResolveObject(scope, collectionPath, out var resolved))
        {
            return string.Empty;
        }

        var sequence = resolved switch
        {
            IEnumerable<JekyllContentItem> typedEnumerable => typedEnumerable.Cast<object?>(),
            IEnumerable<object?> enumerable => enumerable,
            _ => Array.Empty<object?>()
        };

        var output = new System.Text.StringBuilder();
        foreach (var item in sequence)
        {
            var iterationScope = new Dictionary<string, object?>(scope, StringComparer.OrdinalIgnoreCase)
            {
                [itemName] = item switch
                {
                    JekyllContentItem contentItem => ToLiquidObject(contentItem),
                    _ => item
                }
            };

            output.Append(RenderSegment(body, iterationScope, includes));
        }

        return output.ToString();
    }

    private static int ExtractForBody(string template, int startIndex, out string body)
    {
        var depth = 0;
        var cursor = startIndex;

        while (TryFindTag(template, cursor, out var tagStart, out var tagEnd, out var tagContent))
        {
            var tagName = GetTagName(tagContent);
            if (string.Equals(tagName, "for", StringComparison.OrdinalIgnoreCase))
            {
                depth++;
            }
            else if (string.Equals(tagName, "endfor", StringComparison.OrdinalIgnoreCase))
            {
                if (depth == 0)
                {
                    body = template[startIndex..tagStart];
                    return tagEnd + 2;
                }

                depth--;
            }

            cursor = tagEnd + 2;
        }

        body = template[startIndex..];
        return template.Length;
    }

    private static int ExtractIfBranches(string template, int startIndex, out string trueBranch, out string falseBranch)
    {
        var depth = 0;
        var cursor = startIndex;
        var elseContentStart = -1;
        var elseTagStart = -1;

        while (TryFindTag(template, cursor, out var tagStart, out var tagEnd, out var tagContent))
        {
            var tagName = GetTagName(tagContent);
            if (string.Equals(tagName, "if", StringComparison.OrdinalIgnoreCase))
            {
                depth++;
            }
            else if (string.Equals(tagName, "endif", StringComparison.OrdinalIgnoreCase))
            {
                if (depth == 0)
                {
                    trueBranch = elseTagStart >= 0
                        ? template[startIndex..elseTagStart]
                        : template[startIndex..tagStart];
                    falseBranch = elseContentStart >= 0
                        ? template[elseContentStart..tagStart]
                        : string.Empty;
                    return tagEnd + 2;
                }

                depth--;
            }
            else if (string.Equals(tagName, "else", StringComparison.OrdinalIgnoreCase) && depth == 0 && elseTagStart < 0)
            {
                elseTagStart = tagStart;
                elseContentStart = tagEnd + 2;
            }

            cursor = tagEnd + 2;
        }

        trueBranch = template[startIndex..];
        falseBranch = string.Empty;
        return template.Length;
    }

    private static void ExecuteAssign(string tagContent, Dictionary<string, object?> scope)
    {
        var expression = tagContent["assign".Length..].Trim();
        var separator = expression.IndexOf('=');
        if (separator < 0)
        {
            return;
        }

        var key = expression[..separator].Trim();
        var valueExpression = expression[(separator + 1)..].Trim();
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        scope[key] = ResolveExpression(valueExpression, scope);
    }

    private static Dictionary<string, object?> ParseNamedArguments(string input, IReadOnlyDictionary<string, object?> variables)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(input))
        {
            return result;
        }

        var matches = NamedArgumentPattern().Matches(input);
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            result[match.Groups[1].Value.Trim()] = ResolveExpression(match.Groups[2].Value.Trim(), variables);
        }

        return result;
    }

    private static object? ResolveExpression(string expression, IReadOnlyDictionary<string, object?> variables)
    {
        var parts = expression.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        object? current = ResolveBase(parts[0], variables);

        foreach (var filterPart in parts.Skip(1))
        {
            var filterTokens = filterPart.Split(':', 2, StringSplitOptions.TrimEntries);
            current = ApplyFilter(current, filterTokens[0], filterTokens.Length > 1 ? ResolveBase(filterTokens[1], variables)?.ToString() : null);
        }

        return current;
    }

    private static object? ResolveBase(string expression, IReadOnlyDictionary<string, object?> variables)
    {
        if ((expression.StartsWith('"') && expression.EndsWith('"')) || (expression.StartsWith('\'') && expression.EndsWith('\'')))
        {
            return expression[1..^1];
        }

        if (int.TryParse(expression, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
        {
            return intValue;
        }

        if (bool.TryParse(expression, out var boolValue))
        {
            return boolValue;
        }

        return TryResolveObject(variables, expression, out var value) ? value : null;
    }

    private static object? ApplyFilter(object? value, string filterName, string? argument)
    {
        return filterName switch
        {
            "upcase" => value?.ToString()?.ToUpperInvariant(),
            "downcase" => value?.ToString()?.ToLowerInvariant(),
            "default" => string.IsNullOrWhiteSpace(value?.ToString()) ? argument : value,
            "date" => ApplyDateFilter(value, argument),
            "size" => ApplySizeFilter(value),
            "join" => ApplyJoinFilter(value, argument),
            "split" => ApplySplitFilter(value, argument),
            "strip" => value?.ToString()?.Trim(),
            "append" => (value?.ToString() ?? string.Empty) + (argument ?? string.Empty),
            "prepend" => (argument ?? string.Empty) + (value?.ToString() ?? string.Empty),
            "replace" => ApplyReplaceFilter(value, argument),
            "first" => ApplyFirstFilter(value),
            "last" => ApplyLastFilter(value),
            _ => value
        };
    }

    private static object ApplySizeFilter(object? value)
    {
        return value switch
        {
            null => 0,
            string text => text.Length,
            IEnumerable<object?> sequence => sequence.Count(),
            _ => value.ToString()?.Length ?? 0
        };
    }

    private static object ApplyJoinFilter(object? value, string? argument)
    {
        if (value is IEnumerable<object?> sequence)
        {
            return string.Join(argument ?? string.Empty, sequence.Select(x => x?.ToString() ?? string.Empty));
        }

        return value?.ToString() ?? string.Empty;
    }

    private static object ApplySplitFilter(object? value, string? argument)
    {
        return (value?.ToString() ?? string.Empty)
            .Split(argument ?? string.Empty, StringSplitOptions.None)
            .Cast<object?>()
            .ToList();
    }

    private static object ApplyReplaceFilter(object? value, string? argument)
    {
        var source = value?.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(argument))
        {
            return source;
        }

        var parts = argument.Split(',', 2, StringSplitOptions.TrimEntries);
        var oldValue = TrimQuotes(parts.ElementAtOrDefault(0) ?? string.Empty);
        var newValue = TrimQuotes(parts.ElementAtOrDefault(1) ?? string.Empty);
        return source.Replace(oldValue, newValue, StringComparison.Ordinal);
    }

    private static object? ApplyFirstFilter(object? value)
    {
        return value switch
        {
            string text when text.Length > 0 => text[0].ToString(),
            IEnumerable<object?> sequence => sequence.FirstOrDefault(),
            _ => value
        };
    }

    private static object? ApplyLastFilter(object? value)
    {
        return value switch
        {
            string text when text.Length > 0 => text[^1].ToString(),
            IEnumerable<object?> sequence => sequence.LastOrDefault(),
            _ => value
        };
    }

    private static string? ApplyDateFilter(object? value, string? argument)
    {
        if (value is null)
        {
            return null;
        }

        if (value is DateTimeOffset dto)
        {
            return dto.ToString(NormalizeDateFormat(argument), CultureInfo.InvariantCulture);
        }

        if (DateTimeOffset.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out dto))
        {
            return dto.ToString(NormalizeDateFormat(argument), CultureInfo.InvariantCulture);
        }

        return value.ToString();
    }

    private static string NormalizeDateFormat(string? liquidFormat)
    {
        if (string.IsNullOrWhiteSpace(liquidFormat))
        {
            return "yyyy-MM-dd";
        }

        return liquidFormat
            .Replace("%Y", "yyyy", StringComparison.Ordinal)
            .Replace("%m", "MM", StringComparison.Ordinal)
            .Replace("%d", "dd", StringComparison.Ordinal)
            .Replace("%H", "HH", StringComparison.Ordinal)
            .Replace("%M", "mm", StringComparison.Ordinal)
            .Replace("%S", "ss", StringComparison.Ordinal)
            .Replace("%b", "MMM", StringComparison.Ordinal);
    }

    private static string TrimQuotes(string value)
    {
        if ((value.StartsWith('"') && value.EndsWith('"')) || (value.StartsWith('\'') && value.EndsWith('\'')))
        {
            return value[1..^1];
        }

        return value;
    }

    private static bool EvaluateCondition(string expression, IReadOnlyDictionary<string, object?> variables)
    {
        if (expression.Contains("==", StringComparison.Ordinal))
        {
            var parts = expression.Split("==", 2, StringSplitOptions.TrimEntries);
            return string.Equals(ResolveExpression(parts[0], variables)?.ToString(), ResolveExpression(parts[1], variables)?.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        if (expression.Contains("!=", StringComparison.Ordinal))
        {
            var parts = expression.Split("!=", 2, StringSplitOptions.TrimEntries);
            return !string.Equals(ResolveExpression(parts[0], variables)?.ToString(), ResolveExpression(parts[1], variables)?.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        var value = ResolveExpression(expression, variables);
        return value switch
        {
            null => false,
            bool b => b,
            string s when bool.TryParse(s, out var boolString) => boolString,
            string s => !string.IsNullOrWhiteSpace(s),
            IEnumerable<object?> e => e.Any(),
            _ => true
        };
    }

    private static bool TryResolveObject(IReadOnlyDictionary<string, object?> variables, string path, out object? value)
    {
        object? current = variables;

        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (current is IReadOnlyDictionary<string, object?> dict && dict.TryGetValue(segment, out var next))
            {
                current = next;
                continue;
            }

            if (current is Dictionary<string, object?> raw && raw.TryGetValue(segment, out next))
            {
                current = next;
                continue;
            }

            value = null;
            return false;
        }

        value = current;
        return true;
    }

    private static Dictionary<string, object?> ToLiquidObject(JekyllContentItem item)
        => new(item.FrontMatter, StringComparer.OrdinalIgnoreCase)
        {
            ["content"] = item.RenderedContent,
            ["url"] = item.Url,
            ["path"] = item.RelativePath.Replace('\\', '/'),
            ["collection"] = item.Collection,
            ["date"] = item.Date,
            ["title"] = item.FrontMatter.TryGetValue("title", out var title) ? title : Path.GetFileNameWithoutExtension(item.RelativePath)
        };

    private static bool TryFindTag(string template, int startIndex, out int tagStart, out int tagEnd, out string tagContent)
    {
        tagStart = template.IndexOf("{%", startIndex, StringComparison.Ordinal);
        if (tagStart < 0)
        {
            tagEnd = -1;
            tagContent = string.Empty;
            return false;
        }

        tagEnd = template.IndexOf("%}", tagStart + 2, StringComparison.Ordinal);
        if (tagEnd < 0)
        {
            tagContent = string.Empty;
            return false;
        }

        tagContent = template[(tagStart + 2)..tagEnd].Trim();
        return true;
    }

    private static string GetTagName(string tagContent)
    {
        var firstSpace = tagContent.IndexOf(' ');
        return firstSpace >= 0 ? tagContent[..firstSpace] : tagContent;
    }

    private static int NextTokenStart(int variableStart, int tagStart)
    {
        if (variableStart < 0)
        {
            return tagStart;
        }

        if (tagStart < 0)
        {
            return variableStart;
        }

        return Math.Min(variableStart, tagStart);
    }

    [System.Text.RegularExpressions.GeneratedRegex("(\\w+)\\s*=\\s*(\".*?\"|'.*?'|[^\\s]+)", System.Text.RegularExpressions.RegexOptions.Compiled)]
    private static partial System.Text.RegularExpressions.Regex NamedArgumentPattern();
}
