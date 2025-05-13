using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace PetService_Project_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PasswordPolicyController : ControllerBase
    {

        private readonly IOptions<IdentityOptions> _identityOptions;

        public PasswordPolicyController(IOptions<IdentityOptions> identityOptions)
        {
            _identityOptions = identityOptions;
        }

        [HttpGet("GetPasswordPolicy")]
        public IActionResult GetPasswordPolicy()
        {
            var passwordOptions = _identityOptions.Value.Password;

            var policy = new PasswordPolicy
            {
                MinLength = passwordOptions.RequiredLength,
                RequiresDigit = passwordOptions.RequireDigit,
                RequiresLowercase = passwordOptions.RequireLowercase,
                RequiresUppercase = passwordOptions.RequireUppercase,
                RequiresNonAlphanumeric = passwordOptions.RequireNonAlphanumeric
            };

            return Ok(policy);
        }
    }

    internal class PasswordPolicy
    {
        public int MinLength { get; set; }
        public bool RequiresDigit { get; set; }
        public bool RequiresLowercase { get; set; }
        public bool RequiresUppercase { get; set; }
        public bool RequiresNonAlphanumeric { get; set; }
    }
}
