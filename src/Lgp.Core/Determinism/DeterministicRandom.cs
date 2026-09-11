using System.Numerics;

namespace Lgp.Core.Determinism;

/// <summary>
/// Fonte de aleatoriedade determinística da simulação: mesma semente, mesma sequência,
/// em qualquer máquina e em qualquer versão do runtime.
/// </summary>
/// <remarks>
/// <para>
/// Por que não usar o que já existe:
/// </para>
/// <list type="bullet">
///   <item><description>
///     <c>Godot.RandomNumberGenerator</c> é tipo da engine. Usá-lo aqui quebraria a
///     invariante de o núcleo não referenciar o Godot (ver doc de recon do WS-0, §2.2).
///   </description></item>
///   <item><description>
///     <c>System.Random</c> não garante algoritmo estável entre versões do .NET. Uma
///     partida salva com uma seed poderia reproduzir diferente depois de um upgrade de
///     runtime — inaceitável para replay, para o auto-combate do WS-4 e para a promessa
///     do WS-7 de as IAs rodarem a mesma engine determinística.
///   </description></item>
/// </list>
/// <para>
/// Algoritmo: PCG32 na variante XSH-RR (Melissa O'Neill, pcg-random.org, Apache-2.0),
/// portado da implementação de referência em C. A escolha é por ser especificado em
/// ~15 linhas, sem estado oculto e com stream independente — o que permite verificar o
/// port contra vetores gerados pela própria implementação canônica
/// (ver <c>DeterministicRandomTests</c>).
/// </para>
/// <para>
/// É <c>sealed class</c> e não <c>struct</c> de propósito: um struct seria copiado por
/// valor em cada passagem de parâmetro, e cada cópia continuaria a sequência por conta
/// própria. Duas partes da simulação sorteando da "mesma" fonte receberiam os mesmos
/// números sem que nada acusasse o erro. Referência elimina a classe inteira de bug.
/// </para>
/// </remarks>
public sealed class DeterministicRandom
{
    /// <summary>Multiplicador do LCG de 64 bits, fixado pela especificação do PCG32.</summary>
    private const ulong Multiplier = 6364136223846793005UL;

    private ulong _state;

    /// <summary>
    /// Incremento do LCG. Define o <em>stream</em>: é sempre ímpar e nunca muda depois
    /// de construído, então duas instâncias com streams distintos produzem sequências
    /// independentes a partir da mesma semente.
    /// </summary>
    private readonly ulong _increment;

    /// <summary>
    /// Cria um gerador a partir de uma semente e, opcionalmente, de um identificador de
    /// stream.
    /// </summary>
    /// <param name="seed">
    /// Semente da partida. O mesmo valor sempre produz a mesma sequência.
    /// </param>
    /// <param name="stream">
    /// Seletor de sequência. Serve para dar a subsistemas distintos (mapa, combate,
    /// IA) fluxos independentes da mesma semente de partida, de modo que sortear a mais
    /// num subsistema não desloque os sorteios dos outros.
    /// </param>
    public DeterministicRandom(ulong seed, ulong stream = 0UL)
    {
        // Sequência de semeadura da implementação de referência: zera o estado, fixa o
        // incremento, avança, soma a semente e avança de novo.
        _state = 0UL;
        _increment = (stream << 1) | 1UL;
        NextUInt32();
        _state += seed;
        NextUInt32();
    }

    /// <summary>
    /// Sorteia o próximo inteiro de 32 bits, uniforme sobre todo o intervalo do tipo.
    /// </summary>
    public uint NextUInt32()
    {
        ulong previousState = _state;
        _state = unchecked((previousState * Multiplier) + _increment);

        // Função de saída XSH-RR: embaralha o estado anterior e rotaciona por uma
        // quantidade que também vem do estado anterior.
        uint xorshifted = (uint)(((previousState >> 18) ^ previousState) >> 27);
        int rotation = (int)(previousState >> 59);
        return BitOperations.RotateRight(xorshifted, rotation);
    }

    /// <summary>
    /// Sorteia um inteiro em <c>[0, exclusiveBound)</c>, sem o viés que um simples
    /// resto introduziria.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Se o limite for zero.</exception>
    public uint NextUInt32(uint exclusiveBound)
    {
        if (exclusiveBound == 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(exclusiveBound),
                "O limite superior exclusivo precisa ser maior que zero.");
        }

        // Descarta a faixa inicial que não cabe num múltiplo exato do limite; o que
        // sobra é divisível e o resto passa a ser uniforme. O laço termina porque a
        // faixa aceita é sempre maior que metade do intervalo.
        uint threshold = unchecked(0u - exclusiveBound) % exclusiveBound;
        while (true)
        {
            uint value = NextUInt32();
            if (value >= threshold)
            {
                return value % exclusiveBound;
            }
        }
    }

    /// <summary>
    /// Sorteia um inteiro em <c>[minInclusive, maxExclusive)</c>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Se o intervalo for vazio.</exception>
    public int NextInt32(int minInclusive, int maxExclusive)
    {
        if (maxExclusive <= minInclusive)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxExclusive),
                $"O intervalo [{minInclusive}, {maxExclusive}) é vazio.");
        }

        uint range = (uint)((long)maxExclusive - minInclusive);
        return (int)(minInclusive + (long)NextUInt32(range));
    }
}
