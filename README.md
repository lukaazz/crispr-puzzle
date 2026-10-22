# crispr-puzzle

Jogo 2D educativo sobre CRISPR-Cas9: explore o DNA, resolva puzzles e repare
os trechos para concluir dez fases progressivas. Plataforma alvo: desktop Windows.

## Como executar

O projeto usa **Godot 4.6.1 edicao .NET** e **SDK .NET 8** (`net8.0`).
Instale o SDK, nao apenas o runtime. A edicao comum do Godot nao executa C#.

1. Confira `dotnet --list-sdks` no terminal.
2. Na raiz do repositorio, execute `dotnet build`.
3. Abra `project.godot` no Godot .NET e pressione **F5**.

Se o SDK estiver instalado fora do PATH, no PowerShell use, por exemplo:
`& 'C:/Program Files/dotnet/dotnet.exe' build`.
O alvo e desktop Windows; nao ha exportacao web.

## Implementacao e validacao

A migracao para C# esta concluida: autoloads, menu, seletor, componentes,
fase, personagem, fita de DNA, danos, marcador e os tres puzzles usam C#.
Os scripts GDScript antigos e seus arquivos UID foram removidos; as cenas
apontam para os scripts C# correspondentes. Nenhuma funcionalidade nova
foi adicionada durante a migracao. Heredograma implementado com oito modelos fixos.

`PareamentoBases`, `Pergunta` e `Heredograma` oferecem `Setup(int dificuldade)` e o sinal
`PuzzleConcluido(bool sucesso)`, declarado com `[Signal]`. A fase abre o
puzzle, pausa o personagem e recebe o resultado; so o sucesso repara o dano.
O pareamento sorteia bases e seus complementos, embaralha os lados e aceita
qualquer copia complementar. A pergunta continua lendo `data/perguntas.json`
com Godot.FileAccess e System.Text.Json.

