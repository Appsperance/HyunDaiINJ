using System.Collections.Generic;
using HyunDaiINJ.DATA.DAO;
using HyunDaiINJ.DATA.DTO;

namespace HyunDaiINJ.Models.Monitoring.FirstSection
{
    public class LineManagerModel
    {
        private readonly IEmployeeDAO _employeeDAO;

        public LineManagerModel(IEmployeeDAO employeeDAO)
        {
            _employeeDAO = employeeDAO;
        }

        public List<EmployeeDTO> GetManagers()
        {
            return _employeeDAO.GetManagers();
        }
    }
}
