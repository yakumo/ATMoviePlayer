using System;

using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;

namespace ATMoviePlayer
{
	[Activity (
        Label = "ATMoviePlayer",
        MainLauncher = true,
        Theme = "@android:style/Theme.Light",
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.Keyboard | Android.Content.PM.ConfigChanges.KeyboardHidden | Android.Content.PM.ConfigChanges.ScreenLayout | Android.Content.PM.ConfigChanges.ScreenSize,
        Icon = "@drawable/icon")]
    public class MainActivity : Activity
	{
		ListView itemsList = null;
        ICursor cursor = null;
        int idIndex = 0;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			itemsList = (ListView)this.FindViewById (Resource.Id.listView1);
            itemsList.Clickable = true;
            itemsList.ItemClick += (s, e) =>
            {
                if (cursor.MoveToPosition(e.Position))
                {
                    long id = cursor.GetLong(idIndex);
                    DoPlay(id);
                }
            };
		}

        protected override void OnResume()
        {
            base.OnResume();

            Android.Net.Uri uri = MediaStore.Video.Media.ExternalContentUri;
            string[] columns = new string[] {
                MediaStore.Video.VideoColumns.Id,
                MediaStore.Video.VideoColumns.Data,
                MediaStore.Video.VideoColumns.DisplayName,
                MediaStore.Video.VideoColumns.Size,
                MediaStore.Video.VideoColumns.Duration,
            };

            cursor = MediaStore.Video.Query(this.ContentResolver, uri, columns);
            Log.Debug ("Test", "cursor:" + cursor);

            if (cursor != null)
            {
                idIndex = cursor.GetColumnIndex(MediaStore.Video.VideoColumns.Id);
                if (itemsList != null && cursor != null)
                {
                    itemsList.Adapter = new MediaCursorAdapter(this, cursor);
                }
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.MainMenu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            ISharedPreferences pref = GetSharedPreferences(Constants.PrefTag, FileCreationMode.Private);
            IMenuItem mi = menu.FindItem(Resource.Id.repeat_all);
            if (mi != null)
            {
                bool chk = pref.GetBoolean(Constants.PrefRepeat, false);
                mi.SetChecked(chk);
                mi.SetTitle(chk ? Resource.String.menu_repeat_checked : Resource.String.menu_repeat);
            }
            mi = menu.FindItem(Resource.Id.random_all);
            if (mi != null)
            {
                bool chk = pref.GetBoolean(Constants.PrefRandom, false);
                mi.SetChecked(chk);
                mi.SetTitle(chk ? Resource.String.menu_random_checked : Resource.String.menu_random);
            }
            mi = menu.FindItem(Resource.Id.multi_sel);
            if (mi != null)
            {
                bool chk = pref.GetBoolean(Constants.PrefMultiSel, false);
                mi.SetChecked(chk);
                mi.SetTitle(chk ? Resource.String.menu_multisel_checked : Resource.String.menu_multisel);
            }
            return base.OnPrepareOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            MediaCursorAdapter adapter = (MediaCursorAdapter)itemsList.Adapter;
            ISharedPreferences pref = GetSharedPreferences(Constants.PrefTag, FileCreationMode.Private);
            ISharedPreferencesEditor ed = pref.Edit();
            switch (item.ItemId)
            {
            case Resource.Id.play_all:
                DoPlay(null);
                break;
            case Resource.Id.repeat_all:
                ed.PutBoolean(Constants.PrefRepeat, !item.IsChecked);
                ed.Commit();
                break;
            case Resource.Id.random_all:
                ed.PutBoolean(Constants.PrefRandom, !item.IsChecked);
                ed.Commit();
                break;
            case Resource.Id.multi_sel:
                ed.PutBoolean(Constants.PrefMultiSel, !item.IsChecked);
                ed.Commit();
                adapter.NotifyDataSetChanged();
                break;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
        }

        protected override void OnPause()
        {
            base.OnPause();
        }

        private void DoPlay(long? selectedId){
            List<string> filenameList = new List<string>();
            MediaCursorAdapter adapter = (MediaCursorAdapter)itemsList.Adapter;
            List<long> idList = new List<long>(adapter.SelectedIdList);
            Android.Net.Uri uri = MediaStore.Video.Media.ExternalContentUri;
            string[] columns = new string[]
            {
                MediaStore.Video.VideoColumns.Id,
                MediaStore.Video.VideoColumns.Data,
            };
            if (selectedId != null && selectedId.HasValue)
            {
                idList.Add(selectedId.Value);
            }
            foreach (long id in idList)
            {
                ICursor c = this.ContentResolver.Query(uri, columns, MediaStore.Video.VideoColumns.Id + "=?", new string[] { id.ToString() }, null);
                if (c.MoveToFirst())
                {
                    filenameList.Add(c.GetString(1));
                }
            }
            /*
            int filenameIndex = cursor.GetColumnIndex(MediaStore.Video.VideoColumns.Data);
            SparseBooleanArray flags = itemsList.CheckedItemPositions;
            for (int i = 0; i < flags.Size() ; i++)
            {
                if (flags.ValueAt(i))
                {
                    if (cursor.MoveToPosition(i))
                    {
                        filenameList.Add(cursor.GetString(filenameIndex));
                    }
                }
            }
            */
            Intent intent = new Intent(this, typeof(MovieActivity));
            intent.PutStringArrayListExtra("filenames", filenameList);
            StartActivity(intent);
        }
	}
}
