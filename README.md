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

A cena de gameplay ainda não foi criada. Por enquanto, clicar numa fase
liberada salva a seleção e exibe um aviso no seletor.

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
3. Clique na fase 1, volte ao menu e abra Jogar novamente: a seleção persiste.
4. Para testar progressão antes do gameplay, chame
   `GameState.concluir_fase(1)` pelo código de teste: fase 1 ganha OK e fase 2
   fica disponível. Reinicie e confira que os estados persistem.
   Esse passo altera o save de teste.
5. Adicione as imagens, aguarde a importação e execute: os placeholders são
   substituídos, mantendo os rótulos dos botões.

Projeto consiste na criação de um jogo temático na disciplina de biologia. A ideia é que o player, que representa o Crispr-Cas9, técnica de edição gênica, vai passar pelo DNA e quando identificar "erros" irá concertá-los a partir de puzzles completados
