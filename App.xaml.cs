using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string? ConnectionString { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ConnectionString = "Host=localhost;Port=5432;Username=root;Password=vaporcloud;Database=msd_db";
        }

        static App()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
            {
                // 디자인 타임 기본값 설정
                ConnectionString = "DesignTimeConnectionString";
            }
        }
    }
}
