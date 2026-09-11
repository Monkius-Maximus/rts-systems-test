using System.Reflection;
using Lgp.Core.Determinism;

namespace Lgp.Core.Tests;

/// <summary>
/// Guarda da única coisa que o WS-0 entrega e que qualquer commit futuro pode quebrar
/// em silêncio: a separação entre a camada de simulação e a camada de apresentação.
/// </summary>
/// <remarks>
/// Sem este teste, alguém adiciona <c>GodotSharp</c> ao núcleo para resolver um problema
/// pontual, tudo continua compilando, e só muito depois se descobre que a suíte não roda
/// mais sem a engine. O custo de descobrir isso no commit que causou é praticamente zero;
/// o custo de descobrir na Fase C não é.
/// </remarks>
public sealed class ArchitectureTests
{
    private static readonly Assembly CoreAssembly = typeof(DeterministicRandom).Assembly;

    /// <summary>
    /// O núcleo não pode depender de nenhum assembly do Godot.
    /// </summary>
    /// <remarks>
    /// A checagem é sobre as referências gravadas no manifesto, que o compilador emite
    /// para os assemblies cujos tipos são de fato usados. É exatamente a semântica que
    /// interessa: o que quebra o teste headless não é ter o pacote instalado, é o código
    /// do núcleo depender de um tipo da engine.
    /// </remarks>
    [Fact]
    public void CoreAssembly_DoesNotReferenceGodot()
    {
        string[] leaks = CoreAssembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .Where(name => name.Contains("Godot", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.True(
            leaks.Length == 0,
            $"O núcleo de simulação passou a referenciar o Godot ({string.Join(", ", leaks)}). "
            + "Isso quebra o teste headless e a separação de camadas. "
            + "O tipo da engine pertence à camada de apresentação; veja docs/recon/WS-0-recon.md §2.2.");
    }

    /// <summary>
    /// O núcleo é uma biblioteca .NET comum, sem dependência de pacote externo.
    /// </summary>
    /// <remarks>
    /// Complementa o teste acima: pega o caso em que uma dependência não-Godot entra sem
    /// decisão. Hoje a resposta certa é "nenhuma"; se um dia houver uma, que seja por uma
    /// alteração deliberada desta lista e não por acidente.
    /// </remarks>
    [Fact]
    public void CoreAssembly_OnlyReferencesTheBaseClassLibrary()
    {
        string[] allowedPrefixes = ["System.", "System", "netstandard", "mscorlib"];

        string[] unexpected = CoreAssembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .Where(name => !allowedPrefixes.Any(prefix =>
                name.Equals(prefix, StringComparison.Ordinal)
                || name.StartsWith(prefix, StringComparison.Ordinal)))
            .ToArray();

        Assert.True(
            unexpected.Length == 0,
            $"Dependência inesperada no núcleo: {string.Join(", ", unexpected)}. "
            + "O núcleo deve depender apenas da biblioteca padrão do .NET.");
    }
}
