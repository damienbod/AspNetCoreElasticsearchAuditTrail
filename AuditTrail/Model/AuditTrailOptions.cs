namespace AuditTrail.Model;

public class AuditTrailOptions
{
    public bool IndexPerMonth { get; set; }

    public int AmountOfPreviousIndicesUsedInAlias { get; set; }

    public void UseSettings(bool indexPerMonth, int amountOfPreviousIndicesUsedInAlias)
    {
        IndexPerMonth = indexPerMonth;
        AmountOfPreviousIndicesUsedInAlias = amountOfPreviousIndicesUsedInAlias;
    }
}
