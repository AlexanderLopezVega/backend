using api.Other;

namespace api.Models
{
    public class Collection
    {
        //  Properties
        public int ID { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string[]? Tags { get; set; }
        public PublicationStatus PublicationStatus { get; set; }

        public required User User { get; set; }
        public List<Sample>? Samples { get; set; }
    }
}