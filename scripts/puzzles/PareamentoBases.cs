#nullable enable
using Godot;
using System.Collections.Generic;

public partial class PareamentoBases : CanvasLayer
{
    [Signal] public delegate void PuzzleConcluidoEventHandler(bool sucesso);
    private const string CenaBase = "res://scenes/componentes/botao_base.tscn";
    private const int DificuldadeMinima = 1;
    private const int DificuldadeMaxima = 10;
    private const int ParesAdicionais = 2;
    private static readonly string[] Bases = { "A", "T", "G", "C" };
    private static readonly Dictionary<string, string> Complementos = new()
    {
        ["A"] = "T", ["T"] = "A", ["G"] = "C", ["C"] = "G"
    };
    private int _quantidadePares = DificuldadeMinima + ParesAdicionais;
    private int _acertos;
    private bool _concluido;
    private BotaoBase? _esquerda;
    private BotaoBase? _direita;
    private readonly List<BotaoBase> _botoesEsquerda = new();
    private GridContainer _gradeEsquerda = null!;
    private GridContainer _gradeDireita = null!;
    private Label _progresso = null!;

    public void Setup(int dificuldade)
    {
        _quantidadePares = Mathf.Clamp(dificuldade, DificuldadeMinima, DificuldadeMaxima) + ParesAdicionais;
        if (IsNodeReady()) GerarPares();
    }

    public override void _Ready()
    {
        _gradeEsquerda = GetNode<GridContainer>("tela/centralizador/conteudo/listas/esquerda/grade");
        _gradeDireita = GetNode<GridContainer>("tela/centralizador/conteudo/listas/direita/grade");
        _progresso = GetNode<Label>("tela/centralizador/conteudo/progresso");
        GetNode<Button>("tela/centralizador/conteudo/sair").Pressed += Sair;
        GerarPares();
    }

    private void GerarPares()
    {
        foreach (GridContainer grade in new[] { _gradeEsquerda, _gradeDireita })
            foreach (Node filho in grade.GetChildren())
            {
                grade.RemoveChild(filho);
                filho.QueueFree();
            }
        _botoesEsquerda.Clear();
        _esquerda = null;
        _direita = null;
        _acertos = 0;
        _concluido = false;
        Godot.Collections.Array<string> basesEsquerda = new();
        Godot.Collections.Array<string> basesDireita = new();
        for (int indice = 0; indice < _quantidadePares; indice++)
        {
            string baseAtual = Bases[GD.RandRange(0, Bases.Length - 1)];
            basesEsquerda.Add(baseAtual);
            basesDireita.Add(Complementos[baseAtual]);
        }
        basesEsquerda.Shuffle();
        basesDireita.Shuffle();
        foreach (string baseAtual in basesEsquerda)
            _botoesEsquerda.Add(CriarBotao(baseAtual, _gradeEsquerda, true));
        foreach (string baseAtual in basesDireita) CriarBotao(baseAtual, _gradeDireita, false);
        _progresso.Text = $"0 / {_quantidadePares} pares. Escolha uma base de cada lado.";
        FocarProximaBase();
    }

    private BotaoBase CriarBotao(string baseAtual, GridContainer grade, bool ladoEsquerdo)
    {
        BotaoBase botao = GD.Load<PackedScene>(CenaBase).Instantiate<BotaoBase>();
        botao.Base = baseAtual;
        grade.AddChild(botao);
        botao.Pressed += () => Selecionar(botao, ladoEsquerdo);
        return botao;
    }

    private void Selecionar(BotaoBase botao, bool ladoEsquerdo)
    {
        if (_concluido || botao.Resolvida) return;
        BotaoBase? anterior = ladoEsquerdo ? _esquerda : _direita;
        if (anterior != null)
        {
            anterior.Selecionada = false;
            anterior.Atualizar();
        }
        if (ladoEsquerdo) _esquerda = anterior != botao ? botao : null;
        else _direita = anterior != botao ? botao : null;
        botao.Selecionada = anterior != botao;
        botao.Atualizar();
        if (_esquerda == null || _direita == null) return;
        if (Complementos[_esquerda.Base] == _direita.Base)
        {
            _acertos++;
            _esquerda.Resolvida = true;
            _direita.Resolvida = true;
            _progresso.Text = $"{_acertos} / {_quantidadePares} pares. Correto!";
        }
        else
            _progresso.Text = $"{_esquerda.Base}–{_direita.Base} nao forma um par. Tente A–T ou G–C. ({_acertos} / {_quantidadePares})";
        _esquerda.Selecionada = false;
        _direita.Selecionada = false;
        _esquerda.Atualizar();
        _direita.Atualizar();
        _esquerda = null;
        _direita = null;
        if (_acertos == _quantidadePares)
        {
            _concluido = true;
            EmitSignal(SignalName.PuzzleConcluido, true);
        }
        else FocarProximaBase();
    }

    private void FocarProximaBase()
    {
        foreach (BotaoBase botao in _botoesEsquerda)
            if (!botao.Resolvida)
            {
                botao.GrabFocus();
                return;
            }
    }

    private void Sair()
    {
        if (_concluido) return;
        _concluido = true;
        EmitSignal(SignalName.PuzzleConcluido, false);
    }
}
