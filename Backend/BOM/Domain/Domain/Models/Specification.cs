namespace BOM.Models
{
    public class Specification
    {
        public int PostionId { get; set; }

        public int ParentsId { get; set; }

        public string Description { get; set; }

        public int QuantityPerParent { get; set; }

        public string UnitMeasurement { get; set; }

        public ParentsPosition ParentsPosition { get; set; }
    }
}
