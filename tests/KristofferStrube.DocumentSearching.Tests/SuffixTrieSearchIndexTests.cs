using FluentAssertions.Execution;

using KristofferStrube.DocumentSearching.SuffixTree;
using KristofferStrube.DocumentSearching.SuffixTrie;

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

    [Fact]
    public void ExactSearch_S_In_S()
    {
        SuffixTrieSearchIndex st = new SuffixTrieSearchIndex("s");

        int[] searchResults = st.ExactSearch("s");

        searchResults.Should().HaveCount(1);
        searchResults.Should().Contain(0);
    }

    [Fact]
    public void ApproximateSearch_E_In_Hey()
    {
        SuffixTrieSearchIndex st = new SuffixTrieSearchIndex("hey");

        var searchResults = st.ApproximateSearch("e", 1);

        using AssertionScope _ = new();
        searchResults.Should().HaveCount(8);

        // We have the exact match
        searchResults.Should().ContainEquivalentOf(new ApproximateMatch(1, [EditType.Match], 0));

        // We have the results where the e in the query is deleted and matches in all places.
        searchResults.Should().ContainEquivalentOf(new ApproximateMatch(0, [EditType.Delete], 1));
        searchResults.Should().ContainEquivalentOf(new ApproximateMatch(1, [EditType.Delete], 1));
        searchResults.Should().ContainEquivalentOf(new ApproximateMatch(2, [EditType.Delete], 1));

        // We have the results where there are mismatches on all characters.
        searchResults.Should().ContainEquivalentOf(new ApproximateMatch(0, [EditType.MisMatch], 1));
        searchResults.Should().ContainEquivalentOf(new ApproximateMatch(1, [EditType.MisMatch], 1));
        searchResults.Should().ContainEquivalentOf(new ApproximateMatch(2, [EditType.MisMatch], 1));

        // We have the results where it inserts an h in the query to match on "he".
        searchResults.Should().ContainEquivalentOf(new ApproximateMatch(0, [EditType.Insert, EditType.Match], 1));
    }

    [Fact]
    public void ApproximateSearch_Tip_In_Tap()
    {
        SuffixTrieSearchIndex st = new SuffixTrieSearchIndex("tap");

        var searchResults = st.ApproximateSearch("tip", 1);

        using AssertionScope _ = new();
        searchResults.Should().HaveCount(1);

        // We have one match where there the i is replaced with an a.
        searchResults.Should().ContainEquivalentOf(new ApproximateMatch(0, [EditType.Match, EditType.MisMatch, EditType.Match], 1));
    }

    [Fact]
    public void ApproximateSearch_Cat_In_Hat()
    {
        SuffixTrieSearchIndex st = new SuffixTrieSearchIndex("hat");

        var searchResults = st.ApproximateSearch("cat", 1);

        // We have one match where the h is replaced with a c.
        searchResults.Should().ContainEquivalentOf(new ApproximateMatch(0, [EditType.MisMatch, EditType.Match, EditType.Match], 1));
    }

    [Fact]
    public void ApproximateSearch_S_In_Mississippi()
    {
        SuffixTrieSearchIndex st = new SuffixTrieSearchIndex("mississippi");

        var searchResults = st.ApproximateSearch("s", 0);

        // All exact matches
        searchResults.Should().ContainEquivalentOf(new ApproximateMatch(2, [EditType.Match], 0));
        searchResults.Should().ContainEquivalentOf(new ApproximateMatch(3, [EditType.Match], 0));
        searchResults.Should().ContainEquivalentOf(new ApproximateMatch(5, [EditType.Match], 0));
        searchResults.Should().ContainEquivalentOf(new ApproximateMatch(6, [EditType.Match], 0));
    }

    [Fact]
    public void ApproximateSearch_quik_In_TheQuickBrownFoxJumpsOverTheLazyDog()
    {
        SuffixTrieSearchIndex st = new SuffixTrieSearchIndex("the quick brown fox jumps over the lazy dog");

        var searchResults = st.ApproximateSearch("quik", 1);

        // All exact matches
        searchResults.Should().ContainEquivalentOf(new ApproximateMatch(4, [EditType.Match, EditType.Match, EditType.Match, EditType.Insert, EditType.Match], 1));
        searchResults.Should().ContainEquivalentOf(new ApproximateMatch(4, [EditType.Match, EditType.Match, EditType.Match, EditType.MisMatch], 1));
    }

    [Fact]
    public void ApproximateSearch_qich_In_TheQuickBrownFoxJumpsOverTheLazyDog()
    {
        SuffixTrieSearchIndex st = new SuffixTrieSearchIndex("the quick brown fox jumps over the lazy dog");

        var searchResults = st.ApproximateSearch("qich", 2);

        // All exact matches
        searchResults.Should().ContainEquivalentOf(new ApproximateMatch(4, [EditType.Match, EditType.Insert, EditType.Match, EditType.Match, EditType.MisMatch], 2));
    }

    [Fact]
    public void ApproximateSearch_temperature_In_SlightlyAboveRoomTemperature()
    {
        SuffixTrieSearchIndex st = new SuffixTrieSearchIndex("(slightly above room temperature)");

        var searchResults = st.ApproximateSearch("temerature", 1);

        // All exact matches
        searchResults.Should().ContainEquivalentOf(new ApproximateMatch(21, [EditType.Match, EditType.Match, EditType.Match, EditType.Insert, EditType.Match, EditType.Match, EditType.Match, EditType.Match, EditType.Match, EditType.Match, EditType.Match], 1));
    }
}