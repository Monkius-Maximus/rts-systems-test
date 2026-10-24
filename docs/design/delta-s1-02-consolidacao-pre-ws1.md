# Delta S1-02 — Consolidação pré-WS-1

> **APLICADO** no commit que trouxe o vault para o repositório. Este arquivo fica como
> histórico: ele registra *por que* o doc 10 §4-A/§4-B e o doc 30 §9 existem. Os docs
> numerados são a fonte de verdade; em caso de divergência, eles vencem.

**Projeto:** Le Grand Polemarkos (LGP) · **Sprint:** 01 · **Itens:** S1-01 e S1-02
**Natureza:** blocos prontos para colar nos docs canônicos. Cada bloco dizia **SUBSTITUIR** (troca
um trecho existente) ou **ACRESCENTAR** (entra novo). Depois de aplicados, os docs 10 e 30 não têm
mais nenhuma pendência que o WS-1 precise consultar.

**Origem do conteúdo:** decisões já travadas em `mapa-de-execucao.md §1` e
`delta-decisoes-sessao.md`, que nunca foram dobradas nos docs numerados, mais as quatro decisões
do recon do WS-0 que o código mergeado já implementa de fato.

---

## A. Ordem de aplicação (executada)

1. ✅ Criar `docs/design/` no repositório e commitar os docs (S1-01).
2. ✅ Aplicar os blocos B–E nos docs 00/10/20/30 (S1-02).
3. ✅ Atualizar o `CLAUDE.md` — removido o aviso de bloqueio documental.
4. ✅ Commit único.

**Desvios conscientes na aplicação**, registrados para não virarem mistério depois:

- **doc 00 §9 #2 e doc 10 §7 #3 foram marcados ✅ TRAVADA, não "⬜ PRÓXIMA".** As identidades das
  três facções estavam decididas no `40-nacoes-e-geracao-procedural.md` desde antes deste delta;
  o status "próxima" era defasado. Com isso o **S1-07 está fechado** e o único bloqueio de design
  restante é o combate (S1-08).
- **O `CLAUDE.md` não foi substituído, só editado na seção "Onde está o quê".** A versão no
  repositório já estava correta e mais enxuta (4.7.2, invariante de arquitetura, determinismo).
  Trocá-la inteira violaria "mudanças cirúrgicas".
- **doc 30 §10 #6 ganhou uma ressalva:** `FastNoiseLite` é nativo, mas **herda de `Resource`** —
  portanto é tipo do Godot e não pode viver no núcleo. Geração de ruído fica na
  apresentação/ferramenta. Voronoi a partir do Delaunay segue **não verificado**
  (`baseline-fontes-de-verdade.md` §3.2).

---

## B. doc 00 — Visão Geral e Escopo

### B.1 · §6 — engine

"Godot 4.x (4.3+)" foi substituído por **Godot 4.7.2, travada**, com `net8.0` como runtime.
"4.3+" era um intervalo, não uma decisão, e a API mudou dentro dele (`TileMap` → `TileMapLayer`).
Upgrade de engine passa a ser tarefa explícita.

### B.2 · §9 — tabela de decisões em aberto

- #1 Condição de vitória → ✅ TRAVADA (territorial, limiar de %, eliminação = caso degenerado).
- #2 Identidades de facção → ✅ TRAVADA (doc 40).
- #4 Multiplayer → ✅ TRAVADA (adiado; 3 facções = jogador + 2 IA; bit-exact não é requisito).
- #3 e #5 seguem pendentes.

---

## C. doc 10 — Adaptação de Sistemas

- **§3.1** — lista de bens **TRAVADA em 10**, com tabela e diagrama de cadeias. Declarada fonte de
  verdade dos identificadores de bem e de PM.
- **§3.4** — ACRESCENTADO o *esqueleto do ladder de PM*: só aço e pólvora têm ladder; finais herdam
  o tier dos insumos; cobre é gargalo de quantidade, não driver de tier; gating
  Trabalhador→Especialista; ladder-bandeira do aço em 3 degraus.
- **§4-A** (nova) — *topologia da economia*: pool global tier-aware, produção espacial, sem
  logística de transporte. Inclui sítios de extração binários (4-A.1), edifícios em slots de
  província (4-A.2) e a regra de captura com o tier do inimigo (4-A.3). Resolve a pendência
  transversal "ligação território ↔ produção".
- **§4-B** (nova) — *invariantes de escassez do gerador de mapa*: nenhum começo autossuficiente;
  salitre e cobre raros; coração metalúrgico exige expansão; fraqueza de facção vira geografia.
- **§7** — tabela reescrita: #1–#4 travados; #5–#8 pendentes e cada um marcado com o que bloqueia.

---

## D. doc 20 — Combate e Unidades

- **§4** — ACRESCENTADA a *precisão do termo "determinístico"*: significa mesma fórmula para
  jogador e IA, **não** bit-exact entre máquinas. Distingue isso do determinismo de simulação que
  o núcleo já implementa (mesma seed → mesma partida), que continua valendo.
- **§9** — tabela reescrita, com três itens marcados 🔴 como bloqueio do WS-4: modelo de movimento
  (#3, cruzado com doc 30 §10 #2), entidade Comandante (#7) e tier do conceito militar / roster
  (#8). Naval passa a "adiado".
- **§2, §3 e §5** ganharam notas apontando para as pendências correspondentes, para que a leitura
  linear não sugira que o sistema está fechado.

---

## E. doc 30 — Arquitetura Godot

- **§1** — stack: Godot 4.7.2, `net8.0`.
- **§3** — as linhas `UDataAsset → Resource` e `FName → StringName` **saíram**: contradiziam a
  decisão de arquitetura do WS-0. Substituídas por *arquivo de dados → POCO do núcleo* e *tipo de
  identidade próprio do núcleo*.
- **§8** (nova) — camadas em disco.
- **§9** (nova) — *resolução da tensão pura-C# vs. `Resource`/`StringName`*, travada: a simulação
  nunca referencia `GodotSharp`. Quatro razões, a decisiva sendo de design (o gerador procedural
  do doc 40 criaria dois caminhos para fabricar uma facção). Inclui a fronteira com GDScript
  (§9.1) e as quatro restrições de determinismo do núcleo (§9.2).
- **§10** (antiga §8) — tabela reescrita: save travado em JSON (§10.1), teste headless travado,
  libs C++ resolvidas com ressalva, movimento ainda crítico.
- **§7** — o objetivo de validação #2 foi reescrito: a arquitetura data-driven sobrevive **sem**
  `Resource`/`StringName` no núcleo. O critério antigo pedia o oposto do que decidimos.
