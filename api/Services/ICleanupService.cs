namespace api.Services
{
    public interface ICleanupService
    {
        // Methods
        void ScheduleCleanup(Action<CancellationToken> cleanupCallback);
    }
}