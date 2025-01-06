using System.Collections.Generic;
using HyunDaiINJ.DATA.DTO;

namespace HyunDaiINJ.DATA.DAO
{
    public interface IEmployeeDAO
    {
        List<EmployeeDTO> GetManagers();
        List<EmployeeDTO> GetEmployeesByDepartment(string department);
    }
}
