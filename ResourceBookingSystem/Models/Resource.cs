using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ResourceBookingSystem.Models
{
    public class Resource
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Location is required")]
        public string Location { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0")]
        public int Capacity { get; set; }
        public bool IsAvailable { get; set; } = true;

        // The [ValidateNever] attribute is used to prevent this property from being validated during resource form submission.
        [ValidateNever]
        // This declares a property called Bookings in the Resource class.
        // It holds a collection (list) of Booking objects related to that resource.
        // The type ICollection<Booking> means it can hold multiple bookings.
        public ICollection<Booking> Bookings { get; set; }
    }
}
