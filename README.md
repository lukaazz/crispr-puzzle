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
Cas9 é guiada por um RNA guia; o corte e o reparo celular serão abordados
nos próximos marcos.

Este marco permite explorar o caminho, sem puzzles, danos ou conclusão
automática. Chegar ao fim não libera a próxima fase.
O JSON contém as dez configurações: número, dificuldade, tipos de puzzle
(vazios neste marco), quantidade de trechos danificados (zero), quantidade
de pares e sequência repetida na fita superior. A fita inferior é gerada
por complementaridade.

### Assets da fase

Todos são opcionais e usam VisualSlot. Arquivos em `assets/sprites/<id>.png`.

| Id | Uso | Tamanho de referência |
| --- | --- | --- |
| `player/cas9` | Personagem | 96 × 96 |
| `player/cas9_andando_1` … `_N` | Animação opcional, pausada quando para | 96 × 96 por quadro |
| `dna/base_a`, `dna/base_t`, `dna/base_g`, `dna/base_c` | Pares complementares | 64 × 64 |
| `dna/trecho_saudavel` | Segmentos que formam o caminho | 80 × 224 |
| `fundos/fundo_fase_01` … `fundos/fundo_fase_10` | Fundo fixo da fase atual | 1280 × 720 |

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
4. Para testar progressão antes dos puzzles, chame
   `GameState.concluir_fase(1)` pelo código de teste: fase 1 ganha OK e fase 2
   fica disponível. Reinicie e confira que os estados persistem.
   Esse passo altera o save de teste.
5. Adicione as imagens, aguarde a importação e execute: os placeholders são
   substituídos, mantendo os rótulos dos botões.

Projeto consiste na criação de um jogo temático na disciplina de biologia. A ideia é que o player, que representa o Crispr-Cas9, técnica de edição gênica, vai passar pelo DNA e quando identificar "erros" irá concertá-los a partir de puzzles completados
