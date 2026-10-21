#nullable enable
using Godot;
using System.Collections.Generic;

public partial class SeletorFases : Control
{
    private const string CenaBotao = "res://scenes/componentes/botao_visual.tscn";
    private readonly List<BotaoVisual> _botoes = new();
    private Label _aviso = null!;

    public override void _Ready()
    {
        GridContainer grade = GetNode<GridContainer>("centralizador/conteudo/grade");
        _aviso = GetNode<Label>("centralizador/conteudo/aviso");
        Button voltar = GetNode<Button>("centralizador/conteudo/voltar");
        GameState estado = GetNode<GameState>("/root/GameState");
        PackedScene cenaBotao = GD.Load<PackedScene>(CenaBotao);
        for (int numero = estado.PrimeiraFase; numero <= estado.TotalFases; numero++)
        {
            BotaoVisual botao = cenaBotao.Instantiate<BotaoVisual>();
            botao.Name = $"fase_{numero:00}";
            grade.AddChild(botao);
            int fase = numero;
            botao.Pressed += () => SelecionarFase(fase);
            _botoes.Add(botao);
        }
        estado.ProgressoAlterado += AtualizarProgresso;
        voltar.Pressed += VoltarAoMenu;
        AtualizarProgresso();
        _botoes[estado.FaseAtual - estado.PrimeiraFase].GrabFocus();
    }

    public override void _ExitTree()
    {
        GameState? estado = GetNodeOrNull<GameState>("/root/GameState");
        if (estado != null) estado.ProgressoAlterado -= AtualizarProgresso;
    }

    private void AtualizarProgresso()
    {
        GameState estado = GetNode<GameState>("/root/GameState");
        for (int indice = 0; indice < _botoes.Count; indice++)
        {
            int numero = indice + estado.PrimeiraFase;
            _botoes[indice].ConfigurarFase(numero,
                estado.FasesLiberadas.Contains(numero), estado.FasesConcluidas.Contains(numero));
        }
    }

    private void SelecionarFase(int numero)
    {
        if (!GetNode<GameState>("/root/GameState").SelecionarFase(numero)) return;
        Error erro = GetNode<SceneRouter>("/root/SceneRouter").IrParaFase();
        if (erro == Error.FileNotFound)
            _aviso.Text = $"Fase {numero} selecionada. A cena da fase ainda nao esta disponivel.";
        else if (erro != Error.Ok) _aviso.Text = $"Nao foi possivel abrir a fase {numero}.";
    }

    private void VoltarAoMenu()
    {
        if (GetNode<SceneRouter>("/root/SceneRouter").IrParaMenu() != Error.Ok)
            _aviso.Text = "Nao foi possivel voltar ao menu.";
    }
}
