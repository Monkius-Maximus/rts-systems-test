# 00 — Visão Geral e Escopo

**Projeto:** Le Grand Polemarkos (LGP)
**Tipo:** Spin-off 4X-RTS de Le Grand Strategos (LGE)
**Engine:** Godot 4.7.2 com C#
**Status do documento:** travado

> **Nota de numeração.** Esta é uma vault separada, com repositório e contexto próprios.
> A numeração reinicia em `00`. Referências a documentos do LGE são sempre marcadas
> explicitamente (ex.: "doc 70 do LGE") para evitar contaminação cruzada de contexto.

---

## §0 — Trava de Escopo (não-negociável)

Estas afirmações definem o que LGP **é** e fecham discussões recorrentes. Mudá-las exige
reabrir este documento, não improvisar em sessão.

1. **Gênero:** 4X em tempo real, lento e pausável — *não* RTS clássico (sem micro de
   unidade individual, sem APM). O modelo temporal é o mesmo de EU4/Victoria 3/Spice Wars.
2. **Pilar central:** **assimetria de produção** — diferentes facções produzem os mesmos
   bens por métodos diferentes (forja de bloomery vs. conversor Bessemer para aço). Este é
   o mesmo pilar do LGE e é herdado sem diluição.
3. **Sessão:** partida fechada (match-based) de 2–4 horas com condição de vitória, *não*
   campanha persistente de dezenas de horas.
4. **Escala:** um mapa procedural por partida, três facções, ~12 bens, uma condição de
   vitória. Estes números são guarda-corpos, não metas a expandir no primeiro protótipo.
5. **Apresentação:** 2D estilizado. O combate acontece **no próprio mapa**, não em tela
   tática separada.
6. **Cenário:** mesmo mundo do LGE (Albion, Galia, Norden; era industrial). O tempo de jogo
   é abstrato — a progressão de 150 anos do LGE vira ladder de upgrade *dentro* da partida
   (ver doc 10, §5), não calendário literal.

---

## 1. Relação com Le Grand Strategos (LGE)

LGP é um **spin-off que compartilha o núcleo conceitual** do LGE (bens, métodos de produção,
assimetria, interlock militar), mas é **codebase nova em Godot** — não é fork de código do
projeto UE5.

Dois objetivos, em ordem de prioridade:

- **Primário:** entregar um protótipo de 4X-RTS jogável com escopo fechado.
- **Secundário:** servir de **primeiro consumidor real do mapa de subsistemas
  engine-agnostic** documentado no doc 70 do LGE, validando na prática o playbook de
  migração para Godot antes de qualquer decisão sobre migrar o LGE em si.

Consequência de design: o trabalho de design já pago no LGE (docs 41-goods, 50-emergência,
60-batalha) é tratado como **material-fonte da compressão**, não como spec a copiar. "One
source of truth" se aplica no nível de design conceitual, não de implementação.

---

## 2. Reenquadramento conceitual

A confusão mais cara a evitar: **Dune: Spice Wars não é um RTS clássico** (StarCraft, Age of
Empires). É um "4X em tempo real lento e pausável" — o mesmo modelo temporal que LGE já
adota. A diferença real entre LGP e LGE não está no *tempo* (ambos são real-time com pausa),
está em **escopo, granularidade e controle direto**:

| Dimensão     | LGE (GSG)                                              | LGP (4X-RTS)                                          |
|--------------|--------------------------------------------------------|--------------------------------------------------------|
| Sessão       | Campanha de 40–100h                                     | Partida de 2–4h com condição de vitória                |
| Mapa         | Mundo inteiro, centenas de províncias                  | Uma região, ~30–50 territórios capturáveis             |
| Unidades     | Exércitos abstratos, batalha em tela separada          | Unidades visíveis no mapa, combate contínuo no mapa    |
| Pops         | Multidimensionais (profissão, cultura, riqueza, alfab.)| Workforce simples (trabalhador / especialista / soldado)|
| Política     | Sistemas profundos de leis (três fases)                | Resoluções periódicas cronometradas (Landsraad)        |

A implicação prática: o salto é uma **compressão**, não uma reinvenção. O risco de
engenharia é menor do que "transformar Victoria 3 em StarCraft".

---

## 3. Decisões de bifurcação (travadas)

As três bifurcações levantadas no design inicial, resolvidas. Registradas aqui como travadas.

### Fork 1 — Relação entre projetos → **(a) spin-off com core compartilhado**
LGP compartilha o core conceitual mas é codebase nova em Godot, servindo de validação do
doc 70 do LGE. Alinha com "one source of truth" no nível de design e reaproveita o trabalho
de design já pago.

### Fork 2 — Estrutura de partida → **(a) match-based puro**
Um mapa, condições de vitória, 2–4h. É o que define o gênero e o que mantém o escopo
executável por uma pessoa. Campanha persistente foi rejeitada por inflar escopo.

