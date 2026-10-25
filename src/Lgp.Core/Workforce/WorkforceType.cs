namespace Lgp.Core.Workforce;

/// <summary>
/// Os três tipos de workforce do protótipo (doc 10 §3.2), comprimidos das nove profissões do
/// LGE. Três é teto, não meta: o doc 00 §4 fixa esse número como guarda-corpo de escopo.
/// </summary>
/// <remarks>
/// Mesmas convenções de <see cref="Goods.GoodId"/>: valores explícitos a partir de 1 e
/// <c>default</c> indefinido.
/// </remarks>
public enum WorkforceType
{
    /// <summary>Mão de obra genérica. Opera a extração e os degraus baixos.</summary>
    Worker = 1,

    /// <summary>Opera os degraus altos. É o gargalo literal da assimetria de produção.</summary>
    Specialist = 2,

    /// <summary>Manpower militar. É o primeiro drenado em combate (doc 20 §7).</summary>
    Soldier = 3,
}
