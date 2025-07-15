using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ResourceBookingSystem.Data;
using ResourceBookingSystem.Models;

namespace ResourceBookingSystem.Controllers
{
    public class ResourcesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ResourcesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Resources
        public async Task<IActionResult> Index(string searchString)
        {
            try
            {
                IQueryable<Resource> resourcesQuery = _context.Resource;

                // Apply search filter if searchString is provided
                if (!string.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.ToLower();
                    resourcesQuery = resourcesQuery.Where(r =>
                        r.Name.ToLower().Contains(searchString) ||
                        (r.Description != null && r.Description.ToLower().Contains(searchString)) ||
                        (r.Location != null && r.Location.ToLower().Contains(searchString)));
                }

                // Pass search value to view to maintain state
                ViewData["CurrentFilter"] = searchString;

                var resources = await resourcesQuery.ToListAsync();
                return View(resources);
            }
            catch (Exception ex)
            {
                // Log and handle errors
                ModelState.AddModelError("", "Unable to load resources. Please try again later.");
                System.Diagnostics.Debug.WriteLine($"Error loading resources: {ex.Message}");
                return View(new List<Resource>());
            }
        }

        // GET: Resources Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var resource = await _context.Resource
                .Include(r => r.Bookings) // Include bookings for this resource
                .FirstOrDefaultAsync(m => m.Id == id);
                if (resource == null)
                {
                    return NotFound();
                }

                return View(resource);
            }
            catch (Exception ex)
            {
                // Log and handle
                ModelState.AddModelError("", "Unable to load resource details.");
                System.Diagnostics.Debug.WriteLine($"Error fetching details: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }

        }

        // GET: Resources Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Resources Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Resource resource)
        {
            if (!ModelState.IsValid)
            {
                // Log all validation errors
                foreach (var entry in ModelState)
                {
                    if (entry.Value.Errors.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"Field: {entry.Key} - Error: {entry.Value.Errors[0].ErrorMessage}");
                    }
                }

                return View(resource); // Return view to show errors
            }

            try
            {
                // Proceed if valid
                _context.Add(resource);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to create resource. Please try again.");
                System.Diagnostics.Debug.WriteLine($"Create error: {ex.Message}");
                return View(resource);
            }
        }


        // GET: Resources Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var resource = await _context.Resource.FindAsync(id);
                if (resource == null)
                {
                    return NotFound();
                }
                return View(resource);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to load resource for editing.");
                System.Diagnostics.Debug.WriteLine($"Edit GET error: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }

        }

        // POST: Resources Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Location,Capacity,IsAvailable")] Resource resource)
        {
            if (id != resource.Id) return NotFound();

            if (!ModelState.IsValid) return View(resource);

            try
            {
                _context.Update(resource);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResourceExists(resource.Id))
                    return NotFound();
                else
                    throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to update resource.");
                System.Diagnostics.Debug.WriteLine($"Edit POST error: {ex.Message}");
                return View(resource);
            }
        }

        // GET: Resources Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var resource = await _context.Resource.FirstOrDefaultAsync(m => m.Id == id);
                if (resource == null) return NotFound();

                return View(resource);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to load resource for deletion.");
                System.Diagnostics.Debug.WriteLine($"Delete GET error: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Resources Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var resource = await _context.Resource.FindAsync(id);
                if (resource != null)
                {
                    _context.Resource.Remove(resource);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to delete resource.");
                System.Diagnostics.Debug.WriteLine($"Delete POST error: {ex.Message}");
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        private bool ResourceExists(int id)
        {
            return _context.Resource.Any(e => e.Id == id);
        }
    }
}
