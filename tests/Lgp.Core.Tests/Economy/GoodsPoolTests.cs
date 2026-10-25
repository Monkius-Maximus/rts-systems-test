using Lgp.Core.Determinism;
using Lgp.Core.Economy;
using Lgp.Core.Goods;
using Lgp.Core.Production;

namespace Lgp.Core.Tests.Economy;

/// <summary>
/// Testes do pool global tier-aware — o objeto-prova do WS-0.
/// </summary>
public sealed class GoodsPoolTests
{
    private static readonly StockKey IronOre = StockKey.Untiered(GoodId.IronOre);
    private static readonly StockKey SteelTier1 = StockKey.Tiered(GoodId.Steel, ProductionTier.Tier1);
    private static readonly StockKey SteelTier3 = StockKey.Tiered(GoodId.Steel, ProductionTier.Tier3);
    private static readonly StockKey FirearmsTier1 = StockKey.Tiered(GoodId.Firearms, ProductionTier.Tier1);
    private static readonly StockKey FirearmsTier3 = StockKey.Tiered(GoodId.Firearms, ProductionTier.Tier3);

    [Fact]
    public void ANewPool_IsEmptyEverywhere()
    {
        GoodsPool pool = new();

        Assert.All(pool.Snapshot(), entry => Assert.Equal(0L, entry.Value));
    }

    [Fact]
    public void Credit_IncreasesTheBalance()
    {
        GoodsPool pool = new();

        pool.Credit(IronOre, 30);
        pool.Credit(IronOre, 12);

        Assert.Equal(42L, pool.Balance(IronOre));
    }

    /// <summary>
    /// Aço T1 e aço T3 são buckets distintos e nunca se misturam (doc 10 §4-A).
    /// </summary>
    [Fact]
    public void Tiers_AreSeparateBuckets_ForIntermediates()
    {
        GoodsPool pool = new();

        pool.Credit(SteelTier1, 100);

        Assert.Equal(100L, pool.Balance(SteelTier1));
        Assert.Equal(0L, pool.Balance(SteelTier3));
    }

    /// <summary>
    /// A opção A do recon: os finais carregam tier mesmo sem ter ladder. Um fuzil feito com
    /// aço T1 e um feito com aço T3 não podem cair no mesmo bucket.
    /// </summary>
    [Fact]
    public void Tiers_AreSeparateBuckets_ForMilitaryFinals()
    {
        GoodsPool pool = new();

        pool.Credit(FirearmsTier3, 5);

        Assert.Equal(5L, pool.Balance(FirearmsTier3));
        Assert.Equal(0L, pool.Balance(FirearmsTier1));
    }

    [Fact]
    public void Debit_OfTheExactBalance_LeavesZero()
    {
        GoodsPool pool = new();
        pool.Credit(SteelTier3, 10);

        pool.Debit(SteelTier3, 10);

        Assert.Equal(0L, pool.Balance(SteelTier3));
    }

    /// <summary>
    /// O contrato central: sem saldo, lança — e o saldo continua o que era. Nada de clamp.
    /// </summary>
    [Fact]
    public void Debit_BeyondTheBalance_Throws_AndLeavesThePoolUnchanged()
    {
        GoodsPool pool = new();
        pool.Credit(SteelTier3, 10);

        Assert.Throws<InvalidOperationException>(() => pool.Debit(SteelTier3, 11));

        Assert.Equal(10L, pool.Balance(SteelTier3));
    }

