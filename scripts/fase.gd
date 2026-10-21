extends Node2D

const TipoCas9 = preload("res://scripts/cas9.gd")
const TipoTrecho = preload("res://scripts/trecho_danificado.gd")
const CENA_TRECHO: PackedScene = preload("res://scenes/trecho_danificado.tscn")
const CENAS_PUZZLE: Dictionary = {
	"pareamento_bases": preload("res://scenes/puzzles/pareamento_bases.tscn"),
	"pergunta": preload("res://scenes/puzzles/pergunta.tscn"),
}
const CENA_VISUAL: PackedScene = preload("res://scenes/componentes/visual_slot.tscn")
const CENA_MARCADOR: PackedScene = preload("res://scenes/marcador_fim.tscn")
const ARQUIVO_FASES: String = "res://data/fases.json"
const COMPLEMENTOS: Dictionary = {"A": "T", "T": "A", "G": "C", "C": "G"}
const CORES: Dictionary = {
	"A": Color(0.15, 0.6, 0.35), "T": Color(0.75, 0.32, 0.18),
	"G": Color(0.7, 0.53, 0.08), "C": Color(0.15, 0.4, 0.8),
}
const MARGEM: float = 160.0
const PASSO_PAR: float = 80.0
const ALTURA_CAMINHO: float = 360.0
const TAMANHO_BASE: Vector2 = Vector2(64, 64)
const DESLOCAMENTO_BASE: float = 80.0
const TAMANHO_TRECHO: Vector2 = Vector2(80, 224)
const LARGURA_TELA: int = 1280
const DISTANCIA_INTERACAO: float = 120.0

var _trechos: Array[TipoTrecho] = []
var _trecho_em_reparo: TipoTrecho = null
var _puzzle: CanvasLayer = null
var _tipos_puzzle: Array[String] = []
var _dificuldade: int = 1
var _conclusao_iniciada: bool = false
var _marcador: Area2D = null

@onready var fita: Node2D = $fita
@onready var jogador: TipoCas9 = $cas9
@onready var camera: Camera2D = $cas9/camera
@onready var fundo: Control = $fundo/tela
@onready var titulo: Label = $interface/conteudo/titulo
@onready var aviso: Label = $interface/conteudo/aviso


func _ready() -> void:
	$interface/conteudo/voltar.pressed.connect(_voltar_ao_seletor)
	var configuracao: Dictionary = _carregar_configuracao()
	if configuracao.is_empty():
		jogador.set_physics_process(false)
		aviso.text = "Configuracao da fase indisponivel. Volte ao seletor."
		return
	var sequencia: String = str(configuracao["sequencia"])
	var quantidade: int = int(configuracao["quantidade_pares"])
	_dificuldade = int(configuracao.get("dificuldade", 1))
	for tipo: String in configuracao.get("tipos_puzzle", []):
		if CENAS_PUZZLE.has(tipo):
			_tipos_puzzle.append(tipo)
	if _tipos_puzzle.is_empty():
		_tipos_puzzle.append("pareamento_bases")
	var quantidade_danos: int = clampi(int(configuracao.get("trechos_danificados", 0)), 0, quantidade - 2)
	var indices_danos: Array[int] = []
	for dano: int in range(quantidade_danos):
		indices_danos.append(roundi(float((dano + 1) * (quantidade - 1)) / (quantidade_danos + 1)))
	titulo.text = "Fase %d — Explore a fita de DNA" % get_node("/root/GameState").FaseAtual
	fundo.IdVisual = "fundos/fundo_fase_%02d" % get_node("/root/GameState").FaseAtual
	for indice: int in range(quantidade):
		var x: float = MARGEM + indice * PASSO_PAR
		var base: String = sequencia.substr(indice % sequencia.length(), 1)
		if indices_danos.has(indice):
			var trecho: TipoTrecho = CENA_TRECHO.instantiate() as TipoTrecho
			trecho.position = Vector2(x, ALTURA_CAMINHO)
			fita.add_child(trecho)
			_trechos.append(trecho)
		else:
			_criar_visual("dna/trecho_saudavel", Vector2(x, ALTURA_CAMINHO), TAMANHO_TRECHO, Color(0.07, 0.19, 0.24))
		_criar_base(base, Vector2(x, ALTURA_CAMINHO - DESLOCAMENTO_BASE))
		_criar_base(COMPLEMENTOS[base], Vector2(x, ALTURA_CAMINHO + DESLOCAMENTO_BASE))
	var fim: float = MARGEM + (quantidade - 1) * PASSO_PAR
	_marcador = CENA_MARCADOR.instantiate() as Area2D
	_marcador.position = Vector2(fim, ALTURA_CAMINHO)
	_marcador.body_entered.connect(_ao_entrar_no_fim)
	add_child(_marcador)
	jogador.configurar_caminho(MARGEM, fim, ALTURA_CAMINHO)
	camera.limit_left = 0
	camera.limit_right = maxi(LARGURA_TELA, int(fim + MARGEM))
	camera.reset_smoothing()


