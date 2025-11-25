using Fanex.Data.Repository;
using System.Collections;
using ConcurrentUser.Models;
using System.Text;
using System.Linq;

namespace ConcurrentUser.Criteria;

public class InsertOnlineUserCriteria : NonQueryCommand
{
    public InsertOnlineUserCriteria(IEnumerable<ConcurrentUserModel> users)
    {
        Users = users;
        var xml = new StringBuilder("<root>");
        foreach (var user in users)
        {
            xml.Append(user.ToString());
        }
        xml.Append("</root>");

        XmlData = xml.ToString();
    }
    private IEnumerable<ConcurrentUserModel> Users { get; }

    public string XmlData { get; }

    public string ErrorMsg { get; set; } = default!;

    public int ErrCode { get; set; } = default!;

    public override string GetSettingKey()
        => $"Acc_Grafana_OnlineAgentDetail_Ins";

    public override bool IsValid() => Users.Any();
}
