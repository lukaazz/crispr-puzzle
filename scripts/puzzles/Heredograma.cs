#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class Heredograma : CanvasLayer
{
    [Signal] public delegate void PuzzleConcluidoEventHandler(bool sucesso);
    [Export(PropertyHint.Range, "1,10,1")] public int Dificuldade { get; set; } = 1;
    private const string ArquivoDados = "res://data/heredogramas.json";
    private const string CenaPeca = "res://scenes/componentes/peca_heredograma.tscn";
    private const string CenaBotao = "res://scenes/componentes/botao_visual.tscn";
    private const int TamanhoPeca = 48;
    private const int ColunasBandeja = 5;
    private const int PassoBandeja = 64;
    private readonly List<PecaHeredograma> _pecas = new();
    private readonly Dictionary<PecaHeredograma, string> _solucao = new();
    private Control _conteudo = null!;
    private Label _mensagem = null!;
    private string _explicacao = "";
    private bool _resolvido;
    private bool _concluido;

    public void Setup(int dificuldade)
    {
        Dificuldade = Mathf.Clamp(dificuldade, 1, 10);
        if (IsNodeReady()) Montar();
    }

    public override void _Ready() => Montar();

    private void Montar()
    {
        if (_conteudo != null) { RemoveChild(_conteudo); _conteudo.QueueFree(); }
        _conteudo = new Control();
        AddChild(_conteudo);
        _pecas.Clear();
        _solucao.Clear();
        _resolvido = false;
        _concluido = false;
        CriarBotao("Sair do puzzle", new(940, 640), () => Concluir(false));
        _mensagem = Texto("", new(40, 556), new(1180, 76));
        try
        {
            using JsonDocument documento = JsonDocument.Parse(FileAccess.GetFileAsString(ArquivoDados));
            JsonElement? escolhido = SelecionarModelo(documento.RootElement);
            if (!escolhido.HasValue)
            {
                _mensagem.Text = "Nenhum heredograma disponivel para esta dificuldade. Saia do puzzle.";
                return;
            }
            JsonElement modelo = escolhido.Value;
            Texto(modelo.GetProperty("titulo").GetString()!, new(40, 24), new(1180, 32));
            Texto(modelo.GetProperty("enunciado").GetString()!, new(40, 66), new(1180, 90));
            Texto("Arraste as pecas para os +. Soltar sobre outra peca troca suas posicoes.\nQuadrado: homem | Circulo: mulher | Preenchido: afetado", new(40, 158), new(1180, 52));
            _explicacao = modelo.GetProperty("explicacao").GetString()!;
            int numero = 0;
            foreach (JsonElement encaixe in modelo.GetProperty("encaixes").EnumerateArray())
            {
                string tipo = encaixe.GetProperty("tipo").GetString()!;
                Vector2 posicao = new(encaixe.GetProperty("x").GetSingle(), encaixe.GetProperty("y").GetSingle());
                Vector2 tamanho = new(encaixe.GetProperty("largura").GetSingle(), encaixe.GetProperty("altura").GetSingle());
                PecaHeredograma destino = CriarPeca(posicao, tamanho, "");
                _solucao.Add(destino, tipo);
                string rotulo = encaixe.GetProperty("rotulo").GetString()!;
                if (rotulo.Length > 0) Texto(rotulo, posicao + new Vector2(-16, tamanho.Y + 2), new(100, 26));
                // Bandeja em ordem fixa; nenhuma geracao ou distribuicao aleatoria.
                CriarPeca(new(850 + numero % ColunasBandeja * PassoBandeja,
                    244 + numero / ColunasBandeja * PassoBandeja), new(TamanhoPeca, TamanhoPeca), tipo);
                numero++;
            }
            Texto("PECAS", new(850, 212), new(330, 28));
            CriarBotao("Verificar", new(40, 640), Verificar);
            CriarBotao("Continuar", new(340, 640), () => { if (_resolvido) Concluir(true); });
        }
        catch (Exception erro) when (erro is JsonException || erro is InvalidOperationException || erro is KeyNotFoundException || erro is IndexOutOfRangeException)
        {
            _mensagem.Text = "Nao foi possivel carregar o heredograma. Saia do puzzle.";
            GD.PushWarning($"Heredograma invalido: {erro.Message}");
        }
    }

    private JsonElement? SelecionarModelo(JsonElement modelos)
    {
        if (modelos.ValueKind != JsonValueKind.Array) return null;
        JsonElement? escolhido = null;
        int maiorNivel = 0;
        int dificuldade = Mathf.Clamp(Dificuldade, 1, 10);
        foreach (JsonElement modelo in modelos.EnumerateArray())
        {
            if (modelo.ValueKind != JsonValueKind.Object ||
                !modelo.TryGetProperty("dificuldade", out JsonElement valor) ||
                valor.ValueKind != JsonValueKind.Number || !valor.TryGetInt32(out int nivel) ||
                nivel < 1 || nivel > dificuldade || nivel <= maiorNivel) continue;
            escolhido = modelo;
            maiorNivel = nivel;
        }
        return escolhido;
    }

    private Label Texto(string texto, Vector2 posicao, Vector2 tamanho)
    {
        Label legenda = new() { Text = texto, Position = posicao, Size = tamanho,
            AutowrapMode = TextServer.AutowrapMode.WordSmart, MouseFilter = Control.MouseFilterEnum.Ignore };
        _conteudo.AddChild(legenda);
        return legenda;
    }

    private void CriarBotao(string texto, Vector2 posicao, Action acao)
    {
        BotaoVisual botao = GD.Load<PackedScene>(CenaBotao).Instantiate<BotaoVisual>();
        botao.Rotulo = texto;
        botao.Position = posicao;
        botao.CustomMinimumSize = new(280, 56);
        _conteudo.AddChild(botao);
        botao.Pressed += acao;
    }

    private PecaHeredograma CriarPeca(Vector2 posicao, Vector2 tamanho, string tipo)
    {
        PecaHeredograma peca = GD.Load<PackedScene>(CenaPeca).Instantiate<PecaHeredograma>();
        peca.Position = posicao;
        peca.Size = tamanho;
        _conteudo.AddChild(peca);
        peca.DefinirTipo(tipo);
        peca.Trocada += Trocar;
        _pecas.Add(peca);
        return peca;
    }

    private void Trocar(PecaHeredograma origem, PecaHeredograma destino)
    {
        if (_resolvido || _concluido || !_pecas.Contains(origem) || !_pecas.Contains(destino)) return;
        string anterior = destino.Tipo;
        destino.DefinirTipo(origem.Tipo);
        origem.DefinirTipo(anterior);
        _mensagem.Text = "";
    }

    private void Verificar()
    {
        if (_resolvido || _concluido || _solucao.Count == 0) return;
        foreach (KeyValuePair<PecaHeredograma, string> encaixe in _solucao)
            if (encaixe.Key.Tipo != encaixe.Value)
            {
                _mensagem.Text = "Ainda ha pecas ausentes ou incorretas. Confira pessoas, preenchimentos e ligacoes e tente novamente.";
                return;
            }
        _resolvido = true;
        foreach (PecaHeredograma peca in _pecas) peca.Bloqueada = true;
        _mensagem.Text = "Correto! " + _explicacao + " Clique em Continuar.";
    }

    private void Concluir(bool sucesso)
    {
        if (_concluido) return;
        _concluido = true;
        EmitSignal(SignalName.PuzzleConcluido, sucesso);
    }
}
