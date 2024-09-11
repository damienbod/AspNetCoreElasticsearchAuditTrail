namespace AuditTrail.Model;

public class AuditTrailOptions
{
    public bool IndexPerMonth { get; set; }
    public int AmountOfPreviousIndicesUsedInAlias { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;

    public void UseSettings(bool indexPerMonth, int amountOfPreviousIndicesUsedInAlias,
        string userName, string password, string url)
    {
        IndexPerMonth = indexPerMonth;
        AmountOfPreviousIndicesUsedInAlias = amountOfPreviousIndicesUsedInAlias;
        Username = userName;
        Password = password;
        Url = url;
    }
}
