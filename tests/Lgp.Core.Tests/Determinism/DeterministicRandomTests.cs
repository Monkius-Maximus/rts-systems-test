using Lgp.Core.Determinism;

namespace Lgp.Core.Tests.Determinism;

/// <summary>
/// Testes do gerador determinístico.
/// </summary>
/// <remarks>
/// <para>
/// Os vetores esperados NÃO foram escritos a partir da saída do próprio port — isso só
/// provaria que o código concorda consigo mesmo. Eles foram gerados compilando a
/// implementação de referência em C do PCG32 (imneme/pcg-c-basic) e capturando a saída.
/// Os seis primeiros valores de <c>seed=42, stream=54</c> conferem também com a demo
/// publicada em pcg-random.org.
/// </para>
/// <para>
/// O que estes testes seguram: se um refactor mudar a sequência, uma partida salva por
/// seed deixa de reproduzir. Aqui isso falha na hora, com a diferença visível.
/// </para>
/// </remarks>
public sealed class DeterministicRandomTests
{
    [Fact]
    public void NextUInt32_MatchesReferenceImplementation()
    {
        uint[] expected =
        [
            0xa15c02b7u, 0x7b47f409u, 0xba1d3330u, 0x83d2f293u,
            0xbfa4784bu, 0xcbed606eu, 0xbfc6a3adu, 0x812fff6du,
        ];
        DeterministicRandom random = new(seed: 42, stream: 54);

        uint[] actual = Draw(expected.Length, random.NextUInt32);

        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Semente e stream zerados: caso de borda da semeadura, em que o estado inicial e o
    /// incremento assumem os menores valores possíveis.
    /// </summary>
    [Fact]
    public void NextUInt32_MatchesReferenceImplementation_WithZeroSeedAndStream()
    {
        uint[] expected = [0xe4c14788u, 0x379c6516u, 0x5c4ab3bbu, 0x601d23e0u];
        DeterministicRandom random = new(seed: 0, stream: 0);

        uint[] actual = Draw(expected.Length, random.NextUInt32);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void NextUInt32_WithBound_MatchesReferenceImplementation()
    {
        uint[] expected = [3, 3, 2, 1, 1, 4, 5, 3, 0, 2, 0, 1];
        DeterministicRandom random = new(seed: 42, stream: 54);

        uint[] actual = Draw(expected.Length, () => random.NextUInt32(6));

        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Limite de 2^31+1: o pior caso do descarte, que rejeita quase metade do intervalo.
    /// É o único jeito de garantir que o laço de rejeição está sendo exercitado e que o
    /// port concorda com a referência também quando ele dispara.
    /// </summary>
    [Fact]
    public void NextUInt32_WithBound_MatchesReferenceImplementation_OnTheRejectionPath()
    {
        uint[] expected = [68000201u, 2048069982u, 444572430u, 1946830702u, 1316285447u, 1181803802u];
        DeterministicRandom random = new(seed: 7, stream: 1);

        uint[] actual = Draw(expected.Length, () => random.NextUInt32(2147483649u));

        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// A propriedade que a simulação inteira depende: mesma semente, mesma partida.
    /// </summary>
    [Fact]
    public void SameSeed_ProducesTheSameSequence()
    {
        DeterministicRandom first = new(seed: 20260911);
        DeterministicRandom second = new(seed: 20260911);

        Assert.Equal(Draw(1000, first.NextUInt32), Draw(1000, second.NextUInt32));
    }

    [Fact]
    public void DifferentSeeds_ProduceDifferentSequences()
    {
        DeterministicRandom first = new(seed: 1);
        DeterministicRandom second = new(seed: 2);

        Assert.NotEqual(Draw(32, first.NextUInt32), Draw(32, second.NextUInt32));
    }

    /// <summary>
    /// Streams distintos existem para que subsistemas não interfiram entre si: sortear a
    /// mais no mapa não pode deslocar os sorteios do combate.
    /// </summary>
    [Fact]
    public void DifferentStreams_ProduceDifferentSequences()
    {
        DeterministicRandom first = new(seed: 99, stream: 0);
        DeterministicRandom second = new(seed: 99, stream: 1);

        Assert.NotEqual(Draw(32, first.NextUInt32), Draw(32, second.NextUInt32));
    }

    [Fact]
    public void NextUInt32_WithBoundOfOne_AlwaysReturnsZero()
    {
        DeterministicRandom random = new(seed: 5);

        Assert.All(Draw(64, () => random.NextUInt32(1)), value => Assert.Equal(0u, value));
    }

    [Fact]
    public void NextUInt32_WithZeroBound_Throws()
    {
        DeterministicRandom random = new(seed: 1);

        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextUInt32(0));
    }

    [Fact]
    public void NextInt32_StaysWithinTheRequestedRange()
    {
        DeterministicRandom random = new(seed: 123);

        Assert.All(
            Draw(1000, () => (uint)(random.NextInt32(-5, 5) + 5)),
            shifted => Assert.InRange(shifted, 0u, 9u));
    }

    /// <summary>
    /// Intervalo de largura 1 é válido e tem uma única resposta possível.
    /// </summary>
    [Fact]
    public void NextInt32_WithSingleValueRange_ReturnsThatValue()
    {
        DeterministicRandom random = new(seed: 8);

        Assert.All(Draw(16, () => (uint)(random.NextInt32(7, 8) - 7)), value => Assert.Equal(0u, value));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(5, 5)]
    [InlineData(5, 4)]
    [InlineData(int.MaxValue, int.MinValue)]
    public void NextInt32_WithEmptyRange_Throws(int minInclusive, int maxExclusive)
    {
        DeterministicRandom random = new(seed: 1);

        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextInt32(minInclusive, maxExclusive));
    }

    /// <summary>
    /// Intervalo de largura máxima: exercita o cálculo de faixa no limite do tipo, onde
    /// uma conta em <c>int</c> em vez de <c>long</c> estouraria.
    /// </summary>
    [Fact]
    public void NextInt32_SupportsTheFullIntegerRange()
    {
        DeterministicRandom random = new(seed: 2);

        for (int draw = 0; draw < 100; draw++)
        {
            int value = random.NextInt32(int.MinValue, int.MaxValue);
            Assert.InRange(value, int.MinValue, int.MaxValue - 1);
        }
    }

    private static uint[] Draw(int count, Func<uint> next)
    {
        uint[] values = new uint[count];
        for (int index = 0; index < count; index++)
        {
            values[index] = next();
        }

        return values;
    }
}
