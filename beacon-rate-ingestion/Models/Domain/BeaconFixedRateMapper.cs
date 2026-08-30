namespace BeaconDataIngestion.Models.DataModels.DDW.Rates;

public static class BeaconFixedRateMapper
{
    public static ProductRate ToProductRate(this BeaconFixedRate source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new ProductRate
        {
            Id = $"{source.Urn}_rgp",
            ProductId = source.ProductId,
            CategoryId = source.CategoryId,
            StartDate = source.BeginDate,
            EndDate = source.EndDate,
            Premium = CreatePremium(source),
            StateAvailability = source.OverallStateAvailability,
            Term = CreateTerm(source),
            Rate = CreateRate(source),
            Bonus = CreateBonus(source),
            Terms = CreateTerms(source)
        };
    }

    private static ProductRate.PremiumRange? CreatePremium(BeaconFixedRate source)
    {
        if (source.Minimum is null && source.Maximum is null)
        {
            return null;
        }

        return new ProductRate.PremiumRange
        {
            Minimum = source.Minimum,
            Maximum = source.Maximum
        };
    }

    private static ProductRate.RateTerm? CreateTerm(BeaconFixedRate source)
    {
        if (source.Intrateter is null && source.TermBeginDate is null && source.TermEndDate is null)
        {
            return null;
        }

        return new ProductRate.RateTerm
        {
            Value = source.Intrateter,
            StartDate = source.TermBeginDate,
            EndDate = source.TermEndDate
        };
    }

    private static ProductRate.RateValue? CreateRate(BeaconFixedRate source)
    {
        if (source.InitRate is null && source.MinEffRate is null && source.MinGrate is null)
        {
            return null;
        }

        return new ProductRate.RateValue
        {
            Value = source.InitRate,
            Minimum = source.MinEffRate,
            Guaranteed = source.MinGrate
        };
    }

    private static ProductRate.BonusRate? CreateBonus(BeaconFixedRate source)
    {
        if (source.BonusPct is null)
        {
            return null;
        }

        return new ProductRate.BonusRate
        {
            Value = source.BonusPct.Value,
            Term = source.BonusLen,
            Type = source.BonusType
        };
    }

    private static Dictionary<string, object?> CreateTerms(BeaconFixedRate source) => new()
    {
        ["productType"] = source.ProdType,
        ["interestType"] = source.IntType,
        ["mva"] = source.Mva,
        ["rop"] = source.Rop,
        ["qualifier"] = source.Qualifier,
        ["bailoutRate"] = source.BailOutRate,
        ["surrenderExpirationDate"] = source.SurrExpDate,
        ["surrenderId"] = source.SurrId,
        ["surrenderIncreaseDate"] = source.SurrIncDate,
        ["surrenderYear"] = source.SurrYr
    };
}
