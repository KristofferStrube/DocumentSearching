namespace KristofferStrube.DocumentSearching;

public class SearchResult<T>
{
    public T Element { get; }

    public int[] Matches { get; }

    public SearchResult(T element, int[] matches)
    {
        Element = element;
        Matches = matches;
    }
}
