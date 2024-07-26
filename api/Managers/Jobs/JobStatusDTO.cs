namespace api.Managers.Jobs
{
    public class JobStatusDTO<T> where T : class
    {
        //  Properties
        public Guid ID { get; set; }
        public JobStatus Status { get; set; }
        public T? Data { get; set; }
    }
}