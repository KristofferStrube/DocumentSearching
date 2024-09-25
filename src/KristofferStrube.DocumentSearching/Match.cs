namespace KristofferStrube.DocumentSearching;

public readonly struct Match
{
    public int Position { get; }
    public int Length { get; }
    public EditType[] ExpandedCigar { get; }
    public int Edits { get; }

    public Match(int position, int length)
    {
        Position = position;
        Length = length;
        ExpandedCigar = Enumerable.Range(0, length).Select(_ => EditType.Match).ToArray();
        Edits = 0;
    }

    public Match(int position, EditType[] expandedCigar, int edits)
    {
        Position = position;
        ExpandedCigar = expandedCigar;
        Length = expandedCigar.Sum(c => c is EditType.Delete ? 0 : 1);
        Edits = edits;
    }
}
