namespace Lgp.Core.Goods;

/// <summary>
/// O único lugar do núcleo que sabe o papel de cada bem, e as duas perguntas que derivam
/// dele.
/// </summary>
/// <remarks>
/// <para>
/// Cada método é um <c>switch</c> exaustivo <strong>sem braço de descarte</strong>. Isso é
/// deliberado e é o que torna este arquivo seguro de editar:
/// </para>
/// <list type="bullet">
///   <item><description>
///     acrescentar um membro a <see cref="GoodId"/> sem classificá-lo aqui é erro de
///     compilação (CS8509);
///   </description></item>
///   <item><description>
///     um valor sem nome — <c>default(GoodId)</c>, ou um inteiro convertido — lança
///     <see cref="System.Runtime.CompilerServices.SwitchExpressionException"/> em runtime,
///     sem checagem manual.
///   </description></item>
/// </list>
/// <para>
/// Um <c>_ =&gt; ...</c> silenciaria os dois. Ver o comentário sobre CS8524 no
/// <c>Lgp.Core.csproj</c>.
/// </para>
/// </remarks>
public static class GoodCatalog
{
    /// <summary>Papel do bem na cadeia produtiva, conforme o doc 10 §3.1.</summary>
    public static GoodRole RoleOf(GoodId good) => good switch
    {
        GoodId.IronOre => GoodRole.RawMaterial,
        GoodId.Coal => GoodRole.RawMaterial,
        GoodId.Timber => GoodRole.RawMaterial,
        GoodId.Saltpeter => GoodRole.RawMaterial,
        GoodId.Copper => GoodRole.RawMaterial,
        GoodId.Steel => GoodRole.Intermediate,
        GoodId.Gunpowder => GoodRole.Intermediate,
        GoodId.Armaments => GoodRole.MilitaryFinal,
        GoodId.Firearms => GoodRole.MilitaryFinal,
        GoodId.Artillery => GoodRole.MilitaryFinal,
    };

    /// <summary>
    /// Se o bem sobe de degrau por tecnologia. Só os intermediários (doc 10 §3.4).
    /// </summary>
    public static bool HasLadder(GoodId good) => RoleOf(good) switch
    {
        GoodRole.RawMaterial => false,
        GoodRole.Intermediate => true,
        GoodRole.MilitaryFinal => false,
    };

    /// <summary>
    /// Se o bem guarda, no pool, o tier em que foi produzido. Intermediários e finais
    /// militares; só os brutos ficam em bucket único (doc 10 §4-A).
    /// </summary>
    /// <remarks>
    /// Não é o mesmo que <see cref="HasLadder"/>: os finais não têm ladder, mas herdam o tier
    /// dos insumos, e num bucket único essa herança se perderia na mistura.
    /// </remarks>
    public static bool CarriesTier(GoodId good) => RoleOf(good) switch
    {
        GoodRole.RawMaterial => false,
        GoodRole.Intermediate => true,
        GoodRole.MilitaryFinal => true,
    };
}
