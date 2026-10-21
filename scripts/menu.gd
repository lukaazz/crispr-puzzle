extends Control

const VISUAIS_BOTOES: Dictionary = {
	"normal": "ui/botao_normal",
	"hover": "ui/botao_hover",
	"pressed": "ui/botao_hover",
	"disabled": "ui/botao_bloqueado",
}

@onready var botao_jogar: Button = $centralizador/conteudo/jogar
@onready var botao_sair: Button = $centralizador/conteudo/sair
@onready var aviso: Label = $centralizador/conteudo/aviso


func _ready() -> void:
	botao_jogar.grab_focus()
	get_node("/root/Assets").AssetsRecarregados.connect(_atualizar_botoes)
	_atualizar_botoes()


func _atualizar_botoes() -> void:
	# O tema nativo permanece como placeholder de cada estado sem imagem.
	for botao: Button in [botao_jogar, botao_sair]:
		for estado: String in VISUAIS_BOTOES:
			botao.remove_theme_stylebox_override(estado)
			var textura: Texture2D = get_node("/root/Assets").Textura(VISUAIS_BOTOES[estado])
			if textura != null:
				var estilo: StyleBoxTexture = StyleBoxTexture.new()
				estilo.texture = textura
				estilo.axis_stretch_horizontal = StyleBoxTexture.AXIS_STRETCH_MODE_TILE_FIT
				estilo.axis_stretch_vertical = StyleBoxTexture.AXIS_STRETCH_MODE_TILE_FIT
				botao.add_theme_stylebox_override(estado, estilo)


func _ao_jogar_pressed() -> void:
	var erro: Error = get_node("/root/SceneRouter").IrParaSeletorFases()
	if erro == ERR_FILE_NOT_FOUND:
		aviso.text = "O seletor de fases ainda nao esta disponivel."
	elif erro != OK:
		aviso.text = "Nao foi possivel abrir o seletor de fases."


func _ao_sair_pressed() -> void:
	get_node("/root/SceneRouter").Sair()
