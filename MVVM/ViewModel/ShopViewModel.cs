using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TempoWithGUI.Core;

namespace TempoWithGUI.MVVM.ViewModel
{
    class ShopViewModel : ObservableObject
    {
        public RelayCommand ShopViewCommand { get; set; }
        //Window change commands


        private object _currentView;

        public object  CurrentView
        {
            get { return  _currentView; }
            set { 
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public ShopViewModel()
        {
            var AccountsVm = new AccountsModel();
            CurrentView = AccountsVm;

            ShopViewCommand = new RelayCommand(o =>
            {
                CurrentView = new AccountsModel();
            });
        }
    }
}
