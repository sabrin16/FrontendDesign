using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using WebApp.Data; // adjust if your ApplicationDbContext is in a different namespace
using WebApp.Entities;
using WebApp.Filters;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace WebApp.Controllers
{
    [RequireAuth]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(ApplicationDbContext context, IWebHostEnvironment environment, ILogger<ProjectsController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        [Route("admin/projects")]
        public IActionResult Index()
        {
            var viewModel = new ProjectsViewModel()
            {
                Projects = GetProjectsFromDb(),
                AddProjectFormData = new AddProjectViewModel
                {
                    Clients = GetClientsFromDb(),
                    Members = GetMembersFromDb(),
                    Statuses = GetStatusesFromDb()
                },
                EditProjectFormData = new EditProjectViewModel
                {
                    Clients = GetClientsFromDb(),
                    Members = GetMembersFromDb(),
                    Statuses = GetStatusesFromDb(),
                }
            };

            return View(viewModel);
        }

        [HttpPost("admin/projects/add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddProjectViewModel model)
        {
            // Debug: Log received model data
            Console.WriteLine($"Received model data:");
            Console.WriteLine($"ProjectName: {model.ProjectName}");
            Console.WriteLine($"ClientId: {model.ClientId}");
            Console.WriteLine($"Description: {model.Description}");
            Console.WriteLine($"StartDate: {model.StartDate}");
            Console.WriteLine($"EndDate: {model.EndDate}");
            Console.WriteLine($"MemberId: {model.MemberId}");
            Console.WriteLine($"Budget: {model.Budget}");
            Console.WriteLine($"StatusId: {model.StatusId}");

            if (!ModelState.IsValid)
            {
                // Debug: Log validation errors
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation error: {error.ErrorMessage}");
                }

                // Return validation errors as JSON
                return Json(new
                {
                    success = false,
                    errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
                });
            }

            string imagePath = "/images/projects/project-template.svg"; // Default image

            // Handle file upload if an image was provided
            if (model.ProjectImage != null && model.ProjectImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "projects");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ProjectImage.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProjectImage.CopyToAsync(stream);
                }

                imagePath = "/images/projects/" + uniqueFileName;
            }

            var project = new Project
            {
                ProjectName = model.ProjectName,
                ProjectImage = imagePath,
                Description = model.Description,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Budget = model.Budget,
                ClientId = model.ClientId,
                StatusId = model.StatusId // Use the selected status
            };

            _context.Projects.Add(project);

            // Add the selected member to the project
            if (model.MemberId != Guid.Empty)
            {
                var member = await _context.Members.FindAsync(model.MemberId);
                if (member != null)
                {
                    project.Members.Add(member);
                }
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        private IEnumerable<ProjectViewModel> GetProjectsFromDb()
        {
            return _context.Projects
                .Include(p => p.Client)
                .Include(p => p.Members)
                .Include(p => p.Status)
                .Select(p => new ProjectViewModel
                {
                    Id = p.Id.ToString(),
                    ProjectName = p.ProjectName,
                    ClientName = p.Client.ClientName,
                    ProjectImage = p.ProjectImage,
                    Description = p.Description,
                    TimeLeft = p.EndDate <= DateTime.Now
                        ? "0 days left"
                        : ((p.EndDate - DateTime.Now).Days > 30
                            ? "30+ days left"
                            : $"{((p.EndDate - DateTime.Now).Days)} days left"),
                    Members = p.Members.Select(m => m.MemberImage).ToList()
                })
                .ToList();
        }

        private IEnumerable<SelectListItem> GetClientsFromDb()
        {
            return _context.Clients
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.ClientName
                })
                .ToList();
        }

        private IEnumerable<SelectListItem> GetMembersFromDb()
        {
            return _context.Members
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.MemberFirstName + " " + m.MemberLastName
                })
                .ToList();
        }

        private IEnumerable<SelectListItem> GetStatusesFromDb()
        {
            return _context.Statuses
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToList();
        }

        [HttpPost("admin/projects/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("admin/projects/get/{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var project = await _context.Projects
                .Include(p => p.Client)
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            var result = new
            {
                id = project.Id,
                projectName = project.ProjectName,
                clientId = project.ClientId,
                memberId = project.Members.FirstOrDefault()?.Id,
                budget = project.Budget,
                statusId = project.StatusId,
                startDate = project.StartDate.ToString("yyyy-MM-dd"),
                endDate = project.EndDate.ToString("yyyy-MM-dd"),
                description = project.Description,
                projectImage = project.ProjectImage
            };

            return Json(result);
        }

        [HttpPost("admin/projects/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProjectViewModel model)
        {
            try
            {
                // Log received model data
                _logger.LogInformation($"Received edit request for project {model.Id}");
                _logger.LogInformation($"ProjectName: {model.ProjectName}");
                _logger.LogInformation($"ClientId: {model.ClientId}");
                _logger.LogInformation($"Description: {model.Description}");
                _logger.LogInformation($"StartDate: {model.StartDate:yyyy-MM-dd}");
                _logger.LogInformation($"EndDate: {model.EndDate:yyyy-MM-dd}");
                _logger.LogInformation($"MemberId: {model.MemberId}");
                _logger.LogInformation($"Budget: {model.Budget}");
                _logger.LogInformation($"StatusId: {model.StatusId}");

                var project = await _context.Projects
                    .Include(p => p.Members)
                    .FirstOrDefaultAsync(p => p.Id == model.Id);

                if (project == null)
                {
                    _logger.LogWarning($"Project {model.Id} not found");
                    return NotFound();
                }

                // Only update fields that have been changed
                if (!string.IsNullOrWhiteSpace(model.ProjectName))
                {
                    project.ProjectName = model.ProjectName;
                }

                if (model.ClientId != Guid.Empty)
                {
                    project.ClientId = model.ClientId;
                }

                if (!string.IsNullOrWhiteSpace(model.Description))
                {
                    project.Description = model.Description;
                }

                if (model.StartDate != default(DateTime))
                {
                    project.StartDate = model.StartDate;
                }

                if (model.EndDate != default(DateTime))
                {
                    project.EndDate = model.EndDate;
                }

                if (model.Budget > 0)
                {
                    project.Budget = model.Budget;
                }

                if (model.StatusId != Guid.Empty)
                {
                    project.StatusId = model.StatusId;
                }

                // Handle image upload only if a new image was provided
                if (model.ProjectImage != null && model.ProjectImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "projects");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ProjectImage.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProjectImage.CopyToAsync(fileStream);
                    }

                    project.ProjectImage = "/images/projects/" + uniqueFileName;
                    _logger.LogInformation($"Updated project image to {project.ProjectImage}");
                }

                // Update project member only if a new member was selected
                if (model.MemberId != Guid.Empty)
                {
                    var member = await _context.Members.FindAsync(model.MemberId);
                    if (member != null)
                    {
                        // Clear existing members
                        project.Members.Clear();
                        // Add the new member
                        project.Members.Add(member);
                        _logger.LogInformation($"Updated project member to {member.MemberFirstName} {member.MemberLastName}");
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Successfully updated project {model.Id}");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating project {model.Id}");
                return Json(new { success = false, errors = new[] { "An error occurred while updating the project." } });
            }
        }
    }
}
