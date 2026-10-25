using System.Runtime.CompilerServices;
using Lgp.Core.Goods;

namespace Lgp.Core.Tests.Goods;

/// <summary>
/// Testes do catálogo de bens.
/// </summary>
/// <remarks>
/// A exaustividade já é garantida em compilação — um bem sem papel não compila. Estes
/// testes seguram outra coisa: que o catálogo diz <em>o mesmo que o doc 10</em>. Se alguém
/// mudar um papel aqui sem mudar o doc, ou o contrário, a divergência aparece neste arquivo.
/// </remarks>
public sealed class GoodCatalogTests
{
    /// <summary>
    /// Espelho linha a linha da coluna "Papel" do doc 10 §3.1.
    /// </summary>
    [Theory]
    [InlineData(GoodId.IronOre, GoodRole.RawMaterial)]
    [InlineData(GoodId.Coal, GoodRole.RawMaterial)]
    [InlineData(GoodId.Timber, GoodRole.RawMaterial)]
    [InlineData(GoodId.Saltpeter, GoodRole.RawMaterial)]
    [InlineData(GoodId.Copper, GoodRole.RawMaterial)]
    [InlineData(GoodId.Steel, GoodRole.Intermediate)]
    [InlineData(GoodId.Gunpowder, GoodRole.Intermediate)]
    [InlineData(GoodId.Armaments, GoodRole.MilitaryFinal)]
    [InlineData(GoodId.Firearms, GoodRole.MilitaryFinal)]
    [InlineData(GoodId.Artillery, GoodRole.MilitaryFinal)]
    public void RoleOf_MatchesTheDoc10Table(GoodId good, GoodRole expected)
    {
        Assert.Equal(expected, GoodCatalog.RoleOf(good));
    }

    /// <summary>
    /// O doc 10 trava 10 bens. A tabela acima cobre cada um; este teste garante que não
    /// existe um décimo primeiro que a tabela esqueceu.
    /// </summary>
    [Fact]
    public void ThereAreExactlyTheTenGoodsLockedByDoc10()
    {
        Assert.True(
            Enum.GetValues<GoodId>().Length == 10,
            "O doc 10 §3.1 trava a lista em 10 bens. Acrescentar um bem é reabrir o doc 00 §4, "
            + "não editar o enum — e, se foi decisão, a tabela de RoleOf_MatchesTheDoc10Table "
            + "precisa da linha nova.");
    }

    [Fact]
    public void HasLadder_IsTrueExactlyForSteelAndGunpowder()
    {
        GoodId[] expected = [GoodId.Steel, GoodId.Gunpowder];

        GoodId[] laddered = Enum.GetValues<GoodId>().Where(GoodCatalog.HasLadder).ToArray();

        Assert.Equal(expected, laddered);
    }

    /// <summary>
    /// Os finais carregam tier sem ter ladder — a distinção que resolveu a contradição entre o
    /// doc 10 §3.4 e o §4-A. Só os brutos ficam de fora.
    /// </summary>
    [Fact]
    public void CarriesTier_IsTrueForEveryGoodExceptRawMaterials()
    {
        GoodId[] expected =
            [GoodId.Steel, GoodId.Gunpowder, GoodId.Armaments, GoodId.Firearms, GoodId.Artillery];

        GoodId[] tiered = Enum.GetValues<GoodId>().Where(GoodCatalog.CarriesTier).ToArray();

        Assert.Equal(expected, tiered);
    }

    /// <summary>
    /// Um bem com ladder sempre carrega tier. O contrário não vale (os finais).
    /// </summary>
    [Fact]
    public void EveryLadderedGood_AlsoCarriesTier()
    {
        Assert.All(
            Enum.GetValues<GoodId>().Where(GoodCatalog.HasLadder),
            good => Assert.True(
                GoodCatalog.CarriesTier(good),
                $"{good} tem ladder mas não carrega tier: o pool misturaria os degraus dele."));
    }

    /// <summary>
    /// Zero é <c>default(GoodId)</c> — um campo esquecido. 99 é um inteiro qualquer que veio
    /// de fora. Nenhum dos dois pode virar um bem válido.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    public void UndefinedGood_Throws(int raw)
    {
        GoodId undefined = (GoodId)raw;

        Assert.Throws<SwitchExpressionException>(() => GoodCatalog.RoleOf(undefined));
        Assert.Throws<SwitchExpressionException>(() => GoodCatalog.HasLadder(undefined));
        Assert.Throws<SwitchExpressionException>(() => GoodCatalog.CarriesTier(undefined));
    }
}
