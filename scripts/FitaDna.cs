#nullable enable
using Godot;
using System;
using System.Collections.Generic;

public partial class FitaDna : Node2D
{
    public const float Margem = 160.0f;
    public const float AlturaCaminho = 360.0f;
    private const float PassoPar = 80.0f;
    private const float DeslocamentoBase = 80.0f;
    private const int TamanhoFonte = 28;
    private const int TamanhoContorno = 6;
    private static readonly Vector2 TamanhoBase = new(64, 64);
    private static readonly Vector2 TamanhoTrecho = new(80, 224);
    private static readonly Dictionary<char, char> Complementos = new()
    {
        ['A'] = 'T', ['T'] = 'A', ['G'] = 'C', ['C'] = 'G'
    };
    private static readonly Dictionary<char, Color> Cores = new()
    {
        ['A'] = new(0.15f, 0.6f, 0.35f), ['T'] = new(0.75f, 0.32f, 0.18f),
        ['G'] = new(0.7f, 0.53f, 0.08f), ['C'] = new(0.15f, 0.4f, 0.8f)
    };
    private readonly List<TrechoDanificado> _trechos = new();
    private PackedScene _cenaVisual = null!;

    public static bool SequenciaValida(string sequencia)
    {
        if (string.IsNullOrEmpty(sequencia)) return false;
        foreach (char letra in sequencia)
            if (!Complementos.ContainsKey(letra)) return false;
        return true;
    }

    public float Gerar(string sequencia, int quantidade, int quantidadeDanos)
    {
        _cenaVisual = GD.Load<PackedScene>("res://scenes/componentes/visual_slot.tscn");
        PackedScene cenaTrecho = GD.Load<PackedScene>("res://scenes/trecho_danificado.tscn");
        quantidadeDanos = Mathf.Clamp(quantidadeDanos, 0, quantidade - 2);
        HashSet<int> indicesDanos = new();
        for (int dano = 0; dano < quantidadeDanos; dano++)
        {
            float indiceDano = (float)((dano + 1) * (quantidade - 1)) / (quantidadeDanos + 1);
            // GDScript roundi afasta os empates de zero; Mathf.RoundToInt usa o par mais proximo.
            indicesDanos.Add((int)MathF.Round(indiceDano, MidpointRounding.AwayFromZero));
        }
        for (int indice = 0; indice < quantidade; indice++)
        {
            float x = Margem + indice * PassoPar;
            char baseAtual = sequencia[indice % sequencia.Length];
            if (indicesDanos.Contains(indice))
            {
                TrechoDanificado trecho = cenaTrecho.Instantiate<TrechoDanificado>();
                trecho.Position = new Vector2(x, AlturaCaminho);
                AddChild(trecho);
                _trechos.Add(trecho);
            }
            else
                CriarVisual("dna/trecho_saudavel", new Vector2(x, AlturaCaminho), TamanhoTrecho, new Color(0.07f, 0.19f, 0.24f));
            CriarBase(baseAtual, new Vector2(x, AlturaCaminho - DeslocamentoBase));
            CriarBase(Complementos[baseAtual], new Vector2(x, AlturaCaminho + DeslocamentoBase));
        }
        return Margem + (quantidade - 1) * PassoPar;
    }

    public bool TodosTrechosReparados()
    {
        foreach (TrechoDanificado trecho in _trechos)
            if (!trecho.Reparado) return false;
        return true;
    }

    public TrechoDanificado? TrechoProximo(float x, float distancia)
    {
        foreach (TrechoDanificado trecho in _trechos)
            if (!trecho.Reparado && Mathf.Abs(x - trecho.Position.X) <= distancia) return trecho;
        return null;
    }

    private VisualSlot CriarVisual(string id, Vector2 centro, Vector2 tamanho, Color cor)
    {
        VisualSlot slot = _cenaVisual.Instantiate<VisualSlot>();
        slot.IdVisual = id;
        slot.TamanhoPadrao = tamanho;
        slot.Size = tamanho;
        slot.Position = centro - tamanho / 2.0f;
        slot.CorPlaceholder = cor;
        AddChild(slot);
        return slot;
    }

    private void CriarBase(char baseAtual, Vector2 centro)
    {
        VisualSlot slot = CriarVisual("dna/base_" + char.ToLowerInvariant(baseAtual), centro, TamanhoBase, Cores[baseAtual]);
        // A letra permanece legivel mesmo com uma imagem personalizada.
        Label letra = new()
        {
            Name = "letra", Text = baseAtual.ToString(),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        letra.AddThemeFontSizeOverride("font_size", TamanhoFonte);
        letra.AddThemeColorOverride("font_outline_color", Colors.Black);
        letra.AddThemeConstantOverride("outline_size", TamanhoContorno);
        slot.AddChild(letra);
        letra.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
    }
}
