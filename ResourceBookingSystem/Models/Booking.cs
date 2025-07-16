using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace ResourceBookingSystem.Models
{
    // This class represents a booking made for a resource
    // It hols all the information needed to describe a booking
    public class Booking
    {
        // Unique ID for the booking (primary key in the database)
        // This is used to uniquely identify the booking in the system
        public int Id { get; set; }

        // The ID of the resource being booked (foreign key)
        [Required(ErrorMessage = "Please select a resource.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid resource")]
        public int? ResourceId { get; set; }

        // We use the 'Required' attribute for validation
        // To ensure that these fields are not null or blank when making a booking.
        [Required(ErrorMessage = "Start Time is required.")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End Time is required.")]
        public DateTime EndTime { get; set; }

        [Required(ErrorMessage = "Booked By is required.")]
        public string BookedBy { get; set; }

        [Required(ErrorMessage = "Purpose is required.")]
        public string Purpose { get; set; }

        // This property allows access to the full Resource object linked to the booking.
        // It's automatically loaded by EF using the ResourceId.
        // The [BindNever] attribute ensures that this property does not get set via form inputs accidentally.
        [BindNever]
        public Resource Resource { get; set; }
          
    }
}
