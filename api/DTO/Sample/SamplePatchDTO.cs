using api.Other;

namespace api.DTO.Sample
{
    public class SamplePatchDTO
    {
        //  Properties
        public int ID { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string[]? Tags { get; set; }
        public PublicationStatus? PublicationStatus { get; set; }
    }
}