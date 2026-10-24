# 30 — Arquitetura Godot

**Projeto:** Le Grand Polemarkos (LGP)
**Pré-requisito:** ler `00-visao-geral-e-escopo.md`
**Status do documento:** travado quanto a stack, equivalências e camadas; estrutura de nodes é proposta inicial

Este documento define a arquitetura técnica em Godot + C# e, crucialmente, registra como
LGP **consome o mapa de subsistemas engine-agnostic** do doc 70 do LGE — esse consumo é o
objetivo secundário do projeto (validar o playbook de migração).

---

## 1. Stack

- **Engine:** Godot **4.7.2** (travada — ver doc 00 §6). Verificada em
  `docs/pesquisa/P1-P4-verificacao-tecnica.md`.
- **Runtime:** `net8.0` — alvo declarado do `GodotSharp` 4.7.2.
- **Linguagem:** C# (.NET), consistente com a avaliação de migração do LGE.
- **Renderização:** 2D (travado no doc 00, §5).

---

## 2. Consumindo o mapa de subsistemas engine-agnostic (doc 70 do LGE)

O doc 70 do LGE documentou um mapa de subsistemas pensado para ser independente de engine,
mais um playbook de migração para Godot — porém nunca executado. LGP é o **primeiro
consumidor real** desse mapa.

Princípio de arquitetura derivado disso: **a lógica de simulação (produção, workforce,
combate, política) deve viver em C# puro, desacoplada de nodes Godot.** Os nodes são a camada
de apresentação e input; a simulação não importa `Godot.*` — ver §9, que trava isso.

Isto não é purismo — é o que torna o teste de validação significativo. Se a simulação
estiver enredada nos nodes, "validar a migração" não prova nada transferível ao LGE.

Estrutura conceitual em duas camadas (espelhando StrategosCore / StrategosUI do LGE):

