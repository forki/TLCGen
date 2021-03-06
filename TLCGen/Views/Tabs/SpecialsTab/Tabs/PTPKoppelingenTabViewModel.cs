﻿using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using TLCGen.Helpers;
using TLCGen.Messaging.Messages;
using TLCGen.Messaging.Requests;
using TLCGen.Models;
using TLCGen.Plugins;

namespace TLCGen.ViewModels
{
    [TLCGenTabItem(index: 0, type: TabItemTypeEnum.SpecialsTab)]
    public class PTPKoppelingenTabViewModel : TLCGenTabItemViewModel
    {
        #region Fields

        private PTPKoppelingViewModel _SelectedPTPKoppeling;
        private ObservableCollection<PTPKoppelingViewModel> _PTPKoppelingen;

        #endregion // Fields

        #region Properties
        
        public ObservableCollection<PTPKoppelingViewModel> PTPKoppelingen
        {
            get
            {
                if (_PTPKoppelingen == null)
                    _PTPKoppelingen = new ObservableCollection<PTPKoppelingViewModel>();
                return _PTPKoppelingen;
            }
        }

        public PTPKoppelingViewModel SelectedPTPKoppeling
        {
            get { return _SelectedPTPKoppeling; }
            set
            {
                _SelectedPTPKoppeling = value;
                RaisePropertyChanged("SelectedPTPKoppeling");
            }
        }

        #endregion // Properties

        #region Commands

        RelayCommand _AddPTPKoppelingCommand;
        public ICommand AddPTPKoppelingCommand
        {
            get
            {
                if (_AddPTPKoppelingCommand == null)
                {
                    _AddPTPKoppelingCommand = new RelayCommand(AddPTPKoppelingCommand_Executed, AddPTPKoppelingCommand_CanExecute);
                }
                return _AddPTPKoppelingCommand;
            }
        }

        RelayCommand _RemovePTPKoppelingCommand;
        public ICommand RemovePTPKoppelingCommand
        {
            get
            {
                if (_RemovePTPKoppelingCommand == null)
                {
                    _RemovePTPKoppelingCommand = new RelayCommand(RemovePTPKoppelingCommand_Executed, RemovePTPKoppelingCommand_CanExecute);
                }
                return _RemovePTPKoppelingCommand;
            }
        }

        #endregion // Commands

        #region Command Functionality

        private bool AddPTPKoppelingCommand_CanExecute(object obj)
        {
            return true;
        }

        private void AddPTPKoppelingCommand_Executed(object obj)
        {
	        var inewname = 1;
			var ptp = new PTPKoppelingModel();
	        IsElementIdentifierUniqueRequest message;
	        do
	        {
		        inewname++;
		        ptp.TeKoppelenKruispunt = "ptpkruising" + (inewname < 10 ? "0" : "") + inewname;
		        message = new IsElementIdentifierUniqueRequest(ptp.TeKoppelenKruispunt, ElementIdentifierType.Naam);
		        Messenger.Default.Send(message);
	        }
	        while (!message.IsUnique);
			PTPKoppelingen.Add(new PTPKoppelingViewModel(ptp));
			MessengerInstance.Send(new PTPKoppelingenChangedMessage());
        }

        private bool RemovePTPKoppelingCommand_CanExecute(object obj)
        {
            return SelectedPTPKoppeling != null;
        }

        private void RemovePTPKoppelingCommand_Executed(object obj)
        {
            PTPKoppelingen.Remove(SelectedPTPKoppeling);
            SelectedPTPKoppeling = null;
			MessengerInstance.Send(new PTPKoppelingenChangedMessage());
        }

        #endregion // Command Functionality

        #region Public methods

        #endregion // Public methods

        #region TabItem Overrides

        public override string DisplayName
        {
            get
            {
                return "PTP";
            }
        }

        public override bool IsEnabled
        {
            get { return true; }
            set { }
        }

        public override void OnSelected()
        {
        }

        public override ControllerModel Controller
        {
            get
            {
                return base.Controller;
            }

            set
            {
                base.Controller = value;
                if (base.Controller != null)
                {

                    PTPKoppelingen.CollectionChanged -= PTPKoppelingen_CollectionChanged;
                    PTPKoppelingen.Clear();
                    foreach (PTPKoppelingModel ptp in _Controller.PTPData.PTPKoppelingen)
                    {
                        PTPKoppelingen.Add(new PTPKoppelingViewModel(ptp));
                    }
                    PTPKoppelingen.CollectionChanged += PTPKoppelingen_CollectionChanged;
                }
                else
                {
                    PTPKoppelingen.CollectionChanged -= PTPKoppelingen_CollectionChanged;
                    PTPKoppelingen.Clear();
                }
            }
        }

        #endregion // TabItem Overrides

        #region Collection Changed

        private void PTPKoppelingen_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count > 0)
            {
                foreach (PTPKoppelingViewModel ptp in e.NewItems)
                {
                    _Controller.PTPData.PTPKoppelingen.Add(ptp.PTPKoppeling);
                }
            }
            if (e.OldItems != null && e.OldItems.Count > 0)
            {
                foreach (PTPKoppelingViewModel ptp in e.OldItems)
                {
                    _Controller.PTPData.PTPKoppelingen.Remove(ptp.PTPKoppeling);
                }
            };
            MessengerInstance.Send(new ControllerDataChangedMessage());
        }

        #endregion // Collection Changed
        
        #region Constructor

        public PTPKoppelingenTabViewModel() : base()
        {

        }

        #endregion // Constructor
    }
}
