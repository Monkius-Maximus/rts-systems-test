# Delta de Decisões — Sessão de Arquitetura

**Projeto:** Le Grand Polemarkos (LGP)
**Data da sessão:** 2026-06-21
**Natureza:** Changelog de design, **não** documento canônico. Os blocos abaixo são
para serem dobrados nos documentos numerados do vault. Onde diz **TRAVADO**, mover para
a seção apropriada do doc; onde diz **ABERTO**, adicionar à tabela de Decisões em Aberto.

---

## Resumo da sessão

Fechamos, em ordem de dependência, a **condição de vitória** e a **lista de bens + esqueleto
do ladder de PM**. Isso destrava as **identidades de facção** como próxima decisão. Pelo
caminho, esclarecemos o sentido de "determinístico" e abrimos pendências novas (entidade
Comandante, tier do conceito militar, ligação território↔produção, e uma tensão arquitetural
real em Godot — ver doc 30).

---

## doc 00 — Visão Geral e Escopo

- **§9 #1 Condição de vitória → TRAVADA.** Territorial/militar: controlar um limiar de % dos
  territórios capturáveis; eliminação total = vitória automática (caso degenerado de 100%).
  Forma deliberadamente simples (contagem de territórios, não VP). Balanceamento adiado: o
  valor do limiar e um timer de *hold* anti-flip-flop.
- **§9 #2 Identidades de facção → ABERTA, agora destravada como PRÓXIMA.** Depende de bens +
  ladder, que esta sessão travou. É literalmente o degrau inicial de cada facção nos ladders.
- **§9 #4 Multiplayer → reafirmado adiado.** Protótipo é single-player + IA. Convenção fixada:
  **3 facções = jogador + 2 IA.**

## doc 10 — Adaptação de Sistemas

- **§7 #1 Condição de vitória → TRAVADA** (espelha doc 00).
- **§7 #2 Lista dos ~12 bens → TRAVADA em 10 bens** (tabela abaixo). Respeita o teto de ~12
  do doc 00 §4 (teto, não meta) — não inflamos para bater o número.
- **§7 #3 Ladder de PM inicial de cada facção → ABERTA, PRÓXIMA.** É a identidade de facção.

### Bens travados (10)

| Bem | Papel | Insumos | Workforce | Critério (doc 10 §3.1) |
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

Fluxo das cadeias:

```
minério ┐
carvão  ┴─► AÇO  (ladder T1→T3) ─┬──────────────────► armamento ──────► unid. de choque/pesadas
                                 ├─ + pólvora + cobre ► armas + munição ► infantaria de linha
salitre ─► PÓLVORA (T1→T3) ──────┴─ + pólvora ───────► artilharia ──────► artilharia
cobre ──► (cartucho/latão, dobrado no PM de munição)
madeira ─► construção / naval
```

### Esqueleto do ladder de PM

- **PM = `Resource`/`.tres` com um `tier` inteiro.** ID canônico = `StringName` na camada de
  simulação — **esta lista de bens/PMs é a fonte de verdade dos enums** (doc 00 §7), definida
  na simulação e só consumida pela apresentação. (Ver tensão arquitetural no doc 30 abaixo.)
- **Só os intermediários têm ladder (aço, pólvora).** Os finais militares **herdam o tier
  alcançável dos seus insumos**; o interlock (tier do conceito militar × tier do PM dos bens
  constituintes) converte isso em efetividade. Resultado: **2 ladders propagam para todas as
  unidades.** Não se faz fuzil Bessemer com aço de bloomery.
- **Cobre é gargalo de *quantidade* (sourcing) para munição, não driver de tier** — mantém o
  modelo de 2 ladders intacto. Cria um eixo de sourcing distinto do ferro.
- **Gating de workforce** (doc 10 §3.2): tiers baixos rodam em **Trabalhador**; tiers altos
  exigem **Especialista** (o gargalo literal da assimetria).
- **Subir um degrau** = desbloquear o próximo PM via a árvore de upgrade da facção (15 sementes
  → upgrades, doc 10 §3.4) **+** ter Especialistas para operá-lo. Sem bem de "maquinário" como
  gate extra (evitar camada a mais).
- **Ladder-bandeira — aço (3 degraus):**
  - T1 — Forja bloomery: Trabalhador, baixo rendimento, tier baixo.
  - T2 — Pudlagem (alto-forno + pudlagem): transicional, Trabalhador + Especialista.
  - T3 — Conversor Bessemer / Siemens-Martin (open-hearth): Especialista, alto rendimento, tier alto.
- **Pólvora:** mesma forma de 3 degraus (artesanal → moinho → industrial); degraus a detalhar
  quando fecharmos a cadeia.

## doc 20 — Combate e Unidades

- **§4 Determinismo → ESCLARECER/FIXAR.** "Determinístico" = **mesma fórmula para o jogador e
  para a IA** (caminho único, sem auto-resolve separado), **não** bit-exact reproduzível entre
  máquinas. Bit-exact só seria necessário para lockstep multiplayer/replays — ambos fora de
  escopo. Fixar isso evita importar rigor de ponto-flutuante (overengineering).
