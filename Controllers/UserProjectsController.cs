using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkillLoop2.Data;
using SkillLoop2.Models;

namespace SkillLoop2.Controllers
{
    public class UserProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;


        public UserProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UserProjects
        public async Task<IActionResult> Index(string term = "", int page = 1, string sortOrder = "date_desc")
        {
            int pageSize = 4;

            ViewData["CurrentSort"] = sortOrder;
            string currentSort = !string.IsNullOrEmpty(sortOrder) ? sortOrder :
                      (ViewData["CurrentSort"] as string ?? "date_desc");
            ViewData["CurrentFilter"] = term;
            string currentTerm = term ?? (ViewData["CurrentTerm"] as string ?? string.Empty);

            ViewData["DateSortParm"] = sortOrder == "date_asc" ? "date_desc" : "date_asc";
            ViewData["NameSortParm"] = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewData["CurrentPage"] = page;
            ViewData["CurrentTerm"] = term;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


            var baseQuery = _context.UserProject
            .Where(u => u.UserId == userId)
            .Include(u => u.Project)
                .ThenInclude(p => p.ProjectImages)
            .Select(u => u.Project)
            .AsQueryable();

            switch (sortOrder)
            {
                case "date_asc":
                    baseQuery = baseQuery.OrderBy(u => u.PublishDate);
                    break;
                case "name_asc":
                    baseQuery = baseQuery.OrderBy(u => u.ProjectName);
                    break;
                case "name_desc":
                    baseQuery = baseQuery.OrderByDescending(u => u.ProjectName);
                    break;
                default:  
                    baseQuery = baseQuery.OrderByDescending(u => u.PublishDate);
                    break;
            }

            int totalItems = await baseQuery.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            page = Math.Max(1, Math.Min(page, totalPages));

            var items = await baseQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["TotalPages"] = totalPages;
            ViewData["TotalItems"] = totalItems;

            return View(items);
        }

        // GET: UserProjects/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userProject = await _context.UserProject
                .Include(u => u.Project)
                .Include(u => u.User)
                .FirstOrDefaultAsync(m => m.UserProjectID == id);
            if (userProject == null)
            {
                return NotFound();
            }

            return View(userProject);
        }

        // GET: UserProjects/Create
        public IActionResult Create()
        {
            ViewData["ProjectId"] = new SelectList(_context.Projects, "ProjectId", "ProjectId");
            ViewData["UserId"] = new SelectList(_context.Users, "UserID", "UserID");
            return View();
        }

        // POST: UserProjects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserProjectID,UserId,ProjectId")] UserProject userProject)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userProject);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "ProjectId", "ProjectId", userProject.ProjectId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserID", "UserID", userProject.UserId);
            return View(userProject);
        }

        // GET: UserProjects/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userProject = await _context.UserProject.FindAsync(id);
            if (userProject == null)
            {
                return NotFound();
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "ProjectId", "ProjectId", userProject.ProjectId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserID", "UserID", userProject.UserId);
            return View(userProject);
        }

        // POST: UserProjects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("UserProjectID,UserId,ProjectId")] UserProject userProject)
        {
            if (id != userProject.UserProjectID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userProject);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserProjectExists(userProject.UserProjectID))
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
            ViewData["ProjectId"] = new SelectList(_context.Projects, "ProjectId", "ProjectId", userProject.ProjectId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserID", "UserID", userProject.UserId);
            return View(userProject);
        }

        // GET: UserProjects/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userProject = await _context.UserProject
                .Include(u => u.Project)
                .Include(u => u.User)
                .FirstOrDefaultAsync(m => m.UserProjectID == id);
            if (userProject == null)
            {
                return NotFound();
            }

            return View(userProject);
        }

        // POST: UserProjects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var userProject = await _context.UserProject.FindAsync(id);
            if (userProject != null)
            {
                _context.UserProject.Remove(userProject);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserProjectExists(string id)
        {
            return _context.UserProject.Any(e => e.UserProjectID == id);
        }
    }
}
