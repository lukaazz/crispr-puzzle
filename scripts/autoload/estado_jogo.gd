extends Node

signal progresso_alterado

const CAMINHO_SAVE: String = "user://save.cfg"
const SECAO: String = "progresso"
const TOTAL_FASES: int = 10
const PRIMEIRA_FASE: int = 1

var fase_atual: int = PRIMEIRA_FASE
var fases_liberadas: Array[int] = [PRIMEIRA_FASE]
var fases_concluidas: Array[int] = []


func _ready() -> void:
	carregar_progresso()


func selecionar_fase(numero: int) -> bool:
	if not fases_liberadas.has(numero):
		return false
	fase_atual = numero
	salvar_progresso()
	progresso_alterado.emit()
	return true


func concluir_fase(numero: int) -> void:
	if numero < PRIMEIRA_FASE or numero > TOTAL_FASES or not fases_liberadas.has(numero):
		return
	if not fases_concluidas.has(numero):
		fases_concluidas.append(numero)
	if numero < TOTAL_FASES and not fases_liberadas.has(numero + 1):
		fases_liberadas.append(numero + 1)
	fases_concluidas.sort()
	fases_liberadas.sort()
	salvar_progresso()
	progresso_alterado.emit()


func salvar_progresso() -> void:
	var configuracao: ConfigFile = ConfigFile.new()
	configuracao.set_value(SECAO, "fase_atual", fase_atual)
	configuracao.set_value(SECAO, "fases_liberadas", fases_liberadas)
	configuracao.set_value(SECAO, "fases_concluidas", fases_concluidas)
	var erro: Error = configuracao.save(CAMINHO_SAVE)
	if erro != OK:
		push_warning("Nao foi possivel salvar o progresso: %s" % error_string(erro))


func carregar_progresso() -> void:
	fase_atual = PRIMEIRA_FASE
	fases_liberadas = [PRIMEIRA_FASE]
	fases_concluidas.clear()
	var configuracao: ConfigFile = ConfigFile.new()
	var erro: Error = configuracao.load(CAMINHO_SAVE)
	if erro != OK:
		if erro != ERR_FILE_NOT_FOUND:
			push_warning("Nao foi possivel carregar o progresso: %s" % error_string(erro))
		return
	var concluidas: Variant = configuracao.get_value(SECAO, "fases_concluidas", [])
	if concluidas is Array:
		# Reconstrui os desbloqueios a partir das conclusoes consecutivas validas.
		for numero: int in range(PRIMEIRA_FASE, TOTAL_FASES + 1):
			if not concluidas.has(numero):
				break
			fases_concluidas.append(numero)
			if numero < TOTAL_FASES:
				fases_liberadas.append(numero + 1)
	var atual: Variant = configuracao.get_value(SECAO, "fase_atual", PRIMEIRA_FASE)
	if atual is int and fases_liberadas.has(atual):
		fase_atual = int(atual)
	progresso_alterado.emit()
