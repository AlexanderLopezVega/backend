using api.Other;
using api.Models;

namespace api.DTO.Sample
{
    public class SampleDTO
    {
        //  Properties
        public int ID { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string[]? Tags { get; set; }
        public PublicationStatus PublicationStatus { get; set; }
        public required string ModelFile { get; set; }
        public List<int>? CollectionIDs { get; set; }

    }
}