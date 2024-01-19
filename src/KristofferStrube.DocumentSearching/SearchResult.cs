namespace KristofferStrube.DocumentSearching;

public class SearchResult<T>
{
    public T Element { get; }

    public Match[] Matches { get; }

    public SearchResult(T element, Match[] matches)
    {
        Element = element;
        Matches = matches;
    }
}
