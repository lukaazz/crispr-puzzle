extends Node2D

const TipoVisualSlot = preload("res://scripts/componentes/visual_slot.gd")
const TipoCas9 = preload("res://scripts/cas9.gd")
const CENA_VISUAL: PackedScene = preload("res://scenes/componentes/visual_slot.tscn")
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

@onready var fita: Node2D = $fita
@onready var jogador: TipoCas9 = $cas9
@onready var camera: Camera2D = $cas9/camera
@onready var fundo: TipoVisualSlot = $fundo/tela
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
	titulo.text = "Fase %d — Explore a fita de DNA" % GameState.fase_atual
	fundo.id_visual = "fundos/fundo_fase_%02d" % GameState.fase_atual
	for indice: int in range(quantidade):
		var x: float = MARGEM + indice * PASSO_PAR
		var base: String = sequencia.substr(indice % sequencia.length(), 1)
		_criar_visual("dna/trecho_saudavel", Vector2(x, ALTURA_CAMINHO), TAMANHO_TRECHO, Color(0.07, 0.19, 0.24))
		_criar_base(base, Vector2(x, ALTURA_CAMINHO - DESLOCAMENTO_BASE))
		_criar_base(COMPLEMENTOS[base], Vector2(x, ALTURA_CAMINHO + DESLOCAMENTO_BASE))
	var fim: float = MARGEM + (quantidade - 1) * PASSO_PAR
	jogador.configurar_caminho(MARGEM, fim, ALTURA_CAMINHO)
	camera.limit_left = 0
	camera.limit_right = maxi(LARGURA_TELA, int(fim + MARGEM))
	camera.reset_smoothing()


func _criar_visual(id: String, centro: Vector2, tamanho: Vector2, cor: Color) -> TipoVisualSlot:
	var slot: TipoVisualSlot = CENA_VISUAL.instantiate() as TipoVisualSlot
	slot.id_visual = id
	slot.tamanho_padrao = tamanho
	slot.size = tamanho
	slot.position = centro - tamanho / 2.0
	slot.cor_placeholder = cor
	fita.add_child(slot)
	return slot


func _criar_base(base: String, centro: Vector2) -> void:
	var slot: TipoVisualSlot = _criar_visual("dna/base_" + base.to_lower(), centro, TAMANHO_BASE, CORES[base])
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
		if not entrada is Dictionary or int(entrada.get("numero", 0)) != GameState.fase_atual:
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
	if SceneRouter.ir_para_seletor_fases() != OK:
		aviso.text = "Nao foi possivel abrir o seletor."
