#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class Pergunta : CanvasLayer
{
    [Signal] public delegate void PuzzleConcluidoEventHandler(bool sucesso);
    private const string CenaBotao = "res://scenes/componentes/botao_visual.tscn";
    private const string CaminhoPerguntas = "res://data/perguntas.json";
    private const int DificuldadeMaxima = 10;
    private static readonly Vector2 TamanhoAlternativa = new(1000, 64);
    private int _dificuldade = 1;
    private JsonElement _pergunta;
    private bool _respondida;
    private bool _acertou;
    private bool _concluido;
    private Label _enunciado = null!;
    private VBoxContainer _alternativas = null!;
    private Label _explicacao = null!;
    private BotaoVisual _continuar = null!;
    private BotaoVisual _sair = null!;
    private ScrollContainer _rolagem = null!;

    public void Setup(int dificuldade)
    {
        _dificuldade = Mathf.Clamp(dificuldade, 1, DificuldadeMaxima);
        if (IsNodeReady()) SortearPergunta();
    }

    public override void _Ready()
    {
        _enunciado = GetNode<Label>("tela/margem/conteudo/rolagem/textos/enunciado");
        _alternativas = GetNode<VBoxContainer>("tela/margem/conteudo/rolagem/textos/alternativas");
        _explicacao = GetNode<Label>("tela/margem/conteudo/rolagem/textos/explicacao");
        _continuar = GetNode<BotaoVisual>("tela/margem/conteudo/continuar");
        _sair = GetNode<BotaoVisual>("tela/margem/conteudo/sair");
        _rolagem = GetNode<ScrollContainer>("tela/margem/conteudo/rolagem");
        _continuar.Pressed += Continuar;
        _sair.Pressed += Sair;
        SortearPergunta();
    }

    private void SortearPergunta()
    {
        _respondida = false;
        _concluido = false;
        _acertou = false;
        _pergunta = default;
        _continuar.Hide();
        _sair.Show();
        _explicacao.Text = "";
        foreach (Node filho in _alternativas.GetChildren())
        {
            _alternativas.RemoveChild(filho);
            filho.QueueFree();
        }
        List<JsonElement> candidatas = CarregarPerguntas();
        if (candidatas.Count == 0)
        {
            _enunciado.Text = "Nenhuma pergunta valida disponivel para esta dificuldade.";
            _sair.GrabFocus();
            return;
        }
        _pergunta = candidatas[GD.RandRange(0, candidatas.Count - 1)];
        _enunciado.Text = _pergunta.GetProperty("enunciado").GetString()!;
        JsonElement opcoes = _pergunta.GetProperty("alternativas");
        for (int indice = 0; indice < opcoes.GetArrayLength(); indice++)
        {
            BotaoVisual botao = GD.Load<PackedScene>(CenaBotao).Instantiate<BotaoVisual>();
            botao.Rotulo = $"{indice + 1}. {opcoes[indice].GetString()}";
            botao.CustomMinimumSize = TamanhoAlternativa;
            _alternativas.AddChild(botao);
            botao.GetNode<Label>("legenda").AutowrapMode = TextServer.AutowrapMode.WordSmart;
            int opcao = indice;
            botao.Pressed += () => Responder(opcao);
            if (indice == 0) botao.GrabFocus();
        }
    }

    private List<JsonElement> CarregarPerguntas()
    {
        List<JsonElement> resultado = new();
        if (!FileAccess.FileExists(CaminhoPerguntas)) return resultado;
        try
        {
            using JsonDocument documento = JsonDocument.Parse(FileAccess.GetFileAsString(CaminhoPerguntas));
            if (documento.RootElement.ValueKind != JsonValueKind.Array) return resultado;
            int nivelEncontrado = 0;
            foreach (JsonElement entrada in documento.RootElement.EnumerateArray())
            {
                if (!EntradaValida(entrada)) continue;
                int nivel = (int)entrada.GetProperty("dificuldade").GetDouble();
                if (nivel > _dificuldade || nivel < nivelEncontrado) continue;
                if (nivel > nivelEncontrado)
                {
                    resultado.Clear();
                    nivelEncontrado = nivel;
                }
                resultado.Add(entrada.Clone());
            }
        }
        catch (JsonException) { GD.PushWarning("perguntas.json invalido."); }
        return resultado;
    }

    private static bool EntradaValida(JsonElement entrada)
    {
        if (entrada.ValueKind != JsonValueKind.Object) return false;
        foreach (string campo in new[] { "enunciado", "explicacao" })
            if (!entrada.TryGetProperty(campo, out JsonElement texto) || texto.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(texto.GetString())) return false;
        if (!entrada.TryGetProperty("alternativas", out JsonElement opcoes) ||
            opcoes.ValueKind != JsonValueKind.Array || opcoes.GetArrayLength() < 2) return false;
        foreach (JsonElement opcao in opcoes.EnumerateArray())
            if (opcao.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(opcao.GetString())) return false;
        foreach (string campo in new[] { "indice_correto", "dificuldade" })
            if (!entrada.TryGetProperty(campo, out JsonElement valor) || valor.ValueKind != JsonValueKind.Number ||
                !valor.TryGetDouble(out double numero) || !double.IsFinite(numero) || numero != Math.Floor(numero)) return false;
        double correto = entrada.GetProperty("indice_correto").GetDouble();
        double nivel = entrada.GetProperty("dificuldade").GetDouble();
        return correto >= 0 && correto < opcoes.GetArrayLength() && nivel >= 1 && nivel <= DificuldadeMaxima;
    }

    private void Responder(int indice)
    {
        if (_respondida || _concluido) return;
        _respondida = true;
        int correto = (int)_pergunta.GetProperty("indice_correto").GetDouble();
        _acertou = indice == correto;
        foreach (Node filho in _alternativas.GetChildren()) ((BotaoVisual)filho).DefinirDesabilitado(true);
        string correta = _pergunta.GetProperty("alternativas")[correto].GetString()!;
        _explicacao.Text = $"{(_acertou ? "Resposta correta!" : "Resposta incorreta.")}\nResposta correta: {correta}\n\n{_pergunta.GetProperty("explicacao").GetString()}";
        _sair.Hide();
        _continuar.Show();
        _continuar.GrabFocus();
        _rolagem.CallDeferred(ScrollContainer.MethodName.EnsureControlVisible, _explicacao);
    }

    private void Continuar()
    {
        if (!_respondida || _concluido) return;
        _concluido = true;
        EmitSignal(SignalName.PuzzleConcluido, _acertou);
    }

    private void Sair()
    {
        if (_concluido) return;
        _concluido = true;
        EmitSignal(SignalName.PuzzleConcluido, false);
    }
}
