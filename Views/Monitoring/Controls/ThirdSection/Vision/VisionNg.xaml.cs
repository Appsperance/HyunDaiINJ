using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using HyunDaiINJ.ViewModels.Monitoring.ThirdSection;

namespace HyunDaiINJ.Views.Monitoring.Controls.ThirdSection.Vision
{
    public partial class VisionNg : UserControl
    {
        private readonly VisionNgViewModel _viewModel;

        public VisionNg()
        {
            InitializeComponent();
            _viewModel = new VisionNgViewModel();

            // WebView2 초기화
        }
    }
}
