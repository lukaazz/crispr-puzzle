#nullable enable
using Godot;

public partial class PecaHeredograma : Control
{
    [Signal] public delegate void TrocadaEventHandler(PecaHeredograma origem, PecaHeredograma destino);
    private const string CenaVisual = "res://scenes/componentes/visual_slot.tscn";
    public string Tipo { get; private set; } = "";
    public bool Bloqueada { get; set; }

    public void DefinirTipo(string tipo)
    {
        Tipo = tipo;
        foreach (Node filho in GetChildren()) { RemoveChild(filho); filho.QueueFree(); }
        if (tipo.Length == 0)
        {
            Label vazio = new() { Text = "+", Size = Size, HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center, MouseFilter = MouseFilterEnum.Ignore };
            AddChild(vazio);
            return;
        }
        bool pessoa = tipo.StartsWith("homem") || tipo.StartsWith("mulher");
        bool mulher = tipo.StartsWith("mulher");
        string id = pessoa ? (mulher ? "mulher" : "homem") : tipo;
        string forma = pessoa ? (mulher ? "circulo" : "quadrado") : tipo == "linha_uniao" ? "horizontal" : "vertical";
        AdicionarVisual(id, forma);
        if (tipo.EndsWith("_afetado")) AdicionarVisual("afetado", forma + "_cheio");
        TooltipText = tipo.Replace('_', ' ');
    }

    private void AdicionarVisual(string id, string forma)
    {
        VisualSlot visual = GD.Load<PackedScene>(CenaVisual).Instantiate<VisualSlot>();
        visual.IdVisual = "heredograma/" + id;
        visual.TamanhoPadrao = Size;
        visual.Size = Size;
        visual.CorPlaceholder = Colors.White;
        visual.FormaPlaceholder = forma;
        AddChild(visual);
    }

    public override Variant _GetDragData(Vector2 posicao)
    {
        if (Bloqueada || Tipo.Length == 0) return default;
        PecaHeredograma previa = new() { Size = Size, MouseFilter = MouseFilterEnum.Ignore };
        previa.DefinirTipo(Tipo);
        SetDragPreview(previa);
        return this;
    }

    public override bool _CanDropData(Vector2 posicao, Variant dados)
    {
        return !Bloqueada && dados.VariantType == Variant.Type.Object &&
            dados.AsGodotObject() is PecaHeredograma origem && !origem.Bloqueada && origem != this;
    }

    public override void _DropData(Vector2 posicao, Variant dados)
    {
        if (_CanDropData(posicao, dados)) EmitSignal(SignalName.Trocada, dados.AsGodotObject(), this);
    }
}
