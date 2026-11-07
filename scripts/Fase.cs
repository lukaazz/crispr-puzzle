#nullable enable
using Godot;
using System.Collections.Generic;
using System.Text.Json;

public partial class Fase : Node2D
{
    private const string ArquivoFases = "res://data/fases.json";
    private const int AlturaTela = 720;
    private const int LarguraTela = 1280;
    private static readonly Dictionary<string, string> CenasPuzzle = new()
    {
        ["pareamento_bases"] = "res://scenes/puzzles/pareamento_bases.tscn",
        ["pergunta"] = "res://scenes/puzzles/pergunta.tscn",
        ["heredograma"] = "res://scenes/puzzles/heredograma.tscn"
    };
    private readonly List<string> _tiposPuzzle = new();
    private readonly Dictionary<string, double> _pesos = new();
    private readonly Dictionary<string, HashSet<string>> _historicos = new();
    private string? _ultimoTipo;
    private string? _trechoEmReparo;
    private string? _trechoComFalha;
    private CanvasLayer? _puzzle;
    private int _dificuldade = 1;
    private bool _conclusaoIniciada;
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
        foreach (string tipo in _tiposPuzzle.ToArray())
        {
            double peso = dados.GetProperty("pesos").GetProperty(tipo).GetDouble();
            if (double.IsFinite(peso) && peso > 0) _pesos[tipo] = peso;
            else _tiposPuzzle.Remove(tipo);
        }
        int numero = GetNode<GameState>("/root/GameState").FaseAtual;
        titulo.Text = $"Fase {numero} — Explore a fita de DNA";
        fundo.IdVisual = $"fundos/fundo_fase_{numero:00}";
        try
        {
            _mapa.Carregar(dados.GetProperty("mapa").GetString()!, dados.GetProperty("quantidade_defeitos").GetInt32());
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
        string? proximo = _mapa.DefeitoProximo(_jogador.Position);
        if (proximo != null) _aviso.Text = proximo == _trechoComFalha ? "Interagir ? tentar novamente" : "Interagir ? reparar DNA";
        if (_mapa.TipoNaPosicao(_jogador.Position) == "plataforma_fim") AoEntrarNoFim(_jogador);
    }

    private void AoEntrarNoFim(Node2D corpo)
    {
        if (corpo != _jogador || _puzzle != null || _conclusaoIniciada) return;
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
        if (_puzzle != null || _conclusaoIniciada || !evento.IsActionPressed(Acoes.Interagir) || evento.IsEcho()) return;
        string? trecho = _mapa.DefeitoProximo(_jogador.Position);
        if (trecho == null || _tiposPuzzle.Count == 0) return;
        GetViewport().SetInputAsHandled();
        _trechoEmReparo = trecho;
        string tipo = SortearTipo();
        _puzzle = GD.Load<PackedScene>(CenasPuzzle[tipo]).Instantiate<CanvasLayer>();
        if (_puzzle is PareamentoBases pareamento)
        {
            pareamento.Setup(_dificuldade);
            pareamento.PuzzleConcluido += AoPuzzleConcluido;
        }
        else if (_puzzle is Pergunta pergunta)
        {
            pergunta.SelecionarConteudo = opcoes => SortearConteudo("pergunta", opcoes);
            pergunta.Setup(_dificuldade);
            pergunta.PuzzleConcluido += AoPuzzleConcluido;
        }
        else if (_puzzle is Heredograma heredograma)
        {
            heredograma.SelecionarConteudo = opcoes => SortearConteudo("heredograma", opcoes);
            heredograma.Setup(_dificuldade);
            heredograma.PuzzleConcluido += AoPuzzleConcluido;
        }
        _jogador.DefinirMovimentoAtivo(false);
        _conteudo.Hide();
        AddChild(_puzzle);
    }

    private string SortearTipo()
    {
        List<string> opcoes = _tiposPuzzle.FindAll(t => _tiposPuzzle.Count == 1 || t != _ultimoTipo);
        double total = 0;
        foreach (string tipo in opcoes) total += _pesos[tipo];
        double sorteio = GD.Randf() * total;
        string escolhido = opcoes[opcoes.Count - 1];
        foreach (string tipo in opcoes)
        {
            sorteio -= _pesos[tipo];
            if (sorteio < 0) { escolhido = tipo; break; }
        }
        _ultimoTipo = escolhido;
        return escolhido;
    }

    private JsonElement SortearConteudo(string tipo, List<JsonElement> candidatos)
    {
        string chave = tipo + ":" + _dificuldade;
        if (!_historicos.TryGetValue(chave, out HashSet<string>? usados))
            _historicos[chave] = usados = new();
        List<JsonElement> disponiveis = candidatos.FindAll(c => !usados.Contains(c.GetRawText()));
        if (disponiveis.Count == 0) { usados.Clear(); disponiveis = candidatos; }
        JsonElement escolhido = disponiveis[GD.RandRange(0, disponiveis.Count - 1)];
        usados.Add(escolhido.GetRawText());
        return escolhido;
    }

    private void AoPuzzleConcluido(bool sucesso)
    {
        if (_puzzle == null) return;
        _trechoComFalha = sucesso ? null : _trechoEmReparo;
        if (sucesso && _trechoEmReparo != null) _mapa.Reparar(_trechoEmReparo);
        _puzzle.QueueFree();
        _puzzle = null;
        _trechoEmReparo = null;
        _conteudo.Show();
        _jogador.DefinirMovimentoAtivo(true);
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
