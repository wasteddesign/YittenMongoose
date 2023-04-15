using BuzzGUI.Common;
using BuzzGUI.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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

namespace WDE.YittenMongoose
{
    /// <summary>
    /// Interaction logic for PadAssignWindow.xaml
    /// </summary>
    public partial class PadAssignWindow : Window, INotifyPropertyChanged
    {
        private List<IMachine> machines;
        public List<IMachine> Machines { get => machines; set {
                machines = value;
                PropertyChanged.Raise(this, "Machines");
            }
        }

        private IMachine selectedMachine;

        public IMachine SelectedMachine
        {
            get => selectedMachine;
            set
            {
                selectedMachine = value;
                PropertyChanged.Raise(this, "SelectedMachine");
            }
        }

        private List<string> midiNotes;

        public List<string> MidiNotes { get => midiNotes; set
            {
                midiNotes = value;
                PropertyChanged.Raise(this, "MidiNotes");
            }
        }

        private string selectedNote;

        public string SelectedNote
        {
            get => selectedNote; set
            {
                selectedNote = value;
                PropertyChanged.Raise(this, "SelectedNote");
            }
        }

        private int volume;

        public int Volume
        {
            get => volume; set
            {
                volume = value;
                nudVol.Value = value;
            }
        }

        public bool Clear { get; private set; }

        public PadAssignWindow()
        {
            var res = YittenMongooseMachine.GetBuzzThemeResources("PadAssignWindow.xaml");
            this.Resources.MergedDictionaries.Add(res);

            InitializeComponent();
            DataContext = this;

            Machines = Global.Buzz.Song.Machines.Where(x => x.Name != "Master").ToList();

            midiNotes = new List<string>();

            MidiNotes = BuzzNote.Names.ToList();

            SelectedNote = "C-4";

            btOk.Click += (sender, e) =>
            {
                this.DialogResult = true;
                this.Close();
            };

            btCancel.Click += (sender, e) =>
            {
                this.DialogResult = false;
                this.Close();
            };

            btClear.Click += (sender, e) =>
            {
                Clear = true;
                this.DialogResult = true;
                this.Close();
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
