using Lgp.Core.Production;

namespace Lgp.Core.Tests.Production;

public sealed class ProductionTierTests
{
    [Fact]
    public void ThereAreExactlyThreeTiers_InAscendingOrder()
    {
        ProductionTier[] expected = [ProductionTier.Tier1, ProductionTier.Tier2, ProductionTier.Tier3];

        Assert.Equal(expected, Enum.GetValues<ProductionTier>());
    }

    /// <summary>
    /// O doc 40 compara degraus ("tier inicial ≤ teto do ladder"). A comparação entre
    /// membros do enum precisa seguir a ordem dos degraus.
    /// </summary>
    [Fact]
    public void Tiers_CompareInLadderOrder()
    {
        Assert.True(ProductionTier.Tier1 < ProductionTier.Tier2);
        Assert.True(ProductionTier.Tier2 < ProductionTier.Tier3);
    }
}
