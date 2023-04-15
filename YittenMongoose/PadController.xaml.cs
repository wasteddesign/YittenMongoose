using BuzzGUI.Common;
using BuzzGUI.Common.Templates;
using BuzzGUI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WDE.YittenMongoose
{
    /// <summary>
    /// Interaction logic for PadController.xaml
    /// </summary>
    public partial class PadController : UserControl
    {
        int NUM_ROWS = 2;
        int NUM_COLUMNS = 8;
        double BOX_SIZE = 100;

        IEnumerable<Color> padColors;

        private YittenMongooseMachine machine;
        private int selectedPadSet;

        public YittenMongooseMachine Machine { get => machine; set
            {
                machine = value;

                if (machine != null)
                {
                    machine = value;
                    UpdateView();
                }
            }
        }

        private void UpdateView()
        {
            foreach (PadElement c in padGrid.Children) c.Release();
            padGrid.Children.Clear();

            int num = 0;

            for (int i = 0; i < NUM_ROWS; i++)
            {
                for (int j = 0; j < NUM_COLUMNS; j++)
                {   
                    PadElement pad = new PadElement(this, num + selectedPadSet * NUM_ROWS * NUM_COLUMNS, padColors.ElementAt(num))
                    {
                        Width = BOX_SIZE,
                        Height = BOX_SIZE,
                        Margin = new Thickness(4, 4, 4, 4)
                    };
                    Grid.SetRow(pad, i);
                    Grid.SetColumn(pad, j);
                    padGrid.Children.Add(pad);
                    num++;
                }
            }
        }

        internal void PadSet(int selectedPadSet)
        {
            this.selectedPadSet = selectedPadSet;
            UpdateView();
        }

        public PadController()
        {   
            InitializeComponent();
            DataContext = this;

            selectedPadSet = 0;

            padColors = new HSPAColorProvider(16, 0, 0.916667,0.7, 0.7, 0.6, 0.6).Colors;
            padGrid.Width = (BOX_SIZE + 4 + 4) * NUM_COLUMNS;

            for (int i = 0; i < NUM_ROWS; i++)
                padGrid.RowDefinitions.Add(new RowDefinition());
            for (int j = 0; j < NUM_COLUMNS; j++)
                padGrid.ColumnDefinitions.Add(new ColumnDefinition());
        }
    }

    public class PadElement : Border
    {
        private Color bg;
        private PadController pc;
        private int padNum;
        TextBlock tb;
        private bool playing;

        IMachine selectedMachine;
        public IMachine SelectedMachine
        {
            get => selectedMachine; set
            {
                if (selectedMachine != null)
                {
                    selectedMachine.PropertyChanged -= SelectedMachine_PropertyChanged;
                }

                selectedMachine = value;

                if (selectedMachine != null)
                {
                    selectedMachine.PropertyChanged += SelectedMachine_PropertyChanged;
                }
            }
        }
        private void SelectedMachine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
            {
                var pd = pc.Machine.MachineState.PadDataTable[padNum];
                pd.machine = SelectedMachine.Name;
                UpdateText();
            }
        }

        public PadElement(PadController pc, int num, Color bg)
        {
            this.bg = bg;
            this.Background = new SolidColorBrush(bg);
            this.pc = pc;
            this.padNum = num;
            BorderThickness = new Thickness(1, 1, 1, 1);
            CornerRadius = new CornerRadius(8, 8, 8, 8);
            this.Opacity = 0;


            Global.Buzz.Song.MachineRemoved += Song_MachineRemoved;
            Global.Buzz.PropertyChanged += Buzz_PropertyChanged;

            tb = new TextBlock()
            {
                Text = "",
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Tahoma"),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };

            tb.SetBinding(TextBlock.OpacityProperty, new Binding("OpacityProperty") { Source = this, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

            this.Child = tb;
            UpdateText();
            SetInitAnimation();

            this.MouseRightButtonDown += (sender, e) =>
            {
                if (e.ClickCount == 1)
                {   
                    Point pos = Win32Mouse.GetScreenPosition();
                    pos.X /= WPFExtensions.PixelsPerDip;
                    pos.Y /= WPFExtensions.PixelsPerDip;
                    PadAssignWindow paw = new PadAssignWindow();
                    paw.Left = pos.X + 5;
                    paw.Top = pos.Y - 20;

                    var pd = pc.Machine.MachineState.PadDataTable[padNum];
                    paw.SelectedMachine = paw.Machines.FirstOrDefault(m => m.Name == pd.machine);
                    if (paw.SelectedMachine == null)
                        paw.SelectedMachine = paw.Machines.First();
                    paw.SelectedNote = pd.note > 0 ? BuzzNote.ToString(pd.note) : "C-4";
                    paw.midiChannel.Value = pd.midiChannel + 1;
                    paw.nudVol.Value = pd.volume;
                    paw.nudBar.Value = pd.bar;
                    paw.cbToggle.IsChecked = pd.toggle;
                    paw.cbSyncNoteOff.IsChecked = pd.syncNoteOff;

                    StopAnimation();
                    Stop();
                    
                    if (paw.ShowDialog() == true)
                    {
                        if (paw.Clear)
                        {
                            // Clear
                            SelectedMachine = null;
                            pd.Init();
                        }
                        else
                        {
                            pd.volume = (int)paw.nudVol.Value;
                            pd.midiChannel = (int)paw.midiChannel.Value - 1;
                            pd.machine = paw.SelectedMachine.Name;
                            pd.note = BuzzNote.Parse(paw.SelectedNote);
                            pd.toggle = paw.cbToggle.IsChecked == true;
                            pd.bar = (int)paw.nudBar.Value;
                            pd.syncNoteOff = paw.cbSyncNoteOff.IsChecked == true;
                            SelectedMachine = paw.SelectedMachine;
                        }
                        UpdateText();
                    }
                }
            };

            this.MouseLeftButtonDown += (sender, e) =>
            {
                var pd = pc.Machine.MachineState.PadDataTable[padNum];
                var mac = Global.Buzz.Song.Machines.FirstOrDefault(m => m.Name == pd.machine);

                if (mac != null)
                {   
                    if (playing)
                    {
                        Stop(false);
                        StopAnimation();
                        if (!pd.toggle)
                        {
                            Play();
                            SetPadAnimation(false);
                        }
                    }
                    else
                    {
                        Play();
                        SetPadAnimation(pd.toggle);
                    }
                }
            };
        }

        private void Buzz_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Playing")
            {
                if (!Global.Buzz.Playing)
                {
                    StopAnimation();
                }
            }
        }

        private void Song_MachineRemoved(IMachine obj)
        {
            if (SelectedMachine == obj)
            {
                StopAnimation();
                Stop();

                var pd = pc.Machine.MachineState.PadDataTable[padNum];
                pd.Init();
                SelectedMachine = null;
                UpdateText();
            }
        }

        private void UpdateText()
        {
            var pd = pc.Machine.MachineState.PadDataTable[padNum];
            string text = pd.machine;
            if (pd.note > 0)
            {
                text += "\n" + BuzzNote.ToString(pd.note);
                text += "\nMidi C: " + (pd.midiChannel + 1);
            }
            tb.Text = text;
        }

        public void Play()
        {
            var pd = pc.Machine.MachineState.PadDataTable[padNum];
            var mac = Global.Buzz.Song.Machines.FirstOrDefault(m => m.Name == pd.machine);
            int midinote = BuzzNote.ToMIDINote(pd.note);
            playing = true;

            var song = Global.Buzz.Song;
            int bar = pd.bar;
            int time = -1;
            if (bar > 0)
                time = song.LoopStart + ((song.PlayPosition - song.LoopStart + bar) / bar * bar % (song.LoopEnd - song.LoopStart));

            pc.Machine.PlayNote(mac, pd.midiChannel, midinote, pd.volume, time);
        }

        public void Stop(bool immediately = true)
        {
            var pd = pc.Machine.MachineState.PadDataTable[padNum];
            var mac = Global.Buzz.Song.Machines.FirstOrDefault(m => m.Name == pd.machine);
            if (mac != null && playing)
            {
                var song = Global.Buzz.Song;
                int bar = pd.bar;
                int time = -1;
                if (!immediately && bar > 0 && pd.syncNoteOff)
                    time = song.LoopStart + ((song.PlayPosition - song.LoopStart + bar) / bar * bar % (song.LoopEnd - song.LoopStart));

                int midiNote = BuzzNote.ToMIDINote(pd.note);
                playing = false;

                pc.Machine.StopNote(mac, pd.midiChannel, midiNote, time);
            }
        }

        private void SetPadAnimation(bool loop)
        {
            var pd = pc.Machine.MachineState.PadDataTable[padNum];

            if (pd.bar > 0 && !Global.Buzz.Playing)
                return;

            var cAnimation = new ColorAnimation();

            cAnimation.From = Colors.White.Blend(bg, 0.5);
            cAnimation.To = bg;

            cAnimation.Duration = new Duration(TimeSpan.FromSeconds(1.0));
            if (loop)
                cAnimation.RepeatBehavior = RepeatBehavior.Forever;
            this.Background.BeginAnimation(SolidColorBrush.ColorProperty, cAnimation);
        }

        private void SetInitAnimation()
        {
            var myDoubleAnimation = new DoubleAnimation();

            myDoubleAnimation.From = 0.0;
            myDoubleAnimation.To = 1.0;

            myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.2));
            myDoubleAnimation.BeginTime = TimeSpan.FromSeconds((padNum % 16.0) * 0.01);
            myDoubleAnimation.AutoReverse = false;

            Storyboard myStoryboard = new Storyboard();
            myStoryboard.Children.Add(myDoubleAnimation);
            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(Border.OpacityProperty));
            myStoryboard.Begin(this);
        }

        private void StopAnimation()
        {
            this.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);
        }

        public void Release()
        {
            StopAnimation();
            Stop();
            BindingOperations.ClearBinding(tb, TextBlock.OpacityProperty);
            Global.Buzz.Song.MachineRemoved -= Song_MachineRemoved;
            Global.Buzz.PropertyChanged -= Buzz_PropertyChanged;
            SelectedMachine = null;
        }
    }

}
