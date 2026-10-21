#nullable enable
using Godot;

public partial class VisualSlot : Control
{
    private static readonly StringName AnimacaoPadrao = "padrao";
    private const double QuadrosPorSegundo = 8.0;
    private string _idVisual = "";
    private bool _animacaoAtiva = true;
    private ColorRect _placeholder = null!;
    private Label _legenda = null!;
    private Sprite2D _imagem = null!;
    private AnimatedSprite2D _animacao = null!;

    [Export]
    public string IdVisual
    {
        get => _idVisual;
        set
        {
            _idVisual = value;
            if (IsNodeReady()) AtualizarVisual();
        }
    }
    [Export] public Vector2 TamanhoPadrao { get; set; } = new(64, 64);
    [Export] public Color CorPlaceholder { get; set; } = new(0.3f, 0.5f, 0.6f, 1.0f);
    [Export] public string TextoPlaceholder { get; set; } = "";
    // Vazio procura <id>_1, _2...; Cas9 tambem procura cas9_andando_1...
    [Export] public string IdAnimacao { get; set; } = "";

    public override void _Ready()
    {
        _placeholder = GetNode<ColorRect>("placeholder");
        _legenda = GetNode<Label>("legenda");
        _imagem = GetNode<Sprite2D>("imagem");
        _animacao = GetNode<AnimatedSprite2D>("animacao");
        MouseFilter = MouseFilterEnum.Ignore;
        CustomMinimumSize = TamanhoPadrao;
        GetNode<Assets>("/root/Assets").AssetsRecarregados += AtualizarVisual;
        Resized += AjustarImagem;
        _animacao.FrameChanged += AjustarImagem;
        AtualizarVisual();
    }

    public override void _ExitTree()
    {
        Assets? assets = GetNodeOrNull<Assets>("/root/Assets");
        if (assets != null) assets.AssetsRecarregados -= AtualizarVisual;
    }

    private void AtualizarVisual()
    {
        // O foco dos botoes pode mudar enquanto a cena esta saindo da arvore.
        if (!IsInsideTree()) return;
        Assets assets = GetNode<Assets>("/root/Assets");
        _animacao.Stop();
        _animacao.SpriteFrames = null;
        _imagem.Texture = assets.Textura(IdVisual);
        string prefixo = IdAnimacao;
        if (string.IsNullOrEmpty(prefixo))
            prefixo = IdVisual == "player/cas9" ? IdVisual + "_andando" : IdVisual;
        SpriteFrames quadros = new();
        quadros.AddAnimation(AnimacaoPadrao);
        quadros.SetAnimationSpeed(AnimacaoPadrao, QuadrosPorSegundo);
        for (int indice = 1; !string.IsNullOrEmpty(IdVisual); indice++)
        {
            Texture2D? quadro = assets.Textura($"{prefixo}_{indice}");
            if (quadro == null) break;
            quadros.AddFrame(AnimacaoPadrao, quadro);
        }
        _animacao.Visible = quadros.GetFrameCount(AnimacaoPadrao) > 0;
        if (_animacao.Visible)
        {
            _animacao.SpriteFrames = quadros;
            if (_animacaoAtiva) _animacao.Play(AnimacaoPadrao);
            else _animacao.Animation = AnimacaoPadrao;
        }
        _imagem.Visible = !_animacao.Visible && _imagem.Texture != null;
        _placeholder.Color = CorPlaceholder;
        _placeholder.Visible = !_imagem.Visible && !_animacao.Visible;
        _legenda.Text = TextoPlaceholder;
        _legenda.Visible = _placeholder.Visible && !string.IsNullOrEmpty(TextoPlaceholder);
        AjustarImagem();
    }

    public void DefinirAnimacaoAtiva(bool ativa)
    {
        if (_animacaoAtiva == ativa) return;
        _animacaoAtiva = ativa;
        if (_animacao.SpriteFrames == null) return;
        if (ativa) _animacao.Play(AnimacaoPadrao);
        else _animacao.Pause();
    }

    private void AjustarImagem()
    {
        if (!IsInsideTree()) return;
        Texture2D? texturaAtual = _imagem.Texture;
        if (_animacao.Visible && _animacao.SpriteFrames != null)
            texturaAtual = _animacao.SpriteFrames.GetFrameTexture(_animacao.Animation, _animacao.Frame);
        if (texturaAtual == null) return;
        Assets assets = GetNode<Assets>("/root/Assets");
        Vector2 dimensoes = texturaAtual.GetSize();
        float ajuste = Mathf.Min(Size.X / dimensoes.X, Size.Y / dimensoes.Y);
        Vector2 escala = Vector2.One * ajuste * assets.EscalaVisual(IdVisual);
        Vector2 centro = Size / 2.0f + assets.DeslocamentoVisual(IdVisual);
        _imagem.Scale = escala;
        _imagem.Position = centro;
        _animacao.Scale = escala;
        _animacao.Position = centro;
    }
}
