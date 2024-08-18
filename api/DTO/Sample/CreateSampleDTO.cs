using api.Other;

namespace api.DTO.Sample
{
    public class CreateSampleDTO
    {
        //  Properties
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string[]? Tags { get; set; }
        public PublicationStatus PublicationStatus { get; set; }
        public required Guid ModelID { get; set; }
    }
}