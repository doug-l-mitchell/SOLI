using System;
using System.Collections.Generic;

namespace Soli.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public IEnumerable<int> Roles { get; set; }
    }
}
