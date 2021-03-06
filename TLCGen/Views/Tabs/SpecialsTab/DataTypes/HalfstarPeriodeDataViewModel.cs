﻿using GalaSoft.MvvmLight;
using TLCGen.Helpers;
using TLCGen.Models;

namespace TLCGen.ViewModels
{
	public class HalfstarPeriodeDataViewModel : ViewModelBase, IViewModelWithItem
	{
		#region Properties

		public HalfstarPeriodeDataModel PeriodeData { get; }

		public string Periode
		{
			get => PeriodeData.Periode;
		}

		public string Signaalplan
		{
			get => PeriodeData.Signaalplan;
			set
			{
				PeriodeData.Signaalplan = value;
				RaisePropertyChanged();
			}
		}

		public bool VARegelen
		{
			get => PeriodeData.VARegelen;
			set
			{
				PeriodeData.VARegelen = value;
				RaisePropertyChanged();
			}
		}

		#endregion // Properties

		#region IViewModelWithItem

		public object GetItem()
		{
			return PeriodeData;
		}

		#endregion // IViewModelWithItem

		#region Constructor

		public HalfstarPeriodeDataViewModel(HalfstarPeriodeDataModel periodeData)
		{
			PeriodeData = periodeData;
		}

		#endregion // Constructor
	}
}