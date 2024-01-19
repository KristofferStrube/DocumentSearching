namespace KristofferStrube.DocumentSearching;

public readonly struct Match
{
    public int Position { get; }
    public int Length { get; }

    public Match(int position, int length)
    {
        Position = position;
        Length = length;
    }
}
