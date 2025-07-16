using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Elfie.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ResourceBookingSystem.Data;
using ResourceBookingSystem.Models;

namespace ResourceBookingSystem.Controllers
{
    // This controller manages bookings for resources in the system.
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings - Show all bookings, optionally filtered by a specific date
        public async Task<IActionResult> Index(DateTime? bookingDate)
        {
            try
            {
                // Start building query to get bookings and include their related resource info
                var bookingsQuery = _context.Bookings
                    .Include(b => b.Resource) // Load Resource linked to each booking
                    .AsQueryable(); // Makes it ready for more filtering

                // If the user filtered by a date, narrow results to bookings active on that date
                if (bookingDate.HasValue)
                {
                    var selectedDate = bookingDate.Value.Date; // Just the date, no time
                    bookingsQuery = bookingsQuery.Where(b =>
                    b.StartTime.Date <= selectedDate && b.EndTime.Date >= selectedDate);
                }

                // Keep the filter value to show back in the view's search box
                ViewData["BookingDate"] = bookingDate?.ToString("yyyy-MM-dd");

                // Execute query asynchronously and get list of bookings
                var bookings = await bookingsQuery.ToListAsync();

                return View(bookings);
            }
            catch (Exception ex)
            {
                // If something went wrong, log the error for debugging and show an error page
                Debug.WriteLine($"Error fetching bookings: {ex.Message}");
                return View("Error");
            }
        }

