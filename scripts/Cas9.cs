#nullable enable
using Godot;

public partial class Cas9 : CharacterBody2D
{
    private Vector2 _posicaoGrade;
    private MapaIsometrico? _mapa;
    private VisualSlot _visual = null!;

    public override void _Ready() => _visual = GetNode<VisualSlot>("visual");

    public void ConfigurarMapa(MapaIsometrico mapa)
    {
        _mapa = mapa;
        _posicaoGrade = mapa.SpawnGrade;
        Position = mapa.Projetar(_posicaoGrade);
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
        Vector2 direcao = Vector2.Zero;
        foreach (StringName acao in new[] { Acoes.MoverEsq, Acoes.MoverDir, Acoes.MoverCima, Acoes.MoverBaixo })
            direcao += _mapa.DirecaoAcao(acao) * Input.GetActionStrength(acao);
        direcao = direcao.LimitLength();
        _posicaoGrade = _mapa.Mover(_posicaoGrade, direcao * _mapa.VelocidadeGrade * (float)delta);
        Position = _mapa.Projetar(_posicaoGrade);
        Velocity = delta > 0 ? (Position - anterior) / (float)delta : Vector2.Zero;
        Vector2 movimento = Position - anterior;
        _visual.DefinirAnimacaoAtiva(!movimento.IsZeroApprox());
        if (!Mathf.IsZeroApprox(movimento.X)) _visual.DefinirEspelhamento(movimento.X < 0);
    }
}
