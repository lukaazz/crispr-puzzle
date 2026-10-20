extends Node

signal assets_recarregados

const PASTA_SPRITES: String = "res://assets/sprites/"
const PASTA_AUDIO: String = "res://assets/audio/"
const ARQUIVO_VISUAIS: String = "res://data/visuais.json"
const EXTENSOES_AUDIO: Array[String] = [".ogg", ".wav", ".mp3"]

var _texturas: Dictionary = {}
var _sons: Dictionary = {}
var _visuais: Dictionary = {}


func _ready() -> void:
	recarregar()


func textura(id: String) -> Texture2D:
	if id.is_empty():
		return null
	if _texturas.has(id):
		return _texturas[id] as Texture2D
	var arquivo: String = str(configuracao_visual(id).get("arquivo", id + ".png"))
	var caminho: String = PASTA_SPRITES + arquivo
	var imagem: Texture2D = null
	if _caminho_relativo_valido(arquivo) and ResourceLoader.exists(caminho, "Texture2D"):
		imagem = ResourceLoader.load(caminho, "Texture2D", ResourceLoader.CACHE_MODE_REPLACE) as Texture2D
	_texturas[id] = imagem
	return imagem


func som(id: String) -> AudioStream:
	if not _caminho_relativo_valido(id):
		return null
	if _sons.has(id):
		return _sons[id] as AudioStream
	var audio: AudioStream = null
	for extensao: String in EXTENSOES_AUDIO:
		var caminho: String = PASTA_AUDIO + id + extensao
		if ResourceLoader.exists(caminho, "AudioStream"):
			audio = ResourceLoader.load(caminho, "AudioStream", ResourceLoader.CACHE_MODE_REPLACE) as AudioStream
			if audio != null:
				break
	_sons[id] = audio
	return audio


func configuracao_visual(id: String) -> Dictionary:
	var entrada: Variant = _visuais.get(id, {})
	if entrada is Dictionary:
		return entrada
	return {}


func escala_visual(id: String) -> float:
	var valor: Variant = configuracao_visual(id).get("escala", 1.0)
	if (valor is float or valor is int) and is_finite(float(valor)) and float(valor) > 0.0:
		return float(valor)
	return 1.0


func deslocamento_visual(id: String) -> Vector2:
	var valor: Variant = configuracao_visual(id).get("offset", [])
	if valor is Array and valor.size() == 2:
		if (valor[0] is int or valor[0] is float) and (valor[1] is int or valor[1] is float):
			var deslocamento: Vector2 = Vector2(float(valor[0]), float(valor[1]))
			if deslocamento.is_finite():
				return deslocamento
	return Vector2.ZERO


func recarregar() -> void:
	_texturas.clear()
	_sons.clear()
	_visuais.clear()
	if FileAccess.file_exists(ARQUIVO_VISUAIS):
		var analisador: JSON = JSON.new()
		var erro: Error = analisador.parse(FileAccess.get_file_as_string(ARQUIVO_VISUAIS))
		if erro == OK and analisador.data is Dictionary:
			_visuais = analisador.data
		else:
			push_warning("visuais.json invalido; usando os caminhos e ajustes padrao.")
	assets_recarregados.emit()


func _caminho_relativo_valido(caminho: String) -> bool:
	return not caminho.is_empty() and not caminho.is_absolute_path() and not caminho.contains("..") and not caminho.contains(":") and not caminho.contains("\\")
