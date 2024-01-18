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
            offsets[i] = accumulativeOffset;
            accumulativeOffset += elementPart.Length + 1;
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
            offsets[i] = accumulativeOffset;
            accumulativeOffset += elementPart.Length + 1;
            elementParts[i] = elementPart;
        }
        ISearchIndex searchIndex = TSearchIndex.Create(elementParts);

        return new(searchIndex, elements, offsets);
    }

    public SearchResult<TElement>[] ExactSearch(string query)
    {
        int[] results = _searchIndex.ExactSearch(query);

        Dictionary<int, List<int>> buckets = new();

        for (int i = 0; i < results.Length; i++)
        {
            int result = results[i];
            for (int j = 0; j < _offsets.Length; j++)
            {
                if (j == _offsets.Length - 1 || result < _offsets[j + 1])
                {
                    if (buckets.TryGetValue(j, out List<int>? matches))
                    {
                        matches.Add(result - _offsets[j]);
                    }
                    else
                    {
                        matches = [result - _offsets[j]];
                        buckets.Add(j, matches);
                    }
                    break;
                }
            }
        }


        SearchResult<TElement>[] matchingElements = new SearchResult<TElement>[buckets.Count];

        int k = 0;
        foreach (int key in buckets.Keys)
        {
            matchingElements[k] = new(_elements[key], buckets[key].ToArray());
            k++;
        }

        return matchingElements.ToArray();
    }
}
