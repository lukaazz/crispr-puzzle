extends Control

const ANIMACAO: StringName = &"padrao"
const QUADROS_POR_SEGUNDO: float = 8.0

@export var id_visual: String = "":
	set(valor):
		id_visual = valor
		if is_node_ready():
			_atualizar_visual()
@export var tamanho_padrao: Vector2 = Vector2(64, 64)
@export var cor_placeholder: Color = Color(0.3, 0.5, 0.6, 1.0)
@export var texto_placeholder: String = ""
## Vazio procura <id_visual>_1, _2...; Cas9 tambem procura cas9_andando_1...
@export var id_animacao: String = ""

var _animacao_ativa: bool = true

@onready var placeholder: ColorRect = $placeholder
@onready var legenda: Label = $legenda
@onready var imagem: Sprite2D = $imagem
@onready var animacao: AnimatedSprite2D = $animacao


func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_IGNORE
	custom_minimum_size = tamanho_padrao
	get_node("/root/Assets").AssetsRecarregados.connect(_atualizar_visual)
	resized.connect(_ajustar_imagem)
	animacao.frame_changed.connect(_ajustar_imagem)
	_atualizar_visual()


func _atualizar_visual() -> void:
	animacao.stop()
	animacao.sprite_frames = null
	imagem.texture = get_node("/root/Assets").Textura(id_visual)
	var prefixo: String = id_animacao
	if prefixo.is_empty():
		prefixo = id_visual + "_andando" if id_visual == "player/cas9" else id_visual
	var quadros: SpriteFrames = SpriteFrames.new()
	quadros.add_animation(ANIMACAO)
	quadros.set_animation_speed(ANIMACAO, QUADROS_POR_SEGUNDO)
	var indice: int = 1
	while not id_visual.is_empty():
		var quadro: Texture2D = get_node("/root/Assets").Textura("%s_%d" % [prefixo, indice])
		if quadro == null:
			break
		quadros.add_frame(ANIMACAO, quadro)
		indice += 1
	animacao.visible = quadros.get_frame_count(ANIMACAO) > 0
	if animacao.visible:
		animacao.sprite_frames = quadros
		if _animacao_ativa:
			animacao.play(ANIMACAO)
		else:
			animacao.animation = ANIMACAO
	imagem.visible = not animacao.visible and imagem.texture != null
	placeholder.color = cor_placeholder
	placeholder.visible = not imagem.visible and not animacao.visible
	legenda.text = texto_placeholder
	legenda.visible = placeholder.visible and not texto_placeholder.is_empty()
	_ajustar_imagem()


func definir_animacao_ativa(ativa: bool) -> void:
	if _animacao_ativa == ativa:
		return
	_animacao_ativa = ativa
	if animacao.sprite_frames == null:
		return
	if ativa:
		animacao.play(ANIMACAO)
	else:
		animacao.pause()


func _ajustar_imagem() -> void:
	var textura_atual: Texture2D = imagem.texture
	if animacao.visible and animacao.sprite_frames != null:
		textura_atual = animacao.sprite_frames.get_frame_texture(ANIMACAO, animacao.frame)
	if textura_atual == null:
		return
	var dimensoes: Vector2 = textura_atual.get_size()
	var ajuste: float = minf(size.x / dimensoes.x, size.y / dimensoes.y)
	var escala: Vector2 = Vector2.ONE * ajuste * get_node("/root/Assets").EscalaVisual(id_visual)
	var centro: Vector2 = size / 2.0 + get_node("/root/Assets").DeslocamentoVisual(id_visual)
	imagem.scale = escala
	imagem.position = centro
	animacao.scale = escala
	animacao.position = centro