### Fork 3 — Modelo de combate → **(a) blobs de exército com auto-combate**
Grupos de unidades visíveis que lutam automaticamente ao engajar; o jogador decide
posicionamento e composição, não micro. Controle direto de unidades individuais (RTS
clássico) foi rejeitado: exige pathfinding de centenas de unidades, balanceamento de micro e
IA de esquadrão — overengineering proibido para um primeiro protótipo.

---

## 4. Guarda-corpos de escopo

O protótipo tem escopo fechado e explícito. Cada número abaixo é um **teto**, não um alvo a
crescer:

- **1 mapa** procedural (via pipeline Azgaar FMG adaptado — ver doc 10, §2).
- **3 facções** (Albion, Galia, Norden), diferenciadas por ladders de método de produção.
- **~12 bens**, comprimidos dos 31 do LGE (ver doc 10, §3) — **travado em 10**.
- **3 tipos de workforce** (trabalhador, especialista, soldado).
- **1 condição de vitória** (travada — ver §9).

Qualquer item além disso é expansão pós-protótipo e não entra no primeiro ciclo.

---

## 5. Decisão: 2D (travada)

2D estilizado, sem 3D. Três razões:

1. **Coerência com a justificativa de migração.** O doc 70 do LGE concluiu que Godot é o fit
   estruturalmente correto *para uma simulação 2D*. Fazer 3D em Godot abandonaria o próprio
   argumento que motivou considerar a engine.
2. **Custo de produção.** Spice Wars *parece* 3D mas joga-se como mapa 2.5D com câmera quase
   fixa. Direção de arte 2D estilizada (referência FMA: linework e paleta) entrega a mesma
   legibilidade por fração do custo — e custo de produção é a variável crítica para dev solo.
3. **Maturidade da engine.** O servidor de renderização 2D do Godot é seu pilar mais maduro;
   o 3D é onde a engine mais perde para Unreal.

---

## 6. Decisão: Godot + C# (travada)

- **Engine:** Godot **4.7.2**, travada. "4.3+" era um intervalo, não uma decisão; a API mudou
  dentro dele (o `TileMap` foi depreciado em favor do `TileMapLayer`). Runtime: `net8.0`, que é
  o alvo declarado do `GodotSharp` 4.7.2. Upgrade de engine é tarefa explícita, nunca implícita.
- **Linguagem:** C# (.NET), consistente com a avaliação de migração do LGE.
- **API de mapa:** `TileMapLayer` (um node por camada visual). A classe `TileMap` foi
  depreciada a partir do Godot 4.3 e não recebe mais features; projetos novos em 2026 devem
  começar com `TileMapLayer`. Detalhes no doc 30.

---

## 7. Princípios de design herdados (não-negociáveis)

Idênticos aos do LGE. Repetidos aqui porque governam toda decisão de implementação:

- **Fail fast** — lançar erros quando pré-condições não são atendidas, não mascarar.
- **Sem fallbacks** — um caminho correto, sem alternativas.
- **One way** — uma forma de fazer cada coisa.
- **Mudanças cirúrgicas** — fixes mínimos e focados.
- **Sem overengineering** — simples vence complexo.
- **One source of truth para enums** — uma definição canônica, consumida por todos.
- **Separação de concerns** — cada função/módulo com responsabilidade única.

---

## 8. Nota de risco

LGP é um **segundo jogo, não uma feature do primeiro**. O risco real não é técnico — é
**dispersão**: dois projetos simultâneos para um dev solo tende a produzir dois projetos
inacabados.

Mitigação: tratar LGP como **protótipo de escopo fechado** (§4) cujo objetivo secundário é
validar Godot na prática. Se ele provar o pipeline, a decisão de migração do LGE sai de
graça. Se o escopo começar a crescer, é sinal de violação desta nota — voltar ao §4.

Disciplina de produção derivada desta nota: `principios-de-producao.md`.

---

## 9. Decisões em aberto

| # | Item                                                                 | Status                |
|---|----------------------------------------------------------------------|------------------------|
| 1 | Condição de vitória do protótipo                                     | ✅ **TRAVADA** — territorial/militar: controlar um limiar de % dos territórios capturáveis; eliminação total = vitória automática (caso degenerado de 100%). Contagem de territórios, não VP. O *valor* do limiar e o timer de *hold* anti-flip-flop são balanceamento, não design. |
| 2 | Identidades iniciais das três facções (qual ladder cada uma começa)  | ✅ **TRAVADA** — doc 40: Albion (aço T3, pobre em ferro/carvão), Galia (pólvora T3, peso no Landsraad), Norden (T1, farto e marcial). Expressas como schema de campos, não casos especiais. |
| 3 | Mapa fixo de tutorial vs. sempre procedural no protótipo             | Pendente               |
| 4 | Multiplayer no protótipo vs. só single-player + IA                   | ✅ **TRAVADA** — adiado. Protótipo é single-player. Convenção fixada: **3 facções = jogador + 2 IA**. Consequência técnica: determinismo bit-exact entre máquinas **não** é requisito (ver doc 20 §4). |
| 5 | Nome final / branding do projeto                                     | Provisório: Polemarkos |
