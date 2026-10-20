extends StaticBody2D

const TipoVisualSlot = preload("res://scripts/componentes/visual_slot.gd")

var reparado: bool = false

@onready var visual: TipoVisualSlot = $visual
@onready var colisao: CollisionShape2D = $colisao


func reparar() -> void:
	if reparado:
		return
	reparado = true
	colisao.set_deferred("disabled", true)
	visual.cor_placeholder = Color(0.12, 0.45, 0.3)
	visual.texto_placeholder = "OK"
	visual.id_visual = "dna/trecho_reparado"
