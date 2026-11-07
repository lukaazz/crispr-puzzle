#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class MapaIsometrico : Node2D
{
    private sealed record Celula(Vector2I Coordenada, string Tipo, bool Caminhavel, Vector2 Offset);
    private const string ArquivoMedidas = "res://data/isometrico.json";
    private const float PassoMaximo = 1.0f;
    // Pequenas costuras da arte nao interrompem a caminhada.
    private const float ToleranciaCostura = 0.08f;
    private readonly Dictionary<Vector2I, Celula> _celulas = new();
    private readonly List<Node2D> _visuais = new();
    private JsonElement _config;
    private JsonElement _dados;
    private Vector2 _origem;
    private Vector2 _tamanho;
    public Vector2 Spawn { get; private set; }
    public Rect2 Limites { get; private set; }
    public Rect2 LimitesCamera
    {
        get
        {
            Vector2 margem = Vetor(_config.GetProperty("camera").GetProperty("margem_px"));
            return Limites.GrowIndividual(margem.X, margem.Y, margem.X, margem.Y);
        }
    }
    public float ProporcaoVertical => _tamanho.Y / _tamanho.X;

    public override void _Ready()
    {
        YSortEnabled = true;
        TextureFilter = TextureFilterEnum.Nearest;
        GetNode<Assets>("/root/Assets").AssetsRecarregados += Desenhar;
    }

    public override void _ExitTree()
    {
        Assets? assets = GetNodeOrNull<Assets>("/root/Assets");
        if (assets != null) assets.AssetsRecarregados -= Desenhar;
    }

    public void Carregar(string arquivo)
    {
        using JsonDocument medidas = JsonDocument.Parse(FileAccess.GetFileAsString(ArquivoMedidas));
        using JsonDocument mapa = JsonDocument.Parse(FileAccess.GetFileAsString(arquivo));
        _config = medidas.RootElement.Clone();
        _dados = mapa.RootElement.Clone();
        JsonElement tile = _config.GetProperty("tile");
        _tamanho = new(tile.GetProperty("largura_topo").GetSingle(), tile.GetProperty("altura_topo").GetSingle());
        if (_tamanho.X <= 0 || _tamanho.Y <= 0) throw new InvalidOperationException("Medidas invalidas.");
        _origem = Vetor(_dados.GetProperty("origem_centro_px"));
        _celulas.Clear();
        foreach (JsonElement entrada in _dados.GetProperty("celulas").EnumerateArray())
        {
            Vector2I coordenada = new(entrada.GetProperty("x").GetInt32(), entrada.GetProperty("y").GetInt32());
            Vector2 offset = entrada.TryGetProperty("offset_observado_px", out JsonElement ajuste) ? Vetor(ajuste) : Vector2.Zero;
            _celulas.Add(coordenada, new(coordenada, entrada.GetProperty("tipo").GetString()!,
                entrada.GetProperty("caminhavel").GetBoolean(), offset));
        }
        Vector2I inicio = Coordenada(_dados.GetProperty("spawn"));
        if (!_celulas.TryGetValue(inicio, out Celula? celula) || !celula.Caminhavel)
            throw new InvalidOperationException("Spawn fora do piso.");
        Spawn = Centro(celula);
        Desenhar();
    }

    private Vector2 Centro(Celula celula) => Isometrico.CelulaParaTela(celula.Coordenada, _origem, _tamanho) + celula.Offset;
    private static Vector2 Vetor(JsonElement valor) => new(valor[0].GetSingle(), valor[1].GetSingle());
    private static Vector2I Coordenada(JsonElement valor) => new(valor[0].GetInt32(), valor[1].GetInt32());

    public string TipoNaPosicao(Vector2 posicao)
    {
        Vector2 nominal = Isometrico.TelaParaCelula(posicao, _origem, _tamanho);
        Vector2I centro = new(Mathf.RoundToInt(nominal.X), Mathf.RoundToInt(nominal.Y));
        Celula? melhor = null;
        float menorDistancia = float.MaxValue;
        // Offsets da extracao podem deslocar um topo para a celula vizinha.
        for (int y = centro.Y - 2; y <= centro.Y + 2; y++)
            for (int x = centro.X - 2; x <= centro.X + 2; x++)
            {
                if (!_celulas.TryGetValue(new(x, y), out Celula? celula) || !celula.Caminhavel) continue;
                Vector2 local = Isometrico.TelaParaCelula(posicao - celula.Offset, _origem, _tamanho) - (Vector2)celula.Coordenada;
                float distancia = Mathf.Max(Mathf.Abs(local.X), Mathf.Abs(local.Y));
                if (distancia <= 0.5f + ToleranciaCostura && distancia < menorDistancia)
                {
                    melhor = celula;
                    menorDistancia = distancia;
                }
            }
        if (melhor != null) return melhor.Tipo;
        // Os centros dos marcadores parcialmente ocluidos da referencia nao
        // descrevem um piso inteiro. A grade continua sendo a autoridade nas
        // emendas entre esses recortes; nunca criamos uma celula caminhavel.
        return _celulas.TryGetValue(centro, out Celula? nominalCelula) && nominalCelula.Caminhavel
            ? nominalCelula.Tipo : "vazio";
    }

    public Vector2 Mover(Vector2 origem, Vector2 deslocamento)
    {
        int passos = Mathf.Max(1, Mathf.CeilToInt(deslocamento.Length() / PassoMaximo));
        Vector2 passo = deslocamento / passos;
        Vector2 posicao = origem;
        for (int i = 0; i < passos; i++)
        {
            Vector2 destino = posicao + passo;
            bool podeX = TipoNaPosicao(posicao + new Vector2(passo.X, 0)) != "vazio";
            bool podeY = TipoNaPosicao(posicao + new Vector2(0, passo.Y)) != "vazio";
            if (TipoNaPosicao(destino) != "vazio" && (podeX || podeY)) posicao = destino;
            else if (podeX) posicao.X += passo.X;
            else if (podeY) posicao.Y += passo.Y;
        }
        return posicao;
    }

    private void Desenhar()
    {
        if (_config.ValueKind != JsonValueKind.Object) return;
        foreach (Node2D visual in _visuais) { RemoveChild(visual); visual.QueueFree(); }
        _visuais.Clear();
        Assets assets = GetNode<Assets>("/root/Assets");
        JsonElement tile = _config.GetProperty("tile");
        Vector2 origemImagem = Vetor(tile.GetProperty("origem_local_centro_topo"));
        Vector2 offset = Vetor(tile.GetProperty("offset"));
        float escala = tile.GetProperty("escala").GetSingle();
        bool primeiro = true;
        foreach (Celula celula in _celulas.Values)
        {
            if (celula.Tipo == "vazio") continue;
            string id;
            if (celula.Tipo.StartsWith("plataforma_"))
            {
                int indice = ((celula.Coordenada.X + celula.Coordenada.Y) % 2 + 2) % 2;
                id = _config.GetProperty("plataformas").GetProperty("tiles")[indice].GetString()!;
            }
            else id = _config.GetProperty("tiles_por_tipo").GetProperty(celula.Tipo).GetString()!;
            Vector2 centro = Centro(celula);
            Node2D suporte = new() { Position = centro };
            AddChild(suporte);
            _visuais.Add(suporte);
            Texture2D? textura = assets.Textura(id);
            if (textura != null)
                suporte.AddChild(new Sprite2D { Texture = textura, Centered = false,
                    Position = -origemImagem * escala + offset, Scale = Vector2.One * escala });
            else
                suporte.AddChild(new Polygon2D { Polygon = new[] { new Vector2(0, -_tamanho.Y/2),
                    new Vector2(_tamanho.X/2, 0), new Vector2(0, _tamanho.Y/2), new Vector2(-_tamanho.X/2, 0) },
                    Color = Cor(celula.Tipo, celula.Coordenada), Position = offset, Scale = Vector2.One * escala });
            Vector2 dimensoes = textura?.GetSize() ?? new Vector2(_tamanho.X, _tamanho.Y + tile.GetProperty("altura_lateral").GetSingle());
            Rect2 area = new(centro - origemImagem * escala + offset, dimensoes * escala);
            Limites = primeiro ? area : Limites.Merge(area);
            primeiro = false;
        }
        foreach (JsonElement objeto in _dados.GetProperty("objetos").EnumerateArray())
        {
            JsonElement cfg = _config.GetProperty("objetos").GetProperty(objeto.GetProperty("tipo").GetString()!);
            Vector2 centro = Centro(_celulas[Coordenada(objeto.GetProperty("celula"))]);
            Node2D suporte = new() { Position = centro };
            AddChild(suporte);
            _visuais.Add(suporte);
            Texture2D? textura = assets.Textura(cfg.GetProperty("id_visual").GetString()!);
            Vector2 origemObjeto = Vetor(cfg.GetProperty("origem_px"));
            Vector2 ajuste = Vetor(cfg.GetProperty("offset"));
            float escalaObjeto = cfg.GetProperty("escala").GetSingle();
            if (textura != null)
                suporte.AddChild(new Sprite2D { Texture = textura, Centered = false,
                    Position = -origemObjeto * escalaObjeto + ajuste, Scale = Vector2.One * escalaObjeto });
            else
            {
                suporte.AddChild(new Line2D { Points = new[] { Vector2.Zero, new Vector2(0, -_tamanho.Y) }, Width = 2, DefaultColor = Colors.White });
                suporte.AddChild(new Polygon2D { Polygon = new[] { new Vector2(0, -_tamanho.Y),
                    new Vector2(_tamanho.X/3, -_tamanho.Y*0.75f), new Vector2(0, -_tamanho.Y/2) }, Color = Colors.Gold });
            }
        }
    }

    private static Color Cor(string tipo, Vector2I celula) => tipo switch
    {
        "fita_verde" => new Color("1ebc73"), "fita_azul" => new Color("4d9be6"),
        "marcador_carmim" => new Color("c32454"), "marcador_roxo" => new Color("a884f3"),
        "marcador_turquesa" => new Color("30e1b9"), "marcador_oliva" => new Color("92a984"),
        _ => (celula.X + celula.Y) % 2 == 0 ? new Color("1ebc73") : new Color("4d9be6")
    };
}
