using KristofferStrube.DocumentSearching.SuffixTree;

namespace KristofferStrube.DocumentSearching.Tests;

public class DocumentIndexTests
{
    [Fact]
    public void ExactSearch_Mis_In_LastElement()
    {
        // Arrange
        (int id, string content)[] elements = [
            (1, "the fox jumped over the lazy dog!"),
            (2, "1337!"),
            (3, "42"),
            (4, "mississippi"),
        ];

        // Act
        var documentIndex = DocumentIndex<(int id, string content), SuffixTrieSearchIndex>.Create(elements, c => c.content);

        var results = documentIndex.ExactSearch("mis");

        // Assert
        results.Should().HaveCount(1);
        results.Single().Element.id.Should().Be(4);
        results.Single().Matches.Single().Position.Should().Be(0);
    }

    [Fact]
    public void ExactSearch_Exclamation_In_TwoFirstElements()
    {
        // Arrange
        (int id, string content)[] elements = [
            (1, "the fox jumped over the lazy dog!"),
            (2, "1337!"),
            (3, "42"),
            (4, "mississippi"),
        ];

        // Act
        var documentIndex = DocumentIndex<(int id, string content), SuffixTrieSearchIndex>.Create(elements, c => c.content);

        var results = documentIndex.ExactSearch("!");

        // Assert
        results.Should().HaveCount(2);
        results[0].Element.id.Should().Be(2);
        results[0].Matches.Single().Position.Should().Be(4);
        results[1].Element.id.Should().Be(1);
        results[1].Matches.Single().Position.Should().Be(32);
    }

    [Fact]
    public void ExactSearch_S_In_LastElement()
    {
        // Arrange
        (int id, string content)[] elements = [
            (1, "the fox jumped over the lazy dog!"),
            (2, "1337!"),
            (3, "42"),
            (4, "mississippi"),
        ];

        // Act
        var documentIndex = DocumentIndex<(int id, string content), SuffixTrieSearchIndex>.Create(elements, c => c.content);

        var results = documentIndex.ExactSearch("s");

        // Assert
        results.Should().HaveCount(1);
        results[0].Element.id.Should().Be(4);
    }

    [Fact]
    public void ExactSearch_Helium_In_PeriodicTable()
    {
        // Arrange
        (int id, string content)[] elements = [
            (1, """Hydrogen is a chemical element with chemical symbol H and atomic number 1. With an atomic weight of 1.00794 u, hydrogen is the lightest element on the periodic table. Its monatomic form (H) is the most abundant chemical substance in the Universe, constituting roughly 75% of all baryonic mass.""".ToLower()),
            (2, """Helium is a chemical element with symbol He and atomic number 2. It is a colorless, odorless, tasteless, non-toxic, inert, monatomic gas that heads the noble gas group in the periodic table. Its boiling and melting points are the lowest among all the elements.""".ToLower()),
        ];

        // Act
        var documentIndex = DocumentIndex<(int id, string content), SuffixTrieSearchIndex>.Create(elements, c => c.content);

        var results = documentIndex.ExactSearch("2");

        // Assert
        results.Should().HaveCount(1);
        results.Single().Element.id.Should().Be(2);
        results.Single().Matches.Single().Position.Should().Be(62);
    }

    [Fact]
    public void ApproximateMatch_Hellium_In_PeriodicTable()
    {
        // Arrange
        (int id, string content)[] elements = [
            (1, """Hydrogen is a chemical element with chemical symbol H and atomic number 1. With an atomic weight of 1.00794 u, hydrogen is the lightest element on the periodic table. Its monatomic form (H) is the most abundant chemical substance in the Universe, constituting roughly 75% of all baryonic mass.""".ToLower()),
            (2, """Helium is a chemical element with symbol He and atomic number 2. It is a colorless, odorless, tasteless, non-toxic, inert, monatomic gas that heads the noble gas group in the periodic table. Its boiling and melting points are the lowest among all the elements.""".ToLower()),
        ];

        // Act
        var documentIndex = DocumentIndex<(int id, string content), SuffixTrieSearchIndex>.Create(elements, c => c.content);

        var results = documentIndex.ApproximateSearch("hellium", 1);

        // Assert
        results.Should().HaveCount(1);
        results.Single().Element.id.Should().Be(2);
        results.Single().Matches.First().Position.Should().Be(0);
        results.Single().Matches.Last().Position.Should().Be(0);
    }

    [Fact]
    public void Predictions_ShowsContinuationWithMostOccurrences()
    {
        // Arrange
        (int id, string content)[] elements = [
            (1, "PageRank (PR) is an algorithm used by Google Search to rank web pages in their search engine results. It is named after both the term \"web page\" and co-founder Larry Page. PageRank is a way of measuring the importance of website pages."),
            (2, "PageRank works by counting the number and quality of links to a page to determine a rough estimate of how important the website is. The underlying assumption is that more important websites are likely to receive more links from other websites."),
            (3, "Currently, PageRank is not the only algorithm used by Google to order search results, but it is the first algorithm that was used by the company, and it is the best known. As of September 24, 2019, all patents associated with PageRank have expired."),
            (4, "PageRank is a link analysis algorithm and it assigns a numerical weighting to each element of a hyperlinked set of documents, such as the World Wide Web, with the purpose of \"measuring\" its relative importance within the set."),
        ];

        // Act
        var documentIndex = DocumentIndex<(int id, string content), SuffixTrieSearchIndex>.Create(elements, c => c.content.ToLower());

        var results = documentIndex.ContinuationsSortedByOccurrences("pag", [' ', '.', '"'], 10, mustBeAfterBreakChar: false);

        // Assert
        results.Should().HaveCount(3);
        results[0].Should().Be("pagerank");
        results[1].Should().Be("page");
    }

    [Fact]
    public void ApproximateMatch_Channel_In_Types()
    {
        // Arrange
        (int id, string content)[] elements = [
            (1, """change""".ToLower()),
            (2, """channel""".ToLower()),
            (3, """filter""".ToLower())
        ];

        // Act
        var documentIndex = DocumentIndex<(int id, string content), SuffixTrieSearchIndex>.Create(elements, c => c.content);

        var results = documentIndex.ApproximateSearch("chanel", 1);

        // Assert
        results.Should().HaveCount(1);
        results.First().Element.id.Should().Be(2);
    }
}
