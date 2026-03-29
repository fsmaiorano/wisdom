using System.Text.RegularExpressions;
namespace Wisdom.BuildingBlocks.RegularExpressions;

public static partial class EmailRegularExpression
{
    private const string Pattern = "^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$";
    private static Regex Regex => CreateRegex();

    [GeneratedRegex(Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex CreateRegex();

    public static bool IsMatch(string? input) => input is not null && Regex.IsMatch(input);
}