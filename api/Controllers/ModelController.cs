using api.Data;
using api.DTO.Model;
using api.Managers.Jobs;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/model"), ApiController]
    public class ModelController
    (
        ApplicationDBContext context,
         IWebHostEnvironment hostEnvironment,
         IWebCrawlerService webCrawlerService,
         JobStatusManager<ModelDTO> jobStatusManager,
         IBackgroundTaskQueue taskQueue
    ) : ControllerBase
    {
        //  Fields
        private readonly ApplicationDBContext m_Context = context;
        private readonly IWebHostEnvironment m_HostEnvironment = hostEnvironment;
        /// <summary>
        /// For generating 3D models
        /// </summary>
        private readonly IWebCrawlerService m_WebCrawlerService = webCrawlerService;
        /// <summary>
        /// For consulting the status of sample creation jobs
        /// </summary>
        private readonly JobStatusManager<ModelDTO> m_JobStatusManager = jobStatusManager;
        /// <summary>
        /// For handling job queueing
        /// </summary>
        private readonly IBackgroundTaskQueue m_TaskQueue = taskQueue;

        //  Methods
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            Sample? sample = m_Context.Samples.Where(s => s.ID == id).FirstOrDefault();

            if (sample == null) return NotFound();

            string modelPath = Path.Combine(m_HostEnvironment.ContentRootPath, sample.ModelPath);
            string modelFile = System.IO.File.ReadAllText(modelPath);

            return Ok(new ModelDTO() { ModelFile = modelFile });
        }
        [HttpPost("start-job")]
        public async Task<IActionResult> StartJob(CreateModelDTO createModelDTO)
        {
            //  Store image for web crawler service
            string temporaryFolder = Path.Combine(m_HostEnvironment.ContentRootPath, "tmp");

            if (!Directory.Exists(temporaryFolder))
                Directory.CreateDirectory(temporaryFolder);

            IFormFile file = createModelDTO.ModelImage;
            string imagePath = Path.Combine(temporaryFolder, file.FileName);
            using (Stream fileStream = new FileStream(imagePath, FileMode.Create)) { await file.CopyToAsync(fileStream); }

            //  Add job to background service
            Guid jobID = Guid.NewGuid();
            m_JobStatusManager.UpdateJobStatus(jobID, JobStatus.Processing);
            m_TaskQueue.QueueBackgroundWorkItem(async (token) => await CreateModelAsync(createModelDTO, imagePath, jobID, token));

            return Ok(new JobStatusDTO<ModelDTO>() { ID = jobID, Status = JobStatus.Processing, Data = null });
        }
        [HttpGet("job-{id}")]
        public IActionResult GetJob(Guid id)
        {
            JobStatusDTO<ModelDTO>? jobStatus = m_JobStatusManager.GetJobStatus(id);

            return (jobStatus == null) ? NotFound() : Ok(jobStatus);
        }
        private async Task CreateModelAsync(CreateModelDTO createModelDTO, string imagePath, Guid jobID, CancellationToken token)
        {
            //  Download 3D model
            string filePath = await m_WebCrawlerService.Download3DModelAsync(imagePath);

            //  Update job status with results
            ModelDTO modelDTO = new()
            {
                ModelFile = System.IO.File.ReadAllText(filePath)
            };

            m_JobStatusManager.UpdateJobStatus(jobID, JobStatus.Completed, modelDTO);
        }
    }
}