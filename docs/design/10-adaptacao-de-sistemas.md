# 10 — Adaptação de Sistemas: do GSG ao 4X-RTS

**Projeto:** Le Grand Polemarkos (LGP)
**Pré-requisito:** ler `00-visao-geral-e-escopo.md`
**Status do documento:** travado

Este documento mapeia, sistema por sistema, como os subsistemas do LGE se transformam ao
virar 4X-RTS. O framework é simples: cada sistema **porta direto**, **comprime** ou **morre**.

---

## 1. Framework: porta / comprime / morre

- **Porta direto** — o sistema sobrevive quase intacto e, em alguns casos, *brilha mais* no
  formato RTS do que no GSG.
- **Comprime** — o conceito sobrevive, mas a granularidade cai drasticamente para caber na
  largura cognitiva de uma partida de 2–4h.
- **Morre** — o sistema é incompatível com o gênero e precisa ser substituído por outra
  coisa (não apenas removido).

A regra de corte que governa as compressões: **manter o que expressa a assimetria de
produção; cortar o que é flavor.**

---

## 2. Portam direto

### 2.1 Assimetria de produção
Ironicamente, *mais* natural num 4X-RTS do que num GSG. Jogadores de RTS já entendem facções
assimétricas desde StarCraft (Terran/Zerg/Protoss produzem "exército" por métodos
completamente diferentes). Albion com Bessemer vs. Norden com bloomery vira a **identidade de
facção** — exatamente o que Spice Wars faz com Atreides vs. Fremen.

O pilar central não só sobrevive: ele vira o marketing do jogo.

### 2.2 Interlock mecânico
Porta intacto:

> **efetividade da unidade = tier do conceito militar × tier do método de produção dos
> bens constituintes**

Numa partida de 3h, ver o vizinho saltar de forja para conversor no minuto 40 é uma ameaça
*legível e dramática*. O interlock é o que torna a assimetria de produção uma decisão
estratégica, não cosmética. Detalhamento no doc 20, §5.

### 2.3 Pipeline Azgaar FMG
Porta — e fica até mais alinhado. Gerar mapas de partida procedurais (em vez de um mundo
fixo) é coerente com a replayability esperada de um 4X. O commandlet de importação do LGE
(UE5) não porta como código, mas o *fluxo* (FMG → dados de geografia → mapa jogável) porta
como conceito. Reimplementação em Godot no doc 30.

---

## 3. Comprimem

### 3.1 Bens: 31 → 10

Numa partida de 3h o jogador não tem largura cognitiva para 31 cadeias; Spice Wars opera com
~6 recursos. LGP fica no meio.

Regra de compressão:
- **Manter** as cadeias que expressam a assimetria (ex.: minério → aço → armamento), porque
  são onde o interlock militar vive.
- **Cortar** as cadeias de flavor (vidro, papel) que adicionam profundidade econômica mas
  não diferenciam facções nem alimentam o combate.

> **TRAVADO em 10 bens.** O teto do doc 00 §4 é ~12 — teto, não meta; não inflamos a lista para
> bater o número. O critério de seleção permanece: um bem só entra se (a) participa de uma cadeia
> militar, ou (b) é gargalo econômico que cria decisão estratégica.

| Bem | Papel | Insumos | Workforce | Critério |
|---|---|---|---|---|
| Minério de ferro | bruto | território | Trabalhador | cadeia militar |
| Carvão | bruto (combustível) | território | Trabalhador | gargalo + cadeia |
| Madeira | bruto | território | Trabalhador | gargalo + naval |
| Salitre | bruto | território | Trabalhador | cadeia militar |
| Cobre | bruto | território | Trabalhador | gargalo de sourcing (munição) |
| **Aço** | intermediário (laddered) | minério + carvão | Trab.→Especialista | cadeia militar — *bandeira* |
| **Pólvora** | intermediário (laddered) | salitre (+carvão) | Trab.→Especialista | cadeia militar |
| Armamento | final militar | aço | Especialista | cadeia militar |
| Armas de fogo + munição | final militar | aço + pólvora + **cobre** | Especialista | cadeia militar |
| Artilharia | final militar | aço + pólvora | Especialista | cadeia militar |

