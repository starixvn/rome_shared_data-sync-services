using Fanex.Logging;

namespace S1.BackgroundTasks.Helpers
{
    public static class RetryHelpers
    {
        public static async Task Retry(Func<Task> func, int retryTimes = 3, int waitTime = 500)
        {
            for (int i = 0; i < retryTimes; i++)
            {
                try
                {
                    await func();
                    break;
                }
                catch (Exception ex)
                {
                    await Logger.Log.ErrorAsync($"Retry {i + 1}: Getting Exception : {ex.Message}", ex);
                    await Task.Delay(waitTime);
                }
            }
        }

        public static async Task<T> Retry<T>(Func<Task<T>> func, int retryTimes = 3, int waitTime = 500)
        {
            for (int i = 0; i < retryTimes; i++)
            {
                try
                {
                    return await func();
                }
                catch (Exception ex)
                {
                    await Logger.Log.ErrorAsync($"Retry {i + 1}: Getting Exception : {ex.Message}", ex);
                    await Task.Delay(waitTime);
                }
            }

            return await func();
        }
    }
}