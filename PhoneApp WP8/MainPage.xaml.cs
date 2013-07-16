﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Threading;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;

namespace ClarinetTraining
{
    public partial class MainPage : PhoneApplicationPage
    {
        private int currentScale = 0;
        private int currentUpper = 29;
        private int currentLower = 2;
		
		private int _delay = 2000; //ms
        private DispatcherTimer timer;
        private bool playing=false;
        bool soundOn = true;
        ClarinetSound sound;

        private int[] harmonicIntervals = new int[] { 0, 2, 2, 1, 2, 2, 02, 01 };
        private int[] harmonicDistances = new int[] { 0, 2, 4, 5, 7, 9, 11, 12 };
            
        private int[] chromaticNot = new int[] { 0, 0, 1, 1, 2, 3, 3, 4, 4, 5, 5, 6 };
        private int[] chromaticSus = new int[] { 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0 };

        private int[] intervalsValues = { 250, 500, 1000, 2000, 3000, 5000, 7000, 15000, 30000 };

        private Random rnd = new Random();

        public MainPage()
        {
            
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Tick += tick;
            setInterval(_delay);
            sound = new ClarinetSound();
        }


        private void PhoneApplicationPage_Loaded_1(object sender, RoutedEventArgs e)
        {
            loadCondiguration();
            PageIn.Begin();
        }

        private void setInterval(int delay)
        {
            if (timer != null) timer.Interval = new TimeSpan(0, 0, 0, 0, delay);
        }

        
        private void setSound(bool value)
        {
            soundOn = value;

            var on =    new Uri("/Assets/AppBar/sound on.png",UriKind.Relative);
            var off =   new Uri("/Assets/AppBar/sound off.png",UriKind.Relative);

            (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IconUri = value ? on : off;
        }

        private void startShowing()
        {
            tick(null, null);
            timer.Start();
        }

        private void stopShowing()
        {
            timer.Stop();
        }

        /// <summary>
        /// Shows a note 
        /// </summary>
        /// <param name="n"></param>
        private void displayNote(Note n)
        {

            sheet.showNote(n);
            clarinet.showNote(n);
            if (soundOn) sound.playNote(n);
        }

        /// <summary>
        /// Display a Rando note
        /// </summary>
        /// <param name="n"></param>
        private void displayRandomNote()
        {
            Note n;
            do
                n = randomNote(currentScale, currentUpper, currentLower);
            while (!clarinet.showNote(n));
                
            sheet.showNote(n);
            if (soundOn) sound.playNote(n);
        }

        /// <summary>
        /// timer tick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tick(Object sender, EventArgs e){
            displayRandomNote();
        }

        /////////////////////////////////////////////////// Saved Configuration //////////////////////////

        /// <summary>
        /// Loads user configuration and configures UI
        /// </summary>
        private void loadCondiguration()
        {
            bool inverted = false;
            bool sound = true;
            int timeInterval = 3;
            int scale = 0;
            int range = 0;

            IsolatedStorageSettings.ApplicationSettings.TryGetValue("inverted", out inverted);
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("time", out timeInterval);
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("scale", out scale);
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("range", out range);
            if (IsolatedStorageSettings.ApplicationSettings.TryGetValue("sound", out sound))
                setSound(sound);

            RangeList.SelectedIndex = range;
            IntervalList.SelectedIndex = timeInterval;
            ScaleList.SelectedIndex = scale;
            clarinet.setInverted(inverted);
            
        }

        /////////////////////////////////////////////////// this below shoud be in another class /////////
        
        /// <summary>
        /// sets the Current scale
        /// </summary>
        /// <param name="scale"></param>
        private void setScale(int scale)
        {
            
            this.currentScale = scale;
            if (sheet == null) return;
            sheet.setScale(scale);
        }

        /// <summary>
        /// Get the variant note in a specific scale.
        /// </summary>
        /// <param name="scale">Scale</param>
        /// <param name="note">Notes</param>
        /// <returns>Return a new Note and if it is Bmol or Sustenide</returns>
        private Note getNoteVariantOnScale(int scale, int note)
        {
            var n = new Note();

            var newNote = (note + scale) % 7;
            var chromaticId = (harmonicDistances[scale] + harmonicDistances[note]) % 12;
            var diff = chromaticId - harmonicDistances[newNote];

            n.note = newNote;

            if (diff == 1) n.sus = true;
            if (diff == -1) n.bmol = true;

            return n;
        }