- **Camada de simulação** (C# puro): o equivalente ao `StrategosCore`. Sem dependência de
  nodes. Testável headless. Em disco: `src/Lgp.Core`.
- **Camada de apresentação** (nodes Godot + C#): o equivalente ao `StrategosUI`. Lê estado da
  simulação e o desenha; traduz input em comandos para a simulação. Em disco: `game/`.

> **Lição herdada do LGE.** No LGE, `StrategosUI` foi escrito contra enums de design-doc em vez
> dos enums implementados no Core, causando mismatches de compilação. A prevenção em LGP: a
> camada de apresentação **só** referencia tipos definidos pela camada de simulação. Um enum
> tem uma definição canônica, na simulação, consumida pela apresentação — nunca duplicada.
> ("One source of truth para enums", princípio herdado.)

---

## 3. Equivalências UE5 → Godot

Tabela de tradução dos conceitos centrais do LGE. Esta é a espinha do playbook de migração que
LGP valida.

| Conceito (LGE / UE5)              | Equivalente (LGP)                                  | Nota |
|-----------------------------------|----------------------------------------------------|------|
| `UDataAsset` (arquitetura data-driven) | **Arquivo de dados → POCO do núcleo**         | **Não** é `Resource`. `Resource` é asset do Godot (textura, cena, tileset) e vive só na apresentação. Bens, métodos de produção e definições de facção são lidos de arquivo de dados para objetos C# puros. Ver §9. |
| `FName` (identificador interno)   | **Tipo de identidade próprio do núcleo** (enum ou id tipado em C# puro) | **Não** é `StringName`, que é tipo do Godot. A definição canônica vive na simulação e é consumida pela apresentação. |
| `USocialHierarchyDefinition` (DataAsset trocável) | Definição em arquivo de dados, hidratada em POCO | A hierarquia social (comprimida para 3 workforce) segue o mesmo caminho: um só. |
| Módulo `StrategosCore`            | `src/Lgp.Core` — assembly de simulação em C# puro  | Sem `using Godot;`. Garantido por `ArchitectureTests`. |
| Módulo `StrategosUI`              | `game/` — cena raiz + nodes de apresentação        | Lê a simulação; não a contém. |
| `UStrategosImportWorldCommandlet` (FMG → mundo) | Tool/script de importação em C#       | Reimplementação do *fluxo*, não port de código. Pode rodar como ferramenta de editor. |
| Commit com SaveVersion bump por sessão | Mesma disciplina; ver §6 e §10.1               | Princípio de versionamento de save herdado. |

---

## 4. Estrutura de nodes proposta

Proposta inicial (não travada — ajustável conforme o protótipo evolui). Hierarquia da cena
principal de partida:

```
MatchRoot (Node)
├── Simulation (Node)              # ponte fina para a camada de simulação C# pura
├── Map (Node2D)
│   ├── TerrainLayer (TileMapLayer)    # terreno base
│   ├── ProvinceLayer (TileMapLayer)   # fronteiras / territórios capturáveis
│   └── OverlayLayer (TileMapLayer)    # overlays de produção/seleção
├── Armies (Node2D)                # blobs de exército como filhos
├── UILayer (CanvasLayer)
│   ├── ProductionPanel
│   ├── LandsraadPanel
│   └── FactionLadderPanel
└── Camera2D
```

Notas:
- `Simulation` é deliberadamente **fino**: ele instancia e atualiza a simulação C# pura, mas
  não contém regras de jogo. Mantém a separação da §2.
- Cada blob de exército é um node sob `Armies`, lendo sua composição/estado da simulação.
- Os painéis de UI leem estado e emitem comandos; não calculam regras.

---

## 5. Renderização 2D

### 5.1 Mapa: `TileMapLayer` (não `TileMap`)
A partir do Godot **4.3**, a classe `TileMap` foi **depreciada** e substituída por
`TileMapLayer` — cada camada visual é um node `TileMapLayer` independente, com API mais
simples e menos poluição no inspector. A classe `TileMap` antiga permanece por compatibilidade
mas **não recebe novas features**; projetos novos em 2026 devem começar com `TileMapLayer`.

Um `TileMapLayer` por camada (terreno, províncias, overlays), conforme a estrutura da §4.

Se o protótipo pintar terreno em runtime (geração procedural via Azgaar), o plugin community
**Better Terrain** é a primeira dependência a considerar — ele cobre o caso de terrain
matching procedural que o terrain nativo do Godot 4 faz de forma menos confiável. **A
compatibilidade declarada dele com 4.7 é dependência de terceiro no caminho crítico e precisa
ser verificada antes do WS-2** (pesquisa P3).

### 5.2 Direção de arte
2D estilizado, referência FMA (linework, paleta). Detalhes de pipeline de arte ficam fora
deste documento de arquitetura.

### 5.3 Performance de muitas unidades (nota)
Se a contagem de unidades visíveis crescer ao ponto de virar gargalo de draw call,
`MultiMeshInstance2D` desenha milhares de sprites idênticos em uma chamada — mas isto é
otimização **pós-protótipo**. Não introduzir antes de medir um gargalo real (princípio:
sem overengineering).

---

## 6. Persistência (save)

- Disciplina de **SaveVersion bump por sessão**, herdada do LGE.
- O estado de save vem da **camada de simulação** (C# puro), não dos nodes — coerente com a
  separação da §2. Salvar a simulação, não a árvore de cena.
- Formato travado: **JSON legível**. Detalhes e consequências em §10.1.

---

## 7. Objetivo de validação

Registro explícito do que LGP precisa provar para que a decisão de migração do LGE "saia de
graça":

1. A camada de simulação em C# puro roda **headless** (sem nodes), testável. ✅ implementado no
   WS-0 (`dotnet test` sem engine, editor ou binário do Godot).
2. A arquitetura data-driven do LGE sobrevive **sem** `Resource`/`StringName` no núcleo, pelo
   caminho de arquivo de dados → POCO. Ver §9.
3. O fluxo Azgaar FMG → mapa jogável funciona reimplementado em Godot.
4. A separação simulação/apresentação previne o mismatch de enums que ocorreu no LGE.

Se os quatro forem verdadeiros ao fim do protótipo, o playbook do doc 70 do LGE está validado.

---

## 8. Camadas em disco

```
Lgp.sln
src/Lgp.Core/          simulação, C# puro — não referencia o Godot
tests/Lgp.Core.Tests/  xUnit, headless — inclui ArchitectureTests
game/                  projeto Godot 4.7.2 (apresentação)
```

---

## 9. Resolução da tensão pura-C# vs. `Resource`/`StringName` (TRAVADA)

O §2 quer a simulação em C# puro, headless-testável. A versão anterior do §3 definia os dados como
`Resource` e os IDs como `StringName` — **tipos do Godot**. A tensão era real: uma camada "pura"
não pode possuir tipos da engine.

**Resolvido: a simulação nunca referencia `GodotSharp`. `Resource` e `StringName` não existem no
núcleo.** Quatro razões, em ordem de peso:

1. **Vem do design, não da engine — e é a decisiva.** O WS-3 pede um **gerador procedural** de
   facções com orçamento de poder e seed (doc 40). Dado gerado em runtime não é autorado no
   inspetor. Se o schema de facção fosse `Resource`, haveria **dois caminhos** para criar uma
   facção — autorada no editor e gerada por código — ou seja, duas fontes de verdade, exatamente
   o que o doc 00 §7 proíbe.
2. Uma classlib .NET **sem** referência ao Godot é caminho suportado e limpo; com `GodotSharp`
   entra-se no caminho com defeitos conhecidos e registrados (atributos duplicados de
   `System.Reflection`; falha em runtime no primeiro uso de um tipo da lib referenciada).
3. O registro de classes do Godot só acontece dentro do projeto Godot. Um núcleo que herda de
   `Resource` fica estruturalmente amarrado ao editor.
4. `dotnet test` sobre o núcleo puro não precisa de engine, editor nem binário em CI — que é
   exatamente o critério de validação do §7 #1, e é o que se perderia.

**Corolário — e os dados autorados, então?** Conteúdo estático (as três facções canônicas, as
tabelas de bens) vira **arquivo de dados lido para POCOs do núcleo**, o mesmo caminho que o gerador
procedural usa. Um caminho só.

**Custo aceito:** perde-se a edição no inspetor do Godot para dado de jogo. É um custo real de
ergonomia para dev solo, e está sendo pago conscientemente.

**Guarda:** `ArchitectureTests` (em `tests/Lgp.Core.Tests/`) falha se o assembly do núcleo passar a
referenciar qualquer assembly do Godot. Se ele ficar vermelho, a resposta não é relaxar o teste.

### 9.1 Fronteira com GDScript

Better Terrain é um autoload **GDScript**, e chamá-lo do C# é interop dinâmico, sem checagem em
tempo de compilação. É o ponto natural onde alguém redeclara um enum do lado GDScript e reintroduz
o mismatch que o §2 existe para prevenir.

**Regra:** nenhum tipo da simulação atravessa para GDScript. O que atravessa é `int`/`Vector2I`. A
tradução acontece num **único ponto** do lado C#, e **falha alto** se o valor não mapear.

### 9.2 Determinismo do núcleo

Quatro restrições, todas testáveis, todas em vigor desde o WS-0:

1. Aleatoriedade só via `DeterministicRandom` (PCG32 semeado). Nunca `System.Random` (algoritmo
   não estável entre versões do runtime) nem o RNG do Godot (é tipo da engine).
2. Toda iteração que alimenta cálculo precisa de ordem definida. `Dictionary` e `HashSet` não
   garantem ordem.
3. Sem paralelismo dentro do tick.
4. Sem relógio de parede no núcleo: o tick **recebe** o passo, não consulta a hora.

---

## 10. Decisões em aberto

| # | Item                                                                  | Status   |
|---|-----------------------------------------------------------------------|----------|
| 1 | Formato de save                                                       | ✅ **TRAVADO — JSON legível, e "save exportável/inspecionável" é REQUISITO DE ARQUITETURA, não feature.** Ver §10.1. |
| 2 | Movimento no mapa: grid tile-a-tile vs. contínuo                      | 🔴 Pendente e crítico — compartilhado com doc 20 §9 #3. Pesquisar `NavigationServer2D` vs. `AStarGrid2D` em 4.7 **depois** da decisão de design, não antes. |
| 3 | Ferramenta de importação FMG roda no editor ou como script externo    | Pendente — WS-2 |
| 4 | Estrutura de nodes da §4 sobrevive ao primeiro protótipo               | A validar |
| 5 | Estratégia de teste headless da camada de simulação                   | ✅ **TRAVADA** — xUnit + `dotnet test` sobre `Lgp.Core`, sem engine, editor ou binário do Godot em CI. Implementada no WS-0 passo 2. `gdUnit4Net` fica fora: segundo framework sem caso de uso. |
| 6 | Bibliotecas C++ do LGE (delaunator-cpp, FastNoiseLite): há análogo?   | ✅ **Resolvido — não há port a fazer.** Godot 4 traz `FastNoiseLite` nativo e `Geometry2D` tem triangulação de Delaunay. ⚠️ `FastNoiseLite` herda de `Resource`, portanto é tipo do Godot: geração de ruído vive na apresentação/ferramenta, não no núcleo (§9). Voronoi a partir do Delaunay segue não verificado — ver `baseline-fontes-de-verdade.md`. |

### 10.1 Formato de save (travado)

JSON legível e diff-ável. Consequências, que são de **arquitetura** e não de feature:

- O estado autoritativo da partida vive **inteiro** na camada de simulação, numa estrutura
  serializável. **Nenhum estado autoritativo em nodes** — é o que reforça §2 e §6.
- Referência de gênero: Unciv / Paradox — partida inteira em arquivo de texto, engine = regras
  sobre dados. **Limite dessa abordagem:** regras ficam em C# compilado; só conteúdo e parâmetro
  viram dado (`principios-de-producao.md` §3). O interlock **não** vira arquivo de script.
- Escala do protótipo (3 facções, ~30–50 províncias, 10 bens) torna tamanho e velocidade
  irrelevantes. Escrita acontece em momentos discretos — autosave, save manual — **não por tick**.
  Autosave em intervalo e escrita em thread separada são mitigações *se medidas como necessárias*;
  não implementar preventivamente.
- Disciplina de **SaveVersion bump por sessão**: campo de versão no arquivo + um ponto de migração.
- Paga em três frentes de uma vez: **debug** (reproduzir um bug colando um save), **teste
  headless** (fixtures triviais) e **modding** depois.
