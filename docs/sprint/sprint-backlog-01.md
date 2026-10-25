# Sprint Backlog 01 — LGP

**Projeto:** Le Grand Polemarkos (LGP) · **Repositório:** `Monkius-Maximus/rts-systems-test`
**Fase do mapa de execução:** A (Fundação) → entrada na B (Substrato)
**Meta do sprint:** **fechar o WS-0 e deixar o WS-1 (economia) pronto para recon→implementação.**
**Criado:** 2026-09-13 · **Atualizado:** 2026-10-24

---

## 0. Estado verificado

Auditoria de 2026-10-24 (`HEAD` = merge do PR #1, `dba06b3`, de 2026-09-11, mais o commit de
documentação deste sprint).

| Entregável | Estado |
|---|---|
| WS-0 passo 1 — esqueleto da solução (`Lgp.sln`, `src/Lgp.Core`, `tests/Lgp.Core.Tests`, `.gitignore`, CI) | ✅ feito |
| WS-0 passo 2 — harness headless (`dotnet test` sem Godot) | ✅ feito |
| WS-0 passo 3 — `ArchitectureTests` (guarda da separação de camadas) | ✅ feito |
| WS-0 passo 5 — `DeterministicRandom` (PCG32 + testes) | ✅ feito |
| Docs de design no repo | ✅ feito — `docs/design/` (S1-01) |
| Consolidação nos docs canônicos | ✅ feito — delta S1-02 aplicado |
| WS-0 passo 4 — identidade e enums | ✅ feito (S1-03) |
| WS-0 passo 6 — pool de recursos tier-aware (objeto-prova) | ✅ feito (S1-04) |
| WS-0 passo 7 — projeto Godot + fio de ligação | 🔲 **próximo** — fecha o WS-0 |
| `game/` — projeto Godot | ❌ não existe |

**Leitura do estado:** o bloqueio documental que travou o sprint por seis semanas foi removido.
Nada mais impede o WS-0 de fechar — os três passos restantes são código.

---

## 1. Backlog do sprint

Quatro trilhas. A trilha **Código** é sequencial (ordem de dependência); **Design** e **Pesquisa**
correm em paralelo porque não dependem de código existir (mapa §2).

### Trilha A — Destravar

#### S1-01 · Commitar o vault de design no repositório ✅ **FEITO**
- **Tipo:** [Repo] · **Bloqueava:** S1-03, S1-04, S1-06 e todo o WS-1
- **Entregue:** `docs/design/` com os canônicos 00–40, a coordenação (`mapa-de-execucao.md`,
  `principios-de-producao.md`), os deltas como histórico e o `baseline-fontes-de-verdade.md`;
  `docs/pesquisa/prompts-pesquisa-tecnica.md`; este backlog em `docs/sprint/`.
  `CLAUDE.md` sem o aviso de bloqueio.

#### S1-02 · Dobrar as decisões pendentes nos docs canônicos ✅ **FEITO**
- **Tipo:** [Repo/Design] · **Entregue:** blocos B–E do
  `docs/design/delta-s1-02-consolidacao-pre-ws1.md` aplicados nos docs 00/10/20/30. Doc 10 §7 e
  doc 30 §10 não têm mais pendência que o WS-1 precise consultar.

---

### Trilha B — Código (Claude Code, recon→implementação)

