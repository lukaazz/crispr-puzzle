"""Completa a fase 01 e gera a previa, sem alterar a extracao nem o jogo.

Uso: python tools/completar_mapa.py (Pillow, tools/requirements.txt).
Coordenadas e offsets originais sao mantidos. Conectividade usa quatro vizinhos.
"""

from collections import Counter, deque
from copy import deepcopy
from hashlib import sha256
from pathlib import Path
import json

from PIL import Image, ImageDraw

RAIZ = Path(__file__).resolve().parents[1]
FONTE = RAIZ / 'data/mapas/fase_01_extraido.json'
CONFIG = RAIZ / 'data/isometrico.json'
DESTINO = RAIZ / 'data/mapas/fase_01.json'
PREVIA = RAIZ / 'assets/sprites/mapa/_preview_fase_01.png'
FITAS = ('fita_verde', 'fita_azul')
CORES_MARCADORES = ('carmim', 'roxo', 'turquesa', 'oliva')


def vizinhos(ponto):
    x, y = ponto
    return ((x - 1, y), (x, y - 1), (x, y + 1), (x + 1, y))


def alcancaveis(inicio, permitidos):
    visitados = {inicio}
    fila = deque([inicio])
    while fila:
        for ponto in vizinhos(fila.popleft()):
            if ponto in permitidos and ponto not in visitados:
                visitados.add(ponto)
                fila.append(ponto)
    return visitados


def grupos(pontos):
    restantes = set(pontos)
    resultado = []
    while restantes:
        grupo = alcancaveis(min(restantes), restantes)
        restantes -= grupo
        resultado.append(grupo)
    return resultado


class Isometrico:
    """Conversao unica para as ferramentas; nao adiciona logica ao Godot."""

    def __init__(self, config, origem):
        self.meia_largura = config['tile']['largura_topo'] / 2
        self.meia_altura = config['tile']['altura_topo'] / 2
        self.origem = origem

    def tela(self, ponto, offset=(0, 0)):
        x, y = ponto
        return (self.origem[0] + (x-y)*self.meia_largura + offset[0],
                self.origem[1] + (x+y)*self.meia_altura + offset[1])

    def celula(self, tela):
        dx = (tela[0]-self.origem[0]) / self.meia_largura
        dy = (tela[1]-self.origem[1]) / self.meia_altura
        return ((dx+dy)/2, (dy-dx)/2)


