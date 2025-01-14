using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyunDaiINJ.DATA.DAO;
using HyunDaiINJ.DATA.DTO;

namespace HyunDaiINJ.Models.Monitoring.Vision
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
