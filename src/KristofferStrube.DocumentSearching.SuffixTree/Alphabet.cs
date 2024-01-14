namespace KristofferStrube.DocumentSearching.SearchTree;

public class Alphabet
{
    public Dictionary<char, int> EncodeMap { get; }

    public char[] DecodeMap { get; private set; }

    public int Size { get; private set; }

    public Alphabet()
    {
        EncodeMap = [];
        DecodeMap = [];
        Size = 1;
    }

    private Alphabet(Dictionary<char, int> enodeMap, char[] decodeMap)
    {
        EncodeMap = enodeMap;
        DecodeMap = decodeMap;
        Size = decodeMap.Length + 1;
    }

    public static int[] EncodeInput(string input, out Alphabet alphabet)
    {
        HashSet<char> characters = [];
        int encodeIndex = 1;
        Dictionary<char, int> encodeMap = [];
        List<char> decodeMap = [];

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

    public int[] AddAndEncodeInput(string input)
    {
        HashSet<char> characters = new(DecodeMap);
        int encodeIndex = DecodeMap.Length;
        List<char> newDecodeMap = new(DecodeMap);

        int[] encodedInput = new int[input.Length + 1];

        for (int i = 0; i < input.Length; i++)
        {
            char currentCharacter = input[i];
            if (characters.Add(currentCharacter))
            {
                encodedInput[i] = encodeIndex;
                EncodeMap.Add(currentCharacter, encodeIndex);
                newDecodeMap.Add(currentCharacter);
                encodeIndex++;
            }
            else
            {
                encodedInput[i] = EncodeMap[currentCharacter];
            }
        }
        // Adding sentinal
        encodedInput[input.Length] = 0;
        DecodeMap = newDecodeMap.ToArray();
        Size = DecodeMap.Length + 1;

        return encodedInput;
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
