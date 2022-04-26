﻿using Elden_Ring_Debug_Tool;
using Elden_Ring_Debug_Tool_ViewModels.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Elden_Ring_Debug_Tool_ViewModels.Commands
{
    public class ResetParamCommand : CommandBase
    {
        private ParamViewerViewModel _paramViewerViewModel;
        private ERHook _hook;

        public ResetParamCommand(ParamViewerViewModel paramViewerViewModel, ERHook hook)
        {
            _paramViewerViewModel = paramViewerViewModel;
            _hook = hook;
            _paramViewerViewModel.PropertyChanged += _paramViewerViewModel_PropertyChanged;
        }

        private void _paramViewerViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ParamViewerViewModel.Setup))
            {
                OnCanExecuteChanged();
            }
        }


        public override bool CanExecute(object? parameter)
        {
            return _hook?.Setup ?? false && base.CanExecute(parameter);
        }

        public override void Execute(object? parameter)
        {
            var result = MessageBox.Show("Are you sure you want to reset the currently selected param to what it was when the debug tool loaded?", "Reset Param", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                _paramViewerViewModel.SelectedParam.Param.RestoreParam();
            }
        }
    }
}