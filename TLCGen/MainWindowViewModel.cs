﻿using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.Threading;
using GongSolutions.Wpf.DragDrop;
using Microsoft.Win32;
using TLCGen.DataAccess;
using TLCGen.Dialogs;
using TLCGen.GuiActions;
using TLCGen.Helpers;
using TLCGen.Integrity;
using TLCGen.Messaging.Messages;
using TLCGen.Messaging.Requests;
using TLCGen.ModelManagement;
using TLCGen.Models;
using TLCGen.Plugins;
using TLCGen.Settings;
using System.Windows.Shell;

namespace TLCGen.ViewModels
{
    public class MainWindowViewModel : GalaSoft.MvvmLight.ViewModelBase, IDropTarget
    {
        #region Fields

        private ControllerViewModel _ControllerVM;
        private TLCGenStatusBarViewModel _StatusBarVM;

        private List<Tuple<TLCGenPluginElems, ITLCGenPlugin>> _ApplicationParts;

        private List<MenuItem> _ImportMenuItems;
        private List<MenuItem> _PluginMenuItems;
        private List<IGeneratorViewModel> _Generators;
        
        private IGeneratorViewModel _SelectedGenerator;

        #endregion // Fields

        #region Properties

        public List<Tuple<TLCGenPluginElems, ITLCGenPlugin>> ApplicationParts
        {
            get { return _ApplicationParts; }
        }

        /// <summary>
        /// ViewModel for the Controller data object that is being edited via the application.
        /// This is the main ViewModel for the application, which holds all other ViewModels.
        /// (Except for the Settings ViewModel, which belongs to the application rather than 
        /// the Controller.)
        /// </summary>
        public ControllerViewModel ControllerVM
        {
            get { return _ControllerVM; }
            set
            {
                _ControllerVM = value;
                foreach(var pl in ApplicationParts)
                {
                    pl.Item2.Controller = TLCGenControllerDataProvider.Default.Controller;
                }
                
                RaisePropertyChanged("ControllerVM");
                RaisePropertyChanged("HasController");
            }
        }

        /// <summary>
        /// ViewModel for status bar
        /// </summary>
        public TLCGenStatusBarViewModel StatusBarVM
        {
            get
            {
                if (_StatusBarVM == null)
                    _StatusBarVM = new TLCGenStatusBarViewModel();
                return _StatusBarVM;
            }
        }

        /// <summary>
        /// Boolean used by the View to determine of a Controller has been loaded.
        /// This is used in the View to enable/disable appropriate UI elementents.
        /// </summary>
        public bool HasController
        {
            get { return TLCGenControllerDataProvider.Default.Controller != null; }
        }

        /// <summary>
        /// A string to be used in the View as the title of the program.
        /// File operations should call OnPropertyChanged for this property,
        /// so the View updates the program title.
        /// </summary>
        public string ProgramTitle
        {
            get
            {
                if(HasController && !string.IsNullOrEmpty(TLCGenControllerDataProvider.Default.ControllerFileName))
                    return "TLCGen - " + TLCGenControllerDataProvider.Default.ControllerFileName;
                else
                    return "TLCGen";
            }
        }
        
        public List<IGeneratorViewModel> Generators
        {
            get
            {
                if(_Generators == null)
                {
                    _Generators = new List<IGeneratorViewModel>();
                }
                return _Generators;
            }
        }

        private List<ITLCGenImporter> _Importers;
        public List<ITLCGenImporter> Importers
        {
            get
            {
                if(_Importers == null)
                {
                    _Importers = new List<ITLCGenImporter>();
                }
                return _Importers;
            }
        }

        private List<ITLCGenTabItem> _TabItems;
        public List<ITLCGenTabItem> TabItems
        {
            get
            {
                if(_TabItems == null)
                {
                    _TabItems = new List<ITLCGenTabItem>();
                }
                return _TabItems;
            }
        }

        /// <summary>
        /// Holds the selected code generator, on which the appropriate function calls are invoked
        /// when commands relating to generators are executed.
        /// </summary>
        public IGeneratorViewModel SelectedGenerator
        {
            get { return _SelectedGenerator; }
            set
            {
                _SelectedGenerator = value;
                RaisePropertyChanged("SelectedGenerator");
            }
        }

