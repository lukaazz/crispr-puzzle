#nullable enable
using Godot;

public partial class BotaoVisual : Button
{
    private static readonly Color CorNormal = new(0.08f, 0.3f, 0.38f);
    private static readonly Color CorDestaque = new(0.12f, 0.5f, 0.56f);
    private static readonly Color CorBloqueado = new(0.16f, 0.19f, 0.24f);
    [Export] public string Rotulo { get; set; } = "Voltar";
    private VisualSlot _visual = null!;
    private VisualSlot _icone = null!;
    private Label _legenda = null!;
    private Control _indicadorFoco = null!;

    public override void _Ready()
    {
        _visual = GetNode<VisualSlot>("visual");
        _icone = GetNode<VisualSlot>("icone");
        _legenda = GetNode<Label>("legenda");
        _indicadorFoco = GetNode<Control>("indicador_foco");
        foreach (string estado in new[] { "normal", "hover", "pressed", "disabled", "focus" })
            AddThemeStyleboxOverride(estado, new StyleBoxEmpty());
        MouseEntered += AtualizarVisual;
        MouseExited += AtualizarVisual;
        FocusEntered += AtualizarVisual;
        FocusExited += AtualizarVisual;
        ButtonDown += AtualizarVisual;
        ButtonUp += AtualizarVisual;
        _legenda.Text = Rotulo;
        AtualizarVisual();
    }

    public void ConfigurarFase(int numero, bool liberada, bool concluida)
    {
        Disabled = !liberada;
        Rotulo = $"Fase {numero}";
        string estado = !liberada ? "Bloqueada" : concluida ? "Concluida" : "Disponivel";
        _legenda.Text = $"{Rotulo}\n{estado}";
        TooltipText = $"{Rotulo}: {estado}";
        _icone.Visible = !liberada || concluida;
        _icone.TextoPlaceholder = !liberada ? "X" : "OK";
        _icone.CorPlaceholder = !liberada ? CorBloqueado : CorDestaque;
        _icone.IdVisual = !liberada ? "ui/icone_cadeado" : "ui/icone_fase_concluida";
        AtualizarVisual();
    }

    public void DefinirDesabilitado(bool valor)
    {
        Disabled = valor;
        AtualizarVisual();
    }

    private void AtualizarVisual()
    {
        if (!IsInsideTree()) return;
        if (Disabled)
        {
            _visual.CorPlaceholder = CorBloqueado;
            _visual.IdVisual = "ui/botao_bloqueado";
        }
        else if (IsHovered() || HasFocus() || ButtonPressed)
        {
            _visual.CorPlaceholder = CorDestaque;
            _visual.IdVisual = "ui/botao_hover";
        }
        else
        {
            _visual.CorPlaceholder = CorNormal;
            _visual.IdVisual = "ui/botao_normal";
        }
        _indicadorFoco.Visible = HasFocus();
    }
}
