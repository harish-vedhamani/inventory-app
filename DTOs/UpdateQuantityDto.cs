using System.ComponentModel.DataAnnotations;

namespace Playground.DTOs
{
    public class UpdateQuantityDto
    {
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
