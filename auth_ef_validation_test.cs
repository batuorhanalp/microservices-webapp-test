using Microsoft.EntityFrameworkCore;
using WebApp.AuthService.Data;
using WebApp.Common.Entities;

namespace ValidationTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Testing AuthDbContext EF Core configuration...");
            
            try
            {
                // Create in-memory database for testing
                var options = new DbContextOptionsBuilder<AuthDbContext>()
                    .UseInMemoryDatabase(databaseName: "TestDb")
                    .Options;

                using var context = new AuthDbContext(options);

                // Test entity validation by attempting to create a model
                var model = context.Model;
                
                Console.WriteLine($"✅ EF Core model validation passed!");
                Console.WriteLine($"Found {model.GetEntityTypes().Count()} entity types:");
                
                foreach (var entityType in model.GetEntityTypes())
                {
                    Console.WriteLine($"  - {entityType.ClrType.Name}");
                }

                Console.WriteLine("All tests passed - Auth service EF Core configuration is working correctly!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ EF Core validation failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Environment.Exit(1);
            }
        }
    }
}
