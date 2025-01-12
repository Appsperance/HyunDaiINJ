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
using HyunDaiINJ.ViewModels.Main;
using HyunDaiINJ.ViewModels.Plan;

namespace HyunDaiINJ.Views.Plan.Pages
{
    /// <summary>
    /// DailyPlan.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DailyPlan : Page
    {
        public DailyPlan()
        {
            InitializeComponent();

            var mainWin = Application.Current.MainWindow as MainView;
            if (mainWin != null)
            {
                var mainVM = mainWin.DataContext as MainViewModel;
                if (mainVM != null)
                {
                    // Page's DC = mainVM.WeekPlanVM
                    this.DataContext = mainVM.DailyPlanVM;
                    Console.WriteLine("WeekPlan Page DC => " + this.DataContext);
                }
            }
        }
    }
}
