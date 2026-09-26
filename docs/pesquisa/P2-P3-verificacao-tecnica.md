# Pesquisa P2 + P3 — verificação técnica do WS-2 (território e mapa)

**Data:** 2026-09-26 · **Frente:** WS-2 · **Status:** verificado por execução onde foi
possível, por código-fonte onde não foi.

> Ressalva de escopo: `prompts-pesquisa-tecnica.md` continua fora do repositório, então os
> prompts exatos de P2/P3 não foram lidos. As perguntas vieram dos itens 2, 3, 4 e 6 da
> §4 do relatório de recon do WS-0 — "a verificar antes de escrever a linha que os usa".
>
> **Não pesquisado de propósito:** `NavigationServer2D` vs `AStarGrid2D`. O modelo de
> movimento é decisão do chat de design do WS-4 (zona/tile vs região contínua). Pesquisar
> antes da decisão é pesquisar duas vezes.

---

## 1. P3 — Better Terrain no Godot 4.7.2

Era o maior risco registrado: dependência de terceiro no caminho crítico do WS-2, cujo
repositório **não afirma** compatibilidade com 4.7. Resolvido por execução, não por leitura.

**Método:** projeto Godot 4.7.2 mono descartável, com o plugin instalado e habilitado, e
uma cena C# chamando o autoload headless.

**Resultado: compatível.** O plugin carrega, o autoload `BetterTerrain` é registrado, não
há erro de parse e as chamadas respondem. O último commit upstream é de 2026-02-09,
corrigindo input para a 4.6 — está mantido e já atravessou a 4.6.

### 1.1 Interop C#→GDScript: funciona, e marshalling é completo

| Tipo atravessado | Sentido | Verificado |
|---|---|---|
| `TileSet`, `TileMapLayer` (Object) | C# → GDScript | sim |
| `String`, `Color`, `int` | C# → GDScript | sim |
| `Godot.Collections.Array` de `Vector2I` | C# → GDScript | sim |
| `int`, `bool` | retorno | sim |
| `Godot.Collections.Dictionary` (com `String`/`Color` dentro) | retorno | sim |

Chamadas feitas via `GetNode("/root/BetterTerrain").Call("<nome>", ...)`. Não há checagem
de tipo em tempo de compilação — é `Variant` dos dois lados.

### 1.2 O achado que muda o código: a API sinaliza falha por **retorno**, não por erro

Dois resultados observados na sonda:

- `set_cells(layer, coords, 0)` devolveu **`false`**, silenciosamente, porque o `TileSet`
  não tinha tile correspondente ao terreno. Nenhum erro, nenhum aviso no console.
- `get_cell(layer, coord)` devolveu **`-1`** para célula vazia — sentinela, não exceção.

Isso colide de frente com o princípio do projeto ("um clamp silencioso é um fallback
disfarçado"). Um `bool` ignorado aqui vira mapa parcialmente pintado sem ninguém notar, e
o bug aparece longe da causa.

**Regra que o WS-2 tem de implementar:** o ponto único de conversão do lado C# **confere o
retorno e lança**. `false` de `set_cell`/`set_cells` é erro; `-1` de `get_cell` é ausência
explícita e precisa ser tratada como valor nomeado, nunca propagada como inteiro solto.

---

## 2. Assinaturas reais do `TileMapLayer` na 4.7.2

Dumpadas do próprio binário (`godot --headless --doctool`), que é mais autoritativo que a
documentação publicada:

```
void      set_cell(Vector2i coords, int source_id=-1, Vector2i atlas_coords=Vector2i(-1, -1), int alternative_tile=0)
void      set_cells_terrain_connect(Vector2i[] cells, int terrain_set, int terrain, bool ignore_empty_terrains=true)
int       get_cell_source_id(Vector2i coords)
Vector2i  get_cell_atlas_coords(Vector2i coords)
void      erase_cell(Vector2i coords)
Vector2i[] get_used_cells()
Rect2i    get_used_rect()
Vector2i  local_to_map(Vector2 local_position)
Vector2   map_to_local(Vector2i map_position)
```

Confirma a regra de fronteira do CLAUDE.md: o que atravessa é `int` e `Vector2i`. Nenhum
tipo da simulação precisa chegar perto da engine.

---

## 3. P2 — Import do Azgaar FMG

### 3.1 O gerador foi reescrito

O FMG hoje é um projeto **TypeScript + Vite** com o código em `src/`. O layout `modules/`
que a wiki e os tutoriais de import citam **não existe mais**. Qualquer receita de import
escrita contra o FMG antigo deve ser tratada como suspeita.

### 3.2 Campos reais do export GeoJSON de células

Lidos de `src/services/io/export.ts` (`saveGeoJsonCells`), que é a fonte de verdade:

```js
properties = { id, height, biome, type, population, state, province, culture, religion, neighbors }
geometry   = { type: "Polygon", coordinates }   // anel fechado explicitamente
```

### 3.3 `neighbors` vem no export — e isso muda o plano do WS-2

`neighbors` é `cells.c[i]`, a adjacência direta do grafo de Voronoi. **O grafo de
províncias vem pronto; não é preciso derivá-lo da geometria dos polígonos.**

Consequência prática: o problema conhecido de buracos e sobreposições no export de células
deixa de ser risco para a adjacência e passa a ser um problema só de *renderização*. A
invariante de vizinhança, que é o que a simulação precisa, vem autoritativa da origem.

### 3.4 Armadilha: `height` não é a altura bruta

```js
const getHeight = (i) => parseInt(getFriendlyHeight(cells.p[i], pack, grid), 10);
```

`getFriendlyHeight` devolve uma string **já convertida para a unidade escolhida pelo
usuário** (metros ou pés). O `height` do GeoJSON é, portanto, um valor de *apresentação*
que muda conforme a configuração de quem exportou — **não** é o `cells.h` de 0–100.

Usar `height` para derivar terreno→bruto produziria um mapa que depende da preferência de
unidade de quem gerou o arquivo. **O sinal de terreno é `biome`.** O campo `type` é o tipo
de feature (oceano/lago/ilha), útil para separar água de terra, não para bioma.

### 3.5 Coordenadas são geográficas, não pixels

`toGeoCoordinates` chama `getCoordinates(...)` com 4 casas decimais, produzindo
longitude/latitude conforme `options.map.geography.coordinates`. Quem precisar de posição
no plano do mapa terá de inverter isso ou usar outro export.

### 3.6 Nível de evidência

As seções 3.2–3.5 vêm da **leitura do código do export**, não de um arquivo exportado de
verdade — o FMG é uma aplicação de navegador e não foi executado aqui. Os nomes de campo
são certos; o que falta confirmar contra um export real é o *conteúdo* (faixa de valores
de `biome`, o que `province` traz quando a célula não pertence a nenhuma).

**Pendência para o WS-2:** gerar um export de células de verdade e validar contra ele
antes de escrever o importador.

---

## 4. Fontes

- Godot 4.7.2 mono, binário oficial — sonda executada e `--doctool` (referência de classe dumpada do binário).
- Better Terrain: https://github.com/Portponky/better-terrain
- Azgaar FMG, `src/services/io/export.ts` e `src/utils/unitUtils.ts`: https://github.com/Azgaar/Fantasy-Map-Generator
