namespace CompanyManager.Common.Contracts
{
    public interface ISettings
    {
        string? this[string key] { get; }
    }
}