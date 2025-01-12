using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using HyunDaiINJ.ViewModels.Plan;
using HyunDaiINJ.Views.Plan.Pages;

namespace HyunDaiINJ.Views.Plan.Controls.Week
{
    public partial class WeekPlanControl : UserControl
    {

        public WeekPlanControl()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                Console.WriteLine($"WeekPlanControl Loaded. DC: {DataContext?.GetType().Name}");
            };
        }
    }
}
