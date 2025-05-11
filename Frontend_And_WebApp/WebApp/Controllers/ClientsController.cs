using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Entities;
using WebApp.Models;
using WebApp.Filters;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace WebApp.Controllers
{
    [RequireAuth]
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ClientsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet("admin/clients")]
        public async Task<IActionResult> Index()
        {
            var clients = await _context.Clients
                .Select(c => new ClientViewModel
                {
                    Id = c.Id.ToString(),
                    ClientImage = c.ClientImage,
                    ClientName = c.ClientName
                })
                .ToListAsync();

            var clientsViewModel = new ClientsViewModel
            {
                Clients = clients,
                AddClientFormData = new AddClientViewModel()
            };

            return View(clientsViewModel);
        }

        [HttpPost("admin/clients/add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddClientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var clients = await _context.Clients
                    .Select(c => new ClientViewModel
                    {
                        Id = c.Id.ToString(),
                        ClientImage = c.ClientImage,
                        ClientName = c.ClientName
                    })
                    .ToListAsync();

                var clientsViewModel = new ClientsViewModel
                {
                    Clients = clients,
                    AddClientFormData = model
                };

                return View("Index", clientsViewModel);
            }

            string imagePath = "/images/users/user-template-male-green.svg"; // Default image

            if (model.ClientImage != null && model.ClientImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "users");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ClientImage.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ClientImage.CopyToAsync(stream);
                }

                imagePath = "/images/users/" + uniqueFileName;
            }

            var client = new Client
            {
                ClientImage = imagePath,
                ClientName = model.ClientName
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Clients");
        }

        [HttpGet("admin/clients/edit/{id}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            var viewModel = new EditClientViewModel
            {
                Id = client.Id,
                ClientName = client.ClientName
            };

            return View(viewModel);
        }

        [HttpPost("admin/clients/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EditClientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            if (model.ClientImage != null && model.ClientImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "users");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ClientImage.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ClientImage.CopyToAsync(stream);
                }

                client.ClientImage = "/images/users/" + uniqueFileName;
            }

            client.ClientName = model.ClientName;

            _context.Clients.Update(client);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost("admin/clients/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Clients");
        }

        [HttpGet("admin/clients/get/{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            var result = new
            {
                id = client.Id,
                clientImage = client.ClientImage,
                clientName = client.ClientName
            };

            return Json(result);
        }
    }
}