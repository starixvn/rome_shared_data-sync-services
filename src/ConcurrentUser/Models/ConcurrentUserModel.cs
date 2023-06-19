namespace ConcurrentUser.Models;

public class ConcurrentUserModel
{
    public int SiteId { get; set; }

    public int RoleId { get; set; }

    public int RealUserCount { get; set; }

    public int TestingUserCount { get; set; }

    public DateTime CreatedDate { get; set; }

    public override string ToString()
    => $"<r CreatedDate=\"{CreatedDate}\" SiteId=\"{SiteId}\" RoleId=\"{RoleId}\" RealUserCount=\"{RealUserCount}\" TestingUserCount=\"{TestingUserCount}\" />";
}