extends Control

const CENA_BOTAO: PackedScene = preload("res://scenes/componentes/botao_visual.tscn")
const TipoBotaoVisual = preload("res://scripts/componentes/botao_visual.gd")

var _botoes: Array[TipoBotaoVisual] = []

@onready var grade: GridContainer = $centralizador/conteudo/grade
@onready var aviso: Label = $centralizador/conteudo/aviso
@onready var voltar: TipoBotaoVisual = $centralizador/conteudo/voltar


func _ready() -> void:
	for numero: int in range(get_node("/root/GameState").PrimeiraFase, get_node("/root/GameState").TotalFases + 1):
		var botao: TipoBotaoVisual = CENA_BOTAO.instantiate() as TipoBotaoVisual
		botao.name = "fase_%02d" % numero
		grade.add_child(botao)
		botao.pressed.connect(_selecionar_fase.bind(numero))
		_botoes.append(botao)
	get_node("/root/GameState").ProgressoAlterado.connect(_atualizar_progresso)
	voltar.pressed.connect(_voltar_ao_menu)
	_atualizar_progresso()
	var indice: int = get_node("/root/GameState").FaseAtual - get_node("/root/GameState").PrimeiraFase
	_botoes[indice].grab_focus()


func _atualizar_progresso() -> void:
	for indice: int in range(_botoes.size()):
		var numero: int = indice + get_node("/root/GameState").PrimeiraFase
		_botoes[indice].configurar_fase(
			numero,
			get_node("/root/GameState").FasesLiberadas.has(numero),
			get_node("/root/GameState").FasesConcluidas.has(numero)
		)


func _selecionar_fase(numero: int) -> void:
	if not get_node("/root/GameState").SelecionarFase(numero):
		return
	var erro: Error = get_node("/root/SceneRouter").IrParaFase()
	if erro == ERR_FILE_NOT_FOUND:
		aviso.text = "Fase %d selecionada. A cena da fase ainda nao esta disponivel." % numero
	elif erro != OK:
		aviso.text = "Nao foi possivel abrir a fase %d." % numero


func _voltar_ao_menu() -> void:
	if get_node("/root/SceneRouter").IrParaMenu() != OK:
		aviso.text = "Nao foi possivel voltar ao menu."
