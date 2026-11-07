# Lista de assets — LGP

**Natureza:** documento de coordenação de arte. Diz **o que** desenhar, **em que tamanho**, **com
que nome** e **onde guardar**. Não define direção de arte: ela continua sendo 2D estilizado,
referência FMA (linework e paleta), doc 00 §5 e doc 30 §5.2.

**Regra de corte:** só entra aqui asset cujo conteúdo já está **travado** no design. O que
depende de decisão aberta fica na §6, com o motivo, e não deve ser desenhado ainda.

---

## 1. Base de resolução (vale para todos os tamanhos abaixo)

- O jogo é projetado em **1920×1080** (16:9), com stretch mode `canvas_items` e aspect `expand`.
  Em telas maiores a imagem é escalada; em telas mais largas (21:9) aparece mais área nas bordas.
  A sessão do shell configura isso no `project.godot`, pela engine.
- **Entregue tudo em 2× o tamanho de exibição** (ou seja, pensado para 4K). Reduzir fica nítido;
  ampliar borra. Os tamanhos da §3 e da §4 já são o 2×: entregue exatamente aqueles números.
- **Formato:** PNG com transparência (sRGB) para tudo que não seja fundo de tela cheia. Fundo de
  tela cheia pode ser PNG ou JPG.
- **Quadrado é quadrado:** ícones e emblemas em tela quadrada, com o desenho centralizado e uma
  margem de ~8% vazia em volta (o recorte e o contorno de seleção precisam de respiro).

---

## 2. Onde guardar e como nomear

### 2.1 Três lugares, cada um com um papel

| O quê | Onde | Vai para o Git? |
|---|---|---|
| **Arquivos-fonte** (`.psd`, `.kra`, `.aseprite`, `.ai`, camadas) | Uma pasta **sua**, fora do repositório (ex.: `Documentos/LGP-arte/`) | **Não.** São grandes, binários e mudam muito; o repositório não tem Git LFS. |
| **Exportados que o jogo usa** (PNG/JPG finais) | `game/assets/<categoria>/` — dentro do projeto Godot | **Sim.** |
| **Arte da loja da Steam** (Fase E) | `marketing/steam/` — **fora** de `game/` | Sim. Fora de `game/` porque tudo que está dentro do projeto o Godot importa e empacota no build; arte de loja não pode ir junto com o jogo. |

Pode jogar os exportados **direto** em `game/assets/...` quando estiverem prontos. Ao abrir o
editor, o Godot importa cada arquivo e cria um `<arquivo>.import` ao lado: **esse `.import` vai
para o commit junto com a imagem.** A pasta `.godot/` não (já está no `.gitignore`).

Trocar uma arte por uma versão nova = **sobrescrever o arquivo com o mesmo nome**. Nada precisa ser
re-referenciado no código.

### 2.2 Nomenclatura

- **`snake_case`, minúsculas, em inglês, sem acento nem espaço.** É a recomendação oficial do Godot:
  o sistema de arquivos empacotado (PCK) diferencia maiúsculas, o do Windows não, e misturar dá
  arquivo "sumido" só depois de exportar.
- **Padrão:** `<categoria>_<nome>.png`.
- **O `<nome>` é o nome do enum do núcleo em `snake_case`.** `GoodId.IronOre` → `good_iron_ore.png`.
  Assim existe **uma** regra de mapeamento entre código e arquivo, e nenhuma tabela a manter.
- Sem sufixo de versão (`_v2`, `_final`): o histórico fica no Git e nos seus arquivos-fonte.

---

## 3. Pode desenhar agora

### 3.1 Identidade e shell (o menu usa estes primeiro)

O menu funciona sem nenhum deles, com o tema padrão do Godot. Eles entram quando estiverem prontos.

| Arquivo | Tamanho (px) | Observação |
|---|---|---|
| `game/assets/branding/branding_icon.png` | 1024×1024 | Ícone do jogo (janela, barra de tarefas, `.exe`). Precisa ser legível a 16×16: silhueta simples, sem texto. As versões pequenas e o `.ico` saem dele. |
| `game/assets/branding/branding_logo.png` | 2560 de largura, altura livre (até 1440) | Logotipo "Le Grand Polemarkos", fundo transparente, só o letreiro e o símbolo. Reaproveitado como Library Logo da Steam. |
| `game/assets/ui/ui_menu_background.png` (ou `.jpg`) | 3840×2160 | Fundo do menu principal. Conteúdo importante no centro: as bordas podem ser cortadas ou estendidas conforme a tela. |

### 3.2 Facções (3) — travadas no doc 40

| Arquivo | Tamanho (px) |
|---|---|
| `game/assets/factions/faction_albion_emblem.png` | 512×512 |
| `game/assets/factions/faction_galia_emblem.png` | 512×512 |
| `game/assets/factions/faction_norden_emblem.png` | 512×512 |

**Junto com cada emblema, me passe a cor principal da facção em hex** (ex.: `#2E4A7D`). Ela é o
campo `cor` do schema de facção (doc 40 §2.1) e vai pintar fronteira, exército e UI. As três
precisam se distinguir também para quem tem daltonismo: varie o **claro/escuro**, não só o matiz.

Referência de identidade (doc 40): Albion = potência industrial faminta (qualidade, aço T3);
Galia = Estado centralizado da pólvora e da política (artilharia, Landsraad); Norden = fronteira
marcial farta (quantidade, metalurgia primitiva).

### 3.3 Bens (10) — travados no doc 10 §3.1

