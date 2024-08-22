using api.Other;

namespace api.DTO.Collection
{
    public class CreateCollectionDTO
    {
        //  Properties
        public required string Name { get; set; }
        public string? Description { get; set; }
        public PublicationStatus PublicationStatus { get; set; }
        public int UserID { get; set; }
        public List<int>? SamplesID { get; set; }
    }
}