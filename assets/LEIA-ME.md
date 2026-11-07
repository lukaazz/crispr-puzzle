# Assets do CRISPR Puzzle

Todos os arquivos são opcionais. Sem imagens, o jogo usa placeholders; sem
sons, `Assets.som()` retorna `null`. Use nomes em snake_case, sem acentos,
e PNG com transparência quando apropriado. Os tamanhos são sugestões.

## Imagens usadas neste marco

Caminhos relativos a `assets/sprites/`. O id não inclui `.png`.

| Id | Onde aparece | Tamanho recomendado | Opcional |
| --- | --- | --- | --- |
| `fundos/fundo_menu` | Fundo do menu, via VisualSlot | 1280 × 720 | Sim |
| `fundos/fundo_pausa` | Sobreposicao do menu de pausa, via VisualSlot | 1280 × 720 | Sim |
| `fundos/fundo_final` | Tela de conclusao das dez fases, via VisualSlot | 1280 × 720 | Sim |
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
Os menus de pausa e tela final reutilizam os ids de botao em 600 x 60,
com VisualSlot e rotulos permanentes. Seus fundos sao opcionais.
O puzzle de pergunta reutiliza esses ids nas alternativas (mínimo 1000 × 64)
e em Continuar (280 × 56). Após responder, as alternativas usam
`ui/botao_bloqueado`. Todos esses botões mantêm o placeholder via VisualSlot.
As bases do pareamento usam os mesmos ids da fita em VisualSlots de 64 × 64,
dentro de botões de 96 × 72. Colchetes indicam seleção, `>` indica foco/mouse
e OK identifica uma base pareada. As letras continuam visíveis sobre a arte.
Trechos danificados/reparados e o fundo do puzzle usam VisualSlot; os ids
podem receber overrides em `data/visuais.json`. A colisão do dano permanece
80 × 224 independentemente da imagem ou de seus ajustes visuais.

## Heredograma (implementado)

Todos os ids abaixo usam VisualSlot e sao opcionais. Sem arte, os simbolos
sao desenhados como quadrados, circulos, preenchimentos e linhas.

| Id | Onde aparece | Tamanho recomendado | Opcional |
| --- | --- | --- | --- |
| `heredograma/homem` | Pessoa masculina, contorno quadrado | 56 x 56 | Sim |
| `heredograma/mulher` | Pessoa feminina, contorno circular | 56 x 56 | Sim |
| `heredograma/afetado` | Sobreposicao nas pessoas afetadas | 56 x 56 | Sim |
| `heredograma/linha_uniao` | Uniao dos pais e barra de irmaos | 184 x 16 (ate 600 x 16) | Sim |
| `heredograma/linha_filiacao` | Descendencia vertical | 16 x 148 | Sim |

`afetado` deve ser uma marca central transparente que funcione sobre ambos
os sexos sem esconder seu contorno (por exemplo, um disco central). Sem essa
imagem, o preenchimento acompanha exatamente o quadrado ou circulo.
As pecas da bandeja medem 48 x 48; pessoas no diagrama medem 56 x 56.
A bandeja tem cinco colunas para acomodar os oito modelos (ate 21 pecas).
Ao encaixar, a imagem cabe no retangulo
reservado mantendo proporcao. Linhas devem ser centradas na arte.
O fundo reutiliza `fundos/fundo_puzzle`; Verificar, Continuar e Sair reutilizam
os ids de botao existentes em 280 x 56.
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


## Assets isometricos analisados (etapa 1)

Arquivos de preparacao, ainda nao consumidos pelas cenas. Originais preservados.
Reproduzir: `python tools/analisar_assets.py` (Pillow; dependencias em `tools/requirements.txt`).
Topo 48 x 24 px com contorno, lateral 15 px; total 39 px sem mureta.
O sheet tem 6 quadros de 32 x 32 px em uma linha; FPS nao informado pela imagem.
A integracao do sheet ao jogo fica para outra etapa; a animacao existente usa quadros numerados.

| Id / arquivo | Uso | Tamanho nativo recomendado | Opcional no jogo |
| --- | --- | --- | --- |
| `mapa/tileset_full3` | Atlas original, fonte dos recortes | 768 x 1008 | Sim |
| `mapa/mapa_referencia` | Referencia visual da grade | 1024 x 768 | Sim |
| `player/cas9_andando_sheet` | Sheet analisado, integracao futura | 192 x 32 (6 de 32 x 32) | Sim |
| `mapa/tiles/tile_001` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_002` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_003` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_004` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_005` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_006` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_007` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_008` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_009` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_010` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_011` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_012` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_013` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_014` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_015` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_016` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_017` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_018` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_019` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_020` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_021` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_022` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_023` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_024` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_025` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_026` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_027` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_028` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_029` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_030` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_031` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_032` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_033` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_034` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_035` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_036` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_037` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_038` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_039` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_040` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_041` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_042` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_043` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_044` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_045` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_046` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_047` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_048` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_049` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_050` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_051` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_052` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_053` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_054` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_055` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_056` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_057` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_058` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_059` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_060` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_061` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_062` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_063` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_064` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_065` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_066` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_067` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_068` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_069` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_070` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_071` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_072` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_073` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_074` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_075` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_076` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_077` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_078` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_079` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_080` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_081` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_082` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_083` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_084` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_085` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_086` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_087` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_088` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_089` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_090` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_091` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_092` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_093` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_094` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_095` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_096` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_097` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_098` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_099` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_100` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_101` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_102` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_103` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_104` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_105` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_106` | Componente extraido do atlas | 48 x 45 | Sim |
| `mapa/tiles/tile_107` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_108` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_109` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_110` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_111` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_112` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_113` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_114` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_115` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_116` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_117` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_118` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_119` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_120` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_121` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_122` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_123` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_124` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_125` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_126` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_127` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_128` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_129` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_130` | Componente extraido do atlas | 48 x 39 | Sim |
| `mapa/tiles/tile_131` | Componente extraido do atlas | 24 x 31 | Sim |
| `mapa/tiles/tile_132` | Componente extraido do atlas | 18 x 15 | Sim |
| `mapa/tiles/tile_133` | Componente extraido do atlas | 6 x 7 | Sim |
| `mapa/tiles/tile_134` | Componente extraido do atlas | 24 x 24 | Sim |

`mapa/tiles/_indice.png` (800 x 1064): contato numerado, somente para consulta.
`mapa/tiles/_manifesto.json`: caixas [x0,y0,x1,y1), tamanhos e quantidade de pixels visiveis.
`mapa/tiles/_preview_extracao.png` (1024 x 768): ids das 147 celulas observadas na referencia.
Os indices seguem y minimo, depois x minimo, e incluem bandeira e objetos pequenos.
Nenhum componente visivel foi descartado por tamanho; recortes preservam os pixels de alpha > 0.
Medidas e tiles por cor estao em `data/isometrico.json`. A grade esparsa em
`data/mapas/fase_01_extraido.json` preserva as seis cores, os desalinhamentos
e os topos parciais; nao completa lacunas nem define spawn ou defeitos.
