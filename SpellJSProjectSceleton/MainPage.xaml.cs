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
using Newtonsoft.Json;

namespace SpellJSProjectSceleton
{
    public class Request
    {
        public string method { get; set; }
        public string param { get; set; }
    }

    public class AudioContext
    {
        public string id;
        public SoundEffectInstance effect;

        public AudioContext(string id, SoundEffect effect)
        {
            this.id = id;
            this.effect = effect.CreateInstance();
        }
    }

    public partial class MainPage : PhoneApplicationPage
    {
        // Url of Home page
        private string MainUri = "/Html/index.html";

        private List<AudioContext> sounds = new List<AudioContext>();

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

        private AudioContext FindSoundById(string id)
        {
            return this.sounds.Find(
                    delegate(AudioContext sound)
                    {
                        return sound.id == id;
                    }
                );
        }

        private void PlaySound( string id ) {
            var found = this.FindSoundById(id);

            if (found != null)
            {
                found.effect.Play();
                FrameworkDispatcher.Update();
            }
        }

        private void LoadBuffer( string src )
        {
            var tmp = "Html/" + src;
            MessageBox.Show("Loading: '" + tmp + "'");
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
                found.effect.IsLooped = true;
            }
        }

        private void SetVolume(string id, float volume)
        {
            var found = this.FindSoundById(id);

            if (found != null)
            {
                found.effect.Volume = volume;
            }
        }

        private void Browser_ScriptNotify(object sender, NotifyEventArgs e)
        {
            Request request = JsonConvert.DeserializeObject<Request>(e.Value);

            switch(request.method) {
                case "loadBuffer":
                    this.LoadBuffer(request.param);
                    break;
                case "playSound":
                    this.PlaySound(request.param);
                    break;
                case "setLoop":
//                    this.SetLoop(request.param, loop);
                    break;
                case "setVolume":
//                    this.SetVolume(request.param, volume);
                    break;
                default:
                    MessageBox.Show("Method: '" + request.method + "' is unknow");
                    break;

            }

            //using (var stream = TitleContainer.OpenStream( e.Value ))
            //{
            //    var effect = SoundEffect.FromStream(stream);
            //    FrameworkDispatcher.Update();
            //    effect.Play();
            //}
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (Browser.CanGoBack)
            {
                Browser.GoBack();
                e.Cancel = true;
            }
            else
            {
                base.OnBackKeyPress(e);
            }
        }
    }
}
