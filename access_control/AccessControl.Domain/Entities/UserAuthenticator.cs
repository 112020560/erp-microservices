using System;
using System.Collections.Generic;

namespace AccessControl.Domain.Entities;

public partial class UserAuthenticator
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid AuthenticatorId { get; set; }

    public string Credential { get; set; } = null!;

    public string Identity { get; set; } = null!;

    public virtual Authenticator Authenticator { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
