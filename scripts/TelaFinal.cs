#nullable enable
using Godot;

public partial class TelaFinal : Control
{
    private Label _aviso = null!;

    public override void _Ready()
    {
        _aviso = GetNode<Label>("centralizador/conteudo/aviso");
        GameState estado = GetNode<GameState>("/root/GameState");
        GetNode<Label>("centralizador/conteudo/titulo").Text = estado.TodasFasesConcluidas
            ? "Parabens! Jornada concluida!" : "Jornada em andamento";
        GetNode<Label>("centralizador/conteudo/resumo").Text = estado.TodasFasesConcluidas
            ? $"Voce concluiu as {estado.TotalFases} fases do CRISPR Puzzle!\nSeu progresso foi salvo. Voce pode revisitar as fases pelo seletor."
            : "Conclua todas as fases para finalizar sua jornada pelo DNA.";
        Button seletor = GetNode<Button>("centralizador/conteudo/seletor");
        seletor.Pressed += () => Navegar(false);
        GetNode<Button>("centralizador/conteudo/menu").Pressed += () => Navegar(true);
        seletor.GrabFocus();
    }

    private void Navegar(bool menuPrincipal)
    {
        SceneRouter roteador = GetNode<SceneRouter>("/root/SceneRouter");
        Error erro = menuPrincipal ? roteador.IrParaMenu() : roteador.IrParaSeletorFases();
        if (erro != Error.Ok) _aviso.Text = "Nao foi possivel trocar de tela. Tente novamente.";
    }
}
