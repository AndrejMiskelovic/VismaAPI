using Microsoft.EntityFrameworkCore;
using VismaAPI.Models;

namespace VismaAPI.Data
{
    public class EmployeeContext : DbContext
    {
        public EmployeeContext(DbContextOptions<EmployeeContext> options) : base(options) { }

        public DbSet<EmployeeModel> Employees { get; set; }

        public static void AddCustomerData(WebApplication app)
        {
            var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetService<EmployeeContext>();

            var employee1 = new EmployeeModel
            {
                FirstName = "FirstName1",
                LastName = "LastName",
                 BirthDate = DateTime.Parse("2000-01-01"),
                 EmploymentDate = DateTime.Today,
                 Address = "test",
                 Salary = 10000,
                 Role = Role.CEO
            };
            db.Employees.Add(employee1);
            var employee2 = new EmployeeModel
            {
                FirstName = "FirstName2",
                LastName = "LastName",
                BirthDate = DateTime.Parse("2000-01-01"),
                EmploymentDate = DateTime.Today,
                Boss = employee1.Id,
                Address = "test",
                Salary = 1234,
                Role = Role.Employee
            };
            var employee3 = new EmployeeModel
            {
                FirstName = "FirstName3",
                LastName = "LastName",
                BirthDate = DateTime.Parse("2000-01-01"),
                EmploymentDate = DateTime.Today,
                Boss = employee1.Id,
                Address = "test",
                Salary = 4321,
                Role = Role.Employee
            };


            db.Employees.Add(employee2);
            db.Employees.Add(employee3);

            db.SaveChanges();
        }

    }
}
