using System.Text.Json.Serialization;

using KristofferStrube.DocumentSearching.SearchTree;
using KristofferStrube.DocumentSearching.SuffixTrie;

namespace KristofferStrube.DocumentSearching.SuffixTree;

public class SuffixTrieSearchIndex : ISearchIndex<SuffixTrieSearchIndex>
{
    private Node Root { get; init; }
    private Alphabet Alphabet { get; init; }
    private int[] Input { get; init; }

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
        var quer = new Query(Alphabet.EncodeQuery(query));

        Node currentNode = Root;
        int s = 0;
        int x = 0;
        while (s < quer.Length())
        {
            if (currentNode.Match(x) && quer.TheEnd(s, currentNode))
            {
                return [];
            }

            if (!currentNode.Match(x) && !quer.Match(Input, s, x))
            {
                return [];
            }

            if (currentNode.Match(x))
            {
                currentNode = quer.GetNode(currentNode, s);
                x = currentNode.From;
            }
            else
            {
                s++;
                x++;
            }
        }
        return Alphabet.GetOffsetsForSubtree(currentNode).ToArray();
    }

    public ApproximateMatch[] ApproximateSearch(string query, int edits)
    {
        int[] encodedQuery = Alphabet.EncodeQuery(query);

        List<ApproximateMatch> results = [];

        Stack<EditSubTree> subTrees = new();
        subTrees.Push(new(Root, 0, [], 0, edits));
        while (subTrees.TryPop(out EditSubTree subTree))
        {
            if (subTree.OutSideEdge() || subTree.EndOfInput(Input))
            {
            }
            else if (subTree.Match(encodedQuery))
            {
                results.AddRange(subTree.Results(Input, Alphabet, edits, results));
            }
            else
            {
                foreach(var t in subTree.NoResults(Input, encodedQuery))
                    subTrees.Push(t);
            }
        }

        return results.Distinct().ToArray();
    }

    private readonly record struct EditSubTree(Node node, int offset, List<EditType> expandedGigar, int offsetInQuery, int editsLeft)
    {
        public bool Match(int[] encodedQuery)
        {
            return offsetInQuery == encodedQuery.Length;
        }

        public bool EndOfInput(int[] input)
        {
            return node.From + offset == input.Length;
        }

        public bool OutSideEdge()
        {
            return offset > node.To - node.From;
        }

        public List<EditSubTree> NoResults(int[] input, int[] encodedQuery)
        {
            List<EditSubTree> editTree = [];
            if (EndOfLine())
            {
                editTree.AddRange(HandleEndofLine(input, encodedQuery));
            }
            else if (EndOfLineNoMatch(input, encodedQuery))
            {
                editTree.Add(HadnleEolNoMatch());
            }
            else if (NotEolButNoMatch()) // We are not at the end of a line, but we don't match.
            {
                editTree.AddRange(HandleNotEolNoMatch());
            }

            return editTree;
        }

        private IEnumerable<EditSubTree> HandleNotEolNoMatch()
        {
            yield return new(node, offset + 1, [.. expandedGigar, EditType.MisMatch], offsetInQuery + 1, editsLeft - 1);
            yield return new(node, offset + 1, [.. expandedGigar, EditType.Insert], offsetInQuery, editsLeft - 1);
            yield return new(node, offset, [.. expandedGigar, EditType.Delete], offsetInQuery + 1, editsLeft - 1);
        }

        private bool NotEolButNoMatch()
        {
            return editsLeft is not 0;
        }

        private EditSubTree HadnleEolNoMatch()
        {
            return new(node, offset + 1, [.. expandedGigar, EditType.Match], offsetInQuery + 1, editsLeft);
        }

        private bool EndOfLineNoMatch(int[] input, int[] encodedQuery)
        {
            return input[node.From + offset] ==
                   encodedQuery[offsetInQuery];
        }

        private IEnumerable<EditSubTree> HandleEndofLine(int[] input, int[] encodedQuery)
        {
            int encodedCharacter = encodedQuery[offsetInQuery];
            if (encodedCharacter > -1 && node.Children[encodedCharacter] is { } matchingChild)
            {
                yield return new(matchingChild, 1, [.. expandedGigar, EditType.Match], offsetInQuery + 1,
                    editsLeft);
            }

            if (editsLeft is not 0)
            {
                foreach (Node? child in node.Children)
                {
                    if (child is null)
                    {
                        continue;
                    }

                    if (input[child.From] is not 0) // We should not continue if this child is starting with a sentinel.
                    {
                        yield return new(child, 1, [.. expandedGigar, EditType.Insert], offsetInQuery,
                            editsLeft - 1);
                    }

                    yield return new(child, 1, [.. expandedGigar, EditType.MisMatch], offsetInQuery + 1,
                        editsLeft - 1);
                }

                yield return new(node, offset, [.. expandedGigar, EditType.Delete], offsetInQuery + 1,
                    editsLeft - 1);
            }
        }

        private bool EndOfLine()
        {
            return offset == node.To - node.From;
        }

        public List<ApproximateMatch> Results(int[] input, Alphabet alphabet, int edits, List<ApproximateMatch> results)
        {
            List<int> matches = alphabet.GetOffsetsForSubtree(node);
            foreach (int match in matches)
            {
                if (match != input.Length - 1) // We don't want to match on the sentinel in case the input was deleted.
                {
                    results.Add(new(match, expandedGigar.ToArray(), edits - editsLeft));
                }
            }

            return results;
        }
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

public class Query
{
    private int[] Value { get; }

    public Query(int[] value)
    {
        Value = value;
    }

    public bool TheEnd(int s, Node currentNode)
        => Value[s] == -1 || currentNode.Children[Value[s]] is null;

    public Node? GetNode(Node currentNode, int s)
        => currentNode.Children[Value[s]];

    public int Length()
        => Value.Length;

    public bool Match(int[] input, int s, int x)
        => Value[s] == input[x];
}
