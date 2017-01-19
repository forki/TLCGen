﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TLCGen.Models;
using TLCGen.Helpers;
using TLCGen.Plugins;
using TLCGen.ViewModels;
using System.Windows.Input;
using System.Xml;
using System.Windows.Media;
using TLCGen.DataAccess;
using TLCGen.Integrity;

namespace TLCGen.GebruikersOpties
{
    [TLCGenPlugin(TLCGenPluginElems.TabControl | TLCGenPluginElems.XMLNodeWriter)]
    public class GebruikersOptiesTabViewModel : ViewModelBase, ITLCGenXMLNodeWriter, ITLCGenTabItem, ITLCGenElementProvider
    {
        #region Constants

        const int UitgangenConst = 0;
        const int IngangenConst = 1;
        const int HulpElementenConst = 2;
        const int TimersConst = 3;
        const int CountersConst = 4;
        const int SchakelaarsConst = 5;
        const int GeheugenElementenConst = 6;
        const int ParametersConst = 7;
        const int OptiesMax = 8;

        #endregion // Constants

        #region Fields

        private GebruikersOptiesModel _MyGebruikersOpties;
        private int _SelectedTabIndex;

        private object[] _SelectedOptie = new object[OptiesMax];

        private RelayCommand _AddGebruikersOptieCommand;
        private RelayCommand _RemoveGebruikersOptieCommand;
        private RelayCommand _OmhoogCommand;
        private RelayCommand _OmlaagCommand;

        object[] _AlleOpties;

        #endregion // Fields

        #region Properties

        public ObservableCollectionAroundList<GebruikersOptieWithIOViewModel, GebruikersOptieWithIOModel> Uitgangen { get; private set; }
        public ObservableCollectionAroundList<GebruikersOptieWithIOViewModel, GebruikersOptieWithIOModel> Ingangen { get; private set; }
        public ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel> HulpElementen { get; private set; }
        public ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel> Timers { get; private set; }
        public ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel> Counters { get; private set; }
        public ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel> Schakelaars { get; private set; }
        public ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel> GeheugenElementen { get; private set; }
        public ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel> Parameters { get; private set; }

        public GebruikersOptiesModel MyGebruikersOpties
        {
            get { return _MyGebruikersOpties; }
            set
            {
                _MyGebruikersOpties = value;

                OnMonitoredPropertyChanged(null);
            }
        }

        public int SelectedTabIndex
        {
            get { return _SelectedTabIndex; }
            set
            {
                if (CheckCurrentItemNameUnique())
                {
                    _SelectedTabIndex = value;
                }
                OnMonitoredPropertyChanged("SelectedTabIndex");
                OnMonitoredPropertyChanged("SelectedOptie");
            }
        }

        public object SelectedOptie
        {
            get
            {
                if (SelectedTabIndex >= 0 && SelectedTabIndex < OptiesMax)
                    return _SelectedOptie[SelectedTabIndex];
                else
                    return null;
            }
            set
            {
                if (SelectedTabIndex >= 0 && SelectedTabIndex < OptiesMax)
                {
                    _SelectedOptie[SelectedTabIndex] = value;
                }
                OnPropertyChanged("SelectedOptie");
            }
        }

        #endregion // Properties

        #region Commands

        public ICommand AddGebruikersOptieCommand
        {
            get
            {
                if (_AddGebruikersOptieCommand == null)
                {
                    _AddGebruikersOptieCommand = new RelayCommand(AddNewGebruikersOptieCommand_Executed, AddNewGebruikersOptieCommand_CanExecute);
                }
                return _AddGebruikersOptieCommand;
            }
        }

        public ICommand RemoveGebruikersOptieCommand
        {
            get
            {
                if (_RemoveGebruikersOptieCommand == null)
                {
                    _RemoveGebruikersOptieCommand = new RelayCommand(RemoveGebruikersOptieCommand_Executed, RemoveGebruikersOptieCommand_CanExecute);
                }
                return _RemoveGebruikersOptieCommand;
            }
        }

        public ICommand OmhoogCommand
        {
            get
            {
                if (_OmhoogCommand == null)
                {
                    _OmhoogCommand = new RelayCommand(OmhoogCommand_Executed, OmhoogCommand_CanExecute);
                }
                return _OmhoogCommand;
            }
        }

        public ICommand OmlaagCommand
        {
            get
            {
                if (_OmlaagCommand == null)
                {
                    _OmlaagCommand = new RelayCommand(OmlaagCommand_Executed, OmlaagCommand_CanExecute);
                }
                return _OmlaagCommand;
            }
        }

        #endregion // Commands

