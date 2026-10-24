# 20 — Combate e Unidades

**Projeto:** Le Grand Polemarkos (LGP)
**Pré-requisito:** ler `00-visao-geral-e-escopo.md` e `10-adaptacao-de-sistemas.md`
**Status do documento:** travado quanto ao modelo; números de balanceamento em aberto

Este é o sistema com a maior divergência em relação ao LGE. O sistema de batalha por cartas
do LGE (doc 60 do LGE) **morre** aqui (ver doc 10, §4.1). Este documento define o que o
substitui.

---

## 1. A grande mudança: combate no mapa

Em RTS, o combate acontece **no próprio mapa**, em tempo real, não em uma tela tática
separada. Uma tela à parte quebraria o fluxo temporal que define o gênero.

Consequência: exércitos deixam de ser abstrações em uma tela de batalha e passam a ser
**grupos de unidades visíveis** que se movem pelo mapa e lutam automaticamente ao se engajar.

---

## 2. Modelo de blob de exército

O modelo adotado (fork 3, travado no doc 00) é o de **blobs de exército com auto-combate** —
o mesmo de Spice Wars / Northgard.

- Um exército é um **grupo de unidades** representado no mapa.
- Ao entrar em contato com um exército inimigo, o combate é **automático** — não há micro de
  unidade individual.
- O jogador decide **posicionamento e composição**, não ações por unidade por segundo.
- A composição do exército (quais unidades, produzidas por quais métodos) é onde a assimetria
  de produção se expressa no combate.

Explicitamente **fora de escopo** (rejeitado no fork 3): pathfinding de centenas de unidades
individuais, balanceamento de micro, IA de esquadrão. Isso é RTS clássico e seria
overengineering para o protótipo.

> **Atenção — as duas referências divergem na granularidade de movimento.** Northgard é
> zona/tile; Spice Wars é região com movimento mais contínuo. O modelo de *blob* é o mesmo nas
> duas; a *granularidade de movimento* não. Ver §9 #3.

---

## 3. Pontos de comando como cap de exército

O conceito de **pontos de comando do comandante** do LGE sobrevive — mas com função
redefinida. No LGE, pontos de comando vinham da Logística do comandante + doutrina e
governavam a batalha por cartas. Em LGP, eles viram **cap de tamanho de exército**:

- A Logística do comandante define quantas unidades um exército pode conter.
- Um comandante melhor → exército maior → mais poder de blob.
- Isso mantém o comandante como peça relevante sem reintroduzir a máquina de cartas.

> A **entidade** Comandante nunca foi definida — de onde vem, o que é "Logística", se cada
> exército tem um. Ver §9 #7. É bloqueio de implementação, não detalhe.

---

## 4. Engajamento determinístico

Princípio herdado do LGE, **não-negociável**: o auto-resolver roda a mesma engine, sem
information cheating na IA.

Aplicado ao combate no mapa:

- O resultado de um engajamento é **determinístico**, calculado pela mesma fórmula para o
  jogador e para a IA.
- Não há uma fórmula separada de "auto-resolve" e outra de "combate manual" — há um caminho
  só (princípio "one way").
- A IA não tem acesso a informação que o jogador não teria. Sua vantagem (se houver) vem de
  dificuldade declarada, não de trapaça oculta.

