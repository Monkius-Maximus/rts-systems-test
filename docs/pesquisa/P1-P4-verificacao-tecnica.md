# Pesquisa P1 + P4 — verificação técnica (pré-requisito do WS-0)

**Data:** 2026-09-11 · **Frente:** WS-0 (Núcleo de simulação)
**Status:** verificado contra documentação/pacotes oficiais, não contra memória.

> Ressalva de escopo: `prompts-pesquisa-tecnica.md` não está no repositório, então os
> prompts exatos de P1 e P4 não foram lidos. As perguntas abaixo foram derivadas do que
> o mapa de execução declara que P1 ("libs") e P4 ("headless + tensão `Resource`")
> precisam desbloquear. Se os prompts originais pedirem mais, reabra a pesquisa.

---

## 0. Versão de engine e runtime (fato base, muda todas as respostas)

| Item | Valor verificado | Como foi verificado |
|---|---|---|
| Godot estável atual | **4.7.2** (18/08/2026) | busca + `Godot.NET.Sdk` 4.7.2 publicado no NuGet |
| `Godot.NET.Sdk` mais recente | 4.7.2 | índice flat do NuGet (`v3-flatcontainer`) |
| Runtime alvo do `GodotSharp` 4.7.2 | **`net8.0`** | `lib/net8.0/GodotSharp.dll` dentro do próprio `.nupkg`; nuspec com `<group targetFramework="net8.0" />` |
| Mínimo documentado | .NET 8 ou superior (Android exige .NET 9+) | fonte `.rst` da doc oficial |
| C# para Web | **não suportado** no Godot 4 | doc oficial |

**Consequência para o projeto:** o mapa diz "Godot 4.3+". Isso é um intervalo, não uma
decisão. Entre 4.3 e 4.7 houve mudança relevante de API (ver §3). **Recomendação: travar
uma versão exata (4.7.2) no repositório** e tratar upgrade como tarefa explícita.
`net8.0` é o TFM correto — não há ganho em `net9`/`net10` aqui e o SDK não pede.

---

## 1. P1 — Bibliotecas

### 1.1 Teste: `gdUnit4Net` vs xUnit/NUnit puro

- `gdUnit4Net` é compatível com o padrão VSTest, integra com Rider/VS/VS Code e roda
  CLI em modo headless. Versão 5.x suporta .NET 8/9, LangVersion 12.
- Ponto decisivo: **"testes rodam sem o runtime do Godot por padrão, com features do
  Godot opcionais (opt-in)"**, e a própria documentação reporta execução até ~10x mais
  rápida para testes só de lógica.

**Conclusão:** se o núcleo de simulação for uma biblioteca .NET pura (§2), ele não precisa
de `gdUnit4Net` — xUnit ou NUnit com `dotnet test` bastam e não trazem dependência de
engine. `gdUnit4Net` só se justifica quando/se houver teste da **camada de nodes**
(cenas, sinais), que é WS-8. Pelo princípio *one way*, a recomendação é: **um único
framework de teste no repositório**, xUnit, sobre o núcleo puro; a camada de nodes se
valida rodando o jogo, não com um segundo framework.

### 1.2 Mapa: Azgaar FMG (insumo do WS-2, verificado aqui porque afeta a fronteira)

- Formatos de saída: `.map` (formato próprio, recarregável no gerador) e **exportação
  GeoJSON** de células, rotas, rios, marcadores e zonas como arquivos separados.
- O export de células carrega os atributos que interessam: população, altura, **estados,
  províncias**, cultura.
- Alerta documentado pela própria comunidade: **o export de células às vezes tem buracos
  ou sobreposições**. Import ingênuo vai produzir vizinhança inconsistente.

**Conclusão:** GeoJSON de células é o insumo certo (traz província e altura). O importador
é um passo **offline/ferramenta**, não runtime, e precisa de validação que falhe alto
(fail fast) em buraco/sobreposição — não de um caminho de correção silenciosa.

### 1.3 Better Terrain (insumo do WS-2)

- É um plugin de terreno para o tilemap do Godot 4, com versão específica para o node
  **`TileMapLayer`** (a versão antiga era para o `TileMap` depreciado).
- A API pública é um autoload **`BetterTerrain`** com `set_cell(tm: TileMapLayer, coord,
  type)`, `set_cells`, `get_cell`, `update_terrain_cells`, `update_terrain_area`.
- O repositório **não documenta uso a partir de C#**, e a assinatura publicada é GDScript.

