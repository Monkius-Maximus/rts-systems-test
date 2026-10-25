# S1-03 / S1-04 — Identidade do domínio e pool tier-aware · RELATÓRIO DE RECONHECIMENTO

**Projeto:** Le Grand Polemarkos (LGP) · **Frente:** WS-0 passos 4 e 6 (Fase A — Fundação)
**Rodada:** reconhecimento. **Nenhum código de produção foi escrito.**
**Data:** 2026-10-25 · **Branch:** `recon/s1-03-s1-04` (sobre `docs/vault-de-design`, PR #2)

> **Status (2026-10-25, pós-aprovação):** as cinco decisões da §7 foram tomadas conforme as
> recomendações — opção **A** para o tier dos finais; nomes da §2.2 aprovados; conversor de
> fronteira adiado para o WS-2; identidade de PM levada ao WS-1; mensagens de exceção em inglês.
> A opção A foi dobrada no doc 10 §4-A; as pendências novas viraram doc 10 §7 #9 e #10.

Material canônico desta frente: doc 10 (§3.1 bens, §3.4 ladder, §4-A topologia) e doc 30 (§9
resolução pura-C#, §9.2 determinismo). Relatório anterior: `WS-0-recon.md` — passos 1, 2, 3 e 5
já executados; não refeitos aqui.

---

## 0. Limite desta auditoria

- **A suíte xUnit não rodou localmente.** O SDK .NET 8.0.132 foi instalado nesta sessão, mas o
  `api.nuget.org` está bloqueado pela política de rede do ambiente, e o projeto de testes precisa
  de pacotes. A prova da suíte é a CI: os dois jobs `test` do commit `789aae4` estão verdes.
- **O que rodou:** um projeto de console sem pacotes, fora do repositório, para verificar
  empiricamente os comportamentos do compilador e do runtime em que este plano se apoia.
  Resultados na §6 — nenhuma afirmação técnica deste relatório é de memória.

---

## 1. Quais tipos o doc 10 exige no núcleo, e de que forma

**Regra proposta para todo o núcleo:** *identidade* é enum (conjunto fechado, checado em
compilação); *parâmetro* é arquivo de dados (WS-1). O doc 30 §3 deixou isso em aberto ("enum ou id
tipado em C# puro"); esta é a recomendação para fechá-lo.

Por que enum para identidade: o conjunto dos 10 bens está **travado por design** — acrescentar um
bem é reabrir o doc 00 §4, não editar um arquivo. Um enum faz o compilador pegar o erro que um id
textual só pegaria em runtime, e não exige nenhuma infraestrutura de catálogo no WS-0. O custo é
real e aceito: adicionar um bem exige recompilar, o que hoje é verdade de qualquer forma, porque
adicionar um bem é uma mudança de design.

| Tipo | Forma | Por quê |
|---|---|---|
| `GoodId` | enum, 10 membros | Identidade dos bens do doc 10 §3.1. Conjunto travado. |
| `GoodRole` | enum, 3 membros | A coluna "Papel" do doc 10 §3.1 (bruto / intermediário / final militar). É ela que codifica a regra "só intermediários têm ladder" — derivar, não duplicar. |
| `GoodCatalog` | classe estática, um `switch` por pergunta | O único lugar que sabe o papel de cada bem. Responde `RoleOf`, `HasLadder`, `CarriesTier`. |
| `ProductionTier` | enum, 3 membros | T1–T3 do doc 10 §3.4. Ordinal e comparável. |
| `WorkforceType` | enum, 3 membros | Trabalhador / Especialista / Soldado, doc 10 §3.2. |
| `StockKey` | `readonly record struct` | Chave do pool: bem + tier opcional, com construção validada. |
| `GoodsPool` | `sealed class` | O objeto-prova do S1-04. |
| ~~identidade de PM~~ | **adiada para o WS-1** | Ver §2.6. O pool não precisa dela, e defini-la agora obriga a inventar conteúdo que o doc 10 não tem. |

### 1.1 Três convenções que valem para todo enum do núcleo

1. **Valores explícitos, começando em 1.** `default(GoodId)` vale `0`, e `0` fica **sem membro** de
   propósito. Um campo nunca inicializado passa a ser um valor indefinido, que falha alto, em vez
   de virar silenciosamente "Minério de ferro". E os valores explícitos tornam a ordem de
   `Enum.GetValues` uma decisão escrita, não um efeito colateral da ordem de declaração (§5).
2. **`switch` exaustivo sem braço de descarte**, mais `<NoWarn>CS8524</NoWarn>` no `.csproj` do
   núcleo. O efeito combinado, verificado (§6): esquecer um membro **nomeado** é erro de compilação
   (CS8509, promovido por `TreatWarningsAsErrors`); um valor **sem nome** que chegue em runtime lança
   `SwitchExpressionException`. Um braço `_ => ...` destruiria as duas garantias — ele é, literalmente,
   um fallback.
3. **Sem membro `None`.** Ausência de valor é `null` num `Nullable<T>` onde ela é legítima, nunca um
   membro sentinela que todo `switch` precisa tratar.

### 1.2 Esboço (ilustrativo, não é o código final)

```csharp
public enum GoodId
{
    IronOre = 1, Coal = 2, Timber = 3, Saltpeter = 4, Copper = 5,   // brutos
    Steel = 6, Gunpowder = 7,                                        // intermediários
    Armaments = 8, Firearms = 9, Artillery = 10,                     // finais militares
}

public static GoodRole RoleOf(GoodId good) => good switch
{
    GoodId.IronOre or GoodId.Coal or GoodId.Timber
        or GoodId.Saltpeter or GoodId.Copper => GoodRole.RawMaterial,
    GoodId.Steel or GoodId.Gunpowder => GoodRole.Intermediate,
    GoodId.Armaments or GoodId.Firearms or GoodId.Artillery => GoodRole.MilitaryFinal,
};

public static bool HasLadder(GoodId good) => RoleOf(good) == GoodRole.Intermediate;
```

---

## 2. Lacunas, riscos e mismatches

### 2.1 [DECISÃO — bloqueia o S1-04] O doc 10 se contradiz sobre o tier dos bens finais

Duas afirmações travadas do doc 10 não podem valer juntas:

- **§3.4:** "Os finais militares **herdam o tier alcançável dos seus insumos**; o interlock converte
  isso em efetividade." E: "Não se faz fuzil Bessemer com aço de bloomery."
- **§4-A:** "O pool é tier-aware **para os bens laddered**: aço T1 e aço T3 são buckets distintos.
  **Bens sem ladder ocupam um bucket único**." E: "o interlock lê o tier de onde o bem saiu."

Os finais (armamento, armas de fogo, artilharia) **não têm ladder**. Pela §4-A, ocupam um bucket
único. Mas então um fuzil feito com aço T3 e outro feito com aço T1 caem no mesmo bucket, o tier de
origem se perde na mistura, e quando uma unidade consome fuzis do pool **não há como o interlock
saber de que tier eles eram**. A herança da §3.4 fica sem onde morar.

Isto não é detalhe de implementação: decide a **forma da chave do pool**, que é o que o S1-04 constrói.

| Opção | O que significa | Consequência |
|---|---|---|
| **A** (recomendada) | Separar dois conceitos que o doc 10 trata como um: *ter ladder* (aço, pólvora — sobem de degrau por tecnologia) e *carregar tier no pool* (aço, pólvora **e os três finais**). Só os brutos ficam em bucket único. | §3.4 e §4-A passam a valer juntas. O interlock lê o tier do final, que lê o tier dos insumos. 20 buckets (5 brutos + 5 × 3 tiers). |
| B | Finais em bucket único; a unidade usa o tier *atual* da facção. | Contradiz "o interlock lê o tier de onde o bem saiu" e **esvazia a regra de captura** (§4-A.3): capturar a fábrica Bessemer do vizinho deixa de importar para o que já está estocado. |
| C | Finais não passam pelo pool: viram unidade direto ao serem produzidos. | Muda o modelo de produção inteiro. Reabre o doc 10, não é um ajuste. |

**Recomendo A.** É a única que não apaga uma regra travada, e o custo é mínimo: o pool não precisa
saber *como* o tier é herdado, só *quais bens carregam tier*. A **regra de herança** — por exemplo,
"o tier do fuzil é o menor tier entre o aço e a pólvora que o fizeram", que é a leitura natural de
"não se faz fuzil Bessemer com aço de bloomery" — é design do WS-1, não deste passo. Registro como
pendência nova do doc 10 §7, não decido aqui.

### 2.2 [DECISÃO — bloqueia o S1-03] Os nomes dos membros viram formato de save

O save é JSON legível (doc 30 §10.1), e um enum legível é serializado **pelo nome** (verificado,
§6: `GoodId.Steel` vira `"Steel"`). Consequência: a partir do primeiro save, **renomear um membro é
uma migração de save**. Os nomes precisam ser confirmados agora, quando ainda é de graça.

Proposta (identificadores em inglês, conforme o `CLAUDE.md`):

| Doc 10 | `GoodId` | Nota |
|---|---|---|
| Minério de ferro | `IronOre` | |
| Carvão | `Coal` | |
| Madeira | `Timber` | `Wood` também serve; `Timber` é o termo de recurso no gênero |
| Salitre | `Saltpeter` | grafia americana; a britânica é `Saltpetre` |
| Cobre | `Copper` | |
| Aço | `Steel` | |
| Pólvora | `Gunpowder` | |
| Armamento | `Armaments` | ⚠️ o doc 10 não diz o que é "armamento" além de alimentar "unidades de choque/pesadas". O nome é genérico de propósito até o roster (S1-08) existir |
| Armas de fogo + munição | `Firearms` | ⚠️ **um bem só**, munição dobrada nele, como o doc 10 define. O nome não diz "munição"; se isso incomodar, `SmallArms` |
| Artilharia | `Artillery` | |

E: `GoodRole` = `RawMaterial` / `Intermediate` / `MilitaryFinal`; `ProductionTier` = `Tier1` /
`Tier2` / `Tier3`; `WorkforceType` = `Worker` / `Specialist` / `Soldier`.

### 2.3 [DECISÃO — revisa o aceite do S1-03] A fronteira com GDScript ainda não tem cliente

O aceite do S1-03 pede "teste que cobre a conversão de fronteira falhando alto em valor não
mapeável". Auditando quem atravessaria essa fronteira: **ninguém, no WS-0 nem no WS-1.**

- A apresentação em C# referencia `GoodId` direto — não há conversão nenhuma.
- O save usa nomes, não inteiros (§2.2).
- O único cliente GDScript previsto é o Better Terrain (doc 30 §9.1), e o que ele recebe são **tipos
  de terreno** — um enum que nasce no WS-2, não nenhum dos tipos deste passo.

Escrever agora um conversor `int → GoodId` para uma fronteira que nunca vai carregar `GoodId` é
código sem consumidor. **Recomendo adiar a implementação para o WS-2**, junto do primeiro enum que
de fato atravessa, e manter a regra só como texto — ela já está no doc 30 §9.1 e no `CLAUDE.md`.
Se você preferir o teste agora como forma de tornar a regra executável, é um helper genérico de
~10 linhas; a decisão é sua.

### 2.4 Nome: "pool de recursos" não pode virar `ResourcePool`

Neste repositório, `Resource` significa *asset do Godot*, e o `CLAUDE.md` dedica um parágrafo a
dizer que ele **não existe** no núcleo. Uma classe `ResourcePool` no núcleo plantaria exatamente a
confusão que o parágrafo existe para impedir. Proposta: **`GoodsPool`**.

### 2.5 Achado para o S1-06: o conversor padrão de enum do `System.Text.Json` não falha

Verificado (§6): com `new JsonStringEnumConverter()` — o construtor sem parâmetros —, ler o JSON
`99` produz `(GoodId)99`, **um valor indefinido, sem exceção**. O padrão é `allowIntegerValues: true`.
É um clamp silencioso por outro nome: um save corrompido ou editado à mão entra na simulação.

Com `allowIntegerValues: false`, inteiros **e** nomes desconhecidos lançam `JsonException`. O S1-06
precisa usar essa forma, e o teste de round-trip precisa incluir os dois casos que lançam. Fica
registrado aqui porque é consequência direta da decisão de identidade deste passo.

### 2.6 A identidade de método de produção tem buracos no doc 10

O S1-03 lista "a identidade de método de produção". Tentando escrevê-la:

- **Aço:** três PMs nomeados (bloomery, pudlagem, Bessemer/Siemens-Martin). Fechado.
- **Pólvora:** "artesanal → moinho → industrial; degraus a detalhar". Aberto.
- **Finais:** o doc 10 §4-A.2 diz "um PM por edifício", e os finais são produzidos em edifícios —
  mas não diz se cada final tem um PM ou vários.
- **Extração:** binária, sem ladder (§4-A.1). Não está escrito se um sítio de extração "roda um PM".

Definir `ProductionMethodId` agora obrigaria a inventar três desses quatro, e o `CLAUDE.md` é
explícito: não invente conteúdo do doc 10. Como o pool não precisa de PM nenhum — ele é chaveado
por bem e tier —, **recomendo tirar a identidade de PM do S1-03 e entregá-la no recon do WS-1**,
junto com a tabela de PMs. Os três buracos viram pendência no doc 10 §7.

### 2.7 Inconsistência pequena no código existente: idioma das mensagens de exceção

O `CLAUDE.md` diz: código em inglês, incluindo "mensagens de exceção de API"; PT-BR para comentários
e para mensagens de falha de **teste**. O `DeterministicRandom`, que é API pública, lança
`ArgumentOutOfRangeException` com mensagens em português.

Não quebra nada, mas o S1-03 e o S1-04 vão lançar bastante exceção, e preciso saber qual regra
seguir. Recomendo seguir o `CLAUDE.md` no código novo e alinhar o `DeterministicRandom` num commit
separado de duas linhas. A outra saída — emendar o `CLAUDE.md` para PT-BR — também é coerente.

---

## 3. Onde a regra de fronteira com GDScript aparece em código

| Quando | O quê |
|---|---|
| **Agora (S1-03)** | Nada em código, se a §2.3 for aprovada. A regra permanece no doc 30 §9.1 e no `CLAUDE.md`. As convenções da §1.1 já preparam o terreno: valores explícitos tornam o mapeamento `int ↔ enum` estável, e `default` indefinido garante que um `0` vindo de fora não vire um valor válido por acaso. |
| **WS-2** | O conversor de um ponto só, para o enum de terreno, com o teste de valor não mapeável lançando. É ali que o primeiro tipo do núcleo atravessa de fato. |

---

## 4. Plano de implementação

Um commit por item do backlog. Código em inglês, documentação e comentários em PT-BR.

### S1-03 — identidade do domínio (1 commit)

1. **`Lgp.Core.csproj`:** `<NoWarn>CS8524</NoWarn>`, com comentário dizendo por quê (§1.1, item 2).
   *Aceite:* build limpo.
2. **`GoodId`, `GoodRole`, `GoodCatalog`** (`RoleOf`, `HasLadder`, `CarriesTier`).
   *Aceite — testes:*
   - todo membro de `GoodId` tem papel (nenhum lança);
   - contagem por papel é 5 / 2 / 3, espelhando o doc 10 §3.1;
   - `HasLadder` é verdadeiro **exatamente** para `Steel` e `Gunpowder`;
   - `CarriesTier` conforme a decisão da §2.1;
   - `default(GoodId)` e `(GoodId)99` lançam.
3. **`ProductionTier`, `WorkforceType`.**
   *Aceite:* valores explícitos 1–3; `default` indefinido; teste fixando `Tier1 < Tier2 < Tier3`.

**Não entra:** identidade de PM (§2.6), conversor de fronteira (§2.3), tabela de rendimentos (WS-1).

### S1-04 — pool global tier-aware (1 commit)

4. **`StockKey`**, com duas formas de construção: uma sem tier, que lança se o bem carrega tier; uma
   com tier, que lança se o bem não carrega. Mais a **lista canônica de todas as chaves**, em ordem
   fixa — bem pelo valor do enum, depois tier.
   *Aceite:* combinação inválida lança nos dois sentidos; a lista tem 20 chaves (opção A), sem
   repetição, na ordem documentada.
5. **`GoodsPool`** — `Credit`, `Debit`, `Balance`, e uma enumeração dos saldos.
   - Quantidades em `long`, aritmética `checked`: overflow lança, não dá a volta.
   - Quantidade ≤ 0 lança. Crédito ou débito de zero é bug de quem chamou, não operação vazia.
   - **Débito sem saldo lança e não altera nada.** Atomicidade é parte do contrato: um débito que
     falhou pela metade seria o pior tipo de bug, porque o estado fica errado *e* a exceção sugere
     que nada aconteceu.
   - A enumeração dos saldos percorre **só a lista canônica**, nunca um `Dictionary`.
   *Aceite — testes:*
   - separação por tier: creditar aço T1 não altera aço T3;
   - débito sem saldo lança e o saldo continua o mesmo;
   - quantidade zero ou negativa lança; overflow lança;
   - conservação: numa sequência de operações gerada pelo próprio `DeterministicRandom`, soma
     creditada − soma debitada = soma dos saldos;
   - determinismo da ordem: dois pools que recebem os mesmos créditos em ordens diferentes
     enumeram saldos **na mesma sequência**.

**Não entra — mas fica registrado para o WS-1:** débito de **vários bens de uma vez**. Produzir um
fuzil consome aço, pólvora e cobre. Se o WS-1 fizer isso com três `Debit` seguidos e o terceiro
lançar, os dois primeiros já saíram. O WS-1 vai precisar de um débito em lote que valida tudo antes
de mexer em qualquer coisa. Não é escopo deste passo porque nada aqui produz bens ainda.

---

## 5. Onde o pool pode violar determinismo, e como o teste fecha o buraco

| Buraco | Como aparece | Como fecha |
|---|---|---|
| Ordem de iteração de `Dictionary` | Se a enumeração dos saldos percorrer um `Dictionary`, a ordem depende do histórico de inserção — e um consumidor que acumule algo nessa ordem diverge. | A enumeração só percorre a lista canônica. **Teste:** mesmos créditos em ordens diferentes → mesma sequência. Esse teste falharia contra uma implementação ingênua. |
| Ordem de `Enum.GetValues` | A lista canônica é construída a partir dele. | Ordenado pelo valor binário (sem sinal) das constantes — verificado (§6). Com valores explícitos e positivos, a ordem é a escrita no enum. **Teste** fixa a ordem da lista. |
| Ponto flutuante | Somar `float` em ordens diferentes dá resultados diferentes. | Não existe: o pool é inteiro (`long`). Taxas fracionárias, se aparecerem no WS-1, acumulam fora do pool e só entram nele como inteiro. |
| Overflow | `long` daria a volta em silêncio — resultado errado, mesmo que reproduzível. | `checked`. **Teste.** |
| Débito parcial | Ver §4, item 5. | Validar antes de mutar. **Teste.** |
| Paralelismo | Duas threads creditando o mesmo bucket. | Já proibido pela regra 3 do doc 30 §9.2. Fora do escopo do teste. |

---

## 6. Verificações feitas nesta rodada

Fonte primária mais prova empírica num projeto de console sem pacotes (SDK 8.0.132, `net8.0`,
`TreatWarningsAsErrors=true`, `NoWarn=CS8524`), fora do repositório.

| Comportamento | Resultado | Fonte |
|---|---|---|
| CS8509 × CS8524 | Membro nomeado faltando no `switch` → CS8509; valor sem nome → CS8524. Com CS8524 silenciado e sem descarte, remover o braço `Steel` deu `error CS8509 … the pattern 'Good.Steel' is not covered`. | [Pattern matching warnings](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/pattern-matching-warnings) + teste |
| Valor sem nome em runtime | `(Good)0` e `(Good)99` lançaram `System.Runtime.CompilerServices.SwitchExpressionException`, que deriva de `InvalidOperationException`. | [SwitchExpressionException](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.switchexpressionexception) + teste |
| `default` de enum começando em 1 | `Enum.IsDefined(default(Good))` → `False`. | teste |
| Ordem de `Enum.GetValues<T>()` | "Sorted by the binary values of the enumeration constants (that is, by their unsigned magnitude)." Saída: `IronOre,Coal,Steel`. | [Enum.GetValues](https://learn.microsoft.com/en-us/dotnet/api/system.enum.getvalues) + teste |
| `JsonStringEnumConverter()` padrão | Serializa `"Steel"`. **Lê `99` como `(Good)99` sem lançar.** Nome desconhecido lança. Padrão é `allowIntegerValues: true`. | [construtor](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.serialization.jsonstringenumconverter.-ctor) + teste |
| `JsonStringEnumConverter(allowIntegerValues: false)` | Inteiros (`6`, `99`) e nome desconhecido lançam `JsonException`; `"Steel"` lê normalmente. A página de referência **não documenta** a leitura; o comportamento foi estabelecido pelo teste. | teste |

---

## 7. Decisões que preciso de você antes da implementação

1. **Tier dos bens finais (§2.1):** opção A, B ou C? **Bloqueia o S1-04.** Recomendo A.
2. **Nomes dos membros (§2.2):** confirma a tabela, ou ajusta algum? **Bloqueia o S1-03** — depois
   do primeiro save, renomear custa uma migração.
3. **Conversor de fronteira GDScript (§2.3):** adia para o WS-2, ou implementa agora?
   Recomendo adiar.
4. **Identidade de PM (§2.6):** tira do S1-03 e leva para o recon do WS-1? Recomendo que sim.
5. **Idioma das mensagens de exceção (§2.7):** inglês, conforme o `CLAUDE.md`, alinhando o
   `DeterministicRandom` num commit separado? Ou emenda o `CLAUDE.md`?

Se você aprovar 1 e 2, os dois passos ficam executáveis por inteiro. 3, 4 e 5 têm recomendação
e não travam nada se você só disser "segue as recomendações".

**Parei no relatório. Aguardo aprovação explícita para a rodada de implementação.**
