public interface ITable
{
    bool IsInitialized { get; }
    void Load(string url);
}
