#nullable enable
using Godot;

public partial class TrechoDanificado : StaticBody2D
{
    public bool Reparado { get; private set; }
    private VisualSlot _visual = null!;
    private CollisionShape2D _colisao = null!;

    public override void _Ready()
    {
        _visual = GetNode<VisualSlot>("visual");
        _colisao = GetNode<CollisionShape2D>("colisao");
    }

    public void Reparar()
    {
        if (Reparado) return;
        Reparado = true;
        _colisao.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        _visual.CorPlaceholder = new Color(0.12f, 0.45f, 0.3f);
        _visual.TextoPlaceholder = "OK";
        _visual.IdVisual = "dna/trecho_reparado";
    }
}
