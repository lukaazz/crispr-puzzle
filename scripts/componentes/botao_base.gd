extends Button

const TipoVisualSlot = preload("res://scripts/componentes/visual_slot.gd")
const CORES: Dictionary = {
	"A": Color(0.15, 0.6, 0.35), "T": Color(0.75, 0.32, 0.18),
	"G": Color(0.7, 0.53, 0.08), "C": Color(0.15, 0.4, 0.8),
}

@export var base: String = "A"
var selecionada: bool = false
var resolvida: bool = false

@onready var visual: TipoVisualSlot = $visual
@onready var letra: Label = $letra


func _ready() -> void:
	for estado: String in ["normal", "hover", "pressed", "disabled", "focus"]:
		add_theme_stylebox_override(estado, StyleBoxEmpty.new())
	visual.cor_placeholder = CORES[base]
	visual.id_visual = "dna/base_" + base.to_lower()
	focus_entered.connect(atualizar)
	focus_exited.connect(atualizar)
	mouse_entered.connect(atualizar)
	mouse_exited.connect(atualizar)
	atualizar()


func atualizar() -> void:
	disabled = resolvida
	visual.modulate.a = 0.35 if resolvida else 1.0
	letra.text = "%s\nOK" % base if resolvida else ("[%s]" % base if selecionada else base)
	$indicador_foco.visible = not resolvida and (has_focus() or is_hovered())
	tooltip_text = "%s: %s" % [base, "pareada" if resolvida else ("selecionada" if selecionada else "disponivel")]
