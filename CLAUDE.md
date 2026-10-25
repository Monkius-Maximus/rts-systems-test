# LGP — Le Grand Polemarkos

4X em tempo real lento e pausável. Godot 4.7.2 (.NET) + C#, apresentação 2D. Dev solo.

## Convenções

- **Código em inglês** (identificadores, nomes de teste, mensagens de exceção de API).
- **Documentação e comentários em PT-BR.** Mensagens de falha de teste também — são
  documentação para quem quebrou a build.
- **Conversa em PT-BR.**

## Princípios não-negociáveis

Fail fast · sem fallbacks · um jeito só de fazer cada coisa · mudanças cirúrgicas · sem
overengineering · uma fonte de verdade para enums · separação de concerns.

Na prática, o que mais aparece aqui: **um clamp silencioso é um fallback disfarçado.**
Débito sem saldo, índice inválido, intervalo vazio — tudo lança.

## Invariante de arquitetura

**`src/Lgp.Core` não referencia o Godot.** Nem `GodotSharp`, nem `Godot.NET.Sdk`.

Consequências que dependem disso: a suíte roda em `dotnet test` sem engine, sem editor e
sem binário do Godot em CI; a simulação é testável isoladamente; não se cai nos defeitos
conhecidos de bibliotecas que dependem do SDK do Godot.

Corolários:
- **`Resource` e `StringName` não existem na simulação.** `Resource` é asset do Godot
  (textura, cena, tileset) e vive só na apresentação. Dado de jogo é arquivo de dados
  lido para POCOs do núcleo — o mesmo caminho que o gerador procedural do WS-3 usa.
- **Enums são definidos no núcleo.** A apresentação os consome; nunca os redeclara.
- **Nenhum tipo da simulação atravessa para GDScript.** Na fronteira com plugins GDScript
  (ex.: Better Terrain), o que passa é `int`/`Vector2I`, convertido num único ponto do
  lado C# que falha alto se o valor não mapear.

`ArchitectureTests` protege isso. Se ele ficar vermelho, a resposta não é relaxar o teste.

## Determinismo

Mesma seed, mesma partida — exigido pelo auto-combate (WS-4) e pela promessa de as IAs
rodarem a mesma engine sem cheating (WS-7). Quatro regras no núcleo:

1. Aleatoriedade só via `DeterministicRandom` (PCG32 semeado). Nunca `System.Random`
   (algoritmo não estável entre versões do runtime) nem o RNG do Godot (é da engine).
2. Toda iteração que alimenta cálculo precisa de ordem definida. `Dictionary` e `HashSet`
   não garantem ordem.
3. Sem paralelismo dentro do tick.
4. Sem relógio de parede no núcleo: o tick recebe o passo, não consulta a hora.

## Comandos

```bash
dotnet test     # suíte completa, sem Godot instalado
dotnet build

# smoke do fio de ligação (exige o Godot 4.7.2 .NET)
dotnet build game/Lgp.Game.csproj
godot --headless --path game --import
godot --headless --path game      # imprime "SMOKE Steel Tier3 = 42"; sai com 1 se divergir
```

## Onde está o quê

- `src/Lgp.Core/` — simulação, C# puro.
- `tests/Lgp.Core.Tests/` — suíte headless.
- `game/` — projeto Godot 4.7.2 (apresentação). Referencia o núcleo; nunca o contrário.
- `docs/design/` — **vault de design: a fonte de verdade.** Comece pelo `README.md` de lá, que
  diz a ordem de leitura e o peso de cada arquivo. Canônicos: doc 00 (escopo), 10 (bens, ladder,
  economia), 20 (combate), 30 (arquitetura), 40 (facções e gerador).
- `docs/recon/` — relatórios de reconhecimento por frente (auditoria antes de codar).
- `docs/pesquisa/` — verificações de API/lib contra a documentação oficial.
- `docs/sprint/` — backlog corrente.

Dado canônico (os 10 bens, os tiers, a topologia do pool) está no doc 10. **Não invente o
conteúdo dele** — se faltar algo, é pendência registrada na tabela de decisões em aberto do doc
correspondente, não lacuna a preencher por conta.

O que ainda **não** está decidido e bloqueia código: tier do conceito militar, roster de
unidades, entidade Comandante e modelo de movimento (doc 20 §9 #3, #7, #8).

## Regras do projeto Godot

Vieram da leitura do código-fonte do editor 4.7.2 (docs/recon/S1-05-recon.md §2):

- **`game/Lgp.Game.csproj` precisa estar no `Lgp.sln`.** Se o editor não achar uma solution que
  o liste, ele regenera o `.csproj` e apaga o `ProjectReference` para o núcleo.
- **Nunca criar uma configuração `Tools` no `Lgp.sln`.** A presença dela dispara uma migração
  automática do editor que renomeia `Release` para `ExportDebug` — e quebra a CI.
- **Formato do Godot não se escreve à mão.** `project.godot`, `.tscn` e `.uid` saem da engine ou
  do editor; o formato muda entre versões.

## Regra de API

Não assumir API do Godot de memória: ela muda entre versões (o `TileMap` virou
`TileMapLayer` entre 4.3 e 4.7). Conferir na doc da versão travada antes de usar, e
registrar a verificação em `docs/pesquisa/`.
