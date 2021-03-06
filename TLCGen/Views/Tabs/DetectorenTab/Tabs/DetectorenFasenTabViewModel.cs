﻿using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TLCGen.DataAccess;
using TLCGen.Extensions;
using TLCGen.Helpers;
using TLCGen.Messaging;
using TLCGen.Messaging.Messages;
using TLCGen.Messaging.Requests;
using TLCGen.Models;
using TLCGen.Plugins;
using TLCGen.Settings;

namespace TLCGen.ViewModels
{
    /// <summary>
    /// ViewModel for the View of detectors belonging to phases.
    /// </summary>
    [TLCGenTabItem(index: 0, type: TabItemTypeEnum.DetectieTab)]
    public class DetectorenFasenTabViewModel : TLCGenTabItemViewModel, IAllowTemplates<DetectorModel>
    {
        #region Fields

        private ObservableCollection<string> _Templates;
        private string _SelectedFaseNaam;
        private DetectorViewModel _SelectedDetector;
        private IList _SelectedDetectoren = new ArrayList();
        private List<string> _Fasen;
        private FaseCyclusModel _SelectedFase;

        #endregion // Fields

        #region Properties


        public List<string> Fasen
        {
            get
            {
                if(_Fasen == null)
                {
                    _Fasen = new List<string>();
                }
                return _Fasen;
            }
        }

        private ObservableCollection<DetectorViewModel> _Detectoren;
        public ObservableCollection<DetectorViewModel> Detectoren
        {
            get
            {
                if (_Detectoren == null)
                    _Detectoren = new ObservableCollection<DetectorViewModel>();
                return _Detectoren;
            }
        }

        public ObservableCollection<string> Templates
        {
            get
            {
                if (_Templates == null)
                {
                    _Templates = new ObservableCollection<string>();
                    _Templates.Add("Template placeholder");
                }
                return _Templates;
            }
        }