        /// <summary>
        /// Holds a list of available menu items that are bound to the View. This allows the user
        /// to click a menu item to instruct an importer to import data.
        /// </summary>
        public List<MenuItem> ImportMenuItems
        {
            get
            {
                if(_ImportMenuItems == null)
                {
                    _ImportMenuItems = new List<MenuItem>();
                }
                return _ImportMenuItems;
            }
        }

        public List<MenuItem> PluginMenuItems
        {
            get
            {
                if (_PluginMenuItems == null)
                {
                    _PluginMenuItems = new List<MenuItem>();
                }
                return _PluginMenuItems;
            }
        }

        public bool IsPluginMenuVisible
        {
            get
            {
                return _PluginMenuItems?.Count > 0;
            }
        }

        #endregion // Properties

        #region Events

        public EventHandler<string> FileOpened;
        public EventHandler<string> FileOpenFailed;
        public EventHandler<string> FileSaved;

        #endregion // Events

        #region Commands

        RelayCommand _NewFileCommand;
        RelayCommand _OpenFileCommand;
        RelayCommand _SaveFileCommand;
        RelayCommand _SaveAsFileCommand;
        RelayCommand _CloseFileCommand;
        RelayCommand _ExitApplicationCommand;
        RelayCommand _ShowSettingsWindowCommand;
        RelayCommand _ShowAboutCommand;

        public ICommand NewFileCommand
        {
            get
            {
                if (_NewFileCommand == null)
                {
                    _NewFileCommand = new RelayCommand(NewFileCommand_Executed, NewFileCommand_CanExecute);
                }
                return _NewFileCommand;
            }
        }

        public ICommand OpenFileCommand
        {
            get
            {
                if (_OpenFileCommand == null)
                {
                    _OpenFileCommand = new RelayCommand(OpenFileCommand_Executed, OpenFileCommand_CanExecute);
                }
                return _OpenFileCommand;
            }
        }

        public ICommand SaveFileCommand
        {
            get
            {
                if (_SaveFileCommand == null)
                {
                    _SaveFileCommand = new RelayCommand(SaveFileCommand_Executed, SaveFileCommand_CanExecute);
                }
                return _SaveFileCommand;
            }
        }

        public ICommand SaveAsFileCommand
        {
            get
            {
                if (_SaveAsFileCommand == null)
                {
                    _SaveAsFileCommand = new RelayCommand(SaveAsFileCommand_Executed, SaveAsFileCommand_CanExecute);
                }
                return _SaveAsFileCommand;
            }
        }

        public ICommand CloseFileCommand
        {
            get
            {
                if (_CloseFileCommand == null)
                {
                    _CloseFileCommand = new RelayCommand(CloseFileCommand_Executed, CloseFileCommand_CanExecute);
                }
                return _CloseFileCommand;
            }
        }

        public ICommand ExitApplicationCommand
        {
            get
            {
                if (_ExitApplicationCommand == null)
                {
                    _ExitApplicationCommand = new RelayCommand(ExitApplicationCommand_Executed, ExitApplicationCommand_CanExecute);
                }
                return _ExitApplicationCommand;
            }
        }


        RelayCommand _GenerateControllerCommand;
        public ICommand GenerateControllerCommand
        {
            get
            {
                if (_GenerateControllerCommand == null)
                {
                    _GenerateControllerCommand = new RelayCommand(GenerateControllerCommand_Executed, GenerateControllerCommand_CanExecute);
                }
                return _GenerateControllerCommand;
            }
        }

        RelayCommand _ImportControllerCommand;
        public ICommand ImportControllerCommand
        {
            get
            {
                if (_ImportControllerCommand == null)
                {
                    _ImportControllerCommand = new RelayCommand(ImportControllerCommand_Executed, ImportControllerCommand_CanExecute);
                }
                return _ImportControllerCommand;
            }
        }

        public ICommand ShowSettingsWindowCommand
        {
            get
            {
                if (_ShowSettingsWindowCommand == null)
                {
                    _ShowSettingsWindowCommand = new RelayCommand(ShowSettingsWindowCommand_Executed, null);
                }
                return _ShowSettingsWindowCommand;
            }
        }

