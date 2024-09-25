using System.Text.Json.Serialization;

using KristofferStrube.DocumentSearching.SearchTree;
using KristofferStrube.DocumentSearching.SuffixTrie;

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
                int encodedCharacter = encodedQuery[s];
                if (encodedCharacter == -1)
                {
                    return [];
                }
                else
                {
                    currentNode = currentNode.Children[encodedCharacter];
                    if (currentNode is null)
                    {
                        return [];
                    }
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

    public ApproximateMatch[] ApproximateSearch(string query, int edits)
    {
        int[]? encodedQuery = Alphabet.EncodeQuery(query);

        if (encodedQuery is null)
        {
            return [];
        }

        List<ApproximateMatch> results = [];

        Stack<EditSubTree> editTree = new();
        editTree.Push(new(Root, 0, [], 0, edits, 0));
        while (editTree.TryPop(out EditSubTree subTree))
        {
            (Node node, int offset, List<EditType> expandedGigar, int offsetInQuery, int editsLeft, int matchOffset) = subTree;

            if (offset > node.To - node.From)
            {
                continue;
            }

            if (node.From + offset == Input.Length)
            {
                continue;
            }

            if (offsetInQuery == encodedQuery.Length)
            {
                List<int> matches = GetOffsetsForSubtree(node);
                foreach (int match in matches)
                {
                    if (match == Input.Length - 1) // We don't want to match on the sentinel in case the input was deleted.
                    {
                        continue;
                    }

                    results.Add(new(match + matchOffset, expandedGigar.ToArray(), edits - editsLeft));
                }
                continue;
            }

            if (offset == node.To - node.From) // We are at the end of a line.
            {
                int encodedCharacter = encodedQuery[offsetInQuery];
                if (encodedCharacter > -1 && node.Children[encodedCharacter] is { } matchingChild)
                {
                    editTree.Push(new(matchingChild, 1, [.. expandedGigar, EditType.Match], offsetInQuery + 1, editsLeft, matchOffset));
                }
                if (editsLeft is not 0)
                {
                    foreach (Node? child in node.Children)
                    {
                        if (child is null)
                        {
                            continue;
                        }

                        editTree.Push(new(child, 0, [.. expandedGigar, EditType.Insert], offsetInQuery, editsLeft - 1, node == Root ? -1 : matchOffset));
                        editTree.Push(new(child, 1, [.. expandedGigar, EditType.MisMatch], offsetInQuery + 1, editsLeft - 1, matchOffset));
                    }
                    editTree.Push(new(node, offset, [.. expandedGigar, EditType.Delete], offsetInQuery + 1, editsLeft - 1, matchOffset));
                }
            }
            else if (Input[node.From + offset] == encodedQuery[offsetInQuery]) // We are not at the end of a line but we match.
            {
                editTree.Push(new(node, offset + 1, [.. expandedGigar, EditType.Match], offsetInQuery + 1, editsLeft, matchOffset));
            }
            else if (editsLeft is not 0) // We are not at the end of a line, but we don't match.
            {
                editTree.Push(new(node, offset + 1, [.. expandedGigar, EditType.MisMatch], offsetInQuery + 1, editsLeft - 1, matchOffset));
                editTree.Push(new(node, offset + 1, [.. expandedGigar, EditType.Insert], offsetInQuery, editsLeft - 1, matchOffset));
                editTree.Push(new(node, offset, [.. expandedGigar, EditType.Delete], offsetInQuery + 1, editsLeft - 1, matchOffset));
            }
        }

        return results.Distinct().ToArray();
    }

    private readonly record struct EditSubTree(Node node, int offset, List<EditType> expandedGigar, int offsetInQuery, int editsLeft, int matchOffset);

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

    public string Continuation(int from, char[] breakChars, out bool previousCharIsPartOfUntil)
    {
        int[] encodedBreakChars = new int[breakChars.Length + 1];

        for (int i = 0; i < breakChars.Length; i++)
        {
            encodedBreakChars[i] = Alphabet.EncodeMap.TryGetValue(breakChars[i], out int encoded) ? encoded : 0;
        }

        int index = from;
        while (!encodedBreakChars.Contains(Input[index]))
        {
            index++;
        }

        previousCharIsPartOfUntil = from is 0 || encodedBreakChars.Contains(Input[from - 1]);

        return new string(Input[from..index].Select(e => Alphabet.DecodeMap[e]).ToArray());
    }
}
