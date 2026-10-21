# crispr-puzzle

Jogo 2D educativo sobre CRISPR-Cas9: explore o DNA, resolva puzzles e repare
os trechos para concluir dez fases progressivas. Plataforma alvo: desktop Windows.

## Como executar e estado da migracao

Abra `project.godot` na **Godot 4.x edicao .NET**, com o **SDK .NET 8**
instalado (o projeto usa Godot.NET.Sdk 4.6.1 e net8.0). Execute `dotnet build`
na raiz e depois **F5** no editor.

Etapa 1: `GameState.cs`, `SceneRouter.cs` e `Assets.cs` sao os autoloads ativos.
Os tres autoloads `.gd` antigos foram preservados, sem alteracoes, para revisao.
Etapa 2: `scripts/componentes/VisualSlot.cs`, `scripts/Menu.cs` e
`scripts/SeletorFases.cs` substituem os scripts GDScript dessas cenas.
Os tres `.gd` desta etapa e seus `.uid` foram removidos depois do build aprovado.
As demais cenas continuam em GDScript, com chamadas ao VisualSlot em PascalCase
e referencias ao tipo nativo Control para interoperabilidade. Nenhuma funcionalidade nova.
As propriedades exportadas nas instancias `.tscn` tambem foram atualizadas.

VisualSlot preserva placeholders, imagens proporcionais, animacao opcional,
pausa ao parar Cas9 e os ajustes de arquivo, escala e offset em `data/visuais.json`.
Menu preserva o tema nativo quando faltam imagens. Seletor preserva dez botoes,
foco na fase atual, bloqueios, conclusoes e navegacao pelo SceneRouter.
Os sinais dos autoloads sao desconectados quando as cenas saem da arvore.

O padrao unico de acesso e `GetNode<GameState>("/root/GameState")` em C#
(analogamente para SceneRouter e Assets), e `get_node("/root/GameState")`
em GDScript. Nao usamos propriedades estaticas `Instance`.
Os sinais agora sao `ProgressoAlterado` e `AssetsRecarregados`.
A API de assets usa `Textura`, `Som`, `ConfiguracaoVisual`, `EscalaVisual`,
`DeslocamentoVisual` e `Recarregar`. O formato do save continua inalterado.

Validacao da etapa 2: build aprovado com zero erros e zero avisos, usando
`& 'C:/Program Files/dotnet/dotnet.exe' build` no PowerShell (SDK fora do PATH).
Godot .NET 4.6.1 em modo headless: menu, seletor, fase e ambos os puzzles
inicializaram; verificacao automatizada confirmou navegacao menu/seletor/menu,
foco, dez fases, bloqueios, placeholders e recarga repetida sem erros.
A aparencia visual com imagens personalizadas deve ser conferida com F5.
O `.csproj`, `.sln`, TargetFramework e `project.godot` nao mudaram nesta etapa.

Para conferir a migracao com F5: abra menu e seletor, entre numa fase,
resolva o puzzle, alcance FIM e reinicie para verificar o progresso salvo.
Confira tambem os placeholders sem imagens e os visuais com imagens existentes;
`get_node("/root/Assets").Recarregar()` limpa os caches e atualiza os consumidores.

## Estrutura e funcionalidades prontas

- `scenes/`: menu, seletor, fase compartilhada, Cas9, componentes e puzzles.
- `scripts/`: Menu.cs, SeletorFases.cs e scripts GDScript ainda nao migrados;
  `scripts/componentes/`: VisualSlot.cs e botoes GDScript;
  `scripts/autoload/`: os tres autoloads C# e suas versoes antigas.
- `data/`: configuracao das dez fases, perguntas e ajustes opcionais de visuais.
- `assets/sprites/` e `assets/audio/`: imagens e sons opcionais; `assets/LEIA-ME.md`: catalogo dos ids.
- Raiz: `project.godot`, `Crispr-Puzzzle.csproj` e `Crispr-Puzzzle.sln`.

Prontos: menu, seletor com progresso persistente, exploracao do DNA, reparos,
puzzles de pareamento e perguntas, conclusao e desbloqueio da fase seguinte.
O puzzle de heredograma ainda nao esta implementado.

## Seletor de fases

Execute com **F5 no Godot** e clique em **Jogar**. O seletor mostra dez fases
em duas linhas. Em um jogo novo, somente a fase 1 está liberada. As demais
ficam desabilitadas e exibem um cadeado (placeholder X).
O botão **Voltar ao menu** retorna pelo SceneRouter.

O GameState salva a fase selecionada e o progresso em `user://save.cfg`.
Saves existentes são preservados. `get_node("/root/GameState").ConcluirFase(numero)` marca
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
salva o progresso, libera a seguinte e retorna automaticamente ao seletor.
Não é necessário pressionar E no marcador. Na fase 10, a conclusão é salva
sem criar uma fase 11. Rejogar uma fase concluída preserva os desbloqueios.
O JSON contém as dez configurações: número, dificuldade, tipos de puzzle
(`pareamento_bases` e `pergunta`), quantidade de trechos danificados (um por fase), quantidade
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

O puzzle recebe `setup(dificuldade: int)` e emite
`puzzle_concluido(sucesso: bool)` uma única vez por tentativa: `true` ao completar
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

`scenes/puzzles/pergunta.tscn` usa a mesma interface
`setup(dificuldade: int)` e `puzzle_concluido(sucesso: bool)`.
O conteúdo vem exclusivamente de `data/perguntas.json`, com 15 exemplos
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
4. Restaure os dois tipos no JSON para voltar ao sorteio entre puzzles.

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
