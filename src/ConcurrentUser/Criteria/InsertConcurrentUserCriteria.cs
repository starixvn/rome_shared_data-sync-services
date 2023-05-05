using Fanex.Data.Repository;
using System.Collections;
using ConcurrentUser.Models;
using System.Text;

namespace ConcurrentUser.Criteria;

public class InsertConcurrentUserCriteria : NonQueryCommand
{
    public InsertConcurrentUserCriteria(IEnumerable<ConcurrentUserModel> users, string serverName)
    {
        ServerName = serverName;
        var xml = new StringBuilder("<root>");
        foreach (var user in users)
        {
            xml.Append(user.ToString());
        }
        xml.Append("</root>");

        XmlData = xml.ToString();
    }
    private string ServerName { get; }

    public string XmlData { get; }

    public string ErrorMsg { get; set; } = default!;

    public string ErrCode { get; set; } = default!;

    public override string GetSettingKey()
        => $"Age_Rpt_ConcurrentOnlineUserDetail_Ins_{ServerName}";

    public override bool IsValid() => !string.IsNullOrEmpty(XmlData);
}