        #region Command functionality

        void AddNewGebruikersOptieCommand_Executed(object prm)
        {
            int index = -1;

            if (SelectedTabIndex == UitgangenConst || SelectedTabIndex == IngangenConst)
            {
                if(SelectedOptie != null)
                    index = ((ObservableCollectionAroundList<GebruikersOptieWithIOViewModel, GebruikersOptieWithIOModel>)_AlleOpties[SelectedTabIndex]).IndexOf(
                        SelectedOptie as GebruikersOptieWithIOViewModel);

                var o = new GebruikersOptieWithIOViewModel(new GebruikersOptieWithIOModel());

                if(index > 0)
                    ((ObservableCollectionAroundList<GebruikersOptieWithIOViewModel, GebruikersOptieWithIOModel>)_AlleOpties[SelectedTabIndex]).Insert(index, o);
                else
                    ((ObservableCollectionAroundList<GebruikersOptieWithIOViewModel, GebruikersOptieWithIOModel>)_AlleOpties[SelectedTabIndex]).Add(o);
            }
            else
            {
                if(SelectedOptie != null)
                    index = ((ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel>)_AlleOpties[SelectedTabIndex]).IndexOf(
                        SelectedOptie as GebruikersOptieViewModel);
                var o = new GebruikersOptieViewModel(new GebruikersOptieModel());
                if(index > 0)
                    ((ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel>)_AlleOpties[SelectedTabIndex]).Insert(index, o);
                else
                    ((ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel>)_AlleOpties[SelectedTabIndex]).Add(o);
            }

            if (SelectedTabIndex == UitgangenConst || SelectedTabIndex == IngangenConst)
                ((ObservableCollectionAroundList<GebruikersOptieWithIOViewModel, GebruikersOptieWithIOModel>)_AlleOpties[SelectedTabIndex]).RebuildList();
            else
                ((ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel>)_AlleOpties[SelectedTabIndex]).RebuildList();
        }

        bool AddNewGebruikersOptieCommand_CanExecute(object prm)
        {
            return true;
        }

        void RemoveGebruikersOptieCommand_Executed(object prm)
        {
            if (SelectedTabIndex == UitgangenConst || SelectedTabIndex == IngangenConst)
                ((ObservableCollectionAroundList<GebruikersOptieWithIOViewModel, GebruikersOptieWithIOModel>)_AlleOpties[SelectedTabIndex]).Remove(
                    SelectedOptie as GebruikersOptieWithIOViewModel);
            else
                ((ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel>)_AlleOpties[SelectedTabIndex]).Remove(
                    SelectedOptie as GebruikersOptieViewModel);

            SelectedOptie = null;
        }

        bool RemoveGebruikersOptieCommand_CanExecute(object prm)
        {
            return SelectedOptie != null;
        }

        void OmhoogCommand_Executed(object prm)
        {
            var optie = SelectedOptie;

            int index = -1;

            if (SelectedTabIndex == UitgangenConst || SelectedTabIndex == IngangenConst)
                index = ((ObservableCollectionAroundList<GebruikersOptieWithIOViewModel, GebruikersOptieWithIOModel>)_AlleOpties[SelectedTabIndex]).IndexOf(
                    SelectedOptie as GebruikersOptieWithIOViewModel);
            else
                index = ((ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel>)_AlleOpties[SelectedTabIndex]).IndexOf(
                    SelectedOptie as GebruikersOptieViewModel);

            if(index > 0)
            {
                if (SelectedTabIndex == UitgangenConst || SelectedTabIndex == IngangenConst)
                    ((ObservableCollectionAroundList<GebruikersOptieWithIOViewModel, GebruikersOptieWithIOModel>)_AlleOpties[SelectedTabIndex]).Move(
                        index, index - 1);
                else
                    ((ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel>)_AlleOpties[SelectedTabIndex]).Move(
                        index, index - 1);
            }

            if (SelectedTabIndex == UitgangenConst || SelectedTabIndex == IngangenConst)
                ((ObservableCollectionAroundList<GebruikersOptieWithIOViewModel, GebruikersOptieWithIOModel>)_AlleOpties[SelectedTabIndex]).RebuildList();
            else
                ((ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel>)_AlleOpties[SelectedTabIndex]).RebuildList();

            SelectedOptie = optie;
        }

        bool OmhoogCommand_CanExecute(object prm)
        {
            return SelectedOptie != null;
        }

