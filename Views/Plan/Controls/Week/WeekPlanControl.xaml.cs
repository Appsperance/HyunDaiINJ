using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using HyunDaiINJ.Views.Plan.Pages;

namespace HyunDaiINJ.Views.Plan.Controls.Week
{
    public partial class WeekPlanControl : UserControl
    {

        public WeekPlanControl()
        {
            InitializeComponent();

            // 디버깅용 출력
            Console.WriteLine(DataContext?.GetType().Name);

            // DataContext 설정
            DataContext = new WeekPlanViewModel();

            // DataContext 확인
            Console.WriteLine($"DataContext: {DataContext?.GetType().Name}");
        }
    }
}
