namespace Lgp.Core.Goods;

/// <summary>
/// Papel de um bem na cadeia produtiva — a coluna "Papel" do doc 10 §3.1.
/// </summary>
/// <remarks>
/// É o papel, e não uma lista de bens escrita à mão, que responde "este bem tem ladder?" e
/// "este bem carrega tier no pool?". A regra mora num lugar só: <see cref="GoodCatalog"/>.
/// </remarks>
public enum GoodRole
{
    /// <summary>Sai de um depósito no território. Sem ladder e sem tier no pool.</summary>
    RawMaterial = 1,

    /// <summary>
    /// Processado de brutos. É o único papel com ladder: sobe de degrau por tecnologia.
    /// </summary>
    Intermediate = 2,

    /// <summary>
    /// Feito de intermediários e consumido pelas unidades. Sem ladder próprio, mas herda o
    /// tier dos insumos e o carrega no pool (doc 10 §4-A).
    /// </summary>
    MilitaryFinal = 3,
}
