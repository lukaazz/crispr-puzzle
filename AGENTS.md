    # Projeto: crispr-puzzle
    Jogo 2D educativo em Godot 4.x (GDScript), feito principalmente com IA.
    Simula de forma lúdica o funcionamento da técnica CRISPR-Cas9.

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
    Perguntas ficam em arquivo de dados (data/perguntas.json), nunca no código.

    A dificuldade das fases controla o tipo de puzzle sorteado e seus parâmetros
    (mais pares, heredogramas maiores, perguntas mais difíceis, menos tolerância a erros).

    ## Estrutura de pastas
    - scenes/ (arquivos .tscn): menu, seletor de fases, fases, puzzles, player
    - scripts/ (arquivos .gd): um script por cena, mesmo nome da cena
    - scripts/autoload/ (singletons): GameState (progresso) e SceneRouter (troca de telas)
    - data/ (arquivos .json): perguntas, configuração das 10 fases
    - assets/sprites/ e assets/audio/

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
    - Nomes de arquivos, nós e variáveis em português sem acentos (ex.: fase_selector
    só se for necessário para clareza; prefira seletor_fases, trecho_danificado).
    - Prefira signals a referências diretas entre nós.
    - Use Input Map (ações como "mover_esq", "mover_dir", "interagir", "pausar"),
    nunca teclas fixas no código.
    - Resolução base 1280x720, stretch mode "canvas_items".
    - Sem assets externos por enquanto: use ColorRect, Polygon2D e Label como
    placeholders (bases nitrogenadas com cores distintas e letra A, T, G, C visível).
    - Não altere project.godot sem avisar o que mudou e por quê.
    - Não mexa na pasta .godot/.
    - Evite números mágicos: use const ou arquivos de dados.

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

    ## Ao finalizar cada tarefa
    - Resuma em poucas linhas o que mudou e como testar (F5 no Godot).
    - Liste arquivos criados ou alterados.
    - Trabalhe um marco por vez. Não implemente funcionalidades que não foram pedidas.