#### S1-03 · WS-0 passo 4 — identidade de domínio e enums ✅ **FEITO**
- **Entregue:** `GoodId`, `GoodRole`, `GoodCatalog`, `ProductionTier`, `WorkforceType`, mais
  `EnumConventionTests`. Decisões em `docs/recon/S1-03-S1-04-recon.md` §7. Identidade de PM foi
  para o WS-1 (doc 10 §7 #10); o conversor de fronteira, para o WS-2.
- **Tipo:** [Código] · **Depende de:** nada (desbloqueado) · **Bloqueia:** S1-04
- **Escopo:** fixar, na camada de simulação e só nela, os tipos canônicos derivados do doc 10 §3.1:
  `GoodId` (os 10 bens travados), `WorkforceType` (Trabalhador/Especialista/Soldado),
  `ProductionTier` (T1–T3), e a identidade de método de produção. Documentar a regra de fronteira
  com GDScript (doc 30 §9.1): nenhum tipo da simulação atravessa; só `int`/`Vector2I`, com
  conversão num ponto único que falha alto.
- **Não entra:** tabela de PMs completa com rendimentos (é WS-1), qualquer tipo de apresentação.
- **Aceite:** enums no núcleo, sem duplicação; teste que cobre a conversão de fronteira falhando
  alto em valor não mapeável; `dotnet test` verde.
- **Estimativa:** 1 sessão.

#### S1-04 · WS-0 passo 6 — objeto-prova: pool global tier-aware ✅ **FEITO**
- **Entregue:** `StockKey` e `GoodsPool`, com os finais carregando tier (opção A do recon).
- **Tipo:** [Código] · **Depende de:** S1-03 · **Bloqueia:** WS-1 inteiro
- **Escopo:** buckets por `(bem, tier)`; crédito/débito; **débito sem saldo lança** (sem clamp —
  clamp silencioso é fallback disfarçado). Bens sem ladder ocupam um único bucket; aço e pólvora
  ocupam um por tier. Contrato em doc 10 §4-A.
- **Aceite:** testes de separação por tier, débito inválido (lança), conservação total, e
  iteração em ordem determinada (doc 30 §9.2, regra 2).
- **Estimativa:** 1 sessão.

#### S1-05 · WS-0 passo 7 — projeto Godot 4.7.2 e o fio de ligação
- **Tipo:** [Pesquisa curta]→[Código] · **Depende de:** S1-04 · **Fecha o WS-0**
- **Escopo:** `game/` com `project.godot` **gerado pelo editor 4.7.2** (nunca escrito à mão),
  `.csproj` do jogo com `ProjectReference` para `Lgp.Core`, e **um** smoke: cena que lê o pool e
  imprime. Ajustar a CI para continuar rodando `dotnet test` sem o Godot instalado.
- **Não entra:** UI (WS-8), mapa (WS-2).
- **Aceite:** o jogo abre e mostra estado vindo do núcleo; `ArchitectureTests` continua verde;
  CI verde.
- **Estimativa:** 1–1,5 sessão.

#### S1-06 · Espinha do estado serializável (`MatchState` + round-trip JSON)
- **Tipo:** [Código] · **Depende de:** S1-04
- **Por que agora:** `principios-de-producao.md §2` classifica o save exportável como decisão que
  **muda a forma do sistema** — gratuita agora, cara retrofitada. Ele também é o que dá fixtures
  triviais ao harness headless.
- **Escopo mínimo:** uma raiz `MatchState` serializável que hoje contém só o pool e a seed; campo
  `SaveVersion`; teste de round-trip. Sem migração, sem autosave, sem thread de escrita.
- **Aceite:** round-trip verde; JSON legível e diff-ável; nenhum tipo do Godot no arquivo.
- **Estimativa:** 1 sessão.

---

### Trilha C — Design em paralelo (chats separados, propor-confirmar)

#### S1-07 · Identidades das 3 facções ✅ **FECHADO**
- Decidido em `docs/design/40-nacoes-e-geracao-procedural.md`: Albion (aço T3, pobre em
  ferro/carvão), Galia (pólvora T3, peso no Landsraad), Norden (T1, farto e marcial), expressas
  como schema de campos com orçamento de poder — não como casos especiais. Dobrado no doc 00 §9 #2
  e doc 10 §7 #3.

#### S1-08 · Combate — fechar os itens que bloqueiam o WS-4 🔴 **único bloqueio de design restante**
- **Tipo:** [Design] · **Depende de:** nada · **Bloqueia:** WS-4, WS-7
- **Escopo:** os quatro itens abertos (doc 20 §9 #3, #7, #8): tier do conceito militar, roster
  mínimo do protótipo, entidade Comandante/Logística, e modelo de movimento (zona/tile vs. região
  contínua). **Prompt pronto no `mapa-de-execucao.md §4`** — atualizar a versão da engine para
  4.7.2 antes de usar.
- **Aceite:** blocos de delta para o doc 20 §9; a decisão de movimento registrada também no
  doc 30 §10 #2.
- **Estimativa:** 1–2 sessões de chat.

---

### Trilha D — Pesquisa (desbloqueia a Fase B)

#### S1-09 · P2 (import Azgaar FMG) e P3 (Better Terrain em 4.7)
- **Tipo:** [Pesquisa] · **Depende de:** nada · **Bloqueia:** WS-2
- **Escopo:** esquema real do GeoJSON de células do FMG (contra um export real, não contra a wiki)
  e **compatibilidade declarada do Better Terrain com 4.7 especificamente** — o repositório não a
  afirma, e é dependência de terceiro no caminho crítico. Prompts em
  `docs/pesquisa/prompts-pesquisa-tecnica.md`; corrigir "4.3+" para 4.7.2 nas regras comuns antes
  de rodar.
- **Aceite:** `docs/pesquisa/P2-P3-verificacao-tecnica.md` commitado, no mesmo formato do P1/P4;
  se o Better Terrain não suportar 4.7, o relatório traz a alternativa avaliada.
- **Estimativa:** 1 sessão.

---

## 2. Ordem de execução sugerida

```
S1-03 → S1-04 → S1-05 → S1-06        (trilha código, sequencial — fecha o WS-0)
      ‖ S1-08  (chat de combate — único bloqueio de design restante)
      ‖ S1-09  (pesquisa P2/P3)
```

**Fora do sprint, deliberadamente** (`principios-de-producao.md §1 e §7`): diplomacia bilateral
(WS-5), gerador procedural de nações (o schema espera; o gerador é candidato nº 1 a adiar), IA,
UI, naval. Nada disso encurta o caminho até a primeira partida jogável feia.

---

## 3. Definition of Done do sprint

- [ ] `dotnet test` verde, sem Godot instalado, em todos os commits da branch principal.
- [ ] `ArchitectureTests` intacto (o núcleo continua sem referência ao Godot).
- [x] Os docs de design estão no repo e o `CLAUDE.md` não cita bloqueio.
- [ ] WS-0 marcado como **fechado** no `mapa-de-execucao.md §3` — os 7 passos executados.
- [x] Doc 10 §7 e doc 30 §10 sem pendência que o WS-1 precise consultar.
- [ ] Cada sessão de código entregou um commit por passo, em PT-BR, com o recon aprovado antes.

**Critério de saída:** consigo abrir uma sessão de recon do **WS-1** anexando apenas
`docs/design/` do próprio repositório, sem precisar colar nada de fora. ✅ já satisfeito.

---

## 4. Riscos deste sprint

| Risco | Mitigação |
|---|---|
| `project.godot` de 4.7.2 escrito à mão diverge do formato da versão | Gerar pelo editor e commitar o resultado |
| Better Terrain não suportar 4.7 | S1-09 descobre isso **antes** do WS-2 começar, não durante |
| O chat de combate (S1-08) abrir escopo em vez de fechar | Guarda do pilar no prompt: força vem da produção/interlock, não de buffs planos |
| `MatchState` (S1-06) virar refatoração grande | Escopo mínimo declarado: só pool + seed. Cresce junto com os sistemas, não antes |
| **Sprint parado por semanas sem commit** (aconteceu: 11/09 → 24/10) | Sinal de alerta do `principios-de-producao.md §8`. O bloqueio era documental e custava 30 min — priorizar destravar sobre avançar |

---

## 5. Prompt pronto — recon do S1-03/S1-04 (Claude Code)

```
Você é o engenheiro do projeto LGP (Godot 4.7.2/C#, apresentação 2D). Esta é a RODADA DE
RECONHECIMENTO: NÃO escreva código de produção. Faça auditoria e entregue um relatório
escrito; eu aprovo antes de qualquer implementação.

Convenções e princípios: ver CLAUDE.md no repositório (é a fonte de verdade).

Frente desta sessão: WS-0 passos 4 e 6 — identidade/enums do domínio e o objeto-prova
(pool de recursos global tier-aware). Os docs de design estão em docs/design/; os canônicos
para esta frente são o doc 10 (§3.1 bens travados, §3.4 ladder, §4-A topologia) e o doc 30
(§9 resolução pura-C#, §9.2 determinismo).
O relatório anterior está em docs/recon/WS-0-recon.md — os passos 1, 2, 3 e 5 já foram
executados; não os refaça.

Entregue um relatório com:
1. Quais tipos o doc 10 exige que existam no núcleo, e por que cada um é enum, record
   ou POCO lido de arquivo de dados.
2. Onde a regra de fronteira com GDScript (doc 30 §9.1) precisa aparecer em código agora
   e onde ela pode esperar o WS-2.
3. Plano de implementação em passos pequenos e cirúrgicos, na ordem de dependência,
   com o critério de aceite de cada um.
4. Onde o pool tier-aware pode violar determinismo (ordem de iteração) e como o teste
   fecha esse buraco.

Pare no relatório. Aguardo aprovação explícita para a rodada de implementação.
```
