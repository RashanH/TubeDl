﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using YoutubeExtractor;

namespace TubeDl
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            System.Net.ServicePointManager.DefaultConnectionLimit = 100;
        }

        string url;
        string name;
        string savePath;
        VideoInfo video;
        Stopwatch sw = new Stopwatch();    // The stopwatch which we will be using to calculate the download speed

        List<DownloadHelper.downloadFile> ldf = new List<DownloadHelper.downloadFile>();

        //IFileDownloader fileDownloader;
        private static string RemoveIllegalPathCharacters(string path)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(path, "");
        }

        private static string GetFileSize(Uri uriPath)
        {
            var webRequest = HttpWebRequest.Create(uriPath);
            webRequest.Method = "HEAD";

            using (var webResponse = webRequest.GetResponse())
            {
                var fileSize = webResponse.Headers.Get("Content-Length");
                var fileSizeInMegaByte = View.Size.getlength.GetLengthString(Math.Round(
                    Convert.ToDouble(fileSize)));
                return fileSizeInMegaByte;
            }
        }


        private void btndownload_Click(object sender, EventArgs e)
        {
            /*
             * Execute the video downloader.
             * For GUI applications note, that this method runs synchronously.
             */
            try
            {
                btndownload.Enabled = false;


                var ext = video.Resolution.ToString();
                if (ext == "0")
                    ext = ".mp3";
                else
                    ext = ".Mp4";

                ListViewItem item = list_Items.FindItemWithText(name + " " + (video.Resolution == 0 ? ext.Replace(".", "") : video.Resolution.ToString()));

                if (item != null)
                    MessageBox.Show("Video Already in downloading", Text, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                // video exists           

                else
                {

                    // doesn't exist 


                    if (File.Exists(Path.Combine(savePath, name + " " + (video.Resolution == 0 ? ext.Replace(".", "") : video.Resolution.ToString()) + ext)))
                    {
                        if (MessageBox.Show("File Already exist, Replace?", Text,
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            File.Delete(Path.Combine(savePath, name + " " + (video.Resolution == 0 ? ext.Replace(".", "") : video.Resolution.ToString()) + ext));

                            int indx = list_Items.Items.Count;
                            list_Items.Items.Add(name + " " + (video.Resolution == 0 ? ext.Replace(".", "") : video.Resolution.ToString()));
                            for (int i = 1; i < 6; i++)
                            {
                                list_Items.Items[indx].SubItems.Add("");
                            }

                            DownloadHelper.downloadFile d =
                                new DownloadHelper.downloadFile(url, Path.Combine(savePath, name + " " + (video.Resolution == 0 ? ext.Replace(".", "") : video.Resolution.ToString()) + ext));
                            ldf.Add(d);

                            Action<int, int, object> act1 = new Action<int, int, object>(delegate (int idx, int sidx, object obj)
                            {
                                list_Items.Invoke(new Action(() => list_Items.Items[idx].SubItems[sidx].Text = obj.ToString()));

                            });

                            d.eSize += (object s1, string size) => act1.Invoke(indx, 1, size);
                            d.eDownloadedSize += (object s1, string size) => act1.Invoke(indx, 2, size);
                            d.eSpeed += (object s1, string size) => act1.Invoke(indx, 3, size);
                            d.eDownloadState += (object s1, string size) => act1.Invoke(indx, 4, size);
                        }
                        else
                        {
                            btndownload.Enabled = true;
                            btnPause.Enabled = false;
                        }
                    }
                    else
                    {
                        int indx = list_Items.Items.Count;
                        list_Items.Items.Add(name + " " + (video.Resolution == 0 ? ext.Replace(".", "") : video.Resolution.ToString()));
                        for (int i = 1; i < 6; i++)
                        {
                            list_Items.Items[indx].SubItems.Add("");
                        }

                        DownloadHelper.downloadFile d = new DownloadHelper.downloadFile(url, Path.Combine(savePath, name + " " + (video.Resolution == 0 ? ext.Replace(".", "") : video.Resolution.ToString()) + ext));
                        ldf.Add(d);

                        Action<int, int, object> act1 = new Action<int, int, object>(delegate (int idx, int sidx, object obj)
                        {
                            list_Items.Invoke(new Action(() => list_Items.Items[idx].SubItems[sidx].Text = obj.ToString()));



                        });

                        d.eSize += (object s1, string size) => act1.Invoke(indx, 1, size);
                        d.eDownloadedSize += (object s1, string size) => act1.Invoke(indx, 2, size);
                        d.eSpeed += (object s1, string size) => act1.Invoke(indx, 3, size);
                        d.eDownloadState += (object s1, string size) => act1.Invoke(indx, 4, size);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                btndownload.Enabled = true;
                btnPause.Enabled = false;

            }

        }




        /*
        public void DownloadFile(string urlAddress, string location)
        {
            using (webClient = new WebClient())
            {
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

                // The variable that will be holding the url address (making sure it starts with http://)
                Uri URL = urlAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("http://" + urlAddress);

                // Start the stopwatch which we will be using to calculate the download speed
                sw.Start();

                try
                {
                    // Start downloading the file
                    webClient.DownloadFileAsync(URL, location);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        // The event that will fire whenever the progress of the WebClient is changed
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // Calculate download speed and output it to labelSpeed.
            //   lbldownloadinfo.Text = string.Format("{0} kb/s | {1}% | {2} MB's / {3} MB's",
            //      (e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds).ToString("0.00"), e.ProgressPercentage,
            ///     View.Size.getlength.GetLengthString(Math.Round(Convert.ToSingle(e.BytesReceived))),
            //     View.Size.getlength.GetLengthString(Math.Round(Convert.ToSingle(e.TotalBytesToReceive))));

            // Update the progressbar percentage only when the value is not the same.
            //   pbdown.Value = e.ProgressPercentage;

        }

        // The event that will trigger when the WebClient is completed
        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            // Reset the stopwatch.
            sw.Reset();

            if (e.Cancelled)
            {
                btndownload.Enabled = true;
                btnPause.Enabled = false;
                //   lbldownloadinfo.Text = "Download Cancelled!";
            }
            else
            {

                btndownload.Enabled = true;
                btnPause.Enabled = false;
                //   lbldownloadinfo.Text = "--/--";
                var ext = video.Resolution.ToString();
                if (ext == "0")
                    File.Move(Path.Combine(savePath, name + ".tubedl"), Path.Combine(savePath, name + ".Mp3"));
                else
                    File.Move(Path.Combine(savePath, name + ".tubedl"), Path.Combine(savePath, name + video.VideoExtension));
            }
        }
        */
        private void Main_Load(object sender, EventArgs e)
        {
#if DEBUG
            txtlink.Text = "https://www.youtube.com/watch?v=xsXectQvo6o";
#endif
            Activate();
            backgroundWorker1.RunWorkerAsync(list_Items);
            Text = Application.ProductName + " " + Application.ProductVersion;
            btndownload.Enabled = false;
            DirectoryInfo s = new DirectoryInfo(savePath = lblpath.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos));
            foreach (FileInfo f in s.GetFiles("*.tubedl"))
            {
                f.Delete();
            }
            cmbQuality.SelectedIndex = 1;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                savePath = folderBrowserDialog1.SelectedPath;
                lblpath.Text = savePath;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //  lbldownloadinfo.Text = "--/--";
            // cmbQuality.SelectedIndex = 1;
            try
            {
                if (string.IsNullOrWhiteSpace(txtlink.Text))
                {
                    btndownload.Enabled = false;
                    return;
                }
                else
                {
                    //    pbdown.Value = 0;
                    // Our test youtube link
                    string link = txtlink.Text.Trim();

                    /*
                     * Get the available video formats.
                     * We'll work with them in the video and audio download examples.
                     */
                    IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link, false);

                    /*
                       * download selected quality video or extract audio
                       */

                    if (cmbQuality.SelectedItem.ToString().Trim().Contains("Extract Audio"))
                    {
                        video = videoInfos.First(info => info.VideoType == VideoType.Mp4 && info.Resolution == 0 && info.AudioBitrate == 128);

                    }
                    else
                    {
                        int vres;
                        switch (cmbQuality.SelectedItem.ToString().Trim())
                        {
                            case "1080p: 1920x1080 (no audio)":
                                vres = 1080;
                                break;
                            case "720p: 1280x720":
                                vres = 720;
                                break;
                            case "480p: 854x480":
                                vres = 480;
                                break;
                            case "360p: 640x360":
                                vres = 360;
                                break;
                            case "240p: 426x240":
                                vres = 240;
                                break;

                            default:
                                vres = 720;
                                break;

                                /*1080p: 1920x1080 
                                720p: 1280x720
                                480p: 854x480
                                360p: 640x360
                                240p: 426x240*/

                        }
                        video = videoInfos
                           .First(info => info.VideoType == VideoType.Mp4 && info.Resolution == vres);
                    }
                    /*
                     * If the video has a decrypted signature, decipher it
                     */
                    if (video.RequiresDecryption)
                    {
                        DownloadUrlResolver.DecryptDownloadUrl(video);
                    }

                    /*
                     * show video info
                     * */
                    lbltitle.Text = video.Title;
                    if (video.Resolution.ToString() == ("0"))
                        lblformat.Text = "Mp3";
                    else
                        lblformat.Text = "Mp4";
                    lblsize.Text = GetFileSize(new Uri(video.DownloadUrl));
                    lblquality.Text = video.Resolution.ToString();
                    lblaudio.Text = video.AudioType.ToString() + " " + video.AudioBitrate.ToString();

                    url = video.DownloadUrl;
                    name =
                       RemoveIllegalPathCharacters(video.Title);
                    btndownload.Enabled = true;
#if DEBUG
                    //     MessageBox.Show(url + "\n" + savePath + "\n" + name);

#endif
                }
            }
            catch (YoutubeParseException ex)
            {
                btndownload.Enabled = false;
                if (MessageBox.Show("Error while prase URL" + txtlink, Text, MessageBoxButtons.RetryCancel) == DialogResult.Retry)
                {
                    button1_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                btndownload.Enabled = false;
                MessageBox.Show(ex.Message);
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            var indx = list_Items.SelectedItems[0].Index;
            if (btnPause.Text == "Pause")
            {
                ldf[indx].CancelDownload();
                btnPause.Text = "Resume";

            }
            else if (btnPause.Text == "Resume")
            {
                ldf[indx].ResumeDownload();
                btnPause.Text = "Pause";
            }

            btndownload.Enabled = true;
        }

        private void txtlink_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void list_Items_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (list_Items.SelectedItems.Count > 0)
            {
                var indx = list_Items.SelectedItems[0].Index;
                if (list_Items.Items[indx].SubItems[4].Text == "Downloading")
                {
                    btnPause.Enabled = true;
                    btnPause.Text = "Pause";

                }

                else if (list_Items.Items[indx].SubItems[4].Text == "Paused")
                {
                    btnPause.Enabled = true;
                    btnPause.Text = "Resume";

                }
                else
                {
                    btnPause.Enabled = false;
                    btnPause.Text = "Pause/Resume";
                }

                if (list_Items.SelectedItems[0].SubItems[4].Text == "Completed")
                    btncle.Enabled = true;
                else if (list_Items.SelectedItems[0].SubItems[4].Text == "Paused")
                    btncle.Enabled = true;
                else
                    btncle.Enabled = false;
            }
            else
            {
                btnPause.Enabled = false;
                btnPause.Text = "Pause/Resume";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            txtlink.Clear();
        }

        bool done = false;
        ListView lst;

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                BeginInvoke((MethodInvoker)delegate
                {
                   
                    if (list_Items.Items.Count > 0)
                        foreach (ListViewItem items in list_Items.Items)
                        {
                            done = items.SubItems[4].Text == "Completed";
                            Invoke(new Action(() => items.BackColor = done ? Color.Lime : System.Drawing.SystemColors.Window ));

                        }
                    
                });
            }
            catch (Exception wx)
            {
                MessageBox.Show(wx.Message + "\n" + wx.StackTrace);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var ext = video.Resolution.ToString();
            if (ext == "0")
                ext = ".mp3";
            else
                ext = ".Mp4";

            if (list_Items.SelectedItems[0].SubItems[4].Text == "Completed")
                list_Items.SelectedItems[0].Remove();

            else if (list_Items.SelectedItems[0].SubItems[4].Text == "Paused")
            {
                ldf.RemoveAll(a => a.DownloadState == "Paused");
                File.Delete(Path.Combine(savePath, name + " " + (video.Resolution == 0 ? ext.Replace(".", "") : video.Resolution.ToString()) + ext));
                list_Items.SelectedItems[0].Remove();
            }

        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
        }
    }
}

