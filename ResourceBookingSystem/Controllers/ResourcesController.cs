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

        // GET: Resources - This shows the list of all resources.
        // You can optionally search by text and filter by availability.
        public async Task<IActionResult> Index(string searchString, bool? isAvailable)
        {
            try
            {
                // Start with all resources from the database
                IQueryable<Resource> resourcesQuery = _context.Resource;

                // If the user entered a search term, filter resources by name, description or location (case-insensitive)
                if (!string.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.ToLower(); // This makes search case-insensitive
                    resourcesQuery = resourcesQuery.Where(r =>
                        r.Name.ToLower().Contains(searchString) ||
                        (r.Description != null && r.Description.ToLower().Contains(searchString)) ||
                        (r.Location != null && r.Location.ToLower().Contains(searchString)));
                }

                // Apply availability filter
                // If the user wants to filter by availability, we check if isAvailable is true or false
                if (isAvailable.HasValue)
                {
                    resourcesQuery = resourcesQuery.Where(r => r.IsAvailable == isAvailable.Value);
                }

                // Maintain filters in the view
                // Keep the filter values so the view can remember what was searched or selected
                ViewData["CurrentFilter"] = searchString;
                ViewData["IsAvailable"] = isAvailable;

                // Execute the query and get the final list of resources asynchronously (so it doesn't block the server)
                var resources = await resourcesQuery.ToListAsync();

                return View(resources);
            }
            catch (Exception ex)
            {
                // Add an error message that the view can display to the user
                ModelState.AddModelError("", "Unable to load resources. Please try again later.");

                // Log the exact error details to the debug console for developers to investigate
                System.Diagnostics.Debug.WriteLine($"Error loading resources: {ex.Message}");
                return View(new List<Resource>());
            }
        }

        // GET: Resources Details - Shows detailed info about a specific resource, including its bookings.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Try to find the resource by its ID and also include its related bookings
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
                // Handle errors gracefully
                ModelState.AddModelError("", "Unable to load resource details.");
                System.Diagnostics.Debug.WriteLine($"Error fetching details: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }

        }

        // GET: Resources Create - Shows a blank form to create a new resource
        public IActionResult Create()
        {
            return View();
        }

        // POST: Resources Create - Handles form submission to add a new resource to the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Resource resource)
        {
            if (!ModelState.IsValid)
            {
                // If validation fails, log the errors so developers know what went wrong
                foreach (var entry in ModelState)
                {
                    if (entry.Value.Errors.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"Field: {entry.Key} - Error: {entry.Value.Errors[0].ErrorMessage}");
                    }
                }
                return View(resource);
            }

            try
            {
                // If valid, add the new resource to the database context
                _context.Add(resource);

                // Save the changes to the database asynchronously
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Handle errors when saving to the database:
                ModelState.AddModelError("", "Failed to create resource. Please try again.");
                System.Diagnostics.Debug.WriteLine($"Create error: {ex.Message}");
                return View(resource);
            }
        }


        // GET: Resources Edit - Shows a form with existing resource data so the user can edit it
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
                // If loading resource fails, show the error below and redirect to the resource list
                ModelState.AddModelError("", "Failed to load resource for editing.");
                System.Diagnostics.Debug.WriteLine($"Edit GET error: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }

        }

        // POST: Resources Edit - Handles submission of the edited resource data
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Location,Capacity,IsAvailable")] Resource resource)
        {
            if (id != resource.Id) return NotFound();

            if (!ModelState.IsValid) return View(resource);

            try
            {
                // Update the resource in the database context
                _context.Update(resource);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                // If the resource was deleted or changed by someone else during editing
                // Return "Not Found"
                if (!ResourceExists(resource.Id))
                    return NotFound();
                else
                    throw; // Re-throw error if something else went wrong
            }
            catch (Exception ex)
            {
                // General error handler for update failure
                ModelState.AddModelError("", "Failed to update resource.");
                System.Diagnostics.Debug.WriteLine($"Edit POST error: {ex.Message}");
                return View(resource);
            }
        }

        // GET: Resources Delete - Shows the confirmation page before deleting a resource
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Try to find the resource with the given id in the database.
                var resource = await _context.Resource.FirstOrDefaultAsync(m => m.Id == id);

                if (resource == null) return NotFound();
                return View(resource);
            }
            catch (Exception ex)
            {
                // If something went wrong, show an error message on the page
                ModelState.AddModelError("", "Failed to load resource for deletion.");
                System.Diagnostics.Debug.WriteLine($"Delete GET error: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Resources Delete - Actually deletes the resource after confirmation
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Try to find the resource in the database using the ID
                var resource = await _context.Resource.FindAsync(id);
                if (resource != null)
                {
                    // This removes the resource from the database context
                    _context.Resource.Remove(resource);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // If somethin went wrong, show an error message
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
