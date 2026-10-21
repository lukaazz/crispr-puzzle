extends StaticBody2D


var reparado: bool = false

@onready var visual: Control = $visual
@onready var colisao: CollisionShape2D = $colisao


func reparar() -> void:
	if reparado:
		return
	reparado = true
	colisao.set_deferred("disabled", true)
	visual.CorPlaceholder = Color(0.12, 0.45, 0.3)
	visual.TextoPlaceholder = "OK"
	visual.IdVisual = "dna/trecho_reparado"
