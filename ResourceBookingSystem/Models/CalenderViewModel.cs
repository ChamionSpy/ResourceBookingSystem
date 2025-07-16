namespace ResourceBookingSystem.Models
{
    public class CalenderViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }

        // This property holds the list of days in the month
        public List<CalendarDay> Days { get; set; } = new List<CalendarDay>();
    }
    public class CalendarDay
    {
        public DateTime Date { get; set; }

        // This property holds the bookings for the day
        public List<BookingInfo> Bookings { get; set; } = new List<BookingInfo>();
        public bool HasBookings => Bookings.Count > 0;
    }

    public class BookingInfo
    {
        public string ResourceName { get; set; }
        public string TimeRange { get; set; }
        public string BookedBy { get; set; }
    }
}
