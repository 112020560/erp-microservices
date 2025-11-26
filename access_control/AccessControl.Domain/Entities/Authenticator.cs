using System;
using System.Collections.Generic;

namespace AccessControl.Domain.Entities;

public partial class Authenticator
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<UserAuthenticator> UserAuthenticators { get; set; } = new List<UserAuthenticator>();
}
