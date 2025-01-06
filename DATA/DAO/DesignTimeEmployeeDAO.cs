using System.Collections.Generic;
using HyunDaiINJ.DATA.DTO;

namespace HyunDaiINJ.DATA.DAO
{
    public class DesignTimeEmployeeDAO : IEmployeeDAO
    {
        public List<EmployeeDTO> GetManagers()
        {
            return new List<EmployeeDTO>
            {
                new EmployeeDTO { Name = "John Doe", Department = "Quality" },
                new EmployeeDTO { Name = "Jane Smith", Department = "Production" }
            };
        }

        public List<EmployeeDTO> GetEmployeesByDepartment(string department)
        {
            return new List<EmployeeDTO>
            {
                new EmployeeDTO { Name = "John Doe", Department = department },
                new EmployeeDTO { Name = "Jane Smith", Department = department }
            };
        }
    }
}
