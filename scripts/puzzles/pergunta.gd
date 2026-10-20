extends CanvasLayer

signal puzzle_concluido(sucesso: bool)

const TipoBotao = preload("res://scripts/componentes/botao_visual.gd")
const CENA_BOTAO: PackedScene = preload("res://scenes/componentes/botao_visual.tscn")
const CAMINHO_PERGUNTAS: String = "res://data/perguntas.json"
const DIFICULDADE_MAXIMA: int = 10
const TAMANHO_ALTERNATIVA: Vector2 = Vector2(1000, 64)

var _dificuldade: int = 1
var _pergunta: Dictionary = {}
var _respondida: bool = false
var _acertou: bool = false
var _concluido: bool = false

@onready var enunciado: Label = $tela/margem/conteudo/rolagem/textos/enunciado
@onready var alternativas: VBoxContainer = $tela/margem/conteudo/rolagem/textos/alternativas
@onready var explicacao: Label = $tela/margem/conteudo/rolagem/textos/explicacao
@onready var continuar: TipoBotao = $tela/margem/conteudo/continuar
@onready var sair: TipoBotao = $tela/margem/conteudo/sair


func setup(dificuldade: int) -> void:
	_dificuldade = clampi(dificuldade, 1, DIFICULDADE_MAXIMA)
	if is_node_ready():
		_sortear_pergunta()


func _ready() -> void:
	continuar.pressed.connect(_continuar)
	sair.pressed.connect(_sair)
	_sortear_pergunta()


func _sortear_pergunta() -> void:
	_respondida = false
	_concluido = false
	_acertou = false
	_pergunta = {}
	continuar.hide()
	sair.show()
	explicacao.text = ""
	for filho: Node in alternativas.get_children():
		alternativas.remove_child(filho)
		filho.queue_free()
	var candidatas: Array[Dictionary] = _carregar_perguntas()
	if candidatas.is_empty():
		enunciado.text = "Nenhuma pergunta valida disponivel para esta dificuldade."
		sair.grab_focus()
		return
	_pergunta = candidatas[randi_range(0, candidatas.size() - 1)]
	enunciado.text = str(_pergunta["enunciado"])
	var opcoes: Array = _pergunta["alternativas"]
	for indice: int in range(opcoes.size()):
		var botao: TipoBotao = CENA_BOTAO.instantiate() as TipoBotao
		botao.rotulo = "%d. %s" % [indice + 1, str(opcoes[indice])]
		botao.custom_minimum_size = TAMANHO_ALTERNATIVA
		alternativas.add_child(botao)
		var legenda: Label = botao.get_node("legenda") as Label
		legenda.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
		botao.pressed.connect(_responder.bind(indice))
		if indice == 0:
			botao.grab_focus()


func _carregar_perguntas() -> Array[Dictionary]:
	var resultado: Array[Dictionary] = []
	if not FileAccess.file_exists(CAMINHO_PERGUNTAS):
		return resultado
	var analisador: JSON = JSON.new()
	if analisador.parse(FileAccess.get_file_as_string(CAMINHO_PERGUNTAS)) != OK:
		push_warning("perguntas.json invalido.")
		return resultado
	var dados: Variant = analisador.data
	if not dados is Array:
		return resultado
	var nivel_encontrado: int = 0
	for entrada: Variant in dados:
		if not _entrada_valida(entrada):
			continue
		var nivel: int = int(entrada["dificuldade"])
		if nivel > _dificuldade or nivel < nivel_encontrado:
			continue
		if nivel > nivel_encontrado:
			resultado.clear()
			nivel_encontrado = nivel
		resultado.append(entrada)
	return resultado


func _entrada_valida(entrada: Variant) -> bool:
	if not entrada is Dictionary:
		return false
	for campo: String in ["enunciado", "explicacao"]:
		if not entrada.get(campo) is String or str(entrada[campo]).strip_edges().is_empty():
			return false
	var opcoes: Variant = entrada.get("alternativas")
	if not opcoes is Array or opcoes.size() < 2:
		return false
	for opcao: Variant in opcoes:
		if not opcao is String or str(opcao).strip_edges().is_empty():
			return false
	for campo: String in ["indice_correto", "dificuldade"]:
		var valor: Variant = entrada.get(campo)
		if not (valor is int or valor is float):
			return false
		if not is_finite(float(valor)) or float(valor) != floorf(float(valor)):
			return false
	var correto: int = int(entrada["indice_correto"])
	var nivel: int = int(entrada["dificuldade"])
	return correto >= 0 and correto < opcoes.size() and nivel >= 1 and nivel <= DIFICULDADE_MAXIMA


func _responder(indice: int) -> void:
	if _respondida or _concluido:
		return
	_respondida = true
	_acertou = indice == int(_pergunta["indice_correto"])
	for filho: Node in alternativas.get_children():
		var botao: TipoBotao = filho as TipoBotao
		botao.definir_desabilitado(true)
	var opcoes: Array = _pergunta["alternativas"]
	var correta: String = str(opcoes[int(_pergunta["indice_correto"])])
	explicacao.text = "%s\nResposta correta: %s\n\n%s" % [
		"Resposta correta!" if _acertou else "Resposta incorreta.",
		correta, str(_pergunta["explicacao"])
	]
	sair.hide()
	continuar.show()
	continuar.grab_focus()
	$tela/margem/conteudo/rolagem.ensure_control_visible.call_deferred(explicacao)


func _continuar() -> void:
	if not _respondida or _concluido:
		return
	_concluido = true
	puzzle_concluido.emit(_acertou)


func _sair() -> void:
	if _concluido:
		return
	_concluido = true
	puzzle_concluido.emit(false)
