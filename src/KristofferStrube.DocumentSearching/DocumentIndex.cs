using System.Text.Json.Serialization;

namespace KristofferStrube.DocumentSearching;

public class DocumentIndex<TElement>
{
    public SuffixTreeSearchIndex SearchIndex { get; init; }
    public TElement[] Elements { get; init; }
    public int[] Offsets { get; init; }

    [Obsolete("Only use for serialization")]
    [JsonConstructor]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public DocumentIndex() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private DocumentIndex(SuffixTreeSearchIndex searchIndex, TElement[] elements, int[] offsets)
    {
        SearchIndex = searchIndex;
        Elements = elements;
        Offsets = offsets;
    }

    public static async Task<DocumentIndex<TElement>> CreateAsync(TElement[] elements, Func<TElement, Task<string>> elementMapper)
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
        SuffixTreeSearchIndex searchIndex = SuffixTreeSearchIndex.Create(elementParts);

        return new(searchIndex, elements, offsets);
    }

    public static DocumentIndex<TElement> Create(TElement[] elements, Func<TElement, string> elementMapper)
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
        SuffixTreeSearchIndex searchIndex = SuffixTreeSearchIndex.Create(elementParts);

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
            Dictionary<int, List<Match>> buckets = [];
            partBuckets[q] = buckets;

            int[] results = SearchIndex.ExactSearch(queryParts[q]);

            for (int i = 0; i < results.Length; i++)
            {
                int result = results[i];
                for (int j = 0; j < Offsets.Length; j++)
                {
                    if (j == Offsets.Length - 1 || result < Offsets[j + 1])
                    {
                        Match match = new(result - Offsets[j], queryParts[q].Length);
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
            Dictionary<int, List<Match>> temporaryBucket = [];
            foreach(int key in buckets.Keys)
            {
                if (combinedBuckets.ContainsKey(key) && buckets.ContainsKey(key))
                {
                    temporaryBucket.Add(key, [.. combinedBuckets[key], .. buckets[key]]);
                }
            }
            combinedBuckets = temporaryBucket;
        }

        SearchResult<TElement>[] matchingElements = new SearchResult<TElement>[combinedBuckets.Count];

        int k = 0;
        foreach (int key in combinedBuckets.Keys.OrderByDescending(k => combinedBuckets[k].Count))
        {
            matchingElements[k] = new(Elements[key], [.. combinedBuckets[key].OrderBy(m => m.Position)]);
            k++;
        }

        return [.. matchingElements];
    }
}