    /// <summary>
    /// Saldo de outro tier do mesmo bem não cobre o débito: a separação vale também na saída.
    /// </summary>
    [Fact]
    public void Debit_CannotBeCoveredByAnotherTierOfTheSameGood()
    {
        GoodsPool pool = new();
        pool.Credit(SteelTier1, 1_000);

        Assert.Throws<InvalidOperationException>(() => pool.Debit(SteelTier3, 1));
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    [InlineData(long.MinValue)]
    public void NonPositiveAmounts_Throw(long amount)
    {
        GoodsPool pool = new();
        pool.Credit(IronOre, 5);

        Assert.Throws<ArgumentOutOfRangeException>(() => pool.Credit(IronOre, amount));
        Assert.Throws<ArgumentOutOfRangeException>(() => pool.Debit(IronOre, amount));
        Assert.Equal(5L, pool.Balance(IronOre));
    }

    [Fact]
    public void Credit_ThatWouldOverflow_Throws_AndLeavesThePoolUnchanged()
    {
        GoodsPool pool = new();
        pool.Credit(IronOre, long.MaxValue);

        Assert.Throws<OverflowException>(() => pool.Credit(IronOre, 1));

        Assert.Equal(long.MaxValue, pool.Balance(IronOre));
    }

    /// <summary>
    /// <c>default(StockKey)</c> é a única chave que escapa da construção validada. O pool a
    /// recusa em toda operação.
    /// </summary>
    [Fact]
    public void TheDefaultKey_IsRejected()
    {
        GoodsPool pool = new();
        StockKey unbuilt = default;

        Assert.Throws<ArgumentException>(() => pool.Balance(unbuilt));
        Assert.Throws<ArgumentException>(() => pool.Credit(unbuilt, 1));
        Assert.Throws<ArgumentException>(() => pool.Debit(unbuilt, 1));
    }

    [Fact]
    public void Snapshot_FollowsTheCanonicalKeyOrder()
    {
        GoodsPool pool = new();

        StockKey[] keys = pool.Snapshot().Select(entry => entry.Key).ToArray();

        Assert.Equal(StockKey.All.ToArray(), keys);
    }

    /// <summary>
    /// O buraco de determinismo que este teste fecha: se a enumeração seguisse a ordem de
    /// inserção — como um <c>Dictionary</c> tende a seguir —, dois pools com o mesmo conteúdo
    /// enumerariam em ordens diferentes, e qualquer cálculo que acumulasse nessa ordem
    /// divergiria. Contra uma implementação ingênua, este teste falha.
    /// </summary>
    [Fact]
    public void Snapshot_DoesNotDependOnTheOrderOfOperations()
    {
        GoodsPool forward = new();
        GoodsPool backward = new();
        StockKey[] keys = StockKey.All.ToArray();

        for (int index = 0; index < keys.Length; index++)
        {
            forward.Credit(keys[index], index + 1);
        }

        for (int index = keys.Length - 1; index >= 0; index--)
        {
            backward.Credit(keys[index], index + 1);
        }

        Assert.Equal(forward.Snapshot().ToArray(), backward.Snapshot().ToArray());
    }

    [Fact]
    public void Snapshot_IsACopy()
    {
        GoodsPool pool = new();
        pool.Credit(IronOre, 7);

        IReadOnlyList<KeyValuePair<StockKey, long>> before = pool.Snapshot();
        pool.Credit(IronOre, 3);

        Assert.Equal(7L, before.Single(entry => entry.Key == IronOre).Value);
    }

    /// <summary>
    /// Conservação, numa sequência longa de operações sorteada pelo próprio gerador
    /// determinístico — reproduzível por seed. Um modelo independente (um array simples)
    /// acompanha cada operação; ao fim, pool e modelo precisam concordar bucket a bucket, e o
    /// total precisa ser o creditado menos o debitado.
    /// </summary>
    [Fact]
    public void Conservation_HoldsAcrossARandomSequenceOfOperations()
    {
        DeterministicRandom random = new(seed: 20261025);
        GoodsPool pool = new();
        StockKey[] keys = StockKey.All.ToArray();
        long[] model = new long[keys.Length];
        long credited = 0;
        long debited = 0;

        for (int operation = 0; operation < 5_000; operation++)
        {
            int slot = (int)random.NextUInt32((uint)keys.Length);
            long amount = random.NextInt32(1, 101);
            bool wantsDebit = random.NextUInt32(2) == 0;

            if (wantsDebit && model[slot] >= amount)
            {
                pool.Debit(keys[slot], amount);
                model[slot] -= amount;
                debited += amount;
            }
            else if (wantsDebit)
            {
                Assert.Throws<InvalidOperationException>(() => pool.Debit(keys[slot], amount));
            }
            else
            {
                pool.Credit(keys[slot], amount);
                model[slot] += amount;
                credited += amount;
            }
        }

        long[] actual = pool.Snapshot().Select(entry => entry.Value).ToArray();
        Assert.Equal(model, actual);
        Assert.Equal(credited - debited, actual.Sum());
    }
}
