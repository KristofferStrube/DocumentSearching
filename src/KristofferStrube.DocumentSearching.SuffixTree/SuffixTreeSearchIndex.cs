using KristofferStrube.DocumentSearching.SearchTree;

namespace KristofferStrube.DocumentSearching.SuffixTree;

public class SuffixTreeSearchIndex : ISearchIndex
{
    private readonly Node _root;
    private readonly Alphabet _alphabet;
    private int[] _input;

    public SuffixTreeSearchIndex(string input)
    {
        _input = Alphabet.EncodeInput(input, out _alphabet);

        _root = new Node(0, 0, null, _alphabet.Size);

        for (int i = 0; i < _input.Length; i++)
        {
            AddSuffix(i);
        }
    }

    public SuffixTreeSearchIndex(string[] inputParts)
    {
        _input = Alphabet.EncodeInputParts(inputParts, out _alphabet);

        _root = new Node(0, 0, null, _alphabet.Size);

        for (int i = 0; i < _input.Length; i++)
        {
            AddSuffix(i);
        }
    }

    public static ISearchIndex Create(string[] inputParts)
    {
        return new SuffixTreeSearchIndex(inputParts);
    }

    private void AddSuffix(int offset)
    {
        Node currentNode = _root;
        int x = offset;

        while (x < _input.Length)
        {
            int nextChar = _input[x];
            Node? matchingChild = currentNode.Children[nextChar];
            if (matchingChild is null)
            {
                currentNode.Children[nextChar] = new Node(x, _input.Length, currentNode, _alphabet.Size, offset);
                return;
            }

            int s = matchingChild.From;
            while (s != matchingChild.To && x < _input.Length)
            {
                if (_input[s] != _input[x])
                {
                    Node splitNode = SplitEdge(matchingChild, s);
                    splitNode.Children[_input[x]] = new Node(x, _input.Length, splitNode, _alphabet.Size, offset);
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
        Node splitNode = new Node(node.From, splitPoint, parent, _alphabet.Size);
        splitNode.Children[_input[splitPoint]] = node;
        node.From = splitPoint;
        node.Parent = splitNode;
        parent.Children[_input[splitNode.From]] = splitNode;
        return splitNode;
    }

    public int[] ExactSearch(string query)
    {
        int[]? encodedQuery = _alphabet.EncodeQuery(query);

        if (encodedQuery is null)
        {
            return [];
        }

        Node? currentNode = _root;
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
            if (character != _input[x])
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
                for (int i = _alphabet.Size - 1; i >= 0; i--)
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
