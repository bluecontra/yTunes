using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Runtime.Serialization.Json;

namespace Ytunes
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// 
    
    public partial class MainWindow : Window
    {
        //public string defaultPath = @"F:\学习\coding\C#\Ytunes\music";
        public static string[] musicName = new string[10000];
        public static MusicPlayer play;
        public string nowTitle;
        public int count = 0;
        //0代表循环播放，1代表顺序播放，2代表随机播放
        public static int playMode = 1;
        public static int posIndex = -1;
        //堆栈记录播放过的歌曲，以及下标
        public static int[] preSongIndex = new int[10000];
        public static int preIndex = -1;
        public DispatcherTimer timer;
        //记录变往SM之前的歌曲序号，为了修改红色fg
        public int recordIndex = 0;
        public MainWindow()
        {
            InitializeComponent();
            //读取Json文件
            try
            {
                deserializeJsonAsync();
            }
            catch (Exception) { }
            //初始化modeButton
            if (playMode == 0)
            {
                Image img = modeButton.Content as Image;
                img.Source = new BitmapImage(new Uri(@"Sources/循环.png", UriKind.Relative));
            }
            else if (playMode == 2)
            {
                Image img = modeButton.Content as Image;
                img.Source = new BitmapImage(new Uri(@"Sources/随机.png", UriKind.Relative));
            }

            //初始化列表
            InitList();
            initStack();
            //复原关闭时的情况
            if (posIndex >= 0)
            {
                TextBlock p = list.Items[posIndex] as TextBlock;
                p = list.Items[posIndex] as TextBlock;
                //BrushConverter brushConverter = new BrushConverter();
                p.Foreground = new SolidColorBrush(Colors.DarkRed);
                play = new MusicPlayer(new Uri(musicName[posIndex]));
                play.SetVolume(volumnSlider.Value);
                play.Pause();
                songName.Text = play.GetMusicTitle();
                preIndex++;
                preSongIndex[preIndex] = posIndex;
                //设置进度
                double tot = play.GetMusicDurationTime().TotalSeconds;
                double pos = songProcess.Value * tot;
                play.SetPosition(new TimeSpan(0, 0, (int)pos));
            }

            //设置启动计时器
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0,0,0,0,100);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }
        /// <summary>
        /// 初始化列表
        /// </summary>
        public void InitList()
        {
            //GetMusicListUri(defaultPath);
            for (int i = 0; i < 10000; i++)
            {
                if (musicName[i] != null)
                {
                    string name = musicName[i].Substring(musicName[i].LastIndexOf("\\") + 1, musicName[i].Length - musicName[i].LastIndexOf("\\") - 5);
                    TextBlock tb = new TextBlock();
                    tb.Width = 200;
                    tb.Text = (i + 1) + ".   " + name;
                    list.Items.Add(tb);
                }
            }
        }
        /// <summary>
        /// 获取指定地点的文件名称
        /// </summary>
        /// <param name="destination"></param>
        public void GetMusicListUri(string destination)
        {
            DirectoryInfo theFolder = new DirectoryInfo(destination);
            foreach (FileInfo NextFile in theFolder.GetFiles())
            {
                if (houzhuiCheck(NextFile.FullName))
                {
                    if (!fileRepeatCheck(NextFile.FullName))
                    {
                        musicName[count] = NextFile.FullName;
                        count++;
                    }
                }
            }
            /*
            foreach (DirectoryInfo NextFolder in theFolder.GetDirectories())
                foreach (FileInfo NextFile in NextFolder.GetFiles())
                {
                    if (!fileRepeatCheck(NextFile.FullName))
                    {
                        musicName[count] = NextFile.FullName;
                        count++;
                    }
                }
             * */
        }
        /// <summary>
        /// 播放与暂停
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void playButton_Click(object sender, RoutedEventArgs e)
        {
            if(play != null)
            {
                if (play.GetPlayState() == MusicPlayer.PlayState.playing)
                {
                    play.Pause();
                    Image img = playButton.Content as Image;
                    img.Source = new BitmapImage(new Uri(@"Sources/暂停.png", UriKind.Relative));
                }
                else if (play.GetPlayState() == MusicPlayer.PlayState.paused)
                {
                    play.Play();
                    Image img = playButton.Content as Image;
                    img.Source = new BitmapImage(new Uri(@"Sources/播放.png", UriKind.Relative));
                }
            }
            else
            {
                if (GetSongNum() > 0)
                {
                    posIndex = 0;
                    play = new MusicPlayer(new Uri(musicName[posIndex]));
                    play.SetVolume(volumnSlider.Value);
                    songName.Text = play.GetMusicTitle();
                    play.Play();
                    //播放记录
                    preIndex++;
                    preSongIndex[preIndex] = posIndex;

                    TextBlock p = list.Items[posIndex] as TextBlock;
                    p.Foreground = new SolidColorBrush(Colors.DarkRed);
                    Image img = playButton.Content as Image;
                    img.Source = new BitmapImage(new Uri(@"Sources/播放.png", UriKind.Relative));
                }
                else
                    MessageBox.Show("没有歌曲播放...");
            }
        }
        /// <summary>
        /// 双击播放
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void list_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock p = new TextBlock();
            if (play != null)
            {
                play.Stop();
                p = list.Items[posIndex] as TextBlock;
                p.Foreground = new SolidColorBrush(Colors.Cornsilk);
            }

            Image img = playButton.Content as Image;
            img.Source = new BitmapImage(new Uri(@"Sources/播放.png", UriKind.Relative));
            ListBox ls = sender as ListBox;
            ListBoxItem temp = ls.ItemContainerGenerator.ContainerFromItem(ls.SelectedItem) as ListBoxItem;
            TextBlock tp = temp.Content as TextBlock;
            //string name = temp.Content.ToString();
            string name = tp.Text;
            string index = name.Substring(0, name.IndexOf("."));
            posIndex = int.Parse(index) - 1;
            p = list.Items[posIndex] as TextBlock;
            p.Foreground = new SolidColorBrush(Colors.DarkRed);
            play = new MusicPlayer(new Uri(musicName[posIndex]));
            play.SetVolume(volumnSlider.Value);
            songName.Text = play.GetMusicTitle();
            play.Play();
            //播放记录
            preIndex++;
            preSongIndex[preIndex] = posIndex;
        }
        /// <summary>
        /// 下一曲与上一曲
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if(play != null)
            {
                if (play.GetPlayState() != MusicPlayer.PlayState.stoped)
                    play.Stop();
                //循环播放的下一曲
                if (playMode == 0)
                {
                    playMode = 1;
                    nextSongPlay();
                    playMode = 0;
                }
                else
                    nextSongPlay();
                //
                preIndex++;
                preSongIndex[preIndex] = posIndex;

                Image img = playButton.Content as Image;
                img.Source = new BitmapImage(new Uri(@"Sources/播放.png", UriKind.Relative));
            }
            else
            {
                if (GetSongNum() > 0)
                {
                    posIndex = 0;
                    TextBlock p = list.Items[posIndex] as TextBlock;
                    p.Foreground = new SolidColorBrush(Colors.DarkRed);
                    play = new MusicPlayer(new Uri(musicName[posIndex]));
                    songName.Text = play.GetMusicTitle();
                    play.SetVolume(volumnSlider.Value);
                    play.Play();
                    //
                    preIndex++;
                    preSongIndex[preIndex] = posIndex;

                    Image img = playButton.Content as Image;
                    img.Source = new BitmapImage(new Uri(@"Sources/播放.png", UriKind.Relative));
                }
                else
                    MessageBox.Show("没有歌曲播放...");
            }
        }
        private void preButton_Click(object sender, RoutedEventArgs e)
        {
            Image img = playButton.Content as Image;
            img.Source = new BitmapImage(new Uri(@"Sources/播放.png", UriKind.Relative));
            TextBlock p = new TextBlock();
            if (play != null)
            {
                p = list.Items[posIndex] as TextBlock;
                p.Foreground = new SolidColorBrush(Colors.Cornsilk);
                if (play.GetPlayState() != MusicPlayer.PlayState.stoped)
                    play.Stop();
                
                //上一曲目
                do{
                    preIndex--;
                }while(preIndex >=0 && preSongIndex[preIndex] == -1);
                if (preIndex >= 0)
                {
                    posIndex = preSongIndex[preIndex];
                    //play = new MusicPlayer(new Uri(musicName[posIndex]));
                    //play.Play();
                }
                else
                {
                    if(playMode == 2)
                        posIndex = GetRandomPos(posIndex);
                    else
                    {
                        posIndex--;
                        if (posIndex < 0)
                            posIndex = GetSongNum() - 1;
                    }

                }
                play = new MusicPlayer(new Uri(musicName[posIndex]));
                play.Play();
                songName.Text = play.GetMusicTitle();
                //}
                p = list.Items[posIndex] as TextBlock;
                p.Foreground = new SolidColorBrush(Colors.DarkRed);
            }
            else
            {
                MessageBox.Show("没有播放的歌曲...");
            }
            play.SetVolume(volumnSlider.Value);
        }
        /// <summary>
        /// 音量控制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void volumnSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (play != null)
            {
                double volume = volumnSlider.Value;
                play.SetVolume(volume);
                //B.Text = "      " + (int)(play.GetVolume() * 100);
            }

        }
        private void volumnSlider_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (play != null)
            {
                double volume = volumnSlider.Value;
                play.SetVolume(volume);
                //B.Text = "      " + (int)(play.GetVolume() * 100);
            }
        }
        /// <summary>
        /// 跟踪歌曲进度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void timer_Tick(object sender, EventArgs e)
        {
            if (play != null)
            {
                string posStr = play.GetPosition().ToString();
                string pos4 = posStr.Substring(posStr.IndexOf(":") + 1, 5);
                string durStr = play.GetMusicDurationTime().ToString();
                string durStr4 = durStr.Substring(durStr.IndexOf(":") + 1, 5);
                posTextBlock.Text = pos4;
                durTextBlock.Text = durStr4;
                double posSec = play.GetPosition().TotalSeconds;
                double totSec = play.GetMusicDurationTime().TotalSeconds;
                songProcess.Value = (posSec / totSec);
                if (posSec >= totSec)
                {
                    nextSongPlay();
                    //
                    preIndex++;
                    preSongIndex[preIndex] = posIndex;
                }
            }
        }
        /// <summary>
        /// 拖动改变歌曲进度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void songProcess_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (play != null)
                {
                    double tot = play.GetMusicDurationTime().TotalSeconds;
                    double pos = songProcess.Value * tot;
                    play.SetPosition(new TimeSpan(0, 0, (int)pos));
                }
            }

        }
        /// <summary>
        /// 切换模式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void modeButton_Click(object sender, RoutedEventArgs e)
        {
            playMode = (playMode + 1) % 3;
            if (playMode == 0)
            {
                Image img = modeButton.Content as Image;
                img.Source = new BitmapImage(new Uri(@"Sources/循环.png", UriKind.Relative));
            }
            else if(playMode == 1)
            {
                Image img = modeButton.Content as Image;
                img.Source = new BitmapImage(new Uri(@"Sources/顺序.png", UriKind.Relative));
            }
            else
            {
                Image img = modeButton.Content as Image;
                img.Source = new BitmapImage(new Uri(@"Sources/随机.png", UriKind.Relative));
            }
        }
        /// <summary>
        /// 根据不同播放模式播放下一曲目
        /// </summary>
        public void nextSongPlay()
        {
            TextBlock p = list.Items[posIndex] as TextBlock;
            p.Foreground = new SolidColorBrush(Colors.Cornsilk);
            if (playMode == 2)
            {
                posIndex = GetRandomPos(posIndex);
            }
            else if (playMode == 1)
            {
                posIndex = (posIndex + 1) % GetSongNum();
            }
            p = list.Items[posIndex] as TextBlock;
            //BrushConverter brushConverter = new BrushConverter();
            p.Foreground = new SolidColorBrush(Colors.DarkRed);
            play = new MusicPlayer(new Uri(musicName[posIndex]));
            play.SetVolume(volumnSlider.Value);
            play.Play();
            songName.Text = play.GetMusicTitle();
        }
        /// <summary>
        /// 辅助函数，返回数组歌曲数
        /// </summary>
        /// <returns></returns>
        public static int GetSongNum()
        {
            int i = 0;
            for (; i < 10000; i++)
            {
                if (musicName[i] == null)
                    break;
            }
            return i;
        }
        /// <summary>
        /// 辅助函数，返回一个与当前播放曲目序号不同的曲目序号
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetRandomPos(int key)
        {
            Random ran = new Random();
            int temp;
            do
            {
                temp = ran.Next() % GetSongNum();
            } while (temp == key);
            return temp;
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
        /// 关闭按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void shutDownButton_Click(object sender, RoutedEventArgs e)
        {
            writeJsonAsync();
            Application.Current.Shutdown();
        }
        /// <summary>
        /// 最小化按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void smallWindowButton_Click(object sender, RoutedEventArgs e)
        {
            //this.Hide();
            WindowState = WindowState.Minimized;
        }
        /// <summary>
        /// 添加列表文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Title = "请选择需要添加的文件";
            dialog.Filter = "MP3文件|*.mp3|M4A文件|*.m4a|WMA文件|*.wma";
            if (dialog.ShowDialog() == true)
            {
                int i = GetSongNum();
                bool repeatResult = fileRepeatCheck(dialog.FileName);

                if (repeatResult == true)
                    MessageBox.Show("该歌曲已在列表中。");
                else
                {
                    musicName[i] = dialog.FileName;
                    string name = musicName[i].Substring(musicName[i].LastIndexOf("\\") + 1, musicName[i].Length - musicName[i].LastIndexOf("\\") - 5);
                    TextBlock tb = new TextBlock();
                    tb.Width = 200;
                    tb.Text = (i + 1) + ".   " + name;
                    list.Items.Add(tb);
                }
            }
            //writeJsonAsync();
        }
        /// <summary>
        /// 添加文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addFromDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            //MessageBox.Show(fbd.ShowDialog().ToString());
            
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (fbd.SelectedPath != null)
                {
                    int preNum = GetSongNum();
                    string pathSelected = fbd.SelectedPath;
                    GetMusicListUri(pathSelected);
                    int nowNum = GetSongNum();
                    for (int j = preNum; j < nowNum; j++)
                    {
                        string name = musicName[j].Substring(musicName[j].LastIndexOf("\\") + 1, musicName[j].Length - musicName[j].LastIndexOf("\\") - 5);
                        TextBlock tb = new TextBlock();
                        tb.Width = 200;
                        tb.Text = (j + 1) + ".   " + name;
                        list.Items.Add(tb);
                    }
                }
            }
            
            //writeJsonAsync();
        }
        /// <summary>
        /// 重复文件查询
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public bool fileRepeatCheck(string n)
        {
            bool rep = false;
            int i = GetSongNum();
            for (int j = 0; j < i; j++)
            {
                if (musicName[j] == n)
                {
                    rep = true;
                    break;
                }
            }
            return rep;
        }
        /// <summary>
        /// 后缀检查
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public bool houzhuiCheck(string n)
        {
            bool res = false;
            try
            {
                string hz = n.Substring(n.LastIndexOf("."), n.Length - n.LastIndexOf("."));
                if (hz == ".mp3" || hz == ".m4a" || hz == ".wma")
                    res = true;
            }
            catch (Exception)
            {

            }
            return res;
        }
        /// <summary>
        /// simpleMode切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void simpleModeWindowButton_Click(object sender, RoutedEventArgs e)
        {
            recordIndex = posIndex;
            TextBlock p = list.Items[recordIndex] as TextBlock;
            p.Foreground = new SolidColorBrush(Colors.Cornsilk);
            simpleModeWindow SMW = new simpleModeWindow();
            this.Hide();

            if (SMW.ShowDialog() == true)
            {
                songName.Text = play.GetMusicTitle();
                p = list.Items[posIndex] as TextBlock;
                p.Foreground = new SolidColorBrush(Colors.DarkRed);
                this.Show();
                //保持两个界面播放按钮的一致
                if (play.GetPlayState() == MusicPlayer.PlayState.playing)
                {
                    Image img = playButton.Content as Image;
                    img.Source = new BitmapImage(new Uri(@"Sources/播放.png", UriKind.Relative));
                }
                else if (play.GetPlayState() == MusicPlayer.PlayState.paused)
                {
                    Image img = playButton.Content as Image;
                    img.Source = new BitmapImage(new Uri(@"Sources/暂停.png", UriKind.Relative));
                }

            }
        }
        /// <summary>
        /// Json文件的读写
        /// </summary>
        public void writeJsonAsync()
        {
            List<string> data = new List<string>();
            for (int i = 0; i < GetSongNum(); i++)
            {
                data.Add(musicName[i]);
            }
            //
            string songInfo = posIndex.ToString();
            string playModeInfo = playMode.ToString();
            string songPositionInfo = songProcess.Value.ToString();
            data.Add(songInfo);
            data.Add(playModeInfo);
            data.Add(songPositionInfo);
            //序列化
            var serializer = new DataContractJsonSerializer(typeof(List<string>));
            using (var stream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + @"\myData.json", FileMode.Create))
            {
                serializer.WriteObject(stream, data);
            }
        }
        public void deserializeJsonAsync()
        {
            var jsonSerializer = new DataContractJsonSerializer(typeof(List<string>));
            var myData = new List<string>();

            try
            {
                var myStream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + @"\myData.json", FileMode.Open);
                myData = (List<string>)jsonSerializer.ReadObject(myStream);
            }
            catch (Exception)
            {
                
            }
            
            for ( int i = 0; i < myData.Count - 3; i++)
                musicName[i] = myData[i];
            posIndex = int.Parse(myData[myData.Count - 3]);
            playMode = int.Parse(myData[myData.Count - 2]);
            songProcess.Value = double.Parse(myData[myData.Count - 1]);
        }
        private void deleteSong_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem temp = list.ItemContainerGenerator.ContainerFromItem(list.SelectedItem) as ListBoxItem;
            TextBlock tp = temp.Content as TextBlock;
            string name = tp.Text;
            int index = int.Parse(name.Substring(0, name.IndexOf("."))) - 1;
            //list.Items.RemoveAt(index);
            
            for (int k = index; k < GetSongNum(); k++)
            {
                musicName[k] = musicName[k + 1];
            }
            stackFix(index);
            list.Items.Clear();
            InitList();
            
            if (posIndex == index)
            {
                play.Stop();
                TextBlock p = list.Items[posIndex] as TextBlock;
                p = list.Items[posIndex] as TextBlock;
                p.Foreground = new SolidColorBrush(Colors.DarkRed);
                play = new MusicPlayer(new Uri(musicName[posIndex]));
                play.SetVolume(volumnSlider.Value);
                play.Play();
                preIndex++;
                preSongIndex[preIndex] = posIndex;
                songName.Text = play.GetMusicTitle();
            }
            else if (posIndex > index)
            {
                posIndex--;
                TextBlock p = list.Items[posIndex] as TextBlock;
                p = list.Items[posIndex] as TextBlock;
                p.Foreground = new SolidColorBrush(Colors.DarkRed);
            }
            else
            {
                TextBlock p = list.Items[posIndex] as TextBlock;
                p = list.Items[posIndex] as TextBlock;
                p.Foreground = new SolidColorBrush(Colors.DarkRed);
            }
            

        }
        /// <summary>
        /// 初始化preSongIndex[]使每个元素等于-2，代表没有歌曲
        /// </summary>
        public void initStack()
        {
            for (int i = 0; i < 10000; i++)
                preSongIndex[i] = -2;
        }
        /// <summary>
        /// 堆栈调整
        /// </summary>
        public void stackFix(int key)
        {
            for (int i = 0; preSongIndex[i] != -2 && i < 10000; i++)
            {
                if (preSongIndex[i] == key)
                    preSongIndex[i] = -1;
                else if (preSongIndex[i] > key)
                    preSongIndex[i]--;
            }
        }
    }
}
