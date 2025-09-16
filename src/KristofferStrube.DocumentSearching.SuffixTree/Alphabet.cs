using System.Text.Json.Serialization;

using KristofferStrube.DocumentSearching.SuffixTree;

namespace KristofferStrube.DocumentSearching.SearchTree;

public class Alphabet
{
    public Dictionary<char, int> EncodeMap { get; init; }

    public char[] DecodeMap { get; init; }

    public int Size { get; init; }

    [Obsolete("Only use for serialization")]
    [JsonConstructor]
    public Alphabet() { }

    private Alphabet(Dictionary<char, int> enodeMap, char[] decodeMap)
    {
        EncodeMap = enodeMap;
        DecodeMap = decodeMap;
        Size = decodeMap.Length;
    }

    public static int[] EncodeInput(string input, out Alphabet alphabet)
    {
        HashSet<char> characters = [];
        int encodeIndex = 1;
        Dictionary<char, int> encodeMap = [];
        List<char> decodeMap = ['_'];

        int[] encodedInput = new int[input.Length + 1];

        for (int i = 0; i < input.Length; i++)
        {
            char currentCharacter = input[i];
            if (characters.Add(currentCharacter))
            {
                encodedInput[i] = encodeIndex;
                encodeMap.Add(currentCharacter, encodeIndex);
                decodeMap.Add(currentCharacter);
                encodeIndex++;
            }
            else
            {
                encodedInput[i] = encodeMap[currentCharacter];
            }
        }
        // Adding sentinal
        encodedInput[input.Length] = 0;

        alphabet = new(encodeMap, decodeMap.ToArray());
        return encodedInput;
    }

    public static int[] EncodeInputParts(string[] inputParts, out Alphabet alphabet)
    {
        HashSet<char> characters = [];
        int encodeIndex = 1;
        Dictionary<char, int> encodeMap = [];
        List<char> decodeMap = ['_'];

        int sumPartLengths = inputParts.Sum(p => p.Length + 1);

        int[] encodedInput = new int[sumPartLengths];

        int x = 0;
        foreach (var input in inputParts)
        {
            foreach (var currentCharacter in input)
            {
                if (characters.Add(currentCharacter))
                {
                    encodedInput[x] = encodeIndex;
                    encodeMap.Add(currentCharacter, encodeIndex);
                    decodeMap.Add(currentCharacter);
                    encodeIndex++;
                }
                else
                {
                    encodedInput[x] = encodeMap[currentCharacter];
                }
                x++;
            }
            // Adding sentinal
            encodedInput[x] = 0;
            x++;
        }

        alphabet = new(encodeMap, decodeMap.ToArray());
        return encodedInput.ToArray();
    }

    public int[] EncodeQuery(string query) =>
        query
            .Select(x => EncodeMap.GetValueOrDefault(x, -1))
            .ToArray();

    public List<int> GetOffsetsForSubtree(Node node)
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
                for (int i = 0; i < Size; i++)
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
