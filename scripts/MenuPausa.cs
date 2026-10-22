#nullable enable
using Godot;

public partial class MenuPausa : CanvasLayer
{
    private Control? _focoAnterior;
    private Button _continuar = null!;
    private Label _aviso = null!;
    private bool _pausado;

    public override void _Ready()
    {
        _continuar = GetNode<Button>("tela/centralizador/conteudo/continuar");
        _aviso = GetNode<Label>("tela/centralizador/conteudo/aviso");
        _continuar.Pressed += Continuar;
        GetNode<Button>("tela/centralizador/conteudo/seletor").Pressed += () => SairDaFase(false);
        GetNode<Button>("tela/centralizador/conteudo/menu").Pressed += () => SairDaFase(true);
        Hide();
    }

    public override void _Input(InputEvent evento)
    {
        if (!evento.IsActionPressed(Acoes.Pausar) || evento.IsEcho()) return;
        GetViewport().SetInputAsHandled();
        if (_pausado) Continuar();
        else Pausar();
    }

    private void Pausar()
    {
        if (_pausado) return;
        _focoAnterior = GetViewport().GuiGetFocusOwner();
        _focoAnterior?.ReleaseFocus();
        GetViewport().GuiCancelDrag();
        _pausado = true;
        GetTree().Paused = true;
        _aviso.Text = "Ao sair da fase, os reparos desta visita serao descartados.\nAs fases concluidas permanecem salvas.";
        Show();
        _continuar.GrabFocus();
    }

    private void Continuar()
    {
        if (!_pausado) return;
        _pausado = false;
        Hide();
        GetTree().Paused = false;
        if (IsInstanceValid(_focoAnterior) && _focoAnterior!.IsVisibleInTree())
            _focoAnterior.GrabFocus();
        _focoAnterior = null;
    }

    private void SairDaFase(bool menuPrincipal)
    {
        SceneRouter roteador = GetNode<SceneRouter>("/root/SceneRouter");
        Error erro = menuPrincipal ? roteador.IrParaMenu() : roteador.IrParaSeletorFases();
        if (erro != Error.Ok) _aviso.Text = "Nao foi possivel trocar de tela. Tente novamente ou continue a fase.";
    }

    public override void _ExitTree()
    {
        if (_pausado) GetTree().Paused = false;
    }
}
