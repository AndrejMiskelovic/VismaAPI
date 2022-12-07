namespace VismaAPI.Models
{
    public class StatisticsModel
    {
        public int TotalEmployeeCount { get; set; }
        public List<RoleStatisticsModel> Statistics { get; set; }
    }

    public class RoleStatisticsModel
    {
        public string Role { get; set; }
        public int EmployeeCountByRole { get; set; }
        public float EmployeeAverageSalaryByRole { get; set; }
    }
}
