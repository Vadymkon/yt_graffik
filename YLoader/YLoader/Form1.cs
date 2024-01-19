﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;


using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using BrightIdeasSoftware;
using System.Diagnostics;
using Microsoft.CSharp.RuntimeBinder;
using System.Configuration;
using YLoader.Properties;
/*
using Google.GData.Extensions;
using Google.GData.YouTube;
using Google.GData.Extensions.MediaRss;
using Google.YouTube;
using Google.GData.Client;
using System.Diagnostics;
*/
namespace YLoader
{
    public partial class Form1 : Form
    {
        String active_path = ""; 
        public YTStaff yt;
        public Form1()
        {
            InitializeComponent();
            yt_Button2.Click += new System.EventHandler(yt_Button1_Click); //copy action
            
            //set active path
            active_path = Settings.Default["active_path"].ToString(); //
            if (active_path == "")
                active_path = Settings.Default["paths"].ToString().Split('|').ToList().Last(); //
            if (active_path == "")
                take_path();

            update_dropdownlist();
            
            CEO_settings(); //settings the table of CEO-data
            egoldsToggleSwitch1.Checked = Convert.ToBoolean(Settings.Default["open_form2"]); // check parametrs

            // oauth2.0
            yt = new YTStaff();


            //def description update
            if (File.Exists(Path.GetDirectoryName(Application.ExecutablePath) + @"\def_descr.txt")) 
            {
            String def_descr = "\r\n";
            File.ReadAllLines(Path.GetDirectoryName(Application.ExecutablePath) + @"\def_descr.txt").ToList().ForEach(line => def_descr += $"{line}\r\n");
            Settings.Default["def_descr"] = def_descr;
            Settings.Default.Save();
            }

            //MakeGraphik();
        }

        async void button1_Click(object sender, EventArgs e) //TEST-Button
        {
            //UploadVideo(@"C:\Users\vadymkon\Desktop\test.mp4","test","test Description");
            //RunSomethink(); //upload test video
            /*yt.getListOfMyVideos();
            yt.UpdateVideo(new VideoFile("2",@"C:\Users\vadymkon\Desktop"));*/
           // yt.ThumbnailSetResponse(new VideoFile("2", @"C:\Users\vadymkon\Desktop"));

            MessageBox.Show("done");
        }
        
        void button2_Click(object sender, EventArgs e) //save template of mails-sending
        {
            using (OpenFileDialog fileDialog = new OpenFileDialog()) //fileDialog
            {
                fileDialog.Title = "Choose directory to save example";
                fileDialog.ValidateNames = false;
                fileDialog.CheckFileExists = false;
                fileDialog.CheckPathExists = true;
                fileDialog.FileName = "mails_example.txt"; 
                fileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                
                DialogResult result = fileDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string selectedFolder = Path.GetDirectoryName(fileDialog.FileName); 
                    string selectedFileName = Path.GetFileName(fileDialog.FileName).Replace(".txt","");

                    new MailStaff().SaveExample(selectedFolder,selectedFileName);
                }
            }
        }
        
        void yt_Button1_Click(object sender, EventArgs e) //main big button
        {
            if (!File.Exists(Path.GetDirectoryName(Application.ExecutablePath) + @"\GR_history\_graphik.txt")) MakeGraphik();
            new Form2(this).Show();
        }
        
        private void yt_Button9_Click(object sender, EventArgs e) //refresh OAuth2.0
        {
            MessageBox.Show("Now several sites are going to open. \r\nPlease login in your chosen account \r\nfor managing video several times. \r\n\r\n(if you will login into different \r\naccounts - result of working is your deal :) )","ATTENTION!");
            yt.Refresh();
            yt = new YTStaff();
        }