```
minério ┐
carvão  ┴─► AÇO  (ladder T1→T3) ─┬──────────────────► armamento ──────► unid. de choque/pesadas
                                 ├─ + pólvora + cobre ► armas + munição ► infantaria de linha
salitre ─► PÓLVORA (T1→T3) ──────┴─ + pólvora ───────► artilharia ──────► artilharia
cobre ──► (cartucho/latão, dobrado no PM de munição)
madeira ─► construção / naval
```

**Esta lista é a fonte de verdade dos identificadores de bem e de método de produção.** Ela é
definida na camada de simulação, em C# puro, e apenas consumida pela apresentação (doc 00 §7,
doc 30 §2). Ver doc 30 §9 quanto a por que ela **não** é `Resource`/`StringName`.

### 3.2 Pops: 9 profissões → 3 tipos de workforce
O modelo multidimensional de pops do LGE (profissão, cultura, riqueza, alfabetização como
campos armazenados) é o custo de simulação que **menos paga retorno** em sessões curtas.

Compressão: as nove profissões viram **três tipos de workforce**:
- **Trabalhador** — mão de obra genérica de produção primária.
- **Especialista** — opera métodos de produção avançados (o gargalo da assimetria).
- **Soldado** — manpower militar.

Cultura e religião (per-pop / territorial no LGE) **não entram** no protótipo. São profundidade
de GSG sem retorno em partida curta.

### 3.3 Política: três fases → modelo Landsraad
O sistema político de três fases negociadas do LGE comprime para o modelo Landsraad de Spice
Wars: **resoluções periódicas cronometradas** onde facções gastam capital político em votos.

É uma destilação honesta do design de leis negociadas, não uma traição — mantém a ideia de
capital político escasso (inspirado no monarch power/absolutism de EU4) como recurso, mas
remove a máquina de enactment de três fases, que pressupõe horizonte de campanha.

### 3.4 Emergência tecnológica: 7 esferas → ladder de partida
A emergência tecnológica do LGE (sete esferas civilizacionais ao longo de 150 anos) comprime
para **ladder de método de produção dentro da partida**. As 15 sementes de tecnologia do LGE
viram a **árvore de upgrade da facção** — o jogador sobe a ladder de PMs durante a partida, e
é justamente essa subida que expressa a assimetria em tempo real.

Coerente com o princípio do LGE de que a emergência tecnológica é **consumidora, não engine**:
ela continua sentando em cima de produção e workforce, não vira sistema novo.

### Esqueleto do ladder de PM (travado)

- **Só os intermediários têm ladder: aço e pólvora.** Os finais militares **herdam o tier
  alcançável dos seus insumos**; o interlock (doc 20 §5) converte isso em efetividade. Resultado:
  **2 ladders propagam para todas as unidades.** Não se faz fuzil Bessemer com aço de bloomery.
- **Cobre é gargalo de *quantidade* (sourcing) para munição, não driver de tier** — mantém o
  modelo de 2 ladders intacto e cria um eixo de sourcing distinto do ferro.
- **Gating de workforce** (§3.2): tiers baixos rodam em **Trabalhador**; tiers altos exigem
  **Especialista** — o gargalo literal da assimetria.
- **Subir um degrau** = desbloquear o próximo PM pela árvore de upgrade da facção (as 15 sementes,
  §3.4) **+** ter Especialistas para operá-lo. Sem bem de "maquinário" como gate extra.
- **Ladder-bandeira — aço, 3 degraus:**
  - T1 — forja bloomery: Trabalhador, baixo rendimento, tier baixo.
  - T2 — pudlagem (alto-forno + pudlagem): transicional, Trabalhador + Especialista.
  - T3 — conversor Bessemer / Siemens-Martin: Especialista, alto rendimento, tier alto.
- **Pólvora:** mesma forma de 3 degraus (artesanal → moinho → industrial); degraus a detalhar
  quando a cadeia fechar.

---

## 4. Morrem

### 4.1 Sistema de batalha por cartas
A maior baixa. RTS exige combate contínuo **no próprio mapa** — uma tela tática separada
quebra o fluxo de tempo real.

