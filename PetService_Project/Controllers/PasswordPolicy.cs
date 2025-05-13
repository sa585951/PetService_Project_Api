namespace PetService_Project_Api.Controllers
{
    internal class PasswordPolicy
    {
        public int MinLength { get; set; }
        public bool RequiresDigit { get; set; }
        public bool RequiresLowercase { get; set; }
        public bool RequiresUppercase { get; set; }
        public bool RequiresNonAlphanumeric { get; set; }
    }
}