        public ICommand ShowAboutCommand
        {
            get
            {
                if (_ShowAboutCommand == null)
                {
                    _ShowAboutCommand = new RelayCommand(ShowAboutCommand_Executed, null);
                }
                return _ShowAboutCommand;
            }
        }

        #endregion // Commands

        #region Command functionality

        private void NewFileCommand_Executed(object prm)
        {
            if (TLCGenControllerDataProvider.Default.NewController())
            {
                string lastfilename = TLCGenControllerDataProvider.Default.ControllerFileName;

                // This allows plugins to reset their content
                ControllerVM.Controller = null;

                TLCGenControllerDataProvider.Default.NewController();
                SetControllerForStatics(TLCGenControllerDataProvider.Default.Controller);
                ControllerVM.Controller = TLCGenControllerDataProvider.Default.Controller;
                Messenger.Default.Send(new ControllerFileNameChangedMessage(TLCGenControllerDataProvider.Default.ControllerFileName, lastfilename));
                Messenger.Default.Send(new UpdateTabsEnabledMessage());
                RaisePropertyChanged("ProgramTitle");
                RaisePropertyChanged("HasController");
            }
        }

        private bool NewFileCommand_CanExecute(object prm)
        {
            return true;
        }

        private void OpenFileCommand_Executed(object prm)
        {
            LoadController();
        }

        private bool OpenFileCommand_CanExecute(object prm)
        {
            return true;
        }

        private void SaveFileCommand_Executed(object prm)
        {
            if (TLCGenControllerDataProvider.Default.SaveController())
            {
                Messenger.Default.Send(new UpdateTabsEnabledMessage());
                GuiActionsManager.SetStatusBarMessage(
                    DateTime.Now.ToLongTimeString() +
                    " - Regeling " + TLCGenControllerDataProvider.Default.Controller.Data.Naam + " opgeslagen");
                FileSaved?.Invoke(this, TLCGenControllerDataProvider.Default.ControllerFileName);
            }
        }

        private bool SaveFileCommand_CanExecute(object prm)
        {
            return TLCGenControllerDataProvider.Default.Controller != null &&
                   TLCGenControllerDataProvider.Default.ControllerHasChanged;
        }

        private void SaveAsFileCommand_Executed(object prm)
        {
            string lastfilename = TLCGenControllerDataProvider.Default.ControllerFileName;
            if(TLCGenControllerDataProvider.Default.SaveControllerAs())
            {
                Messenger.Default.Send(new ControllerFileNameChangedMessage(TLCGenControllerDataProvider.Default.ControllerFileName, lastfilename));
                Messenger.Default.Send(new UpdateTabsEnabledMessage());
                RaisePropertyChanged("ProgramTitle");
                GuiActionsManager.SetStatusBarMessage(
                    DateTime.Now.ToLongTimeString() +
                    " - Regeling " + TLCGenControllerDataProvider.Default.Controller.Data.Naam + " opgeslagen");
                FileSaved?.Invoke(this, TLCGenControllerDataProvider.Default.ControllerFileName);
            }
        }

        private bool SaveAsFileCommand_CanExecute(object prm)
        {
            return TLCGenControllerDataProvider.Default.Controller != null;
        }

        private void CloseFileCommand_Executed(object prm)
        {
            string lastfilename = TLCGenControllerDataProvider.Default.ControllerFileName;
            if(TLCGenControllerDataProvider.Default.CloseController())
            {
                DefaultsProvider.Default.Controller = null;
                Messenger.Default.Send(new ControllerFileNameChangedMessage(TLCGenControllerDataProvider.Default.ControllerFileName, lastfilename));
                RaisePropertyChanged("HasController");
                RaisePropertyChanged("ProgramTitle");
                GuiActionsManager.SetStatusBarMessage("");
            }
        }

        private bool CloseFileCommand_CanExecute(object prm)
        {
            return TLCGenControllerDataProvider.Default.Controller != null;
        }

        private void ExitApplicationCommand_Executed(object prm)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private bool ExitApplicationCommand_CanExecute(object prm)
        {
            return true;
        }