Substituição (não remoção): exércitos como grupos de unidades visíveis que lutam
automaticamente ao engajar. O conceito de **pontos de comando do comandante sobrevive** como
cap de tamanho de exército. O princípio "auto-resolver roda a mesma engine, sem cheating de
IA" também sobrevive. Detalhamento completo no doc 20.

### 4.2 Pops multidimensionais
Morre como modelo (ver §3.2 para a substituição). A profundidade de Project Alice / Victoria
não cabe em partida curta.

### 4.3 Simulação de calendário de 150 anos
Morre como mecânica literal (ver §5 para a substituição por abstração temporal).

---

## 4-A. Topologia da economia: pool global, produção espacial

Decisão travada. Ela resolve a pendência transversal "ligação território ↔ produção" e é o
contrato que o WS-1 implementa.

**Bens acumulam num pool único por facção.** Não há transporte a microgerenciar, não há estoque
por província, não há rota a proteger. O que é local não é o bem — é a **capacidade de produção**.

- **Dois conceitos distintos, que esta seção não confunde:**
  - *Ter ladder* — subir de degrau por tecnologia. Só os **intermediários** (aço, pólvora), §3.4.
  - *Carregar tier no pool* — o tier de origem viaja com o bem. **Intermediários e finais
    militares.** Só os **brutos** ocupam um balde único.
- Aço T1 e aço T3 são baldes distintos e nunca se misturam; o mesmo vale para um fuzil feito com
  aço T1 e outro feito com aço T3. O interlock lê o tier de onde o bem saiu.
