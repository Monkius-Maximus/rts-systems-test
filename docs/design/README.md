# Vault de Design — LGP

**Projeto:** Le Grand Polemarkos (LGP) — 4X em tempo real lento e pausável. Godot **4.7.2** / C#
(`net8.0`), apresentação 2D. Dev solo.

**O critério que esta pasta cumpre:** uma sessão de recon deve conseguir partir **apenas** de
`docs/design/`, sem colar nada de fora.

---

## Ordem de leitura

O **doc 00 é a autoridade sobre escopo** — em caso de conflito, ele vence.

| # | Arquivo | O que resolve |
|---|---|---|
| 1 | `00-visao-geral-e-escopo.md` | Gênero, pilar, forks travadas, guarda-corpos, engine, os 7 princípios, nota de risco |
| 2 | `10-adaptacao-de-sistemas.md` | Porta/comprime/morre; os 10 bens (§3.1); ladder de PM (§3.4); topologia da economia (§4-A); escassez (§4-B) |
| 3 | `20-combate-e-unidades.md` | Blobs de exército, auto-combate, interlock, precisão de "determinístico" (§4) |
| 4 | `30-arquitetura-godot.md` | Duas camadas; resolução pura-C# vs `Resource` (§9); determinismo do núcleo (§9.2); save (§10.1) |
| 5 | `40-nacoes-e-geracao-procedural.md` | Schema de `FactionDefinition`, orçamento de poder, as 3 facções canônicas |

---

## Tipos de documento

Antes de tratar uma linha como decidida, veja de que tipo é o arquivo.

**Canônicos (numerados).** Docs 00–40. Design travado. Cada um termina numa tabela de **Decisões
em Aberto** — é ali que se lê o que ainda não está decidido. Mudar algo travado exige reabrir o
documento, não improvisar em sessão.

**Coordenação (não canônicos).** Dizem *como* o projeto é conduzido, não o que ele é:
- `mapa-de-execucao.md` — frentes WS-0 a WS-8, fases, prompts por frente.
- `principios-de-producao.md` — disciplina, armadilhas do gênero, candidatos a adiar, checklist
  de sinais de alerta.
- `../sprint/sprint-backlog-01.md` — backlog do sprint corrente.

**Changelogs de sessão.** `delta-*.md` — blocos gerados para serem *dobrados* nos canônicos. Já
aplicados; ficam como histórico de *por que* uma seção existe. **Não são fonte de verdade.** Se um
delta e um doc numerado discordarem, **o doc numerado vence.**

**Auditoria.** `baseline-fontes-de-verdade.md` — separa o que veio dos docs originais do que foi
proposto por IA, e marca o que está verificado contra a doc oficial e o que ainda é suposição.
Consulte antes de tratar uma afirmação técnica como fato. ⚠️ Ele próprio cita "4.3+" como a versão
dos docs; isso foi corrigido para 4.7.2 e o arquivo guarda esse registro histórico.

**Pesquisa.** `../pesquisa/` — verificações de API/lib contra a documentação oficial, no formato do
`P1-P4-verificacao-tecnica.md`. Regra do projeto: **não supor API do Godot de memória** — ela muda
entre versões (o `TileMap` virou `TileMapLayer` entre 4.3 e 4.7).

---

## Estado atual

**Travado e pronto para implementar:**
- **Vitória:** territorial — limiar de % dos territórios capturáveis; eliminação = caso degenerado
  de 100%. Contagem de territórios, não VP.
- **Economia:** 10 bens; ladder só em aço e pólvora (T1→T3); finais herdam o tier dos insumos;
  **pool global tier-aware** (débito sem saldo lança, sem clamp); extração binária operada por
  Trabalhadores; processamento em slots de província com gating Trabalhador→Especialista; captura
  transfere o tier do inimigo mas exige a sua workforce.
- **Facções:** Albion (aço T3, pobre em ferro/carvão), Galia (pólvora T3, política), Norden (T1,
  farto e marcial) — doc 40.
- **Arquitetura:** simulação em C# puro, sem `GodotSharp`; `Resource`/`StringName` **fora** do
  núcleo; save em JSON legível; determinismo via `DeterministicRandom` e ordem de iteração
  definida.

**Aberto e bloqueando código (WS-4):** tier do conceito militar / roster, entidade Comandante, e
modelo de movimento — doc 20 §9 #3, #7 e #8, cruzado com doc 30 §10 #2.

---

## Convenções

- **Código em inglês; documentação e comentários em PT-BR.**
- Princípios não-negociáveis (doc 00 §7): fail fast · sem fallbacks · one way · mudanças
  cirúrgicas · sem overengineering · one source of truth para enums · separação de concerns.
- Sessões de código seguem **duas rodadas**: recon (relatório escrito, sem código) → aprovação
  explícita → implementação. Relatórios em `../recon/`.
- O **`CLAUDE.md` na raiz** é a fonte de verdade operacional das sessões de código.
