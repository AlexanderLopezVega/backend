namespace api.Services
{
    public class ModelGenerationService(IBackgroundTaskQueue taskQueue, ILogger<ModelGenerationService> logger) : BackgroundService
    {
        //  Fields
        private readonly IBackgroundTaskQueue m_TaskQueue = taskQueue;
        private readonly ILogger<ModelGenerationService> m_Logger = logger;

        //  Methods
        protected override async Task ExecuteAsync(CancellationToken token)
        {
            m_Logger.LogInformation("Model generation service is running");

            await BackgroundProcessing(token);
        }
        private async Task BackgroundProcessing(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Func<CancellationToken, Task> workItem = await m_TaskQueue.DequeueAsync(token);

                try
                {
                    await workItem.Invoke(token);
                }
                catch (Exception ex)
                {
                    m_Logger.LogError(ex, $"Error ocurred executing {nameof(workItem)}");
                }
            }
        }
    }
}