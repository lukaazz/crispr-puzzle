#nullable enable
using Godot;
using System.Collections.Generic;

public partial class BotaoBase : Button
{
    private static readonly Dictionary<string, Color> Cores = new()
    {
        ["A"] = new(0.15f, 0.6f, 0.35f), ["T"] = new(0.75f, 0.32f, 0.18f),
        ["G"] = new(0.7f, 0.53f, 0.08f), ["C"] = new(0.15f, 0.4f, 0.8f)
    };
    private const float OpacidadeResolvida = 0.35f;
    [Export] public string Base { get; set; } = "A";
    public bool Selecionada { get; set; }
    public bool Resolvida { get; set; }
    private VisualSlot _visual = null!;
    private Label _letra = null!;
    private Control _indicadorFoco = null!;

    public override void _Ready()
    {
        _visual = GetNode<VisualSlot>("visual");
        _letra = GetNode<Label>("letra");
        _indicadorFoco = GetNode<Control>("indicador_foco");
        foreach (string estado in new[] { "normal", "hover", "pressed", "disabled", "focus" })
            AddThemeStyleboxOverride(estado, new StyleBoxEmpty());
        _visual.CorPlaceholder = Cores[Base];
        _visual.IdVisual = "dna/base_" + Base.ToLowerInvariant();
        FocusEntered += Atualizar;
        FocusExited += Atualizar;
        MouseEntered += Atualizar;
        MouseExited += Atualizar;
        Atualizar();
    }

    public void Atualizar()
    {
        Disabled = Resolvida;
        Color cor = _visual.Modulate;
        cor.A = Resolvida ? OpacidadeResolvida : 1.0f;
        _visual.Modulate = cor;
        _letra.Text = Resolvida ? $"{Base}\nOK" : Selecionada ? $"[{Base}]" : Base;
        _indicadorFoco.Visible = !Resolvida && (HasFocus() || IsHovered());
        TooltipText = $"{Base}: {(Resolvida ? "pareada" : Selecionada ? "selecionada" : "disponivel")}";
    }
}
