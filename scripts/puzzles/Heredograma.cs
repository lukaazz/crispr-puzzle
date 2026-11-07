#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class Heredograma : CanvasLayer
{
    [Signal] public delegate void PuzzleConcluidoEventHandler(bool sucesso);
    [Export(PropertyHint.Range, "1,10,1")] public int Dificuldade { get; set; } = 1;
    private const string ArquivoDados = "res://data/heredogramas.json";
    private const string CenaPeca = "res://scenes/componentes/peca_heredograma.tscn";
    private const string CenaBotao = "res://scenes/componentes/botao_visual.tscn";
    private const int TamanhoPeca = 48;
    private const float RaioAtracao = 64;
    private const float EspacamentoEncaixes = 8;
    private PecaHeredograma? _selecionada;
    private AudioStreamPlayer _somClique = null!;
    private const int ColunasBandeja = 5;
    private const int PassoBandeja = 64;
    private readonly List<PecaHeredograma> _pecas = new();
    private readonly Dictionary<PecaHeredograma, string> _solucao = new();
    private Control _conteudo = null!;
    private Label _mensagem = null!;
    private string _explicacao = "";
    private bool _resolvido;
    private bool _concluido;

    public Func<List<JsonElement>, JsonElement>? SelecionarConteudo { get; set; }

    public void Setup(int dificuldade)
    {
        Dificuldade = Mathf.Clamp(dificuldade, 1, 10);
        if (IsNodeReady()) Montar();
    }

    public override void _Ready() => Montar();

    private void Montar()
    {
        if (_conteudo != null) { RemoveChild(_conteudo); _conteudo.QueueFree(); }
        _conteudo = new Control();
        AddChild(_conteudo);
        _selecionada = null;
        _somClique = new AudioStreamPlayer();
        _conteudo.AddChild(_somClique);
        _pecas.Clear();
        _solucao.Clear();
        _resolvido = false;
        _concluido = false;
        CriarBotao("Sair do puzzle", new(940, 640), () => Concluir(false));
        _mensagem = Texto("", new(40, 556), new(1180, 76));
        try
        {
            using JsonDocument documento = JsonDocument.Parse(FileAccess.GetFileAsString(ArquivoDados));
            JsonElement? escolhido = SelecionarModelo(documento.RootElement);
            if (!escolhido.HasValue)
            {
                _mensagem.Text = "Nenhum heredograma disponivel para esta dificuldade. Saia do puzzle.";
                return;
            }
            JsonElement modelo = escolhido.Value;
            Texto(modelo.GetProperty("titulo").GetString()!, new(40, 24), new(1180, 32));
            Texto(modelo.GetProperty("enunciado").GetString()!, new(40, 66), new(1180, 90));
            Texto("Clique numa peca e depois num +. Clique numa peca colocada para devolver. Esc/direito cancela. Arrastar tambem funciona.\nQuadrado: homem | Circulo: mulher | Preenchido: afetado", new(40, 158), new(1180, 52));
            _explicacao = modelo.GetProperty("explicacao").GetString()!;
            int numero = 0;
            foreach (JsonElement encaixe in modelo.GetProperty("encaixes").EnumerateArray())
            {
                string tipo = encaixe.GetProperty("tipo").GetString()!;
                Vector2 posicao = new(encaixe.GetProperty("x").GetSingle(), encaixe.GetProperty("y").GetSingle());
                Vector2 tamanho = new(encaixe.GetProperty("largura").GetSingle(), encaixe.GetProperty("altura").GetSingle());
                PecaHeredograma destino = CriarPeca(posicao, tamanho, "");
                // Reserva uma area exclusiva, inclusive para linhas estreitas.
                bool sobreposto;
                do
                {
                    sobreposto = false;
                    foreach (PecaHeredograma anterior in _solucao.Keys)
                    {
                        if (!destino.GetRect().Grow(EspacamentoEncaixes / 2)
                            .Intersects(anterior.GetRect().Grow(EspacamentoEncaixes / 2))) continue;
                        destino.Position += new Vector2(PecaHeredograma.AreaCliqueMinima + EspacamentoEncaixes, 0);
                        sobreposto = true;
                        break;
                    }
                } while (sobreposto);
                posicao += destino.Position - (posicao + (tamanho - destino.Size) / 2);
                _solucao.Add(destino, tipo);
                string rotulo = encaixe.GetProperty("rotulo").GetString()!;
                if (rotulo.Length > 0) Texto(rotulo, posicao + new Vector2(-16, tamanho.Y + 2), new(100, 26));
                // Bandeja em ordem fixa; nenhuma geracao ou distribuicao aleatoria.
                CriarPeca(new(850 + numero % ColunasBandeja * PassoBandeja,
                    244 + numero / ColunasBandeja * PassoBandeja), new(TamanhoPeca, TamanhoPeca), tipo);
                numero++;
            }
            Texto("PECAS", new(850, 212), new(330, 28));
            CriarBotao("Verificar", new(40, 640), Verificar);
            CriarBotao("Continuar", new(340, 640), () => { if (_resolvido) Concluir(true); });
        }
        catch (Exception erro) when (erro is JsonException || erro is InvalidOperationException || erro is KeyNotFoundException || erro is IndexOutOfRangeException)
        {
            _mensagem.Text = "Nao foi possivel carregar o heredograma. Saia do puzzle.";
            GD.PushWarning($"Heredograma invalido: {erro.Message}");
        }
    }

    private JsonElement? SelecionarModelo(JsonElement modelos)
    {
        if (modelos.ValueKind != JsonValueKind.Array) return null;
        List<JsonElement> candidatos = new();
        int maiorNivel = 0;
        int dificuldade = Mathf.Clamp(Dificuldade, 1, 10);
        foreach (JsonElement modelo in modelos.EnumerateArray())
        {
            if (modelo.ValueKind != JsonValueKind.Object ||
                !modelo.TryGetProperty("dificuldade", out JsonElement valor) ||
                valor.ValueKind != JsonValueKind.Number || !valor.TryGetInt32(out int nivel) ||
                nivel < 1 || nivel > dificuldade || nivel < maiorNivel) continue;
            if (nivel > maiorNivel) candidatos.Clear();
            candidatos.Add(modelo.Clone());
            maiorNivel = nivel;
        }
        if (candidatos.Count == 0) return null;
        return SelecionarConteudo != null ? SelecionarConteudo(candidatos) : candidatos[GD.RandRange(0, candidatos.Count - 1)];
    }

    private Label Texto(string texto, Vector2 posicao, Vector2 tamanho)
    {
        Label legenda = new() { Text = texto, Position = posicao, Size = tamanho,
            AutowrapMode = TextServer.AutowrapMode.WordSmart, MouseFilter = Control.MouseFilterEnum.Ignore };
        _conteudo.AddChild(legenda);
        return legenda;
    }

    private void CriarBotao(string texto, Vector2 posicao, Action acao)
    {
        BotaoVisual botao = GD.Load<PackedScene>(CenaBotao).Instantiate<BotaoVisual>();
        botao.Rotulo = texto;
        botao.Position = posicao;
        botao.CustomMinimumSize = new(280, 56);
        _conteudo.AddChild(botao);
        botao.Pressed += acao;
    }

    private PecaHeredograma CriarPeca(Vector2 posicao, Vector2 tamanho, string tipo)
    {
        PecaHeredograma peca = GD.Load<PackedScene>(CenaPeca).Instantiate<PecaHeredograma>();
        peca.TamanhoDesenho = tamanho;
        peca.Size = new(PecaHeredograma.AreaCliqueMinima, PecaHeredograma.AreaCliqueMinima);
        peca.Position = posicao + (tamanho - peca.Size) / 2;
        _conteudo.AddChild(peca);
        peca.DefinirTipo(tipo);
        peca.Clicada += Clicar;
        peca.ArrastoIniciado += origem => Selecionar(null);
        peca.Solta += Soltar;
        _pecas.Add(peca);
        return peca;
    }

    private void Selecionar(PecaHeredograma? peca)
    {
        _selecionada = peca;
        foreach (PecaHeredograma item in _pecas)
        {
            item.Selecionada = item == peca;
            item.Destacada = peca != null && _solucao.ContainsKey(item) && item.Tipo.Length == 0;
        }
    }

    private void TocarClique()
    {
        _somClique.Stream = GetNode<Assets>("/root/Assets").Som("sfx/clique");
        if (_somClique.Stream != null) _somClique.Play();
    }

    public override void _Input(InputEvent evento)
    {
        if (evento.IsActionPressed(Acoes.Pausar) ||
            evento is InputEventMouseButton { ButtonIndex: MouseButton.Right, Pressed: true })
        {
            Selecionar(null);
            foreach (PecaHeredograma peca in _pecas) peca.CancelarGesto();
            GetViewport().SetInputAsHandled();
        }
    }

    private void Devolver(PecaHeredograma peca)
    {
        if (!_solucao.ContainsKey(peca) || peca.Tipo.Length == 0) return;
        PecaHeredograma? livre = _pecas.Find(item => !_solucao.ContainsKey(item) && item.Tipo.Length == 0);
        if (livre == null) return;
        livre.DefinirTipo(peca.Tipo);
        peca.DefinirTipo("");
    }

    private void Colocar(PecaHeredograma origem, PecaHeredograma destino)
    {
        Devolver(destino);
        destino.DefinirTipo(origem.Tipo);
        origem.DefinirTipo("");
        Selecionar(null);
        _mensagem.Text = "";
        TocarClique();
    }

    private void Clicar(PecaHeredograma peca)
    {
        if (_resolvido || _concluido) return;
        if (_solucao.ContainsKey(peca))
        {
            if (_selecionada != null) Colocar(_selecionada, peca);
            else Devolver(peca);
        }
        else if (peca.Tipo.Length > 0)
        {
            Selecionar(_selecionada == peca ? null : peca);
            if (_selecionada != null) TocarClique();
        }
    }

    private void Soltar(PecaHeredograma origem, Vector2 posicao)
    {
        if (_resolvido || _concluido) return;
        PecaHeredograma? destino = null;
        float distancia = RaioAtracao;
        foreach (PecaHeredograma encaixe in _solucao.Keys)
        {
            if (encaixe.Tipo.Length > 0 && encaixe != origem) continue;
            float atual = posicao.DistanceTo(encaixe.GlobalPosition + encaixe.Size / 2);
            if (atual <= distancia) { distancia = atual; destino = encaixe; }
        }
        if (destino == origem) return;
        if (destino != null) Colocar(origem, destino);
        else Devolver(origem);
    }

    private void Verificar()
    {
        if (_resolvido || _concluido || _solucao.Count == 0) return;
        foreach (KeyValuePair<PecaHeredograma, string> encaixe in _solucao)
            if (encaixe.Key.Tipo != encaixe.Value)
            {
                _mensagem.Text = "Ainda ha pecas ausentes ou incorretas. Confira pessoas, preenchimentos e ligacoes e tente novamente.";
                return;
            }
        _resolvido = true;
        Selecionar(null);
        foreach (PecaHeredograma peca in _pecas) peca.Bloqueada = true;
        _mensagem.Text = "Correto! " + _explicacao + " Clique em Continuar.";
    }

    private void Concluir(bool sucesso)
    {
        if (_concluido) return;
        _concluido = true;
        EmitSignal(SignalName.PuzzleConcluido, sucesso);
    }
}
