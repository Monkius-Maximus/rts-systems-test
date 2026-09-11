# WS-0 — Núcleo de simulação · RELATÓRIO DE RECONHECIMENTO

**Projeto:** Le Grand Polemarkos (LGP) · **Frente:** WS-0 (Fase A — Fundação)
**Rodada:** reconhecimento. **Nenhum código de produção foi escrito.**
**Data:** 2026-09-11 · **Branch:** `claude/dreamy-meitner-g30tx0`

Pré-requisito do mapa (§3, WS-0: "Antes: rodar Pesquisa P1 e P4") cumprido —
ver `docs/pesquisa/P1-P4-verificacao-tecnica.md`.

---

## 0. Limite desta auditoria (leia antes do resto)

O mapa manda anexar `doc 30` e o resultado de P4 como material do WS-0. **Nenhum doc
numerado está no repositório.** O que existe hoje:

```
rts-systems-test/
├── README.md        (1 linha: "# rts-systems-test")
└── .git/            (1 commit: "Initial commit")
```

Ausentes: `00-visao-geral-e-escopo.md`, `10-adaptacao-de-sistemas.md`,
`20-combate-e-unidades.md`, `30-arquitetura-godot.md`, `delta-decisoes-sessao.md`,
`prompts-pesquisa-tecnica.md`, `nacoes-e-geracao-procedural.md`.

Não há projeto Godot, `.csproj`, `.sln`, `project.godot`, `.gitignore` ou CI.

**O que isso muda:** a Seção 1 ("design exige vs estado atual") foi construída a partir
do **próprio mapa de execução**, que é a única fonte disponível. O mapa é explícito em
dizer que *não é* doc de design canônico. Portanto: as lacunas da Seção 2 marcadas como
**[BLOQUEIO]** não são opinião minha sobre o design — são informação que eu literalmente
não tenho, e que muda o código. P4 foi respondida por pesquisa; P1 idem; o resto não.

---

## 1. O que o design do WS-0 exige vs o estado atual

Escopo declarado (mapa §3): *arquitetura de duas camadas (sim C# puro / apresentação
nodes); harness de teste headless; resolução da tensão `Resource`/`StringName`; primeiro
objeto-prova (o pool de recursos, que não tem dependência de node).*

| # | Exigência do design | Estado atual | Distância |
|---|---|---|---|
| 1 | Separação em duas camadas, sim pura / apresentação | inexistente | precisa da estrutura de solução inteira |
| 2 | Harness de teste headless | inexistente | precisa de projeto de teste + runner |
| 3 | Decidir `Resource`/`StringName` | em aberto (mapa §1) | **decidível agora** — P4 traz o fato que resolve; recomendação em §2.2 |
| 4 | Objeto-prova: pool de recursos global, *tier-aware* | inexistente | estrutura é implementável; **o conjunto dos 10 bens não** (doc 10 ausente) |
| 5 | Versão de engine/runtime travada | não existe projeto Godot | 4.7.2 / `net8.0` verificados (P1 §0) |
| 6 | Determinismo (exigido por WS-4 auto-combate e WS-7 "mesma engine, sem cheating") | inexistente | precisa de fonte de aleatoriedade própria e regras de iteração — §2.5 |
| 7 | *One source of truth* para enums | inexistente | precisa da regra de fronteira fixada aqui, antes de qualquer enum nascer |

Observação de dependência: WS-0 "bloqueia todas as outras", e tudo que está nesta tabela
é barato **agora** e caro depois. O item 6 em particular: determinismo não é um recurso
que se adiciona a um núcleo pronto, é uma propriedade que se preserva desde a primeira
coleção instanciada.

---

## 2. Lacunas, riscos e mismatches

### 2.1 [BLOQUEIO] Os docs de design não estão no repositório

O mapa §5 diz que o vault é a fonte de verdade. O vault não está aqui. Consequências
diretas e concretas:

- **O objeto-prova não pode ser concluído.** O pool é *tier-aware* e cobre "os 10 bens"
  (mapa §3, WS-1), mas a lista dos 10 bens, quais têm ladder (o mapa cita aço e pólvora)
  e quantos tiers existem estão no doc 10. Sem isso, ou eu invento dados — que violam
  *one source of truth* e teriam de ser refeitos — ou o pool fica sem conteúdo.
