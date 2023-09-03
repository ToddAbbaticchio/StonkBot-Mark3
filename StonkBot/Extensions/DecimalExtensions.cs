namespace StonkBot.Extensions;

public static class DecimalExtensions
{
    public static string Clean(this decimal input)
    {
        return input.ToString("F");
    }

    public static string Clean(this decimal? input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        return Convert.ToDecimal(input).ToString("F");
    }

    public static double ToDouble(this decimal input)
    {
        return Convert.ToDouble(input);
    }
}