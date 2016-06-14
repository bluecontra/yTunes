using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Ytunes
{
    /// <summary>
    /// simpleModeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class simpleModeWindow : Window
    {
        public DispatcherTimer timer;
        public simpleModeWindow()
        {
            InitializeComponent();
            if (MainWindow.play != null)
            {
                songName.Text = MainWindow.play.GetMusicTitle();
                if(MainWindow.play.GetPlayState() == MusicPlayer.PlayState.playing)
                {
                    Image img = playButton.Content as Image;
                    img.Source = new BitmapImage(new Uri(@"Sources/播放.png", UriKind.Relative));
                }
            }
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        private void backWindowButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
        /// <summary>
        /// 窗体拖动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        /// <summary>
        /// SM下的播放暂停
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.play != null)
            {
                if (MainWindow.play.GetPlayState() == MusicPlayer.PlayState.playing)
                {
                    MainWindow.play.Pause();
                    Image img = playButton.Content as Image;
                    img.Source = new BitmapImage(new Uri(@"Sources/暂停.png", UriKind.Relative));
                }
                else if (MainWindow.play.GetPlayState() == MusicPlayer.PlayState.paused)
                {
                    MainWindow.play.Play();
                    Image img = playButton.Content as Image;
                    img.Source = new BitmapImage(new Uri(@"Sources/播放.png", UriKind.Relative));
                }
            }
            else
            {
                //
                MainWindow.posIndex = 0;
                MainWindow.play = new MusicPlayer(new Uri(MainWindow.musicName[MainWindow.posIndex]));
                MainWindow.play.Play();
                songName.Text = MainWindow.play.GetMusicTitle();

                MainWindow.preIndex++;
                MainWindow.preSongIndex[MainWindow.preIndex] = MainWindow.posIndex;

                Image img = playButton.Content as Image;
                img.Source = new BitmapImage(new Uri(@"Sources/播放.png", UriKind.Relative));
            }
        }
        /// <summary>
        /// SM下的下一首
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.play != null)
            {
                if (MainWindow.play.GetPlayState() != MusicPlayer.PlayState.stoped)
                    MainWindow.play.Stop();
                if (MainWindow.playMode == 2)
                {
                    MainWindow.posIndex = GetRandomPos(MainWindow.posIndex);
                }
                else
                {
                    MainWindow.posIndex = (MainWindow.posIndex + 1) % MainWindow.GetSongNum();
                }
            }
            else
                MainWindow.posIndex = 0;
            MainWindow.play = new MusicPlayer(new Uri(MainWindow.musicName[MainWindow.posIndex]));
            MainWindow.play.Play();
            songName.Text = MainWindow.play.GetMusicTitle();

            MainWindow.preIndex++;
            MainWindow.preSongIndex[MainWindow.preIndex] = MainWindow.posIndex;
            
            Image img = playButton.Content as Image;
            img.Source = new BitmapImage(new Uri(@"Sources/播放.png", UriKind.Relative));
        }
        /// <summary>
        /// 获取随机歌曲，与MainWindow中的相同
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetRandomPos(int key)
        {
            Random ran = new Random();
            int temp;
            do
            {
                temp = ran.Next() % MainWindow.GetSongNum();
            } while (temp == key);
            return temp;
        }
        /// <summary>
        /// SM下的上一首
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void preButton_Click(object sender, RoutedEventArgs e)
        {
            Image img = playButton.Content as Image;
            img.Source = new BitmapImage(new Uri(@"Sources/播放.png", UriKind.Relative));
            
            if (MainWindow.play != null)
            {
                if (MainWindow.play.GetPlayState() != MusicPlayer.PlayState.stoped)
                    MainWindow.play.Stop();
                if (MainWindow.playMode != 2)
                {
                    MainWindow.posIndex--;
                    if (MainWindow.posIndex < 0)
                    {
                        MainWindow.posIndex = MainWindow.GetSongNum() - 1;
                    }
                }
                //随机播放的上一曲目
                else
                {
                    MainWindow.preIndex--;
                    if (MainWindow.preIndex >= 0)
                    {
                        MainWindow.posIndex = MainWindow.preSongIndex[MainWindow.preIndex];
                    }
                    else
                    {
                        MainWindow.posIndex = GetRandomPos(MainWindow.posIndex);
                    } 
                }
            }
            else
                MainWindow.posIndex = 0;

            MainWindow.play = new MusicPlayer(new Uri(MainWindow.musicName[MainWindow.posIndex]));
            songName.Text = MainWindow.play.GetMusicTitle();
            MainWindow.play.Play();
            songName.Text = MainWindow.play.GetMusicTitle();
        }
        /// <summary>
        /// 保持歌名与歌曲的一致
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void timer_Tick(object sender, EventArgs e)
        {
            if (MainWindow.play != null)
            {
                if (MainWindow.play.GetMusicTitle() != songName.Text)
                {
                    songName.Text = MainWindow.play.GetMusicTitle();
                }
            }
        }
    }
}
