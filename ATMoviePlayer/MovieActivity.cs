
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;

namespace ATMoviePlayer
{
    [Activity(
        Label = "MovieActivity",
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.Keyboard | Android.Content.PM.ConfigChanges.KeyboardHidden | Android.Content.PM.ConfigChanges.ScreenLayout | Android.Content.PM.ConfigChanges.ScreenSize,
        Theme = "@android:style/Theme.NoTitleBar.Fullscreen"
    )]
    public class MovieActivity : Activity, Android.Media.MediaPlayer.IOnCompletionListener
    {
        VideoView videoView = null;
        List<string> srcFileNames = null;
        List<string> playFileNames;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here
            this.SetContentView(Resource.Layout.MoviePlayer);

            videoView = (VideoView)this.FindViewById(Resource.Id.videoPreview);
            videoView.SetOnCompletionListener(this);

            IList<string> src = this.Intent.GetStringArrayListExtra("filenames");
            if (src != null)
            {
                srcFileNames = new List<string>(src);
                DoInitialize();
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            videoView.Start();
        }

        protected override void OnPause()
        {
            base.OnPause();
            videoView.Suspend();
        }

        public void OnCompletion(Android.Media.MediaPlayer mp)
        {
            mp.Stop();
            if (playFileNames.Count > 0)
            {
                string src = playFileNames[0];
                playFileNames.RemoveAt(0);
                videoView.SetVideoPath(src);
                videoView.SeekTo(0);
                videoView.Start();
            }
            else
            {
                ISharedPreferences pref = GetSharedPreferences("prefs", FileCreationMode.Private);
                bool flg = pref.GetBoolean(Constants.PrefRepeat, false);
                if (flg)
                {
                    DoInitialize();
                    videoView.Start();
                    return;
                }
                else
                {
                    Finish();
                }
            }
        }

        private void DoInitialize()
        {
            ISharedPreferences pref = GetSharedPreferences("prefs", FileCreationMode.Private);
            playFileNames = new List<string>(srcFileNames);
            bool rnd = pref.GetBoolean(Constants.PrefRandom, false);
            if (rnd)
            {
                Random r = new Random();
                for (int i = 0; i < playFileNames.Count; i++)
                {
                    int p = r.Next() % playFileNames.Count;
                    string tmp = playFileNames[i];
                    playFileNames[i] = playFileNames[p];
                    playFileNames[p] = tmp;
                }
            }
            string src = playFileNames[0];
            playFileNames.RemoveAt(0);
            videoView.SetVideoPath(src);
            videoView.SeekTo(0);
        }
    }
}

