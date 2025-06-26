using System.Security.Cryptography;
using System.Text;
using Hangfire.Dashboard;

namespace CashServiceBilling.Authorization;

public class BasicAuthAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly string username;
    private readonly string passwordHash;

    public BasicAuthAuthorizationFilter(IConfiguration configuration)
    {
        var hangfire = configuration.GetSection("HangfireDashboard");
        username = hangfire.GetValue<string>("Username");
        passwordHash = hangfire.GetValue<string>("Password");
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        string authHeader = httpContext.Request.Headers["Authorization"];

        if (authHeader != null && authHeader.StartsWith("Basic "))
        {
            // Extract credentials
            var encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
            var encoding = Encoding.GetEncoding("UTF-8");
            var usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));

            var seperatorIndex = usernamePassword.IndexOf(':');
            var username = usernamePassword.Substring(0, seperatorIndex);
            var password = usernamePassword.Substring(seperatorIndex + 1);
            var passwordHash = HashPassword(password);

            if (username == this.username && passwordHash == this.passwordHash)
            {
                return true;
            }
        }

        // Challenge for basic auth
        httpContext.Response.StatusCode = 401;
        httpContext.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Hangfire Dashboard\"";
        return false;
    }

    private static string HashPassword(string plainText)
    {
        using (var sha256 = SHA256.Create())
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var hashBytes = sha256.ComputeHash(plainTextBytes);

            return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
