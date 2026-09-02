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
            VarId = source.VarId,
            Term = CreateTerm(source),
            Rate = CreateRate(source),
            Bonus = CreateBonus(source),
            Terms = PopulateTerms(source)
        };
    }

    private static PremiumRange? CreatePremium(BeaconFixedRate source)
    {
        if (source.Minimum is null && source.Maximum is null)
        {
            return null;
        }

        return new PremiumRange
        {
            Minimum = source.Minimum,
            Maximum = source.Maximum
        };
    }

    private static RateTerm? CreateTerm(BeaconFixedRate source)
    {
        if (source.Intrateter is null && source.TermBeginDate is null && source.TermEndDate is null)
        {
            return null;
        }

        return new RateTerm
        {
            Value = source.Intrateter,
            StartDate = source.TermBeginDate,
            EndDate = source.TermEndDate
        };
    }

    private static RateValue? CreateRate(BeaconFixedRate source)
    {
        if (source.InitRate is null && source.MinEffRate is null && source.MinGrate is null)
        {
            return null;
        }

        return new RateValue
        {
            Value = source.InitRate,
            Minimum = source.MinEffRate,
            Guaranteed = source.MinGrate
        };
    }

    private static BonusRate? CreateBonus(BeaconFixedRate source)
    {
        if (source.BonusPct is null)
        {
            return null;
        }

        return new BonusRate
        {
            Value = source.BonusPct.Value,
            Term = source.BonusLen,
            Type = source.BonusType
        };
    }

    private static Terms PopulateTerms(BeaconFixedRate source)
    {
        return new Terms()
        {
            ProductType = source.ProdType,
            InterestType = source.IntType,
            Mva = source.Mva,
            Rop = source.Rop,
            Qualifier = source.Qualifier,
            BailoutRate = source.BailOutRate,
            SurrenderExpirationDate = source.SurrExpDate,
            SurrenderId = source.SurrId,
            SurrenderIncreaseDate = source.SurrIncDate,
            SurrenderYear = source.SurrYr
        };
    }
}
