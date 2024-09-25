namespace KristofferStrube.DocumentSearching;

public interface ISearchIndex<T> where T : ISearchIndex<T>
{
    public static abstract T Create(string[] inputParts);
    public int[] ExactSearch(string query);
    public ApproximateMatch[] ApproximateSearch(string query, int edits);
    public string Continuation(int from, char[] until, out bool previousCharIsPartOfUntil);
}
