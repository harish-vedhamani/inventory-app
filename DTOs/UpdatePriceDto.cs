using System.ComponentModel.DataAnnotations;

namespace Playground.DTOs
{
    public class UpdatePriceDto
    {
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
    }
}
