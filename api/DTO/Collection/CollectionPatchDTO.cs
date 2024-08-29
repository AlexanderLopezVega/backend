using api.Other;

namespace api.DTO.Collection
{
    public class CollectionPatchDTO
    {
        //  Properties
        public int ID { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string[]? Tags { get; set; }
        public PublicationStatus? PublicationStatus { get; set; }
        public List<int>? SampleIDs { get; set; }
    }
}