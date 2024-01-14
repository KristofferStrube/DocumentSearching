using KristofferStrube.DocumentSearching.SearchTree;

namespace KristofferStrube.DocumentSearching.SuffixTree;

public class SuffixTreeSearchIndex : ISearchIndex
{
    private readonly Node _root;
    private readonly Alphabet _alphabet;
    private int[] _input;

    public SuffixTreeSearchIndex()
    {
        _alphabet = new();
        _root = new Node(_alphabet.Size);
        _input = [];
    }

    public SuffixTreeSearchIndex(string input)
    {
        _input = Alphabet.EncodeInput(input, out _alphabet);

        _root = new Node(_alphabet.Size);
        for (int i = 0; i < input.Length; i++)
        {
            AddSuffix(i);
        }
    }

    public void AddInputPart(string inputPart)
    {
        int[] encodedInputPart = _alphabet.AddAndEncodeInput(inputPart);

        _input = [.. _input, .. encodedInputPart];

        for (int i = _input.Length - encodedInputPart.Length; i < _input.Length; i++)
        {
            AddSuffix(i);
        }
    }

    private void AddSuffix(int offset)
    {
        Node currentNode = _root;
        for (int i = offset; i < _input.Length; i++)
        {
            int character = _input[i];
            if (currentNode.Children.Length > character && currentNode.Children[character] is { } child)
            {
                currentNode = child;
            }
            else
            {
                child = new Node(_alphabet.Size);
                if (character >= currentNode.Children.Length)
                {
                    Node?[] newChildren = new Node?[character + 1];
                    for (int j = 0; j < currentNode.Children.Length; j++)
                    {
                        newChildren[j] = currentNode.Children[j];
                    }
                    currentNode.Children = newChildren;
                }
                currentNode.Children[character] = child;
                currentNode = child;
            }
        }
        currentNode.Offsets.Add(offset);
    }

    public int[] ExactSearch(string query)
    {
        int[]? encodedQuery = _alphabet.EncodeQuery(query);

        if (encodedQuery is null)
        {
            return [];
        }

        Node currentNode = _root;
        for (int i = 0; i < encodedQuery.Length; i++)
        {
            int character = encodedQuery[i];
            if (currentNode.Children[character] is { } existingChild)
            {
                currentNode = existingChild;
            }
            else
            {
                return [];
            }
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
            offsets.AddRange(currentNode.Offsets);
            for (int i = 0; i < _alphabet.Size; i++)
            {
                if (currentNode.Children.Length > i && currentNode.Children[i] is { } existingChild)
                {
                    nodesToVisit.Push(existingChild);
                }
            }
        }

        return offsets;
    }
}
