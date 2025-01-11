using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HyunDaiINJ.ViewModels.Plan;

namespace HyunDaiINJ.Views.Plan.Controls.Daily
{
    /// <summary>
    /// DailyPlanControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DailyPlanControl : UserControl
    {
        public DailyPlanControl()
        {
            InitializeComponent();

            this.DataContext = new InjectionPlanViewModel();
        }
    }
}