        async void yt_Button5_Click(object sender, EventArgs e) //Mailing
        {
            using (OpenFileDialog fileDialog = new OpenFileDialog()) //fileDialog
            {
                fileDialog.Title = "Choose file for emailing.";
                fileDialog.ValidateNames = false;
                fileDialog.CheckFileExists = false;
                fileDialog.CheckPathExists = true;
                fileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";

                DialogResult result = fileDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string selectedFolder = Path.GetDirectoryName(fileDialog.FileName);
                    string selectedFileName = Path.GetFileName(fileDialog.FileName).Replace(".txt", "");

                    var a = new MailStaff();
                    a.readLetters(fileDialog.FileName);
                    var dialogResult = MessageBox.Show($"There are {a.Count()} letters.", "Are you sure?", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        await Task.Run(()=> { a.SendAll(); });
                        MessageBox.Show($"Sended {a.Count()} letters.");
                    }
                }
            }
        }

        async void yt_Button7_Click(object sender, EventArgs e) //MakeGraphik
        {
            await Task.Run(() => {
                MakeGraphik();
                MakeGraphikSh(new DateTime(2024, 06, 01));
            }); 
        }

        void egoldsToggleSwitch1_CheckedChanged(object sender) //auto open form2
        {
            Settings.Default["open_form2"] = egoldsToggleSwitch1.Checked;
            Settings.Default.Save();
        } 

        void yt_Button8_Click(object sender, EventArgs e) //add source-folder
        {
            take_path();
            update_dropdownlist();
        } 

        void button3_Click(object sender, EventArgs e) //reset program settings
        {
            Settings.Default.Reset();
            MessageBox.Show("Please Reload Program");
            Close();
        } 

        void cmbStyle_SelectedIndexChanged(object sender, EventArgs e) //change active_path
        {
            active_path = cmbStyle.Items[cmbStyle.SelectedIndex].ToString();
            Settings.Default["active_path"] = active_path;
            Settings.Default.Save();
            Console.WriteLine(active_path);
            label2.Visible = false; linkLabel1.Visible = false;
            CEO_settings();
        }

        void yt_Button3_Click(object sender, EventArgs e) //shorts-GR
        {
            new Form2(this, true).Show();
        } 

        void Form1_Shown(object sender, EventArgs e) //auto open form2
        {
            if (egoldsToggleSwitch1.Checked) new Form2(this).Show(); // CheckBox action
        }

        void yt_Button6_Click(object sender, EventArgs e) //open custom GR file
        {
            using (OpenFileDialog fileDialog = new OpenFileDialog()) //fileDialog
            {
                fileDialog.Title = "Choose file of GRaffik.";
                fileDialog.ValidateNames = false;
                fileDialog.CheckFileExists = false;
                fileDialog.CheckPathExists = true;
                fileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";

                DialogResult result = fileDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                   new Form2(this, pathToCustomGR: fileDialog.FileName).Show();
                    
                }
            }
        }

        void yt_Button10_Click(object sender, EventArgs e) //Thumbnails UPLOAD
        {
            var a = getVideoList().Where(x => x.Id != "").ToList(); //get Id-in videos
            egoldsProgressBar1.ValueMaximum = a.Count; //progressbar settings
            
            a.ForEach(x => { yt.ThumbnailSetResponse(x); //thumbnails uploading
                ++egoldsProgressBar1.Value;
            });

        }

        void yt_Button4_Click(object sender, EventArgs e) //move next 1 month videos in near folder and open it
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Is there shorts?";
                folderDialog.SelectedPath = Settings.Default.sh_path.Trim() == "" ? Settings.Default.active_path : Settings.Default.sh_path;
                DialogResult result = folderDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string selectedFolder = folderDialog.SelectedPath;
                    //save path
                    Settings.Default.sh_path = selectedFolder;
                    Settings.Default.Save(); 

                    Directory.CreateDirectory(selectedFolder + "\\monthNext"); //create
                    String pathToGrSH = Path.GetDirectoryName(Application.ExecutablePath) + @"\GR_history\_graphik_SH.txt"; //path
                    if (!File.Exists(pathToGrSH)) MakeGraphikSh(new DateTime(2024,06,01)); //safety
                    Graphik a = new Graphik(pathToGrSH); //get GR object
                    int count = a.queueDT.Where(x => x <= DateTime.Now.AddDays(30)).ToList().Count; //how much videos
                    var filesIN = Directory.GetFiles(selectedFolder);
                    a.queue.GetRange(0, count).ForEach(x => {
                        if (filesIN.Contains(selectedFolder+"\\"+x))
                        {
                            File.Move(selectedFolder + "\\" + x, selectedFolder + "\\monthNext" + "\\" + x); //moving to this folder
                        }
                    });

                    Process.Start(selectedFolder+"\\monthNext"); // show folder
                        
                }
            }
        }
    }


    static class MyExtensions
    {
        private static Random rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static DateTime toDateTime(this String line)
        {
            List<int> datePartList = line.Trim().Split('.').ToList().Select(y => Convert.ToInt32(y)).ToList();
            DateTime PublishedDate = new DateTime(datePartList[2], datePartList[1], datePartList[0]);
            return PublishedDate;
        }
    }
}