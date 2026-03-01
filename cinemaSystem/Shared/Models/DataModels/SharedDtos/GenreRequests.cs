using System.ComponentModel.DataAnnotations;

namespace Shared.Models.DataModels.SharedDtos
{
    public class CreateGenreRequest
    {
        [Required]
        [MaxLength(100)]
        public string GenreName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateGenreRequest
    {
        [Required]
        [MaxLength(100)]
        public string GenreName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; }
    }
}
