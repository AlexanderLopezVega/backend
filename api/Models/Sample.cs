using api.Other;

namespace api.Models
{
    public class Sample
    {
        //  Properties
        public int ID { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string[]? Tags { get; set; }
        public PublicationStatus PublicationStatus { get; set; }
        public required string ModelPath { get; set; }

        public required User User { get; set; }
        public List<Collection>? Collections { get; set; }
    }
}