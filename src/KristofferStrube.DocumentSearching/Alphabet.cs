using System.Text.Json.Serialization;

namespace KristofferStrube.DocumentSearching;

public class Alphabet
{
    public Dictionary<char, int> EncodeMap { get; init; }

    public char[] DecodeMap { get; init; }

    public int Size { get; init; }

    [Obsolete("Only use for serialization")]
    [JsonConstructor]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Alphabet() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

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

        alphabet = new(encodeMap, [.. decodeMap]);
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
        for (int j = 0; j < inputParts.Length; j++)
        {
            string input = inputParts[j];
            for (int i = 0; i < input.Length; i++)
            {
                char currentCharacter = input[i];
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

        alphabet = new(encodeMap, [.. decodeMap]);
        return [.. encodedInput];
    }

    public int[]? EncodeQuery(string query)
    {
        int[] encoded = new int[query.Length];
        for (int i = 0; i < query.Length; i++)
        {
            if (EncodeMap.TryGetValue(query[i], out int encodedValue))
            {
                encoded[i] = encodedValue;
            }
            else
            {
                return null;
            }
        }
        return encoded;
    }
}
