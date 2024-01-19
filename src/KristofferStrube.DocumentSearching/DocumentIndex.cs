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
        string[] queryParts = query.Split(" ").Where(s => s.Trim() is not "").Distinct().ToArray();

        if (queryParts.Length is 0)
            return []; 

        Dictionary<int, List<Match>>[] partBuckets = new Dictionary<int, List<Match>>[queryParts.Length];

        for (int q = 0; q < queryParts.Length; q++)
        {
            Dictionary<int, List<Match>> buckets = new();
            partBuckets[q] = buckets;

            int[] results = _searchIndex.ExactSearch(queryParts[q]);

            for (int i = 0; i < results.Length; i++)
            {
                int result = results[i];
                for (int j = 0; j < _offsets.Length; j++)
                {
                    if (j == _offsets.Length - 1 || result < _offsets[j + 1])
                    {
                        Match match = new Match(result - _offsets[j], queryParts[q].Length);
                        if (buckets.TryGetValue(j, out List<Match>? matches))
                        {
                            matches.Add(match);
                        }
                        else
                        {
                            matches = [match];
                            buckets.Add(j, matches);
                        }
                        break;
                    }
                }
            }
        }

        Dictionary<int, List<Match>> combinedBuckets = partBuckets.First();

        foreach (Dictionary<int, List<Match>> buckets in partBuckets.Skip(1))
        {
            Dictionary<int, List<Match>> temporaryBucket = new();
            foreach(int key in buckets.Keys)
            {
                if (combinedBuckets.ContainsKey(key) && buckets.ContainsKey(key))
                {
                    temporaryBucket.Add(key, combinedBuckets[key].Concat(buckets[key]).ToList());
                }
            }
            combinedBuckets = temporaryBucket;
        }

        SearchResult<TElement>[] matchingElements = new SearchResult<TElement>[combinedBuckets.Count];

        int k = 0;
        foreach (int key in combinedBuckets.Keys.OrderByDescending(k => combinedBuckets[k].Count))
        {
            matchingElements[k] = new(_elements[key], combinedBuckets[key].OrderBy(m => m.Position).ToArray());
            k++;
        }

        return matchingElements.ToArray();
    }
}
