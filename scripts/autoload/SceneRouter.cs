using Godot;

public partial class SceneRouter : Node
{
    private const string CenaMenu = "res://scenes/menu.tscn";
    private const string CenaSeletorFases = "res://scenes/seletor_fases.tscn";
    private const string CenaFase = "res://scenes/fase.tscn";

    public Error IrParaMenu() => TrocarCena(CenaMenu);
    public Error IrParaSeletorFases() => TrocarCena(CenaSeletorFases);
    public Error IrParaFase() => TrocarCena(CenaFase);

    public Error TrocarCena(string caminho)
    {
        if (!ResourceLoader.Exists(caminho, "PackedScene")) return Error.FileNotFound;
        return GetTree().ChangeSceneToFile(caminho);
    }

    public void Sair() => GetTree().Quit();
}
