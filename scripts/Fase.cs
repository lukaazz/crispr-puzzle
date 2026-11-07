#nullable enable
using Godot;
using System.Collections.Generic;
using System.Text.Json;

public partial class Fase : Node2D
{
    private const string ArquivoFases = "res://data/fases.json";
    private const int AlturaTela = 720;
    private const int LarguraTela = 1280;
    private const float DistanciaInteracao = 120.0f;
    private static readonly Dictionary<string, string> CenasPuzzle = new()
    {
        ["pareamento_bases"] = "res://scenes/puzzles/pareamento_bases.tscn",
        ["pergunta"] = "res://scenes/puzzles/pergunta.tscn",
        ["heredograma"] = "res://scenes/puzzles/heredograma.tscn"
    };
    private readonly List<string> _tiposPuzzle = new();
    private TrechoDanificado? _trechoEmReparo;
    private TrechoDanificado? _trechoComFalha;
    private CanvasLayer? _puzzle;
    private int _dificuldade = 1;
    private bool _conclusaoIniciada;
    private MarcadorFim? _marcador = null;
    // Legado preservado para a etapa 4; nenhuma fita antiga e instanciada.
    private FitaDna? _fita = null;
    private MapaIsometrico _mapa = null!;
    private Cas9 _jogador = null!;
    private Control _conteudo = null!;
    private Label _aviso = null!;

    public override void _Ready()
    {
        _mapa = GetNode<MapaIsometrico>("mapa");
        _jogador = GetNode<Cas9>("mapa/cas9");
        Camera2D camera = GetNode<Camera2D>("mapa/cas9/camera");
        VisualSlot fundo = GetNode<VisualSlot>("fundo/tela");
        Label titulo = GetNode<Label>("interface/conteudo/titulo");
        _aviso = GetNode<Label>("interface/conteudo/aviso");
        _conteudo = GetNode<Control>("interface/conteudo");
        GetNode<Button>("interface/conteudo/voltar").Pressed += VoltarAoSeletor;
        JsonElement? configuracao = CarregarConfiguracao();
        if (!configuracao.HasValue)
        {
            _jogador.SetPhysicsProcess(false);
            _aviso.Text = "Configuracao da fase indisponivel. Volte ao seletor.";
            return;
        }
        JsonElement dados = configuracao.Value;
        _dificuldade = LerInteiro(dados, "dificuldade", 1);
        if (dados.TryGetProperty("tipos_puzzle", out JsonElement tipos) && tipos.ValueKind == JsonValueKind.Array)
            foreach (JsonElement tipo in tipos.EnumerateArray())
                if (tipo.ValueKind == JsonValueKind.String && CenasPuzzle.ContainsKey(tipo.GetString()!) &&
                    !_tiposPuzzle.Contains(tipo.GetString()!))
                    _tiposPuzzle.Add(tipo.GetString()!);
        int numero = GetNode<GameState>("/root/GameState").FaseAtual;
        titulo.Text = $"Fase {numero} — Explore a fita de DNA";
        fundo.IdVisual = $"fundos/fundo_fase_{numero:00}";
        try
        {
            _mapa.Carregar(dados.GetProperty("mapa").GetString()!);
            _jogador.ConfigurarMapa(_mapa);
            Rect2 limites = _mapa.LimitesCamera;
            // Se o mapa for menor que a viewport, centraliza a area disponivel.
            Vector2 tamanho = limites.Size.Max(new Vector2(LarguraTela, AlturaTela));
            Vector2 inicio = limites.GetCenter() - tamanho / 2;
            camera.LimitLeft = Mathf.FloorToInt(inicio.X);
            camera.LimitTop = Mathf.FloorToInt(inicio.Y);
            camera.LimitRight = Mathf.CeilToInt(inicio.X + tamanho.X);
            camera.LimitBottom = Mathf.CeilToInt(inicio.Y + tamanho.Y);
            camera.ResetSmoothing();
        }
        catch (System.Exception erro)
        {
            GD.PushError($"Nao foi possivel carregar o mapa: {erro.Message}");
            _jogador.DefinirMovimentoAtivo(false);
            _aviso.Text = "Mapa indisponivel. Volte ao seletor.";
        }
    }

    public override void _Process(double delta)
    {
        if (_conclusaoIniciada || !_jogador.IsPhysicsProcessing()) return;
        _aviso.Text = "Explore as fitas e alcance a plataforma com a bandeira.";
        if (_mapa.TipoNaPosicao(_jogador.Position) == "plataforma_fim") AoEntrarNoFim(_jogador);
    }

    // Logica antiga sem uso nesta etapa; sera migrada para os defeitos do mapa.
    private void ProcessarTrechosAntigos()
    {
        if (_fita == null) return;
        if (_puzzle != null || _conclusaoIniciada) return;
        TrechoDanificado? trecho = _fita.TrechoProximo(_jogador.Position.X, DistanciaInteracao);
        if (trecho != null)
        {
            if (_tiposPuzzle.Count == 0)
            {
                _aviso.Text = "Esta fase nao tem tipos de puzzle validos configurados. Volte ao seletor.";
                return;
            }
            Godot.Collections.Array<InputEvent> eventos = InputMap.ActionGetEvents(Acoes.Interagir);
            string tecla = eventos.Count > 0 ? eventos[0].AsText() : Acoes.Interagir.ToString();
            _aviso.Text = trecho == _trechoComFalha
                ? $"O trecho continua danificado. Use {tecla} para tentar novamente."
                : $"Trecho danificado. Use {tecla} para abrir um puzzle.";
        }
        else if (_marcador != null)
            _aviso.Text = _fita.TodosTrechosReparados()
                ? "Caminho liberado! Alcance o marcador FIM para concluir a fase."
                : "Repare todos os trechos antes de alcancar o marcador FIM.";
    }

