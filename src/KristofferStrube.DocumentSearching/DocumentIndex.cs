using System.Text.Json.Serialization;

namespace KristofferStrube.DocumentSearching;

public class DocumentIndex<TElement, TSearchIndex> where TSearchIndex : ISearchIndex<TSearchIndex>
{
    public TSearchIndex SearchIndex { get; init; }
    public TElement[] Elements { get; init; }
    public int[] Offsets { get; init; }

    [Obsolete("Only use for serialization")]
    [JsonConstructor]
    public DocumentIndex() { }

    private DocumentIndex(TSearchIndex searchIndex, TElement[] elements, int[] offsets)
    {
        SearchIndex = searchIndex;
        Elements = elements;
        Offsets = offsets;
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
        TSearchIndex searchIndex = TSearchIndex.Create(elementParts);

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
        TSearchIndex searchIndex = TSearchIndex.Create(elementParts);

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

            int[] results = SearchIndex.ExactSearch(queryParts[q]);

            for (int i = 0; i < results.Length; i++)
            {
                int result = results[i];
                for (int j = 0; j < Offsets.Length; j++)
                {
                    if (j == Offsets.Length - 1 || result < Offsets[j + 1])
                    {
                        Match match = new Match(result - Offsets[j], queryParts[q].Length);
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
            matchingElements[k] = new(Elements[key], combinedBuckets[key].OrderBy(m => m.Position).ToArray());
            k++;
        }

        return matchingElements;
    }

    public string[] ContinuationsSortedByOccurrences(string query, char[] breakChars, int count, bool mustBeAfterBreakChar)
    {
        if (breakChars.Length is 0)
            return [];

        string[] queryParts = query.Split(" ").Where(s => s.Trim() is not "").Distinct().ToArray();

        if (queryParts.Length is 0)
            return [];

        List<int> firstMatches = new();

        for(int i = 0; i < queryParts.Length - 1; i++)
        {
            string queryPart = queryParts[i];

            firstMatches.AddRange(SearchIndex.ExactSearch(queryPart));
        }

        int[] lastMatches = SearchIndex.ExactSearch(queryParts[^1]);

        Dictionary<string, int> firstContinutions = CountedContinuations(firstMatches, queryParts, breakChars, mustBeAfterBreakChar);
        Dictionary<string, int> lastContinutions = CountedContinuations(lastMatches, queryParts, breakChars, mustBeAfterBreakChar);

        return lastContinutions
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => kvp.Key)
            .Take(count)
            .Concat(firstContinutions
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .Take(Math.Max(0, count - lastContinutions.Count))
            ).ToArray();
    }

    private Dictionary<string, int> CountedContinuations(IEnumerable<int> matches, string[] queryParts, char[] breakChar, bool mustBeAfterBreakChar)
    {
        Dictionary<string, int> continutions = new();

        foreach(int match in matches)
        {
            string continuation = SearchIndex.Continuation(from: match, until: breakChar, out bool previousCharIsPartOfUntil);

            if (queryParts.Contains(continuation))
                continue;

            if (mustBeAfterBreakChar && !previousCharIsPartOfUntil)
                continue;

            if (continuation.Length is 0)
                continue;

            if (continutions.ContainsKey(continuation))
            {
                continutions[continuation]++;
            }
            else
            {
                continutions[continuation] = 1;
            }
        }

        return continutions;
    }
}
