using backend.DTO.Sample;
using backend.Models;

namespace backend.Mappers
{
    public static class SampleMappers
    {
        //  Methods
        public static SampleDTO ToSampleDTO(this Sample sampleModel) => new()
        {
            ID = sampleModel.ID,
            Name = sampleModel.Name,
            ModelFile = File.ReadAllText(sampleModel.ModelPath)
        };     
    }
}