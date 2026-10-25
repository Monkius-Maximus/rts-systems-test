using Lgp.Core.Workforce;

namespace Lgp.Core.Tests.Workforce;

public sealed class WorkforceTypeTests
{
    /// <summary>
    /// Três tipos é guarda-corpo do doc 00 §4, não detalhe de implementação.
    /// </summary>
    [Fact]
    public void ThereAreExactlyTheThreeWorkforceTypesOfDoc10()
    {
        WorkforceType[] expected = [WorkforceType.Worker, WorkforceType.Specialist, WorkforceType.Soldier];

        Assert.Equal(expected, Enum.GetValues<WorkforceType>());
    }
}
