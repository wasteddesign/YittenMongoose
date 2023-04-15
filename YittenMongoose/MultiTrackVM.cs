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
	public class MultiTrackVM : ITrackVM, INotifyPropertyChanged
	{
		YittenMongooseVM YittenMongooseVM;
		CountSet<string> patternCounts = new CountSet<string>();

		public ObservableCollection<MultiPatternVM> Patterns { get; private set; }
		
		public MultiTrackVM(YittenMongooseVM YittenMongooseVM)
		{
			this.YittenMongooseVM = YittenMongooseVM;
			Patterns = new ObservableCollection<MultiPatternVM>();
		}

		public void Release()
		{
		}

		public void Update()
		{
		}

		internal void PatternAdded(string pattern)
		{
			if (patternCounts.Increase(pattern) == 2)
			{
				Patterns.Add(new MultiPatternVM(this, pattern));
			}
		}

		internal void PatternRemoved(string pattern)
		{
			if (patternCounts.Decrease(pattern) == 1)
			{
				var vm = Patterns.First(p => p.Name == pattern);
				Patterns.Remove(vm);
			}
		}

		internal void SelectPattern(MultiPatternVM pvm)
		{
			if (!pvm.IsSelected)
			{
				YittenMongooseVM.PlayAllPatterns(pvm.Name, true);
				pvm.IsSelected = true;
			}
			else
			{
				YittenMongooseVM.PlayAllPatterns(pvm.Name, false);
				pvm.IsSelected = false;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
