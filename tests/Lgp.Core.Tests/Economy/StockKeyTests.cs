using System.Runtime.CompilerServices;
using Lgp.Core.Economy;
using Lgp.Core.Goods;
using Lgp.Core.Production;

namespace Lgp.Core.Tests.Economy;

public sealed class StockKeyTests
{
    [Fact]
    public void Untiered_ForAGoodThatCarriesTier_Throws()
    {
        Assert.Throws<ArgumentException>(() => StockKey.Untiered(GoodId.Steel));
        Assert.Throws<ArgumentException>(() => StockKey.Untiered(GoodId.Firearms));
    }

    [Fact]
    public void Tiered_ForARawMaterial_Throws()
    {
        Assert.Throws<ArgumentException>(() => StockKey.Tiered(GoodId.IronOre, ProductionTier.Tier3));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public void Tiered_WithAnUndefinedTier_Throws(int raw)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => StockKey.Tiered(GoodId.Steel, (ProductionTier)raw));
    }

    [Fact]
    public void BuildingAKey_ForAnUndefinedGood_Throws()
    {
        Assert.Throws<SwitchExpressionException>(() => StockKey.Untiered((GoodId)99));
        Assert.Throws<SwitchExpressionException>(() => StockKey.Tiered((GoodId)99, ProductionTier.Tier1));
    }

    [Fact]
    public void KeysAreEqual_ExactlyWhenGoodAndTierAreEqual()
    {
        Assert.Equal(StockKey.Tiered(GoodId.Steel, ProductionTier.Tier1), StockKey.Tiered(GoodId.Steel, ProductionTier.Tier1));
        Assert.NotEqual(StockKey.Tiered(GoodId.Steel, ProductionTier.Tier1), StockKey.Tiered(GoodId.Steel, ProductionTier.Tier3));
        Assert.NotEqual(StockKey.Tiered(GoodId.Steel, ProductionTier.Tier1), StockKey.Tiered(GoodId.Gunpowder, ProductionTier.Tier1));
    }

    /// <summary>
    /// A ordem canônica, escrita por extenso. É a ordem em que o pool é percorrido e,
    /// portanto, a ordem em que ele será serializado: mudá-la muda o formato do save.
    /// </summary>
    [Fact]
    public void All_ListsEveryKeyInCanonicalOrder()
    {
        StockKey[] expected =
        [
            StockKey.Untiered(GoodId.IronOre),
            StockKey.Untiered(GoodId.Coal),
            StockKey.Untiered(GoodId.Timber),
            StockKey.Untiered(GoodId.Saltpeter),
            StockKey.Untiered(GoodId.Copper),
            StockKey.Tiered(GoodId.Steel, ProductionTier.Tier1),
            StockKey.Tiered(GoodId.Steel, ProductionTier.Tier2),
            StockKey.Tiered(GoodId.Steel, ProductionTier.Tier3),
            StockKey.Tiered(GoodId.Gunpowder, ProductionTier.Tier1),
            StockKey.Tiered(GoodId.Gunpowder, ProductionTier.Tier2),
            StockKey.Tiered(GoodId.Gunpowder, ProductionTier.Tier3),
            StockKey.Tiered(GoodId.Armaments, ProductionTier.Tier1),
            StockKey.Tiered(GoodId.Armaments, ProductionTier.Tier2),
            StockKey.Tiered(GoodId.Armaments, ProductionTier.Tier3),
            StockKey.Tiered(GoodId.Firearms, ProductionTier.Tier1),
            StockKey.Tiered(GoodId.Firearms, ProductionTier.Tier2),
            StockKey.Tiered(GoodId.Firearms, ProductionTier.Tier3),
            StockKey.Tiered(GoodId.Artillery, ProductionTier.Tier1),
            StockKey.Tiered(GoodId.Artillery, ProductionTier.Tier2),
            StockKey.Tiered(GoodId.Artillery, ProductionTier.Tier3),
        ];

        Assert.Equal(expected, StockKey.All.ToArray());
    }

    [Fact]
    public void All_HasNoDuplicates()
    {
        Assert.Equal(StockKey.All.Count, StockKey.All.Distinct().Count());
    }
}
