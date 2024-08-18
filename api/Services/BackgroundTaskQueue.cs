
using System.Threading.Channels;

namespace api.Services
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        //  Fields
        private readonly Channel<Func<CancellationToken, Task>> m_Queue;

        //  Constructors
        public BackgroundTaskQueue()
        {
            BoundedChannelOptions options = new(100) { FullMode = BoundedChannelFullMode.Wait };

            m_Queue = Channel.CreateBounded<Func<CancellationToken, Task>>(options);
        }

        //  Interface implementations
        async Task<Func<CancellationToken, Task>> IBackgroundTaskQueue.DequeueAsync(CancellationToken cancellationToken)
        {
            return await m_Queue.Reader.ReadAsync(cancellationToken);
        }
        void IBackgroundTaskQueue.QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
        {
            workItem = workItem ?? throw new ArgumentNullException(nameof(workItem));

            bool wasWritten = m_Queue.Writer.TryWrite(workItem);

            Console.WriteLine(wasWritten);
        }
    }
}