        public string SelectedFaseNaam
        {
            get => _SelectedFaseNaam;
	        set
            {
                _SelectedFaseNaam = value;
                if(_Controller != null && _Controller.Fasen.Any(x => x.Naam == value))
                {
                    _SelectedFase = _Controller.Fasen.First(x => x.Naam == value);
                }

                Detectoren.Clear();
                if (_SelectedFase != null)
                {
                    foreach (var dm in _SelectedFase.Detectoren)
                    {
	                    var dvm = new DetectorViewModel(dm) {FaseCyclus = value};
	                    dvm.PropertyChanged += Detector_PropertyChanged;
                        Detectoren.Add(dvm);
                    }
					Detectoren.BubbleSort();
                }
                if(Detectoren.Count > 0)
                    SelectedDetector = Detectoren[0];

                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Detectoren));
            }
        }

        public DetectorViewModel SelectedDetector
        {
            get { return _SelectedDetector; }
            set
            {
                _SelectedDetector = value;
                RaisePropertyChanged();
            }
        }

        public IList SelectedDetectoren
        {
            get { return _SelectedDetectoren; }
            set
            {
                _SelectedDetectoren = value;
                RaisePropertyChanged();
            }
        }

        private TemplateProviderViewModel<TLCGenTemplateModel<DetectorModel>, DetectorModel> _TemplatesProviderVM;
        public TemplateProviderViewModel<TLCGenTemplateModel<DetectorModel>, DetectorModel> TemplatesProviderVM
        {
            get
            {
                if (_TemplatesProviderVM == null)
                {
                    _TemplatesProviderVM = new TemplateProviderViewModel<TLCGenTemplateModel<DetectorModel>, DetectorModel>(this);
                }
                return _TemplatesProviderVM;
            }
        }

        #endregion // Properties

        #region Commands

        RelayCommand _AddDetectorCommand;
        public ICommand AddDetectorCommand
        {
            get
            {
                if (_AddDetectorCommand == null)
                {
                    _AddDetectorCommand = new RelayCommand(AddDetectorCommand_Executed, AddDetectorCommand_CanExecute);
                }
                return _AddDetectorCommand;
            }
        }


        RelayCommand _RemoveDetectorCommand;
        public ICommand RemoveDetectorCommand
        {
            get
            {
                if (_RemoveDetectorCommand == null)
                {
                    _RemoveDetectorCommand = new RelayCommand(RemoveDetectorCommand_Executed, RemoveDetectorCommand_CanExecute);
                }
                return _RemoveDetectorCommand;
            }
        }

        #endregion // Commands

        #region Command functionality

        void AddDetectorCommand_Executed(object prm)
        {
            var _dm = new DetectorModel();
            var newname = "1";
	        foreach (var dm in _SelectedFase.Detectoren)
            {
                if (Regex.IsMatch(dm.Naam, @"[0-9]$"))
                {
                    var m = Regex.Match(dm.Naam, @"[0-9]$");
                    var next = m.Value;
	                if (int.TryParse(next, out var inewname))
                    {
                        IsElementIdentifierUniqueRequest message;
                        do
                        {
                            newname = inewname.ToString();
                            message = new IsElementIdentifierUniqueRequest(_SelectedFase.Naam + newname, ElementIdentifierType.Naam);
                            Messenger.Default.Send(message);
                            if(!message.IsUnique)
                                inewname++;
                        }
                        while (!message.IsUnique);
                    }
                }
            }
            _dm.Naam = _SelectedFase.Naam + newname;
            _dm.VissimNaam = _dm.Naam;
            if(_SelectedFase.Detectoren.Count == 0)
            {
                if (_SelectedFase.Type == Models.Enumerations.FaseTypeEnum.Auto) _dm.Type = Models.Enumerations.DetectorTypeEnum.Kop;
                if (_SelectedFase.Type == Models.Enumerations.FaseTypeEnum.Fiets) _dm.Type = Models.Enumerations.DetectorTypeEnum.Kop;
                if (_SelectedFase.Type == Models.Enumerations.FaseTypeEnum.Voetganger) _dm.Type = Models.Enumerations.DetectorTypeEnum.KnopBuiten;
            }
            else
            {
                if(_SelectedFase.Type == Models.Enumerations.FaseTypeEnum.Auto) _dm.Type = Models.Enumerations.DetectorTypeEnum.Lang;
                if(_SelectedFase.Type == Models.Enumerations.FaseTypeEnum.Fiets) _dm.Type = Models.Enumerations.DetectorTypeEnum.Knop;
                if(_SelectedFase.Type == Models.Enumerations.FaseTypeEnum.Voetganger) _dm.Type = Models.Enumerations.DetectorTypeEnum.KnopBinnen;
            }
            DefaultsProvider.Default.SetDefaultsOnModel(_dm, _dm.Type.ToString(), _SelectedFase.Type.ToString());
	        var dvm1 = new DetectorViewModel(_dm)
	        {
		        FaseCyclus = _SelectedFase.Naam,
		        Rijstrook = 1
	        };
	        _SelectedFase.Detectoren.Add(_dm);
            _Detectoren.Add(dvm1);
	        Detectoren.BubbleSort();
			Messenger.Default.Send(new DetectorenChangedMessage());
        }

        bool AddDetectorCommand_CanExecute(object prm)
        {
            return _SelectedFase?.Detectoren != null;
        }

        void RemoveDetectorCommand_Executed(object prm)
        {
            var changed = false;
            if (SelectedDetectoren != null && SelectedDetectoren.Count > 0)
            {
                changed = true;
                foreach (DetectorViewModel dvm in SelectedDetectoren)
                {
                    Integrity.TLCGenControllerModifier.Default.RemoveDetectorFromController(dvm.Naam);
                }
            }
            else if (SelectedDetector != null)
            {
                changed = true;
                Integrity.TLCGenControllerModifier.Default.RemoveDetectorFromController(SelectedDetector.Naam);
            }

            if (changed)
            {
                SelectedFaseNaam = SelectedFaseNaam;
                Messenger.Default.Send(new DetectorenChangedMessage());
            }
        }

        bool RemoveDetectorCommand_CanExecute(object prm)
        {
            return _SelectedFase?.Detectoren != null && (SelectedDetector != null ||
                                                         SelectedDetectoren != null && SelectedDetectoren.Count > 0);
        }

        #endregion // Command functionality

        #region TabItem Overrides

        public override string DisplayName => "Fasen";

	    public override bool IsEnabled
        {
            get { return true; }
            set { }
        }

        public override ControllerModel Controller
        {
            get => base.Controller;

	        set
            {
                base.Controller = value;
                Detectoren.Clear();
                SelectedFaseNaam = null;
            }
        }

        public override void OnSelected()
        {
            var tfc = SelectedFaseNaam;
            Fasen.Clear();
            foreach (var fcm in _Controller.Fasen)
            {
                Fasen.Add(fcm.Naam);
            }
            if(tfc == null || !Fasen.Contains(tfc))
            {
                if (Fasen.Count > 0)
                {
                    SelectedFaseNaam = Fasen[0];
                    if (Detectoren?.Count > 0)
                    {
                        SelectedDetector = Detectoren[0];
                    }
                }
                else
                {
                    SelectedFaseNaam = null;
                    SelectedDetector = null;
                }
            }
            else if(Fasen.Contains(tfc))
            {
                SelectedFaseNaam = tfc;
            }
            if(SelectedDetector == null && Detectoren?.Count > 0)
            {
                SelectedDetector = Detectoren[0];
            }
        }

        public override void OnDeselected()
        {
	        if (_Controller == null) return;

            foreach (var fcm in _Controller.Fasen)
            {
                fcm.Detectoren.BubbleSort();
            }
        }

        #endregion // TabItem Overrides

        #region IAllowTemplates

        public void InsertItemsFromTemplate(List<DetectorModel> items)
        {
            if (_SelectedFase == null || _Controller == null)
                return;

            foreach(var d in items)
            {
                if (!Integrity.TLCGenIntegrityChecker.IsElementNaamUnique(_Controller, d.Naam))
                {
                    MessageBox.Show("Error bij toevoegen van detector met naam " + d.Naam + ".\nDe detector naam is niet uniek in de regeling.", "Error bij toepassen template");
                    return;
                }
            }
            foreach(var d in items)
            {
                _SelectedFase.Detectoren.Add(d);
                var dvm = new DetectorViewModel(d);
                dvm.FaseCyclus = SelectedFaseNaam;
                Detectoren.Add(dvm);

                Messenger.Default.Send(new ControllerDataChangedMessage());
            }
        }

        #endregion // IAllowTemplates

        #region Event handling

        private bool _SettingMultiple = false;
        private void Detector_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_SettingMultiple || string.IsNullOrEmpty(e.PropertyName))
                return;

            if (SelectedDetectoren != null && SelectedDetectoren.Count > 1)
            {
                _SettingMultiple = true;
                MultiPropertySetter.SetPropertyForAllItems<DetectorViewModel>(sender, e.PropertyName, SelectedDetectoren);
            }
            _SettingMultiple = false;
        }

        #endregion // Event handling

        #region Collection Changed

        #endregion // Collection Changed

        #region Constructor

        public DetectorenFasenTabViewModel() : base()
        {
        }

        #endregion // Constructor
    }
}
