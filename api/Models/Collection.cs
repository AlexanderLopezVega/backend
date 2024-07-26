namespace api.Models
{
    public class Collection
    {
        //  Properties
        public int ID { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public List<Sample>? Samples {get; set;}
    }
}