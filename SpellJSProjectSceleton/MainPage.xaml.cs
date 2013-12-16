using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Phone.Tasks;

namespace SpellJSProjectSceleton
{
    public class AudioContext
    {
        public string src;
        public SoundEffect effect;

        public AudioContext(string src, SoundEffect effect)
        {
            this.src = src;
            this.effect = effect;
        }
    }

    public class PlayingSound
    {
        public string id;
        public SoundEffectInstance soundInstance;
        public float volume;
        public bool loop;

        public PlayingSound(string id, AudioContext context, float volume = 1, bool loop = false)
        {
            this.id = id;
            this.soundInstance = context.effect.CreateInstance();
            this.soundInstance.Volume = volume;
            this.soundInstance.IsLooped = loop;
            this.soundInstance.Play();
        }        
    }

    public partial class MainPage : PhoneApplicationPage
    {
        // Url of Home page
        private string MainUri = "/Html/index.html";
        private List<AudioContext> sounds = new List<AudioContext>();
        private List<PlayingSound> playingSounds = new List<PlayingSound>();

        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void Browser_Loaded(object sender, RoutedEventArgs e)
        {
            Browser.IsScriptEnabled = true;
            // Add your URL here
            Browser.Navigate(new Uri(MainUri, UriKind.Relative));
        }

        // Handle navigation failures.
        private void Browser_NavigationFailed(object sender, System.Windows.Navigation.NavigationFailedEventArgs e)
        {
            MessageBox.Show("Navigation to this page failed, check your internet connection");
        }

        private PlayingSound FindSoundById(string id)
        {
            return this.playingSounds.Find(
                delegate(PlayingSound sound)
                {
                    return sound.id == id;
                }
            );
        }

        private void PlaySound( string id, string src, float volume, bool loop ) {
            var found = this.FindSoundById(id);

            if (found != null)
            {
                FrameworkDispatcher.Update();
                found.soundInstance.Play();
            }
            else
            {
                AudioContext context = this.sounds.Find(
                    delegate(AudioContext sound)
                    {
                        return sound.src == src;
                    }
                );

                FrameworkDispatcher.Update();
                PlayingSound playingSound = new PlayingSound(id, context, volume, loop);
                playingSounds.Add(playingSound);
            }

            this.cleanUp();
        }

        private static bool endedSounds(PlayingSound sound)
        {
            return sound.soundInstance.State != SoundState.Playing;
        }

        public async void cleanUp()
        {
            this.playingSounds.RemoveAll(endedSounds);
        }

        private void LoadBuffer( string src )
        {
            var tmp = "Html/" + src;

            using (var stream = TitleContainer.OpenStream(tmp))
            {
                var effect = SoundEffect.FromStream(stream);
                this.sounds.Add(new AudioContext(src, effect));
                FrameworkDispatcher.Update();
            }
            
        }

        private void SetLoop(string id, bool loop)
        {
            var found = this.FindSoundById(id);

            if (found != null)
            {
                found.soundInstance.IsLooped = true;
            }
        }

        private void SetVolume(string id, float volume)
        {
            var found = this.FindSoundById(id);

            if (found != null)
            {
                found.soundInstance.Volume = volume;
            }
        }

        private void StopSound(string id)
        {
            var found = this.FindSoundById(id);

            if (found != null)
            {
                found.soundInstance.Stop();
                this.playingSounds.Remove(found);
            }
        }

        private void StopAll()
        {
            this.playingSounds.ForEach(
                delegate( PlayingSound sound )
                {
                    sound.soundInstance.Volume = 0;
                }
            );
        }

        private void ResumeAll()
        {
            this.playingSounds.ForEach(
                delegate(PlayingSound sound)
                {
                    sound.soundInstance.Volume = 1;
                }
            );
        }

        private void OpenUrl(string url)
        {
            WebBrowserTask wbt = new WebBrowserTask();
            wbt.URL = url;
            wbt.Show();
        }

        private void EndApplication()
        {
            Application.Current.Terminate();
        }

        private void Browser_ScriptNotify(object sender, NotifyEventArgs e)
        {
            string[] args = e.Value.Split(';');
            string method = args[0];

            switch (method)
            {
                case "loadBuffer":
                    this.LoadBuffer(args[1]);
                    break;
                case "playSound":
                    this.PlaySound(args[1], args[2], float.Parse(args[3]), bool.Parse(args[4]));
                    break;
                case "setLoop":
                    this.SetLoop(args[1], bool.Parse(args[2]) );
                    break;
                case "setVolume":
                    this.SetVolume(args[1], float.Parse(args[2]) );
                    break;
                case "stopAll":
                    this.StopAll();
                    break;
                case "stopSound":
                    this.StopSound(args[1]);
                    break;
                case "resumeAll":
                    this.ResumeAll();
                    break;
                case "openUrl":
                    this.OpenUrl(args[1]);
                    break;
                case "endApplication":
                    this.EndApplication();
                    break;
                default:
                    MessageBox.Show("Method: '" + method + "' is unknow");
                    break;

            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            Browser.InvokeScript("backButtonPressed", "230", "keydown");
            Browser.InvokeScript("backButtonPressed", "230", "keyup");
            e.Cancel = true;
        }
    }
}
