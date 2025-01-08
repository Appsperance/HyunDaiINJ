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
using HyunDaiINJ.ViewModels.MQTT;

namespace HyunDaiINJ.Views.Monitoring.Controls.FirstSection.Vision
{ 
    /// <summary>
    /// VisionProcedure.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class VisionProcedure : UserControl
    {
        public VisionProcedure()
        {
            InitializeComponent();
            DataContext = new MqttViewModel(); // ViewModel 설정
        }
    }
}
