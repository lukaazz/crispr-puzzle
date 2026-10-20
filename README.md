# crispr-puzzle

## Seletor de fases

Execute com **F5 no Godot** e clique em **Jogar**. O seletor mostra dez fases
em duas linhas. Em um jogo novo, somente a fase 1 está liberada. As demais
ficam desabilitadas e exibem um cadeado (placeholder X).
O botão **Voltar ao menu** retorna pelo SceneRouter.

O GameState salva a fase selecionada e o progresso em `user://save.cfg`.
Saves existentes são preservados. `GameState.concluir_fase(numero)` marca
a fase como concluída, libera a seguinte e salva; o seletor acompanha o signal
`progresso_alterado` e mostra o ícone de conclusão (placeholder OK).
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
o reparo. O botão do puzzle de teste representa apenas a conclusão desse desafio.

Este marco permite explorar e reparar o caminho com um puzzle de teste.
Depois de reparar todos os trechos, tocar o marcador **FIM** conclui a fase,
salva o progresso, libera a seguinte e retorna automaticamente ao seletor.
Não é necessário pressionar E no marcador. Na fase 10, a conclusão é salva
sem criar uma fase 11. Rejogar uma fase concluída preserva os desbloqueios.
O JSON contém as dez configurações: número, dificuldade, tipos de puzzle
(`teste` neste marco), quantidade de trechos danificados (um por fase), quantidade
de pares e sequência repetida na fita superior. A fita inferior é gerada
por complementaridade.

### Trecho danificado e puzzle de teste

O trecho danificado é um StaticBody2D que bloqueia Cas9 nos dois sentidos.
Perto dele, use **E**, ação `interagir` do Input Map, para abrir o puzzle.
A dica mostra o mapeamento atual da ação. Longe do dano, a ação não faz nada.

A fase instancia o puzzle sobre a tela atual e suspende o movimento. Clique
em **Consertar** (ou confirme o botão em foco pelo teclado). O puzzle recebe
`setup(dificuldade: int)` e emite `puzzle_concluido(sucesso: bool)`.
A fase recebe o sucesso, remove a colisão, troca o visual para reparado e
devolve o controle a Cas9 na mesma posição. Repetir a interação em um trecho
reparado não reabre o puzzle.

Os reparos duram durante essa visita à fase; sair e entrar novamente recria
os danos. Sair antes de alcançar FIM não marca a fase como concluída.
Conclusões e desbloqueios já salvos continuam preservados.

Teste com **F5 → Jogar → Fase 1**:

1. Caminhe para a direita até o trecho vermelho, aproximadamente no meio
   da fita, e confira que ele impede a passagem.
2. Pressione E: o puzzle abre uma única vez e Cas9 não se move com A/D.
3. Clique em Consertar: o trecho fica verde/OK, o puzzle fecha e o personagem
   pode atravessar nos dois sentidos.
4. Pressione E novamente perto do trecho reparado: nada deve abrir.
5. Continue até FIM: o seletor deve abrir com a fase 1 concluída e a fase 2
   liberada. Reinicie o jogo e confira que esses estados persistem.
6. Reentre na fase 1: o dano reaparece para a nova tentativa, mas o progresso
   salvo permanece. Voltar ao seletor antes de FIM não conclui uma fase nova.

O marcador verifica todos os trechos, inclusive quando o JSON configura mais
de um dano. Chegar a ele com algum dano pendente não conclui a fase.
Uma fase configurada sem danos pode ser concluída diretamente em FIM.

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
| `fundos/fundo_puzzle` | Fundo do puzzle de teste | 1280 × 720 |
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
