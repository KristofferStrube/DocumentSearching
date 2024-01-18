using System.Diagnostics;

namespace KristofferStrube.DocumentSearching;

public class DocumentIndex<TElement, TSearchIndex> where TSearchIndex : ISearchIndex
{
    private readonly ISearchIndex _searchIndex;
    private readonly TElement[] _elements;
    private readonly int[] _offsets;

    private DocumentIndex(ISearchIndex searchIndex, TElement[] elements, int[] offsets)
    {
        _searchIndex = searchIndex;
        _elements = elements;
        _offsets = offsets;
    }

    public static async Task<DocumentIndex<TElement, TSearchIndex>> CreateAsync(TElement[] elements, Func<TElement, Task<string>> elementMapper)
    {
        int[] offsets = new int[elements.Length];
        int accumulativeOffset = 0;
        string[] elementParts = new string[elements.Length];

        for (int i = 0; i < elements.Length; i++)
        {
            string elementPart = await elementMapper(elements[i]);
            accumulativeOffset += elementPart.Length + 1;
            offsets[i] = accumulativeOffset;
            elementParts[i] = elementPart;
        }
        ISearchIndex searchIndex = TSearchIndex.Create(elementParts);

        return new(searchIndex, elements, offsets);
    }

    public static DocumentIndex<TElement, TSearchIndex> Create(TElement[] elements, Func<TElement, string> elementMapper)
    {
        int[] offsets = new int[elements.Length];
        int accumulativeOffset = 0;
        string[] elementParts = new string[elements.Length];

        for (int i = 0; i < elements.Length; i++)
        {
            string elementPart = elementMapper(elements[i]);
            accumulativeOffset += elementPart.Length + 1;
            offsets[i] = accumulativeOffset;
            elementParts[i] = elementPart;
        }
        ISearchIndex searchIndex = TSearchIndex.Create(elementParts);

        return new(searchIndex, elements, offsets);
    }

    public SearchResult<TElement>[] ExactSearch(string query)
    {
        int[] results = _searchIndex.ExactSearch(query);

        List<SearchResult<TElement>> matchingElements = [];

        int currentOffset = -1;
        int i = -1;
        List<int> matchesForCurrentElementMatch = new();
        while (true)
        {
            if (currentOffset < _offsets.Length - 1)
            {
                currentOffset++;
            }
            if (i < results.Length - 1)
            {
                i++;
                matchesForCurrentElementMatch.Add(results[i]);
            }
            if ((currentOffset == _offsets.Length - 1 && i == results.Length - 1) || (results[i] < _offsets[currentOffset] && (i+1 == results.Length || results[i+1] > _offsets[currentOffset])))
            {
                if (matchesForCurrentElementMatch.Count is not 0)
                {
                    matchingElements.Add(new(_elements[currentOffset], matchesForCurrentElementMatch.ToArray()));
                    matchesForCurrentElementMatch.Clear();
                }
            }
            if (currentOffset == _offsets.Length - 1 && i == results.Length - 1)
            {
                break;
            }
        }

        return matchingElements.ToArray();
    }
}