        void OmlaagCommand_Executed(object prm)
        {
            var optie = SelectedOptie;

            int index = -1;
            int max = -1;

            if (SelectedTabIndex == UitgangenConst || SelectedTabIndex == IngangenConst)
            {
                index = ((ObservableCollectionAroundList<GebruikersOptieWithIOViewModel, GebruikersOptieWithIOModel>)_AlleOpties[SelectedTabIndex]).IndexOf(
                    SelectedOptie as GebruikersOptieWithIOViewModel);
                max = (((ObservableCollectionAroundList<GebruikersOptieWithIOViewModel, GebruikersOptieWithIOModel>)_AlleOpties[SelectedTabIndex]).Count - 1);
            }
            else
            {
                index = ((ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel>)_AlleOpties[SelectedTabIndex]).IndexOf(
                    SelectedOptie as GebruikersOptieViewModel);
                max = (((ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel>)_AlleOpties[SelectedTabIndex]).Count - 1);
            }

            if (index >= 0 && index < max)
            {
                if (SelectedTabIndex == UitgangenConst || SelectedTabIndex == IngangenConst)
                    ((ObservableCollectionAroundList<GebruikersOptieWithIOViewModel, GebruikersOptieWithIOModel>)_AlleOpties[SelectedTabIndex]).Move(
                        index, index + 1);
                else
                    ((ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel>)_AlleOpties[SelectedTabIndex]).Move(
                        index, index + 1);
            }

            if (SelectedTabIndex == UitgangenConst || SelectedTabIndex == IngangenConst)
                ((ObservableCollectionAroundList<GebruikersOptieWithIOViewModel, GebruikersOptieWithIOModel>)_AlleOpties[SelectedTabIndex]).RebuildList();
            else
                ((ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel>)_AlleOpties[SelectedTabIndex]).RebuildList();

            SelectedOptie = optie;
        }

        bool OmlaagCommand_CanExecute(object prm)
        {
            return SelectedOptie != null;
        }

        #endregion // Command functionality

        #region Private Methods

        private bool CheckCurrentItemNameUnique()
        {
            if (SelectedOptie == null)
                return true;

            var oio = SelectedOptie as GebruikersOptieWithIOViewModel;
            var o = SelectedOptie as GebruikersOptieViewModel;
            if (oio != null)
            {
                return IntegrityChecker.IsElementNaamUnique(_Controller, oio.Naam);
            }
            else
            {
                return IntegrityChecker.IsElementNaamUnique(_Controller, o.Naam);
            }
        }

        #endregion // Private Methods

        #region ITLCGenTabItem

        private ControllerModel _Controller;
        public ControllerModel Controller
        {
            get { return _Controller; }
            set
            {
                _Controller = value;
                OnMonitoredPropertyChanged("Controller");
            }
        }

        DataTemplate _ContentDataTemplate;
        public DataTemplate ContentDataTemplate
        {
            get
            {
                if (_ContentDataTemplate == null)
                {
                    _ContentDataTemplate = new DataTemplate();
                    _ContentDataTemplate.VisualTree = new FrameworkElementFactory(typeof(GebruikersOptiesTabView));
                }
                return _ContentDataTemplate;
            }
        }

        public string DisplayName
        {
            get
            {
                return "Gebruikersopties";
            }
        }

        public string GetPluginName()
        {
            return "CCOLGebruikersOpties";
        }

        private bool _IsEnabled;
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set
            {
                _IsEnabled = value;
                OnPropertyChanged("IsEnabled");
            }
        }

        public ImageSource Icon
        {
            get
            {
                ResourceDictionary dict = new ResourceDictionary();
                Uri u = new Uri("pack://application:,,,/" +
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Name +
                    ";component/" + "GebruikersOptiesIcon.xaml");
                dict.Source = u;
                return (DrawingImage)dict["GebruikersOptiesTabDrawingImage"];
            }
        }

        public bool CanBeEnabled()
        {
            return true;
        }

        public void OnSelected()
        {
            
        }

        public bool OnSelectedPreview()
        {
            return CheckCurrentItemNameUnique();
        }

        public void OnDeselected()
        {
            
        }

        public bool OnDeselectedPreview()
        {
            return true;
        }

        #endregion // ITLCGenTabItem

        #region ITLCGenXMLNodeWriter

