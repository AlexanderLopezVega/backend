using api.Data;
using api.DTO.Sample;
using api.Managers.Jobs;
using api.Mappers;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/sample"), ApiController]
    public class SampleController
    (
        ApplicationDBContext dbContext,
        IDbContextFactory<ApplicationDBContext> dbContextFactory,
        IWebCrawlerService webCrawlerService,
        JobStatusManager<SampleDTO> jobStatusManager,
        IBackgroundTaskQueue taskQueue,
        IWebHostEnvironment hostEnvironment
    ) : ControllerBase
    {
        /// <summary>
        /// For use outside of job calls
        /// </summary>
        private readonly ApplicationDBContext m_DBContext = dbContext;
        //  Fields
        /// <summary>
        /// For accessing the ApplicationDBContext within job calls
        /// </summary>
        private readonly IDbContextFactory<ApplicationDBContext> m_DBContextFactory = dbContextFactory;
        /// <summary>
        /// For generating 3D models
        /// </summary>
        private readonly IWebCrawlerService m_WebCrawlerService = webCrawlerService;
        /// <summary>
        /// For consulting the status of sample creation jobs
        /// </summary>
        private readonly JobStatusManager<SampleDTO> m_JobStatusManager = jobStatusManager;
        /// <summary>
        /// For handling job queueing
        /// </summary>
        private readonly IBackgroundTaskQueue m_TaskQueue = taskQueue;
        /// <summary>
        /// For directory handling
        /// </summary>
        private readonly IWebHostEnvironment m_HostEnvironment = hostEnvironment;

        //  Methods
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(m_DBContext.Samples.Select(s => s.ToSampleDTO()));
        }
        [HttpGet("preview")]
        public IActionResult GetAllPreview()
        {
            return Ok(m_DBContext.Samples.Select(s => s.ToSamplePreviewDTO()));
        }
        [HttpGet("{id}")]
        public IActionResult GetByID([FromRoute] int id)
        {
            Sample? sample = m_DBContext.Samples.Find(id);
            return (sample != null) ? Ok(sample.ToSampleDTO()) : NotFound();
        }
        [HttpPost("start-job")]
        public async Task<IActionResult> StartCreateSampleJob(CreateSampleDTO createSampleDTO)
        {
            //  Store image for web crawler service
            IFormFile file = createSampleDTO.ModelImage;
            string temporaryFolder = Path.Combine(m_HostEnvironment.ContentRootPath, "tmp");

            //  Ensure temporary folder exists
            if (!Directory.Exists(temporaryFolder))
                Directory.CreateDirectory(temporaryFolder);

            //  Store image in temporary folder
            string imagePath = Path.Combine(temporaryFolder, file.FileName);
            using (Stream fileStream = new FileStream(imagePath, FileMode.Create)) { await file.CopyToAsync(fileStream); }

            //  Add job to background service
            Guid jobID = Guid.NewGuid();
            m_JobStatusManager.UpdateJobStatus(jobID, JobStatus.Processing);
            m_TaskQueue.QueueBackgroundWorkItem(async (token) => await CreateSampleAsync(createSampleDTO, imagePath, jobID, token));

            return Ok(new JobStatusDTO<SampleDTO>() { ID = jobID, Status = JobStatus.Processing, Data = null });
        }
        [HttpGet("job-status/{ID}")]
        public IActionResult GetJobStatus(Guid ID)
        {
            JobStatusDTO<SampleDTO>? jobStatus = m_JobStatusManager.GetJobStatus(ID);

            return (jobStatus == null) ? NotFound() : Ok(jobStatus);
        }
        private async Task CreateSampleAsync(CreateSampleDTO createSampleDTO, string imagePath, Guid jobID, CancellationToken token)
        {
            //  Download 3D model
            string downloadedFilePath = await m_WebCrawlerService.Download3DModelAsync(imagePath);

            //  Create sample
            Sample sample = new()
            {
                Name = createSampleDTO.Name,
                Description = createSampleDTO.Description,
                ModelPath = downloadedFilePath,
                Collections = []
            };

            //  Store sample in database
            using ApplicationDBContext context = m_DBContextFactory.CreateDbContext();
            context.Samples.Add(sample);
            await context.SaveChangesAsync(token);

            //  Update job status with results
            SampleDTO sampleDTO = new()
            {
                Name = sample.Name,
                Description = sample.Description,
                ModelFile = System.IO.File.ReadAllText(downloadedFilePath)
            };

            m_JobStatusManager.UpdateJobStatus(jobID, JobStatus.Completed, sampleDTO);
        }
    }
}