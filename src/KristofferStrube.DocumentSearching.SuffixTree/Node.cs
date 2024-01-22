using System.Text.Json.Serialization;

using KristofferStrube.DocumentSearching.SearchTree;

namespace KristofferStrube.DocumentSearching.SuffixTree;

public class Node
{
    public int? Label { get; set; }
    public int From { get; set; }
    public int To { get; set; }
    public Node Parent { get; set; }
    public Node?[] Children { get; set; }

    [Obsolete("Only use for serialization")]
    [JsonConstructor]
    public Node() { }

    public Node(int from, int to, Node parent, int alphabetSize, int? label = null)
    {
        From = from;
        To = to;
        Parent = parent;
        Children = new Node?[alphabetSize];
        Label = label;
    }
}
