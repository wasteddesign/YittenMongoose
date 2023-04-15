using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BuzzGUI.Interfaces;
using BuzzGUI.Common;
using System.ComponentModel;

namespace WDE.YittenMongoose
{
	[MachineGUIFactoryDecl(PreferWindowedGUI = true, IsGUIResizable = true, UseThemeStyles = true)]
	public class MachineGUIFactory : IMachineGUIFactory 
	{ 
		public IMachineGUI CreateGUI(IMachineGUIHost host) { return new YittenMongooseGUI(); }
	}

	/// <summary>
	/// Interaction logic for YittenMongooseGUI.xaml
	/// </summary>
	public partial class YittenMongooseGUI : UserControl, IMachineGUI, INotifyPropertyChanged
	{
		IMachine machine;
		YittenMongooseMachine YittenMongooseMachine;
		YittenMongooseVM viewModel;

        public event PropertyChangedEventHandler PropertyChanged;

        public IMachine Machine
		{
			get { return machine; }
			set
			{
				if (machine != null)
				{
					viewModel.Release();
					this.DataContext = viewModel = null;
				}

				machine = value;

				if (machine != null)
				{
					YittenMongooseMachine = (YittenMongooseMachine)machine.ManagedMachine;
					this.DataContext = viewModel = new YittenMongooseVM(machine, this);
					padController.Machine = YittenMongooseMachine;
                }
			}
		}

		public YittenMongooseGUI()
		{
            InitializeComponent();

            var res = YittenMongooseMachine.GetBuzzThemeResources("YittenMongoose.xaml");
            this.Resources.MergedDictionaries.Add(res);

            new MachineGUIUpdateTimer(this, () =>
			{
				if (viewModel != null)
					viewModel.Update();
			});
		}
	}
}
