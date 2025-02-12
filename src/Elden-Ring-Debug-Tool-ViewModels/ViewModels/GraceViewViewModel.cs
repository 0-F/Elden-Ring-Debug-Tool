﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Elden_Ring_Debug_Tool_ViewModels.Commands;
using Elden_Ring_Debug_Tool_ViewModels.ViewModels.SubViewModels;
using Erd_Tools;
using Erd_Tools.Models;
using PropertyHook;
using Grace = Erd_Tools.Models.Grace;
using GraceViewModel = Elden_Ring_Debug_Tool_ViewModels.ViewModels.SubViewModels.GraceViewModel;

namespace Elden_Ring_Debug_Tool_ViewModels.ViewModels
{
    public class GraceViewViewModel : ViewModelBase
    {
        public ICollectionView GraceCollectionView { get; set; }
        public ICollectionView HubCollectionView { get; set; }

        public ICommand WarpCommand { get; }
        public ICommand SetGraceCommand { get; }
        public ICommand ManageAllGraceCommand { get; }
        public ICommand ManageAllHubsCommand { get; }

        internal ErdHook Hook { get; set; }

        public GraceViewViewModel()
        {
            WarpCommand = new WarpCommand(this);
            SetGraceCommand = new SetGraceCommand(this);
            ManageAllGraceCommand = new ManageAllGraceCommand(this);
            ManageAllHubsCommand = new ManageAllHubsCommand(this);
        }
        
        public bool MassChange { get; set; }

        public void UpdateViewModel() {

            if (!MassChange) 
            {
                foreach (GraceViewModel grace in GraceViewModel.All)
                {
                    grace.Update(Hook.CheckGraceStatus(grace.PtrOffset, grace.DataOffset, grace.BitStart));
                }
            }
       

            LastGraceID = Hook.LastGrace;

            Setup = Hook.Setup;
            Loaded = Hook.Loaded;
        }


        public void ReloadViewModel()
        {
            if (string.IsNullOrWhiteSpace(GraceFilter))
            {
                GraceViewModel? graceViewModel = GraceViewModel.All.FirstOrDefault(g => g.EntityID + 1000 == Hook.LastGrace);

                if (graceViewModel != null)
                {
                    SelectedGraceViewModel = graceViewModel;
                }
            }

        }

        private bool _setup;
        public bool Setup
        {
            get => _setup;
            set => SetField(ref _setup, value);
        }

        private bool _loaded;
        public bool Loaded
        {
            get => _loaded;
            set => SetField(ref _loaded, value);
        }

        public void InitViewModel(ErdHook hook)
        {
            Hook = hook;
            Hook.OnSetup += Hook_OnSetup;
            foreach (Continent continent in Continent.All)
            {
                new ContinentViewModel(continent, Hook);
            }

            GraceCollectionView = CollectionViewSource.GetDefaultView(GraceViewModel.All);
            GraceCollectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(GraceViewModel.Continent)));
            GraceCollectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(GraceViewModel.Hub)));
            GraceCollectionView.Filter += FilerGrace;
            SelectedGraceViewModel = (GraceViewModel)GraceCollectionView.CurrentItem;

            HubCollectionView = CollectionViewSource.GetDefaultView(HubViewModel.All);
            HubCollectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(HubViewModel.Continent)));
            HubCollectionView.Filter += FilerHub;
            SelectedHubViewModel = (HubViewModel)HubCollectionView.CurrentItem;
        }

        private void Hook_OnSetup(object? sender, PHEventArgs e)
        {
            //System.Windows.Application.Current.Dispatcher.Invoke(() =>
            //{
            //    foreach (GraceViewModel grace in GraceViewModel.All)
            //    {
            //        grace.Update(Hook.CheckGraceStatus(grace.Grace));
            //    }
            //});
        }

        private bool _quickSelectBonfire;

        public bool QuickSelectBonfire
        {
            get => _quickSelectBonfire;
            set => SetField(ref _quickSelectBonfire, value);
        }

        private int _lastGraceID;

        public int LastGraceID
        {
            get => _lastGraceID;
            set
            {
                if (SetField(ref _lastGraceID, value))
                {
                    int lastGraceID = LastGraceID - 1000;
                    if (lastGraceID != LastGraceViewModel?.EntityID)
                    {
                        GraceViewModel? graceViewModel = GraceViewModel.All.FirstOrDefault(g => g.EntityID == lastGraceID);
                        if (graceViewModel != null)
                        {
                            LastGraceViewModel = graceViewModel;
                        }
                    }
                }
              
            }

        }

        private GraceViewModel? _lastGraceViewModel;
        public GraceViewModel? LastGraceViewModel
        {
            get => _lastGraceViewModel;
            set => SetField(ref _lastGraceViewModel, value);
        }


        private GraceViewModel _selectedGraceViewModel;

        public GraceViewModel SelectedGraceViewModel
        {
            get => _selectedGraceViewModel;
            set
            {
                if (SetField(ref _selectedGraceViewModel, value) && QuickSelectBonfire)
                {
                    Hook.LastGrace = SelectedGraceViewModel.EntityID + 1000;
                    //LastGraceViewModel = SelectedGraceViewModel;
                }
            }


        }

        private HubViewModel _selectedHubViewModel;

        public HubViewModel SelectedHubViewModel
        {
            get => _selectedHubViewModel;
            set => SetField(ref _selectedHubViewModel, value);
        }
        #region Search

        private string _graceFilter = string.Empty;
        public string GraceFilter
        {
            get => _graceFilter;
            set
            {
                if (SetField(ref _graceFilter, value))
                {
                    GraceCollectionView.Refresh();

                    if (!GraceCollectionView.IsEmpty)
                    {
                        GraceCollectionView.MoveCurrentToFirst();
                        SelectedGraceViewModel = (GraceViewModel)GraceCollectionView.CurrentItem;
                    }
                }
            }
        }

        private bool FilerGrace(object obj)
        {
            if (obj is GraceViewModel grace)
            {
                return grace.Name.Contains(GraceFilter, StringComparison.InvariantCultureIgnoreCase);
            }

            return false;
        }

        private string _hubFilter = string.Empty;
        public string HubFilter
        {
            get => _hubFilter;
            set
            {
                if (SetField(ref _hubFilter, value))
                {
                    HubCollectionView.Refresh();
                    if (!HubCollectionView.IsEmpty)
                    {
                        HubCollectionView.MoveCurrentToFirst();
                        SelectedHubViewModel = (HubViewModel)HubCollectionView.CurrentItem;
                        return;
                    }
                }
            }
        }

        private bool FilerHub(object obj)
        {
            if (obj is HubViewModel hub)
            {
                return hub.Name.Contains(HubFilter, StringComparison.InvariantCultureIgnoreCase);
            }

            return false;
        }
        #endregion

    }
}
