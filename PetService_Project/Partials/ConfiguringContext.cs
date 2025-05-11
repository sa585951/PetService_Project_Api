using Microsoft.EntityFrameworkCore;

namespace PetService_Project.Partials
{
    public partial class ConfiguringContext : DbContext
    {

        public ConfiguringContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            if (!optionsBuilder.IsConfigured) 
            {
                IConfiguration Config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();
                optionsBuilder.UseSqlServer(Config.GetConnectionString("DefaultConnection"));
            }
        }
    }
}
