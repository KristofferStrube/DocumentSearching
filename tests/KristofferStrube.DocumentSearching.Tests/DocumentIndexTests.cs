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
        results[1].Element.id.Should().Be(2);
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
}
