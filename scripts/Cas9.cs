#nullable enable
using Godot;

public partial class Cas9 : CharacterBody2D
{
    [Export] public float Velocidade { get; set; } = 280.0f;
    public float LimiteEsquerdo { get; set; }
    public float LimiteDireito { get; set; }
    public float AlturaCaminho { get; set; }
    private VisualSlot _visual = null!;

    public override void _Ready() => _visual = GetNode<VisualSlot>("visual");

    public void ConfigurarCaminho(float inicio, float fim, float altura)
    {
        LimiteEsquerdo = inicio;
        LimiteDireito = fim;
        AlturaCaminho = altura;
        Position = new Vector2(inicio, altura);
    }

    public void DefinirMovimentoAtivo(bool ativo)
    {
        SetPhysicsProcess(ativo);
        Velocity = Vector2.Zero;
        _visual.DefinirAnimacaoAtiva(false);
    }

    public override void _PhysicsProcess(double delta)
    {
        float xAnterior = Position.X;
        float direcao = Input.GetAxis(Acoes.MoverEsq, Acoes.MoverDir);
        Velocity = new Vector2(direcao * Velocidade, 0);
        MoveAndSlide();
        Position = new Vector2(Mathf.Clamp(Position.X, LimiteEsquerdo, LimiteDireito), AlturaCaminho);
        _visual.DefinirAnimacaoAtiva(!Mathf.IsEqualApprox(Position.X, xAnterior));
    }
}
