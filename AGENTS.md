    # Projeto: crispr-puzzle
    Jogo 2D educativo em Godot 4.x (GDScript).
    Simula de forma lúdica o funcionamento da técnica CRISPR-Cas9.
    O repositório remoto já existe no GitHub (remote "origin").

    ## O jogo
    O jogador controla o CRISPR (Cas9) e caminha ao longo de uma fita de DNA.
    O caminho é feito do próprio DNA, e trechos danificados bloqueiam a passagem.
    Para avançar, o jogador deve "consertar" o trecho, o que abre um puzzle.
    Resolver o puzzle repara o DNA e libera o caminho até o fim da fase.

    - 10 fases, com dificuldade progressiva.
    - Fase 1 liberada no início. Concluir uma fase libera a seguinte.
    - Fluxo de telas: MENU -> SELETOR DE FASES -> FASE -> PUZZLE -> SELETOR DE FASES
    (ao terminar uma fase, volta ao seletor; o puzzle retorna à fase de origem).

    ## Tipos de puzzle
    1. **Pareamento de bases:** ligar pares complementares.
    Guanina (G) com Citosina (C) e Adenina (A) com Timina (T).
    Os pares sorteados variam a cada tentativa.
    2. **Heredograma:** montar um heredograma coerente a partir de peças soltas
    (quadrado = homem, círculo = mulher, preenchido = afetado, linhas de união
    e de filiação). Cada heredograma deve ter solução válida e verificável.
    3. **Pergunta:** questão de múltipla escolha sobre biologia molecular e genética.
    Perguntas ficam em data/perguntas.json, nunca no código.

    A dificuldade das fases controla o tipo de puzzle sorteado e seus parâmetros
    (mais pares, heredogramas maiores, perguntas mais difíceis, menos tolerância a erros).

    ## Estrutura de pastas
    - scenes/ (.tscn): menu, seletor de fases, fases, puzzles, player, componentes
    - scripts/ (.gd): um script por cena, mesmo nome da cena
    - scripts/autoload/: GameState (progresso), SceneRouter (troca de telas),
    Assets (carregamento de imagens e sons)
    - data/ (.json): perguntas, heredogramas, configuração das 10 fases, visuais
    - assets/sprites/<categoria>/ e assets/audio/{sfx,musica}/
    - assets/LEIA-ME.md: lista de todos os nomes de arquivo esperados (ver abaixo)
    - project.godot fica na raiz do repositório.

    ## Sistema de assets (REGRA CENTRAL)
    O jogo deve funcionar sem nenhuma imagem e melhorar sozinho quando eu
    colocar arquivos nas pastas. Nunca escreva caminhos de imagem fixos nas cenas.

    - **Autoload Assets** com:
    - textura(id: String) -> Texture2D: procura res://assets/sprites/<id>.png
        (o id inclui a categoria, ex.: "dna/base_a"). Se existir, retorna a textura;
        se não, retorna null. Use ResourceLoader.exists() antes de carregar.
    - som(id: String) -> AudioStream: procura em res://assets/audio/<id>.ogg
        (ou .wav / .mp3), com a mesma lógica de retornar null se não existir.
    - Cache interno, para não recarregar a mesma imagem.
    - Recarregar sob demanda (método recarregar()) para eu testar sem reiniciar.
    - **Componente reutilizável cena VisualSlot** (scenes/componentes/visual_slot.tscn):
    - Exporta id_visual: String, tamanho_padrao: Vector2 e cor_placeholder: Color.
    - Se Assets.textura(id_visual) existir, mostra um Sprite2D/TextureRect
        com a imagem, ajustada ao tamanho_padrao (manter proporção).
    - Se não existir, mostra um ColorRect/Polygon2D com a cor do placeholder
        e, quando útil, um Label (ex.: a letra "A" na base nitrogenada).
    - Todo elemento visual do jogo (player, bases, trechos, peças de heredograma,
        fundos, ícones, botões) usa VisualSlot ou o mesmo mecanismo. Sem exceção.
    - **Override opcional em data/visuais.json:** mapeia um id para outro arquivo,
    escala e deslocamento, para eu usar nomes de arquivo diferentes ou ajustar
    a imagem sem mexer no código. Formato:
    { "player/cas9": { "arquivo": "player/meu_sprite.png", "escala": 1.5,
        "offset": [0, -8] } }
    Sem entrada no JSON, vale a convenção id = caminho do arquivo.
    - **Convenção de nomes (snake_case, sem acentos, PNG com fundo transparente):**
    - player/cas9, player/cas9_andando_1..N (animação opcional por quadros)
    - dna/base_a, base_t, base_g, base_c
    - dna/trecho_saudavel, trecho_danificado, trecho_reparado
    - heredograma/homem, mulher, afetado, linha_uniao, linha_filiacao
    - fundos/fundo_menu, fundo_seletor, fundo_fase_01 ... fundo_fase_10, fundo_puzzle
    - ui/botao_normal, botao_hover, botao_bloqueado, icone_fase_concluida, icone_cadeado
    - audio/sfx/acerto, erro, reparo, clique; audio/musica/menu, fase, puzzle
    - **Animações:** se existir uma sequência numerada (cas9_andando_1, _2, ...),
    monte um AnimatedSprite2D automaticamente. Se só existir uma imagem,
    use estática. Sem imagem, use o placeholder.
    - **Tamanho padrão:** cada VisualSlot declara seu tamanho em pixels; qualquer
    imagem que eu colocar é escalada para caber, sem distorcer a proporção.
    Não force um tamanho de arquivo.
    - **Filtro de textura:** use "Nearest" por padrão (pixel art). Ajuste no
    project.godot (default_texture_filter) se eu disser que a arte é suave.
    - **assets/LEIA-ME.md:** mantenha atualizado com a tabela de todos os ids
    usados no jogo, onde aparecem, tamanho recomendado e se é opcional.
    Sempre que criar um novo id, adicione uma linha. Esse arquivo é o meu guia
    de quais imagens posso fornecer.
    - Ao adicionar um novo elemento visual ao jogo, crie o id, o placeholder
    e a linha no LEIA-ME. Nunca dependa de uma imagem existir.

    ## Arquitetura (siga esta divisão)
    - **SceneRouter (autoload):** única forma de trocar de tela. Nenhuma cena chama
    get_tree().change_scene_to_file() diretamente.
    - **GameState (autoload):** fase atual, fases liberadas, fases concluídas.
    Salva e carrega em user://save.cfg usando ConfigFile.
    - **Fases configuradas por dados:** cada fase lê sua configuração de
    data/fases.json (número, dificuldade, tipos de puzzle permitidos, quantidade
    de trechos danificados). Não crie uma cena duplicada por fase sem necessidade.
    - **Puzzles com interface comum:** todo puzzle é uma cena que recebe a
    dificuldade (método setup(dificuldade: int)) e emite o signal
    puzzle_concluido(sucesso: bool). Quem abre o puzzle é a fase, e não o contrário.
    - **Player:** CharacterBody2D que anda ao longo do DNA. Ao tocar um trecho
    danificado, a fase abre o puzzle correspondente.

    ## Regras de código
    - Use apenas GDScript tipado (var x: int, func f() -> void).
    - Nomes de arquivos, nós e variáveis em português, sem acentos, em snake_case.
    - Prefira signals a referências diretas entre nós.
    - Use Input Map (ações como "mover_esq", "mover_dir", "interagir", "pausar"),
    nunca teclas fixas no código.
    - Resolução base 1280x720, stretch mode "canvas_items".
    - Não altere project.godot sem avisar o que mudou e por quê.
    - Não mexa na pasta .godot/.
    - Evite números mágicos: use const ou arquivos de dados.
    - Não apague nem renomeie arquivos dentro de assets/ (são meus).

    ## Fidelidade científica
    - Conteúdo de biologia deve ser correto: A-T e G-C; o Cas9 é guiado por um
    RNA guia, corta o DNA no local-alvo e a célula repara o corte.
    - Ao escrever perguntas ou explicações, mantenha linguagem adequada ao ensino
    médio. Na dúvida sobre um fato, sinalize para revisão em vez de inventar.

    ## Validação
    Depois de mudar código, rode:
    godot --headless --path . --quit
    e corrija erros de parse antes de terminar.
    Se o comando "godot" não existir no PATH, avise e pule este passo.

    ## Controle de versão (Git e GitHub)
    O repositório está conectado ao GitHub (origin), branch main.

    - Cada alteração concluída vira um commit próprio, seguido de push.
    - Antes de começar uma tarefa: git status. Se houver alterações minhas
    não commitadas (inclusive imagens novas em assets/), avise antes de continuar.
    - Fluxo ao terminar uma mudança:
    1. Rode a validação (se disponível). Se houver erro de parse, corrija
        antes de commitar.
    2. git status, para conferir o que mudou.
    3. git add somente dos arquivos alterados por você nesta tarefa
        (nunca adicione .godot/ nem credenciais).
    4. git commit -m "tipo: descrição curta em português"
    5. git push
    - Prefixos: feat, fix, refactor, data, assets (imagens e sons), docs.
    - Se a tarefa tiver várias partes independentes, um commit e um push por parte.
    - Se o push falhar, pare, mostre o erro e me pergunte. Não resolva à força.
    - Nunca use git push --force, git reset --hard, nem apague branches ou
    histórico sem eu pedir. Não troque de branch sem eu pedir.
    - Ao final, informe o hash e a mensagem de cada commit e se o push concluiu.

    ## Ao finalizar cada tarefa
    - Resuma em poucas linhas o que mudou e como testar (F5 no Godot).
    - Liste arquivos criados ou alterados e os commits enviados.
    - Trabalhe um marco por vez. Não implemente funcionalidades que não foram pedidas.