        private void ImportControllerCommand_Executed(object obj)
        {
            if (obj == null)
                throw new NullReferenceException();
	        if (!(obj is ITLCGenImporter imp))
                throw new InvalidCastException();

            // Import into existing controller
            if (TLCGenControllerDataProvider.Default.CheckChanged()) return;
            if (imp.ImportsIntoExisting)
            {
                // Request to process all synchronisation data from matrix to model
                Messenger.Default.Send(new ProcessSynchronisationsRequest());

                // Check data integrity
                var s1 = TLCGenIntegrityChecker.IsConflictMatrixOK(ControllerVM.Controller);
                if (s1 != null)
                {
                    MessageBox.Show("Kan niet importeren:\n\n" + s1, "Error bij importeren: fout in regeling");
                    return;
                }
                // Import to clone of original (so we can discard if wrong)
                var c1 = DeepCloner.DeepClone(ControllerVM.Controller);
                var c2 = imp.ImportController(c1);

                // Do nothing if the importer returned nothing
                if (c2 == null)
                    return;

                // Check data integrity
                c2.Data.GarantieOntruimingsTijden = false;
                s1 = TLCGenIntegrityChecker.IsConflictMatrixOK(c2);
                if (s1 != null)
                {
                    MessageBox.Show("Fout bij importeren:\n\n" + s1, "Error bij importeren: fout in data");
                    return;
                }
                if(c1.Data.GarantieOntruimingsTijden)
                {
                    MessageBox.Show("De bestaande regeling had garantie ontruimingstijden.\nDeze zijn nu uitgeschakeld.", "Garantie ontruimingstijden uitrgeschakeld");
                }
                SetController(c2);
                ControllerVM.ReloadController();
                GuiActionsManager.SetStatusBarMessage(
                    DateTime.Now.ToLongTimeString() +
                    " - Data in regeling " + TLCGenControllerDataProvider.Default.Controller.Data.Naam + " geïmporteerd");
            }
            // Import as new controller
            else
            {
                var c1 = imp.ImportController();

                // Do nothing if the importer returned nothing
                if (c1 == null)
                    return;

                // Check data integrity
                var s1 = TLCGenIntegrityChecker.IsConflictMatrixOK(c1);
                if (s1 != null)
                {
                    MessageBox.Show("Fout bij importeren:\n\n" + s1, "Error bij importeren: fout in data");
                    return;
                }
                TLCGenControllerDataProvider.Default.CloseController();
                DefaultsProvider.Default.SetDefaultsOnModel(c1.Data);
                DefaultsProvider.Default.SetDefaultsOnModel(c1.OVData);
                SetController(c1);
                ControllerVM.ReloadController();
                GuiActionsManager.SetStatusBarMessage(
                    DateTime.Now.ToLongTimeString() +
                    " - Regeling geïmporteerd");
            }
            Messenger.Default.Send(new UpdateTabsEnabledMessage());
            RaisePropertyChanged("HasController");
        }

        private bool GenerateControllerCommand_CanExecute(object obj)
        {
            return SelectedGenerator != null && SelectedGenerator.Generator != null && SelectedGenerator.Generator.CanGenerateController();
        }

        private void GenerateControllerCommand_Executed(object obj)
        {
            // Request to process all synchronisation data from matrix to model
            Messenger.Default.Send(new ProcessSynchronisationsRequest());

            string s = TLCGenIntegrityChecker.IsControllerDataOK(TLCGenControllerDataProvider.Default.Controller);
            if (s == null)
            {
                SelectedGenerator.Generator.GenerateController();
                MessengerInstance.Send(new ControllerCodeGeneratedMessage());
            }
            else
            {
                System.Windows.MessageBox.Show(s + "\n\nKan regeling niet genereren.", "Error bij genereren: fout in regeling");
            }
        }

        private bool ImportControllerCommand_CanExecute(object obj)
        {
            if (obj == null)
                return false;

	        if (!(obj is ITLCGenImporter imp))
                throw new InvalidCastException();

            if (imp.ImportsIntoExisting)
                return TLCGenControllerDataProvider.Default.Controller != null;

            return true;
        }

        private void ShowSettingsWindowCommand_Executed(object obj)
        {
            Settings.Views.TLCGenSettingsWindow settingswin = new Settings.Views.TLCGenSettingsWindow();
            settingswin.DataContext = new TLCGenSettingsViewModel();
            settingswin.ShowDialog();
        }

