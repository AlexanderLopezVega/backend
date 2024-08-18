using System.Collections.Concurrent;

namespace api.Services
{
    public class CleanupService(ILogger<CleanupService> logger) : BackgroundService, ICleanupService
    {
        //  Constants
        private const int MSToMinutes = 1000 * 60;
        private const int WaitTime = 15 * MSToMinutes;
        private const int PollingTime = 1 * MSToMinutes;

        //  Fields
        private readonly ILogger<CleanupService> m_Logger = logger;
        private readonly ConcurrentQueue<ExpirationToken> m_CleanupQueue = new();

        //  Interface implementations
        void ICleanupService.ScheduleCleanup(Action<CancellationToken> cleanupCallback) => m_CleanupQueue.Enqueue(new ExpirationToken(DateTime.UtcNow, cleanupCallback));

        //  Methods
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            m_Logger.LogInformation("CleanupService is starting");

            while (!cancellationToken.IsCancellationRequested)
            {
                //  See if elements exist for cleanup
                if (m_CleanupQueue.TryPeek(out ExpirationToken token))
                {
                    double lifetime = (DateTime.UtcNow - token.ArrivalTime).TotalMinutes;

                    //  Has the element spent enough time to be cleaned up?
                    if (lifetime > WaitTime)
                    {
                        //  Force dequeue element
                        while (!m_CleanupQueue.TryDequeue(out token)) ;

                        //  Call cleanup callback
                        m_Logger.LogInformation($"Deleting item");

                        token.CleanupCallback?.Invoke(cancellationToken);
                    }
                    else
                    {
                        //  Element has not expired. Wait until expiration
                        await Task.Delay((int)Math.Ceiling(WaitTime - lifetime), cancellationToken);
                    }
                }
                else
                    //  No elements available, wait polling time
                    await Task.Delay(PollingTime, cancellationToken);
            }
        }

        //  Structs
        private struct ExpirationToken(DateTime arrivalTime, Action<CancellationToken> cleanupCallback)
        {
            //  Properties
            public DateTime ArrivalTime { get; set; } = arrivalTime;
            public Action<CancellationToken> CleanupCallback { get; set; } = cleanupCallback;
        }
    }
}