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
using System.Windows.Media;
using System.Windows;

namespace WDE.YittenMongoose
{
	public class YittenMongooseVM : INotifyPropertyChanged
	{
		public ObservableCollection<ITrackVM> Tracks { get; private set; }
		public MultiTrackVM MultiTrackVM { get; private set; }
		public double Scale { get; private set; }
		public TextFormattingMode TextFormattingMode { get; private set; }

		public int Bar 
		{ 
			get { return barParameter.GetValue(0); }
			set
			{
				barParameter.SetValue(0, value);
				PropertyChanged.Raise(this, "Bar");
			}
		}

		int zoomLevel = -1;
		public int ZoomLevel
		{
			get
			{
				return zoomLevel;
			}
			set
			{
				if (zoomLevel != value)
				{
					zoomLevel = value;
					Scale = Math.Pow(2, ((zoomLevel - 5) / 5.0));
					TextFormattingMode = Scale == 1.0 ? TextFormattingMode.Display : TextFormattingMode.Ideal;

					PropertyChanged.Raise(this, "ZoomLevel");
					PropertyChanged.Raise(this, "Scale");
					PropertyChanged.Raise(this, "TextFormattingMode");
				}
			}
		}

		int selectedPadSet = 0;
		public int SelectedPadSet
		{
			get => selectedPadSet;
			set
			{
				selectedPadSet = value;
				YittenMongooseGUI.padController.PadSet(selectedPadSet);
                PropertyChanged.Raise(this, "SelectedPadSet");
            }
		}

		IMachine machine;
		ISong song;
		private YittenMongooseGUI YittenMongooseGUI;
		IParameter barParameter;

        bool isHelpVisible = false;
        public bool IsHelpVisible
        {
            get { return isHelpVisible; }
            set
            {
                isHelpVisible = value;
                PropertyChanged.Raise(this, "IsHelpVisible");
                YittenMongooseGUI.helpControl.Visibility = isHelpVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public YittenMongooseVM(IMachine machine, YittenMongooseGUI YittenMongooseGUI)
		{
			this.machine = machine;
			this.song = machine.Graph.Buzz.Song;
			this.YittenMongooseGUI = YittenMongooseGUI;

			barParameter = machine.GetParameter("Bar");
			barParameter.SubscribeEvents(0, BarChanged, null);

			song.SequenceAdded += song_SequenceAdded;
			song.SequenceChanged += song_SequenceChanged;
			song.SequenceRemoved += song_SequenceRemoved;

			Tracks = new ObservableCollection<ITrackVM>();
			Tracks.Add(MultiTrackVM = new MultiTrackVM(this));
			for (int i = 0; i < song.Sequences.Count; i++) song_SequenceAdded(i);

			ZoomLevel = 5;

			YittenMongooseGUI.btPadLeft.Click += (sender, e) =>
			{
                if (selectedPadSet > 0)
					SelectedPadSet = selectedPadSet - 1;
            };

            YittenMongooseGUI.btPadRight.Click += (sender, e) =>
            {
                if (selectedPadSet < YittenMongooseMachine.NUM_PAD_SETS - 1)
                    SelectedPadSet = selectedPadSet + 1;
            };
        }

		public void Release()
		{
			for (int i = song.Sequences.Count - 1; i >= 0; i--) song_SequenceRemoved(i);

			barParameter.UnsubscribeEvents(0, BarChanged, null);

			song.SequenceAdded -= song_SequenceAdded;
			song.SequenceChanged -= song_SequenceChanged;
			song.SequenceRemoved -= song_SequenceRemoved;
		}

		void BarChanged(IParameter p, int track)
		{
			PropertyChanged.Raise(this, "Bar");
		}

		void song_SequenceAdded(int index)
		{
			Tracks.Add(new TrackVM(this, song.Sequences[index]));
		}

		void song_SequenceChanged(int index)
		{
			Tracks[index + 1].Release();
			Tracks[index + 1] = new TrackVM(this, song.Sequences[index]);
		}

		void song_SequenceRemoved(int index)
		{
			Tracks[index + 1].Release();
			Tracks.RemoveAt(index + 1);
		}

		public void Update()
		{
			foreach (var t in Tracks)
				t.Update();
		}

		internal void PlayAllPatterns(string pattern, bool play)
		{
			Tracks.Run((TrackVM t) => t.SelectPattern(pattern, play));
		}

		public event PropertyChangedEventHandler PropertyChanged;

	}
}
