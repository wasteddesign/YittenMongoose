using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using BuzzGUI.Interfaces;
using BuzzGUI.Common;
using BuzzGUI.Common.InterfaceExtensions;
using System.Windows.Input;

namespace WDE.YittenMongoose
{
	public class MultiPatternVM : INotifyPropertyChanged
	{
		public string Name { get; private set; }
		public ICommand MouseDownCommand { get; private set; }
		MultiTrackVM trackVM;

		bool isSelected;
		public bool IsSelected
		{
			get { return isSelected; }
			set
			{
				if (value != isSelected)
				{
					isSelected = value;
					PropertyChanged.Raise(this, "IsSelected");
				}
			}
		}

		public MultiPatternVM(MultiTrackVM trackVM, string name)
		{
			this.trackVM = trackVM;
			this.Name = name;

			MouseDownCommand = new SimpleCommand()
			{
				CanExecuteDelegate = x => true,
				ExecuteDelegate = x => 
				{
					var e = x as MouseButtonEventArgs;

					if (e.ChangedButton == MouseButton.Left)
						trackVM.SelectPattern(this);
				}
			};
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
