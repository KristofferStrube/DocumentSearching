using System.Text.Json.Serialization;
using KristofferStrube.DocumentSearching.SearchTree;

namespace KristofferStrube.DocumentSearching.SuffixTree;

public class SuffixTrieSearchIndex : ISearchIndex<SuffixTrieSearchIndex>
{
    public Node Root { get; init; }
    public Alphabet Alphabet { get; init; }
    public int[] Input { get; init; }

    [Obsolete("Only use for serialization")]
    [JsonConstructor]
    public SuffixTrieSearchIndex() { }

    public SuffixTrieSearchIndex(string input)
    {
        Input = Alphabet.EncodeInput(input, out Alphabet alphabet);
        Alphabet = alphabet;

        Root = new Node(0, 0, null, Alphabet.Size);

        for (int i = 0; i < Input.Length; i++)
        {
            AddSuffix(i);
        }
    }

    public SuffixTrieSearchIndex(string[] inputParts)
    {
        Input = Alphabet.EncodeInputParts(inputParts, out Alphabet alphabet);
        Alphabet = alphabet;

        Root = new Node(0, 0, null, Alphabet.Size);

        for (int i = 0; i < Input.Length; i++)
        {
            AddSuffix(i);
        }
    }

    public static SuffixTrieSearchIndex Create(string[] inputParts)
    {
        return new SuffixTrieSearchIndex(inputParts);
    }

    private void AddSuffix(int offset)
    {
        Node currentNode = Root;
        int x = offset;

        while (x < Input.Length)
        {
            int nextChar = Input[x];
            Node? matchingChild = currentNode.Children[nextChar];
            if (matchingChild is null)
            {
                currentNode.Children[nextChar] = new Node(x, Input.Length, currentNode, Alphabet.Size, offset);
                return;
            }

            int s = matchingChild.From;
            while (s != matchingChild.To && x < Input.Length)
            {
                if (Input[s] != Input[x])
                {
                    Node splitNode = SplitEdge(matchingChild, s);
                    splitNode.Children[Input[x]] = new Node(x, Input.Length, splitNode, Alphabet.Size, offset);
                    return;
                }
                s++;
                x++;
            }
            currentNode = matchingChild;
        }
    }

    private Node SplitEdge(Node node, int splitPoint)
    {
        Node parent = node.Parent;
        Node splitNode = new Node(node.From, splitPoint, parent, Alphabet.Size);
        splitNode.Children[Input[splitPoint]] = node;
        node.From = splitPoint;
        node.Parent = splitNode;
        parent.Children[Input[splitNode.From]] = splitNode;
        return splitNode;
    }

    public int[] ExactSearch(string query)
    {
        int[]? encodedQuery = Alphabet.EncodeQuery(query);

        if (encodedQuery is null)
        {
            return [];
        }

        Node? currentNode = Root;
        int s = 0;
        int x = 0;
        while (s < encodedQuery.Length)
        {
            int character = encodedQuery[s];
            if (x == currentNode?.To)
            {
                currentNode = currentNode.Children[encodedQuery[s]];
                if (currentNode is null)
                {
                    return [];
                }
                x = currentNode.From;
                continue;
            }
            if (character != Input[x])
            {
                return [];
            }

            s++;
            x++;
        }
        if (currentNode is null)
        {
            return [];
        }

        return GetOffsetsForSubtree(currentNode).ToArray();
    }

    private List<int> GetOffsetsForSubtree(Node node)
    {
        List<int> offsets = [];

        Stack<Node> nodesToVisit = new();
        nodesToVisit.Push(node);

        while (nodesToVisit.TryPop(out Node? currentNode))
        {
            if (currentNode.Label is { } label)
            {
                offsets.Add(label);
            }
            else
            {
                for (int i = 0; i < Alphabet.Size; i++)
                {
                    if (currentNode.Children.Length > i && currentNode.Children[i] is { } existingChild)
                    {
                        nodesToVisit.Push(existingChild);
                    }
                }
            }
        }

        return offsets;
    }
}
