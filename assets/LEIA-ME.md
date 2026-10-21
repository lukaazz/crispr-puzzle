# Assets do CRISPR Puzzle

Todos os arquivos são opcionais. Sem imagens, o jogo usa placeholders; sem
sons, `Assets.som()` retorna `null`. Use nomes em snake_case, sem acentos,
e PNG com transparência quando apropriado. Os tamanhos são sugestões.

## Imagens usadas neste marco

Caminhos relativos a `assets/sprites/`. O id não inclui `.png`.

| Id | Onde aparece | Tamanho recomendado | Opcional |
| --- | --- | --- | --- |
| `fundos/fundo_menu` | Fundo do menu, via VisualSlot | 1280 × 720 | Sim |
| `fundos/fundo_seletor` | Fundo do seletor, via VisualSlot | 1280 × 720 | Sim |
| `ui/botao_normal` | Menu e seletor, estado normal | 520 × 60 no menu; 180 × 130 nas fases; 220 × 56 em Voltar | Sim |
| `ui/botao_hover` | Destaque dos botões; também foco por teclado no seletor | Mesmos tamanhos de `botao_normal` | Sim |
| `ui/botao_bloqueado` | Fases bloqueadas no seletor | 180 × 130 | Sim |
| `ui/icone_cadeado` | Fases bloqueadas; placeholder X e texto Bloqueada | 36 × 36 | Sim |
| `ui/icone_fase_concluida` | Fases concluídas; placeholder OK e texto Concluida | 36 × 36 | Sim |
| `ui/marcador_fim` | Chegada na extremidade direita da fita; placeholder azul/FIM | 80 × 96 | Sim |
| `player/cas9` | Personagem na fita; placeholder Cas9 | 96 × 96 | Sim |
| `player/cas9_andando_1` … `player/cas9_andando_N` | Animação durante o movimento | 96 × 96 por quadro | Sim |
| `dna/base_a` | Adenina na fita e no puzzle, verde, letra A | 64 × 64 | Sim |
| `dna/base_t` | Timina na fita e no puzzle, laranja, letra T | 64 × 64 | Sim |
| `dna/base_g` | Guanina na fita e no puzzle, amarela, letra G | 64 × 64 | Sim |
| `dna/base_c` | Citosina na fita e no puzzle, azul, letra C | 64 × 64 | Sim |
| `dna/trecho_saudavel` | Segmentos contíguos do caminho de DNA | 80 × 224 | Sim |
| `dna/trecho_danificado` | Trecho que bloqueia Cas9; placeholder vermelho/DANO | 80 × 224 | Sim |
| `dna/trecho_reparado` | Trecho após sucesso no puzzle, sem colisão; placeholder verde/OK | 80 × 224 | Sim |
| `fundos/fundo_puzzle` | Fundo do pareamento e da pergunta | 1280 × 720 | Sim |
| `fundos/fundo_fase_01` … `fundos/fundo_fase_10` | Fundo fixo correspondente à fase selecionada | 1280 × 720 | Sim |

Os botões do menu usam Assets e StyleBoxTexture, com repetição ajustada ao retângulo.
Na ausência de cada imagem, usam o tema do Godot como placeholder.
O contorno de foco e os textos continuam visíveis.
Os botões e ícones do seletor usam VisualSlot, mantendo proporção e os ajustes
de escala/offset do JSON. Os mesmos ids de botão são compartilhados; prefira
arte simples que funcione nos diferentes formatos. O marcador `>` indica
foco por teclado mesmo quando há imagens personalizadas.
Na fase, as letras A/T/G/C permanecem sobre as imagens para facilitar a
leitura dos pares. A animação de Cas9 pausa ao parar, inclusive nas extremidades.
O botão Voltar ao seletor usa os ids de botão existentes em 260 × 56.
O botão Sair do puzzle reutiliza `ui/botao_normal` e `ui/botao_hover` em 280 × 56.
O puzzle de pergunta reutiliza esses ids nas alternativas (mínimo 1000 × 64)
e em Continuar (280 × 56). Após responder, as alternativas usam
`ui/botao_bloqueado`. Todos esses botões mantêm o placeholder via VisualSlot.
As bases do pareamento usam os mesmos ids da fita em VisualSlots de 64 × 64,
dentro de botões de 96 × 72. Colchetes indicam seleção, `>` indica foco/mouse
e OK identifica uma base pareada. As letras continuam visíveis sobre a arte.
Trechos danificados/reparados e o fundo do puzzle usam VisualSlot; os ids
podem receber overrides em `data/visuais.json`. A colisão do dano permanece
80 × 224 independentemente da imagem ou de seus ajustes visuais.

