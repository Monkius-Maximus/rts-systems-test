# Nações e Geração Procedural — Proposta

**Projeto:** Le Grand Polemarkos (LGP)
**Status:** proposta (candidato a **doc 40** do vault — número a confirmar por você)
**Pré-requisito:** decisões travadas na sessão de arquitetura (vitória territorial; 10 bens;
esqueleto de ladder de aço/pólvora)

Decisão de identidade das três facções, expressa como **schema de dados** para que as três
canônicas e um **gerador procedural de nações** compartilhem um único espaço de campos. O
gerador serve dois propósitos no mesmo molde: ferramenta de **debug/teste** (nações extremas
sob demanda) e ferramenta **ingame** (nações assimétricas balanceadas para o usuário final).

---

## 1. Princípio: facção = dado, não caso especial

- Uma facção é um `FactionDefinition` (`Resource`/`.tres`), data-driven como a definição de
  hierarquia social (doc 30 §3). As três canônicas são **instâncias autoradas**; o gerador
  emite **novas instâncias no mesmo schema**. Uma fonte de verdade.
- Todos os campos que referenciam bens/PMs usam os **IDs canônicos** (`StringName`) da camada
  de simulação — não duplicam definição (doc 00 §7).
- **Guarda do pilar:** a assimetria flui pela **produção** (posição nos ladders, extração de
  brutos, composição de workforce), **não** por buffs planos de combate. O schema reflete isso
  — o modificador de combate plano é deliberadamente estreito e desencorajado. Poder de blob é
  *função* da cadeia produtiva (doc 20 §5), não um número de facção.
- **Relação com a tensão pura-C# vs `Resource` (doc 30, pendência aberta):** a simulação
  segura um modelo de facção em C# puro; o `.tres` é o formato de autoração/serialização que o
  hidrata; o gerador pode emitir tanto o objeto puro (runtime/ingame) quanto um `.tres`
  (editor/debug). Isto fica coerente com qualquer das duas resoluções daquela pendência.

---

## 2. Schema de campos do `FactionDefinition`

`Gerável?` = o gerador pode selecionar/sortear o campo. `Custo` = direção no orçamento de poder
(§3): vantagem **gasta**, desvantagem **devolve**. `Núcleo` = mínimo para as 3 facções
funcionarem; `Estendido` = riqueza adicional, adiável.

### 2.1 Identidade (cosmético — gerador escolhe de pools; custo zero)

| Campo | Tipo / Faixa | Gerável? | Camada |
|---|---|---|---|
| `id` | `StringName` único | não (gerado único) | Núcleo |
| `nome` | string | sim (pool de nomes) | Núcleo |
| `adjetivo` | string | sim | Estendido |
| `cor` | Color (paleta) | sim (pool) | Núcleo |
| `lema` | string curto | sim (pool) | Estendido |

### 2.2 Economia — posição nos ladders (núcleo da assimetria)

| Campo | Tipo / Faixa | Gerável? | Custo | Camada |
|---|---|---|---|---|
| `tierInicial[aço]` | int ∈ {1,2,3} | sim | + por tier | **Núcleo** |
| `tierInicial[pólvora]` | int ∈ {1,2,3} | sim | + por tier | **Núcleo** |
| `modCustoSubida[aço]` | float ~0.7–1.3 | sim | + se barato | Estendido |
| `modCustoSubida[pólvora]` | float ~0.7–1.3 | sim | + se barato | Estendido |
| `tetoLadder[aço]` | int ∈ {1,2,3} | sim | − se < 3 | Estendido |
| `tetoLadder[pólvora]` | int ∈ {1,2,3} | sim | − se < 3 | Estendido |
| `modExtração[bruto]` (×5: ferro, carvão, madeira, salitre, cobre) | float ~0.5–1.5 | sim | + se > 1 | **Núcleo** |

> Só os **intermediários** (aço, pólvora) têm campo de ladder — os finais herdam o tier dos
> insumos (decisão travada). Facção não tem campo por bem final.

### 2.3 Workforce

| Campo | Tipo / Faixa | Gerável? | Custo | Camada |
|---|---|---|---|---|
| `distribuiçãoInicial` | {Trab., Espec., Sold.} soma 1 | sim (com restrição de soma) | + por proporção de Espec. | **Núcleo** |
| `modConversãoSoldado` | float ~0.7–1.3 | sim | + se rápido/barato | Estendido |
| `tetoEspecialista` | float (proporção máx.) | sim | − se baixo | Estendido |

### 2.4 Militar (vários campos **reservados** — dependem de sistemas ainda abertos; ver §5)

| Campo | Tipo / Faixa | Gerável? | Custo | Camada |
|---|---|---|---|---|
| `modCapExército` | float ~0.7–1.3 | sim | + se grande | Estendido |
| `modCombatePlano` | float ~0.95–1.05 (estreito de propósito) | sim (desencorajado) | + se > 1 | Estendido |
| `tierConceitoMilitarInicial` | **reservado** | — | — | (pend.) |
| `arquétiposDisponíveis` | **reservado** (lista de IDs de unidade) | — | — | (pend.) |

### 2.5 Político — Landsraad (parcialmente aberto; ver §5)

