using Godot;
using Godot.Collections;

public partial class GameState : Node
{
    [Signal] public delegate void ProgressoAlteradoEventHandler();

    private const string CaminhoSave = "user://save.cfg";
    private const string Secao = "progresso";
    private const int NumeroPrimeiraFase = 1;
    private const int QuantidadeFases = 10;

    // Propriedades de instancia tambem ficam acessiveis aos consumidores GDScript.
    public int PrimeiraFase => NumeroPrimeiraFase;
    public int TotalFases => QuantidadeFases;
    public int FaseAtual { get; set; } = NumeroPrimeiraFase;
    public Array<int> FasesLiberadas { get; set; } = new() { NumeroPrimeiraFase };
    public Array<int> FasesConcluidas { get; set; } = new();
    public bool TodasFasesConcluidas
    {
        get
        {
            for (int numero = PrimeiraFase; numero <= TotalFases; numero++)
                if (!FasesConcluidas.Contains(numero)) return false;
            return true;
        }
    }

    public override void _Ready() => CarregarProgresso();

    public bool SelecionarFase(int numero)
    {
        if (!FasesLiberadas.Contains(numero)) return false;
        FaseAtual = numero;
        SalvarProgresso();
        EmitSignal(SignalName.ProgressoAlterado);
        return true;
    }

    public void ConcluirFase(int numero)
    {
        if (numero < PrimeiraFase || numero > TotalFases || !FasesLiberadas.Contains(numero)) return;
        if (!FasesConcluidas.Contains(numero)) FasesConcluidas.Add(numero);
        if (numero < TotalFases && !FasesLiberadas.Contains(numero + 1)) FasesLiberadas.Add(numero + 1);
        FasesConcluidas.Sort();
        FasesLiberadas.Sort();
        SalvarProgresso();
        EmitSignal(SignalName.ProgressoAlterado);
    }

    public void SalvarProgresso()
    {
        using ConfigFile configuracao = new();
        configuracao.SetValue(Secao, "fase_atual", FaseAtual);
        configuracao.SetValue(Secao, "fases_liberadas", FasesLiberadas);
        configuracao.SetValue(Secao, "fases_concluidas", FasesConcluidas);
        Error erro = configuracao.Save(CaminhoSave);
        if (erro != Error.Ok) GD.PushWarning($"Nao foi possivel salvar o progresso: {erro}");
    }

    public void CarregarProgresso()
    {
        FaseAtual = PrimeiraFase;
        FasesLiberadas = new() { PrimeiraFase };
        FasesConcluidas.Clear();
        using ConfigFile configuracao = new();
        Error erro = configuracao.Load(CaminhoSave);
        if (erro != Error.Ok)
        {
            if (erro != Error.FileNotFound) GD.PushWarning($"Nao foi possivel carregar o progresso: {erro}");
            return;
        }

        Variant concluidas = configuracao.GetValue(Secao, "fases_concluidas", new Array());
        if (concluidas.VariantType == Variant.Type.Array)
        {
            // Reconstrui os desbloqueios a partir das conclusoes consecutivas validas.
            Array lista = concluidas.AsGodotArray();
            for (int numero = PrimeiraFase; numero <= TotalFases; numero++)
            {
                if (!lista.Contains(numero)) break;
                FasesConcluidas.Add(numero);
                if (numero < TotalFases) FasesLiberadas.Add(numero + 1);
            }
        }
        Variant atual = configuracao.GetValue(Secao, "fase_atual", PrimeiraFase);
        if (atual.VariantType == Variant.Type.Int && atual.AsInt64() >= PrimeiraFase &&
            atual.AsInt64() <= TotalFases && FasesLiberadas.Contains(atual.AsInt32()))
            FaseAtual = atual.AsInt32();
        EmitSignal(SignalName.ProgressoAlterado);
    }
}
