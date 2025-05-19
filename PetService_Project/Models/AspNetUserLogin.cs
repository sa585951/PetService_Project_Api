using System;
using System.Collections.Generic;

namespace PetService_Project_Api.Models;

public partial class AspNetUserLogin
{
    public string UserId { get; set; } = null!;

    public string? LoginProvider { get; set; }

    public string? ProviderKey { get; set; }

    public string? ProviderDisplayName { get; set; }

    public virtual AspNetUser User { get; set; } = null!;
}
