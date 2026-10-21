extends Button

const COR_NORMAL: Color = Color(0.08, 0.3, 0.38)
const COR_DESTAQUE: Color = Color(0.12, 0.5, 0.56)
const COR_BLOQUEADO: Color = Color(0.16, 0.19, 0.24)

@export var rotulo: String = "Voltar"

@onready var visual: Control = $visual
@onready var icone: Control = $icone
@onready var legenda: Label = $legenda


func _ready() -> void:
	for estado: String in ["normal", "hover", "pressed", "disabled", "focus"]:
		add_theme_stylebox_override(estado, StyleBoxEmpty.new())
	mouse_entered.connect(_atualizar_visual)
	mouse_exited.connect(_atualizar_visual)
	focus_entered.connect(_atualizar_visual)
	focus_exited.connect(_atualizar_visual)
	button_down.connect(_atualizar_visual)
	button_up.connect(_atualizar_visual)
	legenda.text = rotulo
	_atualizar_visual()


func configurar_fase(numero: int, liberada: bool, concluida: bool) -> void:
	disabled = not liberada
	rotulo = "Fase %d" % numero
	var estado: String = "Disponivel"
	if not liberada:
		estado = "Bloqueada"
	elif concluida:
		estado = "Concluida"
	legenda.text = "%s\n%s" % [rotulo, estado]
	tooltip_text = "%s: %s" % [rotulo, estado]
	icone.visible = not liberada or concluida
	icone.TextoPlaceholder = "X" if not liberada else "OK"
	icone.CorPlaceholder = COR_BLOQUEADO if not liberada else COR_DESTAQUE
	icone.IdVisual = "ui/icone_cadeado" if not liberada else "ui/icone_fase_concluida"
	_atualizar_visual()


func definir_desabilitado(valor: bool) -> void:
	disabled = valor
	_atualizar_visual()


func _atualizar_visual() -> void:
	if disabled:
		visual.CorPlaceholder = COR_BLOQUEADO
		visual.IdVisual = "ui/botao_bloqueado"
	elif is_hovered() or has_focus() or button_pressed:
		visual.CorPlaceholder = COR_DESTAQUE
		visual.IdVisual = "ui/botao_hover"
	else:
		visual.CorPlaceholder = COR_NORMAL
		visual.IdVisual = "ui/botao_normal"
	# Indicador de teclado permanece visivel mesmo com imagens personalizadas.
	$indicador_foco.visible = has_focus()
