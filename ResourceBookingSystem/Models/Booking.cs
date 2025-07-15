namespace ResourceBookingSystem.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int ResourceId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string BookedBy { get; set; }
        public string Purpose { get; set; }

        // This points to the resource beign booked
        public int ResourceID { get; set; }

        // This lets us get the full resource details from a booking
        public Resource Resource { get; set; }
    }
}
