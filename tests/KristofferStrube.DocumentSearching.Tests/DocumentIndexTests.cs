using KristofferStrube.DocumentSearching.SuffixTree;

namespace KristofferStrube.DocumentSearching.Tests;

public class DocumentIndexTests
{
    [Fact]
    public void ExactSearch_Mis_In_LastElement()
    {
        // Arrange
        SuffixTreeSearchIndex searchIndex = new();
        (int id, string content)[] elements = [
            (1, "the fox jumped over the lazy dog!"),
            (2, "1337!"),
            (3, "42"),
            (4, "mississippi"),
        ];

        // Act
        DocumentIndex<(int id, string content)> documentIndex = DocumentIndex<(int id, string content)>.Create(searchIndex, elements, c => c.content);

        (int id, string content)[] results = documentIndex.ExactSearch("mis");

        // Assert
        results.Should().HaveCount(1);
        results.Single().id.Should().Be(4);
    }

    [Fact]
    public void ExactSearch_Exclamation_In_TwoFirstElements()
    {
        // Arrange
        SuffixTreeSearchIndex searchIndex = new();
        (int id, string content)[] elements = [
            (1, "the fox jumped over the lazy dog!"),
            (2, "1337!"),
            (3, "42"),
            (4, "mississippi"),
        ];

        // Act
        DocumentIndex<(int id, string content)> documentIndex = DocumentIndex<(int id, string content)>.Create(searchIndex, elements, c => c.content);

        (int id, string content)[] results = documentIndex.ExactSearch("!");

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain((1, "the fox jumped over the lazy dog!"));
        results.Should().Contain((2, "1337!"));
    }

    [Fact]
    public void ExactSearch_S_In_LastElement()
    {
        // Arrange
        SuffixTreeSearchIndex searchIndex = new();
        (int id, string content)[] elements = [
            (1, "the fox jumped over the lazy dog!"),
            (2, "1337!"),
            (3, "42"),
            (4, "mississippi"),
        ];

        // Act
        DocumentIndex<(int id, string content)> documentIndex = DocumentIndex<(int id, string content)>.Create(searchIndex, elements, c => c.content);

        (int id, string content)[] results = documentIndex.ExactSearch("s");

        // Assert
        results.Should().HaveCount(1);
        results.Single().id.Should().Be(4);
    }
}
