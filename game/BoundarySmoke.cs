using Godot;
using Lgp.Core.Determinism;

/// <summary>
/// Prova, dentro da engine de verdade, que a fronteira núcleo↔apresentação funciona.
/// </summary>
/// <remarks>
/// <para>
/// Não é um "olá mundo". Ele confere que o <c>DeterministicRandom</c> produz, rodando
/// dentro do Godot, exatamente a mesma sequência que produz sob <c>dotnet test</c> —
/// os mesmos valores que vieram da implementação canônica do PCG32 em C.
/// </para>
/// <para>
/// O que isso descarta, de uma vez: que o <c>ProjectReference</c> para uma biblioteca
/// sem o SDK do Godot funcione no build mas falhe ao carregar o tipo em runtime (é um
/// defeito conhecido quando a biblioteca referenciada usa o SDK do Godot — a nossa não
/// usa, e este smoke é a verificação dessa aposta); e que o runtime da engine mude o
/// comportamento da simulação.
/// </para>
/// <para>
/// Sai com código 1 em caso de divergência, para poder ser encadeado num script ou em CI
/// sem ninguém precisar ler a saída.
/// </para>
/// </remarks>
public partial class BoundarySmoke : Node
{
    /// <summary>
    /// Sequência de referência para <c>seed=42, stream=54</c>, a mesma fixada em
    /// <c>DeterministicRandomTests</c>.
    /// </summary>
    private static readonly uint[] ExpectedDraws =
    [
        0xa15c02b7u, 0x7b47f409u, 0xba1d3330u, 0x83d2f293u,
        0xbfa4784bu, 0xcbed606eu, 0xbfc6a3adu, 0x812fff6du,
    ];

    public override void _Ready()
    {
        DeterministicRandom random = new(seed: 42, stream: 54);

        for (int index = 0; index < ExpectedDraws.Length; index++)
        {
            uint drawn = random.NextUInt32();
            if (drawn != ExpectedDraws[index])
            {
                GD.PrintErr(
                    $"A simulação divergiu dentro da engine: sorteio {index} deu "
                    + $"0x{drawn:x8}, esperado 0x{ExpectedDraws[index]:x8}.");
                GetTree().Quit(1);
                return;
            }
        }

        GD.Print($"Fronteira ok: {ExpectedDraws.Length} sorteios do núcleo conferem dentro do Godot.");
        GetTree().Quit(0);
    }
}
