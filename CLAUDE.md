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
dotnet test                       # suíte do núcleo, sem Godot instalado
dotnet build game/Lgp.Game.csproj # camada de apresentação
godot --headless --path game      # smoke da fronteira: sai 0 se sã, 1 se divergiu
```

`Lgp.sln` contém apenas o núcleo e os testes. `game/` fica **deliberadamente fora** dele,
para que `dotnet test` nunca precise sequer restaurar o SDK do Godot. Não "conserte" isso
adicionando o projeto do jogo à solução. Pelo mesmo motivo o CI roda só `dotnet test`: se
um dia ele precisar baixar a engine, a invariante foi quebrada.

O Godot builda o projeto sozinho (`godot --headless --path game --build-solutions --quit`)
e copia `Lgp.Core.dll` junto — não é preciso gerar `.sln` para a pasta `game/`.

## Onde está o quê

- `src/Lgp.Core/` — simulação, C# puro.
- `tests/Lgp.Core.Tests/` — suíte headless.
- `game/` — projeto Godot (apresentação). Único lugar onde a engine existe.
- `docs/recon/` — relatórios de reconhecimento por frente (auditoria antes de codar).
- `docs/pesquisa/` — verificações de API/lib contra a documentação oficial.

**Docs de design (vault): ainda fora do repositório.** Doc 00/10/20/30,
`delta-decisoes-sessao.md`, `nacoes-e-geracao-procedural.md`. Enquanto não entrarem, as
frentes que dependem de dado canônico (bens, tiers, roster) ficam bloqueadas — não invente
o conteúdo delas.

## Regra de API

Não assumir API do Godot de memória: ela muda entre versões (o `TileMap` virou
`TileMapLayer` entre 4.3 e 4.7). Conferir na doc da versão travada antes de usar, e
registrar a verificação em `docs/pesquisa/`.
