#nullable enable
using Godot;
using System.Collections.Generic;
using System.Text.Json;

public partial class Assets : Node
{
    [Signal] public delegate void AssetsRecarregadosEventHandler();

    private const string PastaSprites = "res://assets/sprites/";
    private const string PastaAudio = "res://assets/audio/";
    private const string ArquivoVisuais = "res://data/visuais.json";
    private static readonly string[] ExtensoesAudio = { ".ogg", ".wav", ".mp3" };
    private readonly Dictionary<string, Texture2D?> _texturas = new();
    private readonly Dictionary<string, AudioStream?> _sons = new();
    private Godot.Collections.Dictionary _visuais = new();

    public override void _Ready() => Recarregar();

    public Texture2D? Textura(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (_texturas.TryGetValue(id, out Texture2D? existente)) return existente;
        Godot.Collections.Dictionary configuracao = ConfiguracaoVisual(id);
        string arquivo = configuracao.TryGetValue("arquivo", out Variant valor) ? valor.ToString() : id + ".png";
        string caminho = PastaSprites + arquivo;
        Texture2D? imagem = null;
        if (CaminhoRelativoValido(arquivo) && ResourceLoader.Exists(caminho, "Texture2D"))
            imagem = ResourceLoader.Load(caminho, "Texture2D", ResourceLoader.CacheMode.Replace) as Texture2D;
        _texturas[id] = imagem;
        return imagem;
    }

    public AudioStream? Som(string id)
    {
        if (!CaminhoRelativoValido(id)) return null;
        if (_sons.TryGetValue(id, out AudioStream? existente)) return existente;
        AudioStream? audio = null;
        foreach (string extensao in ExtensoesAudio)
        {
            string caminho = PastaAudio + id + extensao;
            if (!ResourceLoader.Exists(caminho, "AudioStream")) continue;
            audio = ResourceLoader.Load(caminho, "AudioStream", ResourceLoader.CacheMode.Replace) as AudioStream;
            if (audio != null) break;
        }
        _sons[id] = audio;
        return audio;
    }

    public Godot.Collections.Dictionary ConfiguracaoVisual(string id)
    {
        return _visuais.TryGetValue(id, out Variant entrada) && entrada.VariantType == Variant.Type.Dictionary
            ? entrada.AsGodotDictionary() : new();
    }

    public float EscalaVisual(string id)
    {
        if (ConfiguracaoVisual(id).TryGetValue("escala", out Variant valor) && Numero(valor))
        {
            double escala = valor.AsDouble();
            if (double.IsFinite(escala) && escala > 0) return (float)escala;
        }
        return 1.0f;
    }

    public Vector2 DeslocamentoVisual(string id)
    {
        if (ConfiguracaoVisual(id).TryGetValue("offset", out Variant valor) && valor.VariantType == Variant.Type.Array)
        {
            Godot.Collections.Array valores = valor.AsGodotArray();
            if (valores.Count == 2 && Numero(valores[0]) && Numero(valores[1]))
            {
                Vector2 deslocamento = new((float)valores[0].AsDouble(), (float)valores[1].AsDouble());
                if (deslocamento.IsFinite()) return deslocamento;
            }
        }
        return Vector2.Zero;
    }

    public void Recarregar()
    {
        _texturas.Clear();
        _sons.Clear();
        _visuais.Clear();
        if (FileAccess.FileExists(ArquivoVisuais))
        {
            try
            {
                using JsonDocument documento = JsonDocument.Parse(FileAccess.GetFileAsString(ArquivoVisuais));
                if (documento.RootElement.ValueKind == JsonValueKind.Object)
                    _visuais = ConverterJson(documento.RootElement).AsGodotDictionary();
                else
                    GD.PushWarning("visuais.json invalido; usando os caminhos e ajustes padrao.");
            }
            catch (JsonException)
            {
                GD.PushWarning("visuais.json invalido; usando os caminhos e ajustes padrao.");
            }
        }
        EmitSignal(SignalName.AssetsRecarregados);
    }

    private static bool Numero(Variant valor) => valor.VariantType is Variant.Type.Int or Variant.Type.Float;

    private static bool CaminhoRelativoValido(string caminho) =>
        !string.IsNullOrEmpty(caminho) && !caminho.StartsWith('/') && !caminho.Contains("..") &&
        !caminho.Contains(':') && !caminho.Contains('\\');

    // Mantem o Dictionary/Array de Variants esperado pelos consumidores GDScript.
    private static Variant ConverterJson(JsonElement elemento)
    {
        switch (elemento.ValueKind)
        {
            case JsonValueKind.Object:
                Godot.Collections.Dictionary objeto = new();
                foreach (JsonProperty propriedade in elemento.EnumerateObject())
                    objeto[propriedade.Name] = ConverterJson(propriedade.Value);
                return objeto;
            case JsonValueKind.Array:
                Godot.Collections.Array lista = new();
                foreach (JsonElement item in elemento.EnumerateArray()) lista.Add(ConverterJson(item));
                return lista;
            case JsonValueKind.String: return elemento.GetString()!;
            case JsonValueKind.Number: return elemento.GetDouble();
            case JsonValueKind.True: return true;
            case JsonValueKind.False: return false;
            default: return default;
        }
    }
}
