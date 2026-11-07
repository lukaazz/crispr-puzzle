"""Teste Godot real da etapa 3. Uso: python tools/validar_etapa3.py CAMINHO_GODOT.
Requer dotnet build e assets importados. Nao altera o save do usuario.
"""
from pathlib import Path
import subprocess
import sys
import tempfile

RAIZ = Path(__file__).resolve().parents[1]
TESTE = r'''extends SceneTree
var erros = 0
func verificar(condicao, mensagem):
    if not condicao:
        push_error(mensagem)
        erros += 1
func _initialize():
    call_deferred("executar")
func executar():
    var estado = root.get_node("GameState")
    estado.FaseAtual = 1
    var fase = load("res://scenes/fase.tscn").instantiate()
    root.add_child(fase)
    current_scene = fase
    fase.set_process(false)
    var mapa = fase.get_node("mapa")
    var jogador = mapa.get_node("cas9")
    jogador.set_physics_process(false)
    verificar(mapa.TipoNaPosicao(jogador.position) == "plataforma_inicio", "Spawn invalido")
    verificar(not fase.has_node("fita"), "Fita antiga instanciada")
    verificar(mapa.y_sort_enabled, "Y-sort ausente")
    var dados = JSON.parse_string(FileAccess.get_file_as_string("res://data/mapas/fase_01.json"))
    var origem = Vector2(dados.origem_centro_px[0],dados.origem_centro_px[1])
    var pontos = {}
    var marcadores = 0
    for c in dados.celulas:
        if not c.caminhavel: continue
        var p = Vector2(c.x,c.y)
        var centro = origem + Vector2((p.x-p.y)*24,(p.x+p.y)*12) + Vector2(c.offset_observado_px[0],c.offset_observado_px[1])
        pontos[Vector2i(c.x,c.y)] = centro
        verificar(mapa.TipoNaPosicao(centro) != "vazio", "Centro bloqueado: " + str(p))
        if c.tipo.begins_with("marcador_"): marcadores += 1
    verificar(pontos.size() == 275 and marcadores == 16, "Contagem de pisos incorreta")
    var visitados = {Vector2i(-2,5):true}
    var fila = [Vector2i(-2,5)]
    while not fila.is_empty():
        var p = fila.pop_front()
        for passo in [Vector2i.LEFT, Vector2i.RIGHT, Vector2i.UP, Vector2i.DOWN]:
            var q = p + passo
            if not pontos.has(q) or visitados.has(q): continue
            var chegada = mapa.Mover(pontos[p], pontos[q]-pontos[p])
            if chegada.distance_to(pontos[q]) < 1.0:
                visitados[q] = true
                fila.append(q)
    print("Pisos alcancaveis pelo movimento real: ", visitados.size(), "/", pontos.size())
    for p in pontos:
        if not visitados.has(p): print("INACESSIVEL ",p, " ", pontos[p])
    verificar(visitados.size() == pontos.size(), "Mapa visual interrompe movimento")
    var fora = mapa.Mover(jogador.position, Vector2(-10000,0))
    verificar(mapa.TipoNaPosicao(fora) != "vazio" and fora.distance_to(jogador.position)<1000, "Player atravessou vazio")
    var visual = jogador.get_node("visual")
    verificar(visual.get_node("imagem").visible, "Imagem parada ausente")
    visual.DefinirAnimacaoAtiva(true)
    verificar(visual.get_node("animacao").sprite_frames.get_frame_count("padrao") == 6, "Sheet incorreto")
    verificar(visual.get_node("animacao").visible, "Animacao invisivel")
    visual.DefinirEspelhamento(true)
    verificar(visual.get_node("animacao").flip_h, "Espelhamento ausente")
    visual.DefinirAnimacaoAtiva(false)
    verificar(visual.get_node("imagem").visible and not visual.get_node("animacao").visible, "Parada nao volta ao estatico")
    var posicao_inicial = jogador.position
    Input.action_press("mover_dir")
    jogador._PhysicsProcess(0.01)
    Input.action_release("mover_dir")
    var horizontal = jogador.position.x - posicao_inicial.x
    jogador.position = posicao_inicial
    Input.action_press("mover_baixo")
    jogador._PhysicsProcess(0.01)
    Input.action_release("mover_baixo")
    var vertical = jogador.position.y - posicao_inicial.y
    verificar(abs(horizontal - 2 * vertical) < 0.01, "Velocidade vertical incorreta")
    jogador.position = posicao_inicial
    jogador.DefinirMovimentoAtivo(false)
    var camera = jogador.get_node("camera")
    verificar(camera.limit_right>camera.limit_left and camera.limit_bottom>camera.limit_top, "Camera sem limites")
    var visuais_originais = FileAccess.get_file_as_string("res://data/visuais.json")
    var cfg = JSON.parse_string(FileAccess.get_file_as_string("res://data/isometrico.json"))
    var substitutos = JSON.parse_string(visuais_originais)
    for id in cfg.tiles_por_tipo.values() + cfg.plataformas.tiles + [cfg.objetos.bandeira.id_visual, "player/cas9", "player/cas9_andando_sheet"]:
        substitutos[id] = {"arquivo":"ausente_teste.png"}
    var arquivo = FileAccess.open("res://data/visuais.json", FileAccess.WRITE)
    arquivo.store_string(JSON.stringify(substitutos))
    arquivo.close()
    root.get_node("Assets").Recarregar()
    var poligonos = 0
    for suporte in mapa.get_children():
        for filho in suporte.get_children():
            if filho is Polygon2D: poligonos += 1
    verificar(poligonos == 276, "Placeholders de pisos/bandeira ausentes")
    verificar(visual.get_node("placeholder").visible, "Cas9 sem placeholder")
    arquivo = FileAccess.open("res://data/visuais.json", FileAccess.WRITE)
    arquivo.store_string(visuais_originais)
    arquivo.close()
    root.get_node("Assets").Recarregar()
    verificar(visual.get_node("imagem").visible, "Recarregar nao restaurou imagem")
    # Impede gravacao de progresso neste ensaio de navegacao.
    estado.FasesLiberadas.clear()
    jogador.position = pontos[Vector2i(50,5)]
    jogador.set_physics_process(true)
    fase.set_process(true)
    for i in 5: await process_frame
    verificar(current_scene.scene_file_path == "res://scenes/seletor_fases.tscn", "Chegada nao voltou ao seletor")
    print("ETAPA3: ", erros, " falhas")
    quit(0 if erros == 0 else 1)
'''

def main():
    if len(sys.argv) != 2:
        raise SystemExit('Uso: python tools/validar_etapa3.py CAMINHO_GODOT')
    visuais = RAIZ / 'data/visuais.json'
    original = visuais.read_bytes()
    arquivo = None
    try:
        with tempfile.NamedTemporaryFile(mode='w', suffix='.gd', prefix='_teste_etapa3_',
                                         dir=RAIZ/'tools', encoding='utf-8', delete=False) as f:
            f.write(TESTE)
            arquivo = Path(f.name)
        resultado = subprocess.run([sys.argv[1], '--headless', '--path', str(RAIZ),
                                    '--fixed-fps', '60', '--script',
                                    'res://tools/'+arquivo.name], timeout=60)
        raise SystemExit(resultado.returncode)
    finally:
        visuais.write_bytes(original)
        if arquivo:
            arquivo.unlink(missing_ok=True)
            Path(str(arquivo)+'.uid').unlink(missing_ok=True)

if __name__ == '__main__':
    main()
