using System;
using System.Windows;
using System.Windows.Controls;
using HyunDaiINJ.ViewModels.Main;
using HyunDaiINJ.Views.Plan.Controls.Week;

namespace HyunDaiINJ.Views.Plan.Pages
{
    public partial class WeekPlan : Page
    {
        //public WeekPlan()
        //{
        //    InitializeComponent();

        //    // 1) 부모 MainView
        //    var mainWin = Application.Current.MainWindow as MainView;
        //    if (mainWin != null)
        //    {
        //        // 2) 그 DataContext = MainViewModel
        //        var mainVM = mainWin.DataContext as MainViewModel;
        //        if (mainVM != null)
        //        {
        //            // 3) 이 Page의 DataContext = mainVM.WeekPlanVM
        //            this.DataContext = mainVM.WeekPlanVM;
        //            Console.WriteLine("WeekPlan DataContext set to: " + this.DataContext);
        //        }
        //    }


        //    // 디버깅용 출력
        //    Console.WriteLine(DataContext?.GetType().Name);

        //    // DataContext 확인
        //    Console.WriteLine($"DataContext: {DataContext?.GetType().Name}");
        //}
        //protected override void OnInitialized(EventArgs e)
        //{
        //    base.OnInitialized(e);
        //    Console.WriteLine("WeekPlan Page DataContext? " + (DataContext?.GetType().Name));
        //}

        public WeekPlan()
        {
            InitializeComponent();

            var mainWin = Application.Current.MainWindow as MainView;
            if (mainWin != null)
            {
                var mainVM = mainWin.DataContext as MainViewModel;
                if (mainVM != null)
                {
                    // Page's DC = mainVM.WeekPlanVM
                    this.DataContext = mainVM.WeekPlanVM;
                    Console.WriteLine("WeekPlan Page DC => " + this.DataContext);
                }
            }
        }

    }
}
