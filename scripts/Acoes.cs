using Godot;

public static class Acoes
{
    // StringName nao pode ser const em C#; readonly preserva os identificadores.
    public static readonly StringName MoverEsq = "mover_esq";
    public static readonly StringName MoverDir = "mover_dir";
    public static readonly StringName Interagir = "interagir";
    public static readonly StringName Pausar = "pausar";
}
