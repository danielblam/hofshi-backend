namespace VacationManager.Services
{
    public class DbService
    {
        public string connectionString;

        public DbService(IConfiguration config)
        {
            connectionString = config.GetConnectionString("DefaultConnection");
        }
    }
}