    private void AoEntrarNoFim(Node2D corpo)
    {
        if (corpo != _jogador || _puzzle != null || _conclusaoIniciada || (_fita != null && !_fita.TodosTrechosReparados())) return;
        _conclusaoIniciada = true;
        _jogador.DefinirMovimentoAtivo(false);
        // A troca de cena ocorre fora do callback de fisica da Area2D.
        Callable.From(ConcluirFase).CallDeferred();
    }

    private void ConcluirFase()
    {
        GameState estado = GetNode<GameState>("/root/GameState");
        estado.ConcluirFase(estado.FaseAtual);
        SceneRouter roteador = GetNode<SceneRouter>("/root/SceneRouter");
        bool campanhaConcluida = estado.FaseAtual == estado.TotalFases && estado.TodasFasesConcluidas;
        Error erro = campanhaConcluida ? roteador.IrParaTelaFinal() : roteador.IrParaSeletorFases();
        if (erro != Error.Ok)
        {
            _aviso.Text = "Fase concluida. Use Voltar ao seletor para tentar retornar.";
            _jogador.DefinirMovimentoAtivo(true);
        }
    }

    public override void _UnhandledInput(InputEvent evento)
    {
        if (_fita == null || _puzzle != null || _conclusaoIniciada || !evento.IsActionPressed(Acoes.Interagir) || evento.IsEcho()) return;
        TrechoDanificado? trecho = _fita.TrechoProximo(_jogador.Position.X, DistanciaInteracao);
        if (trecho == null || _tiposPuzzle.Count == 0) return;
        GetViewport().SetInputAsHandled();
        _trechoEmReparo = trecho;
        string tipo = _tiposPuzzle[GD.RandRange(0, _tiposPuzzle.Count - 1)];
        _puzzle = GD.Load<PackedScene>(CenasPuzzle[tipo]).Instantiate<CanvasLayer>();
        if (_puzzle is PareamentoBases pareamento)
        {
            pareamento.Setup(_dificuldade);
            pareamento.PuzzleConcluido += AoPuzzleConcluido;
        }
        else if (_puzzle is Pergunta pergunta)
        {
            pergunta.Setup(_dificuldade);
            pergunta.PuzzleConcluido += AoPuzzleConcluido;
        }
        else if (_puzzle is Heredograma heredograma)
        {
            heredograma.Setup(_dificuldade);
            heredograma.PuzzleConcluido += AoPuzzleConcluido;
        }
        _jogador.DefinirMovimentoAtivo(false);
        _conteudo.Hide();
        AddChild(_puzzle);
    }

    private void AoPuzzleConcluido(bool sucesso)
    {
        if (_puzzle == null) return;
        _trechoComFalha = sucesso ? null : _trechoEmReparo;
        if (sucesso && IsInstanceValid(_trechoEmReparo)) _trechoEmReparo!.Reparar();
        _puzzle.QueueFree();
        _puzzle = null;
        _trechoEmReparo = null;
        _conteudo.Show();
        _jogador.DefinirMovimentoAtivo(true);
        if (_marcador != null && _marcador.OverlapsBody(_jogador)) AoEntrarNoFim(_jogador);
    }

    private JsonElement? CarregarConfiguracao()
    {
        if (!FileAccess.FileExists(ArquivoFases)) return null;
        try
        {
            using JsonDocument documento = JsonDocument.Parse(FileAccess.GetFileAsString(ArquivoFases));
            if (documento.RootElement.ValueKind != JsonValueKind.Array) return null;
            foreach (JsonElement entrada in documento.RootElement.EnumerateArray())
            {
                if (entrada.ValueKind != JsonValueKind.Object ||
                    LerInteiro(entrada, "numero", 0) != GetNode<GameState>("/root/GameState").FaseAtual) continue;
                if (!entrada.TryGetProperty("mapa", out JsonElement mapa) ||
                    mapa.ValueKind != JsonValueKind.String || !FileAccess.FileExists(mapa.GetString()!)) return null;
                return entrada.Clone();
            }
        }
        catch (JsonException) { }
        return null;
    }

    private static int LerInteiro(JsonElement entrada, string campo, int padrao)
    {
        if (!entrada.TryGetProperty(campo, out JsonElement valor)) return padrao;
        // O JSON do GDScript representa numeros como float e int() os trunca.
        if (valor.ValueKind == JsonValueKind.Number && valor.TryGetDouble(out double numero)) return (int)numero;
        if (valor.ValueKind == JsonValueKind.String && int.TryParse(valor.GetString(), out int inteiro)) return inteiro;
        return padrao;
    }

    private void VoltarAoSeletor()
    {
        if (GetNode<SceneRouter>("/root/SceneRouter").IrParaSeletorFases() != Error.Ok)
            _aviso.Text = "Nao foi possivel abrir o seletor.";
    }
}
