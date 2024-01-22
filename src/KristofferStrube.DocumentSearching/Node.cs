using System.Text.Json.Serialization;

namespace KristofferStrube.DocumentSearching;

public class Node
{
    public int? Label { get; set; }
    public int From { get; set; }
    public int To { get; set; }
    public Node Parent { get; set; }
    public Node?[] Children { get; set; }

    [Obsolete("Only use for serialization")]
    [JsonConstructor]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Node() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public Node(int from, int to, Node parent, int alphabetSize, int? label = null)
    {
        From = from;
        To = to;
        Parent = parent;
        Children = new Node?[alphabetSize];
        Label = label;
    }
}
