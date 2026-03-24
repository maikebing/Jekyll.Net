namespace JekyllNet.Core.Translation;

public interface IAiTranslationClient
{
    Task<string> TranslateAsync(
        string sourceLanguage,
        string targetLanguage,
        string text,
        AiTextKind textKind,
        CancellationToken cancellationToken = default);
}
