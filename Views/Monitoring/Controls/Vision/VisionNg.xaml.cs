using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using HyunDaiINJ.ViewModels.Monitoring.ThirdSection;

namespace HyunDaiINJ.Views.Monitoring.Controls.Vision
{
    public partial class VisionNg : UserControl
    {
        private readonly VisionNgViewModel viewModel;

        public VisionNg()
        {
            InitializeComponent();
            viewModel = new VisionNgViewModel();

            // WebView2 초기화
        }
    }
}
