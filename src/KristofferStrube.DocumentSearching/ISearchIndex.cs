namespace KristofferStrube.DocumentSearching;

public interface ISearchIndex
{
    public int[] ExactSearch(string query);

    public static abstract ISearchIndex Create(string[] inputParts);
}
