using api.DTO.Sample;
using api.Models;

namespace api.Mappers
{
    public static class SampleMappers
    {
        //  Methods
        public static SampleDTO ToSampleDTO(this Sample sample) => new()
        {
            ID = sample.ID,
            Name = sample.Name,
            Description = sample.Description,
            ModelFile = File.ReadAllText(sample.ModelPath),
        };
        public static SamplePreviewDTO ToSamplePreviewDTO(this Sample sample) => new()
        {
            ID = sample.ID,
            Name = sample.Name,
            Description = sample.Description,
        };
    }
}