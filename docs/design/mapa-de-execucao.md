# Mapa de Execução do Projeto — LGP

**Projeto:** Le Grand Polemarkos (LGP)
**Natureza:** documento de coordenação (não é doc de design canônico). É o "centro de comando":
quais frentes existem, em que ordem atacá-las, e o prompt + material para abrir cada uma em um
chat de design, uma sessão de pesquisa, ou no Claude Code.
**Meta final:** protótipo jogável → publicação na Steam.

---

## 0. Como usar este mapa

Três tipos de trabalho, cada frente marcada por um:

- **[Design]** — chat fresco do Claude, modo propor-confirmar (como a sessão de arquitetura).
  Fecha decisões abertas antes de codar.
- **[Pesquisa]** — sessão com web, usando os prompts de `prompts-pesquisa-tecnica.md`. Verifica
  API/lib contra a doc.
- **[Código]** — Claude Code, seu onboarding de duas rodadas: **recon** (auditoria + relatório
  escrito, sem código) → aprovação → **implementação**.

Você é solo: a **ordem de dependência** importa mais que paralelizar. Siga as fases (§2).

**Material recorrente** (anexar conforme indicado em cada frente):
`00-visao-geral-e-escopo.md` · `10-adaptacao-de-sistemas.md` · `20-combate-e-unidades.md` ·
`30-arquitetura-godot.md` · `delta-decisoes-sessao.md` · `prompts-pesquisa-tecnica.md` ·
`nacoes-e-geracao-procedural.md` · este mapa.

---

## 1. Estado de design (atualizado nesta sessão — dobrar nos docs numerados)

Travado desde o último delta:

- **Topologia da economia → pool global + produção espacial (dobrar em doc 10 / doc 30).** Bens
  acumulam num pool único por facção; o que é local é a *capacidade de produção* (extração só
  onde há bruto; indústria ocupa slot de província). Sem transporte a microgerenciar. Pool é
  **tier-aware** para bens laddered (aço T1 e aço T3 são buckets distintos).
- **Sítios de produção → doc 10 / doc 30.** Extração: depósito dormente até receber
  Trabalhadores (binário, sem ladder). Processamento: edifícios em **slots de província**,
  um PM por edifício; tier baixo = Trabalhador, tier alto = Especialista (gargalo espacial).
  Captura transfere edifícios **com o tier do inimigo**, mas operá-los no tier exige *sua*
  workforce (equipe incompleta = output reduzido).
- **Escassez → doc 10.** Gerador de mapa impõe invariantes: nenhum começo autossuficiente;
  salitre/cobre raros; coração metalúrgico (ferro+carvão) exige expansão. Fraqueza de facção
  vira geografia (Albion pobre em ferro/carvão; Norden pobre em salitre; Galia com salitre).
- **Diplomacia elevada a foco central (ao lado do combate) → exige cluster de design novo.** Hoje
  os docs só têm o **Landsraad** (arena multilateral de votos). Diplomacia **bilateral**
  (alianças, pactos, guerra/paz, comércio) é **não especificada**. Decisão de escopo nova: o
  Landsraad *é* a diplomacia, ou precisa de camada bilateral por cima? Ver frente **WS-5**.

