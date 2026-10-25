# Linha de Base — Fontes de Verdade

**Projeto:** Le Grand Polemarkos (LGP)
**Data:** 2026-09-13
**Natureza:** documento de auditoria. Separa **o que veio dos seus docs** do **que eu (Claude)
introduzi**, e dentro do que introduzi, o que está **verificado** e o que é **suposição**.
Objetivo: partir do mesmo ponto, sem herdar afirmação minha como se fosse decisão sua.

---

## 1. O que foi fonte de verdade (input canônico)

Estes são **seus** documentos. Foram a única base factual das decisões desta sessão:

| Doc | O que trava |
|---|---|
| `00-visao-geral-e-escopo.md` | §0 trava de escopo; 3 forks; guarda-corpos (1 mapa, 3 facções, ~12 bens, 3 workforce, 1 vitória); decisão 2D; Godot 4.x+C#; 7 princípios; nota de risco |
| `10-adaptacao-de-sistemas.md` | Framework porta/comprime/morre; critério de seleção de bens; 3 workforce; Landsraad; ladder de PM; abstração temporal |
| `20-combate-e-unidades.md` | Blobs com auto-combate; pontos de comando como cap; engajamento determinístico; fórmula de interlock; manpower |
| `30-arquitetura-godot.md` | Duas camadas (sim C# pura / apresentação); equivalências UE5→Godot; `TileMapLayer`; estrutura de nodes; save da simulação |

Também tratei como dado (você declarou, não os docs): idioma (conversa PT-BR, código EN, docs
PT-BR); modo propor-confirmar; sem código nesta fase; onboarding de duas rodadas no Claude Code;
verificar API em vez de supor.

---

## 2. O que eu produzi nesta sessão (derivado, não canônico até você dobrar)

| Arquivo | Natureza |
|---|---|
| `delta-decisoes-sessao.md` | Changelog para dobrar nos docs numerados |
| `nacoes-e-geracao-procedural.md` | Proposta de schema (candidato a doc 40) |
| `mapa-de-execucao.md` | Coordenação (não é design canônico) |
| `prompts-pesquisa-tecnica.md` | Pack de pesquisa |
| `principios-de-producao.md` | Disciplina de produção |

---

## 3. Afirmações minhas que NÃO estão nos seus docs

Esta é a seção que importa para "mesmo ponto de partida".

### 3.1 Verificadas agora (contra a doc oficial)

| Afirmação | Estado |
|---|---|
| `FastNoiseLite` é nativo no Godot 4 | ✅ **Confirmado.** Classe nativa, herda `Noise < Resource < RefCounted < Object`. Gera ruído Cellular/Perlin/Value etc. Documentada em 4.0, 4.3, 4.4 e stable. |
| Delaunay 2D existe no Godot sem lib externa | ✅ **Confirmado em existência.** Foi exposto no singleton de geometria (PR godotengine/godot#29127, fechando a issue #25587), retornando índices dos pontos triangulados. ⚠️ O **nome exato do método em Godot 4** (`Geometry2D`) não apareceu no extrato da doc que consegui ler — confirmar no class reference do editor antes de codar. |
| Existe Godot 4.7 | ✅ Confirmado (a doc oficial tem guia "Upgrading from Godot 4.6 to Godot 4.7"). |

**Consequência arquitetural importante:** `FastNoiseLite` herda de `Resource` — ou seja, é **tipo
Godot**. Se a geração de ruído/mapa for usada, ela **não pode viver na camada C# pura** sem
importar Godot. Isso é mais um caso da tensão já aberta no doc 30, e deve entrar na pesquisa P4.

### 3.2 Não verificadas — tratar como suposição até checar

- Que o **Voronoi** (base do Azgaar FMG) tenha caminho nativo a partir do Delaunay do Godot.
  Há assets de comunidade (Delaunator em GDScript; Delaunay+Voronoi via Bowyer-Watson), o que
  sugere que a comunidade **não** considera o nativo suficiente. **Investigar (P1/P2).**
- **Better Terrain** compatível com a versão que você vai usar — não verificado.
- Formato real de export do Azgaar FMG e como parseá-lo — não verificado (P2).
- Se `StringName`/`Resource` funcionam fora do runtime Godot (o nó da tensão do doc 30) — **não
  verificado**; é o item de maior alavancagem do pack (P4).

### 3.3 Conhecimento de gênero que usei como argumento (não é doc, não é fonte primária)

Influenciaram recomendações. Se alguma estiver errada, a recomendação correspondente merece
revisão:

- Northgard = movimento por zona/tile; Spice Wars = região contínua. (Seu doc 20 §2 cita os dois
  jogos; a distinção de *granularidade* foi minha.)
- Northgard/Spice Wars usam pool global de recursos sem logística de transporte — **minha
  afirmação**, base da recomendação de topologia que você aprovou.
- Unciv e Paradox salvam a partida inteira em texto; Victoria 3 salva em texto e é ordens de
  magnitude maior que o protótipo — **minha afirmação**, base do argumento de desempenho do save.
- Unciv roda bem em hardware modesto — minha afirmação.
- Tiers históricos da metalurgia (bloomery → pudlagem → Bessemer/Siemens-Martin) e a associação
  Galia↔artilharia napoleônica — conhecimento histórico meu, usado como *flavor* do ladder.

### 3.4 Invenções de design minhas (não estavam em lugar nenhum)

Nada disto veio dos seus docs — foram propostas que você confirmou:

- **Orçamento de poder** para o gerador de nações (mecanismo de balanceamento).
- Schema de `FactionDefinition`, campos, modos `debug`/`ingame`, campos reservados.
- **Identidades das três facções** (Albion industrial-faminta / Galia pólvora-política /
  Norden marcial-farta) — decisão criativa que você me delegou.
- Mapa terreno→bruto (montanha=ferro, colina=carvão, pântano=salitre etc.).
- Sítio de extração binário operado por Trabalhadores; edifícios em slots; **captura transfere
  o tier do inimigo mas exige sua workforce**.
- Pool **tier-aware** (aço T1 e T3 como buckets distintos).
- Invariantes de escassez; fraqueza de facção como geografia.
- A distinção "determinístico = mesma fórmula ≠ bit-exact".
- O teste "muda a forma do sistema ou é só um botão?".

---

## 4. Divergências a resolver antes de codar

1. **Versão da engine.** Os quatro docs dizem **Godot 4.3+**. O `claude/sprint-backlog-01.md`
   (no conhecimento do projeto, auditoria de 2026-09-13) diz que o repositório travou
   **4.7.2 / net8.0**. São 4 versões menores de diferença; a API muda entre versões. **Dobrar a
   versão real nos docs 00 §6 e 30 §1** — senão todo prompt de pesquisa parte da premissa errada.
2. **Escopo do que conversamos vs. o que existe.** Nesta sessão eu trabalhei **apenas** sobre os
   quatro docs numerados. O sprint backlog mostra que o projeto já andou além disso (solução
   `Lgp.sln`, testes headless, `ArchitectureTests`, `DeterministicRandom`, PR #1 mergeado). Nada
   do que decidimos aqui contradiz esse código — mas as decisões desta sessão **não foram
   tomadas olhando para ele**.
3. **Bloqueio documental.** O sprint backlog registra que os docs de design **não estão no
   repositório** e que isso bloqueia três tarefas de código (S1-03, S1-04, S1-06). É o item
   S1-01, ~30 min de trabalho, e é literalmente o de maior alavancagem hoje.

---

## 5. Como usar este documento

Guarde-o junto aos demais. Quando uma decisão futura se apoiar numa afirmação da §3.2 ou §3.3,
verifique antes de tratá-la como travada. As da §3.4 são reversíveis por definição — foram
propostas, não descobertas.
