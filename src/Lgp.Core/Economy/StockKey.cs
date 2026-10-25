using Lgp.Core.Goods;
using Lgp.Core.Production;

namespace Lgp.Core.Economy;

/// <summary>
/// Endereço de um balde do <see cref="GoodsPool"/>: um bem e, se ele carrega tier, o tier.
/// </summary>
/// <remarks>
/// <para>
/// Só se constrói por <see cref="Untiered"/> ou <see cref="Tiered"/>, e cada um recusa a
/// combinação errada. Não existe "aço sem tier" nem "minério T3": o tipo não deixa escrever.
/// </para>
/// <para>
/// As propriedades são só de leitura, então uma expressão <c>with</c> não consegue contornar a
/// validação. Resta uma porta que nenhum struct fecha: <c>default(StockKey)</c>. O pool a
/// recusa ao usá-la — ver <see cref="GoodsPool"/>.
/// </para>
/// </remarks>
public readonly record struct StockKey
{
    private StockKey(GoodId good, ProductionTier? tier)
    {
        Good = good;
        Tier = tier;
    }

    /// <summary>O bem guardado neste balde.</summary>
    public GoodId Good { get; }

    /// <summary>
    /// O tier em que o bem foi produzido, ou <c>null</c> para um bruto, que ocupa balde único.
    /// </summary>
    public ProductionTier? Tier { get; }

    /// <summary>
    /// Todas as chaves válidas, na ordem canônica: pelo valor de <see cref="GoodId"/> e, dentro
    /// de um bem, pelo valor de <see cref="ProductionTier"/>.
    /// </summary>
    /// <remarks>
    /// É a única ordem em que o estado do pool é percorrido. Ela depende só dos valores
    /// escritos nos enums — nunca do histórico de operações —, e é isso que torna a enumeração
    /// do pool determinística.
    /// </remarks>
    public static IReadOnlyList<StockKey> All { get; } = BuildCanonicalOrder();

    /// <summary>Chave de um bem que não carrega tier — um bruto.</summary>
    /// <exception cref="ArgumentException">Se o bem carrega tier.</exception>
    public static StockKey Untiered(GoodId good)
    {
        if (GoodCatalog.CarriesTier(good))
        {
            throw new ArgumentException(
                $"{good} carries a production tier; use {nameof(StockKey)}.{nameof(Tiered)}.",
                nameof(good));
        }

        return new StockKey(good, tier: null);
    }

    /// <summary>Chave de um bem que carrega tier, no tier dado.</summary>
    /// <exception cref="ArgumentException">Se o bem não carrega tier.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Se o tier não é um membro definido.</exception>
    public static StockKey Tiered(GoodId good, ProductionTier tier)
    {
        if (!GoodCatalog.CarriesTier(good))
        {
            throw new ArgumentException(
                $"{good} does not carry a production tier; use {nameof(StockKey)}.{nameof(Untiered)}.",
                nameof(good));
        }

        if (!Enum.IsDefined(tier))
        {
            throw new ArgumentOutOfRangeException(nameof(tier), tier, "Undefined production tier.");
        }

        return new StockKey(good, tier);
    }

    private static StockKey[] BuildCanonicalOrder()
    {
        List<StockKey> keys = [];
        foreach (GoodId good in Enum.GetValues<GoodId>())
        {
            if (GoodCatalog.CarriesTier(good))
            {
                foreach (ProductionTier tier in Enum.GetValues<ProductionTier>())
                {
                    keys.Add(Tiered(good, tier));
                }
            }
            else
            {
                keys.Add(Untiered(good));
            }
        }

        return [.. keys];
    }
}
