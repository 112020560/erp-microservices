using System;
using System.Collections.Generic;

namespace AccessControl.Domain.Entities;

public partial class User
{
    public Guid Id { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool? IsEmailVerified { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<UserAuthenticator> UserAuthenticators { get; set; } = new List<UserAuthenticator>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
