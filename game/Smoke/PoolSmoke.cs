using System.Collections.Generic;
using System.Linq;
using Godot;
using Lgp.Core.Economy;
using Lgp.Core.Goods;
using Lgp.Core.Production;

namespace Lgp.Game.Smoke;

/// <summary>
/// Smoke do fio de ligação do WS-0: prova que a camada de apresentação (Godot) carrega e usa um
/// tipo da camada de simulação (<c>Lgp.Core</c>) em runtime.
/// </summary>
/// <remarks>
/// <para>
/// O risco que isto cobre não aparece em compilação: "falha em runtime na primeira vez que um tipo
/// da biblioteca referenciada é usado" (docs/pesquisa/P1-P4-verificacao-tecnica.md §2.1). Por isso
/// o smoke roda o jogo de verdade, headless, na CI.
/// </para>
/// <para>
/// Contrato com a CI (.github/workflows/ci.yml, job <c>smoke</c>): imprimir exatamente a linha
/// <c>SMOKE Steel Tier3 = 42</c> e sair com código 0. Qualquer divergência sai com código 1.
/// </para>
/// <para>
/// É provisório: some quando a cena de partida (doc 30 §4) existir.
/// </para>
/// </remarks>
public partial class PoolSmoke : Node
{
    private const long ExpectedAmount = 42;

    public override void _Ready()
    {
        StockKey steelTier3 = StockKey.Tiered(GoodId.Steel, ProductionTier.Tier3);

        GoodsPool pool = new();
        pool.Credit(steelTier3, ExpectedAmount);

        // Lê de volta pelo Snapshot, não pelo Balance: exercita a enumeração canônica, que é o que
        // a apresentação vai usar para desenhar o estoque.
        KeyValuePair<StockKey, long>[] nonEmpty = pool.Snapshot()
            .Where(entry => entry.Value != 0)
            .ToArray();

        foreach (KeyValuePair<StockKey, long> entry in nonEmpty)
        {
            GD.Print($"SMOKE {entry.Key.Good} {entry.Key.Tier} = {entry.Value}");
        }

        bool ok = nonEmpty.Length == 1
            && nonEmpty[0].Key == steelTier3
            && nonEmpty[0].Value == ExpectedAmount;

        if (!ok)
        {
            GD.PushError("SMOKE falhou: o pool lido pela apresentação não é o que foi creditado.");
        }

        GetTree().Quit(ok ? 0 : 1);
    }
}
