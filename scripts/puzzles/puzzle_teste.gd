extends CanvasLayer

signal puzzle_concluido(sucesso: bool)

var dificuldade: int = 1
var _concluido: bool = false


func setup(valor_dificuldade: int) -> void:
	dificuldade = maxi(1, valor_dificuldade)


func _ready() -> void:
	$tela/centralizador/conteudo/consertar.pressed.connect(_consertar)
	$tela/centralizador/conteudo/consertar.grab_focus()


func _consertar() -> void:
	if _concluido:
		return
	_concluido = true
	puzzle_concluido.emit(true)
