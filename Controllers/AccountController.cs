using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SkillLoop2.Data;
using SkillLoop2.Models;
using SkillLoop2.ViewModels;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SkillLoop2.Controllers
{

    public class AccountController : Controller
    {
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment env;


        [ActivatorUtilitiesConstructor]
        public AccountController(SignInManager<User> signInManager, UserManager<User> userManager,ApplicationDbContext context, IWebHostEnvironment env)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.context = context;
            this.env = env;
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Email or Password is incorrect.");
                    return View(model);
                }

            }
            return View(model);
        }

        public async Task<IActionResult> Profile()
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();


            var user = await context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(string id, [FromForm] User user, IFormFile imageFile)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            var existingUser = await userManager.FindByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            if (user.IsPhotoDeleted == true)
            {
                user.image = "/images/profile.jpg";
            }

            // Update properties
            existingUser.Email = user.Email;
            existingUser.UserName = user.Email;
            existingUser.Age = user.Age;
            existingUser.Gander = user.Gander;
            existingUser.FullName = user.FullName;
            existingUser.image = user.image;

            // Handle image upload
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(env.WebRootPath, "images", "profile");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                existingUser.image = "~/images/profile/" + uniqueFileName;
            }

            var result = await userManager.UpdateAsync(existingUser);
            if (result.Succeeded)
            {
                return RedirectToAction("Profile", new { id = user.Id });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(user);
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = new User
                {
                    FullName = model.Name,
                    Email = model.Email,
                    UserName = model.Email,

                };
                var result = await userManager.CreateAsync(user,model.Password);

                if (result.Succeeded) { 
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    foreach (var error in result.Errors) {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View(model);
                }

            }
            return View(model);

        }

        public IActionResult VerifyEmail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(model.Email);

                if (user == null)
                {
                    ModelState.AddModelError("", "Something is wrong!");
                    return View(model);
                }
                else
                {
                    return RedirectToAction("ChangePassword","Account",new {username = user.UserName});
                }
            }
            return View(model);
        }
        public IActionResult ChangePassword(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("VerifyEmail", "Account");
            }
            return View(new ChangePasswordViewModel { Email = username });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(model.Email);
                if(user != null)
                {
                    var result = await userManager.RemovePasswordAsync(user);
                    if (result.Succeeded)
                    {
                        result = await userManager.AddPasswordAsync(user, model.NewPassword);
                        await signInManager.SignOutAsync();
                        return RedirectToAction("Login", "Account");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }

                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Email not found!");
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("", "Something went wrong, try again.");
                return View(model);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete()
        {
            // Get current user
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // First sign out the user
            await signInManager.SignOutAsync();


            // 1. Delete user's comments
            var userComments = context.Comments
                .Where(c => c.Review.UserId == userId);
            context.Comments.RemoveRange(userComments);

            // 2. Delete user's reviews
            var userReviews = context.Reviews
                .Where(r => r.UserId == userId)
                .Include(r => r.Comments);
            context.Reviews.RemoveRange(userReviews);

            // 3. Delete user's project associations
            var userProjects = context.UserProject
                .Where(up => up.UserId == userId);
            context.UserProject.RemoveRange(userProjects);

            // 4. If the user owns any projects, handle those
            var ownedProjects = context.Projects
                .Where(p => p.UserProjects.Any(up => up.UserId == userId))
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.Comments)
                .Include(p => p.UserProjects)
                .Include(p => p.ProjectFiles)
                .Include(p => p.ProjectTechnologies)
                .Include(p => p.ProjectImages);


            foreach (var project in ownedProjects)
            {
                foreach (var file in project.ProjectFiles)
                {
                    var filePath = Path.Combine(env.WebRootPath, file.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                // Delete project files
                context.ProjectFiles.RemoveRange(project.ProjectFiles);

                // Delete project technologies
                context.ProjectTechnology.RemoveRange(project.ProjectTechnologies);

                foreach (var image in project.ProjectImages)
                {
                    var imagePath = Path.Combine(env.WebRootPath, image.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
                // Delete project images
                context.ProjectImages.RemoveRange(project.ProjectImages);

                // Delete project reviews and comments
                foreach (var review in project.Reviews)
                {
                    context.Comments.RemoveRange(review.Comments);
                }
                context.Reviews.RemoveRange(project.Reviews);

                // Delete user-project associations
                context.UserProject.RemoveRange(project.UserProjects);

                // Finally delete the project itself
                context.Projects.Remove(project);
            }

            // Save all changes before deleting the user
            await context.SaveChangesAsync();

            // Now delete the user
            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                // If deletion failed, add errors and redirect back to profile
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return RedirectToAction("Profile");
            }

            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("index", "Home");
        }
    }
}
