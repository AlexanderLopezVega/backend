namespace api.DTO.Sample
{
    public class SampleDTO
    {
        //  Properties
        public int ID { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string ModelFile { get; set; }
    }
}