        /// <summary>
        /// Select a random note in a scale(or not)
        /// </summary>
        /// <param name="scale">Scale</param>
        /// <param name="upper">upper limit</param>
        /// <param name="lower">lower limit</param>
        /// <returns>a rando note according with parameters.</returns>
        private Note randomNote(int scale=-1,int upper=29, int lower=2){
            var n = new Note();

            //select note
            //harmonic Scale
            if (scale <=6 )
            {
                var note = rnd.Next(7);
                n = getNoteVariantOnScale(scale, note);
                n.octave = rnd.Next(4);

            }
            //Any Scale
            else
            {

                n.note = rnd.Next(7);
                n.octave = rnd.Next(4);
                var sus = rnd.Next(6);

                switch (sus)
                {
                    case 0: n.sus = true; break;
                    case 1: n.bmol = true; break;
                }

                //System.Diagnostics.Debug.WriteLine("n:" + n.note + " s:" + n.scale);
                
            }

            //limit-it
            while (n.note + n.octave * 7 > upper) n.octave--;
            while (n.note + n.octave * 7 < lower) n.octave++;
          
            return n;
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        #region callbacks

        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            
            setSound(!soundOn); 


            IsolatedStorageSettings.ApplicationSettings["sound"] = soundOn;
            IsolatedStorageSettings.ApplicationSettings.Save();

        }

		private void hideLists(){
            try
            {
                //listsBackground.Visibility = System.Windows.Visibility.Collapsed;
                CloseList.Begin();
                ApplicationBar.IsVisible = true;
            }
            catch
            {
            }
		}

        private void ScaleButton_Click(object sender, System.EventArgs e)
        {
            ApplicationBar.IsVisible = false;
            listsBackground.Visibility = System.Windows.Visibility.Visible;
			ScaleList.Visibility = System.Windows.Visibility.Visible;
            IntervalList.Visibility = System.Windows.Visibility.Collapsed;
            RangeList.Visibility = System.Windows.Visibility.Collapsed;
            OpenList.Begin();


        }

        private void IntervalButton_Click(object sender, System.EventArgs e)
        {
            ApplicationBar.IsVisible = false;
            listsBackground.Visibility = System.Windows.Visibility.Visible;
			ScaleList.Visibility = System.Windows.Visibility.Collapsed;
            RangeList.Visibility = System.Windows.Visibility.Collapsed;
            IntervalList.Visibility = System.Windows.Visibility.Visible;
            OpenList.Begin();
        }

        private void RangeButton_Click(object sender, System.EventArgs e)
        {
            ApplicationBar.IsVisible = false;
            listsBackground.Visibility = System.Windows.Visibility.Visible;
            ScaleList.Visibility = System.Windows.Visibility.Collapsed;
            RangeList.Visibility = System.Windows.Visibility.Visible;
            IntervalList.Visibility = System.Windows.Visibility.Collapsed;
            OpenList.Begin();
        }

        private void IntervalList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        	hideLists();
            var i = (sender as ListBox).SelectedIndex;
            setInterval(intervalsValues[i]);

            IsolatedStorageSettings.ApplicationSettings["time"] = i;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        private void ScaleList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

            var i = (sender as ListBox).SelectedIndex;
            setScale(i);
        	hideLists();


            IsolatedStorageSettings.ApplicationSettings["scale"] = i;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

		private void RangeList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

            var i = (sender as ListBox).SelectedIndex;
        	switch(i ){
				case 0: 
					currentLower = 2;
					currentUpper = 29;
					break;
				case 1: 
					currentUpper= 29;
					currentLower=18;
					break;
				case 2: 
					currentUpper= 17;
					currentLower=9;
					break;
				case 3: 
					currentUpper= 9;
					currentLower=2;
					break;
			}
			hideLists();

            IsolatedStorageSettings.ApplicationSettings["range"] = i;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {

            if( ApplicationBar.IsVisible == false){
                hideLists();
                e.Cancel = true;
            }
            base.OnBackKeyPress(e);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

            playing = !playing;
            if (playing)
            {
                startShowing();

                PlaySymbol.Visibility = System.Windows.Visibility.Collapsed;
                PauseSymbol1.Visibility = System.Windows.Visibility.Visible;

            }
            else
            {
                stopShowing();
                PlaySymbol.Visibility = System.Windows.Visibility.Visible;
                PauseSymbol1.Visibility = System.Windows.Visibility.Collapsed;

            }
        }


        /// Menus ///////////////////////////////////////////////////////////////////////////////
        
        private void ApplicationBarMenuItem_Click_1(object sender, System.EventArgs e)
        {
            var uri = new Uri("/Dictionary.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);

        }


        private void Invert_Click(object sender, EventArgs e)
        {
            clarinet.setInverted(!clarinet.getInverted());

            IsolatedStorageSettings.ApplicationSettings["inverted"] = clarinet.getInverted();
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        private void listsBackground_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
        	hideLists();
        }


        #endregion

 
    }
}