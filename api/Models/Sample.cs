namespace api.Models
{
    public class Sample
    {
        //  Properties
        public int ID { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string ModelPath { get; set; }
        public List<Collection>? Collections { get; set; }
    }
}