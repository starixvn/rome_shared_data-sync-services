using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace S1.BackgroundTasks
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            return true;
        }
    }
}