﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mediapfunctions;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;



namespace WindowsFormsApp3
{
    public partial class mediaform : Form
    {
        #region defined properties/variables

        //defining our private objects of outputdevice and audio files to send to the sound card
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;
        
        
        

        //indicator for pause and mute button
        public bool PlayButtonClicked = false;
        public bool MuteButtonClicked = false;

        //fields for playlist
        public string[] files, paths;

        //linkedlist initialization
        DoubleLinkedList<string> MyList = new DoubleLinkedList<string>();
        

        #endregion

        #region initializer

        public mediaform()
        {
            InitializeComponent();
        }

        #endregion

        #region Exit toolstrip

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion

        #region OpenfileDialouge for retreiving the mp3 file

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //filter for mp3 type files only
            openFileDialog1.Filter = "Mp3 Files|*.mp3";
            
                //Openfiledialoguecode
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {          
                
                    string FileName = openFileDialog1.FileName;  //retreiving path of mp3 file to FileName                                                                 
                    outputDevice = new WaveOutEvent(); //initializing a output device of type waveoutevent                                        
                    audioFile = new AudioFileReader(FileName); //initializing an object of audiofile of type audiofilereader the refrences the path of the mp3 file

                }                        
        }

        #endregion

        #region PlayAndPause button

        private void button1_Click(object sender, EventArgs e)
        {
            if (PlayButtonClicked == false) //checks if playbutton was clicked and if not then it plays file
            {
                pictureBox2.Visible = false;
                pictureBox1.Visible = true;
                PlayButtonClicked = true; //to indicate if the playbutton was clicked so that next time it pauses instead

                try //due to exception occuring when trying to rewind then play we added this try/catch 
                {
                    if (outputDevice==null)
                    {
                        
                        openFileDialog1.Filter = "Mp3 Files|*.mp3";
                        openFileDialog1.Multiselect = false;
                        if (openFileDialog1.ShowDialog() == DialogResult.OK) 
                        {
                            PlayButtonClicked = false; ///to indicate if the playbutton was clicked so that next time it plays instead
                            string FileName = openFileDialog1.FileName;//retreiving path of mp3 file to FileName
                            files = openFileDialog1.SafeFileNames; //save file name

                            for (int i = 0; i < files.Length; i++)
                            {
                                listBox1.Items.Add(files[i]); //add file name to our listbox 

                            }


                            outputDevice = new WaveOutEvent(); //initializing a output device of type waveoutevent
                            audioFile = new AudioFileReader(FileName); //initializing an object of audiofile of type audiofilereader the refrences the path of the mp3 file
                            outputDevice.Init(audioFile); //loads the audiofile to the sound card
                            songtrack.Maximum = (int)audioFile.TotalTime.TotalSeconds; //makes the scale of the trackbar the same length as the 
                            songtrack.Value = 0; // makes the pointer's initial value = 0
                            outputDevice.Play();//plays the audio
                            timer1.Start(); //starts the timer for the pointer to start with the song 
                        }
                    }
                    else
                    {
                        outputDevice.Play(); //plays the audio
                        timer1.Start(); //starts the timer for the pointer to start with the song 
                    }
                    
                }
                catch (System.InvalidOperationException)
                {
                   
                }

            

            }
            else
            {
                pictureBox2.Visible = false;
                pictureBox1.Visible = false;
                outputDevice?.Stop(); //checks to see if the output device running the stops if it find it
                timer1.Stop(); //stops the timer for the pointer 
                PlayButtonClicked = false; ///to indicate if the playbutton was clicked so that next time it plays instead
            }
        }

        #endregion

        #region rewind button

        private void rewind_Click(object sender, EventArgs e)
        {
            try
            {
                outputDevice.Stop(); //stops playing
                audioFile.Position = 0; //adjusts position of file back to 0 (to rewind) 
                outputDevice.Init(audioFile); //loads the audiofile to the soundcard(output device) again
                outputDevice.Play(); //plays audio
            }
            catch (System.NullReferenceException)
            {
                MessageBox.Show("Open file first");
              
            }
            
        }


        #endregion

        #region rewindback15sec
        private void skipback10_Click(object sender, EventArgs e)
        {
            outputDevice.Stop(); //stops playing
            audioFile.CurrentTime = audioFile.CurrentTime.Subtract(TimeSpan.FromSeconds(15));//subtracts 15 seconds from current time to rewind back 15 seconds
            outputDevice.Play(); //plays audio
        }
        #endregion

        #region skipahead15sec
        private void skipahead10_Click(object sender, EventArgs e)
        {
            outputDevice.Stop(); //stops playing
            audioFile.CurrentTime = audioFile.CurrentTime.Add(TimeSpan.FromSeconds(15));//adds 15 seconds from current time to skip ahead 15 seconds
            outputDevice.Play(); //plays audio
        }
        #endregion

        #region MuteButton
        private void button3_Click(object sender, EventArgs e)
        {

            if (MuteButtonClicked == false)
            {
                outputDevice.Volume = 0.0f;
                MuteButtonClicked = true;
            }
            else 
            {
                outputDevice.Volume = volumebar.Value/10f;
                MuteButtonClicked = false;

            }

        }
        #endregion

