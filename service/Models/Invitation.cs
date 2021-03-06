using System;

namespace Soli.Models
{
    public class Invitation
    {
        public Guid Id { get; set; }
        public string InvitedBy { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Roles { get; set; }
    }
}
