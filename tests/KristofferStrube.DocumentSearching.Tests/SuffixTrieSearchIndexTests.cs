using KristofferStrube.DocumentSearching.SuffixTree;

namespace KristofferStrube.DocumentSearching.Tests;

public class SuffixTrieSearchIndexTests
{
    [Fact]
    public void ExactSearch_Mis_In_Mississippi()
    {
        SuffixTrieSearchIndex st = new SuffixTrieSearchIndex("mississippi");

        int[] searchResults = st.ExactSearch("mis");

        searchResults.Should().HaveCount(1);
        searchResults.Should().Contain(0);
    }

    [Fact]
    public void ExactSearch_Ppi_In_Mississippi()
    {
        SuffixTrieSearchIndex st = new SuffixTrieSearchIndex("mississippi");

        int[] searchResults = st.ExactSearch("ppi");

        searchResults.Should().HaveCount(1);
        searchResults.Should().Contain(8);
    }

    [Fact]
    public void ExactSearch_Sis_In_Mississippi()
    {
        SuffixTrieSearchIndex st = new SuffixTrieSearchIndex("mississippi");

        int[] searchResults = st.ExactSearch("sis");

        searchResults.Should().HaveCount(1);
        searchResults.Should().Contain(3);
    }

    [Fact]
    public void ExactSearch_Pip_NotIn_Mississippi()
    {
        SuffixTrieSearchIndex st = new SuffixTrieSearchIndex("mississippi");

        int[] searchResults = st.ExactSearch("pip");

        searchResults.Should().HaveCount(0);
    }

    [Fact]
    public void ExactSearch_Fork_NotIn_Mississippi()
    {
        SuffixTrieSearchIndex st = new SuffixTrieSearchIndex("mississippi");

        int[] searchResults = st.ExactSearch("fork");

        searchResults.Should().HaveCount(0);
    }

    [Fact]
    public void ExactSearch_S_In_Mississippi()
    {
        SuffixTrieSearchIndex st = new SuffixTrieSearchIndex("mississippi");

        int[] searchResults = st.ExactSearch("s");

        searchResults.Should().HaveCount(4);
        searchResults.Should().Contain(2);
        searchResults.Should().Contain(3);
        searchResults.Should().Contain(5);
        searchResults.Should().Contain(6);
    }
}