        #region volumebar
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            volumebar.Scroll += (low, high) => outputDevice.Volume = volumebar.Value / 10f;
        }
        #endregion

        #region trackbar
        private void songtrack_Scroll(object sender, EventArgs e)
        {
            audioFile.CurrentTime = TimeSpan.FromSeconds(songtrack.Value);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                songtrack.Value = (int)audioFile.CurrentTime.TotalSeconds;
                songpostion.Text = audioFile.CurrentTime + "";
                songtime.Text = audioFile.TotalTime + "";
            }
            catch (System.NullReferenceException)
            {

            }
        }

        #endregion

        #region Exit Button
        private void button5_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to exit?!! ", "Exit", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    outputDevice.Dispose(); //disposes of resources taken by Naudio library
                    audioFile.Dispose(); //disposes of resources taken by Naudio library
                }
                catch (NullReferenceException)
                {

                }

                Close(); //closes form
            }
        }

        #endregion


        #region goto mp4form
        private void openMp4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Hide();
            mp4Form mp4 = new mp4Form();
            mp4.Show();
        }
        #endregion

        #region playlist
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) //event when selecting from the list box
        {
            if (outputDevice == null)
            {
                try
                {
                    string filename = MyList.search(listBox1.SelectedItem.ToString()); //take the selected index and pass it to the array to pick out the path to the file we want to play

                    outputDevice = new WaveOutEvent(); //initializing a output device of type waveoutevent
                    audioFile = new AudioFileReader(filename); //initializing an object of audiofile of type audiofilereader the refrences the path of the mp3 file
                    outputDevice.Init(audioFile); //loads the audiofile to the sound card
                    songtrack.Maximum = (int)audioFile.TotalTime.TotalSeconds; //makes the scale of the trackbar the same length as the 
                    songtrack.Value = 0; // makes the pointer's initial value = 0
                    outputDevice.Play();//plays the audio
                    timer1.Start(); //starts the timer for the pointer to start with the song
                }
                catch (NullReferenceException) { }
            }
            else
            {
                outputDevice.Dispose();
                try
                {
                    string filename = MyList.search(listBox1.SelectedItem.ToString()); //take the selected index and pass it to the array to pick out the path to the file we want to play
                    outputDevice = new WaveOutEvent(); //initializing a output device of type waveoutevent
                    audioFile = new AudioFileReader(filename); //initializing an object of audiofile of type audiofilereader the refrences the path of the mp3 file

                    outputDevice.Init(audioFile); //loads the audiofile to the sound card
                    songtrack.Maximum = (int)audioFile.TotalTime.TotalSeconds; //makes the scale of the trackbar the same length as the 
                    songtrack.Value = 0; // makes the pointer's initial value = 0
                    outputDevice.Play();//plays the audio
                    timer1.Start(); //starts the timer for the pointer to start with the song
                }
                catch (NullReferenceException) { }
            }
        }
           

        

        private void button4_Click(object sender, EventArgs e)
        {

            //listBox1.Items.Clear(); //clears items in playlist
            openFileDialog1.Multiselect = true; //for selecting multiple files
            openFileDialog1.Filter = "Mp3 Files|*.mp3"; //filter 
            
            

            if (openFileDialog1.ShowDialog()==DialogResult.OK)
            {
               
                files = openFileDialog1.SafeFileNames; //save file name
                paths = openFileDialog1.FileNames; //save file path
             
                for (int i=0 ; i < files.Length; i++)
                {
                   MyList.AddEnd(paths[i]);
                    listBox1.Items.Add(paths[i]); //add file name to our listbox 
                                                          
                }
            }
        }
        #endregion

        #region clear button
        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }
        #endregion

        #region next
        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                string filename = MyList.SearchToNext(listBox1.SelectedItem.ToString());
                try
                {
                    listBox1.SelectedIndex += 1;


                    outputDevice.Dispose();
                    outputDevice = new WaveOutEvent();
                    audioFile = new AudioFileReader(filename);
                    outputDevice.Init(audioFile);
                    songtrack.Maximum = (int)audioFile.TotalTime.TotalSeconds;
                    songtrack.Value = 0;


                    outputDevice.Play();
                    timer1.Start();
                }
                catch (ArgumentOutOfRangeException)
                {
                    outputDevice.Stop();
                    outputDevice.Dispose();
                    pictureBox2.Visible = true;
                    
                    return;
                }

            }
            catch (NullReferenceException) { }
        }

       

  

        private void mediaform_Load(object sender, EventArgs e)
        {
            pictureBox1.Visible = false;
            pictureBox2.Visible = false;
        }
        #endregion

        #region prev
        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                string filename = MyList.SearchToPrev(listBox1.SelectedItem.ToString());
                listBox1.SelectedIndex -= 1;
                outputDevice.Dispose();
                outputDevice = new WaveOutEvent();
                audioFile = new AudioFileReader(filename);
                outputDevice.Init(audioFile);
                songtrack.Maximum = (int)audioFile.TotalTime.TotalSeconds;
                songtrack.Value = 0;
                if (listBox1.SelectedIndex != -1)
                {
                    outputDevice.Play();
                    timer1.Start();
                }
            }
            catch (NullReferenceException) { pictureBox2.Visible = true; }
        }
        #endregion
    }
}
