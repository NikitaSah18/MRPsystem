using System.ComponentModel.DataAnnotations;

namespace BOM.Models
{
    public class Specification
    {
        [Key]
        public int PositionId { get; set; }

        public int? ParentsId { get; set; }

        public string? Description { get; set; }

        public int? QuantityPerParent { get; set; }

        public string? UnitMeasurement { get; set; }

        
    }
}
