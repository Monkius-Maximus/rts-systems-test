# Princípios de Produção — LGP

**Projeto:** Le Grand Polemarkos (LGP)
**Natureza:** documento de disciplina de produção. Não define mecânicas — define como o projeto
é conduzido e onde ele tende a descarrilar. Complementa a **nota de risco** do doc 00 §8.
**Meta:** protótipo jogável → Steam, por um dev solo.

---

## 1. O risco não é técnico, é dispersão

Reafirmando o doc 00 §8: nada aqui é tecnicamente difícil demais. O modo de falha realista é
**superfície de sistemas maior do que uma pessoa termina**. Todo "sim" de design acrescenta
superfície — inclusive os "sim" bons.

Contabilidade honesta da superfície atual: combate + diplomacia (foco central, recém-elevada) +
economia com dois ladders + assimetria de produção + gerador procedural de nações + IA para dois
oponentes. É muito para um primeiro protótipo solo.

**Regra de ouro:** antes de aceitar um sistema novo, perguntar *o que ele desbloqueia no loop
jogável*. Se a resposta é "profundidade", ele é Fase D — não agora.

---

## 2. Teste de "feature obrigatória"

Nem toda feature desejada é igual. O critério que distingue:

> **A feature muda a *forma* do sistema, ou é só um botão?**

- **Muda a forma → decidir agora.** É gratuito quando decidido cedo e caro quando retrofitado.
  Exemplo: *save exportável/inspecionável*. Ele força a disciplina de não haver estado
  autoritativo escondido em nodes — disciplina que o projeto já quer (doc 30 §2, §6).
- **É só um botão → adiar sem culpa.** Não muda arquitetura; entra quando houver jogo.

O save exportável passa no teste e paga em três frentes de uma vez: **debug** (reproduzir um bug
colando um save), **teste headless** (fixtures triviais para WS-0) e **modding/comunidade**
depois. Decisão correspondente: ver `delta-decisoes-sessao.md`, doc 30 §8 #1.

---

## 3. Até onde seguir a abordagem Paradox/Unciv

A parte a **imitar**: o estado da partida vive inteiro numa estrutura serializável, e o engine é
um conjunto de regras que opera sobre ela. Isso o projeto já decidiu (doc 30 §6: salvar a
simulação, não a árvore de cena).

A parte a **resistir**: Paradox é data-driven até o último parafuso — regras em arquivos de
script, engine como interpretador. Isso serve uma empresa com designers não-programadores e
décadas de modding. Para um dev solo, cada camada de indireção data-driven é **trabalho de
tooling sem um segundo usuário para pagá-lo**.

**Linha divisória adotada** (já implícita nos docs):

| Vai para dados (`Resource`/`.tres`, JSON) | Fica em C# compilado |
|---|---|
| Conteúdo: bens, métodos de produção, definições de facção | Regras: interlock, resolução de combate, tick de produção |
| Parâmetros de balanceamento | Fluxo de partida, condição de vitória |

Não transformar o interlock (nem qualquer regra) num arquivo de script.

---

## 4. Preocupação com desempenho: onde ela é e não é real

- **Estrutura serializável em memória não é mais lenta** por ser serializável. O custo de um save
  em texto é *escrever o arquivo*, e escrita acontece em momentos discretos (autosave, save
  manual), não a cada tick.
- **Escala do protótipo:** 3 facções, ~30–50 províncias, 10 bens. Ordens de magnitude abaixo de
  um Victoria 3, que salva em texto. JSON comprime muito bem.
- **Mitigações, se algum dia incomodar:** autosave em intervalo configurável; escrita em thread
  separada. Não implementar preventivamente (sem overengineering — doc 00 §7).
- **Onde o desempenho *pode* doer de verdade:** contagem de sprites/draw calls com muitas
  unidades visíveis (já previsto em doc 30 §5.3: `MultiMeshInstance2D` **pós-protótipo, após
  medir**) e o tick de simulação se a escala crescer. Medir antes de otimizar, sempre.

---

## 5. Onde a simplicidade já está correta (resistir à pressão)

Estas decisões vão *parecer* "faltando" durante o desenvolvimento. Não são — são o que mantém o
projeto terminável:

- Pool global sem logística de transporte.
- Blobs de exército sem micro de unidade.
- Sem cultura, religião ou pops multidimensionais.
- Um caminho único de combate (sem auto-resolve separado).
- Extração binária (sem ladder na extração).

Reabrir qualquer uma exige reabrir o doc 00, não improvisar em sessão.

---

## 6. Armadilhas específicas do gênero

1. **O protótipo não prova o pilar até o loop fechar.** Produzir → formar exército → capturar
   território → vencer. Antes de *jogar* isso, não se sabe se a assimetria de produção é divertida
   ou se é planilha. **A coisa mais valiosa a fazer é encurtar o caminho até a primeira partida
   jogável feia.** (É a razão de a Fase C existir antes da D.)
2. **IA é sistematicamente subestimada.** Em single-player contra 2 IAs, **a IA é o jogo**. Uma IA
   que não sobe o ladder nem contesta províncias faz a assimetria inteira desaparecer da
   experiência. Tratar WS-7 como sistema de primeira classe, não polish.
3. **Ritmo é design, não balanceamento.** Em 2–4h com ladder de 3 degraus: subida rápida demais →
   todos maximizam e a assimetria evapora; lenta demais → ninguém passa do T1. Só se descobre
   jogando; prever iterações.
4. **Steam é uma fase de trabalho real**, não o dia seguinte ao protótipo: build, ícones, capsule
   art, página de loja, playtest, precificação. Está na Fase E do mapa de execução.

---

## 7. Candidatos a adiar (se o escopo apertar)

Em ordem do que eu cortaria primeiro:

1. **Gerador procedural de nações.** Multiplica combinações *antes* de existir uma partida
   balanceada para comparar. O schema (`nacoes-e-geracao-procedural.md`) vale agora porque é
   estrutura; o **gerador** pode esperar. Manter as 3 facções autoradas à mão até a fatia
   vertical existir.
2. **Naval** (doc 20 §9 #1).
3. **Diplomacia bilateral**, se o chat de WS-5 concluir que o Landsraad basta para o protótipo.
4. **Multiplayer** — já adiado; manter adiado (evita exigir determinismo bit-exact).

---

## 8. Sinais de alerta (checklist de sessão)

Se qualquer um destes acontecer, voltar ao doc 00 §4:

- [ ] Um número de guarda-corpo cresceu (mais de 3 facções, mais de ~12 bens, mais de 1 condição
      de vitória).
- [ ] Um sistema novo foi aceito sem desbloquear nada do loop jogável.
- [ ] Está sendo otimizado algo que não foi medido.
- [ ] Uma regra de jogo migrou para arquivo de script/dados (ver §3).
- [ ] Passaram-se semanas sem o loop jogável ficar mais perto de fechar.