func _process(_delta: float) -> void:
	if _puzzle != null or _conclusao_iniciada:
		return
	if _trecho_proximo() != null:
		var eventos: Array[InputEvent] = InputMap.action_get_events("interagir")
		var tecla: String = eventos[0].as_text() if not eventos.is_empty() else "interagir"
		aviso.text = "Trecho danificado. Use %s para abrir um puzzle." % tecla
	elif _marcador != null:
		aviso.text = "Caminho liberado! Alcance o marcador FIM para concluir a fase." if _todos_trechos_reparados() else "Repare todos os trechos antes de alcancar o marcador FIM."


func _todos_trechos_reparados() -> bool:
	for trecho: TipoTrecho in _trechos:
		if not trecho.reparado:
			return false
	return true


func _ao_entrar_no_fim(corpo: Node2D) -> void:
	if corpo != jogador or _puzzle != null or _conclusao_iniciada:
		return
	if not _todos_trechos_reparados():
		return
	_conclusao_iniciada = true
	jogador.definir_movimento_ativo(false)
	# A troca de cena ocorre fora do callback de fisica da Area2D.
	_concluir_fase.call_deferred()


func _concluir_fase() -> void:
	get_node("/root/GameState").ConcluirFase(get_node("/root/GameState").FaseAtual)
	if get_node("/root/SceneRouter").IrParaSeletorFases() != OK:
		aviso.text = "Fase concluida. Use Voltar ao seletor para tentar retornar."
		jogador.definir_movimento_ativo(true)


func _trecho_proximo() -> TipoTrecho:
	for trecho: TipoTrecho in _trechos:
		if not trecho.reparado and absf(jogador.position.x - trecho.position.x) <= DISTANCIA_INTERACAO:
			return trecho
	return null


func _unhandled_input(evento: InputEvent) -> void:
	if _puzzle != null or _conclusao_iniciada or not evento.is_action_pressed("interagir") or evento.is_echo():
		return
	var trecho: TipoTrecho = _trecho_proximo()
	if trecho == null:
		return
	get_viewport().set_input_as_handled()
	_trecho_em_reparo = trecho
	var tipo: String = _tipos_puzzle[randi_range(0, _tipos_puzzle.size() - 1)]
	var cena: PackedScene = CENAS_PUZZLE[tipo]
	_puzzle = cena.instantiate() as CanvasLayer
	_puzzle.call("setup", _dificuldade)
	_puzzle.connect("puzzle_concluido", _ao_puzzle_concluido)
	jogador.definir_movimento_ativo(false)
	$interface/conteudo.hide()
	add_child(_puzzle)


func _ao_puzzle_concluido(sucesso: bool) -> void:
	if _puzzle == null:
		return
	if sucesso and is_instance_valid(_trecho_em_reparo):
		_trecho_em_reparo.reparar()
	_puzzle.queue_free()
	_puzzle = null
	_trecho_em_reparo = null
	$interface/conteudo.show()
	jogador.definir_movimento_ativo(true)
	if _marcador != null and _marcador.overlaps_body(jogador):
		_ao_entrar_no_fim(jogador)


func _criar_visual(id: String, centro: Vector2, tamanho: Vector2, cor: Color) -> Control:
	var slot: Control = CENA_VISUAL.instantiate() as Control
	slot.IdVisual = id
	slot.TamanhoPadrao = tamanho
	slot.size = tamanho
	slot.position = centro - tamanho / 2.0
	slot.CorPlaceholder = cor
	fita.add_child(slot)
	return slot


func _criar_base(base: String, centro: Vector2) -> void:
	var slot: Control = _criar_visual("dna/base_" + base.to_lower(), centro, TAMANHO_BASE, CORES[base])
	# A letra permanece legivel mesmo com uma imagem personalizada.
	var letra: Label = Label.new()
	letra.name = "letra"
	letra.text = base
	letra.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	letra.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	letra.mouse_filter = Control.MOUSE_FILTER_IGNORE
	letra.add_theme_font_size_override("font_size", 28)
	letra.add_theme_color_override("font_outline_color", Color.BLACK)
	letra.add_theme_constant_override("outline_size", 6)
	slot.add_child(letra)
	letra.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)


func _carregar_configuracao() -> Dictionary:
	if not FileAccess.file_exists(ARQUIVO_FASES):
		return {}
	var analisador: JSON = JSON.new()
	if analisador.parse(FileAccess.get_file_as_string(ARQUIVO_FASES)) != OK:
		return {}
	var dados: Variant = analisador.data
	if not dados is Array:
		return {}
	for entrada: Variant in dados:
		if not entrada is Dictionary or int(entrada.get("numero", 0)) != get_node("/root/GameState").FaseAtual:
			continue
		var sequencia: String = str(entrada.get("sequencia", ""))
		if sequencia.is_empty() or int(entrada.get("quantidade_pares", 0)) < 2:
			return {}
		for letra: String in sequencia:
			if not COMPLEMENTOS.has(letra):
				return {}
		return entrada
	return {}


func _voltar_ao_seletor() -> void:
	if get_node("/root/SceneRouter").IrParaSeletorFases() != OK:
		aviso.text = "Nao foi possivel abrir o seletor."
