namespace Lgp.Core.Economy;

/// <summary>
/// O estoque global de bens de uma facção (doc 10 §4-A): um pool único, sem estoque por
/// província e sem transporte. É o que é local — a capacidade de produção — que fica fora daqui.
/// </summary>
/// <remarks>
/// <para>
/// Contrato, e por que cada item existe:
/// </para>
/// <list type="bullet">
///   <item><description>
///     <strong>Débito sem saldo lança e não altera nada.</strong> Não há clamp: corrigir para zero
///     em silêncio é um fallback disfarçado, e o estado ficaria errado sem nenhum sinal.
///   </description></item>
///   <item><description>
///     <strong>Toda operação que lança deixa o pool como estava.</strong> Um débito ou crédito que
///     falhasse pela metade seria o pior tipo de bug: o estado errado <em>e</em> uma exceção que
///     sugere que nada aconteceu.
///   </description></item>
///   <item><description>
///     <strong>Quantidade zero ou negativa lança.</strong> Mover zero não é operação vazia, é bug de
///     quem chamou; e um crédito negativo seria um débito que pula a checagem de saldo.
///   </description></item>
///   <item><description>
///     <strong>Overflow lança</strong> (aritmética <c>checked</c>). Um <c>long</c> que desse a volta
///     produziria um saldo negativo reproduzível — determinístico e errado.
///   </description></item>
///   <item><description>
///     <strong>Quantidades são inteiras.</strong> Somar ponto flutuante em ordens diferentes dá
///     resultados diferentes; inteiro não. Taxas fracionárias, quando existirem, acumulam fora do
///     pool e entram nele como inteiro.
///   </description></item>
/// </list>
/// <para>
/// Determinismo: os saldos vivem num array na ordem canônica de <see cref="StockKey.All"/>, e
/// <see cref="Snapshot"/> percorre só esse array. O dicionário de índices serve apenas para
/// <em>localizar</em> um balde e nunca é iterado — a ordem dele não alcança nenhum cálculo.
/// </para>
/// </remarks>
public sealed class GoodsPool
{
    private static readonly IReadOnlyDictionary<StockKey, int> IndexByKey = StockKey.All
        .Select((key, index) => (key, index))
        .ToDictionary(entry => entry.key, entry => entry.index);

    private readonly long[] _balances = new long[StockKey.All.Count];

    /// <summary>Saldo atual do balde. Um balde que nunca recebeu nada tem saldo zero.</summary>
    /// <exception cref="ArgumentException">Se a chave não foi construída por <see cref="StockKey"/>.</exception>
    public long Balance(StockKey key) => _balances[IndexOf(key)];

    /// <summary>Acrescenta ao balde.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Se a quantidade não é positiva.</exception>
    /// <exception cref="ArgumentException">Se a chave não foi construída por <see cref="StockKey"/>.</exception>
    /// <exception cref="OverflowException">Se o saldo resultante não cabe num <c>long</c>.</exception>
    public void Credit(StockKey key, long amount)
    {
        RequirePositive(amount);
        int index = IndexOf(key);

        // A soma é avaliada antes da atribuição: se ela estourar, o saldo não é tocado.
        _balances[index] = checked(_balances[index] + amount);
    }

    /// <summary>Retira do balde.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Se a quantidade não é positiva.</exception>
    /// <exception cref="ArgumentException">Se a chave não foi construída por <see cref="StockKey"/>.</exception>
    /// <exception cref="InvalidOperationException">Se o saldo é menor que a quantidade.</exception>
    public void Debit(StockKey key, long amount)
    {
        RequirePositive(amount);
        int index = IndexOf(key);

        long balance = _balances[index];
        if (balance < amount)
        {
            throw new InvalidOperationException(
                $"Insufficient balance for {key}: requested {amount}, available {balance}.");
        }

        _balances[index] = balance - amount;
    }

    /// <summary>
    /// Cópia de todos os saldos, inclusive os zerados, na ordem canônica de
    /// <see cref="StockKey.All"/>.
    /// </summary>
    /// <remarks>
    /// A ordem não depende da ordem em que as operações aconteceram. É uma cópia: alterar o pool
    /// depois não altera o resultado já devolvido.
    /// </remarks>
    public IReadOnlyList<KeyValuePair<StockKey, long>> Snapshot()
    {
        var entries = new KeyValuePair<StockKey, long>[_balances.Length];
        for (int index = 0; index < _balances.Length; index++)
        {
            entries[index] = new(StockKey.All[index], _balances[index]);
        }

        return entries;
    }

    private static int IndexOf(StockKey key)
    {
        if (!IndexByKey.TryGetValue(key, out int index))
        {
            // Única forma de chegar aqui: default(StockKey), a porta que nenhum struct fecha.
            throw new ArgumentException(
                $"{key} is not a valid stock key; build it with {nameof(StockKey)}.{nameof(StockKey.Untiered)} "
                + $"or {nameof(StockKey)}.{nameof(StockKey.Tiered)}.",
                nameof(key));
        }

        return index;
    }

    private static void RequirePositive(long amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be positive.");
        }
    }
}
