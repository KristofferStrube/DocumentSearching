namespace KristofferStrube.DocumentSearching.SuffixTrie;

public readonly record struct ApproximateMatch(int Position, EditType[] ExpandedGigar, int Edits);
