namespace api.Services
{
    public interface IBackgroundTaskQueue
    {
        //  Methods
        Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
        void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);
    }
}