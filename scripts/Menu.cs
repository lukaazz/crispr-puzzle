#nullable enable
using Godot;
using System.Collections.Generic;

public partial class Menu : Control
{
    private static readonly Dictionary<string, string> VisuaisBotoes = new()
    {
        ["normal"] = "ui/botao_normal",
        ["hover"] = "ui/botao_hover",
        ["pressed"] = "ui/botao_hover",
        ["disabled"] = "ui/botao_bloqueado"
    };
    private Button _botaoJogar = null!;
    private Button _botaoSair = null!;
    private Label _aviso = null!;

    public override void _Ready()
    {
        _botaoJogar = GetNode<Button>("centralizador/conteudo/jogar");
        _botaoSair = GetNode<Button>("centralizador/conteudo/sair");
        _aviso = GetNode<Label>("centralizador/conteudo/aviso");
        _botaoJogar.GrabFocus();
        GetNode<Assets>("/root/Assets").AssetsRecarregados += AtualizarBotoes;
        AtualizarBotoes();
    }

    public override void _ExitTree()
    {
        Assets? assets = GetNodeOrNull<Assets>("/root/Assets");
        if (assets != null) assets.AssetsRecarregados -= AtualizarBotoes;
    }

    private void AtualizarBotoes()
    {
        // O tema nativo permanece como placeholder de cada estado sem imagem.
        foreach (Button botao in new[] { _botaoJogar, _botaoSair })
        {
            foreach (KeyValuePair<string, string> visual in VisuaisBotoes)
            {
                botao.RemoveThemeStyleboxOverride(visual.Key);
                Texture2D? textura = GetNode<Assets>("/root/Assets").Textura(visual.Value);
                if (textura == null) continue;
                StyleBoxTexture estilo = new()
                {
                    Texture = textura,
                    AxisStretchHorizontal = StyleBoxTexture.AxisStretchMode.TileFit,
                    AxisStretchVertical = StyleBoxTexture.AxisStretchMode.TileFit
                };
                botao.AddThemeStyleboxOverride(visual.Key, estilo);
            }
        }
    }

    private void AoJogarPressed()
    {
        Error erro = GetNode<SceneRouter>("/root/SceneRouter").IrParaSeletorFases();
        if (erro == Error.FileNotFound) _aviso.Text = "O seletor de fases ainda nao esta disponivel.";
        else if (erro != Error.Ok) _aviso.Text = "Nao foi possivel abrir o seletor de fases.";
    }

    private void AoSairPressed() => GetNode<SceneRouter>("/root/SceneRouter").Sair();
}