def completar(extraido, config):
    grade = {}
    for original in extraido['celulas']:
        celula = deepcopy(original)
        cor = original['cor']
        celula.update(tipo=f'fita_{cor}' if cor in ('verde', 'azul') else f'marcador_{cor}',
                      caminhavel=True, origem='referencia')
        grade[(celula['x'], celula['y'])] = celula
    parametros = config['completar_fase_01']
    xmin = min(x for x, y in grade)
    ymin, ymax = min(y for x, y in grade), max(y for x, y in grade)
    fim = parametros['coluna_final_fitas']
    assert fim > max(x for x, y in grade)

    def adicionar(ponto, tipo, motivo):
        if ponto in grade:
            assert grade[ponto]['tipo'] == tipo, f'Tentativa de substituir {ponto}'
            return
        grade[ponto] = {'x': ponto[0], 'y': ponto[1], 'tipo': tipo,
                        'caminhavel': True, 'origem': motivo,
                        'offset_observado_px': parametros['offset_novas_celulas'][:]}

    def pontos_tipo(tipo):
        return {p for p, c in grade.items() if c['tipo'] == tipo}

    def caminho(origens, destinos, tipo):
        """Menor caminho cardinal sem atravessar outra fita ou marcadores."""
        fila = deque(sorted(origens))
        anteriores = dict.fromkeys(origens)
        while fila:
            atual = fila.popleft()
            if atual in destinos:
                trecho = []
                while atual is not None:
                    trecho.append(atual)
                    atual = anteriores[atual]
                return trecho
            for ponto in vizinhos(atual):
                x, y = ponto
                if not (xmin <= x <= fim and ymin <= y <= ymax):
                    continue
                if ponto in anteriores or (ponto in grade and grade[ponto]['tipo'] != tipo):
                    continue
                anteriores[ponto] = atual
                fila.append(ponto)
        raise ValueError(f'Nao ha passagem livre para {tipo}')

    # Conecta todos os fragmentos, inclusive os isolados por arredondamento da extracao.
    for tipo in FITAS:
        while len(partes := grupos(pontos_tipo(tipo))) > 1:
            for ponto in caminho(partes[0], set.union(*partes[1:]), tipo):
                adicionar(ponto, tipo, 'ligacao_lacuna')

    # Marcadores continuam sendo pisos de suas proprias cores. Se necessario,
    # uma pequena ligacao da fita vizinha permite chegar ate eles sem usa-los
    # como parte da continuidade principal das fitas.
    for cor in CORES_MARCADORES:
        tipo_fita = 'fita_verde' if cor in ('carmim', 'roxo') else 'fita_azul'
        for grupo in grupos(pontos_tipo(f'marcador_{cor}')):
            borda = {v for p in grupo for v in vizinhos(p)
                     if v not in grade or grade[v]['tipo'] == tipo_fita}
            assert borda
            for ponto in caminho(pontos_tipo(tipo_fita), borda, tipo_fita):
                adicionar(ponto, tipo_fita, 'acesso_marcador')

    # Duas linhas por fita na extensao final. Plataformas ficam inteiramente
    # fora das celulas de referencia; nenhuma celula original muda de tipo.
    for tipo in FITAS:
        pontos = pontos_tipo(tipo)
        primeiro_x, ultimo_x = min(x for x, y in pontos), max(x for x, y in pontos)
        linhas_inicio = sorted(y for x, y in pontos if x == primeiro_x)
        linha_final = min(y for x, y in pontos if x == ultimo_x)
        if tipo == 'fita_verde':
            linhas_fim = [linha_final - 1, linha_final]
        else:
            linhas_fim = [linha_final, linha_final + 1]
        for y in linhas_inicio:
            for x in range(xmin, primeiro_x):
                adicionar((x, y), tipo, 'acesso_inicio')
        for y in linhas_fim:
            for x in range(ultimo_x, fim + 1):
                adicionar((x, y), tipo, 'extensao_direita')

    plataformas = []
    largura = parametros['largura_plataforma']
    for tipo, colunas in (('plataforma_inicio', range(xmin-largura, xmin)),
                          ('plataforma_fim', range(fim+1, fim+1+largura))):
        pontos = [(x, y) for x in colunas for y in range(ymin, ymax+1)]
        for ponto in pontos:
            adicionar(ponto, tipo, tipo)
        plataformas.append({'tipo': tipo, 'celulas': [list(p) for p in pontos]})
    spawn = [xmin-(largura+1)//2, (ymin+ymax)//2]
    bandeira = [fim+(largura+1)//2, (ymin+ymax)//2]
    marcadores = [{'cor': c['cor'], 'celula': [c['x'], c['y']]} for c in extraido['celulas']
                  if c['cor'] in CORES_MARCADORES]
    # Grade explicita: vazio e nao caminhavel, dentro dos limites do mapa.
    for x in range(xmin-largura, fim+largura+1):
        for y in range(ymin, ymax+1):
            if (x, y) not in grade:
                grade[(x, y)] = {'x': x, 'y': y, 'tipo': 'vazio', 'caminhavel': False}
    # Interpola somente os offsets dos pisos adicionados entre os pontos fixos
    # da referencia e as plataformas regulares. Evita saltos bruscos de alinhamento.
    novos = [p for p, c in grade.items() if c['tipo'] in FITAS and c['origem'] != 'referencia']
    for _ in range(2000):
        maior_ajuste = 0
        for p in novos:
            celula = grade[p]
            adjacentes = [grade[v]['offset_observado_px'] for v in vizinhos(p)
                          if v in grade and (grade[v]['tipo'] == celula['tipo']
                                             or grade[v]['tipo'].startswith('plataforma_'))]
            assert adjacentes
            offset = [sum(v[eixo] for v in adjacentes)/len(adjacentes) for eixo in range(2)]
            maior_ajuste = max(maior_ajuste, *(abs(a-b) for a, b in zip(offset, celula['offset_observado_px'])))
            celula['offset_observado_px'] = offset
        if maior_ajuste < 0.0001:
            break
    else:
        raise ValueError('Interpolacao de offsets nao convergiu')
    for p in novos:
        grade[p]['offset_observado_px'] = [round(v, 4) for v in grade[p]['offset_observado_px']]
    return {'versao': 1, 'numero': 1, 'fonte': 'data/mapas/fase_01_extraido.json',
            'estado': 'mapa completo para previa, ainda nao ligado ao jogo',
            'origem_centro_px': extraido['metodo']['origem_centro_px'],
            'preservacao': 'x, y e offsets das 147 celulas originais preservados; novos pisos nao substituem marcadores',
            'limites': {'x_min': xmin-largura, 'x_max': fim+largura, 'y_min': ymin, 'y_max': ymax},
            'celulas': sorted(grade.values(), key=lambda c: (c['y'], c['x'])),
            'spawn': spawn, 'plataformas': plataformas, 'marcadores': marcadores,
            'objetos': [{'tipo': 'bandeira', 'celula': bandeira}], 'defeitos': []}


def validar(mapa, fonte, config):
    grade = {(c['x'], c['y']): c for c in mapa['celulas']}
    assert len(grade) == len(mapa['celulas'])
    iso = Isometrico(config, mapa['origem_centro_px'])
    for original in fonte['celulas']:
        atual = grade[(original['x'], original['y'])]
        assert all(atual[k] == v for k, v in original.items()), 'Referencia alterada'
        assert iso.tela((atual['x'], atual['y']), atual['offset_observado_px']) == tuple(original['centro_visivel_px'])
    for tipo in FITAS:
        pontos = {p for p, c in grade.items() if c['tipo'] == tipo}
        assert len(grupos(pontos)) == 1, f'{tipo} descontinua'
        assert any(iso.tela(p, grade[p]['offset_observado_px'])[0] > fonte['dimensoes_imagem'][0]
                   for p in pontos), f'{tipo} nao ultrapassa a imagem'
        for plataforma in mapa['plataformas']:
            assert any(v in pontos for p in plataforma['celulas'] for v in vizinhos(p)), 'Fita sem acesso a plataforma'
    assert Counter(m['cor'] for m in mapa['marcadores']) == Counter({c: 4 for c in CORES_MARCADORES})
    for m in mapa['marcadores']:
        assert grade[tuple(m['celula'])]['tipo'] == 'marcador_' + m['cor']
    assert grade[tuple(mapa['spawn'])]['tipo'] == 'plataforma_inicio'
    assert grade[tuple(mapa['objetos'][0]['celula'])]['tipo'] == 'plataforma_fim'
    caminhaveis = {p for p, c in grade.items() if c['caminhavel']}
    assert alcancaveis(tuple(mapa['spawn']), caminhaveis) == caminhaveis
    assert mapa['defeitos'] == []
    for p in grade:
        assert iso.celula(iso.tela(p)) == p


def renderizar(mapa, config):
    iso = Isometrico(config, mapa['origem_centro_px'])
    imagens = {}

    def carregar(id_visual):
        if id_visual not in imagens:
            imagens[id_visual] = Image.open(RAIZ / 'assets/sprites' / (id_visual+'.png')).convert('RGBA')
        return imagens[id_visual]

    pisos, comandos = {}, []
    for c in mapa['celulas']:
        if not c['caminhavel']:
            continue
        ponto = (c['x'], c['y'])
        cx, cy = iso.tela(ponto, c['offset_observado_px'])
        pisos[ponto] = (cx, cy)
        if c['tipo'].startswith('plataforma_'):
            tiles = config['plataformas']['tiles']
            assert config['plataformas']['padrao'] == 'xadrez' and len(tiles) == 2
            visual = tiles[(c['x']+c['y']) % 2]
        else:
            visual = config['tiles_por_tipo'][c['tipo']]
        im = carregar(visual)
        ox, oy = config['tile']['origem_local_centro_topo']
        escala = config['tile']['escala']
        offset = config['tile']['offset']
        im = im.resize((round(im.width*escala), round(im.height*escala)), Image.Resampling.NEAREST)
        comandos.append((cy, 0, im, round(cx-ox*escala+offset[0]), round(cy-oy*escala+offset[1])))
    for objeto in mapa['objetos']:
        cfg = config['objetos'][objeto['tipo']]
        im = carregar(cfg['id_visual'])
        escala = cfg['escala']
        im = im.resize((round(im.width*escala), round(im.height*escala)), Image.Resampling.NEAREST)
        cx, cy = pisos[tuple(objeto['celula'])]
        ox, oy = cfg['origem_px']
        comandos.append((cy, 1, im, round(cx-ox*escala+cfg['offset'][0]), round(cy-oy*escala+cfg['offset'][1])))
    margem = config['completar_fase_01']['margem_previa_px']
    minx, miny = min(c[3] for c in comandos)-margem, min(c[4] for c in comandos)-margem
    maxx = max(c[3]+c[2].width for c in comandos)+margem
    maxy = max(c[4]+c[2].height for c in comandos)+margem
    previa = Image.new('RGBA', (maxx-minx, maxy-miny), '#161d29')
    for _, _, im, x, y in sorted(comandos, key=lambda c: (c[0], c[1], c[3])):
        previa.alpha_composite(im, (x-minx, y-miny))
    desenho = ImageDraw.Draw(previa)
    desenho.text((16, 12), 'FASE 01 | previa de dados | verde + azul | plataformas em xadrez', fill='white')
    sx, sy = pisos[tuple(mapa['spawn'])]
    sx, sy = round(sx-minx), round(sy-miny)
    desenho.ellipse((sx-3, sy-3, sx+3, sy+3), fill='white', outline='#18202b')
    desenho.text((sx-20, sy-22), 'SPAWN', fill='white', stroke_width=1, stroke_fill='#18202b')
    previa.save(PREVIA)


def main():
    hash_fonte = sha256(FONTE.read_bytes()).hexdigest()
    extraido = json.loads(FONTE.read_text(encoding='utf-8'))
    config = json.loads(CONFIG.read_text(encoding='utf-8'))
    mapa = completar(extraido, config)
    validar(mapa, extraido, config)
    DESTINO.write_text(json.dumps(mapa, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    renderizar(mapa, config)
    for tipo in FITAS:
        celulas = [c for c in mapa['celulas'] if c['tipo'] == tipo]
        adicionadas = Counter(c['origem'] for c in celulas if c['origem'] != 'referencia')
        print(f'{tipo}: {len(celulas)} celulas; adicionadas: {sum(adicionadas.values())} {dict(adicionadas)}')
    total = sum(c['caminhavel'] for c in mapa['celulas'])
    print(f'Total caminhavel: {total}; adicionado: {total-len(extraido["celulas"])} (inclui plataformas)')
    print(f'Spawn: {mapa["spawn"]}; bandeira: {mapa["objetos"][0]["celula"]}')
    print('Marcadores:', dict(Counter(m['cor'] for m in mapa['marcadores'])))
    assert sha256(FONTE.read_bytes()).hexdigest() == hash_fonte
    print('OK: referencia preservada; fitas continuas; todos os pisos alcancaveis; bandeira sobre piso final.')
    print('Previa:', PREVIA.relative_to(RAIZ))


if __name__ == '__main__':
    main()
