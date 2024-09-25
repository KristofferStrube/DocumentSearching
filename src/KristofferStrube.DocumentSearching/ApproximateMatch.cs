namespace KristofferStrube.DocumentSearching;

public readonly record struct ApproximateMatch(int Position, EditType[] ExpandedGigar, int Edits);
