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
	public class TrackVM : ITrackVM, INotifyPropertyChanged
	{
		public ISequence Sequence { get; private set; }
		public string Name { get { return Sequence.Machine.Name; } }
		public ObservableCollection<PatternVM> Patterns { get; private set; }
		public YittenMongooseVM YittenMongooseVM { get; private set; }
		public ICommand MouseDownCommand { get; private set; }
		ISong song;

		public TrackVM(YittenMongooseVM YittenMongooseVM, ISequence sequence)
		{
			this.YittenMongooseVM = YittenMongooseVM;
			this.Sequence = sequence;
			song = (ISong)Sequence.Machine.Graph;

			Sequence.Machine.PatternAdded += Machine_PatternAdded;
			Sequence.Machine.PatternRemoved += Machine_PatternRemoved;
			Sequence.Machine.PropertyChanged += Machine_PropertyChanged;

            Patterns = new ObservableCollection<PatternVM>();
			foreach (var p in sequence.Machine.Patterns) Machine_PatternAdded(p);

			MouseDownCommand = new SimpleCommand()
			{
				CanExecuteDelegate = x => true,
				ExecuteDelegate = x =>
				{
					var e = x as MouseButtonEventArgs;

					if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
						Sequence.Machine.DoubleClick();
				}
			};

		}

		public void Release()
		{
			foreach (var p in Sequence.Machine.Patterns) Machine_PatternRemoved(p);

			Sequence.Machine.PatternAdded -= Machine_PatternAdded;
			Sequence.Machine.PatternRemoved -= Machine_PatternRemoved;
			Sequence.Machine.PropertyChanged -= Machine_PropertyChanged;
		}

		void Machine_PatternAdded(IPattern pattern)
		{
			Patterns.Add(new PatternVM(this, pattern));
			YittenMongooseVM.MultiTrackVM.PatternAdded(pattern.Name);
		}

		void Machine_PatternRemoved(IPattern pattern)
		{
			var vm = Patterns.First(p => p.Pattern == pattern);
			vm.Release();
			Patterns.Remove(vm);
			YittenMongooseVM.MultiTrackVM.PatternRemoved(pattern.Name);
		}

		void Machine_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Name":
					PropertyChanged.Raise(this, "Name");
					break;
			}
		}

		public void Update()
		{
			var pp = Sequence.PlayingPattern;

			foreach (var p in Patterns)
			{
				if (p.Pattern == pp)
				{
					p.IsPlaying = true;
					p.PlayPosition = (double)(1 + Sequence.PlayingPatternPosition) / p.Pattern.Length;
				}
				else
				{
					p.IsPlaying = false;
					p.PlayPosition = 0;
				}

			}

		}

		internal void SelectPattern(PatternVM pvm)
		{
			if (!pvm.IsSelected)
			{
				foreach (var p in Patterns)
				{
					if (p != pvm)
						p.IsSelected = false;
				}

				var time = song.LoopStart + ((song.PlayPosition - song.LoopStart + YittenMongooseVM.Bar) / YittenMongooseVM.Bar * YittenMongooseVM.Bar % (song.LoopEnd - song.LoopStart));
				Sequence.TriggerEvent(time, new SequenceEvent(SequenceEventType.PlayPattern, pvm.Pattern), true);

				pvm.IsSelected = true;
			}
			else
			{	
				Sequence.TriggerEvent(0, null, false);
                pvm.IsSelected = false;
			}
		}

		internal void SelectPattern(string pattern, bool select)
		{
			var pvm = Patterns.FirstOrDefault(p => p.Name == pattern);
			if (pvm == null) return;

			if (select)
			{
				if (!pvm.IsSelected)
					SelectPattern(pvm);
			}
			else
			{
				if (pvm.IsSelected)
					SelectPattern(pvm);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