Todos **256×256**. Exibidos a 64 px ou menos: legibilidade em tamanho pequeno vale mais que
detalhe.

| Arquivo | Bem | Papel |
|---|---|---|
| `game/assets/goods/good_iron_ore.png` | Minério de ferro | bruto |
| `game/assets/goods/good_coal.png` | Carvão | bruto |
| `game/assets/goods/good_timber.png` | Madeira | bruto |
| `game/assets/goods/good_saltpeter.png` | Salitre | bruto |
| `game/assets/goods/good_copper.png` | Cobre | bruto |
| `game/assets/goods/good_steel.png` | Aço | intermediário (ladder) |
| `game/assets/goods/good_gunpowder.png` | Pólvora | intermediário (ladder) |
| `game/assets/goods/good_armaments.png` | Armamento | final militar |
| `game/assets/goods/good_firearms.png` | Armas de fogo + munição | final militar |
| `game/assets/goods/good_artillery.png` | Artilharia | final militar |

Sugestão de leitura rápida: os 5 brutos com um tratamento (forma natural, cor de terra), os 2
intermediários com outro (lingote / barril), os 3 finais com outro (silhueta de arma).

### 3.4 Tiers (3)

O tier aparece **por cima** do ícone do bem (aço T1 e aço T3 são buckets distintos no pool), então
é um selo separado, não 30 ícones.

| Arquivo | Tamanho (px) |
|---|---|
| `game/assets/tiers/tier_tier1.png` | 96×96 |
| `game/assets/tiers/tier_tier2.png` | 96×96 |
| `game/assets/tiers/tier_tier3.png` | 96×96 |

O nome repete "tier" porque segue a regra da §2.2 (`ProductionTier.Tier1` → `tier1`). Selo
pequeno, legível a 24 px, encaixado no canto inferior direito do ícone do bem.

### 3.5 Workforce (3) — travada no doc 10 §3.2

| Arquivo | Tamanho (px) |
|---|---|
| `game/assets/workforce/workforce_worker.png` | 256×256 |
| `game/assets/workforce/workforce_specialist.png` | 256×256 |
| `game/assets/workforce/workforce_soldier.png` | 256×256 |

**Total da §3: 22 arquivos.** Ordem sugerida: ícone do jogo → 3 emblemas + cores → 10 bens →
3 selos de tier → 3 de workforce → logo → fundo do menu.

---

## 4. Steam — Fase E, só quando a página da loja for montada

Tamanhos da documentação oficial da Steamworks (conferidos em 2026-11). **Verificar de novo antes
de produzir:** a Valve mudou todos os tamanhos em 2024 e pode mudar de novo.

| Arquivo (em `marketing/steam/`) | Tamanho (px) | Obrigatório |
|---|---|---|
| `steam_header_capsule.png` | 920×430 | sim |
| `steam_small_capsule.png` | 462×174 | sim — o logo precisa ser legível mesmo bem pequeno |
| `steam_main_capsule.png` | 1232×706 | sim |
| `steam_vertical_capsule.png` | 748×896 | sim |
| `steam_screenshot_01.png` … `_05` | 1920×1080 no mínimo, 16:9 | sim, pelo menos 5 (capturas do jogo, não arte) |
| `steam_page_background.png` | 1438×810 | opcional |
| `steam_library_capsule.png` | 600×900 | sim |
| `steam_library_header.png` | 920×430 | sim |
| `steam_library_hero.png` | 3840×1240 | sim — só arte, sem texto |
| `steam_library_logo.png` | até 1280×720, transparente | sim — sai do `branding_logo.png` |
| `steam_app_icon.jpg` | 184×184, JPG | — sai do `branding_icon.png` |

Fonte: [Store Graphical Assets](https://partner.steamgames.com/doc/store/assets/standard)
(Steamworks), mais a página de Library Assets linkada a partir dela.

---

## 5. Áudio

A sessão do shell cria os buses `Master`, `Music` e `SFX` sem nenhum som. Quando houver áudio:
`game/assets/audio/music/music_<nome>.ogg` e `game/assets/audio/sfx/sfx_<nome>.wav` (música longa
em Ogg Vorbis, efeito curto em WAV). Lista de sons só depois que existir partida jogável.

---

## 6. Ainda NÃO desenhar (depende de decisão aberta)

| Asset | Bloqueado por | Por que esperar |
|---|---|---|
| Sprites / tokens de exército e unidades | Roster e "tier do conceito militar" (doc 20 §9 #8) | Não se sabe quais unidades existem. |
| Tiles de terreno e de província | Modelo de movimento (doc 20 §9 #3) e import do mapa (WS-2) | O tamanho e o formato do tile dependem de zona/tile vs. região contínua. |
| Edifícios de pólvora | Degraus da pólvora a detalhar (doc 10 §3.4) | Os três métodos de produção ainda não têm nome. |
| Edifícios de aço (bloomery, pudlagem, Bessemer) | Só a forma de exibição no mapa | O conteúdo está travado, mas o tamanho depende dos tiles. Pode fazer esboço; não feche o tamanho. |
| Skin de UI (botões, painéis, 9-slice) | Nada de design — é polish | Fase E. O tema padrão do Godot serve até a fatia vertical jogável existir (`principios-de-producao.md` §1). |
| Retrato de Comandante | Entidade Comandante indefinida (doc 20 §9 #7) | Nem se sabe se é personagem. |

Quando uma dessas decisões fechar, ela ganha uma seção na §3 com tamanho e nome.