        private void ShowAboutCommand_Executed(object obj)
        {
            AboutWindow about = new AboutWindow();
            about.ShowDialog();
        }

        #endregion // Command functionality

        #region Private methods

        private void SetControllerForStatics(ControllerModel c)
        {
            DefaultsProvider.Default.Controller = c;
            TLCGenControllerModifier.Default.Controller = c;
            TLCGenModelManager.Default.Controller = c;
        }

        /// <summary>
        /// Called before the application is closed. This allows to save data and settings
        /// </summary>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (TLCGenControllerDataProvider.Default.CheckChanged())
            {
                e.Cancel = true;
            }
            else
            {
                foreach (var pl in _ApplicationParts)
                {
                    var setpl = pl.Item2 as ITLCGenHasSettings;
                    if (setpl != null)
                    {
                        setpl.SaveSettings();
                    }
                }

                SaveGeneratorControllerSettingsToModel();
                SettingsProvider.Default.SaveApplicationSettings();
                DefaultsProvider.Default.SaveSettings();
                TemplatesProvider.Default.SaveSettings();
            }
        }

		// TODO: Make this generic, just like how settings are loaded
        private void SaveGeneratorControllerSettingsToModel()
        {
            //SettingsVM.Settings.CustomData.AddinSettings.Clear();
            foreach(IGeneratorViewModel genvm in Generators)
            {
                ITLCGenGenerator gen = genvm.Generator;
                AddinSettingsModel gendata = new AddinSettingsModel();
                gendata.Naam = gen.GetGeneratorName();
                Type t = gen.GetType();
                // From the Generator, real all properties attributed with [TLCGenGeneratorSetting]
                var dllprops = t.GetProperties().Where(
                    prop => Attribute.IsDefined(prop, typeof(TLCGenCustomSettingAttribute)));
                foreach (PropertyInfo propertyInfo in dllprops)
                {
                    if (propertyInfo.CanRead)
                    {
                        TLCGenCustomSettingAttribute propattr = (TLCGenCustomSettingAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(TLCGenCustomSettingAttribute));
                        if (propattr.SettingType == TLCGenCustomSettingAttribute.SettingTypeEnum.Application)
                        {
                            try
                            {

                                string name = propertyInfo.Name;
                                var v = propertyInfo.GetValue(gen);
                                if (v != null)
                                {
                                    string value = v.ToString();
                                    AddinSettingsPropertyModel prop = new AddinSettingsPropertyModel();
                                    prop.Naam = name;
                                    prop.Setting = value;
                                    gendata.Properties.Add(prop);
                                }
                            }
                            catch
                            {

                            }
                        }
                    }
                }
                //SettingsVM.Settings.CustomData.AddinSettings.Add(gendata);
            }
        }

        #endregion // Private methods

        #region Public methods

        /// <summary>
        /// Updates the ViewModel structure, causing the View to reload all bound properties.
        /// </summary>
        public void UpdateController()
        {
            ControllerVM.ReloadController();
            RaisePropertyChanged("");
        }

        /// <summary>
        /// Used to set the loaded Controller to a the parsed instance of ControllerModel. The method also 
        /// checks for changes, sets program title, and updates bound properties.
        /// </summary>
        /// <param name="cm">The instance of ControllerModel to be loaded.</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool SetController(ControllerModel cm)
        {
            if (TLCGenControllerDataProvider.Default.SetController(cm))
            {
                string filename = TLCGenControllerDataProvider.Default.ControllerFileName;
                SetControllerForStatics(cm);
                ControllerVM.Controller = cm;
                return true;
            }
            return false;
        }

        public bool LoadController(string filename = null)
        {
            if (TLCGenControllerDataProvider.Default.OpenController(filename))
            {
                string lastfilename = TLCGenControllerDataProvider.Default.ControllerFileName;
                SetControllerForStatics(TLCGenControllerDataProvider.Default.Controller);
                ControllerVM.Controller = TLCGenControllerDataProvider.Default.Controller;
                Messenger.Default.Send(new ControllerFileNameChangedMessage(TLCGenControllerDataProvider.Default.ControllerFileName, lastfilename));
                Messenger.Default.Send(new UpdateTabsEnabledMessage());
                RaisePropertyChanged("ProgramTitle");
                RaisePropertyChanged("HasController");
                FileOpened?.Invoke(this, TLCGenControllerDataProvider.Default.ControllerFileName);
                var jumpTask = new JumpTask
                {
                    Title = Path.GetFileName(TLCGenControllerDataProvider.Default.ControllerFileName),
                    Arguments = TLCGenControllerDataProvider.Default.ControllerFileName
                };
                JumpList.AddToRecentCategory(jumpTask);
                return true;
            }
            if (filename != null) FileOpenFailed?.Invoke(this, filename);
            return false;
        }