## Convenções para os próximos marcos

Estes arquivos ainda não aparecem no jogo. Todos são opcionais.

| Id / nomes esperados | Uso previsto | Tamanho recomendado |
| --- | --- | --- |
| `heredograma/homem`, `heredograma/mulher`, `heredograma/afetado` | Peças | 64 × 64 |
| `heredograma/linha_uniao`, `heredograma/linha_filiacao` | Ligações | 64 × 64 |

## Áudio

Ids relativos a `assets/audio/`, sem prefixo `audio/`.
`Assets.som("sfx/acerto")` procura `assets/audio/sfx/acerto.ogg`,
depois `.wav` e `.mp3`, nessa ordem. Nenhum som é reproduzido neste marco.

| Ids esperados | Uso previsto | Opcional |
| --- | --- | --- |
| `sfx/acerto`, `sfx/erro`, `sfx/reparo`, `sfx/clique` | Efeitos | Sim |
| `musica/menu`, `musica/fase`, `musica/puzzle` | Trilhas | Sim |

## Nomes alternativos e ajustes

`data/visuais.json` começa vazio (`{}`). Exemplo:

```json
{
  "fundos/fundo_menu": {
    "arquivo": "fundos/minha_imagem.png",
    "escala": 1.0,
    "offset": [0, 0]
  },
  "player/cas9": {
    "arquivo": "player/meu_sprite.png",
    "escala": 1.5,
    "offset": [0, -8]
  }
}
```

`arquivo` é relativo a `assets/sprites/`. Sem override, o caminho é
`assets/sprites/<id>.png`. VisualSlot centraliza a imagem e preserva a
proporção no espaço disponível, usando `TamanhoPadrao` como tamanho mínimo.
`escala` multiplica esse ajuste; `offset` desloca a imagem em pixels.
Escala maior que 1 pode ultrapassar o slot. Esses ajustes geométricos são
aplicados por VisualSlot; estilos de botão usam apenas `arquivo`.
JSON inválido ou ajustes inválidos voltam aos valores padrão.

## VisualSlot e animações

Instancie `scenes/componentes/visual_slot.tscn` e configure `IdVisual`,
`TamanhoPadrao` e `CorPlaceholder`. `TextoPlaceholder` permite exibir
uma letra, como A, T, G ou C, quando a imagem não existe.
O filtro padrão é Nearest, apropriado para pixel art.

O componente procura `<IdVisual>_1.png`, `_2.png`, etc. Para `player/cas9`,
procura automaticamente `player/cas9_andando_1.png`, `_2.png`, etc.
O campo opcional `IdAnimacao` permite escolher outro prefixo.
A sequência começa em 1 e termina no primeiro quadro ausente, sem lacunas.
Os quadros usam AnimatedSprite2D a 8 FPS, em loop; um único quadro fica
estático. Sem sequência, usa a imagem estática; sem imagem, o placeholder.
Os ajustes geométricos da animação vêm do id principal do slot.

## Testar e recarregar

1. Abra o projeto no Godot e pressione F5: o menu funciona sem imagens.
2. Adicione `assets/sprites/fundos/fundo_menu.png` e deixe o editor importar.
3. Execute novamente ou chame `GetNode<Assets>("/root/Assets").Recarregar()` em código durante a
   execução. O método limpa os caches, relê o JSON e emite
   `AssetsRecarregados`; slots e botões atualizam automaticamente.
4. Experimente um `arquivo` alternativo, escala e offset no JSON.

Recarregar depende de o Godot já ter importado os arquivos novos/alterados.
Uma exportação não importa arquivos soltos adicionados depois: inclua os
assets antes de exportar. Inclua `data/*.json` no filtro de arquivos não
recurso da exportação.
Os `.gitkeep` mantêm as pastas vazias no Git e podem permanecer junto da arte.
