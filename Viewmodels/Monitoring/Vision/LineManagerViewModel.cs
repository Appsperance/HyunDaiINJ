using System.Collections.Generic;
using HyunDaiINJ.DATA.DAO;
using HyunDaiINJ.DATA.DTO;

namespace HyunDaiINJ.ViewModels.Monitoring.vision
{
    public class LineManagerViewModel
    {
        public List<EmployeeDTO> Managers { get; }

        public LineManagerViewModel() : this(IsInDesignMode ? new DesignTimeEmployeeDAO() : new EmployeeDAO())
        {
        }

        public LineManagerViewModel(IEmployeeDAO employeeDAO)
        {
            Managers = employeeDAO.GetManagers();
        }

        private static bool IsInDesignMode =>
            System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject());
    }
}


