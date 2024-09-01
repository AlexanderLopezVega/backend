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
            Tags = sample.Tags,
            PublicationStatus = sample.PublicationStatus,
            ModelFile = File.ReadAllText(sample.ModelPath),
            CollectionIDs = sample.Collections != null ? [ ..sample.Collections.Select(c => c.ID) ] : [],
        };
        public static SamplePreviewDTO ToSamplePreviewDTO(this Sample sample) => new()
        {
            ID = sample.ID,
            Name = sample.Name,
            Description = sample.Description,
        };
    }
}