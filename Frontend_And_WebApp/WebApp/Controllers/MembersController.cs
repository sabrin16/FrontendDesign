using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Entities;
using WebApp.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using WebApp.Filters;

namespace WebApp.Controllers
{
    [RequireAuth]
    public class MembersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        // Constructor to inject ApplicationDbContext and IWebHostEnvironment
        public MembersController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet("admin/members")]
        public async Task<IActionResult> Index()
        {
            // Fetch members from the database and map to MemberViewModel
            var members = await _context.Members
                .Select(m => new MemberViewModel
                {
                    Id = m.Id.ToString(),
                    MemberImage = m.MemberImage,
                    MemberFirstName = m.MemberFirstName,
                    MemberLastName = m.MemberLastName,
                    MemberEmail = m.MemberEmail,
                    MemberPhone = m.MemberPhone,
                    MemberJobTitle = m.MemberJobTitle,
                    MemberAddress = m.MemberAddress,
                    MemberBirthDate = m.MemberBirthDate.ToString("yyyy-MM-dd")
                })
                .ToListAsync();

            var membersViewModel = new MembersViewModel
            {
                Members = members,
                AddMemberFormData = new AddMemberViewModel() // Initialize the form data
            };

            return View(membersViewModel);
        }

        [HttpGet("admin/members/add")]
        public IActionResult Add()
        {
            var viewModel = new AddMemberViewModel();
            return View(viewModel);
        }

        [HttpPost("admin/members/add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddMemberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // If validation fails, get the current members and return to Index view
                var members = await _context.Members
                    .Select(m => new MemberViewModel
                    {
                        Id = m.Id.ToString(),
                        MemberImage = m.MemberImage,
                        MemberFirstName = m.MemberFirstName,
                        MemberLastName = m.MemberLastName,
                        MemberEmail = m.MemberEmail,
                        MemberPhone = m.MemberPhone,
                        MemberJobTitle = m.MemberJobTitle,
                        MemberAddress = m.MemberAddress,
                        MemberBirthDate = m.MemberBirthDate.ToString("yyyy-MM-dd")
                    })
                    .ToListAsync();

                var membersViewModel = new MembersViewModel
                {
                    Members = members,
                    AddMemberFormData = model // Pass the invalid model back to the view
                };

                return View("Index", membersViewModel);
            }

            string imagePath = "/images/users/user-template-male-green.svg"; // Default image

            // Handle file upload if an image was provided
            if (model.MemberImage != null && model.MemberImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "users");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.MemberImage.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.MemberImage.CopyToAsync(stream);
                }

                imagePath = "/images/users/" + uniqueFileName;
            }


            var member = new Member
            {
                MemberImage = imagePath,
                MemberFirstName = model.MemberFirstName,
                MemberLastName = model.MemberLastName,
                MemberEmail = model.MemberEmail,
                MemberPhone = model.MemberPhone,
                MemberJobTitle = model.MemberJobTitle,
                MemberAddress = model.MemberAddress,
                MemberBirthDate = model.MemberBirthDate ?? DateOnly.MinValue
            };

            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Members");
        }

        [HttpGet("admin/members/edit/{id}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            // Fetch the member by ID from the database
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            var viewModel = new EditMemberViewModel
            {
                MemberFirstName = member.MemberFirstName,
                MemberLastName = member.MemberLastName,
                MemberEmail = member.MemberEmail,
                MemberPhone = member.MemberPhone,
                MemberJobTitle = member.MemberJobTitle,
                MemberAddress = member.MemberAddress,
                // No need for conversion, just assign the DateOnly
                MemberBirthDate = member.MemberBirthDate
            };

            return View(viewModel);
        }

        [HttpPost("admin/members/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EditMemberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            // Handle file upload if a new image was provided
            if (model.MemberImage != null && model.MemberImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "users");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.MemberImage.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.MemberImage.CopyToAsync(stream);
                }

                member.MemberImage = "/images/users/" + uniqueFileName;
            }

            // Update member fields
            member.MemberFirstName = model.MemberFirstName;
            member.MemberLastName = model.MemberLastName;
            member.MemberEmail = model.MemberEmail;
            member.MemberPhone = model.MemberPhone;
            member.MemberJobTitle = model.MemberJobTitle;
            member.MemberAddress = model.MemberAddress;
            member.MemberBirthDate = model.MemberBirthDate ?? DateOnly.MinValue;

            _context.Members.Update(member);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost("admin/members/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Members");
        }

        [HttpGet("admin/members/get/{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            var result = new
            {
                id = member.Id,
                memberImage = member.MemberImage,
                memberFirstName = member.MemberFirstName,
                memberLastName = member.MemberLastName,
                memberEmail = member.MemberEmail,
                memberPhone = member.MemberPhone,
                memberJobTitle = member.MemberJobTitle,
                memberAddress = member.MemberAddress,
                memberBirthDate = member.MemberBirthDate.ToString("yyyy-MM-dd")
            };

            return Json(result);
        }
    }
}