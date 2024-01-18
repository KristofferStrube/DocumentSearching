namespace KristofferStrube.DocumentSearching.SuffixTree;

public class Node
{
    public int? Label { get; }
    public int From { get; set; }
    public int To { get; }
    public Node Parent { get; set; }
    public Node?[] Children { get; set; }

    public Node(int from, int to, Node parent, int alphabetSize, int? label = null)
    {
        From = from;
        To = to;
        Parent = parent;
        Children = new Node?[alphabetSize];
        Label = label;
    }
}
