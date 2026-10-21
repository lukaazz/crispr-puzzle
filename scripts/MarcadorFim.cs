using Godot;

public partial class MarcadorFim : Area2D
{
    [Signal] public delegate void CorpoChegouEventHandler(Node2D corpo);

    public override void _Ready() => BodyEntered += AoEntrar;

    private void AoEntrar(Node2D corpo) => EmitSignal(SignalName.CorpoChegou, corpo);
}
