namespace Lgp.Core.Goods;

/// <summary>
/// Identidade dos bens do protótipo. É a definição canônica: a apresentação consome este
/// enum e nunca o redeclara.
/// </summary>
/// <remarks>
/// <para>
/// Fonte: doc 10 §3.1, que trava a lista em 10 bens. Acrescentar um membro aqui não é
/// edição de dado — é reabrir o doc 00 §4, que fixa "~12 bens" como teto.
/// </para>
/// <para>
/// Os valores são explícitos e começam em 1. O <c>0</c> fica sem membro de propósito:
/// um campo nunca inicializado vale <c>default(GoodId)</c>, e isso tem de ser um valor
/// indefinido que falha alto, não "Minério de ferro" por acaso. Valores explícitos também
/// fazem da ordem de <see cref="Enum.GetValues{TEnum}"/> uma decisão escrita aqui, e não um
/// efeito colateral da ordem de declaração — a ordem canônica do pool depende disso.
/// </para>
/// <para>
/// Os nomes são formato de save: o JSON serializa o membro pelo nome. Renomear um membro
/// depois do primeiro save exige migração.
/// </para>
/// </remarks>
public enum GoodId
{
    /// <summary>Bruto. Insumo do aço.</summary>
    IronOre = 1,

    /// <summary>Bruto. Combustível do aço e, secundariamente, da pólvora.</summary>
    Coal = 2,

    /// <summary>Bruto. Construção e naval.</summary>
    Timber = 3,

    /// <summary>Bruto e raro. Insumo da pólvora.</summary>
    Saltpeter = 4,

    /// <summary>
    /// Bruto e raro. Gargalo de <em>quantidade</em> da munição, não de tier: não altera o
    /// tier das armas de fogo, só limita quantas se fazem.
    /// </summary>
    Copper = 5,

    /// <summary>Intermediário com ladder de três degraus (bloomery → pudlagem → Bessemer).</summary>
    Steel = 6,

    /// <summary>Intermediário com ladder de três degraus.</summary>
    Gunpowder = 7,

    /// <summary>
    /// Final militar, feito de aço. Alimenta as unidades de choque e pesadas. O nome é
    /// genérico de propósito até o roster de unidades existir (doc 20 §9 #8).
    /// </summary>
    Armaments = 8,

    /// <summary>
    /// Final militar: armas de fogo <em>e</em> munição, que o doc 10 define como um bem só.
    /// Feito de aço, pólvora e cobre.
    /// </summary>
    Firearms = 9,

    /// <summary>Final militar, feito de aço e pólvora.</summary>
    Artillery = 10,
}
