using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using HyunDaiINJ.ViewModels.Monitoring.Process;

namespace HyunDaiINJ.Views.Monitoring.Controls.Process
{
    public partial class ProcessProducer : UserControl
    {
        public ProcessProducer()
        {
            InitializeComponent();

            // 부모의 DataContext를 사용하도록 설정
            DataContextChanged += (s, e) =>
            {
                if (e.NewValue is ProcessProcedureViewModel viewModel)
                {
                    DataContext = viewModel;
                }
            };
        }
    }

    }
