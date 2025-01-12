//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.ComponentModel;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using HyunDaiINJ.DATA.DTO;
//using HyunDaiINJ.Models.Plan;
//using HyunDaiINJ.Services;

//namespace HyunDaiINJ.ViewModels.Plan
//{
//    public class InjectionPlanViewModel : INotifyPropertyChanged
//    {
//        // 주간 계획용 VM
//        public WeekPlanViewModel WeekPlanVM { get; }
        
//        // 일일 계획용 VM (원하신다면)
//        //public DailyPlanViewModel DailyPlanVM { get; }

//        public InjectionPlanViewModel()
//        {
//            // 하나의 인스턴스를 생성하여 공유
//            WeekPlanVM = new WeekPlanViewModel();
//            Console.WriteLine($"InjecitonVM : {WeekPlanVM}");
//            //DailyPlanVM = new DailyPlanViewModel();
//        }

//        public event PropertyChangedEventHandler PropertyChanged;
//        protected virtual void OnPropertyChanged(string propName)
//        {
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
//        }
//    }
//}
