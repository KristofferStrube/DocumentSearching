namespace KristofferStrube.DocumentSearching;

public interface ISearchIndex
{
    public int[] ExactSearch(string query);

    public void AddInputPart(string inputPart);
}
