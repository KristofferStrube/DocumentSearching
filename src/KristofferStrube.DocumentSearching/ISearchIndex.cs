namespace KristofferStrube.DocumentSearching;

public interface ISearchIndex
{
    public static abstract ISearchIndex Create(string[] inputParts);
    public int[] ExactSearch(string query);
}
