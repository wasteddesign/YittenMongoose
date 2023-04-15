using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Windows;
using Buzz.MachineInterface;
using BuzzGUI.Common;
using BuzzGUI.Interfaces;

namespace WDE.YittenMongoose
{
    [MachineDecl(Name = "WDE Yitten Mongoose", ShortName = "Mongoose", Author = "WDE")]
	public class YittenMongooseMachine : IBuzzMachine, INotifyPropertyChanged
	{
		IBuzzMachineHost host;
        public static int NUM_PADS = 16;
        public static int NUM_PAD_SETS = 4;

        public YittenMongooseMachine(IBuzzMachineHost host)
		{
			this.host = host;
            Global.Buzz.PropertyChanged += Buzz_PropertyChanged;
		}

        private void Buzz_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Playing")
            {
                if (!Global.Buzz.Playing)
                {
                    foreach(var note in noteBuffer)
                    {
                        note.machine.SendMIDINote(note.midiChannel, note.midiNote, 0);
                    }

                    noteBuffer.Clear();
                }
            }
        }

        [ParameterDecl(MinValue = 1, MaxValue = 128, DefValue = 16)]
		public int Bar { get; set; }

        public class PadData
        {
            public string machine;
            public int midiChannel;
            public int note;
            public int volume;
            public int bar;
            public bool toggle;
            public bool syncNoteOff;

            public PadData()
            {
                Init();
            }

            public void Init()
            {
                machine = null;
                midiChannel = 0;
                note = 0;
                volume = 127;
                bar = 0;
                toggle = true;
                syncNoteOff = false;
            }
        }
        
        public class State : INotifyPropertyChanged
        {
            public State()
            {
                padDataTable = new PadData[NUM_PADS * NUM_PAD_SETS];
                for (int i = 0; i < NUM_PADS * NUM_PAD_SETS; i++)
                    padDataTable[i] = new PadData();

            }  // NOTE: parameterless constructor is required by the xml serializer

            public PadData [] padDataTable;
            public PadData[] PadDataTable
            {
                get { return padDataTable; }
                set
                {
                    padDataTable = value;
                    if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Data"));
                    // NOTE: the INotifyPropertyChanged stuff is only used for data binding in the GUI in this demo. it is not required by the serializer.
                }
            }
            public event PropertyChangedEventHandler PropertyChanged;
        }

        State machineState = new State();

        public event PropertyChangedEventHandler PropertyChanged;

        public State MachineState           // a property called 'MachineState' gets automatically saved in songs and presets
        {
            get { return machineState; }
            set
            {
                machineState = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("MachineState"));
            }
        }

        internal static ResourceDictionary GetBuzzThemeResources(string name)
        {
            ResourceDictionary skin = new ResourceDictionary();

            try
            {
                string selectedTheme = Global.Buzz.SelectedTheme == "<default>" ? "Default" : Global.Buzz.SelectedTheme;
                string skinPath = Global.BuzzPath + "\\Themes\\" + selectedTheme + "\\Gear\\YittenMongoose\\" + name;
               
                skin = (ResourceDictionary)XamlReaderEx.LoadHack(skinPath);
            }
            catch (Exception)
            {
                string skinPath = Global.BuzzPath + "\\Themes\\Default\\Gear\\YittenMongoose\\" + name;
                skin.Source = new Uri(skinPath, UriKind.Absolute);
            }

            return skin;
        }

        public void Work()
        {
            if (host.MasterInfo.PosInTick == 0 && host.SubTickInfo.PosInSubTick == 0 && Global.Buzz.Playing)
            {
                int songPos = Global.Buzz.Song.PlayPosition;
                for (int i = 0; i < noteBuffer.Count; i++)
                {
                    if (noteBuffer[i].time == songPos)
                    {
                        Note note = noteBuffer[i];
                        var mac = note.machine;
                        mac.SendMIDINote(note.midiChannel, note.midiNote, note.volume);
                        noteBuffer.RemoveAt(i);
                        i--;
                    }
                }
                
            }
        }

        struct Note
        {
            public IMachine machine;
            public int midiChannel;
            public int midiNote;
            public int volume;
            public int time;
        }

        List<Note> noteBuffer = new List<Note>();

        internal void PlayNote(IMachine mac, int midiChannel, int midiNote, int volume, int time)
        {
            Note note = new Note();
            note.machine = mac;
            note.volume = volume;
            note.time = time;
            note.midiChannel = midiChannel;
            note.midiNote = midiNote;

            if (mac != null)
            {
                if (time == -1)
                {
                    mac.SendMIDINote(note.midiChannel, note.midiNote, note.volume);
                }
                else
                {
                    noteBuffer.Add(note);
                }
            }

        }

        internal void StopNote(IMachine mac, int midiChannel, int midiNote, int time)
        {
            Note note = new Note();
            note.machine = mac;
            note.volume = 0;
            note.time = time;
            note.midiChannel = midiChannel;
            note.midiNote = midiNote;

            if (mac != null)
            {
                if (time == -1)
                {
                    mac.SendMIDINote(midiChannel, midiNote, 0);
                    noteBuffer.RemoveAll(x =>
                        x.machine == mac &&
                        x.midiNote == midiNote &&
                        x.midiChannel == midiChannel);
                }
                else
                {
                    noteBuffer.Add(note);
                }
            }
        }
    }

}
