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
	public class PatternVM : INotifyPropertyChanged
	{
		TrackVM trackVM;
		public IPattern Pattern { get; private set; }
		public string Name { get; private set; }
		public ICommand MouseDownCommand { get; private set; }

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

		bool isPlaying;
		public bool IsPlaying
		{
			get { return isPlaying; }
			set
			{
				if (value != isPlaying)
				{
					isPlaying = value;
					PropertyChanged.Raise(this, "IsPlaying");
				}
			}
		}

		double playPosition;
		public double PlayPosition
		{
			get { return playPosition; }
			set
			{
				if (value != playPosition)
				{
					playPosition = value;
					PropertyChanged.Raise(this, "PlayPosition");
				}
			}
		}

		public PatternVM(TrackVM trackVM, IPattern pattern)
		{
			this.trackVM = trackVM;
			this.Pattern = pattern;
			Name = pattern.Name;

			Pattern.PropertyChanged += Pattern_PropertyChanged;

			MouseDownCommand = new SimpleCommand()
			{
				CanExecuteDelegate = x => true,
				ExecuteDelegate = x =>
				{
					var e = x as MouseButtonEventArgs;

					if (e.ChangedButton == MouseButton.Left)
					{
						trackVM.SelectPattern(this);
					}
					else
					{
						var song = (ISong)pattern.Machine.Graph;
						song.Buzz.SetPatternEditorPattern(Pattern);
						song.Buzz.ActivatePatternEditor();
					}
				}
			};
		}

		public void Release()
		{
			Pattern.PropertyChanged -= Pattern_PropertyChanged;
		}

		void Pattern_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Name":
					trackVM.YittenMongooseVM.MultiTrackVM.PatternRemoved(Name);
					Name = Pattern.Name;
					trackVM.YittenMongooseVM.MultiTrackVM.PatternAdded(Name);
					PropertyChanged.Raise(this, "Name");
					break;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
