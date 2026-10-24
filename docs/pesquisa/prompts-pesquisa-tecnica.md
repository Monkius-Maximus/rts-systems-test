# Pack de Pesquisa Técnica Direcionada

**Projeto:** Le Grand Polemarkos (LGP)
**Uso:** prompts auto-contidos para uma IA **com acesso à web** (ex.: Claude com web search,
ou outra ferramenta de pesquisa) fechar as pendências **técnicas** (API/biblioteca) do projeto
por verificação contra a documentação. Pendência de *design* é fechada no chat de arquitetura;
pendência *técnica* vira prompt aqui.

> **Princípio:** verificar, não assumir. Cada prompt exige fonte para cada afirmação.

---

## Regras comuns a todos os prompts

Cole estas regras junto com qualquer prompt abaixo:

1. **Godot 4.3+ apenas.** A API muda entre versões (ex.: `TileMap` foi depreciado para
   `TileMapLayer` no 4.3).
2. **Toda afirmação técnica ancorada na página da doc oficial correspondente** — zero suposição
   de memória. Se não achar a fonte, dizer "não confirmado", não chutar.
3. **Linguagem-alvo C# (.NET)**, não GDScript. Onde a doc só existir em GDScript, mapear para C#.
4. **Se algo depender de plugin (não nativo)**, nomear o plugin, o repositório, a última
   atualização e confirmar compatibilidade declarada com 4.3+.
5. **Terminar com um veredito acionável** (sim/não/parcial + recomendação).

---

## Prompt 1 — Libs C++ do LGE têm análogo nativo em Godot?

```
Objetivo: confirmar se delaunator-cpp e FastNoiseLite (libs C++ MIT usadas no LGE)
têm análogo nativo em Godot 4.3+/C#, fechando doc 30 §8 #6 sem port nem interop C++.

Verificar (doc oficial Godot 4.3+):
- A classe `FastNoiseLite` é nativa? Qual a API em C# para gerar ruído 2D (heightmap)?
- `Geometry2D` expõe triangulação de Delaunay? Assinatura e tipo de retorno em C#.
- Há suporte nativo a diagrama de Voronoi? O pipeline Azgaar FMG é baseado em células
  Voronoi — se não houver nativo, qual o caminho a partir do Delaunay?

Entregar: tabela "lib C++ → análogo Godot (nativo / plugin / precisa port)" com link da
doc de cada API citada; veredito final: port necessário? sim/não/parcial, e se parcial,
exatamente qual parte.
```

## Prompt 2 — Caminho de import Azgaar FMG → Godot

```
Objetivo: definir o caminho de import do Azgaar Fantasy Map Generator (FMG) para um mapa
jogável em Godot 4.3+/C#, reimplementando o *fluxo* do commandlet do LGE (não portando
código). Contexto: doc 30 §8 #3 e doc 10 §2.3.

Verificar / levantar:
- Quais formatos o FMG exporta (.map nativo, GeoJSON, .svg) e qual carrega melhor dados de
  células / províncias / altura programaticamente.
- Como parsear esse formato em C# (bibliotecas .NET de JSON/GeoJSON; evitar dependências
  pesadas).
- Como mapear células/províncias do FMG para `TileMapLayer` (terreno + províncias
  capturáveis), sabendo que `TileMap` está depreciado em 4.3+.
- Custo de a ferramenta rodar como plugin de editor Godot vs script externo.

Entregar: fluxo passo-a-passo (export FMG → parse C# → estruturas de dados → TileMapLayer);
formato de export recomendado com justificativa; recomendação editor-plugin vs script externo.
```

## Prompt 3 — Better Terrain vs terrain nativo

```
Objetivo: avaliar se o protótipo precisa do plugin Better Terrain ou se o terrain nativo do
Godot 4.3+ basta para pintura de terreno (possivelmente em runtime, via geração procedural).
Contexto: doc 30 §5.1.

Verificar:
- Estado atual do plugin Better Terrain (repositório, última atualização, compatibilidade
  declarada com Godot 4.3+/4.x).
- O que o terrain matching NATIVO do Godot 4.3+ cobre (autotiling, terrain sets) e onde falha
  para terreno gerado proceduralmente em runtime.
- Se o caso de uso (pintar terreno proceduralmente em runtime, não no editor) é suportado
  nativamente ou exige o plugin.

Entregar: veredito "nativo basta / precisa Better Terrain / precisa outra solução", com a
limitação nativa concreta que justifica (se for o caso).
```