- **§9 (nova) ABERTA — Entidade "Comandante" indefinida.** A Logística do comandante governa o
  cap de exército (§3), mas o comandante nunca é definido: de onde vem (unidade? herói? stat de
  facção?), o que é a "Logística" e como escala, se cada exército tem um. Bloqueia a
  implementação do cap de exército.
- **§9 #3 Movimento → reforçada como crítica e cruzada** (ver doc 30 #2). Nota: as duas
  referências do §2 divergem justamente aqui — Northgard é zona/tile, Spice Wars é região. O
  modelo de *blob* é o mesmo; a *granularidade de movimento*, não.
- **§9 (nova) ABERTA — "Tier do conceito militar" / roster de unidades.** É a **outra metade do
  interlock**. Fechamos o lado da produção (tiers de PM nos bens), mas a fórmula precisa do
  arquétipo de unidade e do seu tier (milícia vs linha vs guarda?). Sem isso, a efetividade não
  fecha. **Item mais load-bearing depois das facções.**

## doc 30 — Arquitetura Godot

- **§8 #1 Formato de save → TRAVADO: JSON legível, e "save exportável/inspecionável" é
  REQUISITO DE ARQUITETURA (não feature).** Consequências:
  - O estado autoritativo da partida vive **inteiro** na camada de simulação, numa estrutura
    serializável. **Nenhum estado autoritativo em nodes** — reforça doc 30 §2 e §6 (salvar a
    simulação, não a árvore de cena).
  - Referência de gênero: Unciv / Paradox (partida inteira em arquivo de texto; engine = regras
    sobre dados). Ver limite dessa abordagem em `principios-de-producao.md` §3 — **regras ficam
    em C# compilado; só conteúdo/parâmetro vira dado.**
  - Legível e diff-ável (era o critério do §6). JSON comprime bem; escala do protótipo
    (3 facções, ~30–50 províncias, 10 bens) torna tamanho/velocidade irrelevantes.
  - Performance: escrita ocorre em momentos discretos (autosave/save manual), **não por tick**.
    Autosave em intervalo e escrita em thread separada são mitigações *se medidas como
    necessárias* — não implementar preventivamente.
  - Mantém a disciplina de **SaveVersion bump por sessão** (campo de versão no arquivo + ponto
    de migração).
  - Paga em 3 frentes: debug (reproduzir bug colando um save), teste headless (fixtures para
    WS-0), modding/comunidade depois.
  - **Nota:** decidido antes da pesquisa P5 (`prompts-pesquisa-tecnica.md`). P5 fica reduzido a
    confirmar *como* serializar a simulação C# pura em JSON sem arrastar tipos Godot — não mais
    a escolher formato.
- **§8 #6 Libs C++ (delaunator-cpp, FastNoiseLite) → provável que NÃO precise port.** Godot 4
  traz `FastNoiseLite` nativo e `Geometry2D` tem triangulação de Delaunay. **A confirmar contra
  a doc oficial** (ver pack de pesquisa, prompt 1). Não assumir até verificar.
- **§7 / §2 (nova) ABERTA — Tensão pura-C# vs `Resource`/`StringName`.** O §2 quer a simulação
  em C# puro, sem `using Godot;`, headless-testável (§7 #1). Mas o §3 define os dados como
  `Resource` e os IDs como `StringName` — **que são tipos Godot** (§7 #2 quer que eles sustentem
  a arquitetura data-driven). Tensão: se as definições canônicas são `Resource`/`StringName`,
  a camada "pura" não pode possuí-las sem importar Godot. **Duas resoluções possíveis a decidir:**
  (a) simulação usa tipos C# puros (enums/records/IDs próprios) e a camada `.tres` é mecanismo
  de *loading* na apresentação que hidrata objetos da simulação; ou (b) a simulação referencia
  tipos Godot e "headless" passa a significar Godot `--headless`, não .NET puro. Afeta
  diretamente o critério de validação §7 #1. Roteado para o prompt de pesquisa 4.

## Novas pendências transversais (surgidas na sessão)

- **ABERTA — Ligação território ↔ produção.** Território é a unidade de captura/vitória; produção
  é o pilar; mas não dissemos *como* um território rende bens (cada território dá quais brutos?
  edifícios hospedam os PMs?). Amarra economia + movimento + vitória. Fechar logo após facções.
- **ABERTA — Camada de moeda/upkeep.** Existe um recurso de moeda *separado* (não um dos 10 bens)
  para manutenção/comércio? Os docs não mencionam. Indefinido.
- **ABERTA — Consumo de workforce.** Trabalhador/Especialista consomem algo (comida) ou são pool
  fixo/convertível sem consumo? Conecta ao manpower (doc 20 §7).

---

## Sequência de fechamento (status)

1. Condição de vitória — ✅ **travada**
2. Bens + esqueleto de ladder — ✅ **travada**
3. Identidades das 3 facções (degrau inicial por ladder) — ⬜ **próxima**
4. (depois) Tier do conceito militar / roster · Ligação território↔produção
