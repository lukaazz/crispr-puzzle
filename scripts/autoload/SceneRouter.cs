using Godot;

public partial class SceneRouter : Node
{
    private const string CenaMenu = "res://scenes/menu.tscn";
    private const string CenaSeletorFases = "res://scenes/seletor_fases.tscn";
    private const string CenaFase = "res://scenes/fase.tscn";
    private const string CenaFinal = "res://scenes/tela_final.tscn";

    public Error IrParaMenu() => TrocarCena(CenaMenu);
    public Error IrParaSeletorFases() => TrocarCena(CenaSeletorFases);
    public Error IrParaFase() => TrocarCena(CenaFase);
    public Error IrParaTelaFinal() => TrocarCena(CenaFinal);

    public Error TrocarCena(string caminho)
    {
        if (!ResourceLoader.Exists(caminho, "PackedScene")) return Error.FileNotFound;
        SceneTree arvore = GetTree();
        Error erro = arvore.ChangeSceneToFile(caminho);
        if (erro == Error.Ok) arvore.Paused = false;
        return erro;
    }

    public void Sair() => GetTree().Quit();
}
