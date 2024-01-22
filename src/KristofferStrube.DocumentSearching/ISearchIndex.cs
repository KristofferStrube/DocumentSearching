namespace KristofferStrube.DocumentSearching;

public interface ISearchIndex<T> where T : ISearchIndex<T>
{
    public static abstract T Create(string[] inputParts);
    public int[] ExactSearch(string query);
}
