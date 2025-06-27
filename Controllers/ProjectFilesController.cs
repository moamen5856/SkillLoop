using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkillLoop2.Data;
using SkillLoop2.Models;

namespace SkillLoop2.Controllers
{
    public class ProjectFilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProjectFilesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [Authorize]
        [HttpGet] // Explicit route

        public async Task<IActionResult> DownloadProjectFiles(string projectId)
        {
            // Get all project files from database
            var projectFiles = await _context.ProjectFiles
                .Where(pf => pf.ProjectId == projectId)
                .ToListAsync();

            if (!projectFiles.Any())
            {
                return NotFound("No files found for this project in database.");
            }

            // Create memory stream for ZIP file
            var memoryStream = new MemoryStream();
            int filesAdded = 0;
            var missingFiles = new List<string>();

            using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var file in projectFiles)
                {
                    try
                    {
                        // Normalize file path
                        var relativePath = file.FilePath
                            .Replace("~/", string.Empty)
                            .TrimStart('/')
                            .Replace('/', Path.DirectorySeparatorChar);

                        var fullPath = Path.Combine(_env.WebRootPath, relativePath);

                        // Debug output
                        Console.WriteLine($"Checking file at: {fullPath}");

                        if (System.IO.File.Exists(fullPath))
                        {
                            // Create entry and copy file
                            var entry = zipArchive.CreateEntry(file.FileName);
                            using (var entryStream = entry.Open())
                            using (var fileStream = System.IO.File.OpenRead(fullPath))
                            {
                                await fileStream.CopyToAsync(entryStream);
                                filesAdded++;
                                Console.WriteLine($"Added to ZIP: {file.FileName}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"File not found: {fullPath}");
                            missingFiles.Add(file.FileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing file {file.FileName}: {ex.Message}");
                    }
                }
            }

            // Check if any files were added
            if (filesAdded == 0)
            {
                var errorMessage = "No files could be added to ZIP. Missing files:\n" +
                                 string.Join("\n", missingFiles);
                return NotFound(errorMessage);
            }

            // Prepare response
            memoryStream.Position = 0;
            Response.Headers.Append("X-Files-Added", filesAdded.ToString());

            return File(memoryStream, "application/zip", $"Project_{projectId}_Files.zip");
        }
        // GET: ProjectFiles
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ProjectFiles.Include(p => p.Project);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ProjectFiles/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projectFile = await _context.ProjectFiles
                .Include(p => p.Project)
                .FirstOrDefaultAsync(m => m.ProjectFilesId == id);
            if (projectFile == null)
            {
                return NotFound();
            }

            return View(projectFile);
        }

        // GET: ProjectFiles/Create
        public IActionResult Create()
        {
            ViewData["ProjectId"] = new SelectList(_context.Projects, "ProjectId", "ProjectId");
            return View();
        }

        // POST: ProjectFiles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FileName,FilePath,FileType,FileExtension,FileSize,ProjectId")] ProjectFile projectFile)
        {
            if (ModelState.IsValid)
            {
                var lastFile = _context.ProjectFiles.AsEnumerable().OrderByDescending(f => int.Parse(f.ProjectFilesId)).FirstOrDefault();
                int nextIdNumber = lastFile == null ? 0 : int.Parse(lastFile.ProjectFilesId) + 1; 
                string newId = nextIdNumber.ToString();

                projectFile.ProjectFilesId = newId;

                _context.Add(projectFile);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "ProjectId", "ProjectId", projectFile.ProjectId);
            return View(projectFile);
        }

        // GET: ProjectFiles/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projectFile = await _context.ProjectFiles.FindAsync(id);
            if (projectFile == null)
            {
                return NotFound();
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "ProjectId", "ProjectId", projectFile.ProjectId);
            return View(projectFile);
        }

        // POST: ProjectFiles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("FileName,FilePath,FileType,FileExtension,FileSize,fileDescription,ProjectId")] ProjectFile projectFile)
        {
            if (id != projectFile.ProjectFilesId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(projectFile);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectFileExists(projectFile.ProjectFilesId))
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
            ViewData["ProjectId"] = new SelectList(_context.Projects, "ProjectId", "ProjectId", projectFile.ProjectId);
            return View(projectFile);
        }

        // GET: ProjectFiles/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projectFile = await _context.ProjectFiles
                .Include(p => p.Project)
                .FirstOrDefaultAsync(m => m.ProjectFilesId == id);
            if (projectFile == null)
            {
                return NotFound();
            }

            return View(projectFile);
        }

        // POST: ProjectFiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var projectFile = await _context.ProjectFiles.FindAsync(id);
            if (projectFile != null)
            {
                _context.ProjectFiles.Remove(projectFile);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectFileExists(string id)
        {
            return _context.ProjectFiles.Any(e => e.ProjectFilesId == id);
        }
    }
}
