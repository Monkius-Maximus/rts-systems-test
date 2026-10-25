# S1-05 — Projeto Godot e o fio de ligação · RELATÓRIO DE RECONHECIMENTO

**Projeto:** Le Grand Polemarkos (LGP) · **Frente:** WS-0 passo 7 — **fecha o WS-0**
**Rodada:** reconhecimento. **Nenhum código de produção foi escrito no repositório.**
**Data:** 2026-10-25 · **Branch:** `recon/s1-05` (sobre `ws0/s1-03-s1-04`, PR #4)

Escopo do backlog: `game/` com `project.godot` **gerado pelo editor 4.7.2** (nunca à mão), um
`.csproj` do jogo com `ProjectReference` para `Lgp.Core`, e **um** smoke — uma cena que lê o pool
e imprime. A CI continua rodando `dotnet test` sem o Godot instalado.

---

## 0. O resultado principal, antes do resto

**O passo inteiro é viável neste ambiente, e foi provado de ponta a ponta num rascunho fora do
repositório:**

```
$ godot --headless --path <rascunho>
SMOKE Steel Tier3 = 42
exit=0
```

A engine 4.7.2 oficial, em modo headless, carregou um script C# que referencia o `Lgp.Core` **real
deste repositório**, criou um `GoodsPool`, creditou aço T3 e imprimiu o saldo. Isso fecha o risco
que o P1-P4 registrou (§2.1): "falhas em runtime na primeira vez que um tipo da biblioteca
referenciada é usado". Com um núcleo sem `GodotSharp`, o tipo carregou e funcionou.

O que tornou isso possível — e não era óbvio:

1. **O binário 4.7.2 .NET é baixável daqui**, e confere com o `SHA512-SUMS.txt` oficial da release.
2. **O NuGet está bloqueado nesta sessão, mas o Godot traz os próprios pacotes.** O download inclui
   `GodotSharp/Tools/nupkgs/` com `Godot.NET.Sdk`, `GodotSharp`, `Godot.SourceGenerators` e
   `GodotSharpEditor` 4.7.2. Um `nuget.config` local apontando para essa pasta compila o jogo offline.
   (Esse `nuget.config` é do ambiente, com caminho de máquina — **não** entra no repositório.)

---

## 1. "Gerado pelo editor, nunca à mão" — como cumprir isso de verdade

### 1.1 O jeito documentado de criar um projeto pela CLI é um arquivo vazio

Da doc oficial (`command_line_tutorial.rst`, seção *Creating a project*), literal:

> "Creating a project from the command line can be done by navigating the shell to the desired
> place and making a `project.godot` file." — `touch project.godot` — "The project can now be
> opened with Godot."

Mas abrir não basta. Testado no 4.7.2:

| Tentativa | Resultado |
|---|---|
| `touch project.godot` + `--headless --import` | arquivo continua com **0 bytes** |
| `touch project.godot` + `--headless -e --quit-after 30` | continua com **0 bytes** |
| `touch project.godot` + script que chama `ProjectSettings.save()` | **a engine escreve o arquivo no formato canônico dela** |

O terceiro é o caminho: eu digo *quais* configurações; **a engine decide o formato**. Saída real:

```ini
config_version=5

[application]
config/name="Le Grand Polemarkos"
run/main_scene="res://Smoke/PoolSmoke.tscn"
config/features=PackedStringArray("4.7")

[dotnet]
project/assembly_name="Lgp.Game"
project/solution_directory=".."

[rendering]
renderer/rendering_method="gl_compatibility"
```

Nenhuma dessas linhas de formato (`config_version=5`, `PackedStringArray("4.7")`) foi escrita por
mim. É exatamente o que a regra quer proteger.

### 1.2 A cena também tem de ser serializada pela engine — e há prova de por quê

A cena de smoke, salva por `ResourceSaver.save()`:

```ini
[gd_scene format=3]

[ext_resource type="Script" path="res://Smoke/PoolSmoke.cs" id="1_o5kae"]

[node name="PoolSmoke" type="Node" unique_id=1739525893]
script = ExtResource("1_o5kae")
```

Repare em `unique_id=` no nó: é um campo que o formato `.tscn` ganhou em versões recentes. Um
`.tscn` escrito à mão a partir de um exemplo de 4.3 não o teria. É a regra se justificando sozinha.

**Pegadinha encontrada:** a engine só anexa um script C# à cena **se o assembly já estiver
compilado**. Sem build, ela salva a cena *sem o script*, e só avisa no log
(`Cannot instantiate C# script because the associated class could not be found`). A ordem
obrigatória é: build → importar → salvar cena.

### 1.3 O `.csproj` reproduz o gerador da própria engine

O editor só cria o `.csproj` quando alguém cria o primeiro script C# ou clica em "Create C#
solution" — não há flag de CLI para isso (`--build-solutions` não gera; testado). A fonte primária
do que o 4.7.2 escreveria é o gerador dele, na tag exata
(`GodotTools.ProjectEditor/ProjectGenerator.cs @ 4.7.2-stable`):

```csharp
root.Sdk = $"Godot.NET.Sdk/{GeneratedGodotNupkgsVersions.GodotNETSdk}";
mainGroup.AddProperty("TargetFramework", "net8.0");
var net9 = mainGroup.AddProperty("TargetFramework", "net9.0");
net9.Condition = " '$(GodotTargetPlatform)' == 'android' ";
mainGroup.AddProperty("EnableDynamicLoading", "true");
// RootNamespace só se o nome não for identificador válido — "Lgp.Game" é.
```

O `Lgp.Game.csproj` proposto é **essa saída, byte a byte**, mais um `ItemGroup` com o
`ProjectReference` para o núcleo. Não é formato inventado: é o do gerador.

---

## 2. Riscos encontrados lendo o código-fonte do editor 4.7.2

### 2.1 [CRÍTICO] Se o editor não achar a solution, ele **sobrescreve o `.csproj`**

`GodotSharpDirs.FindSolutionFileWithAssemblyName` (4.7.2) procura, na pasta de
`dotnet/project/solution_directory`, uma `.sln` que liste um projeto cujo caminho absoluto seja
**exatamente** o do `.csproj` do jogo (comparação ordinal). Se não achar, devolve o caminho de uma
solution inexistente. Aí `CreateProjectSolutionIfNeeded` dispara `CreateProjectSolution`, que
**regenera o `.csproj` do zero** — apagando o `ProjectReference` para o núcleo. Isso acontece no
primeiro build ou no primeiro Play no editor.

**Consequência:** `game/Lgp.Game.csproj` **tem de estar listado no `Lgp.sln`**, com
`solution_directory=".."`. Não é preferência de organização — é o que impede o editor de destruir o
fio de ligação. A falha não seria silenciosa (o jogo pararia de compilar e o `git diff` mostraria o
`.csproj` reescrito), mas seria confusa.

### 2.2 [VERIFICADO SEGURO] A migração de configurações não toca o `Lgp.sln`

Ao abrir o projeto, o editor roda `DotNetSolution.MigrateFromOldConfigNames` **sobre a `.sln`**,
renomeando `Release|Any CPU` → `ExportDebug|Any CPU`. Isso quebraria `dotnet test --configuration
Release` na CI. Mas a primeira linha do método é:

```csharp
if (!Regex.IsMatch(input, Regex.Escape("Tools|Any CPU"))) return;
```

Só roda em solutions legadas do Godot 3. O `Lgp.sln` tem `Debug` e `Release`, sem `Tools` —
verificado. **Regra para o futuro: nunca criar uma configuração `Tools` no `Lgp.sln`.**

### 2.3 [VERIFICADO SEGURO] O editor compila o `.csproj`, não a `.sln`

`BuildSystem.cs` (4.7.2) passa `buildInfo.Project` — o `.csproj` — para `dotnet build` e
`dotnet publish`. Então as configurações do `Lgp.sln` não afetam o build do editor nem o export. E o
jogo compila tanto em `-c Release` (o que a CI usa) quanto em `-c ExportRelease` — os dois testados.

### 2.4 [FASE E] O núcleo provavelmente sai sem otimização no export

O SDK do Godot declara `Debug;ExportDebug;ExportRelease`, e o próprio `Sdk.props` avisa:

> "The Microsoft.NET.Sdk only understands of the Debug and Release configurations. We need to set
> the following properties manually for ExportDebug and ExportRelease."

Ele faz isso **para o projeto do jogo**. O `Lgp.Core` é um projeto .NET comum: compilado como
`ProjectReference` dentro de um export `ExportRelease`, ele não recebe essas propriedades e tende a
sair sem `Optimize`. Não verifiquei o efeito no binário final — é inferência a partir do comentário
do SDK. **Não é problema do protótipo**; fica registrado para a Fase E (Steam), com a correção
provável no `ProjectReference` do jogo, não no núcleo.

### 2.5 Arquivos `.uid`

O Godot 4.4+ gera arquivos `.uid` ao lado de scripts e cenas, e eles **devem ser commitados**. No
rascunho, o script de scaffold não os gerou (não rodou o importador). Na implementação, rodar
`--import` antes de salvar a cena, para que a cena já referencie o UID e o primeiro Play no editor
não gere diff. Conferir se o `.gitignore` não os exclui — hoje ele ignora só `.godot/`, `bin/`, `obj/`.

---

## 3. Decisões que preciso de você

### 3.1 Renderer — recomendo **Compatibility**

A doc oficial (`renderers.rst`) manda escolher Compatibility, entre outros casos, quando:

> "You are developing a 2D game, or a 3D game which does not need advanced rendering features."
> e "The Compatibility renderer supports the widest range of hardware."

O LGP é 2D (doc 00 §5) e vai para a Steam, onde alcance de hardware é público. Forward+ só valeria
por recurso avançado (a doc cita compute shaders), que nada no design pede. Trocar depois é uma
configuração, mas shaders e iluminação 2D escritos contra um renderer podem se comportar diferente
no outro — melhor fixar antes de existir arte.

### 3.2 CI — recomendo **um job de smoke separado**

| Opção | O que significa |
|---|---|
| **A** (recomendada) | O job `test` continua como está: `dotnet test`, sem engine. Ele passa a **compilar** o jogo também (porque `Lgp.Game` entra no `Lgp.sln`), o que pega erro de compilação da apresentação. Um **segundo job**, `smoke`, baixa o Godot 4.7.2 pela URL fixa, **confere o SHA-512 com o `SHA512-SUMS.txt` oficial e falha se não bater**, compila, roda a cena headless e falha se a saída não tiver a linha esperada ou tiver qualquer `ERROR:`. |
| B | Só a compilação na CI; o smoke roda à mão. |

A diferença real: B só pega quebra que **compila mas falha em runtime** quando alguém lembrar de
rodar o jogo. É justamente a classe de falha do P1-P4 §2.1. O custo de A é ~107 MB baixados por
execução e um job a mais — e o comentário do `ci.yml` precisa ser ajustado para dizer que o job
`test` continua sem engine e o `smoke` é o único lugar que a instala, de propósito.

### 3.3 O script de scaffold — recomendo **não commitar**

O `scaffold.gd` que pede à engine para serializar o `project.godot` e a cena roda **uma vez**. Depois
disso, quem mexe nesses arquivos é o editor. Commitá-lo deixaria no repositório um script que
ninguém roda de novo e que apodrece. O procedimento fica registrado no commit e neste relatório
(§4, passo 2), suficiente para refazer.

### 3.4 Ajuste opcional, aproveitando a mexida na CI

A CI anotou que o `ubuntu-latest` está migrando para Ubuntu 26. Fixar `ubuntu-24.04` nos dois jobs
segue a mesma regra que já vale para a engine: upgrade explícito, nunca implícito.

---

## 4. Plano de implementação

Um commit por passo. Ferramentas do ambiente (o binário do Godot e o `nuget.config` offline) ficam
fora do repositório.

1. **`game/Lgp.Game.csproj` + entrada no `Lgp.sln`.**
   - `.csproj` = saída do `ProjectGenerator` 4.7.2 + `ProjectReference` para
     `../src/Lgp.Core/Lgp.Core.csproj`.
   - `dotnet sln add game/Lgp.Game.csproj`; inspecionar o mapeamento de configurações que o comando
     gravar e corrigi-lo se necessário (`Release` da solution → uma configuração que o SDK do Godot
     declara).
   - *Aceite:* jogo compila em `Debug` e `Release`; `ArchitectureTests` continua verde (o núcleo
     segue sem referência ao Godot — a seta vai do jogo para o núcleo, nunca o contrário).

2. **`game/project.godot`, serializado pela engine.**
   - `touch game/project.godot`, depois o script de scaffold com: nome, `assembly_name=Lgp.Game`,
     `solution_directory=".."`, renderer (decisão 3.1), cena principal.
   - *Aceite:* arquivo gerado contém `config_version=5` e `features ("4.7")` escritos pela engine.

3. **Smoke: `game/Smoke/PoolSmoke.cs` e a cena.**
   - O script cria um `GoodsPool`, credita aço T3, imprime as entradas não zeradas e sai.
   - Ordem obrigatória (§1.2): build → `--import` → salvar a cena pela engine.
   - Commitar os `.uid` gerados.
   - *Aceite:* `godot --headless --path game` imprime `SMOKE Steel Tier3 = 42`, sem `ERROR:`,
     `exit=0`. Abrir o projeto de novo não gera diff em `project.godot`, `.csproj` nem `.tscn`.

4. **CI** (se a decisão 3.2 for A).
   - Job `smoke` com URL fixa, verificação de SHA-512, build e asserção da saída.
   - Ajustar o comentário do topo do `ci.yml`.
   - *Aceite:* os dois jobs verdes no PR.

5. **Docs.** Backlog: S1-05 feito; `mapa-de-execucao.md` §3: **WS-0 fechado**; `CLAUDE.md`: `game/`
   em "Onde está o quê", mais a regra da §2.1 (o jogo precisa estar no `Lgp.sln`) e a da §2.2
   (nunca uma configuração `Tools`).

**Não entra:** estrutura de nodes do doc 30 §4 (`MatchRoot`, `Simulation`, painéis) — é WS-6/WS-8.
A cena de smoke é provisória e é substituída quando a cena de partida existir.

---

## 5. Verificações feitas nesta rodada

Tudo no binário oficial `4.7.2.stable.mono.official.ed1daf0bf` (SHA-512 conferido) ou na fonte da
engine na tag `4.7.2-stable`, num rascunho fora do repositório.

| O quê | Resultado | Fonte |
|---|---|---|
| Versão do binário | `4.7.2.stable.mono.official.ed1daf0bf` | `--version` |
| Integridade | SHA-512 do zip confere com o oficial | `SHA512-SUMS.txt` da release |
| Flags `--headless`, `--import`, `-e`, `-s`, `--build-solutions` | conforme §1.1 | `--help` do binário |
| Criar projeto pela CLI | `touch project.godot` | `command_line_tutorial.rst` |
| `--import` / `-e --quit` preenchem o `project.godot`? | **não** (0 bytes) | teste |
| `ProjectSettings.save()` preenche? | **sim**, formato canônico | teste |
| Cena com script C# sem build | salva **sem** o script | teste + log |
| Cena com script C# após build | salva com `ext_resource` e `unique_id` | teste |
| Build offline do jogo | sucesso com os `.nupkg` embarcados | teste |
| `-c Release` e `-c ExportRelease` | ambos compilam | teste |
| Smoke headless com `Lgp.Core` real | `SMOKE Steel Tier3 = 42`, `exit=0` | teste |
| Gerador do `.csproj` | conteúdo da §1.3 | `ProjectGenerator.cs @ 4.7.2-stable` |
| Localização da solution e sobrescrita do `.csproj` | §2.1 | `GodotSharpDirs.cs`, `GodotSharpEditor.cs @ 4.7.2-stable` |
| Migração de configurações | só com `Tools\|Any CPU` | `DotNetSolution.cs @ 4.7.2-stable` |
| Editor compila `.csproj`, não `.sln` | `buildInfo.Project` | `BuildSystem.cs @ 4.7.2-stable` |
| Configurações do SDK do Godot | `Debug;ExportDebug;ExportRelease` | `Sdk.props @ 4.7.2-stable` |
| Renderer para 2D | Compatibility recomendado | `renderers.rst` |

---

## 6. Decisões pedidas

1. **Renderer** (§3.1): Compatibility ou Forward+? Recomendo Compatibility.
2. **CI** (§3.2): A (job de smoke) ou B (só compilação)? Recomendo A.
3. **Script de scaffold** (§3.3): não commitar? Recomendo que não.
4. **Fixar `ubuntu-24.04`** (§3.4)? Opcional; recomendo que sim, no mesmo commit da CI.

**Parei no relatório. Aguardo aprovação explícita para a rodada de implementação.**
