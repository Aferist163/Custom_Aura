using System;
using Custom_Aura.Core;

namespace Custom_Aura.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {
        public RelayComand All_ViewComand { get; set; }
        public RelayComand Star_ViewComand { get; set; }
        public RelayComand Rain_ViewComand { get; set; }


        public All_Color_ViewModel All_C { get; set; }
        public Rain_Effect_ViewModel Rain_E { get; set; }

        public Star_Effect_ViewModel Star_E { get; set; }

        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set { 
                _currentView = value;
                OnPropertyChanged();
            }
        }


        public MainViewModel()
        {
            All_C = new All_Color_ViewModel();
            Star_E = new Star_Effect_ViewModel();
            Rain_E = new Rain_Effect_ViewModel();
            CurrentView = All_C;

            Star_ViewComand = new RelayComand(o => 
            { 
                CurrentView = Star_E;
            });

            All_ViewComand = new RelayComand(o =>
            {
                CurrentView = All_C;
            });

            Rain_ViewComand = new RelayComand(o =>
            {
                CurrentView = Rain_E;
            });
        }
    }
}
