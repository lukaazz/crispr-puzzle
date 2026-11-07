#nullable enable
using Godot;

public partial class PecaHeredograma : Control
{
    [Signal] public delegate void ClicadaEventHandler(PecaHeredograma peca);
    [Signal] public delegate void ArrastoIniciadoEventHandler(PecaHeredograma peca);
    [Signal] public delegate void SoltaEventHandler(PecaHeredograma peca, Vector2 posicao);
    public const float AreaCliqueMinima = 48;
    public const float LimiarArrasto = 8;
    public Vector2 TamanhoDesenho { get; set; }
    public bool Selecionada { get; set; }
    public bool Destacada { get; set; }
    private bool _pressionada;
    private bool _arrastando;
    private Vector2 _inicio;
    private PecaHeredograma? _previa;
    private double _tempo;
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
        visual.TamanhoPadrao = TamanhoDesenho;
        visual.Size = TamanhoDesenho;
        visual.Position = (Size - TamanhoDesenho) / 2;
        visual.CorPlaceholder = Colors.White;
        visual.FormaPlaceholder = forma;
        AddChild(visual);
    }

    public override void _GuiInput(InputEvent evento)
    {
        if (Bloqueada) return;
        if (evento is InputEventMouseButton botao && botao.ButtonIndex == MouseButton.Left && botao.Pressed)
        {
            _pressionada = true;
            _inicio = GetGlobalMousePosition();
            AcceptEvent();
        }
    }

    public override void _Input(InputEvent evento)
    {
        if (!_pressionada) return;
        if (Bloqueada) { CancelarGesto(); return; }
        if (evento is InputEventMouseMotion && Tipo.Length > 0 &&
            !_arrastando && GetGlobalMousePosition().DistanceTo(_inicio) >= LimiarArrasto)
        {
            _arrastando = true;
            _previa = new PecaHeredograma { Size = Size, TamanhoDesenho = TamanhoDesenho,
                MouseFilter = MouseFilterEnum.Ignore, ZIndex = 100 };
            GetParent().AddChild(_previa);
            _previa.DefinirTipo(Tipo);
            EmitSignal(SignalName.ArrastoIniciado, this);
        }
        if (_previa != null) _previa.GlobalPosition = GetGlobalMousePosition() - Size / 2;
        if (evento is InputEventMouseButton botao && botao.ButtonIndex == MouseButton.Left && !botao.Pressed)
        {
            bool arrastou = _arrastando;
            Vector2 posicao = GetGlobalMousePosition();
            CancelarGesto();
            if (arrastou) EmitSignal(SignalName.Solta, this, posicao);
            else if (GetGlobalRect().HasPoint(posicao)) EmitSignal(SignalName.Clicada, this);
            GetViewport().SetInputAsHandled();
        }
    }

    public void CancelarGesto()
    {
        _pressionada = false;
        _arrastando = false;
        _previa?.QueueFree();
        _previa = null;
    }

    public override void _Process(double delta)
    {
        _tempo += delta;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (Selecionada || Destacada)
        {
            float alpha = Selecionada ? 1 : 0.55f + 0.25f * Mathf.Sin((float)_tempo * 5);
            DrawRect(new Rect2(Vector2.One, Size - Vector2.One * 2),
                new Color(0.3f, 0.9f, 1, alpha), false, 3);
        }
    }
}