## Prompt 4 — Estratégia de teste headless da simulação (resolve a tensão pura-C# vs Resource)

```
Objetivo: definir a estratégia de teste headless da camada de simulação C# pura (análoga ao
StrategosCore) E resolver a tensão arquitetural: o design quer a simulação sem `using Godot;`
e headless-testável (doc 30 §7 #1), mas define os dados como `Resource` e os IDs como
`StringName` (doc 30 §3), que são tipos Godot.

Verificar / levantar:
- `StringName` e `Resource` exigem o runtime Godot carregado, ou podem ser instanciados/usados
  numa assembly .NET pura rodando fora do Godot (ex.: sob xUnit/NUnit)? Confirmar com a doc do
  binding C# do Godot.
- Se exigem o runtime: comparar (a) a simulação usar tipos C# puros (enums/records/struct de ID
  próprio) com a camada `.tres` virando mecanismo de loading na apresentação que hidrata objetos
  puros da simulação; vs (b) a simulação referenciar tipos Godot e "headless" passar a significar
  rodar via Godot `--headless` (engine sem render/nodes), não .NET puro.
- Qual setup de solução .NET separa a assembly de simulação (testável pura) da assembly de
  apresentação (Godot) de forma limpa.

Entregar: recomendação entre (a) e (b) com trade-offs; arquitetura de testes correspondente
(framework + estrutura de assemblies); e o impacto no critério de validação doc 30 §7 #1.
```

## Prompt 5 — Formato de save

```
Objetivo: escolher o formato de save do protótipo (doc 30 §8 #1), sabendo que o estado salvo
vem da camada de SIMULAÇÃO (não da árvore de cena), que se quer algo versionável e diff-ável,
e que há disciplina de SaveVersion bump por sessão.

Verificar / comparar:
- Opções: binário versionado custom, JSON, recurso Godot (.tres/.res).
- Trade-offs em 4 eixos: diff-abilidade (texto vs binário); versionamento/migração de schema;
  acoplamento ao Godot (queremos a simulação desacoplada — ver tensão do prompt 4);
  tamanho/performance.
- Como serializar a camada de simulação C# pura sem arrastar tipos Godot para o save.

Entregar: recomendação de formato justificada nos 4 eixos; esboço de como o SaveVersion bump
se integra (campo de versão + ponto de migração).
```

## Prompt 6 — Modelo de movimento (design informado por referência + engine)

```
Objetivo: alimentar a decisão de modelo de movimento (doc 20 §9 #3 / doc 30 §8 #2): zona/tile
(tipo Northgard) vs região contínua (tipo Spice Wars). NÃO é pergunta de API pura — é design
informado por como jogos comparáveis resolvem e pelo que o Godot 4.3+ afere.

Levantar:
- Como Northgard (zona/tile) e Dune: Spice Wars (região) tratam, respectivamente: movimento de
  exército, captura de território, e adjacência/fronteira. Onde cada modelo brilha e onde
  frustra num 4X lento pausável.
- Implicação de cada um para: o blob de exército (doc 20 §2), a unidade de captura (~30–50
  territórios), e a renderização em `TileMapLayer`.
- O que o Godot 4.3+ oferece nativamente para cada modelo (navegação/pathfinding de blob em grid
  vs contínuo; `NavigationServer2D`, A* nativo, etc.).

Entregar: tabela comparativa (zona/tile vs região contínua) nos eixos
movimento/captura/render/custo-de-implementação; recomendação fundamentada para dev solo,
sinalizando o que ela trava ou destrava no protótipo de mapa+combate.
```

---

## Ordem sugerida de execução

1. **Prompt 1** (libs) e **Prompt 4** (headless + tensão arquitetural) primeiro — destravam a
   forma da camada de simulação, que é o coração da arquitetura.
2. **Prompt 6** (movimento) — destrava mapa + combate; é o gargalo de mais coisas.
3. **Prompts 2, 3, 5** — caminho de import, terrain e save, conforme o protótipo encostar neles.
