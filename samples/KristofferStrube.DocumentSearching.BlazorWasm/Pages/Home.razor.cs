using System.Diagnostics;
using System.Globalization;

using CsvHelper;
using CsvHelper.Configuration.Attributes;

using KristofferStrube.DocumentSearching.SuffixTree;

namespace KristofferStrube.DocumentSearching.BlazorWasm.Pages
{
    public partial class Home
    {
        private double? searchTime;
        private double? naive;
        private Element[] elements = default!;
        private SearchResult<Element>[] searchResults = [];
        private string[] continuations = [];

        private DocumentIndex<Element, SuffixTrieSearchIndex> index = default!;

        private string query = "";

        protected override async Task OnInitializedAsync()
        {
            Stream periodicTableCsv = await HttpClient.GetStreamAsync("data/periodic-table-detailed.csv");
            using StreamReader streamReader = new StreamReader(periodicTableCsv);

            using var csv = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            elements = csv.GetRecords<Element>().ToArray();

            index = DocumentIndex<Element, SuffixTrieSearchIndex>.Create(elements, e => e.Summary.ToLower());
        }

        public void Search()
        {
            if (query is "")
            {
                searchResults = [];
                searchTime = null;
                return;
            }

            Stopwatch sw = Stopwatch.StartNew();
            searchResults = index.ApproximateSearch(query.ToLower(), 1);
            searchTime = sw.ElapsedTicks / (double)1_000_000;
            continuations = index.ContinuationsSortedByOccurrences(query.ToLower(), [' ', '"', '-', '.', ',', '\\', '(', ')'], 10, mustBeAfterBreakChar: true);

            sw = Stopwatch.StartNew();
            elements
                .Select(element =>
                    (
                        score: query
                            .Split(" ")
                            .Where(s => s.Trim().Length is not 0)
                            .Sum(queryPart => NaiveStringSearch(element.Summary.ToLower(), queryPart.ToLower()).Count),
                        element: element
                    ))
                .Where(tuple => tuple.score > 0)
                .OrderByDescending(tuple => tuple.score)
                .Select(tuple => tuple.element)
                .ToArray();
            naive = sw.ElapsedTicks / (double)1_000_000;
        }

        public static List<int> NaiveStringSearch(string str, string value)
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("the string to find may not be empty", "value");
            List<int> indexes = new List<int>();

            int index = 0;
            while (true)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                {
                    break;
                }
                index++;
                indexes.Add(index);
            }
            return indexes;
        }

        public class Element
        {
            [Name("name")]
            public required string Name { get; set; }
            [Name("summary")]
            public required string Summary { get; set; }
        }
    }
}