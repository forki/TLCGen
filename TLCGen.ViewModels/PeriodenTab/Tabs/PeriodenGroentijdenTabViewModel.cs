﻿using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using TLCGen.Helpers;
using TLCGen.Messaging.Messages;
using TLCGen.Models;

namespace TLCGen.ViewModels
{
    [TLCGenTabItem(index: 0, type: TabItemTypeEnum.PeriodenTab)]
    public class PeriodenGroentijdenTabViewModel : TLCGenTabItemViewModel
    {
        #region Fields
        
        private PeriodeViewModel _SelectedPeriode;
        private ObservableCollection<string> _GroentijdenSets;

        #endregion // Fields

        #region Properties

        public ObservableCollectionAroundList<PeriodeViewModel, PeriodeModel> Periodes
        {
            get; private set;
        }

        public ObservableCollection<string> GroentijdenSets
        {
            get
            {
                if (_GroentijdenSets == null)
                {
                    _GroentijdenSets = new ObservableCollection<string>();
                }
                return _GroentijdenSets;
            }
        }

        public PeriodeViewModel SelectedPeriode
        {
            get { return _SelectedPeriode; }
            set
            {
                _SelectedPeriode = value;
                OnPropertyChanged("SelectedPeriode");
            }
        }

        public string DefaultPeriodeGroentijdenSet
        {
            get { return _Controller.PeriodenData.DefaultPeriodeGroentijdenSet; }
            set
            {
                _Controller.PeriodenData.DefaultPeriodeGroentijdenSet = value;
                OnMonitoredPropertyChanged("DefaultPeriodeGroentijdenSet");
            }
        }

        public string DefaultPeriodeNaam
        {
            get { return _Controller.PeriodenData.DefaultPeriodeNaam; }
            set
            {
                _Controller.PeriodenData.DefaultPeriodeNaam = value;
                OnMonitoredPropertyChanged("DefaultPeriodeNaam");
            }
        }

        #endregion // Properties

        #region Commands


        RelayCommand _AddPeriodeCommand;
        public ICommand AddPeriodeCommand
        {
            get
            {
                if (_AddPeriodeCommand == null)
                {
                    _AddPeriodeCommand = new RelayCommand(AddNewPeriodeCommand_Executed, AddNewPeriodeCommand_CanExecute);
                }
                return _AddPeriodeCommand;
            }
        }


        RelayCommand _RemovePeriodeCommand;
        public ICommand RemovePeriodeCommand
        {
            get
            {
                if (_RemovePeriodeCommand == null)
                {
                    _RemovePeriodeCommand = new RelayCommand(RemovePeriodeCommand_Executed, ChangePeriodeCommand_CanExecute);
                }
                return _RemovePeriodeCommand;
            }
        }

        RelayCommand _MovePeriodeUpCommand;
        public ICommand MovePeriodeUpCommand
        {
            get
            {
                if (_MovePeriodeUpCommand == null)
                {
                    _MovePeriodeUpCommand = new RelayCommand(MovePeriodeUpCommand_Executed, ChangePeriodeCommand_CanExecute);
                }
                return _MovePeriodeUpCommand;
            }
        }

        RelayCommand _MovePeriodeDownCommand;
        public ICommand MovePeriodeDownCommand
        {
            get
            {
                if (_MovePeriodeDownCommand == null)
                {
                    _MovePeriodeDownCommand = new RelayCommand(MovePeriodeDownCommand_Executed, ChangePeriodeCommand_CanExecute);
                }
                return _MovePeriodeDownCommand;
            }
        }

        #endregion // Commands

        #region Command Functionality

        private void MovePeriodeUpCommand_Executed(object obj)
        {
            int index = -1;
            foreach (PeriodeViewModel mvm in Periodes)
            {
                ++index;
                if (mvm == SelectedPeriode)
                {
                    break;
                }
            }
            if (index >= 1)
            {
                PeriodeViewModel mvm = SelectedPeriode;
                SelectedPeriode = null;
                Periodes.Remove(mvm);
                Periodes.Insert(index - 1, mvm);
                SelectedPeriode = mvm;
            }

            Periodes.RebuildList();
        }


        private void MovePeriodeDownCommand_Executed(object obj)
        {
            int index = -1;
            
            foreach (PeriodeViewModel mvm in Periodes)
            {
                ++index;
                if (mvm == SelectedPeriode)
                {
                    break;
                }
            }
            if (index >= 0 && (index <= (Periodes.Count - 2)))
            {
                PeriodeViewModel mvm = SelectedPeriode;
                SelectedPeriode = null;
                Periodes.Remove(mvm);
                Periodes.Insert(index + 1, mvm);
                SelectedPeriode = mvm;
            }

            Periodes.RebuildList();
        }

        void AddNewPeriodeCommand_Executed(object prm)
        {
            PeriodeModel mm = new PeriodeModel();
            mm.Type = Models.Enumerations.PeriodeTypeEnum.Groentijden;
            mm.DagCode = Models.Enumerations.PeriodeDagCodeEnum.AlleDagen;
            mm.Naam = "MGPeriode" + (Periodes.Count + 1).ToString();
            PeriodeViewModel mvm = new PeriodeViewModel(mm);
            Periodes.Add(mvm);
        }

        bool AddNewPeriodeCommand_CanExecute(object prm)
        {
            return Periodes != null && GroentijdenSets?.Count > 0;
        }

        void RemovePeriodeCommand_Executed(object prm)
        {
            Periodes.Remove(SelectedPeriode);
        }

        bool ChangePeriodeCommand_CanExecute(object prm)
        {
            return SelectedPeriode != null;
        }

        #endregion // Command Functionality

        #region TabItem Overrides

        public override string DisplayName
        {
            get
            {
                return "Groentijden";
            }
        }

        public override bool IsEnabled
        {
            get { return true; }
            set { }
        }

        public override void OnSelected()
        {
            var v = _Controller.PeriodenData.DefaultPeriodeGroentijdenSet;
            GroentijdenSets.Clear();
            foreach (GroentijdenSetModel gsm in _Controller.GroentijdenSets)
            {
                GroentijdenSets.Add(gsm.Naam);
            }
            _Controller.PeriodenData.DefaultPeriodeGroentijdenSet = v;
            OnPropertyChanged("DefaultPeriodeGroentijdenSet");
        }

        #endregion // TabItem Overrides

        #region Private Methods

        private bool FilterPerioden(object o)
        {
            var per = (PeriodeViewModel)o;
            return per.Type == Models.Enumerations.PeriodeTypeEnum.Groentijden;
        }

        #endregion // Private Methods

        #region Collection Changed

        private void Periodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Messenger.Default.Send(new ControllerDataChangedMessage());
        }

        #endregion // Collection Changed

        #region Constructor

        public PeriodenGroentijdenTabViewModel(ControllerModel controller) : base(controller)
        {
            Periodes = new ObservableCollectionAroundList<PeriodeViewModel, PeriodeModel>(controller.PeriodenData.Perioden);

            Periodes.CollectionChanged += Periodes_CollectionChanged;

            ICollectionView view = CollectionViewSource.GetDefaultView(Periodes);
            view.Filter = FilterPerioden;
        }

        #endregion // Constructor
    }
}