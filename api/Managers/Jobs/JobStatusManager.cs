using System.Collections.Concurrent;

namespace api.Managers.Jobs
{
    public class JobStatusManager<T> where T : class
    {
        //  Fields
        private readonly ConcurrentDictionary<Guid, JobStatusDTO<T>> m_JobStatuses;

        //  Constructors
        public JobStatusManager()
        {
            m_JobStatuses = new();
        }

        //  Methods
        public JobStatusDTO<T>? GetJobStatus(Guid jobID)
        {
            return m_JobStatuses.TryGetValue(jobID, out JobStatusDTO<T>? jobStatus) ? jobStatus : null;
        }
        public void UpdateJobStatus(Guid ID, JobStatus status, T? data = null)
        {
            m_JobStatuses[ID] = new()
            {
                ID = ID,
                Status = status,
                Data = data
            };
        }
    }
}