- *Por que os finais carregam tier, se não têm ladder:* eles **herdam** o tier dos insumos (§3.4).
  Num balde único, um fuzil Bessemer e um fuzil de bloomery se misturariam, a herança se perderia
  e a regra de captura (§4-A.3) deixaria de valer para o que já está estocado. *Como* o tier é
  herdado é pendência (§7 #9); *que* os finais o carregam está travado.
- Débito sem saldo **lança**. Não há clamp: um clamp silencioso é um fallback disfarçado
  (doc 00 §7).

**Por que pool global e não logística.** Logística de transporte é um sistema inteiro — rotas,
capacidade, interdição, IA de roteamento — que não desbloqueia nada do loop jogável (produzir →
formar exército → capturar território → vencer). É profundidade de GSG, e o protótipo já paga a
assimetria de produção em outro lugar. Ver `principios-de-producao.md` §5: esta simplicidade vai
*parecer* faltando durante o desenvolvimento e não é.

### 4-A.1 Sítios de extração

- Todo bem bruto (minério, carvão, madeira, salitre, cobre) sai de um **depósito** no território.
- O depósito é **dormente** até ser guarnecido por Trabalhadores. Guarnecido, produz.
- A extração é **binária: não tem ladder.** Toda a progressão de tier vive no processamento.
  (Uma ladder na extração multiplicaria combinações sem acrescentar decisão — ver
  `principios-de-producao.md` §5.)

### 4-A.2 Edifícios de processamento e slots de província

- O processamento acontece em **edifícios**, e cada edifício ocupa um **slot de província**.
- **Um método de produção por edifício.** O slot é o gargalo espacial: subir o ladder disputa
  espaço com ampliar a base.
- Tier baixo opera com **Trabalhador**; tier alto exige **Especialista**.

### 4-A.3 Captura

Capturar uma província **transfere os edifícios com o tier do inimigo** — você leva a fábrica
Bessemer dele. Mas **operá-la no tier exige a *sua* workforce**: sem Especialistas suficientes, o
edifício fica **subguarnecido e o output é reduzido**, não zerado.

É o ponto onde combate, economia e território se amarram num nó só: tomar território do vizinho
mais avançado é um atalho tecnológico real, e ainda assim não é de graça.

---

## 4-B. Invariantes de escassez do gerador de mapa

O gerador não distribui recursos uniformemente. Ele **impõe invariantes**, porque é aí que a
assimetria de facção deixa de ser número e vira geografia:

1. **Nenhum começo é autossuficiente.** Toda facção precisa de algo que não tem em casa.
2. **Salitre e cobre são raros** — são os gargalos de sourcing da cadeia de munição.
3. **O coração metalúrgico (ferro + carvão junto) exige expansão.** Ninguém começa com os dois
   em volume.
4. **A fraqueza declarada de cada facção vira posição no mapa:** Albion pobre em ferro/carvão;
   Norden pobre em salitre; Galia com salitre.

Consequência de design: a decisão "para onde expandir" é lida no mapa, não numa tela de menu, e
o vizinho que tem o que falta é um alvo legível em tempo real.

---

## 5. Abstração temporal: 150 anos → ladder de partida

Ponto de design importante e fácil de errar.

Numa partida de 2–4h, **não se simula 150 anos de calendário**. A partida é uma *fatia*. O
cenário e a estética continuam os mesmos (Albion/Galia/Norden, era industrial), mas o tempo é
abstrato:

- A progressão tecnológica de 150 anos do LGE vira o **ladder de upgrade da facção** que se
  sobe durante a partida.
- Não há datas de calendário relevantes nem soft end-date civilizacional. O fim da partida é
  definido por condição de vitória (§7, item 1), não por uma data.
- O "salto" de um método de produção primitivo para um avançado, que no LGE levaria décadas
  de jogo, na partida acontece em minutos — e é exatamente isso que torna a ameaça do vizinho
  legível em tempo real.

---

## 6. Tabela-resumo de compressão

| Sistema do LGE                  | Destino   | Forma em LGP                                           |
|---------------------------------|-----------|--------------------------------------------------------|
| Assimetria de produção          | Porta     | Identidade de facção; vira marketing                   |
| Interlock militar               | Porta     | Mesma fórmula, ameaça em tempo real                    |
| Pipeline Azgaar FMG             | Porta     | Mapa procedural por partida                            |
| 31 bens                         | Comprime  | 10, só os que alimentam cadeia militar ou gargalo      |
| 9 profissões                    | Comprime  | 3 workforce (trabalhador / especialista / soldado)     |
| Política de 3 fases             | Comprime  | Resoluções periódicas no Landsraad                     |
| 7 esferas de tecnologia         | Comprime  | Ladder de PM dentro da partida (15 sementes → upgrades)|
| Batalha por cartas              | Morre     | Blobs de exército com auto-combate no mapa             |
| Pops multidimensionais          | Morre     | Substituído por 3 workforce                            |
| Calendário de 150 anos          | Morre     | Abstração temporal (ladder de partida)                 |
| Cultura/religião per-pop/territ.| Morre     | Fora do protótipo                                      |

---

## 7. Decisões em aberto

| # | Item                                                                  | Status   |
|---|-----------------------------------------------------------------------|----------|
| 1 | Condição de vitória do protótipo (compartilhada com doc 00 §9 #1)     | ✅ **TRAVADA** |
| 2 | Lista exata dos bens                                                  | ✅ **TRAVADA** — 10 bens, §3.1 |
| 3 | Ladder de PM inicial de cada facção (Albion/Galia/Norden)             | ✅ **TRAVADA** — doc 40 |
| 4 | Ligação território ↔ produção                                         | ✅ **TRAVADA** — §4-A |
| 5 | Capital político: gerado por quê? (território? população? edifícios?) | Pendente — bloqueia o Landsraad (WS-5), não o WS-1 |
| 6 | Quais resoluções existem no Landsraad do protótipo                    | Pendente — WS-5 |
| 7 | Camada de moeda/upkeep separada dos 10 bens                           | Pendente — **não** entra no protótipo até que algo do loop jogável a exija |
| 8 | Consumo de workforce (Trabalhador/Especialista consomem comida?)      | Pendente — pool fixo convertível é o comportamento assumido até decisão contrária |
| 9 | Regra de herança do tier dos finais (ex.: o menor tier entre os insumos com tier?) | Pendente — **bloqueia o WS-1** (produção de finais). *Que* os finais carregam tier está travado em §4-A; *como* ele é calculado, não |
| 10 | Identidade dos métodos de produção: degraus da pólvora; se cada final tem um PM ou vários; se um sítio de extração "roda um PM" | Pendente — **bloqueia o WS-1** (tabela de PMs). O aço está fechado (§3.4) |