        public void CheckCommandLineArgs()
        {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length > 1 && args[1].ToLower().EndsWith(".tlc") && System.IO.File.Exists(args[1]))
            {
                LoadController(args[1]);
            }
        }

        #endregion // Public methods

        #region TLCGen Messaging

        private void OnPrepareForGenerationRequest(Messaging.Requests.PrepareForGenerationRequest request)
        {
            var procreq = new Messaging.Requests.ProcessSynchronisationsRequest();
            MessengerInstance.Send(procreq);
        }

        private void OnControllerCodeGenerated(ControllerCodeGeneratedMessage message)
        {
            GuiActionsManager.SetStatusBarMessage(
                DateTime.Now.ToLongTimeString() +
                " - Regeling " + TLCGenControllerDataProvider.Default.Controller.Data.Naam + " gegenereerd");
        }

        private void OnControllerProjectGenerated(ControllerProjectGeneratedMessage message)
        {
            GuiActionsManager.SetStatusBarMessage(
                DateTime.Now.ToLongTimeString() +
                " - Project voor regeling " + TLCGenControllerDataProvider.Default.Controller.Data.Naam + " gegenereerd");
        }

	    private void OnControllerFileNameChanged(ControllerFileNameChangedMessage message)
	    {
			RaisePropertyChanged(nameof(ProgramTitle));
	    }

        #endregion // TLCGen Messaging

        #region Constructor

        public MainWindowViewModel()
        {
            var tmpCurDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            try
            {
                GuiActionsManager.SetStatusBarMessage = (string text) =>
                {
                    StatusBarVM.StatusText = text;
                };

                MessengerInstance.Register(this, new Action<Messaging.Requests.PrepareForGenerationRequest>(OnPrepareForGenerationRequest));
                MessengerInstance.Register(this, new Action<ControllerCodeGeneratedMessage>(OnControllerCodeGenerated));
                MessengerInstance.Register(this, new Action<ControllerProjectGeneratedMessage>(OnControllerProjectGenerated));
                MessengerInstance.Register(this, new Action<ControllerFileNameChangedMessage>(OnControllerFileNameChanged));

                // Load application settings and defaults
	            TLCGenSplashScreenHelper.ShowText("Laden instellingen en defaults...");
                SettingsProvider.Default.LoadApplicationSettings();
                DefaultsProvider.Default.LoadSettings();
                TemplatesProvider.Default.LoadSettings();

	            TLCGenModelManager.Default.InjectDefaultAction(x => DefaultsProvider.Default.SetDefaultsOnModel(x));
	            TLCGenControllerDataProvider.Default.InjectDefaultAction(x => DefaultsProvider.Default.SetDefaultsOnModel(x));

                // Load available applicationparts and plugins
	            var assms = Assembly.GetExecutingAssembly();
	            var types = from t in assms.GetTypes()
		            where t.IsClass && t.Namespace == "TLCGen.ViewModels"
		            select t;
	            TLCGenSplashScreenHelper.ShowText("Laden applicatie onderdelen...");
                TLCGenPluginManager.Default.LoadApplicationParts(types.ToList());
	            TLCGenSplashScreenHelper.ShowText("Laden plugins...");
                TLCGenPluginManager.Default.LoadPlugins(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Plugins\\"));

                // Instantiate all parts
                _ApplicationParts = new List<Tuple<TLCGenPluginElems, ITLCGenPlugin>>();
                var parts = TLCGenPluginManager.Default.ApplicationParts.Concat(TLCGenPluginManager.Default.ApplicationPlugins);
                foreach (var part in parts)
                {
                    ITLCGenPlugin instpl = part.Item2;
					TLCGenSplashScreenHelper.ShowText($"Laden plugin {instpl.GetPluginName()}...");
                    var flags = Enum.GetValues(typeof(TLCGenPluginElems));
                    foreach (TLCGenPluginElems elem in flags)
                    {
                        if ((part.Item1 & elem) == elem)
                        {
                            switch (elem)
                            {
                                case TLCGenPluginElems.Generator:
                                    Generators.Add(new IGeneratorViewModel(instpl as ITLCGenGenerator));
                                    break;
                                case TLCGenPluginElems.HasSettings:
                                    ((ITLCGenHasSettings)instpl).LoadSettings();
                                    break;
                                case TLCGenPluginElems.Importer:
                                    MenuItem mi = new MenuItem();
                                    mi.Header = instpl.GetPluginName();
                                    mi.Command = ImportControllerCommand;
                                    mi.CommandParameter = instpl;
                                    ImportMenuItems.Add(mi);
                                    break;
                                case TLCGenPluginElems.IOElementProvider:
                                    break;
                                case TLCGenPluginElems.MenuControl:
                                    PluginMenuItems.Add(((ITLCGenMenuItem)instpl).Menu);
                                    break;
                                case TLCGenPluginElems.TabControl:
                                    break;
                                case TLCGenPluginElems.ToolBarControl:
                                    break;
                                case TLCGenPluginElems.XMLNodeWriter:
                                    break;
                                case TLCGenPluginElems.PlugMessaging:
                                    (instpl as ITLCGenPlugMessaging).UpdateTLCGenMessaging();
                                    break;
								case TLCGenPluginElems.Switcher:
									(instpl as ITLCGenSwitcher).ControllerSet += (sender, model) => { SetController(model); };
									(instpl as ITLCGenSwitcher).FileNameSet += (sender, model) =>
										{
											TLCGenControllerDataProvider.Default.ControllerFileName = model;
										};
									break;
                            }
                        }
                        TLCGenPluginManager.LoadAddinSettings(instpl, part.Item2.GetType(), SettingsProvider.Default.Settings.CustomData);
                    }
                    _ApplicationParts.Add(new Tuple<TLCGenPluginElems, ITLCGenPlugin>(part.Item1, instpl as ITLCGenPlugin));
                }
                if (Generators.Count > 0) SelectedGenerator = Generators[0];

                // Construct the ViewModel
                ControllerVM = new ControllerViewModel();

                if (!DesignMode.IsInDesignMode)
                {
                    if (Application.Current != null && Application.Current.MainWindow != null)
                        Application.Current.MainWindow.Closing += new CancelEventHandler(MainWindow_Closing);
                }

#if !DEBUG
                Application.Current.DispatcherUnhandledException += (o, e) =>
                {
                    string message = "Er is een onverwachte fout opgetreden.\n\n";
                    if (TLCGenControllerDataProvider.Default.Controller != null)
                    {
                        try
                        {
                            string t = TLCGenControllerDataProvider.Default.ControllerFileName;
                            if (string.IsNullOrWhiteSpace(TLCGenControllerDataProvider.Default.ControllerFileName))
                            {
                                TLCGenControllerDataProvider.Default.ControllerFileName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TLC_recoverysave.tlc");
                            }
                            TLCGenControllerDataProvider.Default.ControllerFileName =
                            System.IO.Path.Combine(
                                System.IO.Path.GetDirectoryName(TLCGenControllerDataProvider.Default.ControllerFileName),
                                DateTime.Now.ToString("yyyyMMdd-HHmmss-", System.Globalization.CultureInfo.InvariantCulture) +
                                System.IO.Path.GetFileName(TLCGenControllerDataProvider.Default.ControllerFileName));
                            TLCGenControllerDataProvider.Default.SaveController();
                            message += "De huidige regeling is hier opgeslagen:\n" + TLCGenControllerDataProvider.Default.ControllerFileName + "\n\n";
                            if (t != null)
                            {
                                TLCGenControllerDataProvider.Default.ControllerFileName = t;
                            }
                        }
                        catch
                        {

                        }
                    }
                    message += "Gelieve dit probleem inclusief onderstaande details doorgeven aan de ontwikkelaar:\n\n";
                    var win = new TLCGen.Dialogs.UnhandledExceptionWindow();
                    win.DialogTitle = "Onverwachte fout in TLCGen";
                    win.DialogMessage = message;
                    win.DialogExpceptionText = e.Exception.ToString();
                    win.ShowDialog();
                };
#endif
            }
            catch (Exception e)
            {
	            TLCGenSplashScreenHelper.Hide();

                string message = "Er is een onverwachte fout opgetreden.\n\n";
                message += "Gelieve dit probleem inclusief onderstaande details doorgeven aan de ontwikkelaar:\n\n";
                var win = new TLCGen.Dialogs.UnhandledExceptionWindow();
                win.DialogTitle = "Onverwachte fout in TLCGen";
                win.DialogMessage = message;
                win.DialogExpceptionText = e.ToString();
                win.ShowDialog();
            }

            Directory.SetCurrentDirectory(tmpCurDir);

#if !DEBUG1
            // Find out if there is a newer version available via Wordpress REST API
            Task.Run(() =>
            {
				// clean potential old data
	            var key = Registry.CurrentUser.OpenSubKey("Software", true);
	            var sk1 = key?.OpenSubKey("CodingConnected e.U.", true);
	            var sk2 = sk1?.OpenSubKey("TLCGen", true);
	            var tempFile = (string) sk2?.GetValue("TempInstallFile", null);
	            if (tempFile != null)
	            {
		            if (File.Exists(tempFile))
		            {
			            File.Delete(tempFile);
		            }
					sk2?.DeleteValue("TempInstallFile");
	            }
			
	            var webRequest = WebRequest.Create(@"https://codingconnected.eu/wp-json/wp/v2/pages/1105");
	            webRequest.UseDefaultCredentials = true;
	            using (var response = webRequest.GetResponse())
				using (var content = response.GetResponseStream())
				if(content != null) {
					using (var reader = new StreamReader(content))
					{
						var strContent = reader.ReadToEnd().Replace("\n", "");
						var jsonDeserializer = new JavaScriptSerializer();
						var deserializedJson = jsonDeserializer.Deserialize<dynamic>(strContent);
						if (deserializedJson == null) return;
						var contentData = deserializedJson["content"];
						if (contentData == null) return;
						var renderedData = contentData["rendered"];
						if (renderedData == null) return;
						var data = renderedData as string;
						if (data == null) return;
						var all = data.Split('\r');
						var tlcgenVer = all.FirstOrDefault(v => v.StartsWith("TLCGen="));
						if (tlcgenVer == null) return;
						var oldvers = Assembly.GetEntryAssembly().GetName().Version.ToString().Split('.');
						var newvers = tlcgenVer.Replace("TLCGen=", "").Split('.');
                        bool newer = false;
						if (oldvers.Length > 0 && oldvers.Length == newvers.Length)
						{
							for (int i = 0; i < newvers.Length; i++)
							{
                                    var o = int.Parse(oldvers[i]);
                                    var n = int.Parse(newvers[i]);
                                    if(o > n)
                                    {
                                        break;
                                    }
                                    if(n > o)
                                    {
                                        newer = true;
                                        break;
                                    }
							}
						}
						if (newer)
						{
							DispatcherHelper.CheckBeginInvokeOnUI(() =>
							{
								var w = new NewVersionAvailableWindow(tlcgenVer.Replace("TLCGen=", ""));
								w.ShowDialog();
							});
						}
					}
				}
            });
#endif
		}
#endregion // Constructor

        #region IDropTarget

        public void DragOver(IDropInfo dropInfo)
        {
	        if (!(dropInfo.Data is DataObject d) || !d.ContainsFileDropList()) return;
            var files = d.GetFileDropList();
            if (files.Count == 1 && files[0].ToLower().EndsWith(".tlc") || files[0].ToLower().EndsWith(".tlcgz"))
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
	        if (!(dropInfo.Data is DataObject d) || !d.ContainsFileDropList()) return;
            {
                var files = d.GetFileDropList();
                if (files.Count == 1 && files[0].ToLower().EndsWith(".tlc") || files[0].ToLower().EndsWith(".tlcgz"))
                {
                    LoadController(files[0]);
                }
            }
        }

        #endregion // IDropTarget
    }
}
