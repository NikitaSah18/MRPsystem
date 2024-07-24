using System.ComponentModel.DataAnnotations;

namespace BOM.Models
{
    public class Storage
    {
        [Key]
        public int Id { get; set; }

        public int? SpecificationId { get; set; }

        public DateTime StorageDate { get; set; }

        public string? Description { get; set; }

        public double? Count { get; set; }

        public string? MeasureUnit { get; set; }

        public Specification? Specification { get; set; }
    }
}
