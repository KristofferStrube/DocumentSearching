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
        var documentIndex = DocumentIndex<(int id, string content), SuffixTreeSearchIndex>.Create(elements, c => c.content);

        var results = documentIndex.ExactSearch("mis");

        // Assert
        results.Should().HaveCount(1);
        results.Single().Element.id.Should().Be(4);
        results.Single().Matches.Single().Should().Be(0);
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
        var documentIndex = DocumentIndex<(int id, string content), SuffixTreeSearchIndex>.Create(elements, c => c.content);

        var results = documentIndex.ExactSearch("!");

        // Assert
        results.Should().HaveCount(2);
        results[0].Element.id.Should().Be(1);
        results[0].Matches.Single().Should().Be(32);
        results[1].Element.id.Should().Be(2);
        results[1].Matches.Single().Should().Be(4);
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
        var documentIndex = DocumentIndex<(int id, string content), SuffixTreeSearchIndex>.Create(elements, c => c.content);

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
        var documentIndex = DocumentIndex<(int id, string content), SuffixTreeSearchIndex>.Create(elements, c => c.content);

        var results = documentIndex.ExactSearch("helium");

        // Assert
        results.Should().HaveCount(1);
        results.Single().Element.id.Should().Be(2);
    }
}