        public void GetXmlFromDocument(XmlDocument document)
        {
            bool found = false;
            foreach(XmlNode node in document.FirstChild.ChildNodes)
            {
                if(node.LocalName == "GebruikersOpties")
                {
                    MyGebruikersOpties = XmlNodeConverter.ConvertNode<GebruikersOptiesModel>(node);
                    found = true;
                    break;
                }
            }

            if (found)
            {
                Uitgangen = new ObservableCollectionAroundList<GebruikersOptieWithIOViewModel, GebruikersOptieWithIOModel>(_MyGebruikersOpties.Uitgangen);
                Ingangen = new ObservableCollectionAroundList<GebruikersOptieWithIOViewModel, GebruikersOptieWithIOModel>(_MyGebruikersOpties.Ingangen);
                HulpElementen = new ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel>(_MyGebruikersOpties.HulpElementen);
                Timers = new ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel>(_MyGebruikersOpties.Timers);
                Counters = new ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel>(_MyGebruikersOpties.Counters);
                Schakelaars = new ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel>(_MyGebruikersOpties.Schakelaars);
                GeheugenElementen = new ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel>(_MyGebruikersOpties.GeheugenElementen);
                Parameters = new ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel>(_MyGebruikersOpties.Parameters);

                _AlleOpties = new object[8];
                _AlleOpties[UitgangenConst] = Uitgangen;
                _AlleOpties[IngangenConst] = Ingangen;
                _AlleOpties[HulpElementenConst] = HulpElementen;
                _AlleOpties[TimersConst] = Timers;
                _AlleOpties[CountersConst] = Counters;
                _AlleOpties[SchakelaarsConst] = Schakelaars;
                _AlleOpties[GeheugenElementenConst] = GeheugenElementen;
                _AlleOpties[ParametersConst] = Parameters;

                OnPropertyChanged(null);
            }
        }

        public void SetXmlInDocument(XmlDocument document)
        {
            XmlDocument doc = TLCGenSerialization.SerializeToXmlDocument(MyGebruikersOpties);
            XmlNode node = document.ImportNode(doc.DocumentElement, true);
            document.DocumentElement.AppendChild(node);
        }

        #endregion // ITLCGenXMLNodeWriter

        #region ITLCGenElementProvider

        public List<IOElementModel> GetOutputItems()
        {
            List<IOElementModel> items = new List<IOElementModel>();
            foreach(var v in Uitgangen)
            {
                items.Add(v.GebruikersOptieWithOI as IOElementModel);
            }
            return items;
        }

        public List<IOElementModel> GetInputItems()
        {
            List<IOElementModel> items = new List<IOElementModel>();
            foreach (var v in Ingangen)
            {
                items.Add(v.GebruikersOptieWithOI as IOElementModel);
            }
            return items;
        }

        public bool IsElementNameUnique(string name)
        {
            foreach(var o in Uitgangen) { if (o.Naam == name) return false; }
            foreach(var o in Ingangen) { if (o.Naam == name) return false; }
            foreach(var o in HulpElementen) { if (o.Naam == name) return false; }
            foreach(var o in Timers) { if (o.Naam == name) return false; }
            foreach(var o in Counters) { if (o.Naam == name) return false; }
            foreach(var o in Schakelaars) { if (o.Naam == name) return false; }
            foreach(var o in GeheugenElementen) { if (o.Naam == name) return false; }
            foreach(var o in Parameters) { if (o.Naam == name) return false; }
            return true;
        }

        #endregion // ITLCGenElementProvider

        #region Constructor

        public GebruikersOptiesTabViewModel()
        {
            MyGebruikersOpties = new GebruikersOptiesModel();

            Uitgangen = new ObservableCollectionAroundList<GebruikersOptieWithIOViewModel, GebruikersOptieWithIOModel>(_MyGebruikersOpties.Uitgangen);
            Ingangen = new ObservableCollectionAroundList<GebruikersOptieWithIOViewModel, GebruikersOptieWithIOModel>(_MyGebruikersOpties.Ingangen);
            HulpElementen = new ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel>(_MyGebruikersOpties.HulpElementen);
            Timers = new ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel>(_MyGebruikersOpties.Timers);
            Counters = new ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel>(_MyGebruikersOpties.Counters);
            Schakelaars = new ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel>(_MyGebruikersOpties.Schakelaars);
            GeheugenElementen = new ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel>(_MyGebruikersOpties.GeheugenElementen);
            Parameters = new ObservableCollectionAroundList<GebruikersOptieViewModel, GebruikersOptieModel>(_MyGebruikersOpties.Parameters);

            _AlleOpties = new object[8];
            _AlleOpties[UitgangenConst] = Uitgangen;
            _AlleOpties[IngangenConst] = Ingangen;
            _AlleOpties[HulpElementenConst] = HulpElementen;
            _AlleOpties[TimersConst] = Timers;
            _AlleOpties[CountersConst] = Counters;
            _AlleOpties[SchakelaarsConst] = Schakelaars;
            _AlleOpties[GeheugenElementenConst] = GeheugenElementen;
            _AlleOpties[ParametersConst] = Parameters;
        }

        #endregion // Constructor
    }
}