- `doc 30` é o material canônico do WS-0 e contém a estrutura de nodes (§4) e a pendência
  `Resource` (§8 #2) que esta frente deve resolver. Minha recomendação em §2.2 pode
  colidir com algo que o doc 30 já decidiu.

**Pedido:** commitar o vault no repositório (sugestão: `docs/design/`) antes da rodada de
implementação. Isso também resolve o problema estrutural de o "centro de comando" viver
fora do repositório que ele comanda.

### 2.2 Tensão `Resource`/`StringName` — recomendação (decisão sua)

Recomendo: **a simulação nunca referencia `GodotSharp`. `Resource` não é usado para dado
de simulação. `StringName` não existe no núcleo.**

Justificativa, dos fatos de P4 e não de preferência estética:

1. Uma classlib .NET **sem** referência ao Godot é um caminho suportado e limpo. Com
   `GodotSharp`, entra-se no caminho com defeitos conhecidos e registrados (atributos
   duplicados de `System.Reflection`; falha em runtime no primeiro uso de um tipo da
   biblioteca referenciada).
2. O registro de classes do Godot só acontece dentro do projeto Godot. Um núcleo que
   herda de `Resource` está estruturalmente amarrado ao editor.
3. `dotnet test` sobre o núcleo puro não precisa de engine, editor nem binário em CI. Essa
   propriedade — o harness headless que o WS-0 deve entregar — **é exatamente o que se
   perde** ao herdar de `Resource`.
4. **Argumento decisivo, e vem do design, não da engine:** o WS-3 pede um *gerador
   procedural* de facções com orçamento de poder e seed. Dado gerado em runtime não é
   autorado no inspetor. Se o schema de facção fosse `Resource`, haveria dois caminhos
   para criar uma facção (autorada no editor e gerada pelo código) — duas fontes de
   verdade, exatamente o que os princípios proíbem.

Corolário, para não deixar o buraco do "e os dados autorados, então?": conteúdo estático
(as três facções canônicas, tabelas de bens) vira **arquivo de dados lido para POCOs do
núcleo**, mesmo caminho que o gerador procedural usa. Um caminho só.
`Resource` continua sendo o que ele é de fato: asset do Godot (textura, cena, tileset), na
camada de apresentação.

**Risco desta recomendação:** perde-se a edição no inspetor do Godot para dado de jogo.
É um custo real de ergonomia para dev solo. Estou recomendando pagá-lo; a decisão é sua,
e se o doc 30 já decidiu o contrário, o doc 30 ganha e eu preciso vê-lo.

### 2.3 Enums e a fronteira com GDScript

A regra do mapa é que a apresentação só pode referenciar tipos definidos pela simulação.
Há um lugar onde essa regra vai ser testada e é melhor saber agora: **Better Terrain é um
autoload GDScript** (P1 §1.3). Chamá-lo do C# é interop dinâmico, sem checagem em tempo de
compilação, e é o ponto natural onde alguém redeclara um enum do lado GDScript.

Regra de fronteira proposta (a fixar no WS-0, aplicada no WS-2): **nenhum tipo da
simulação atravessa para GDScript.** O que atravessa é `int`/`Vector2I`. A tradução
acontece num único ponto do lado C#, e falha alto se o valor não mapear.

### 2.4 Mismatch de versão: "Godot 4.3+" é um intervalo, não uma decisão

Estável atual é **4.7.2**; `GodotSharp` 4.7.2 tem alvo `net8.0` (verificado no pacote).
Entre 4.3 e 4.7 o `TileMap` foi depreciado em favor de `TileMapLayer` — a ponto de o
plugin de terreno se dividir em duas versões. Um doc de arquitetura escrito contra 4.3
pode estar descrevendo um node que não é mais o recomendado.

**Recomendação:** travar `4.7.2` e `net8.0` no repositório e tratar upgrade como tarefa
explícita. Upgrade de engine no meio do caminho é barato agora e caro na Fase C.

### 2.5 Determinismo não é automático

WS-4 pede auto-combate determinístico e WS-7 pede que as duas IAs rodem a **mesma engine
determinística sem cheating**. Isso só se sustenta se o núcleo respeitar, desde o começo:

- **Aleatoriedade:** uma fonte semeada e explícita, própria do núcleo. `RandomNumberGenerator`
  é tipo do Godot (fora, por §2.2) e `System.Random` não tem algoritmo estável garantido
  entre versões de runtime — para "mesma seed, mesma partida" valendo entre builds, o
  núcleo precisa do seu próprio PRNG, pequeno e testado.
- **Ordem de iteração:** `Dictionary`/`HashSet` não garantem ordem. Um sistema que itera
  um dicionário e acumula `float` produz resultado dependente de ordem de inserção. Toda
  iteração que alimenta cálculo precisa de ordem definida.
- **Sem paralelismo** no tick da simulação.
- **Sem tempo de parede** dentro do núcleo: o tick recebe o passo, não consulta relógio.

Não é overengineering: são quatro restrições, todas testáveis com um teste de "mesma seed,
duas execuções, mesmo estado final" — que é justamente o tipo de teste que o harness do
WS-0 existe para hospedar.

### 2.6 Risco de escopo do próprio WS-0

O escopo do mapa para o WS-0 é fundação, não features. O loop de tick/pausa e o ciclo de
partida pertencem ao **WS-6**. O plano da Seção 3 entrega o mínimo para o núcleo ser
testável e não inclui máquina de estados de partida. Se o relatório for aprovado, esse
limite é o contrato.

---

## 3. Plano de implementação, em passos cirúrgicos e na ordem de dependência

Cada passo é um commit. Nenhum passo depende de um posterior. Código em inglês,
documentação e comentários em PT-BR.

**Passo 1 — Esqueleto da solução e higiene do repositório**
- `.gitignore` para Godot + .NET; `.sln`; estrutura `src/Lgp.Core/`, `tests/Lgp.Core.Tests/`,
  `game/` (projeto Godot, vazio por ora).
- `Lgp.Core.csproj`: `net8.0`, `Nullable=enable`, `TreatWarningsAsErrors=true`,
  **zero referência a `GodotSharp`** — esta é a invariante que segura tudo.
- *Aceite:* `dotnet build` limpo. *Não entra:* projeto Godot ainda.

**Passo 2 — Harness de teste headless**
- `Lgp.Core.Tests` com xUnit, `ProjectReference` para o núcleo, um teste trivial.
- Script único de verificação (`dotnet test`).
- *Aceite:* `dotnet test` verde sem engine instalada. **Este passo é o entregável nº 2 do
  WS-0.** *Não entra:* `gdUnit4Net` (P1 §1.1 — segundo framework sem caso de uso hoje).

**Passo 3 — Teste de guarda da arquitetura**
- Um teste que falha se o assembly do núcleo passar a ter dependência de `GodotSharp`.
- *Por quê:* a separação de camadas é a única coisa que o WS-0 entrega que pode ser
  quebrada silenciosamente por qualquer commit futuro. Um teste, ~10 linhas.
- *Aceite:* o teste falha se alguém adicionar a referência.

**Passo 4 — Identidade e enums (fixar a regra antes do primeiro enum)**
- Onde enums vivem (núcleo), como a apresentação os consome, como a fronteira GDScript
  converte (§2.3). Poucos tipos, documentados.
- **Depende de doc 10** para o conteúdo (quais bens). A *regra* não depende.

**Passo 5 — Determinismo**
- PRNG semeado próprio do núcleo + teste "mesma seed → mesma sequência".
- *Aceite:* sequência reproduzível e fixada em teste.

**Passo 6 — Objeto-prova: o pool de recursos global tier-aware**
- Baldes por (bem, tier); operações de crédito/débito; **falha alta** em débito sem saldo
  (fail fast, sem clamp silencioso — clamp é fallback disfarçado).
- Testes cobrindo: separação por tier, débito inválido, conservação.
- **BLOQUEADO por doc 10** quanto ao conjunto de bens e ao número de tiers.

**Passo 7 — Projeto Godot e o fio de ligação**
- `game/` com `project.godot` (4.7.2), `.csproj` do jogo com `ProjectReference` para o
  núcleo, e **um** smoke: uma cena que lê o pool e imprime. Prova a fronteira ponta a ponta.
- *Aceite:* o jogo roda e mostra estado vindo do núcleo. *Não entra:* UI (WS-8).

Passos 1–3 e 5 **não dependem de nenhum doc** e podem começar assim que você aprovar.
4 e 6 esperam o doc 10. 7 depende do 1.

---

## 4. Pontos onde a API/lib precisa ser verificada antes de assumir

Já verificados nesta rodada (com fonte em `docs/pesquisa/P1-P4-verificacao-tecnica.md`):
Godot estável 4.7.2; `GodotSharp` alvo `net8.0`; classlib sem Godot é caminho suportado e
com Godot é o caminho com defeitos conhecidos; gdUnit4Net roda sem runtime do Godot;
Better Terrain é autoload com API GDScript sobre `TileMapLayer`; FMG exporta GeoJSON de
células com província/altura e tem buracos/sobreposições conhecidos.

**A verificar antes de escrever a linha que os usa** (não assumir de memória):

1. `project.godot` de 4.7.2 — chaves e formato mudam entre versões; gerar pelo editor, não à mão.
2. Sintaxe exata do interop C#→autoload GDScript em 4.7 (`GetNode`/`Call` e marshalling de `Vector2I`) — WS-2.
3. `TileMapLayer` em 4.7: assinaturas de `SetCell`/`SetCellsTerrainConnect` — WS-2.
4. Compatibilidade declarada do Better Terrain com 4.7 especificamente: o repositório não a
   afirma. **Verificar antes de adotar** — é dependência de terceiro no caminho crítico do WS-2.
5. `NavigationServer2D` vs `AStarGrid2D` em 4.7 — só quando o chat de design do WS-4
   decidir zona/tile vs região contínua (mapa §4). Pesquisar depois da decisão, não antes.
6. Esquema exato do GeoJSON de células do FMG (nomes de campo de província/altura) —
   contra um export real, não contra a wiki.

---

## 5. Decisões que preciso de você antes da implementação

1. **Commitar o vault de design no repositório?** (§2.1) — desbloqueia passos 4 e 6.
2. **Aprova `Resource`/`StringName` fora do núcleo, dados em arquivo?** (§2.2) — é a
   decisão de arquitetura que o WS-0 existe para fechar, e ela é irreversível na prática.
3. **Travar Godot 4.7.2 / `net8.0`?** (§2.4)
4. **Confirma o limite de escopo** do WS-0 (sem loop de partida, sem UI)? (§2.6)

Se você quiser destravar o caminho imediatamente sem esperar o vault: aprove 2, 3 e 4 e eu
executo os passos 1, 2, 3 e 5, que não dependem de nenhum doc.

**Parei no relatório. Aguardo aprovação explícita para a rodada de implementação.**
