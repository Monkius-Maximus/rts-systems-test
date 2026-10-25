namespace Lgp.Core.Production;

/// <summary>
/// Degrau de um método de produção (doc 10 §3.4). É ordinal: <c>Tier1 &lt; Tier2 &lt; Tier3</c>.
/// </summary>
/// <remarks>
/// <para>
/// O valor inteiro é identidade, não número de jogo. A fórmula do interlock (doc 20 §5)
/// multiplica "tiers", mas o quanto cada degrau vale em efetividade é balanceamento ainda em
/// aberto (doc 20 §9 #2). Não escreva <c>(int)tier * algo</c>: isso fixaria, em silêncio,
/// que a efetividade cresce linearmente com o degrau. O mapeamento de degrau para número
/// pertence a quem calcula efetividade, quando ela existir.
/// </para>
/// <para>
/// Mesmas convenções de <see cref="Goods.GoodId"/>: valores explícitos a partir de 1 e
/// <c>default</c> indefinido.
/// </para>
/// </remarks>
public enum ProductionTier
{
    /// <summary>Degrau inicial. No aço, a forja bloomery.</summary>
    Tier1 = 1,

    /// <summary>Degrau transicional. No aço, alto-forno com pudlagem.</summary>
    Tier2 = 2,

    /// <summary>Degrau alto. No aço, o conversor Bessemer ou Siemens-Martin.</summary>
    Tier3 = 3,
}
