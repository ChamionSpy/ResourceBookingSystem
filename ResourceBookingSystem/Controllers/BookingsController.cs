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
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            try
            {
                var bookings = await _context.Bookings
                    .Include(b => b.Resource)
                    .ToListAsync();
                return View(bookings);
            }
            catch (Exception ex)
            {
                // Log error (consider using a logging framework)
                Debug.WriteLine($"Error fetching bookings: {ex.Message}");
                return View("Error");
            }
        }
        // GET: Bookings Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            try
            {
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

        // GET: Bookings Create
        public IActionResult Create()
        {
            ViewBag.Resources = new SelectList(_context.Resource, "Id", "Name");
            return View();
        }

        // POST: Bookings Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ResourceId,StartTime,EndTime,BookedBy,Purpose")] Booking booking)
        {

            // Manual validation
            if (booking.ResourceId == 0)
            {
                ModelState.AddModelError("ResourceId", "Please select a resource");
            }

            if (booking.EndTime <= booking.StartTime)
            {
                ModelState.AddModelError("", "End time must be after start time.");
            }

            ModelState.Remove("Resource");

            if (!ModelState.IsValid)
            {
                // Repopulate resources dropdown before returning view
                ViewBag.Resources = new SelectList(_context.Resource, "Id", "Name", booking.ResourceId);
                return View(booking);
            }

            try
            {
                // Check for booking conflicts
                bool hasConflict = await _context.Bookings
                    .AnyAsync(b => b.ResourceId == booking.ResourceId &&
                                  booking.StartTime < b.EndTime &&
                                  booking.EndTime > b.StartTime);

                if (hasConflict)
                {
                    ModelState.AddModelError("", "This resource is already booked during the requested time. Please choose another slot or resource, or adjust your times");
                    ViewBag.Resources = new SelectList(_context.Resource, "Id", "Name", booking.ResourceId);
                    return View(booking);
                }

                // Save booking if valid
                _context.Add(booking);
                await _context.SaveChangesAsync();
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

        // GET: Bookings Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var booking = await _context.Bookings.FindAsync(id);
                if (booking == null)
                {
                    return NotFound();
                }
                ViewBag.Resources = new SelectList(_context.Resource, "Id", "Name", booking.ResourceId);
                return View(booking);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading edit form: {ex.Message}");
                return View("Error");
            }

        }

        // POST: Bookings Edit
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
                _context.Update(booking);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
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

        // GET: Bookings Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
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

        // POST: Bookings Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(id);
                if (booking != null)
                {
                    _context.Bookings.Remove(booking);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting booking: {ex.Message}");
                return View("Error");
            }
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }
    }
}
