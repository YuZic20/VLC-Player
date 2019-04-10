using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using System.Windows.Threading;

namespace VLC_Player
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<string> Paths = new List<string>();
        public int PlayingIndex;
        static string[] mediaExtensions = {".AVI", ".MP4", ".WMV", ".MOV"};


        public MainWindow()
        {
            InitializeComponent();

            Paths.Add("http://download.blender.org/peach/bigbuckbunny_movies/big_buck_bunny_480p_h264.mov");
            


            var currentDirectory = Directory.GetCurrentDirectory();
            

            var vlcLibDirectory = new DirectoryInfo(System.IO.Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));

            var options = new string[]
            {
                // VLC options can be given here. Please refer to the VLC command line documentation.
            };

            this.MyControl.SourceProvider.CreatePlayer(vlcLibDirectory, options);

            // Load libvlc libraries and initializes stuff. It is important that the options (if you want to pass any) and lib directory are given before calling this method.
            this.MyControl.SourceProvider.MediaPlayer.Play(Paths[0]);
            PlayingIndex = 0;

            PrintPaths(Paths);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();

        }

        private void Button_Click_Reload(object sender, RoutedEventArgs e)
        {


            VideoReload();

        }
        void VideoReload()
        {
            ChangeVideo(PlayingIndex);
        }
        private void Button_Click_backwards(object sender, RoutedEventArgs e)
        {
            
            if (this.MyControl.SourceProvider.MediaPlayer.Time - 5000 < 0)
            {
                this.MyControl.SourceProvider.MediaPlayer.Time = 0;
            }
            else
            {
                this.MyControl.SourceProvider.MediaPlayer.Time-= 5000;
            }
            
            
        }
        private void Button_Click_Play_And_Stop(object sender, RoutedEventArgs e)
        {
            if (this.MyControl.SourceProvider.MediaPlayer.IsPlaying())
            {
                this.MyControl.SourceProvider.MediaPlayer.Pause();

            }
            else
            {
                this.MyControl.SourceProvider.MediaPlayer.Play();
            }
            
        }
        
        private void Button_Click_forward(object sender, RoutedEventArgs e)
        {
            
            if (this.MyControl.SourceProvider.MediaPlayer.Time + 30000 >= this.MyControl.SourceProvider.MediaPlayer.Length)
            {
                this.MyControl.SourceProvider.MediaPlayer.Time = this.MyControl.SourceProvider.MediaPlayer.Length;
            }
            else
            {
                this.MyControl.SourceProvider.MediaPlayer.Time += 30000;
            }

            
        }
        private void Button_Click_Back(object sender, RoutedEventArgs e)
        {
            
            if(PlayingIndex -1 < 0)
            {
                PlayingIndex = Paths.Count()-1;
                ChangeVideo(PlayingIndex);
            }
            else
            {
                PlayingIndex--;
                ChangeVideo(PlayingIndex);
            }
            PrintPaths(Paths);
        }
        private void Button_Click_Next(object sender, RoutedEventArgs e)
        {
            NextVideo();
        }
        public void NextVideo()
        {
            if (PlayingIndex + 1 > Paths.Count() - 1)
            {
                PlayingIndex = 0;
                ChangeVideo(PlayingIndex);
            }
            else
            {
                PlayingIndex++;
                ChangeVideo(PlayingIndex);
            }
            PrintPaths(Paths);
        }
        private void Button_Click_Add_Path(object sender, RoutedEventArgs e)
        {
            AddFile(Path_Input.Text);
            PrintPaths(Paths);
        }

        private void Button_Click_Remove(object sender, RoutedEventArgs e)
        {
            if (Paths.Count() == 1)
            {
                
                Paths.RemoveAt(PlayingIndex);
                PlayingIndex = 0;

                if (this.MyControl.SourceProvider.MediaPlayer.IsPlaying())
                {
                    this.MyControl.SourceProvider.MediaPlayer.Play("");

                }
            }else if(Paths.Count() == 0)
            {

            }
            else
            {
                Paths.RemoveAt(PlayingIndex);
                

                NextVideo();
            }
            PrintPaths(Paths);

        }

        public void ChangeVideo(int Index)
        {
            if (Paths.Count() == 0)
            {
                return;
            }

            if (IsLocalPath(Paths[Index]))
            {
                FileInfo file = new FileInfo(Paths[Index]);
                this.MyControl.SourceProvider.MediaPlayer.Play(file);
            }
            else
            {
                this.MyControl.SourceProvider.MediaPlayer.Play(Paths[Index]);
            }

                       
            
        }

        bool IsLocalPath(string p)
        {
            if (p.StartsWith("http"))
            {
                return false;
            }
            if (File.Exists(p))
            {
                return new Uri(p).IsFile;
            }
            return false;
        }

        void AddFile(string path)
        {
            if(IsLocalPath(path))
            {
                if (IsMediaFile(path))
                {
                    Paths.Add(path);
                }
                if (Paths.Count() == 1)
                {
                    VideoReload();
                }

            }
            if (path.StartsWith("http"))
            {
                if (IsMediaFile(path))
                {
                    Paths.Add(path);
                }
                if (Paths.Count() == 1)
                {
                    VideoReload();
                }
            }
            
        }
        static bool IsMediaFile(string path)
        {
            return -1 != Array.IndexOf(mediaExtensions, System.IO.Path.GetExtension(path).ToUpperInvariant());
        }
        void PrintPaths(List<string>PathsToPrint)
        {
            Paths_Display.Children.Clear();
            for (int i =0;i<PathsToPrint.Count();i++)
            {
                Label label = new Label();
                label.Content = PathsToPrint[i];
                if(i == PlayingIndex)
                {
                    label.Foreground = System.Windows.Media.Brushes.Cyan;
                }
                Paths_Display.Children.Add(label);
            }
        }
        

        void timer_Tick(object sender, EventArgs e)
        {
            Slider.Maximum = this.MyControl.SourceProvider.MediaPlayer.Length;
            Slider.Value = this.MyControl.SourceProvider.MediaPlayer.Time;  
            
            
        }

        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            this.MyControl.SourceProvider.MediaPlayer.Time = (int)Math.Round(Slider.Value);
        }

        
    }
    
}