**Precisão do termo "determinístico" (fixada).** Aqui ele significa **mesma fórmula para o jogador
e para a IA** — caminho único, sem auto-resolve separado. **Não** significa bit-exact reproduzível
entre máquinas e versões de runtime. Bit-exact só seria exigido por lockstep multiplayer ou por
replay, ambos fora de escopo (doc 00 §9 #4); importar rigor de ponto flutuante sem esse requisito
seria overengineering.

Isso **não** dispensa o determinismo de simulação que o núcleo já implementa (mesma seed → mesma
partida, via `DeterministicRandom` e ordem de iteração definida). As duas coisas são distintas: a
primeira é uma promessa de *justiça* para com a IA; a segunda é uma propriedade de *reprodução*
que paga em teste e em debug.

---

## 5. Fórmula de efetividade no contexto RTS

A fórmula de interlock do LGE porta direto (doc 10, §2.2):

> **efetividade da unidade = tier do conceito militar × tier do método de produção dos bens
> constituintes**

No contexto RTS, isso significa:

- Duas facções com o mesmo número de unidades podem ter poder de combate completamente
  diferente, porque uma produz seus armamentos por bloomery (tier baixo) e a outra por
  Bessemer (tier alto).
- Subir a ladder de PM (doc 10, §3.4) durante a partida **aumenta a efetividade das unidades
  futuras** — é o vetor primário de poder militar, e portanto a expressão em tempo real do
  pilar de assimetria.
- O jogador que negligencia a cadeia produtiva perde no campo mesmo com paridade numérica.

Esta é a razão de o combate não poder ser desacoplado da economia: o poder de blob é uma
*função* da cadeia de produção, não um número independente.

> **O lado da produção está fechado** (tiers de PM nos bens, doc 10 §3.1). O **tier do conceito
> militar** — a outra metade desta fórmula — **não está definido**. Ver §9 #8.

---

## 6. O que o jogador controla

Resumo explícito da superfície de controle, para evitar drift de escopo:

| O jogador controla            | O jogador NÃO controla                          |
|-------------------------------|--------------------------------------------------|
| Posicionamento de exércitos   | Ações de unidade individual (sem micro)          |
| Composição de exércitos       | Pathfinding fino dentro do blob                  |
| Quando engajar / recuar       | Resolução do engajamento (determinística)        |
| Qual ladder de PM priorizar   | Fórmula de efetividade (fixa)                     |
| Tamanho via comandante        | —                                                |

---

## 7. Manpower e drain de pops

Porta do LGE com a compressão de pops do doc 10, §3.2:

- O drain de manpower em combate consome **pops militares (soldados) primeiro** — princípio
  herdado do LGE.
- Como o modelo de pops foi comprimido para três tipos de workforce, o drain é mais simples:
  baixas reduzem a contagem de soldados; reforçar exige reconverter workforce em soldado.
- Não há drain por cultura, riqueza ou alfabetização (esses campos não existem em LGP).

---

## 8. Naval (auto-resolve)

O LGE já usava auto-resolve para combate naval. Em LGP, **todo combate é resolvido pela mesma
engine determinística** (§4), então naval não é um caso especial — é o mesmo caminho. Se o
protótipo incluir naval (decisão em aberto), ele roda a mesma fórmula de efetividade aplicada
a unidades navais.

---

## 9. Decisões em aberto

| # | Item                                                                     | Status   |
|---|--------------------------------------------------------------------------|----------|
| 1 | Naval entra no protótipo ou fica para depois                             | Adiado — candidato nº 2 de corte (`principios-de-producao.md` §7) |
| 2 | Números de balanceamento (tamanho de blob por Logística, dano)           | Pendente — balanceamento, não design; só se resolve jogando |
| 3 | **Modelo de movimento no mapa** (zona/tile tipo Northgard vs. região contínua tipo Spice Wars) | 🔴 **CRÍTICO E CRUZADO** (doc 30 §10 #2). As duas referências do §2 divergem exatamente aqui: o modelo de *blob* é o mesmo, a *granularidade de movimento* não. Bloqueia WS-4. |
| 4 | Detecção/fog of war: exércitos inimigos sempre visíveis ou ocultos       | Pendente |
| 5 | Recuo: livre, ou com penalidade/risco de rota                            | Pendente |
| 6 | Reforço em campo vs. apenas em território próprio                        | Pendente |
| 7 | **Entidade "Comandante" indefinida**                                     | 🔴 **BLOQUEIA WS-4.** A Logística do comandante governa o cap de exército (§3), mas o comandante nunca foi definido: de onde vem (unidade? herói? stat de facção?), o que é "Logística" e como escala, se cada exército tem um. |
| 8 | **"Tier do conceito militar" / roster de unidades**                      | 🔴 **BLOQUEIA WS-4.** É a **outra metade do interlock** (§5). O lado da produção está fechado (tiers de PM nos bens); falta o arquétipo de unidade e seu tier — milícia vs. linha vs. guarda. Item mais load-bearing do design restante. |
