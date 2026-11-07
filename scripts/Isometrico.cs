using Godot;

public static class Isometrico
{
    public static Vector2 CelulaParaTela(Vector2 celula, Vector2 origem, Vector2 tamanho) =>
        origem + new Vector2((celula.X - celula.Y) * tamanho.X / 2,
            (celula.X + celula.Y) * tamanho.Y / 2);

    public static Vector2 TelaParaCelula(Vector2 tela, Vector2 origem, Vector2 tamanho)
    {
        Vector2 deslocamento = (tela - origem) / (tamanho / 2);
        return new Vector2((deslocamento.X + deslocamento.Y) / 2,
            (deslocamento.Y - deslocamento.X) / 2);
    }
}
