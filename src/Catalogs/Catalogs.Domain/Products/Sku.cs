using System.Text.RegularExpressions;
using SharedKernel;

namespace Catalogs.Domain.Products;

public sealed record Sku
{
    private static readonly Regex ValidPattern = new(@"^[A-Z0-9\-_]+$", RegexOptions.Compiled);

    public string Value { get; }

    private Sku(string value) => Value = value;

    public static Result<Sku> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Sku>(ProductError.SkuRequired);

        value = value.ToUpperInvariant().Trim();

        if (value.Length > 50)
            return Result.Failure<Sku>(ProductError.SkuTooLong);

        if (!ValidPattern.IsMatch(value))
            return Result.Failure<Sku>(ProductError.SkuInvalidFormat);

        return Result.Success(new Sku(value));
    }

    public static Sku FromPersistence(string value) => new(value);

    public override string ToString() => Value;
}
