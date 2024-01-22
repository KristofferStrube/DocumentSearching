using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using CsvHelper;
using CsvHelper.Configuration.Attributes;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace KristofferStrube.DocumentSearching.BlazorWasm5.Pages;

public partial class Index : ComponentBase
{
    private List<double> readCSVTimings = new();
    private List<double> constructIndexTimings = new();
    private List<double> jsonSerializeTimings = new();
    private List<double> writeLocalStorageTimings = new();
    private List<double> readLocalStorageTimings = new();
    private List<double> jsonDeserializeTimings = new();
    private List<double> exactSearchTimings = new();

    private string CSVStats = "";
    private string constructIndexStats = "";
    private string jsonSerializeStats = "";
    private string writeLocalStorageStats = "";
    private string readLocalStorageStats = "";
    private string jsonDeserializeStats = "";
    private string exactSearchStats = "";

    private Element[] elements = default!;
    private DocumentIndex<Element> index = default!;
    private SearchResult<Element>[] searchResults = [];
    private readonly JsonSerializerOptions serializerOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve
    };

    [Inject]
    public HttpClient HttpClient { get; set; }

    [Inject]
    public IJSInProcessRuntime JSRuntime { get; set; }

    public async Task Start()
    {
        for (int i = 0; i < 110; i++)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Read CSV
            {
                Stream periodicTableCsv = await HttpClient.GetStreamAsync("data/periodic-table-detailed.csv");
                using StreamReader streamReader = new StreamReader(periodicTableCsv);
                using CsvReader csv = new(streamReader, CultureInfo.InvariantCulture);
                elements = csv.GetRecords<Element>().Take(10).ToArray();
            }

            readCSVTimings.Add(stopwatch.ElapsedTicks / (double)1_000_000);
            stopwatch.Restart();

            // Construct Index
            index = DocumentIndex<Element>.Create(elements, e => e.Summary.ToLower());

            constructIndexTimings.Add(stopwatch.ElapsedTicks / (double)1_000_000);
            stopwatch.Restart();

            // JSON Serialize Index
            string serialized = JsonSerializer.Serialize(index, serializerOptions);

            jsonSerializeTimings.Add(stopwatch.ElapsedTicks / (double)1_000_000);
            stopwatch.Restart();

            // Write to LocalStorage
            JSRuntime.InvokeVoid("window.localStorage.setItem", "index", serialized);

            writeLocalStorageTimings.Add(stopwatch.ElapsedTicks / (double)1_000_000);
            stopwatch.Restart();

            // Read from LocalStorage
            string readIndex = JSRuntime.Invoke<string>("window.localStorage.getItem", "index");

            readLocalStorageTimings.Add(stopwatch.ElapsedTicks / (double)1_000_000);
            stopwatch.Restart();

            // JSON Deserialize Index
            DocumentIndex<Element> deserializedIndex = JsonSerializer.Deserialize<DocumentIndex<Element>>(readIndex, serializerOptions);

            jsonDeserializeTimings.Add(stopwatch.ElapsedTicks / (double)1_000_000);
            stopwatch.Restart();

            // Search for words in summaries.
            for (int j = 0; j < 10; j++)
            {
                searchResults = deserializedIndex.ExactSearch("hydrogen");
                searchResults = deserializedIndex.ExactSearch("oxygen");
                searchResults = deserializedIndex.ExactSearch("light");
                searchResults = deserializedIndex.ExactSearch("heavy");
                searchResults = deserializedIndex.ExactSearch("chemical");
                searchResults = deserializedIndex.ExactSearch("element");
                searchResults = deserializedIndex.ExactSearch("symbol");
                searchResults = deserializedIndex.ExactSearch("atomic");
                searchResults = deserializedIndex.ExactSearch("number");
                searchResults = deserializedIndex.ExactSearch("weight");
            }

            exactSearchTimings.Add(stopwatch.ElapsedTicks / (double)1_000_000);
            stopwatch.Restart();

            elements = default!;
            index = default!;
            JSRuntime.InvokeVoid("window.localStorage.clear");
            searchResults = [];
            await Task.Delay(200);
        }

        CSVStats = $"Read CSV: {GetStatistics(readCSVTimings)}";
        constructIndexStats = $"Construct Index: {GetStatistics(constructIndexTimings)}";
        jsonSerializeStats = $"JSON Serialize: {GetStatistics(jsonSerializeTimings)}";
        writeLocalStorageStats = $"Write LocalStorage: {GetStatistics(writeLocalStorageTimings)}";
        readLocalStorageStats = $"Read LocalStorage: {GetStatistics(readLocalStorageTimings)}";
        jsonDeserializeStats = $"JSON Deserialize: {GetStatistics(jsonDeserializeTimings)}";
        exactSearchStats = $"Search: {GetStatistics(exactSearchTimings)}";
    }

    private string GetStatistics(List<double> timings)
    {
        return $"Min: {Math.Round(timings.Skip(5).SkipLast(5).Min(), 2)} ms; Average: {Math.Round(timings.Skip(5).SkipLast(5).Average(), 2)} ms; Max: {Math.Round(timings.Skip(5).SkipLast(5).Max(), 2)} ms;";
    }

    public class Element
    {
        [Name("name")]
        public string Name { get; set; }

        [Name("summary")]
        public string Summary { get; set; }
    }
}