Pendências de design ainda abertas que bloqueiam código: tier do conceito militar, entidade
Comandante, modelo de movimento (doc 20), capital político (doc 10 §7 #4), resolução da tensão
pura-C# vs `Resource` (doc 30).

---

## 2. Fases (caminho crítico para um jogável)

> Princípio: **fatia vertical jogável primeiro** (produzir → formar exército → capturar
> território → vencer), depois profundidade. Combate e diplomacia são o foco central, mas
> diplomacia é a mais *aberta*, então desenha-se cedo (chat) e implementa-se depois da fatia.

| Fase | Objetivo | Frentes | Desbloqueia |
|---|---|---|---|
| **A — Fundação** | Esqueleto da simulação C# pura testável | WS-0 + Pesquisa P1, P4 | tudo |
| **B — Substrato** | Mapa + economia rodando headless | WS-1, WS-2 + Pesquisa P2, P3 | combate ter sentido |
| **C — Fatia vertical** | Primeiro *jogável* (loop completo) | WS-4 (mínimo), WS-6, WS-8 (mínimo) | validar o pilar em jogo |
| **D — Profundidade** | Identidade + rejogabilidade + oponente | WS-3, WS-5, WS-7 | jogo "de verdade" |
| **E — Steam** | Polish, build, loja | WS-8 (completo), empacotamento | publicação |

Design das frentes abertas (WS-4 itens abertos, WS-5 diplomacia) pode correr **em paralelo** em
chats próprios durante as Fases A–B, já que não dependem de código existir.

---

## 3. Frentes de trabalho (cards)

### WS-0 — Núcleo de simulação (fundação) · [Pesquisa]→[Código]
- **Escopo:** arquitetura de duas camadas (sim C# puro / apresentação nodes); harness de teste
  headless; resolução da tensão `Resource`/`StringName`; primeiro objeto-prova (o pool de
  recursos, que não tem dependência de node).
- **Depende de:** nada. **Bloqueia:** todas as outras.
- **Antes:** rodar **Pesquisa P1** (libs) e **P4** (headless + tensão) do pack.
- **Prompt de recon (Claude Code):** ver template em §4.
- **Material:** doc 30, `prompts-pesquisa-tecnica.md`, resultado de P4.

### WS-1 — Economia (o pilar) · [Código]
- **Escopo:** os 10 bens; sistema de PM com ladder (aço/pólvora); pool global tier-aware;
  sítios de extração e edifícios de processamento; slots de província; atribuição de workforce
  (Trabalhador/Especialista) e conversão.
- **Depende de:** WS-0. **Acopla com:** WS-2 (províncias hospedam produção).
- **Status:** desenhado (10 bens, ladders, pool, sítios). Pronto para recon→implementar.
- **Material:** doc 10, `delta-decisoes-sessao.md`, este mapa (§1).

### WS-2 — Território e mapa · [Pesquisa]→[Código]
- **Escopo:** import Azgaar FMG; derivação terreno→bruto; modelo de província (unidade
  capturável); slots de edifício; render em `TileMapLayer`.
- **Depende de:** WS-0. **Acopla com:** WS-1.
- **Antes:** **Pesquisa P2** (import FMG) e **P3** (Better Terrain).
- **Material:** doc 30, `prompts-pesquisa-tecnica.md`, resultados de P2/P3.

### WS-3 — Nações e gerador procedural · [Código]
- **Escopo:** `FactionDefinition` (schema de campos); gerador com orçamento de poder; modos
  debug/ingame; seed. As três facções canônicas como instâncias.
- **Depende de:** WS-1 (campos referenciam bens/PM/ladder), WS-0 (resolução `Resource`).
- **Status:** desenhado (`nacoes-e-geracao-procedural.md`). Implementar pela camada **Núcleo**
  do schema primeiro.
- **Material:** `nacoes-e-geracao-procedural.md`, doc 30.

### WS-4 — Combate (foco central) · [Design]→[Código]
- **Escopo:** blobs de exército; auto-combate determinístico; fórmula de interlock; cap de
  exército via Comandante. **Itens abertos a fechar antes de codar:** tier do conceito militar,
  roster de unidades, modelo de movimento, definição da entidade Comandante.
- **Depende de:** WS-1 (unidades consomem bens no tier), WS-2 (movimento no mapa).
- **Antes:** **[Design] chat de combate** (prompt em §4) para fechar os itens abertos.
- **Material:** doc 20, doc 00 (§0), `delta-decisoes-sessao.md`.

### WS-5 — Diplomacia (foco central, mais aberto) · [Design]→[Código]
- **Escopo:** Landsraad (multilateral, em esboço) **+** decisão de escopo: bilateral ou não.
  Se bilateral: alianças, pactos de não-agressão, guerra/paz, acordos comerciais, e a IA
  diplomática correspondente.
- **Depende de:** WS-3 (facções), WS-6 (fluxo de partida). **Cluster de design novo.**
- **Antes:** **[Design] chat de diplomacia** (prompt em §4). Pesa escopo vs nota de dispersão.
- **Material:** doc 00 (§0, §8), doc 10 (§3.3 Landsraad), este mapa (§1).

### WS-6 — Fluxo de partida e vitória · [Código]
- **Escopo:** ciclo de vida da partida; loop de tick/pausa; condição de vitória territorial
  (limiar de %, eliminação como caso degenerado); win/loss.
- **Depende de:** WS-1, WS-2, WS-4 (a maior parte dos sistemas).
- **Status:** vitória desenhada; loop a especificar.
- **Material:** doc 00, doc 10 (§5 abstração temporal), `delta-decisoes-sessao.md`.

### WS-7 — IA · [Design]→[Código]
- **Escopo:** os 2 oponentes rodando a **mesma engine determinística** (sem cheating, doc 20
  §4); IA econômica + de combate + (se houver) diplomática. Dificuldade por parâmetro declarado.
- **Depende de:** WS-4, WS-5 (atravessa ambos).
- **Material:** doc 20 (§4), saídas de WS-4 e WS-5.

### WS-8 — Apresentação / UI · [Código]
- **Escopo:** camada de nodes; painéis (Produção, Landsraad, FactionLadder); interação do mapa;
  câmera. Mínimo na Fase C, completo na Fase E.
- **Depende de:** lê estado das frentes; sobe por cima de tudo.
- **Material:** doc 30 (§4 estrutura de nodes), `frontend-design` skill quando for ao Claude Code.

---

## 4. Ações imediatas e prompts

### Abrir agora (em paralelo)

1. **Pesquisa** — rodar **P1** e **P4** (`prompts-pesquisa-tecnica.md`). Desbloqueiam WS-0.
2. **[Design] Diplomacia** — é o foco central mais aberto; comece o design cedo. Prompt abaixo.
3. **[Design] Combate** — fecha os itens que bloqueiam WS-4. Prompt abaixo.

### Prompt — [Design] chat de Diplomacia (colar em chat novo, anexar material de WS-5)

```
Contexto: LGP, 4X em tempo real lento e pausável (Godot 4.3+/C#). Estou em fase de
design, modo propor-confirmar — NÃO escreva código. Anexei os docs do projeto.

A diplomacia foi elevada a foco central do jogo, ao lado do combate. Hoje os docs só
têm o Landsraad (arena multilateral de votos cronometrados, estilo Spice Wars). Falta
toda a diplomacia bilateral.

Quero fechar, em ordem de dependência e no modo propor-depois-confirmar:
1. Decisão de escopo: o Landsraad É a diplomacia, ou preciso de uma camada bilateral
   por cima? Pese contra a nota de dispersão (doc 00 §8) e o escopo de dev solo.
2. Se bilateral: que primitivas entram no protótipo (aliança, não-agressão, guerra/paz,
   comércio de bens do pool, voto coordenado no Landsraad)?
3. Como a diplomacia serve a vitória territorial (alianças e traições a serviço de
   tomar território), sem virar um sistema desacoplado.
4. O que isso exige da IA (WS-7) e quais campos novos de facção/estado surgem.

Pesquise como o gênero resolve isso (Landsraad de Spice Wars, diplomacia de EU4/Vic3)
e traga os padrões que a comunidade desse tipo de jogo costuma gostar. Recomende uma
opção por bifurcação com justificativa e espere minha confirmação. Ao final, gere
blocos de delta prontos para colar como um doc de diplomacia novo no meu vault.
```

### Prompt — [Design] chat de Combate (colar em chat novo, anexar material de WS-4)

```
Contexto: LGP, 4X em tempo real lento e pausável (Godot 4.3+/C#). Fase de design, modo
propor-confirmar — NÃO escreva código. Anexei os docs.

O modelo de combate está travado (blobs de exército com auto-combate no mapa, doc 20).
Faltam os itens que bloqueiam a implementação. Quero fechá-los em ordem de dependência:
1. "Tier do conceito militar": é a outra metade do interlock (efetividade = tier do
   conceito militar × tier do PM dos bens). Defina o que é — arquétipos de unidade e
   seus tiers (milícia/linha/guarda?).
2. Roster de unidades do protótipo, derivado dos finais militares (armamento → choque;
   armas+munição → infantaria de linha; artilharia). Quais unidades, mínimas.
3. Entidade Comandante: de onde vem, o que é a "Logística" que vira cap de exército, se
   cada exército tem um.
4. Modelo de movimento no mapa (pendência cruzada doc 20 §9 #3 / doc 30 §8 #2): zona/
   tile (tipo Northgard) vs região contínua (tipo Spice Wars). Pesquise os dois e o que
   o Godot 4.3+ oferece nativamente (NavigationServer2D, A*); recomende para dev solo.

Recomende uma opção por bifurcação com justificativa e espere confirmação. Mantenha o
guarda do pilar: a força vem da produção/interlock, não de buffs planos. Ao final, gere
blocos de delta prontos para colar no doc 20 e onde mais tocar.
```

### Template — [Código] recon no Claude Code (parametrizar por frente)

```
Você é o engenheiro do projeto LGP (Godot 4.3+/C#, apresentação 2D). Esta é a RODADA DE
RECONHECIMENTO: NÃO escreva código de produção. Faça auditoria e entregue um relatório
escrito; eu aprovo antes de qualquer implementação.

Convenções: código em inglês; documentação e comentários em PT-BR; conversa em PT-BR.
Princípios não-negociáveis (governam tudo): fail fast, sem fallbacks, one way, mudanças
cirúrgicas, sem overengineering, one source of truth para enums, separação de concerns.

Frente desta sessão: <WS-N — nome>.
Docs de design (anexados / no repo): <listar o material da frente>.

Entregue um relatório com:
1. Mapa do que o design desta frente exige vs o estado atual do repositório.
2. Lacunas, riscos e qualquer mismatch (especialmente enums: a apresentação só pode
   referenciar tipos definidos pela camada de simulação — ver doc 30).
3. Plano de implementação em passos pequenos e cirúrgicos, na ordem de dependência.
4. Pontos onde uma API/lib precisa ser verificada na doc antes de assumir (não suponha
   API do Godot de memória; a API muda entre versões).

Pare no relatório. Aguarde minha aprovação explícita para a rodada de implementação.
```

---

## 5. Onde dobrar as decisões desta sessão

- Blocos de §1 (topologia, sítios, escassez) → **doc 10** e **doc 30**.
- Diplomacia como cluster novo → **doc novo** (sugestão: doc 50), gerado pelo chat de WS-5.
- Schema de nações → **doc 40** (candidato), de `nacoes-e-geracao-procedural.md`.
- Saídas dos chats de design (WS-4, WS-5) e das pesquisas (P1–P6) → de volta aos docs
  correspondentes, mantendo o vault como fonte de verdade.
