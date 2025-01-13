using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using HyunDaiINJ.ViewModels.Monitoring.Process;

namespace HyunDaiINJ.Views.Monitoring.Controls.Process
{
    public partial class ProcessProducer : UserControl
    {
        // 1) 부모(예: Page)에서 "Y41"/"Y42"/"Y43" 값을 바인딩 받을 DependencyProperty
        public static readonly DependencyProperty StageValProperty =
            DependencyProperty.Register(
                nameof(StageVal),
                typeof(string),
                typeof(ProcessProducer),
                new PropertyMetadata(null, OnStageValChanged));

        /// <summary>
        /// 부모 XAML에서:
        ///   <process:ProcessProducer StageVal="{Binding Y41}" />
        /// 처럼 바인딩하면, 이 프로퍼티가 값(예: "Y41")을 받음.
        /// </summary>
        public string StageVal
        {
            get => (string)GetValue(StageValProperty);
            set => SetValue(StageValProperty, value);
        }

        // 2) 내부에서 사용할 ViewModel 인스턴스
        private readonly ProcessProcedureViewModel _viewModel;

        // 3) 외부 XAML 바인딩용으로 MyVM 프로퍼티를 노출
        //    (XAML에서 "{Binding MyVM.CurrentImage}" 등으로 접근 가능)
        public ProcessProcedureViewModel MyVM => _viewModel;

        public ProcessProducer()
        {
            InitializeComponent();

            // ViewModel 생성
            _viewModel = new ProcessProcedureViewModel();

            // UserControl의 DataContext = 자기자신(this)
            //   => XAML 바인딩에서 {Binding MyVM.XXX} 가능
            this.DataContext = this;

            Debug.WriteLine($"[ProcessProducer] UserControl created.");
            Debug.WriteLine($"[ProcessProducer] MyVM = {_viewModel}");
        }

        // 4) StageVal이 바뀔 때마다 자동 호출
        private static void OnStageValChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProcessProducer control)
            {
                string newVal = e.NewValue as string ?? "";
                string oldVal = e.OldValue as string ?? "";

                Debug.WriteLine($"[ProcessProducer] OnStageValChanged: oldVal={oldVal}, newVal={newVal}");

                // UserControl의 ViewModel에 StageVal 값 대입
                control._viewModel.StageVal = newVal;

                Debug.WriteLine($"[ProcessProducer] _viewModel.StageVal = {control._viewModel.StageVal}");
            }
        }
    }
}