**Consequência:** usar Better Terrain a partir de C# significa interop dinâmico com um
autoload GDScript (`Call("set_cells", ...)`), sem checagem de tipo em tempo de
compilação. Isso é exatamente o tipo de fronteira onde enums se duplicam. **Risco
registrado para o WS-2** — não é problema do WS-0, mas define a regra da fronteira que o
WS-0 vai fixar: *nenhum tipo da simulação atravessa para GDScript; o que atravessa é
índice inteiro/coordenada.*

---

## 2. P4 — Headless e a tensão pura-C# vs `Resource`

### 2.1 O fato técnico que resolve a tensão

Da discussão oficial sobre múltiplos projetos .NET por projeto Godot:

- **Se os arquivos de código não referenciam o Godot, é possível fazer um pacote C#
  comum com `dotnet new classlib`.** Se a biblioteca precisar de API do Godot, ela passa
  a exigir `GodotSharp` na mesma versão do `Godot.NET.Sdk` do jogo.
- Referência entre projetos funciona via `<ProjectReference Include="..\Core\Core.csproj"/>`.
- **Os problemas conhecidos aparecem justamente quando a biblioteca usa o SDK do Godot:**
  erros de atributo duplicado de `System.Reflection` e falhas em runtime na primeira vez
  que um tipo da biblioteca referenciada é usado.
- O registro de classes do Godot acontece **só dentro do projeto Godot**, com as classes
  dele.

**Tradução para a decisão do WS-0:** a tensão não é uma questão de gosto. Uma biblioteca
sem referência ao Godot é um caminho suportado e limpo; uma biblioteca *com* `GodotSharp`
é o caminho com defeitos conhecidos. Herdar de `Resource` no núcleo obriga o segundo.

### 2.2 Headless

- Testar o núcleo puro **não usa o Godot de forma alguma**: é `dotnet test` sobre uma
  classlib. Não precisa de `--headless`, de editor, nem de binário da engine em CI.
- `--headless` do Godot continua sendo o caminho para rodar o *jogo* sem janela, e o
  runner CLI do gdUnit4 roda headless — mas isso só importa se houver teste de cena.

**Conclusão:** o harness de teste headless do WS-0 é `dotnet test`, e a única exigência
de arquitetura para que ele continue existindo é **o núcleo não referenciar `GodotSharp`**.

### 2.3 `StringName`

`StringName` é um tipo do `GodotSharp`. Usá-lo em identificadores da simulação arrasta a
dependência inteira para dentro do núcleo e derruba §2.1. Seu uso legítimo é onde ele
resolve um problema real de performance do Godot: nomes de sinal, ações de input e
caminhos de node — tudo na camada de apresentação.

---

## 3. Deriva de API entre 4.3 e 4.7 (por que travar versão)

- `TileMap` está depreciado em favor de **`TileMapLayer`**; o plugin de terreno já se
  dividiu em duas versões por causa disso. Código escrito contra a doc de 4.3 pode estar
  falando de um node que não é mais o recomendado.
- Regra operacional a adotar: **nenhuma API do Godot assumida de memória**; antes de usar,
  conferir na doc da versão travada. Esta pesquisa seguiu essa regra (os números acima
  vieram de pacote e fonte, não de lembrança).

---

## 4. Fontes

- Godot Engine — C# basics (fonte `.rst` oficial): https://github.com/godotengine/godot-docs/blob/master/tutorials/scripting/c_sharp/c_sharp_basics.rst
- Godot Engine — C# global classes: https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_global_classes.html
- NuGet — `Godot.NET.Sdk` / `GodotSharp` 4.7.2: https://www.nuget.org/packages/Godot.NET.Sdk
- gdUnit4Net (C#): https://github.com/godot-gdunit-labs/gdUnit4Net
- gdUnit4: https://github.com/godot-gdunit-labs/gdUnit4
- godot-proposals #1141 — múltiplos projetos SDK-style por projeto Godot: https://github.com/godotengine/godot-proposals/issues/1141
- godot#65317 — falha ao referenciar projeto que depende do `Godot.NET.Sdk`: https://github.com/godotengine/godot/issues/65317
- Azgaar FMG — GIS data export (wiki): https://github.com/Azgaar/Fantasy-Map-Generator/wiki/GIS-data-export
- Azgaar FMG — Cells Export Information: https://github.com/Azgaar/Fantasy-Map-Generator/discussions/580
- Better Terrain: https://github.com/Portponky/better-terrain
