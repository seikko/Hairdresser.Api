using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hairdresser.Api.Models
{
    [Table("business_config")]
    public class BusinessConfig
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("config_key")]
        [Required]
        [StringLength(50)]
        public string ConfigKey { get; set; } = null!;

        [Column("config_value")]
        [Required]
        [StringLength(100)]
        public string ConfigValue { get; set; } = null!;

        [Column("description")]
        public string? Description { get; set; }
    }
}

