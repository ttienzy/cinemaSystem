using System;

namespace Shared.Models.DataModels.SharedDtos
{
    public class GenreDto
    {
        public Guid Id { get; set; }
        public string? GenreName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
