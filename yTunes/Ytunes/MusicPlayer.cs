using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Ytunes
{
    public class MusicPlayer
    {
        public enum PlayState : int
        {
            stoped = 0,
            playing = 1,
            paused = 2
        }
        private MediaPlayer player = null;
        private Uri musicfile;
        private PlayState state;

        public Uri MusicFile
        {
            set
            {
                musicfile = value;
            }
            get
            {
                return musicfile;
            }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public MusicPlayer()
        {
            player = new MediaPlayer();
        }
        public MusicPlayer(Uri file)
        {
            Load(file);
        }
        /// <summary>
        /// 构造函数将传入的文件路径传到Load方法中处理
        /// </summary>
        /// <param name="file"></param>
        public void Load(Uri file)
        {
            player = new MediaPlayer();
            musicfile = file;
            player.Open(musicfile);
        }
        /// <summary>
        /// 播放，暂停，以及停止函数
        /// </summary>
        public void Play()
        {
            player.Play();
            state = PlayState.playing;
        }
        public void Pause()
        {
            player.Pause();
            state = PlayState.paused;
        }
        public void Stop()
        {
            player.Stop();
            state = PlayState.stoped;
        }
        /// <summary>
        /// 获取音频文件自然持续时间
        /// </summary>
        /// <returns></returns>
        public TimeSpan GetMusicDurationTime()
        {
            while (!player.NaturalDuration.HasTimeSpan)
            {
                if (player.NaturalDuration.HasTimeSpan)
                {
                return player.NaturalDuration.TimeSpan;
                }
            }
            return player.NaturalDuration.TimeSpan;
        }
        /// <summary>
        /// 设置进度和获取进度
        /// </summary>
        /// <param name="tp"></param>
        public void SetPosition(TimeSpan tp)
        {
            player.Position = tp;
        }
        public TimeSpan GetPosition()
        {
            return player.Position;
        }
        /// <summary>
        /// 设置音量，读取音量
        /// </summary>
        /// <param name="volume"></param>
        public void SetVolume(double volume)
        {
            player.Volume = volume;
        }
        public double GetVolume()
        {
            return player.Volume;
        }
        /// <summary>
        /// 获取和设定音乐播放器的状态
        /// </summary>
        /// <returns></returns>
        public PlayState GetPlayState()
        {
            return state;
        }
        public void SetPlayState(PlayState state)
        {
            if (state == PlayState.playing)
            {
                this.Play();
            }
            else if (state == PlayState.paused)
            {
                this.Pause();
            }
            else if (state == PlayState.stoped)
            {
                this.Stop();
            }
        }
        /// <summary>
        /// 获取音乐标题，获取最后一个/之后的部分，去除文件后缀
        /// </summary>
        /// <returns></returns>
        public string GetMusicTitle()
        {
            string title = player.Source.ToString();
            return title.Substring(title.LastIndexOf("/") + 1, title.Length - title.LastIndexOf("/") - 5);
        }
    }
}
