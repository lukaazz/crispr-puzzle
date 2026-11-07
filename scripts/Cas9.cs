#nullable enable
using Godot;

public partial class Cas9 : CharacterBody2D
{
    [Export] public float Velocidade { get; set; } = 280.0f;
    private MapaIsometrico? _mapa;
    private VisualSlot _visual = null!;

    public override void _Ready() => _visual = GetNode<VisualSlot>("visual");

    public void ConfigurarMapa(MapaIsometrico mapa)
    {
        _mapa = mapa;
        Position = mapa.Spawn;
        _visual.DefinirAnimacaoAtiva(false);
    }

    public void DefinirMovimentoAtivo(bool ativo)
    {
        SetPhysicsProcess(ativo);
        Velocity = Vector2.Zero;
        _visual.DefinirAnimacaoAtiva(false);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_mapa == null) return;
        Vector2 anterior = Position;
        Vector2 direcao = Input.GetVector(Acoes.MoverEsq, Acoes.MoverDir, Acoes.MoverCima, Acoes.MoverBaixo);
        Velocity = new Vector2(direcao.X, direcao.Y * _mapa.ProporcaoVertical) * Velocidade;
        Position = _mapa.Mover(Position, Velocity * (float)delta);
        Vector2 movimento = Position - anterior;
        _visual.DefinirAnimacaoAtiva(!movimento.IsZeroApprox());
        if (!Mathf.IsZeroApprox(movimento.X)) _visual.DefinirEspelhamento(movimento.X < 0);
    }
}
