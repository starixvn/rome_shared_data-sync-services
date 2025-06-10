namespace ResetPotentialWinLimit.Models;

public class MemberBatchModel
{
    public IEnumerable<int> MemberIds { get; set; }
    public int LastMaxCustID { get; set; }
}
