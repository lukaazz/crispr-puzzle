# Assets do CRISPR Puzzle

Todos os arquivos são opcionais. Sem imagens, o jogo usa placeholders; sem
sons, `Assets.som()` retorna `null`. Use nomes em snake_case, sem acentos,
e PNG com transparência quando apropriado. Os tamanhos são sugestões.

## Imagens usadas neste marco

Caminhos relativos a `assets/sprites/`. O id não inclui `.png`.

| Id | Onde aparece | Tamanho recomendado | Opcional |
| --- | --- | --- | --- |
| `fundos/fundo_menu` | Fundo do menu, via VisualSlot | 1280 × 720 | Sim |
| `ui/botao_normal` | Jogar e Sair, estado normal | 520 × 60 | Sim |
| `ui/botao_hover` | Jogar e Sair, mouse sobre ou pressionado | 520 × 60 | Sim |
| `ui/botao_bloqueado` | Estado desabilitado dos botões (reservado) | 520 × 60 | Sim |

Os botões usam Assets e StyleBoxTexture, com repetição ajustada ao retângulo.
Na ausência de cada imagem, usam o tema do Godot como placeholder.
O contorno de foco e os textos continuam visíveis.

## Convenções para os próximos marcos

Estes arquivos ainda não aparecem no jogo. Todos são opcionais.

| Id / nomes esperados | Uso previsto | Tamanho recomendado |
| --- | --- | --- |
| `player/cas9` | Personagem | 96 × 96 |
| `player/cas9_andando_1` … `player/cas9_andando_N` | Animação | 96 × 96 por quadro |
| `dna/base_a`, `dna/base_t`, `dna/base_g`, `dna/base_c` | Bases nitrogenadas | 64 × 64 |
| `dna/trecho_saudavel`, `dna/trecho_danificado`, `dna/trecho_reparado` | Caminho | 256 × 96 |
| `heredograma/homem`, `heredograma/mulher`, `heredograma/afetado` | Peças | 64 × 64 |
| `heredograma/linha_uniao`, `heredograma/linha_filiacao` | Ligações | 64 × 64 |
| `fundos/fundo_seletor` | Seletor de fases | 1280 × 720 |
| `fundos/fundo_fase_01` … `fundos/fundo_fase_10` | Fases 1 a 10 | 1280 × 720 |
| `fundos/fundo_puzzle` | Puzzles | 1280 × 720 |
| `ui/icone_fase_concluida`, `ui/icone_cadeado` | Indicadores | 48 × 48 |

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
proporção no espaço disponível, usando `tamanho_padrao` como tamanho mínimo.
`escala` multiplica esse ajuste; `offset` desloca a imagem em pixels.
Escala maior que 1 pode ultrapassar o slot. Esses ajustes geométricos são
aplicados por VisualSlot; estilos de botão usam apenas `arquivo`.
JSON inválido ou ajustes inválidos voltam aos valores padrão.

## VisualSlot e animações

Instancie `scenes/componentes/visual_slot.tscn` e configure `id_visual`,
`tamanho_padrao` e `cor_placeholder`. `texto_placeholder` permite exibir
uma letra, como A, T, G ou C, quando a imagem não existe.
O filtro padrão é Nearest, apropriado para pixel art.

O componente procura `<id_visual>_1.png`, `_2.png`, etc. Para `player/cas9`,
procura automaticamente `player/cas9_andando_1.png`, `_2.png`, etc.
O campo opcional `id_animacao` permite escolher outro prefixo.
A sequência começa em 1 e termina no primeiro quadro ausente, sem lacunas.
Os quadros usam AnimatedSprite2D a 8 FPS, em loop; um único quadro fica
estático. Sem sequência, usa a imagem estática; sem imagem, o placeholder.
Os ajustes geométricos da animação vêm do id principal do slot.

## Testar e recarregar

1. Abra o projeto no Godot e pressione F5: o menu funciona sem imagens.
2. Adicione `assets/sprites/fundos/fundo_menu.png` e deixe o editor importar.
3. Execute novamente ou chame `Assets.recarregar()` em código durante a
   execução. O método limpa os caches, relê o JSON e emite
   `assets_recarregados`; slots e botões atualizam automaticamente.
4. Experimente um `arquivo` alternativo, escala e offset no JSON.

Recarregar depende de o Godot já ter importado os arquivos novos/alterados.
Uma exportação não importa arquivos soltos adicionados depois: inclua os
assets antes de exportar. Inclua `data/*.json` no filtro de arquivos não
recurso da exportação.
Os `.gitkeep` mantêm as pastas vazias no Git e podem permanecer junto da arte.
