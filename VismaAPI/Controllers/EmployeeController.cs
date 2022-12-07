using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Reflection;
using VismaAPI.Data;
using VismaAPI.Models;

namespace VismaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeContext _employeeContext;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(EmployeeContext employee, ILogger<EmployeeController> logger)
        {
            _employeeContext = employee;
            _logger = logger;
        }

        [HttpGet(("{Id:Guid}"))]
        public async Task<ActionResult<EmployeeModel>> GetEmployeeByID(Guid Id)
        {
            try
            {
                var employee = await _employeeContext.Employees.FindAsync(Id);
                if (employee == null)
                    return BadRequest("Employee not found.");
                return Ok(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500);
            }
        }
        [HttpGet("NameAndTimePeriod")]
        public async Task<ActionResult<EmployeeModel>> GetEmployeeByNameAndTimePeriod(string firstName, string lastName, DateTime dateTimeFrom, DateTime dateTimeTo)
        {
            try
            {
                var employee = await _employeeContext.Employees.Where(emp => emp.FirstName == firstName && emp.LastName == lastName && (emp.BirthDate >= dateTimeFrom && emp.BirthDate <= dateTimeTo)).ToListAsync();
                if (!employee.Any())
                    return BadRequest("Employee not found.");
                return Ok(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500);
            }
        }
        [HttpGet]
        [Route("GetAllEmployees")]
        public async Task<ActionResult<EmployeeModel>> GetAllEmployees()
        {
            try
            {
                return Ok(await _employeeContext.Employees.ToListAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500);
            }
        }
        [HttpGet("bossid={bossID:Guid}")]
        public async Task<ActionResult<EmployeeModel>> GetEmployeesByBossID([FromRoute] Guid bossID)
        {
            try
            {
                var employee = await _employeeContext.Employees.Where(emp => emp.Boss == bossID).ToListAsync();
                if (employee == null)
                    return BadRequest("Employee not found.");
                return Ok(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500);
            }
        }
        [HttpGet]
        [Route("GetStatistic")]
        public async Task<ActionResult<StatisticsModel>> GetStatistic()
        {
            try
            {
                var response = new StatisticsModel();
                response.Statistics = new List<RoleStatisticsModel>();

                response.TotalEmployeeCount = _employeeContext.Employees.Count();

                if (response.TotalEmployeeCount == 0)
                    return Ok(response);

                foreach (var role in Enum.GetValues<Role>())
                {
                    try
                    {
                        response.Statistics.Add(new RoleStatisticsModel
                        {
                            Role = role.ToString(),
                            EmployeeCountByRole = _employeeContext.Employees.Count(emp => emp.Role == role),
                            EmployeeAverageSalaryByRole = await _employeeContext.Employees
                            .Where(emp => emp.Role == role)
                            .AverageAsync(emp => emp.Salary)
                        });
                    }
                    catch
                    {
                        response.Statistics.Add(new RoleStatisticsModel
                        {
                            Role = role.ToString(),
                            EmployeeCountByRole = 0,
                            EmployeeAverageSalaryByRole = 0 
                        });
                    }

                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500);
            }
        }
        [HttpPost]
        public async Task<ActionResult<EmployeeModel>> AddEmployee(EmployeeModel employee)
        {
            try
            {
                var validationResult = ValidateEmployee(employee);
                if (validationResult != null)
                    return validationResult;

                _employeeContext.Employees.Add(employee);
                await _employeeContext.SaveChangesAsync();

                return Ok(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500);
            }
        }
        private ActionResult ValidateEmployee(EmployeeModel employee)
        {
            var employeeEntry = _employeeContext.Employees.Entry(employee);
            foreach (var property in employeeEntry.Entity.GetType().GetTypeInfo().DeclaredProperties)
            {
                if(employeeEntry.Property(property.Name).CurrentValue.GetType() == typeof(string))
                    if((string)employeeEntry.Property(property.Name).CurrentValue == "")
                        return BadRequest("Not all required data is filled");

                if (employeeEntry.Property(property.Name).CurrentValue == null)
                    return BadRequest("Not all required data is filled");
            }

            string error = ""; //
            if (employee.Role != Role.CEO)
            {
                var dbEntry = _employeeContext.Employees.Where(emp => emp.Id == employee.Boss);
                if (!dbEntry.Any() || employee.Boss == Guid.Empty)
                    error += "There Is No Employee(Boss) with such ID|";
            }
            else
            {
               var dbEntry = _employeeContext.Employees.Where(emp => emp.Role == Role.CEO && emp.Id != employee.Id);
                if (dbEntry.Any())
                    error += "CEO already exist|";
                employee.Boss = Guid.Empty;
            }

            if (employee.FirstName == employee.LastName)
                error += "First Name and Last Name cannot be same|";

            if (employee.FirstName.Length > 50 || employee.LastName.Length > 50 || employee.FirstName.TrimStart().TrimEnd().Length < 1 || employee.LastName.TrimStart().TrimEnd().Length < 1)
                error += "First name and Last name cannot be empty or longer than 50 symbols|";

            if (employee.Salary < 0)
                error += "Salary cannot be negative|";

            int age = GetAge(employee.BirthDate);
            if (age < 18 || age > 70)
                error += "Employee age must be between 18 and 70 years|";

            if (employee.EmploymentDate > DateTime.Today)
                error += "Employment date cannot be future date|";

            if (employee.EmploymentDate < employee.BirthDate)
                error += "Employee started working before was born. Yeah, that's the spirit!|";

            if (employee.EmploymentDate < new DateTime(2000, 1, 1))
                error += "Employment date cannot be earlier than 2000-01-01|";
            
            if(error == "")
                return null;

            return BadRequest(error);
        }
        private int GetAge(DateTime bornDate)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - bornDate.Year;
            if (bornDate > today.AddYears(-age))
                age--;

            return age;
        }
        [HttpPut]
        [Route("UpdateEmployee/{Id}")]
        public async Task<ActionResult<EmployeeModel>> UpdateEmployee([FromRoute] Guid Id, [FromBody] EmployeeModel employee)
        {
            try
            {
                var dbEmployee = await _employeeContext.Employees.FindAsync(Id);
                if (dbEmployee == null)
                    return NotFound("Employee not found.");

                var dbEmployeeEntry = _employeeContext.Employees.Update(dbEmployee);
                var employeeEntry = _employeeContext.Employees.Entry(employee);
                foreach (var property in dbEmployeeEntry.Entity.GetType().GetTypeInfo().DeclaredProperties)
                {

                    var currentValue = dbEmployeeEntry.Property(property.Name).CurrentValue;
                    var currentValue2 = employeeEntry.Property(property.Name).CurrentValue;

                    if (property.Name == "Id" || currentValue2 == null)
                        continue;

                    if (currentValue2.GetType() == typeof(DateTime))
                        if ((DateTime)currentValue2 == DateTime.MinValue)
                            continue;

                    dbEmployeeEntry.Property(property.Name).IsModified = false;
                    if (currentValue != currentValue2 && currentValue2 != null)
                    {
                        dbEmployeeEntry.Property(property.Name).CurrentValue = employeeEntry.Property(property.Name).CurrentValue;
                        dbEmployeeEntry.Property(property.Name).IsModified = true;
                    }
                }

                var validationResult = ValidateEmployee(employee);
                if (validationResult != null)
                    return validationResult;

                await _employeeContext.SaveChangesAsync();
                return Ok(dbEmployee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500);
            }
        }
        [HttpPut]
        [Route("UpdateSalary")]
        public async Task<ActionResult<EmployeeModel>> UpdateSalary(Guid id, float salary)
        {
            try
            {
                var employee = await _employeeContext.Employees.FindAsync(id);
                if (employee == null)
                    return BadRequest("Employee not found.");

                if (employee.Salary < 0)
                    return BadRequest("Salary cannot be negative");

                employee.Salary = salary;
                await _employeeContext.SaveChangesAsync();

                return Ok(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500);
            }
        }
        [HttpDelete("{Id}")]
        public async Task<ActionResult> DeleteEmployee([FromRoute] Guid Id)
        {
            try
            {
                var employee = await _employeeContext.Employees.FindAsync(Id);
                if (employee == null)
                    return BadRequest("Employee not found.");

                _employeeContext.Employees.Remove(employee);
                await _employeeContext.SaveChangesAsync();

                return Ok("Employee is deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500);
            }
        }
    }
}
