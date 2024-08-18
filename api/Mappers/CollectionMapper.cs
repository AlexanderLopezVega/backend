using api.DTO.Collection;
using api.Models;

namespace api.Mappers
{
    public static class CollectionMapper
    {
        //  Methods
        public static CollectionDTO ToCollectionDTO(this Collection collection) => new()
        {
            Name = collection.Name,
            Description = collection.Description,
        };
        public static CollectionPreviewDTO ToCollectionPreviewDTO(this Collection collection) => new()
        {
            ID = collection.ID,
            Name = collection.Name,
            Description = collection.Description,
        };
    }
}