        // GET: Bookings Details - Show details of a specific booking
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            try
            {
                // Find booking by ID, include the linked resource details
                var booking = await _context.Bookings
                    .Include(b => b.Resource)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (booking == null) return NotFound();

                return View(booking);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching booking details: {ex.Message}");
                return View("Error");
            }
        }

        // GET: Bookings Create - Shows form to create new booking
        public IActionResult Create()
        {
            // Prepare dropdown list of resources for the form
            ViewBag.Resources = new SelectList(_context.Resource, "Id", "Name");
            return View();
        }

        // POST: Bookings Create -  Hnandles form submission to create a new booking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ResourceId,StartTime,EndTime,BookedBy,Purpose")] Booking booking)
        {

            // Manual validation checks:
            // Check if ResourceId is not selected (0 means no resource selected)
            if (booking.ResourceId == 0)
            {
                // Add model error to indicate resource selection is required
                ModelState.AddModelError("ResourceId", "Please select a resource");
            }

            // Check if EndTime is before or equal to StartTime
            if (booking.EndTime <= booking.StartTime)
            {
                // Add model error to indicate end time must be after start time
                ModelState.AddModelError("", "End time must be after start time.");
            }

            // Tell system to ignore Resource navigation property validation
            // This is because we are not binding it from the form
            ModelState.Remove("Resource");

            if (!ModelState.IsValid)
            {
                // Repopulate resources dropdown before returning view
                ViewBag.Resources = new SelectList(_context.Resource, "Id", "Name", booking.ResourceId);
                return View(booking); // Return the view with validation errors
            }

            try
            {
                // Check for booking conflicts
                // This checks if there are any existing bookings for the same resource that overlap with the requested time
                bool hasConflict = await _context.Bookings
                    .AnyAsync(b => b.ResourceId == booking.ResourceId &&
                                  booking.StartTime < b.EndTime &&
                                  booking.EndTime > b.StartTime);

                // If there is a conflict, add an error to the model state
                if (hasConflict)
                {
                    ModelState.AddModelError("", "This resource is already booked during the requested time. Please choose another slot or resource, or adjust your times");
                    ViewBag.Resources = new SelectList(_context.Resource, "Id", "Name", booking.ResourceId);
                    return View(booking);
                }

                // Save booking if valid
                // This adds the new booking to the database context
                _context.Add(booking);
                await _context.SaveChangesAsync();

                // Redirect to the booking list page after success
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating booking: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while saving the booking.");
                ViewBag.Resources = new SelectList(_context.Resource, "Id", "Name", booking.ResourceId);
                return View(booking);
            }

        }

        // GET: Bookings Edit - Shows form to edit an existing booking
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Find the booking by ID, including the linked resource details
                var booking = await _context.Bookings.FindAsync(id);
                if (booking == null)
                {
                    return NotFound();
                }

                // Prepare dropdown list of resources for the form
                ViewBag.Resources = new SelectList(_context.Resource, "Id", "Name", booking.ResourceId);
                return View(booking);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading edit form: {ex.Message}");
                return View("Error");
            }

        }

        // POST: Bookings Edit - Handles form submission to update an existing booking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ResourceId,StartTime,EndTime,BookedBy,Purpose")] Booking booking)
        {
            if (id != booking.Id)
            {
                return NotFound();
            }

            // Manual validations like in Create
            if (booking.ResourceId == 0)
            {
                ModelState.AddModelError("ResourceId", "Please select a resource");
            }

            if (booking.EndTime <= booking.StartTime)
            {
                ModelState.AddModelError("", "End time must be after start time.");
            }

            // Remove navigation property validation (if any)
            ModelState.Remove("Resource");

            if (!ModelState.IsValid)
            {
                // Repopulate dropdown before returning view
                ViewBag.Resources = new SelectList(_context.Resource, "Id", "Name", booking.ResourceId);
                return View(booking);
            }

            try
            {
                // Try to update the booking in the database
                _context.Update(booking);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // If a concurrency error occurs (e.g., another user updated the booking at the same time)
                if (!BookingExists(booking.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Bookings Delete - Shows confirmation page to delete a booking
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Find the booking by ID, including the linked resource details
                var booking = await _context.Bookings
                    .Include(b => b.Resource)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (booking == null) return NotFound();

                return View(booking);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading delete view: {ex.Message}");
                return View("Error");
            }
        }

        // POST: Bookings Delete - Actually deletes the booking
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Find the booking by ID and remove it from the database
                var booking = await _context.Bookings.FindAsync(id);

                if (booking != null)
                {
                    _context.Bookings.Remove(booking);
                }

                // Save changes to the database
                await _context.SaveChangesAsync();
                // Redirect to the booking list page after deletion
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting booking: {ex.Message}");
                return View("Error");
            }
        }

        // GET: Bookings Dashboard - Shows a calendar view of bookings for a specific month
        public async Task<IActionResult> Dashboard(int? year, int? month)
        {
            try
            {
                // If no year or month is provided, use the current date
                var currentDate = DateTime.Now;
                var viewModel = new CalenderViewModel
                {
                    Year = year ?? currentDate.Year,
                    Month = month ?? currentDate.Month,
                };

                // Get all bookings for the month
                // Calculate the first and last day of the month
                var firstDayOfMonth = new DateTime(viewModel.Year, viewModel.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                // Fetch bookings for the month from the database and include related resource info
                var bookings = await _context.Bookings
                    .Include(b => b.Resource)
                    .Where(b => b.StartTime >= firstDayOfMonth && b.EndTime <= lastDayOfMonth)
                    .ToListAsync();

                // Populate calendar days
                // Loop through each day of the month and create a CalendarDay object
                for (var date = firstDayOfMonth; date <= lastDayOfMonth; date = date.AddDays (1))
                {
                    // Create a new CalendarDay object for each date
                    var day = new CalendarDay { Date = date };
                    var dailyBookings = bookings.Where(b => b.StartTime.Date <= date.Date && b.EndTime.Date >= date);

                    // Add bookings for the day
                    foreach (var booking in dailyBookings)
                    {
                        // Add booking info to the day's bookings list
                        day.Bookings.Add(new BookingInfo
                        {
                            // Set resource name, time range, and booked by info
                            ResourceName = booking.Resource?.Name ?? "Unknown",
                            StartTime = booking.StartTime, 
                            EndTime = booking.EndTime,
                            BookedBy = booking.BookedBy
                        });
                    }

                    // Add the day to the view model's days list
                    viewModel.Days.Add(day);
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading calendar: {ex.Message}");
                return View("Error");
            }
        }

        // Checks if a booking exists by ID
        // This is used to handle concurrency issues when editing bookings
        private bool BookingExists(int id)
        {
            // Check if any booking with the given ID exists in the database
            // This is used to verify if a booking still exists before updating or deleting it
            return _context.Bookings.Any(e => e.Id == id);
        }
    }
}
