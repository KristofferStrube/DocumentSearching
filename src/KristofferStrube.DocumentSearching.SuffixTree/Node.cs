namespace KristofferStrube.DocumentSearching.SuffixTree;

public class Node
{
    public HashSet<int> Offsets { get; }
    public Node?[] Children { get; set; }

    public Node(int alphabetSize)
    {
        Children = new Node?[alphabetSize];
        Offsets = [];
    }
}
