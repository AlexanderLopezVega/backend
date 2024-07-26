namespace api.DTO.Sample
{
    public class CreateSampleDTO
    {
        //  Properties
        public int ID { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required IFormFile ModelImage { get; set; }
    }
}