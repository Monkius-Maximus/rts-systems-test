# LGP — Le Grand Polemarkos

4X em tempo real lento e pausável (Godot 4.7.2 / C#).

```bash
dotnet test                   # suíte do núcleo
godot --headless --path game  # smoke da fronteira núcleo↔engine
```

A suíte roda sem o Godot instalado, de propósito: o núcleo de simulação não referencia a
engine. O smoke é o contrapeso — prova, dentro da engine de verdade, que a simulação se
comporta lá exatamente como se comporta sob `dotnet test`. Ver [CLAUDE.md](CLAUDE.md) para convenções, princípios e a invariante de
arquitetura, e `docs/` para os relatórios de reconhecimento e as verificações técnicas.
