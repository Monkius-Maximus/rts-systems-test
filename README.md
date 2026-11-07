# LGP — Le Grand Polemarkos

4X em tempo real lento e pausável (Godot 4.7.2 / C#).

```bash
dotnet test
```

A suíte roda sem o Godot instalado, de propósito: o núcleo de simulação não referencia a
engine. Ver [CLAUDE.md](CLAUDE.md) para convenções, princípios e a invariante de
arquitetura, e `docs/` para os relatórios de reconhecimento e as verificações técnicas.

## Ambiente local

- **Godot 4.7.2, versão .NET** (não a padrão, que não roda C#). Precisa ser **exatamente** a
  4.7.2: o `game/Lgp.Game.csproj` usa o `Godot.NET.Sdk/4.7.2` e a CI testa contra a 4.7.2.
  Upgrade de engine é tarefa explícita, num commit próprio.
- **.NET SDK 8 ou mais novo.** O projeto compila para `net8.0`; um SDK mais novo (9, 10) compila
  esse alvo normalmente. A CI usa o 8.

Arte e áudio: ver [docs/arte/lista-de-assets.md](docs/arte/lista-de-assets.md).
