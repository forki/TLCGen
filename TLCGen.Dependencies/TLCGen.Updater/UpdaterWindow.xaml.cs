﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TLCGen.Updater
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class UpdaterWindow : Window
	{
		public UpdaterWindow()
		{
			InitializeComponent();

			var updaterVm = new UpdaterViewModel();
			this.Closed += (sender, args) =>
			{
				updaterVm.CleanUp();
			};
			Switcher.pageSwitcher = this;
			Switcher.Switch(new UpdaterHome(), updaterVm);
		}

		public void Navigate(UserControl nextPage)
		{
			this.Content = nextPage;
		}

		public void Navigate(UserControl nextPage, object state)
		{
			this.Content = nextPage;

			if (nextPage is ISwitchable s)
				s.UtilizeState(state);
			else
				throw new ArgumentException("NextPage is not ISwitchable! " + nextPage.Name);
		}
	}
}
