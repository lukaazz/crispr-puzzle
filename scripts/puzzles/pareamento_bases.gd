extends CanvasLayer

signal puzzle_concluido(sucesso: bool)

const TipoBotaoBase = preload("res://scripts/componentes/botao_base.gd")
const CENA_BASE: PackedScene = preload("res://scenes/componentes/botao_base.tscn")
const BASES: Array[String] = ["A", "T", "G", "C"]
const COMPLEMENTOS: Dictionary = {"A": "T", "T": "A", "G": "C", "C": "G"}
const DIFICULDADE_MINIMA: int = 1
const DIFICULDADE_MAXIMA: int = 10
const PARES_ADICIONAIS: int = 2

var _quantidade_pares: int = DIFICULDADE_MINIMA + PARES_ADICIONAIS
var _acertos: int = 0
var _concluido: bool = false
var _esquerda: TipoBotaoBase = null
var _direita: TipoBotaoBase = null
var _botoes_esquerda: Array[TipoBotaoBase] = []

@onready var grade_esquerda: GridContainer = $tela/centralizador/conteudo/listas/esquerda/grade
@onready var grade_direita: GridContainer = $tela/centralizador/conteudo/listas/direita/grade
@onready var progresso: Label = $tela/centralizador/conteudo/progresso


func setup(dificuldade: int) -> void:
	_quantidade_pares = clampi(dificuldade, DIFICULDADE_MINIMA, DIFICULDADE_MAXIMA) + PARES_ADICIONAIS
	if is_node_ready():
		_gerar_pares()


func _ready() -> void:
	$tela/centralizador/conteudo/sair.pressed.connect(_sair)
	_gerar_pares()


func _gerar_pares() -> void:
	for grade: GridContainer in [grade_esquerda, grade_direita]:
		for filho: Node in grade.get_children():
			grade.remove_child(filho)
			filho.queue_free()
	_botoes_esquerda.clear()
	_esquerda = null
	_direita = null
	_acertos = 0
	_concluido = false
	var bases_esquerda: Array[String] = []
	var bases_direita: Array[String] = []
	for indice: int in range(_quantidade_pares):
		var base: String = BASES[randi_range(0, BASES.size() - 1)]
		bases_esquerda.append(base)
		bases_direita.append(COMPLEMENTOS[base])
	bases_esquerda.shuffle()
	bases_direita.shuffle()
	for base: String in bases_esquerda:
		_botoes_esquerda.append(_criar_botao(base, grade_esquerda, true))
	for base: String in bases_direita:
		_criar_botao(base, grade_direita, false)
	progresso.text = "0 / %d pares. Escolha uma base de cada lado." % _quantidade_pares
	_focar_proxima_base()


func _criar_botao(base: String, grade: GridContainer, lado_esquerdo: bool) -> TipoBotaoBase:
	var botao: TipoBotaoBase = CENA_BASE.instantiate() as TipoBotaoBase
	botao.base = base
	grade.add_child(botao)
	botao.pressed.connect(_selecionar.bind(botao, lado_esquerdo))
	return botao


func _selecionar(botao: TipoBotaoBase, lado_esquerdo: bool) -> void:
	if _concluido or botao.resolvida:
		return
	var anterior: TipoBotaoBase = _esquerda if lado_esquerdo else _direita
	if anterior != null:
		anterior.selecionada = false
		anterior.atualizar()
	if lado_esquerdo:
		_esquerda = botao if anterior != botao else null
	else:
		_direita = botao if anterior != botao else null
	botao.selecionada = anterior != botao
	botao.atualizar()
	if _esquerda == null or _direita == null:
		return
	var correto: bool = COMPLEMENTOS[_esquerda.base] == _direita.base
	if correto:
		_acertos += 1
		_esquerda.resolvida = true
		_direita.resolvida = true
		progresso.text = "%d / %d pares. Correto!" % [_acertos, _quantidade_pares]
	else:
		progresso.text = "%s–%s nao forma um par. Tente A–T ou G–C. (%d / %d)" % [_esquerda.base, _direita.base, _acertos, _quantidade_pares]
	_esquerda.selecionada = false
	_direita.selecionada = false
	_esquerda.atualizar()
	_direita.atualizar()
	_esquerda = null
	_direita = null
	if _acertos == _quantidade_pares:
		_concluido = true
		puzzle_concluido.emit(true)
	else:
		_focar_proxima_base()


func _focar_proxima_base() -> void:
	for botao: TipoBotaoBase in _botoes_esquerda:
		if not botao.resolvida:
			botao.grab_focus()
			return


func _sair() -> void:
	if _concluido:
		return
	_concluido = true
	puzzle_concluido.emit(false)
