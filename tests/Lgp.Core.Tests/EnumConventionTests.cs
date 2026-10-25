using System.Reflection;
using Lgp.Core.Determinism;

namespace Lgp.Core.Tests;

/// <summary>
/// Guarda das convenções que valem para <strong>todo</strong> enum público do núcleo, inclusive
/// os que ainda não existem.
/// </summary>
/// <remarks>
/// Como o <see cref="ArchitectureTests"/>, este teste existe porque a convenção pode ser
/// quebrada em silêncio por qualquer enum novo: compila, os testes do próprio enum passam, e o
/// bug só aparece quando um campo esquecido vira um valor válido. Ver
/// docs/recon/S1-03-S1-04-recon.md §1.1.
/// </remarks>
public sealed class EnumConventionTests
{
    private static readonly Type[] CoreEnums = typeof(DeterministicRandom).Assembly
        .GetTypes()
        .Where(type => type.IsEnum && type.IsPublic)
        .OrderBy(type => type.FullName, StringComparer.Ordinal)
        .ToArray();

    /// <summary>
    /// Sem este teste, um assembly sem enums faria os dois testes abaixo passarem por
    /// vacuidade.
    /// </summary>
    [Fact]
    public void TheCoreDeclaresEnums()
    {
        Assert.NotEmpty(CoreEnums);
    }

    /// <summary>
    /// <c>default</c> de todo enum do núcleo precisa ser indefinido: um campo nunca
    /// inicializado tem de falhar alto, não virar o primeiro membro por acaso.
    /// </summary>
    [Fact]
    public void NoCoreEnum_DefinesAMemberForZero()
    {
        string[] offenders = CoreEnums
            .Where(type => Enum.IsDefined(type, Activator.CreateInstance(type)!))
            .Select(type => type.FullName!)
            .ToArray();

        Assert.True(
            offenders.Length == 0,
            $"Enum com membro em 0: {string.Join(", ", offenders)}. "
            + "Comece os valores em 1 — o 0 é o valor de um campo esquecido e não pode ser válido.");
    }

    /// <summary>
    /// <see cref="Enum.GetValues(Type)"/> ordena pelo valor binário <em>sem sinal</em>: um
    /// membro negativo iria para o fim da lista, e a ordem canônica do pool deixaria de ser a
    /// escrita no enum.
    /// </summary>
    [Fact]
    public void EveryCoreEnumMember_HasAPositiveValue()
    {
        string[] offenders = CoreEnums
            .SelectMany(type => Enum.GetValues(type).Cast<object>()
                .Where(value => Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture) <= 0)
                .Select(value => $"{type.Name}.{value}"))
            .ToArray();

        Assert.True(
            offenders.Length == 0,
            $"Membros com valor não positivo: {string.Join(", ", offenders)}. "
            + "Valores explícitos e positivos mantêm a ordem de Enum.GetValues igual à escrita.");
    }
}
