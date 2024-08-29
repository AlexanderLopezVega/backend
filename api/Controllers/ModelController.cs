using api.Data;
using api.DTO.Model;
using api.Managers.Jobs;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/models"), ApiController]
    public class ModelController
    (
        ApplicationDBContext context,
         IWebHostEnvironment hostEnvironment,
         IWebCrawlerService webCrawlerService,
         JobStatusManager<ModelDTO> jobStatusManager,
         IBackgroundTaskQueue taskQueue,
         ICleanupService cleanupService
    ) : ControllerBase
    {
        //  Fields
        private readonly ApplicationDBContext m_Context = context;
        private readonly IWebHostEnvironment m_HostEnvironment = hostEnvironment;
        private readonly IWebCrawlerService m_WebCrawlerService = webCrawlerService;
        private readonly JobStatusManager<ModelDTO> m_JobStatusManager = jobStatusManager;
        private readonly IBackgroundTaskQueue m_TaskQueue = taskQueue;
        private readonly ICleanupService m_CleanupService = cleanupService;

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
        [HttpPost("jobs")]
        public async Task<IActionResult> StartJob(CreateModelDTO createModelDTO)
        {
            Guid jobID = Guid.NewGuid();

            //  Store image for web crawler service
            if (!Directory.Exists("Public/Images/Temp"))
                Directory.CreateDirectory("Public/Images/Temp");

            string temporaryFolder = Path.Combine(m_HostEnvironment.ContentRootPath, "Public/Images/Temp");

            IFormFile file = createModelDTO.ModelImage;
            string imagePath = Path.Combine(temporaryFolder, $"{jobID}{Path.GetExtension(file.FileName)}");
            using (Stream fileStream = new FileStream(imagePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            //  Add job to background service
            Console.WriteLine("adding job");
            m_JobStatusManager.UpdateJobStatus(jobID, JobStatus.Processing);
            m_TaskQueue.QueueBackgroundWorkItem(async (token) =>
            {
                Console.WriteLine("starting job");
                await CreateModelAsync(imagePath, jobID, token);
                ScheduleDataCleanup(imagePath, jobID);
                Console.WriteLine("job finished");
            });

            return Ok(new JobStatusDTO<ModelDTO>() { ID = jobID, Status = JobStatus.Processing, Data = null });
        }
        [HttpGet("jobs/{id}")]
        public IActionResult GetJob(Guid id)
        {
            JobStatusDTO<ModelDTO>? jobStatus = m_JobStatusManager.GetJobStatus(id);

            return (jobStatus == null) ? NotFound() : Ok(jobStatus);
        }
        private async Task CreateModelAsync(string imagePath, Guid jobID, CancellationToken token)
        {
            //  Download 3D model
            string filePath = await m_WebCrawlerService.Download3DModelAsync(jobID, imagePath);

            //  Update job status with results
            string modelFile = System.IO.File.ReadAllText(filePath);
            ModelDTO modelDTO = new() { ModelFile = modelFile };

            m_JobStatusManager.UpdateJobStatus(jobID, JobStatus.Completed, modelDTO);
        }
        private void ScheduleDataCleanup(string imagePath, Guid id)
        {
            m_CleanupService.ScheduleCleanup((token) =>
            {
                TryDeleteTemporaryModel();
                TryDeleteTemporaryImage();
            });

            void TryDeleteTemporaryModel()
            {
                //  Calculate model filepath
                string filePath = $"Public/3D Models/Temp/{id}.obj";
                bool fileExists = System.IO.File.Exists(filePath);

                //  Model has been removed from Temp folder (probably due to form confirmation or cancellation)
                if (!fileExists) return;

                //  Delete file
                System.IO.File.Delete(filePath);
            }
            void TryDeleteTemporaryImage()
            {
                //  Calculate image filepath
                bool fileExists = System.IO.File.Exists(imagePath);

                //  Model has been removed from Temp folder (probably due to form confirmation or cancellation)
                if (!fileExists) return;

                //  Delete file
                System.IO.File.Delete(imagePath);
            }
        }
    }
}