using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SkillLoop2.Data;
using SkillLoop2.Models;
using SkillLoop2.ViewModels;

namespace SkillLoop2.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProjectsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Projects
        public async Task<IActionResult> Index(string term, int page = 1, string sortOrder = "date_desc")
        {

            int pageSize = 8;

            ViewData["CurrentSort"] = sortOrder;
            ViewData["DateSortParm"] = sortOrder == "date_asc" ? "date_desc" : "date_asc";
            ViewData["NameSortParm"] = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewData["CurrentPage"] = page;
            ViewData["CurrentTerm"] = term;
            var baseQuery = _context.Projects.Include(p => p.Category)
                .Include(p => p.ProjectImages).AsQueryable();

            if (!String.IsNullOrEmpty(term))
            {
                term = term.ToLower();
                baseQuery = baseQuery.Where(u =>
                u.ProjectName.ToLower().Contains(term) ||
                u.ProjectId.ToLower().Contains(term) ||
                u.Category.CategoryName.ToLower().Contains(term));
            }

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

        public async Task<IActionResult> IndexID(string id, int page = 1, string sortOrder = "date_desc", string searchString = "")
        {
            int pageSize = 4;

            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentPage"] = page;

            ViewData["DateSortParm"] = sortOrder == "date_asc" ? "date_desc" : "date_asc";
            ViewData["NameSortParm"] = sortOrder == "name_asc" ? "name_desc" : "name_asc";

            var baseQuery = _context.Projects
                .Where(p => p.CategoryId == id)
                .Include(p => p.ProjectImages)
                .AsQueryable();


            // Apply sorting
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


        // GET: Projects/Details/5
        public async Task<IActionResult> Details(string id)


        {
            if (id == null)
            {
                return NotFound();
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userReview = _context.Reviews
    .FirstOrDefault(r => r.ProjectId == id && r.UserId == userId);




            var project = await _context.Projects
                .Include(p => p.UserProjects)
                .Include(p => p.Category)
                .Include(p => p.ProjectFiles)
                .Include(p => p.ProjectImages)
                .Include(p => p.ProjectTechnologies)
                .FirstOrDefaultAsync(m => m.ProjectId == id);


            var reviews = await _context.Reviews
                .Where(r => r.ProjectId == id)
                .Include(r => r.User)
                .Include(r => r.Comments)
                .ToListAsync();

            if (project == null)
            {
                return NotFound();
            }

            var viewModel = new ProjectDetailsViewModel
            {
                Project = project,
                Reviews = reviews,
                AverageRating = reviews.Where(r => r.Rating > 0 && r.Rating <= 5).Any() ? project.Reviews.Average(r => r.Rating) : 0,
                TotalReviews = reviews.Where(r => r.Rating > 0 && r.Rating <= 5).Count(),
                UserRating = reviews.FirstOrDefault(r => r.UserId == userId)?.Rating,
                UserLike = userReview?.Like
            };



            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitRating([FromForm] string projectId, [FromForm] float rating)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized(); 
            }

            if (string.IsNullOrEmpty(projectId))
            {
                return NotFound(); // Handle error if projectId is null
            }

            var project = await _context.Projects
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null)
            {
                return NotFound(); // Handle error if project is not found
            }

            var existingReview = project.Reviews.FirstOrDefault(r => r.UserId == userId);

            if (existingReview != null)
            {
                existingReview.Rating = rating;
                existingReview.ReviewDate = DateTime.Now;
                _context.Reviews.Update(existingReview);

            }
            else
            {
                var newReview = new Review
                {
                    ProjectId = projectId,
                    UserId = userId,
                    Rating = rating,
                    ReviewDate = DateTime.Now,
                    Like = -1
                };
                _context.Reviews.Add(newReview);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Projects", new { id = projectId });
        }

        // GET: Projects/Create
        [Authorize]

        public IActionResult Create()
        {
            var project = new Models.Project
            {
                PublishDate = DateTime.Now,
            };

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            return View(project);
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Produces("application/json")]
        [Authorize]

        public async Task<IActionResult> Create(
            [Bind("ProjectName,Description,ProjectURL,PublishDate,CategoryId")] Models.Project project,
            List<IFormFile> files, List<IFormFile> images, List<string> ProjectTechnologies)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Validation failed",
                        errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                    });
                }

                var lastProject = _context.Projects
                    .AsEnumerable()
                    .OrderByDescending(p => int.Parse(p.ProjectId))
                    .FirstOrDefault();

                project.ProjectId = (lastProject != null ? int.Parse(lastProject.ProjectId) + 1 : 1).ToString();

                _context.Add(project);
                await _context.SaveChangesAsync();

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var lastUserProject = _context.UserProject
                    .AsEnumerable()
                    .OrderByDescending(up => int.Parse(up.UserProjectID))
                    .FirstOrDefault();
                var userProject = new UserProject
                {
                    UserId = userId,
                    ProjectId = project.ProjectId
                };

                userProject.UserProjectID = (lastUserProject != null ? int.Parse(lastUserProject.UserProjectID) + 1 : 1).ToString();



                _context.UserProject.Add(userProject);
                await _context.SaveChangesAsync();

                if (ProjectTechnologies.Count != 0)
                {
                    foreach (var technology in ProjectTechnologies)
                    {
                        var lastTechnology = _context.ProjectTechnology
                                .AsEnumerable()
                                .OrderByDescending(f => int.Parse(f.Id))
                                .FirstOrDefault();

                        var projectTech = new ProjectTechnology
                        {
                            Id = (lastTechnology != null ? int.Parse(lastTechnology.Id) + 1 : 1).ToString(),
                            ProjectId = project.ProjectId,
                            TechologyName = technology

                        };
                        _context.ProjectTechnology.Add(projectTech);
                        await _context.SaveChangesAsync();


                    }
                }

                // معالجة الملفات إذا وجدت
                if (files != null && files.Count > 0)
                {
                    var uploadsPath = Path.Combine(_env.WebRootPath, "uploads/images");

                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    foreach (var image in images)
                    {
                        if (image.Length > 0)
                        {
                            var fileExtension = Path.GetExtension(image.FileName).ToLower();


                            //if (file.Length > 10 * 1024 * 1024)
                            //{
                            //    continue;
                            //}

                            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

                            using (var fileStream = new FileStream(Path.Combine(uploadsPath, uniqueFileName), FileMode.Create))
                            {
                                await image.CopyToAsync(fileStream);
                            }

                            var lastImage = _context.ProjectImages
                                .AsEnumerable()
                                .OrderByDescending(f => int.Parse(f.Id))
                                .FirstOrDefault();

                            var projectImage = new ProjectImage
                            {
                                ImageUrl = $"/uploads/images/{uniqueFileName}",
                                ProjectId = project.ProjectId,
                                Id = (lastImage != null ? int.Parse(lastImage.Id) + 1 : 1).ToString()
                            };

                            _context.ProjectImages.Add(projectImage);
                            await _context.SaveChangesAsync();


                        }
                    }

                }

                if (files != null && files.Count > 0)
                {
                    var uploadsPath = Path.Combine(_env.WebRootPath, "uploads/files");

                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }


                    if (files != null && files.Count > 0)
                    {
                        foreach (var file in files)
                        {
                            if (file.Length > 0)
                            {
                                var fileExtension = Path.GetExtension(file.FileName).ToLower();


                                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

                                using (var fileStream = new FileStream(Path.Combine(uploadsPath, uniqueFileName), FileMode.Create))
                                {
                                    await file.CopyToAsync(fileStream);
                                }

                                var lastFile = _context.ProjectFiles
                                    .AsEnumerable()
                                    .OrderByDescending(f => int.Parse(f.ProjectFilesId))
                                    .FirstOrDefault();

                                var projectFile = new ProjectFile
                                {
                                    FileName = file.FileName,
                                    FilePath = $"/uploads/files/{uniqueFileName}",
                                    FileType = file.ContentType,
                                    FileExtension = fileExtension,
                                    FileSize = file.Length,
                                    ProjectId = project.ProjectId,
                                    ProjectFilesId = (lastFile != null ? int.Parse(lastFile.ProjectFilesId) + 1 : 1).ToString()
                                };

                                _context.ProjectFiles.Add(projectFile);
                                await _context.SaveChangesAsync();


                            }
                        }
                    }

                }


                return Json(new
                {
                    success = true,
                    message = "تم رفع المشروع والملفات بنجاح",
                    projectId = project.ProjectId,
                    redirectUrl = Url.Action("Details", new { id = project.ProjectId })
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new
                {

                    success = false,
                    message = "خطأ في الصلاحيات: " + ex.Message + EventLogEntryType.Error
                });
            }
            catch (IOException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطأ في نظام الملفات: " + ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "حدث خطأ غير متوقع: " + ex.Message
                });
            }
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.ProjectFiles)
                .Include(p => p.ProjectImages)
                .Include(p => p.ProjectTechnologies)
                .FirstOrDefaultAsync(m => m.ProjectId == id);
            if (project == null)
            {
                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", project.CategoryId);
            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ProjectId,ProjectName,Description,ProjectURL,PublishDate,CategoryId")] Models.Project project,
            List<IFormFile> files, List<IFormFile> images, List<string> ProjectTechnologies, List<string> DeletedFiles, List<string> DeletedImages, List<string> DeltedProjectTechnologies)
        {

            //var exProject = _context.Projects.FirstOrDefaultAsync(p => p.ProjectId == id);

            if (id != project.ProjectId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {

                    if (DeletedFiles != null && DeletedFiles.Any())
                    {
                        var filesToDelete = await _context.ProjectFiles
                            .Where(f => DeletedFiles.Contains(f.ProjectFilesId))
                            .ToListAsync();

                        _context.ProjectFiles.RemoveRange(filesToDelete);
                    }

                    if (DeletedImages != null && DeletedImages.Any())
                    {
                        var imagesToDelete = await _context.ProjectImages
                            .Where(f => DeletedImages.Contains(f.Id))
                            .ToListAsync();

                        _context.ProjectImages.RemoveRange(imagesToDelete);
                    }
                    var uploadsPath = Path.Combine(_env.WebRootPath, "uploads/images");

                    if (DeltedProjectTechnologies != null && DeltedProjectTechnologies.Any())
                    {
                        var technologiesToDelete = await _context.ProjectTechnology
                            .Where(f => DeltedProjectTechnologies.Contains(f.Id))
                            .ToListAsync();

                        _context.ProjectTechnology.RemoveRange(technologiesToDelete);
                    }

                    if (files != null && files.Count > 0)
                    {
                        foreach (var file in files)
                        {
                            if (file.Length > 0)
                            {
                                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

                                using (var fileStream = new FileStream(Path.Combine(uploadsPath, uniqueFileName), FileMode.Create))
                                {
                                    await file.CopyToAsync(fileStream);
                                }

                                var lastFile = _context.ProjectFiles
                                    .AsEnumerable()
                                    .OrderByDescending(f => int.Parse(f.ProjectFilesId))
                                    .FirstOrDefault();

                                var projectFile = new ProjectFile
                                {
                                    FileName = file.FileName,
                                    FilePath = $"/uploads/{uniqueFileName}",
                                    FileType = file.ContentType,
                                    FileExtension = fileExtension,
                                    FileSize = file.Length,
                                    ProjectId = project.ProjectId,
                                    ProjectFilesId = (lastFile != null ? int.Parse(lastFile.ProjectFilesId) + 1 : 1).ToString()
                                };

                                _context.ProjectFiles.Add(projectFile);
                                await _context.SaveChangesAsync();


                            }
                        }
                    }

                    if (images != null && images.Count > 0)
                    {
                        foreach (var image in images)
                        {
                            if (image.Length > 0)
                            {
                                var fileExtension = Path.GetExtension(image.FileName).ToLower();


                                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

                                using (var fileStream = new FileStream(Path.Combine(uploadsPath, uniqueFileName), FileMode.Create))
                                {
                                    await image.CopyToAsync(fileStream);
                                }

                                var lastImage = _context.ProjectImages
                                    .AsEnumerable()
                                    .OrderByDescending(f => int.Parse(f.Id))
                                    .FirstOrDefault();

                                var projectImage = new ProjectImage
                                {
                                    ImageUrl = $"/uploads/images/{uniqueFileName}",
                                    ProjectId = project.ProjectId,
                                    Id = (lastImage != null ? int.Parse(lastImage.Id) + 1 : 1).ToString()
                                };

                                _context.ProjectImages.Add(projectImage);
                                await _context.SaveChangesAsync();


                            }
                        }
                    }
                    if (ProjectTechnologies != null && ProjectTechnologies.Count != 0)
                    {
                        foreach (var technology in ProjectTechnologies)
                        {
                            bool exists = _context.ProjectTechnology.Any(pt =>
                                pt.ProjectId == project.ProjectId &&
                                pt.TechologyName.ToLower() == technology.ToLower());

                            if (!exists)
                            {
                                var lastTechnology = _context.ProjectTechnology
                                    .AsEnumerable()
                                    .OrderByDescending(f => int.Parse(f.Id))
                                    .FirstOrDefault();

                                var projectTech = new ProjectTechnology
                                {
                                    Id = (lastTechnology != null ? int.Parse(lastTechnology.Id) + 1 : 1).ToString(),
                                    ProjectId = project.ProjectId,
                                    TechologyName = technology
                                };
                                _context.ProjectTechnology.Add(projectTech);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    if (DeltedProjectTechnologies != null && DeltedProjectTechnologies.Any())
                    {
                        var technologiesToDelete = await _context.ProjectTechnology
                            .Where(f => DeltedProjectTechnologies.Contains(f.Id))
                            .ToListAsync();

                        _context.ProjectTechnology.RemoveRange(technologiesToDelete);
                    }



                    _context.Update(project);
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.ProjectId))
                    {
                        return NotFound();
                    }
                }
                return RedirectToAction("Index","Projects", new { id = project.ProjectId });
            }
            return RedirectToAction("Index", "Projects", new { id = project.ProjectId });
        }


        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var project = await _context.Projects
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.Comments)
                .Include(p => p.UserProjects)
                .Include(p => p.ProjectFiles)
                .Include(p => p.ProjectTechnologies)
                .Include(p => p.ProjectImages)
                .FirstOrDefaultAsync(p => p.ProjectId == id);

            if (project == null)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var file in project.ProjectFiles)
            {
                var filePath = Path.Combine(_env.WebRootPath, file.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            foreach (var image in project.ProjectImages)
            {
                var imagePath = Path.Combine(_env.WebRootPath, image.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Comments.RemoveRange(project.Reviews.SelectMany(r => r.Comments));
            _context.Reviews.RemoveRange(project.Reviews);
            _context.UserProject.RemoveRange(project.UserProjects);
            _context.ProjectFiles.RemoveRange(project.ProjectFiles);
            _context.ProjectTechnology.RemoveRange(project.ProjectTechnologies);
            _context.ProjectImages.RemoveRange(project.ProjectImages);

            _context.Projects.Remove(project);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(string id)
        {
            return _context.Projects.Any(e => e.ProjectId == id);
        }
    }
}