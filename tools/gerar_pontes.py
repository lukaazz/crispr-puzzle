"""Regenera apenas as pontes decorativas e a previa do mapa existente.

Uso: python tools/gerar_pontes.py (requer Pillow).
"""
import json
from completar_mapa import CONFIG, DESTINO, renderizar


def main():
    mapa = json.loads(DESTINO.read_text(encoding='utf-8'))
    config = json.loads(CONFIG.read_text(encoding='utf-8'))
    cfg = config['pontes_decorativas']
    passo = cfg['espacamento']
    if not isinstance(passo, int) or passo <= 0:
        raise ValueError('Espacamento deve ser um inteiro positivo.')
    celulas = {(c['x'], c['y']): c for c in mapa['celulas']}
    protegidas = {tuple(mapa['spawn'])}
    protegidas.update(tuple(m['celula']) for m in mapa['marcadores'])
    protegidas.update(tuple(c) for d in mapa['defeitos'] for c in d['celulas'])
    colunas_defeitos = {c[0] for d in mapa['defeitos'] for c in d['celulas']}
    pontes = []
    for x in range(cfg['inicio_x'], cfg['fim_x'] + 1, passo):
        coluna = [c for c in mapa['celulas'] if c['x'] == x]
        if x in colunas_defeitos or any(c['tipo'].startswith('plataforma_') for c in coluna):
            continue
        verde = [c['y'] for c in coluna if c['tipo'] == 'fita_verde']
        azul = [c['y'] for c in coluna if c['tipo'] == 'fita_azul']
        if not verde or not azul:
            continue
        pontos = [(x, y) for y in range(max(verde) + 1, min(azul))]
        if not pontos or any(p in protegidas or (p in celulas and
                (celulas[p]['tipo'] != 'vazio' or celulas[p]['caminhavel'])) for p in pontos):
            continue
        pontes.append({'tipo': 'ponte_decorativa', 'celulas': pontos})
    mapa['pontes_decorativas'] = pontes
    DESTINO.write_text(json.dumps(mapa, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    renderizar(mapa, config)
    print(f'Pontes criadas: {len(pontes)}; espacamento: {passo} celulas no eixo x.')
    print('Colunas:', [p['celulas'][0][0] for p in pontes])


if __name__ == '__main__':
    main()
