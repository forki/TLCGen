﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using TLCGen.Messaging.Messages;
using TLCGen.Models;
using TLCGen.Models.Enumerations;

namespace TLCGen.ViewModels
{
	public class GelijkstartViewModel : ViewModelBase
	{
        #region Fields

		private readonly GelijkstartModel _gelijkstart;

		#endregion // Fields

        #region Properties

		public bool DeelConflict
		{
			get => _gelijkstart.DeelConflict;
			set
			{
				_gelijkstart.DeelConflict = value;
				RaisePropertyChanged();
			}
		}

		public int OntruimingstijdFaseVan
		{
			get => _gelijkstart.GelijkstartOntruimingstijdFaseVan;
			set
			{
				_gelijkstart.GelijkstartOntruimingstijdFaseVan = value;
				RaisePropertyChanged();
			}
		}

		public int OntruimingstijdFaseNaar
		{
			get => _gelijkstart.GelijkstartOntruimingstijdFaseNaar;
			set
			{
				_gelijkstart.GelijkstartOntruimingstijdFaseNaar = value;
				RaisePropertyChanged();
			}
		}

		public string Comment1 =>
			$"Fictive ontruimingstijd van {_gelijkstart.FaseVan} naar {_gelijkstart.FaseNaar}";

		public string Comment2 =>
			$"Fictive ontruimingstijd van {_gelijkstart.FaseNaar} naar {_gelijkstart.FaseVan}";

        #endregion // Properties

        #region Private methods

        #endregion // Private methods

        #region Collection changed

        #endregion // Collection changed

        #region TLCGen Events
      
        #endregion // TLCGen Events

        #region Constructor

        public GelijkstartViewModel(GelijkstartModel gs)
        {
            _gelijkstart = gs;
        }

        #endregion // Constructor
	}
}