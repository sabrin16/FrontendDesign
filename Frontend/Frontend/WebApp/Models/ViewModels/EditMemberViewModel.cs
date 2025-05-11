using System;
using Microsoft.AspNetCore.Http;

namespace WebApp.Models
{
    public class EditMemberViewModel
    {
        public Guid Id { get; set; }  // Add an Id property to identify the member

        public IFormFile? MemberImage { get; set; }

        public string MemberFirstName { get; set; } = string.Empty;

        public string MemberLastName { get; set; } = string.Empty;

        public string MemberEmail { get; set; } = string.Empty;

        public string MemberPhone { get; set; } = string.Empty;

        public string MemberJobTitle { get; set; } = string.Empty;

        public string MemberAddress { get; set; } = string.Empty;

        public DateOnly? MemberBirthDate { get; set; }  // Optional if BirthDate is nullable

        // Any other fields you need can be added here.
    }
}
