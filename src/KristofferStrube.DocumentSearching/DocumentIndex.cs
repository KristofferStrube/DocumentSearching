namespace KristofferStrube.DocumentSearching;

public class DocumentIndex<T>
{
    private readonly ISearchIndex _searchIndex;
    private readonly T[] _elements;
    private readonly int[] _offsets;

    private DocumentIndex(ISearchIndex searchIndex, T[] elements, int[] offsets)
    {
        _searchIndex = searchIndex;
        _elements = elements;
        _offsets = offsets;
    }

    public static async Task<DocumentIndex<T>> CreateAsync<T>(ISearchIndex emptySearchIndex, T[] elements, Func<T, Task<string>> elementMapper)
    {
        int[] offsets = new int[elements.Length];
        int accumulativeOffset = 0;

        for (int i = 0; i < elements.Length; i++)
        {
            offsets[i] = accumulativeOffset;
            string elementPart = await elementMapper(elements[i]);
            accumulativeOffset += elementPart.Length + 1;

            emptySearchIndex.AddInputPart(elementPart);
        }

        return new(emptySearchIndex, elements, offsets);
    }

    public static DocumentIndex<T> Create<T>(ISearchIndex emptySearchIndex, T[] elements, Func<T, string> elementMapper)
    {
        int[] offsets = new int[elements.Length];
        int accumulativeOffset = 0;

        for (int i = 0; i < elements.Length; i++)
        {
            offsets[i] = accumulativeOffset;
            string elementPart = elementMapper(elements[i]);
            accumulativeOffset += elementPart.Length + 1;

            emptySearchIndex.AddInputPart(elementPart);
        }

        return new(emptySearchIndex, elements, offsets);
    }

    public T[] ExactSearch(string query)
    {
        int[] results = _searchIndex.ExactSearch(query);

        HashSet<T> matchingElements = [];
        for (int i = 0; i < results.Length; i++)
        {
            T matchingElement = _elements.First();
            for (int j = 1; j < _elements.Length; j++)
            {
                if (_offsets[j] > results[i])
                {
                    break;
                }
                matchingElement = _elements[j];
            }
            matchingElements.Add(matchingElement);
        }

        return matchingElements.ToArray();
    }
}
