using AutoFocusMachine.Model.Recipe;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using YuanliCore.AffineTransform;
using YuanliCore.Interface;

namespace AutoFocusMachine.ViewModel
{
    public partial class MainViewModel
    {

        public ICommand TestCommand => new RelayCommand(async () =>
        {
           



        });
        public ICommand EditSampleCommand => new RelayCommand(async () =>
        {




        });
    }

}
