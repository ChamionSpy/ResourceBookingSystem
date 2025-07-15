using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace ResourceBookingSystem.Models
{
    public class Booking
    {
        public int Id { get; set; }

        // This points to the resource beign booked
        [Required(ErrorMessage = "Please select a resource.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid resource")]
        public int? ResourceId { get; set; }
        [Required(ErrorMessage = "Start Time is required.")]
        public DateTime StartTime { get; set; }
        [Required(ErrorMessage = "End Time is required.")]
        public DateTime EndTime { get; set; }
        [Required(ErrorMessage = "Booked By is required.")]
        public string BookedBy { get; set; }

        [Required(ErrorMessage = "Purpose is required.")]
        public string Purpose { get; set; }

        // This lets us get the full resource details from a booking
        [BindNever]
        public Resource Resource { get; set; }

        // Custom validation: for resource selection and EndTime must be after StartTime
        
    }
}
