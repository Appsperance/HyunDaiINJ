using System.Windows.Controls;
using HyunDaiINJ.DATA.DAO;
using HyunDaiINJ.ViewModels.Monitoring.FirstSection;

namespace HyunDaiINJ.Views.Monitoring.Controls.ThirdSection.Vision
{
    public partial class LineManager : UserControl
    {
        public LineManager()
        {
            InitializeComponent();

            // 디자인 타임 확인
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                // 디자인 타임 데이터로 ViewModel 초기화
                DataContext = new LineManagerViewModel(new DesignTimeEmployeeDAO());
            }
            else
            {
                // 런타임 데이터로 ViewModel 초기화
                var employeeDAO = new EmployeeDAO();
                DataContext = new LineManagerViewModel(employeeDAO);
            }
        }
    }
}
