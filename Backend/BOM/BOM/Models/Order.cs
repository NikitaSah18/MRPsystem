using System.ComponentModel.DataAnnotations;

namespace BOM.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public int? SpecificationId { get; set; }
        
        public DateTime OrderDate { get; set; }

        public string? ClientName { get; set; }

        public double? Quantity { get; set; }

        public string? MeasureUnit { get; set; }
        
        public Specification? Specification { get; set; }


    }
}
