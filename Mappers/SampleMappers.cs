using api.DTO.Sample;
using api.Models;

namespace api.Mappers
{
    public static class SampleMappers
    {
        //  Methods
        public static SampleDTO ToSampleDTO(this Sample sampleModel) => new()
        {
            ID = sampleModel.ID,
            Name = sampleModel.Name,
        };     
    }
}