| Campo | Tipo / Faixa | Gerável? | Custo | Camada |
|---|---|---|---|---|
| `capitalInicial` | int | sim | + | Estendido |
| `modCapitalPolítico` | float ~0.7–1.3 | sim | + se alto | Estendido |

---

## 3. Metadados de geração (o que torna o gerador *balanceado*)

Camada do gerador, não por-facção:

- **Orçamento de poder.** Toda nação — canônica ou gerada — gasta o **mesmo total**. Vantagem
  custa pontos; desvantagem devolve. É o que faz o gerador produzir assimetria *balanceada* em
  vez de nações OP aleatórias, e o que garante que as três canônicas sejam comparáveis. (Valor
  do orçamento e custos por campo = balanceamento, adiado.)
- **Restrições.** Validações que toda nação respeita:
  - `distribuiçãoInicial` soma 1; `tetoEspecialista` respeitado.
  - **Nenhuma nação começa maximizada em tudo:** ao menos um ladder inicia em T1.
  - Gasto líquido ≤ orçamento.
  - `tierInicial ≤ tetoLadder` por ladder.
- **Modos** (atende o duplo propósito):
  - `debug` — ignora orçamento e restrições; permite nações extremas para teste.
  - `ingame` — impõe orçamento + restrições; assimetria balanceada para o usuário final.
- **Seed.** Geração reproduzível a partir de uma seed (coerente com "determinístico = mesma
  fórmula"; sem necessidade de bit-exact). Essencial para o debug reproduzir casos.

---

## 4. As três facções canônicas (a decisão criativa)

Valores expressos em termos **relativos** (T1/T2/T3, alto/normal/baixo) — os números exatos são
balanceamento. Cada uma gasta o mesmo orçamento de forma diferente, formando um triângulo de
contra-jogo.

### Albion — a potência industrial faminta
Metalurgia de ponta e indústria madura, mas curta em manpower e **faminta por matéria-prima**:
sua força (aço alto) é estrangulada por sua fraqueza (extração baixa), o que a **empurra a
expandir** — casando com a vitória territorial. Qualidade sobre quantidade.

### Galia — o estado centralizado da pólvora
O caminho do meio, com **supremacia em pólvora/artilharia** (herança napoleônica) e **peso no
Landsraad**. Economia equilibrada, jogo longo: artilharia de alto tier + influência política.

### Norden — a fronteira marcial e farta
Metalurgia primitiva (a facção *bloomery* do exemplo canônico), mas **rica em recursos** e
**marcial**: exércitos grandes, baratos e de baixo tier. Vence cedo, na pressão e no número,
antes que o vizinho industrialize; se sobreviver, a fartura de brutos a sustenta.

### Comparativo

| Campo | Albion | Galia | Norden |
|---|---|---|---|
| Tier inicial **aço** | **T3** (Bessemer) | T2 (pudlagem) | **T1** (bloomery) |
| Tier inicial **pólvora** | T2 | **T3** (industrial) | T1 |
| Desconto de subida | barato no aço | barato na pólvora | nenhum |
| Workforce | Especialista-pesado | equilibrado | Soldado-pesado |
| Conversão → soldado | lenta | normal | rápida/barata |
| Extração de brutos | baixa (faminta) | normal (+ salitre) | alta (ferro/carvão/madeira; − salitre) |
| Cap de exército | baixo | médio | alto |
| Capital político | médio | alto (Landsraad) | baixo |
| Identidade de combate | qualidade (alto tier) | artilharia / pólvora | quantidade (números baratos) |
| Plano de jogo | sobreviver, industrializar, expandir por brutos | longo prazo: artilharia + política | rush/pressão cedo no número |

Fraqueza técnica de Norden = start baixo **+** poucos especialistas (não um terceiro castigo de
subida cara), compensada por brutos + cap de exército + reforço barato. Albion e Galia cada um
"coroam" um ladder distinto; Norden não coroa nenhum cedo, mas é quem mais pode produzir tarde.

---

## 5. Campos reservados e dependências

O schema é **forward-compatible**: campos abaixo existem stub agora e são preenchidos quando a
decisão correspondente fechar.

| Campo reservado | Espera por |
|---|---|
| `tierConceitoMilitarInicial` | "Tier do conceito militar" / roster de unidades (doc 20, pendência nova) |
| `arquétiposDisponíveis` | idem (roster) |
| `modCapExército` (semântica final) | Definição da entidade **Comandante** (doc 20, pendência nova) |
| `capitalInicial`, `modCapitalPolítico` (semântica final) | Geração de capital político (doc 10 §7 #4) |
| `modExtração[bruto]` (como vira produção real) | Ligação **território ↔ produção** (transversal, aberta) |

---

## 6. Próximos passos

1. Você revisa/ajusta as identidades das três facções (§4) — são minha decisão criativa, mas
   reversíveis.
2. Implementação pode começar pela **camada Núcleo** do schema (§2) com as três instâncias; o
   gerador `ingame`/`debug` (§3) vem em seguida.
3. Campos reservados (§5) preenchem-se conforme suas decisões fecham.
4. Números (orçamento, custos por campo, multiplicadores) = passo de balanceamento posterior.
