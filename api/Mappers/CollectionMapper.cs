using api.DTO.Collection;
using api.Models;

namespace api.Mappers
{
    public static class CollectionMapper
    {
        //  Methods
        public static ViewCollectionDTO ToCollectionDTO(this Collection collection) => new()
        {
            ID = collection.ID,
            Name = collection.Name,
            Description = collection.Description,
            Tags = collection.Tags,
            UserID = collection.User.ID,
            SampleIDs = collection.Samples == null ? [] : [.. collection.Samples.Select(x => x.ID)],
        };

        public static CollectionPreviewDTO ToCollectionPreviewDTO(this Collection collection) => new()
        {
            ID = collection.ID,
            Name = collection.Name,
            Description = collection.Description,
        };
    }
}