O acesso aos autoloads usa `GetNode<GameState>("/root/GameState")`, com o
mesmo padrao para Assets e SceneRouter. Acoes centraliza os identificadores
do Input Map como `static readonly StringName` (StringName nao pode ser
const em C#). `Pausar` aciona o menu de pausa pelo Input Map (Esc por padrao).
O progresso continua em `user://save.cfg`, com formato preservado.

VisualSlot e os botoes mantem placeholders sem imagens, proporcao das
texturas, animacao opcional e ajustes de `data/visuais.json`.
`GetNode<Assets>("/root/Assets").Recarregar()` limpa os caches e atualiza os
consumidores. As trocas de tela passam exclusivamente pelo SceneRouter.

Validacao final: `dotnet build` aprovado com **zero erros e zero avisos**.
Testes headless no Godot .NET 4.6.1, com save isolado, verificaram:

- Limites de dificuldade (3 a 12 pares), selecao, desmarcacao e troca de base.
- Erros sem acerto, resolucao completa, conclusao unica e cancelamento unico.
- Reinicio por Setup depois de a cena estar pronta.
- Perguntas corretas/incorretas, explicacao e resultado somente ao Continuar.
- Abertura dos dois puzzles pela fase, pausa, reparo e retomada do personagem.
- Retorno ao seletor, desbloqueio e recarga de assets com os botoes C#.

As etapas anteriores tambem verificaram movimento, colisoes, camera, pares
complementares, multiplos danos, persistencia e conclusao da fase 10.
Para conferir a aparencia, use F5, abra uma fase, resolva o puzzle e alcance
FIM; teste com placeholders e com suas imagens personalizadas.
O `.csproj`, `.sln`, TargetFramework e `project.godot` foram preservados
nesta etapa.

## Estrutura e funcionalidades prontas

- `scenes/`: menu, menu de pausa, tela final, seletor, fase compartilhada, Cas9, marcador, trechos e componentes.
- `scenes/puzzles/`: pareamento de bases, pergunta e heredograma.
- `scripts/`: Menu, MenuPausa, TelaFinal, SeletorFases, Fase, Cas9, FitaDna, TrechoDanificado, MarcadorFim e Acoes.
- `scripts/componentes/`: VisualSlot, BotaoVisual, BotaoBase e PecaHeredograma.
- `scripts/puzzles/`: PareamentoBases, Pergunta e Heredograma.
- `scripts/autoload/`: GameState, SceneRouter e Assets.
- `data/`: configuracao das dez fases, perguntas, heredogramas e ajustes opcionais dos visuais.
- `assets/sprites/` e `assets/audio/`: imagens e sons opcionais; `assets/LEIA-ME.md`: catalogo dos ids.
- Raiz: `project.godot`, `Crispr-Puzzzle.csproj` e `Crispr-Puzzzle.sln`.

Prontos: menu, pausa, seletor com progresso persistente, exploracao do DNA, reparos,
puzzles de pareamento, perguntas e heredograma, conclusao e desbloqueio da fase seguinte,
e tela final ao completar as dez fases.

## Seletor de fases

Execute com **F5 no Godot** e clique em **Jogar**. O seletor mostra dez fases
em duas linhas. Em um jogo novo, somente a fase 1 está liberada. As demais
ficam desabilitadas e exibem um cadeado (placeholder X).
O botão **Voltar ao menu** retorna pelo SceneRouter.

O GameState salva a fase selecionada e o progresso em `user://save.cfg`.
Saves existentes são preservados. `GetNode<GameState>("/root/GameState").ConcluirFase(numero)` marca
a fase como concluída, libera a seguinte e salva; o seletor acompanha o signal
`ProgressoAlterado` e mostra o ícone de conclusão (placeholder OK).
A fase 10 não libera uma fase 11.

Clicar numa fase liberada salva a seleção e abre a cena compartilhada
`scenes/fase.tscn`, configurada por `data/fases.json`.

## Fase base: exploração do DNA

Execute **F5 → Jogar → Fase 1**. Use **A/D ou setas esquerda/direita**
para mover Cas9 ao longo da fita. As ações `mover_esq` e `mover_dir`
podem ser remapeadas em Projeto → Configurações do Projeto → Input Map.
A câmera acompanha o personagem e respeita as extremidades da fase.
O botão **Voltar ao seletor** retorna sem alterar os desbloqueios.

Cas9 é um CharacterBody2D com movimento horizontal limitado à fita.
Cada par mostra bases complementares A–T ou G–C, com cores distintas e
letras visíveis mesmo quando há imagens. A representação é esquemática:
Cas9 é guiada por um RNA guia e corta o DNA no local-alvo; a célula realiza
o reparo. O pareamento é uma atividade educativa sobre complementaridade;
não representa literalmente o mecanismo celular de reparo.

Este marco permite explorar e reparar o caminho com pareamento ou pergunta.
Depois de reparar todos os trechos, tocar o marcador **FIM** conclui a fase,
salva o progresso, libera a seguinte e retorna automaticamente ao seletor nas fases 1-9.
Não é necessário pressionar E no marcador. Ao concluir a fase 10 com todas
as dez fases concluídas, abre a tela final, sem criar uma fase 11.
Rejogar uma fase concluída preserva os desbloqueios.
O JSON contém as dez configurações: número, dificuldade, tipos de puzzle
(`pareamento_bases`, `pergunta` e `heredograma`), quantidade de trechos danificados (1 a 5), quantidade
de pares e sequência repetida na fita superior. A fita inferior é gerada
por complementaridade.

### Trecho danificado e pareamento de bases

O trecho danificado é um StaticBody2D que bloqueia Cas9 nos dois sentidos.
Perto dele, use **E**, ação `interagir` do Input Map, para abrir o puzzle.
A dica mostra o mapeamento atual da ação. Longe do dano, a ação não faz nada.

A fase sorteia um dos tipos permitidos em `tipos_puzzle`, instancia o puzzle
sobre a tela atual e suspende o movimento. As instruções abaixo descrevem o
pareamento; o tipo pergunta está explicado na próxima seção.
Escolha uma base de cada lado, usando o mouse ou Tab/setas e a ação padrão
de confirmação. A combina com T e G combina com C, também na ordem inversa.
Qualquer cópia da base complementar é válida: não há pares ocultos por índice.
Clique novamente na base selecionada para desfazer a seleção, ou escolha
outra do mesmo lado para substituí-la.

Os pares são sorteados a cada abertura. Uma lista recebe bases aleatórias e
a outra recebe seus complementos; ambas são embaralhadas, garantindo solução.
Bases repetidas são permitidas. Um erro mostra a regra e libera nova tentativa,
sem limite de erros neste marco. Pares corretos ficam desabilitados com OK,
e o contador mostra quantos faltam. As letras permanecem visíveis sobre os
VisualSlots, inclusive com imagens personalizadas.

O puzzle recebe `Setup(int dificuldade)` e emite
`PuzzleConcluido(bool sucesso)` uma única vez por tentativa: `true` ao completar
todos os pares, ou `false` ao usar **Sair do puzzle**. Sair não repara o dano;
abrir novamente inicia um novo sorteio.

A dificuldade controla somente a quantidade de pares: `dificuldade + 2`,
limitando a entrada entre 1 e 10 (3 a 12 pares). A dificuldade vem de
`data/fases.json`; o campo `quantidade_pares` desse JSON continua sendo o
comprimento da fita da fase, não o tamanho do puzzle.

A fase recebe o sucesso, remove a colisão, troca o visual para reparado e
devolve o controle a Cas9 na mesma posição. Repetir a interação em um trecho
reparado não reabre o puzzle.

Os reparos duram durante essa visita à fase; sair e entrar novamente recria
os danos. Sair antes de alcançar FIM não marca a fase como concluída.
Conclusões e desbloqueios já salvos continuam preservados.

Teste com **F5 → Jogar → Fase 1**. Para testar apenas pareamento, configure
`"tipos_puzzle": ["pareamento_bases"]` na fase desejada:

1. Caminhe para a direita até o trecho vermelho, aproximadamente no meio
   da fita, e confira que ele impede a passagem.
2. Pressione E: o puzzle abre uma única vez e Cas9 não se move com A/D.
3. Experimente uma combinação incorreta, quando houver, e confirme que ela
   não conta como acerto. Complete os pares: o trecho fica verde/OK, o puzzle
   fecha e o personagem pode atravessar nos dois sentidos.
4. Pressione E novamente perto do trecho reparado: nada deve abrir.
5. Continue até FIM: o seletor deve abrir com a fase 1 concluída e a fase 2
   liberada. Reinicie o jogo e confira que esses estados persistem.
6. Reentre na fase 1: o dano reaparece para a nova tentativa, mas o progresso
   salvo permanece. Voltar ao seletor antes de FIM não conclui uma fase nova.
7. Use Sair do puzzle durante uma tentativa: o dano deve continuar bloqueado.
   Reabra e confira o novo sorteio. Na fase 1 são 3 pares; na fase 10 são 12.

O marcador verifica todos os trechos, inclusive quando o JSON configura mais
de um dano. Chegar a ele com algum dano pendente não conclui a fase.
Uma fase configurada sem danos pode ser concluída diretamente em FIM.

### Perguntas de múltipla escolha

O script [`Pergunta.cs`](scripts/puzzles/Pergunta.cs) lê o arquivo por
`Godot.FileAccess` e o interpreta com `System.Text.Json`. As 40 questões
cobrem DNA/RNA, expressão gênica, herança mendeliana, mutações e CRISPR-Cas9,
distribuídas entre as dificuldades 1 e 10. O JSON está formatado com um campo
por linha para facilitar a edição.

Exemplo de abertura a partir de um nó C# (já integrado em `Fase.cs`):

```csharp
PackedScene cena = GD.Load<PackedScene>("res://scenes/puzzles/pergunta.tscn");
Pergunta puzzle = cena.Instantiate<Pergunta>();
puzzle.Setup(3);
puzzle.PuzzleConcluido += AoPuzzleConcluido;
AddChild(puzzle);
```

O receptor tem assinatura `void AoPuzzleConcluido(bool sucesso)`.
O sinal é declarado com `[Signal]` e emitido uma única vez por tentativa.
`Setup` também pode reiniciar a cena aberta, sorteando uma questão e limpando
a resposta anterior. Responder exibe o resultado e a explicação; o sinal
aguarda **Continuar**, permitindo a leitura antes de voltar à fase.

Validação estrutural do banco: 40 enunciados distintos, quatro alternativas por
questão, índices corretos dentro dos limites e explicações preenchidas.
Cada dificuldade de 1 a 10 tem quatro perguntas. O conteúdo precisa de revisão científica.

`scenes/puzzles/pergunta.tscn` usa a mesma interface
`Setup(int dificuldade)` e `PuzzleConcluido(bool sucesso)`.
O conteúdo vem exclusivamente de `data/perguntas.json`, com 40 exemplos
de biologia molecular e genética, em linguagem de ensino médio.

Cada entrada tem:

| Campo | Formato |
| --- | --- |
| `enunciado` | Texto da pergunta |
| `alternativas` | Lista de textos; os exemplos têm quatro opções |
| `indice_correto` | Índice começando em **0**: 0 é a primeira alternativa |
| `dificuldade` | Inteiro entre 1 e 10 |
| `explicacao` | Texto apresentado após qualquer resposta |

O sorteio usa o maior nível disponível que não ultrapasse a dificuldade
recebida. Por exemplo, dificuldade 3 sorteia entre as perguntas de nível 3;
se esse nível não existir, procura o nível inferior mais próximo.
As alternativas mantêm a ordem do JSON. Entradas inválidas são ignoradas;
se não houver pergunta elegível, aparece um aviso e o jogador pode sair.

Clique numa alternativa ou confirme pelo teclado. A resposta fica bloqueada
e a tela mostra se houve acerto, a alternativa correta e a explicação.
O botão **Continuar** só aparece depois de responder: ele devolve o controle
à fase e emite sucesso apenas se a resposta estava correta. Uma resposta
errada mantém o trecho bloqueado, permitindo nova tentativa ao interagir.
**Sair do puzzle**, antes de responder, também mantém o dano.
O texto possui rolagem para permitir ler explicações maiores.

Para testar:

1. Configure temporariamente `"tipos_puzzle": ["pergunta"]` na fase 1, ou
   abra `scenes/puzzles/pergunta.tscn` no editor e pressione F6 para inspecionar
   a tela isolada (sem a fase para consumir o sinal e fechá-la).
2. Com F5, entre na fase e interaja no dano. Responda errado: leia a explicação,
   clique em Continuar e confirme que a passagem continua bloqueada.
3. Tente novamente e acerte: a explicação deve permanecer até Continuar;
   só então o trecho é reparado.
4. Restaure os tipos originais dessa fase no JSON para manter a progressao.

### Assets da fase

Todos são opcionais e usam VisualSlot. Arquivos em `assets/sprites/<id>.png`.

| Id | Uso | Tamanho de referência |
| --- | --- | --- |
| `player/cas9` | Personagem | 96 × 96 |
| `player/cas9_andando_1` … `_N` | Animação opcional, pausada quando para | 96 × 96 por quadro |
| `dna/base_a`, `dna/base_t`, `dna/base_g`, `dna/base_c` | Pares complementares | 64 × 64 |
| `dna/trecho_saudavel` | Segmentos que formam o caminho | 80 × 224 |
| `dna/trecho_danificado` | Bloqueio; placeholder vermelho/DANO | 80 × 224 |
| `dna/trecho_reparado` | Passagem liberada; placeholder verde/OK | 80 × 224 |
| `fundos/fundo_puzzle` | Fundo do pareamento e da pergunta | 1280 × 720 |
| `fundos/fundo_fase_01` … `fundos/fundo_fase_10` | Fundo fixo da fase atual | 1280 × 720 |
| `ui/marcador_fim` | Marcador de chegada; placeholder azul com FIM | 80 × 96 |

Teste percorrendo a fita nos dois sentidos: a câmera acompanha Cas9,
o personagem não ultrapassa as extremidades e a interface permanece fixa.
Confirme também os pares A–T/G–C e o retorno ao seletor.

### Assets do seletor

Todos são opcionais, em `assets/sprites/<id>.png`. Fundo, botões e ícones
usam VisualSlot, com placeholders e suporte a `data/visuais.json`.

| Id | Uso | Tamanho de referência |
| --- | --- | --- |
| `fundos/fundo_seletor` | Fundo | 1280 × 720 |
| `ui/botao_normal` | Fase disponível e Voltar | 180 × 130 / 220 × 56 |
| `ui/botao_hover` | Mouse, pressionado e foco por teclado | Mesmo tamanho do botão |
| `ui/botao_bloqueado` | Fase bloqueada | 180 × 130 |
| `ui/icone_cadeado` | Indicador de bloqueio | 36 × 36 |
| `ui/icone_fase_concluida` | Indicador de conclusão | 36 × 36 |

Consulte [o guia de assets](assets/LEIA-ME.md) para todos os ids e instruções
de importação e recarga.

### Verificação manual

1. Sem save anterior, abra Jogar: fase 1 disponível e fases 2–10 bloqueadas.
2. Use Tab/setas e a ação padrão de confirmação; fases bloqueadas não ativam.
3. Clique na fase 1, volte ao seletor, depois ao menu e abra Jogar novamente:
   a seleção persiste.
4. Repare o trecho da fase 1 e alcance FIM: fase 1 ganha OK e fase 2 fica
   disponível. Reinicie e confira que os estados persistem.
5. Adicione as imagens, aguarde a importação e execute: os placeholders são
   substituídos, mantendo os rótulos dos botões.

Projeto consiste na criação de um jogo temático na disciplina de biologia. A ideia é que o player, que representa o Crispr-Cas9, técnica de edição gênica, vai passar pelo DNA e quando identificar "erros" irá concertá-los a partir de puzzles completados

## Heredogramas fixos

`data/heredogramas.json` contem oito familias de heranca autossomica
recessiva, com genotipos explicitos e de um a oito filhos (3 a 10 pessoas).
A caracteristica e ficticia, com penetrancia completa: somente aa e afetado.
O puzzle escolhe o maior nivel do campo `dificuldade` que nao ultrapassa o
nivel recebido em Setup. Sem candidato elegivel, mostra um aviso para sair.
Em caso de empate, usa a primeira entrada do JSON. Nao ha geracao nem
embaralhamento dos heredogramas; o sorteio do tipo de puzzle pela fase permanece.
A ordem dos individuos e indicada por I-1, I-2 e II-1 etc. A barra horizontal
entre irmaos reutiliza `linha_uniao`; as linhas verticais indicam filiacao.

Arraste as pecas da bandeja para os sinais +. Soltar sobre uma peca troca as
posicoes; pecas do mesmo tipo sao intercambiaveis. **Verificar** confere todos
os encaixes (sexo, estado afetado e ligacoes). Erros permitem novas tentativas;
o acerto bloqueia a montagem e mostra uma explicacao. **Continuar** emite sucesso;
**Sair do puzzle** emite falha. Cada tentativa emite o sinal uma unica vez.
`Setup(int dificuldade)` tambem reinicia uma cena ja aberta.

Para testar com **F5 no Godot .NET**, configure temporariamente a fase 1 com
`"tipos_puzzle": ["heredograma"]` e dificuldades de 1 a 10. Confira montagem
incompleta, pessoa trocada, ligacao errada, correcao e retorno ao DNA somente
apos Continuar; sair deve manter o dano. Restaure o JSON depois do teste.
Tambem e possivel abrir `scenes/puzzles/heredograma.tscn` com F6 e ajustar
Dificuldade no Inspector. Todos os cinco ids de pecas usam VisualSlot e
funcionam sem imagens. Pessoas afetadas combinam o simbolo de sexo com o
VisualSlot `heredograma/afetado`.

Os campos `pessoas` (id, sexo, genotipo, afetado e pais), `heranca` e
`penetrancia` documentam a solucao para auditoria. `encaixes` define as pecas
e suas posicoes; `revisao_cientifica: "pendente"` registra a revisao ainda necessaria.
A bandeja comporta ate 25 pecas em cinco colunas, com pecas de 48 x 48.
O maior modelo usa 21 pecas. Os simbolos no diagrama continuam com 56 x 56.

| Modelo | Dificuldade minima | Dificuldades que o selecionam | Pessoas | Pecas |
| --- | --- | --- | --- | --- |
| familia_01 | 1 | 1 | 3 | 5 |
| familia_02 | 2 | 2 | 4 | 9 |
| familia_03 | 3 | 3 | 5 | 11 |
| familia_04 | 4 | 4 | 6 | 13 |
| familia_05 | 5 | 5 | 7 | 15 |
| familia_06 | 6 | 6-7 | 8 | 17 |
| familia_07 | 8 | 8-9 | 9 | 19 |
| familia_08 | 10 | 10 | 10 | 21 |

A progressao das fases continua introduzindo heredograma na fase 5. Os
modelos de niveis 1-4 podem ser testados com F6 e a propriedade Dificuldade,
ou habilitados em `tipos_puzzle` das fases iniciais para uso pedagogico.

**Conteudo precisa de revisao cientifica antes de uso didatico.** A verificacao
dos genotipos e das solucoes nao substitui revisao dos enunciados, explicacoes
e adequacao das dificuldades por um professor de biologia.
Referencias de apoio: [heranca recessiva e probabilidades (MedlinePlus)](https://medlineplus.gov/genetics/understanding/inheritance/riskassessment/)
e [funcionamento da edicao genomica (NHGRI)](https://www.genome.gov/about-genomics/policy-issues/Genome-Editing/How-genome-editing-works).

## Sorteio de puzzles e novas tentativas

A cada interacao com um trecho danificado, `Fase.cs` sorteia um dos tipos
validos de `tipos_puzzle` em `data/fases.json`: `pareamento_bases`, `pergunta`
ou `heredograma`. Tipos repetidos contam uma vez; desconhecidos sao ignorados.
Sem tipos validos, a fase mostra um aviso e permite voltar ao seletor, sem
abrir um puzzle fora da configuracao.

A instancia escolhida recebe `Setup(dificuldade)` antes de entrar na arvore.
Enquanto o puzzle esta aberto, Cas9 fica parado. O sinal
`PuzzleConcluido(false)` fecha o puzzle e devolve o controle sem reparar o
trecho; a mensagem orienta a interagir novamente. Cada nova tentativa faz
outro sorteio, que pode repetir o mesmo tipo. Nao ha limite de tentativas.
Somente `PuzzleConcluido(true)` repara o DNA e libera a passagem.
Erros corrigiveis dentro de pareamento e heredograma continuam permitindo
corrigir a montagem na mesma tentativa.

Teste com F5 no Godot .NET: interaja num dano, erre uma pergunta e clique em
Continuar (ou saia de qualquer puzzle); confira que o dano permanece e tente
novamente. Ao acertar, confira que a passagem abre. Para conferir cada tipo,
use temporariamente apenas seu identificador em `tipos_puzzle` e depois
restaure a lista desejada.

Validacao deste marco: dotnet build com zero erros e avisos. Testes headless
verificaram tipos individuais e mistos, listas vazias/invalidas/duplicadas,
Setup com dificuldade 7 nos tres puzzles, pausa e retomada, novas tentativas
apos falha e reparo somente no sucesso.

## Configuracao das dez fases

`data/fases.json` define a progressao abaixo. A mesma `scenes/fase.tscn`
le a entrada cujo `numero` corresponde a fase selecionada no GameState,
usando Godot.FileAccess e System.Text.Json. Nao ha cenas duplicadas por fase.

| Fase | Dificuldade | Tipos permitidos | Trechos danificados |
| --- | --- | --- | --- |
| 1 | 1 | Pareamento | 1 |
| 2 | 2 | Pareamento | 1 |
| 3 | 3 | Pareamento, pergunta | 2 |
| 4 | 4 | Pareamento, pergunta | 2 |
| 5 | 5 | Pareamento, pergunta, heredograma | 3 |
| 6 | 6 | Pareamento, pergunta, heredograma | 3 |
| 7 | 7 | Pareamento, pergunta, heredograma | 4 |
| 8 | 8 | Pareamento, pergunta, heredograma | 4 |
| 9 | 9 | Pareamento, pergunta, heredograma | 5 |
| 10 | 10 | Pareamento, pergunta, heredograma | 5 |

Campos: `numero`, `dificuldade`, `tipos_puzzle` e `trechos_danificados`.
Os campos `quantidade_pares` e `sequencia` continuam definindo o comprimento
e as bases da fita. Os danos sao distribuidos ao longo dela, e todos precisam
ser reparados antes de concluir a fase. O sorteio por tentativa pode repetir
tipos: permitir um tipo novo nao garante que ele apareca naquela visita.

Cada fase configura o VisualSlot do fundo com `fundos/fundo_fase_NN`, de
`fundos/fundo_fase_01` a `fundos/fundo_fase_10`. As imagens opcionais ficam em
`assets/sprites/fundos/fundo_fase_NN.png`, com referencia de 1280 x 720.
Sem imagem, aparece o placeholder. Overrides em `data/visuais.json` continuam
funcionando; nenhum caminho de imagem foi inserido nas cenas.

Teste com F5 no Godot .NET: avance pelas fases e confira a tabela, o bloqueio
nos danos e a liberacao de FIM apenas depois de todos os reparos. As duas
primeiras fases sempre abrem pareamento. Para testar o fundo opcional,
adicione a imagem da fase desejada, aguarde a importacao e execute novamente.

Validacao desta progressao: dotnet build sem erros ou avisos. Teste headless
nas dez fases confirmou leitura do JSON, quantidade de danos, tipos de puzzle
permitidos, reparo de todos os trechos e ids de fundo com fallback sem imagem.

Validacao da expansao do conteudo: dotnet build sem erros ou avisos; 40
enunciados distintos e quatro questoes por nivel. Conferencia dos oito modelos
verificou genotipos possiveis, fenotipos e correspondencia com as pecas.
Testes headless nos dez niveis confirmaram selecao, montagem incorreta/correta,
conclusao unica, reinicio, cancelamento e acesso ao gabarito das 40 perguntas.
As pecas e enunciados couberam nas areas reservadas. Confira a aparencia
e o gesto de arrastar com F5/F6; a revisao cientifica continua pendente.

## Menu de pausa

Durante a fase ou qualquer puzzle, use a acao `pausar` (Esc por padrao) para
abrir o menu. Esc novamente ou **Continuar** retoma exatamente a mesma visita,
com a montagem/respostas do puzzle preservadas. A arvore fica pausada e apenas
o menu processa entrada; a pausa cancela um arraste em andamento sem mover a
peca e devolve o foco anterior ao continuar. Os botoes aceitam mouse e teclado.

**Voltar ao seletor** e **Menu principal** usam SceneRouter e retiram a pausa
antes de mostrar a tela de destino. Sair descarta reparos e puzzles da visita
atual, mas preserva fases concluidas e desbloqueios salvos. Em caso de falha
na troca de tela, o menu permanece disponivel para continuar ou tentar sair.
O fundo `fundos/fundo_pausa` e opcional e os botoes reutilizam VisualSlot.

Teste com F5 no Godot .NET: pause ao andar, confira que Cas9 para, continue;
abra cada puzzle, faca parte da resolucao, pause e retome. Confira que nada
muda sob o menu. Teste as duas saidas e confirme que a tela de destino responde.
`project.godot` recebeu somente a acao `pausar` ligada a Esc no Input Map.

Pausa validada com build sem erros/avisos e testes headless: Input Map,
interrupcao do processamento, retomada e foco nos tres puzzles, saidas
para seletor/menu e permanencia na pausa quando uma troca de tela falha.

## Tela final da campanha

Ao reparar todos os danos e alcancar FIM na fase 10, o GameState salva a
conclusao e a fase verifica `TodasFasesConcluidas`. Essa propriedade exige
os numeros 1 a 10 no progresso; nao basta contar entradas. SceneRouter abre
`scenes/tela_final.tscn`, que mostra a conclusao das dez fases e oferece
**Voltar ao seletor** e **Menu principal**. O save continua em `user://save.cfg`,
sem mudanca de formato nem reinicio de progresso.

Fases 1-9 continuam voltando ao seletor. Rejogar e concluir a fase 10 mostra
a tela final novamente; rejogar uma fase anterior volta ao seletor normalmente.
O fundo opcional `fundos/fundo_final` e os botoes usam VisualSlot, funcionando
com placeholders. F6 na cena final sem todas as conclusoes mostra uma mensagem
de jornada em andamento, em vez de afirmar que a campanha foi concluida.

Teste com F5 no Godot .NET: conclua as dez fases e confira a tela final;
volte ao seletor, confirme as dez conclusoes e reinicie para conferir o save.
Teste tambem o botao Menu principal. O build passou sem erros ou avisos.
Testes headless com save isolado confirmaram bloqueio da conclusao com danos,
retorno da fase 1 ao seletor, desbloqueio da fase 2, tela final apos a fase 10,
recarga das dez conclusoes, ausencia de fase 11 e os dois botoes de navegacao.
