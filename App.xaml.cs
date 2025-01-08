using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using LiveCharts.Configurations;
using Dapper;

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

            // Dapper의 snake_case 자동 매핑 설정
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

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
