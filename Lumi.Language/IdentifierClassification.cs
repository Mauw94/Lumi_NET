namespace Lumi.Language;

public enum IdentifierClassification
{
    Identifier,
    Keyword,
    ContextualKeyword,
    BooleanLiteral,
    NullLiteral,
    UndefinedLiteral
}

public static class IdentifierClassifier
{
    public static bool IsBooleanLiteral(string? value)
    {
        return value is "true" or "false";
    }

    public static bool IsContextualKeyword(string? value)
    {
        return value is "this" or "super";
    }

    public static bool IsKeywordLike(string? value)
    {
        return KeywordCatalog.Contains(value) || IsContextualKeyword(value);
    }

    public static IdentifierClassification Classify(string? value)
    {
        if (value is null)
        {
            return IdentifierClassification.Identifier;
        }

        if (value is "true" or "false")
        {
            return IdentifierClassification.BooleanLiteral;
        }

        if (value == "null")
        {
            return IdentifierClassification.NullLiteral;
        }

        if (value == "undefined")
        {
            return IdentifierClassification.UndefinedLiteral;
        }

        if (KeywordCatalog.Contains(value))
        {
            return IdentifierClassification.Keyword;
        }

        if (IsContextualKeyword(value))
        {
            return IdentifierClassification.ContextualKeyword;
        }

        return IdentifierClassification.Identifier;
    }
}