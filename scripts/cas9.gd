extends CharacterBody2D

const TipoVisualSlot = preload("res://scripts/componentes/visual_slot.gd")

@export var velocidade: float = 280.0

var limite_esquerdo: float = 0.0
var limite_direito: float = 0.0
var altura_caminho: float = 0.0

@onready var visual: TipoVisualSlot = $visual


func configurar_caminho(inicio: float, fim: float, altura: float) -> void:
	limite_esquerdo = inicio
	limite_direito = fim
	altura_caminho = altura
	position = Vector2(inicio, altura)


func definir_movimento_ativo(ativo: bool) -> void:
	set_physics_process(ativo)
	velocity = Vector2.ZERO
	visual.definir_animacao_ativa(false)


func _physics_process(_delta: float) -> void:
	var x_anterior: float = position.x
	var direcao: float = Input.get_axis("mover_esq", "mover_dir")
	velocity = Vector2(direcao * velocidade, 0.0)
	move_and_slide()
	position.x = clampf(position.x, limite_esquerdo, limite_direito)
	position.y = altura_caminho
	var andando: bool = not is_equal_approx(position.x, x_anterior)
	visual.definir_animacao_ativa(andando)
