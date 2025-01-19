using System;
using System.Windows;
using System.Windows.Controls;
using HyunDaiINJ.ViewModels.Monitoring.Process;

namespace HyunDaiINJ.Views.Monitoring.Controls.Process
{
    public partial class ProcessProducer : UserControl
    {
        // XAML에서 MqttTopic="Process/PLC/1" 처럼 지정하면 여기로 들어옴
        public static readonly DependencyProperty MqttTopicProperty =
            DependencyProperty.Register(
                nameof(MqttTopic),
                typeof(string),
                typeof(ProcessProducer),
                new PropertyMetadata(null, OnMqttTopicChanged));

        public string? MqttTopic
        {
            get => (string?)GetValue(MqttTopicProperty);
            set => SetValue(MqttTopicProperty, value);
        }

        private ProcessProcedureViewModel? _viewModel;

        public ProcessProducer()
        {
            InitializeComponent();
            // 혹시 부모 DataContext 상속하는 코드를 쓰지 않도록 주석 처리
            //DataContextChanged += (s, e) =>
            //{
            //    if (e.NewValue is ProcessProcedureViewModel vm)
            //    {
            //        _viewModel = vm;
            //        DataContext = _viewModel;
            //    }
            //};
        }

        private static void OnMqttTopicChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProcessProducer producer && e.NewValue is string topic)
            {
                // 만약 이미 ViewModel이 없으면 (UserControl이 새로 생성된 시점)
                if (producer._viewModel == null)
                {
                    // 독립적인 MQTTModel 인스턴스 생성
                    var mqttModel = new MQTTModel();
                    // 해당 토픽 전용의 ProcessProcedureViewModel 생성
                    producer._viewModel = new ProcessProcedureViewModel(mqttModel, topic);
                }

                // UserControl의 DataContext를 이 뷰모델로 설정
                producer.DataContext = producer._viewModel;
            }
        }
    }
}
