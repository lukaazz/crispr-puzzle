extends Node

const CENA_MENU: String = "res://scenes/menu.tscn"
const CENA_SELETOR_FASES: String = "res://scenes/seletor_fases.tscn"
const CENA_FASE: String = "res://scenes/fase.tscn"


func ir_para_menu() -> Error:
	return trocar_cena(CENA_MENU)


func ir_para_seletor_fases() -> Error:
	return trocar_cena(CENA_SELETOR_FASES)


func ir_para_fase() -> Error:
	return trocar_cena(CENA_FASE)


func trocar_cena(caminho: String) -> Error:
	if not ResourceLoader.exists(caminho, "PackedScene"):
		return ERR_FILE_NOT_FOUND
	return get_tree().change_scene_to_file(caminho)


func sair() -> void:
	get_tree().quit()
