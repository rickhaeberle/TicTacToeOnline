public abstract class Player
{
    public float Id { get; set; }

    public string Name { get; set; }
    public char Marker { get; set; }

    public abstract int Play(Board board);

}
