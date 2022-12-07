using System.ComponentModel.DataAnnotations.Schema;

namespace VismaAPI.Models
{
    public class EmployeeModel
    {
        private DateTime _birthDate;
        private DateTime _employmentDate;
        
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; private set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime BirthDate { get => _birthDate; set => _birthDate = value.Date; } //will set time to 00:00:00  
        public DateTime EmploymentDate { get => _employmentDate; set => _employmentDate = value.Date; }
        public Guid Boss { get; set; }
        public string? Address { get; set; }
        public float Salary { get; set; }
        public Role Role { get; set; }

    }
    //Enum will prevent additional case sensitive checking (e.g. ceo, Ceo, CeO etc.), request will accept both string and int
    public enum Role
    {
        Employee,
        Manager,
        CEO,
    }
}
