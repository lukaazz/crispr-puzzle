"""Etapa 1: python tools/analisar_assets.py (dependencia: Pillow)."""
from collections import Counter
from hashlib import sha256
from pathlib import Path
import json
from PIL import Image, ImageDraw, ImageFilter

RAIZ = Path(__file__).resolve().parents[1]
SPRITES = RAIZ / 'assets/sprites'
SAIDA = SPRITES / 'mapa/tiles'
CORES = {'verde': (30,188,115), 'azul': (77,155,230), 'carmim': (195,36,84),
         'roxo': (168,132,243), 'turquesa': (48,225,185), 'oliva': (146,169,132)}
EROSAO = 5  # Separa topos unidos por linhas finas e elimina detalhes laterais.
AREA_MINIMA = 21  # O menor topo parcialmente visivel conserva 30 pixels.

def dados(im):
    return list(im.get_flattened_data() if hasattr(im, 'get_flattened_data') else im.getdata())

def componentes(mascara, largura, altura):
    """Oito vizinhos, sem descartar componentes pequenos do tileset."""
    restantes = set(mascara)
    resultado = []
    while restantes:
        inicio = restantes.pop()
        pilha, pontos = [inicio], [inicio]
        while pilha:
            atual = pilha.pop()
            x, y = atual % largura, atual // largura
            for dy in (-1,0,1):
                for dx in (-1,0,1):
                    nx, ny = x+dx, y+dy
                    p = ny*largura+nx
                    if 0 <= nx < largura and 0 <= ny < altura and p in restantes:
                        restantes.remove(p)
                        pilha.append(p)
                        pontos.append(p)
        xs, ys = [p % largura for p in pontos], [p // largura for p in pontos]
        resultado.append(((min(xs),min(ys),max(xs)+1,max(ys)+1), pontos))
    return sorted(resultado, key=lambda c: (c[0][1], c[0][0]))

def salvar_json(caminho, valor):
    caminho.parent.mkdir(parents=True, exist_ok=True)
    caminho.write_text(json.dumps(valor, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')

def fatiar(im):
    px = dados(im)
    grupos = componentes((i for i,p in enumerate(px) if p[3]), *im.size)
    SAIDA.mkdir(parents=True, exist_ok=True)
    registros = []
    reconstruida = Image.new('RGBA', im.size)
    for numero, (caixa, pontos) in enumerate(grupos, 1):
        x0,y0,x1,y1 = caixa
        tile = Image.new('RGBA', (x1-x0,y1-y0))
        for p in pontos:
            tile.putpixel((p%im.width-x0,p//im.width-y0),px[p])
            reconstruida.putpixel((p%im.width,p//im.width),px[p])
        nome = f'tile_{numero:03d}'
        tile.save(SAIDA / f'{nome}.png')
        registros.append({'id':f'mapa/tiles/{nome}', 'caixa_origem_px':list(caixa),
                          'tamanho_px':list(tile.size), 'pixels_visiveis':len(pontos)})
    assert all(a == b for a,b in zip(px,dados(reconstruida)) if a[3])
    assert sum(r['pixels_visiveis'] for r in registros) == sum(p[3]>0 for p in px)
    indice = Image.new('RGB', (800,((len(registros)+9)//10)*76), '#252936')
    desenho = ImageDraw.Draw(indice)
    for n,r in enumerate(registros):
        tile = Image.open(SPRITES/(r['id']+'.png')).convert('RGBA')
        x,y = (n%10)*80,(n//10)*76
        indice.paste(tile,(x+(80-tile.width)//2,y+4),tile)
        desenho.text((x+24,y+55),f'{n+1:03d}',fill='white')
    indice.save(SAIDA/'_indice.png')
    salvar_json(SAIDA/'_manifesto.json',{'conectividade':8,'ordem':'y_min, x_min','tiles':registros})
    return registros

def medir(tileset, registros, sheet):
    largura = Counter(r['tamanho_px'][0] for r in registros).most_common(1)[0][0]
    total = min(r['tamanho_px'][1] for r in registros if r['tamanho_px'][0] == largura)
    simples = min((r for r in registros if r['tamanho_px']==[largura,total]),key=lambda r:r['pixels_visiveis'])
    tile = tileset.crop(simples['caixa_origem_px'])
    perfil = [sum(tile.getpixel((x,y))[3]>0 for x in range(largura)) for y in range(total)]
    assert perfil[:4]==[2,6,10,14], 'O topo mudou: revisar medidas geometricas.'
    topo = largura//2  # Perfil cresce 4 px/linha: inclinacao 2:1.
    grupos = componentes((i for i,p in enumerate(dados(sheet)) if p[3]), *sheet.size)
    grupos.sort(key=lambda c:c[0][0])
    quantidade = len(grupos)
    assert sheet.width % quantidade == 0
    quadro = sheet.width//quantidade
    assert all(n*quadro<=b[0]<b[2]<=(n+1)*quadro for n,(b,_) in enumerate(grupos))
    por_cor = {}
    for cor,rgb in CORES.items():
        candidatos = [r for r in registros if r['tamanho_px']==[largura,total]
                      and (*rgb,255) in dados(tileset.crop(r['caixa_origem_px']))]
        por_cor[cor] = min(candidatos,key=lambda r:r['pixels_visiveis'])['id']
    return {'versao':1,'estado':'medidas extraidas; ainda nao integrado ao jogo',
            'tile':{'largura_topo':largura,'altura_topo':topo,'altura_lateral':total-topo,
                    'altura_total_sem_mureta':total,'proporcao':[2,1],'escala':1.0,'offset':[0,0],
                    'origem_local_centro_topo':[largura/2,topo/2]},
            'medicao':{'tile_amostra':simples['id'],'perfil_alpha_px':perfil,
                       'nota':'Topo 48x24 inclui contorno; cor interna 45x23. Muretas nao integram a lateral.'},
            'tiles_por_cor':por_cor,
            'player_sheet':{'arquivo':'player/cas9_andando_sheet.png','colunas':quantidade,'linhas':1,
                            'quantidade_quadros':quantidade,'tamanho_quadro':[quadro,sheet.height],
                            'caixas_conteudo_px':[list(b) for b,_ in grupos],
                            'fps':None,'nota':'PNG nao informa velocidade de animacao.'}}

def extrair_mapa(im, medidas):
    celulas = []
    for nome,cor in CORES.items():
        mascara = Image.new('L',im.size)
        mascara.putdata([255 if p==(*cor,255) else 0 for p in dados(im)])
        erodida = mascara.filter(ImageFilter.MinFilter(EROSAO))
        grupos = componentes((i for i,p in enumerate(dados(erodida)) if p),*im.size)
        for b,pontos in grupos:
            if len(pontos)<AREA_MINIMA:
                continue
            celulas.append({'cor':nome,'centro_visivel_px':[(b[0]+b[2])/2,(b[1]+b[3])/2],
                            'caixa_nucleo_px':list(b),'area_nucleo':len(pontos),'parcial':len(pontos)<250})
    celulas.sort(key=lambda c:(c['centro_visivel_px'][1],c['centro_visivel_px'][0]))
    origem = celulas[0]['centro_visivel_px']
    meia_largura,meia_altura = medidas['tile']['largura_topo']/2,medidas['tile']['altura_topo']/2
    ocupadas = set()
    for n,c in enumerate(celulas,1):
        px,py = c['centro_visivel_px']
        dx,dy = (px-origem[0])/meia_largura,(py-origem[1])/meia_altura
        x,y = round((dx+dy)/2),round((dy-dx)/2)
        assert (x,y) not in ocupadas, 'Grade ambigua: revisar, sem mover celulas automaticamente.'
        ocupadas.add((x,y))
        c.update(id=n,x=x,y=y,offset_observado_px=[px-(origem[0]+(x-y)*meia_largura),py-(origem[1]+(x+y)*meia_altura)])
    resultado = {'versao':1,'fonte':'assets/sprites/mapa/mapa_referencia.png','dimensoes_imagem':list(im.size),
                 'estado':'extracao visual, nao e fase jogavel',
                 'metodo':{'paleta_rgb':CORES,'erosao_px':EROSAO,'area_minima_nucleo':AREA_MINIMA,
                           'origem_centro_px':origem,
                           'grade':'projecao inversa 2:1 arredondada; offsets preservam desalinhamentos',
                           'limites':'Centros ocluidos representam a parte visivel. Nenhuma celula ausente e inferida.'},
                 'contagem_por_cor':dict(Counter(c['cor'] for c in celulas)), 'spawn':None,'defeitos':None,
                 'nota':'Sem inferir spawn, defeitos, plataformas, ligacoes ou preenchimentos de lacunas.', 'celulas':celulas}
    salvar_json(RAIZ/'data/mapas/fase_01_extraido.json',resultado)
    previa = im.copy()
    desenho = ImageDraw.Draw(previa)
    for c in celulas:
        x,y = c['centro_visivel_px']
        desenho.text((x-5,y-4),str(c['id']),fill='white',stroke_width=1,stroke_fill='black')
    previa.save(SAIDA/'_preview_extracao.png')
    return resultado

def main():
    nomes = ['mapa/tileset_full3.png','mapa/mapa_referencia.png','player/cas9_andando_sheet.png']
    hashes = {n:sha256((SPRITES/n).read_bytes()).hexdigest() for n in nomes}
    imagens = [Image.open(SPRITES/n).convert('RGBA') for n in nomes]
    for nome,im in zip(nomes,imagens):
        print(f'{nome}: {im.width} x {im.height} px')
    tileset,mapa,sheet = imagens
    registros = fatiar(tileset)
    medidas = medir(tileset,registros,sheet)
    medidas['sha256_originais'] = hashes
    salvar_json(RAIZ/'data/isometrico.json',medidas)
    resultado = extrair_mapa(mapa,medidas)
    t,p = medidas['tile'],medidas['player_sheet']
    print(f"Topo: {t['largura_topo']} x {t['altura_topo']} px; lateral: {t['altura_lateral']} px")
    print(f"Player: {p['quantidade_quadros']} quadros de {p['tamanho_quadro']} px, 1 linha")
    print(f'Tiles/componentes: {len(registros)}; todos os pixels visiveis preservados.')
    print(f"Celulas: {len(resultado['celulas'])}; por cor: {resultado['contagem_por_cor']}")
    assert hashes == {n:sha256((SPRITES/n).read_bytes()).hexdigest() for n in nomes}
    print('Originais preservados (SHA-256). Grade sem coordenadas duplicadas.')

if __name__ == '__main__':
    main()
