using Android.Widget;
using Android.App;
using Android.Database;
using Android.Content;
using Android.Util;
using Android.Provider;
using Android.Graphics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace ATMoviePlayer
{
    public class MediaCursorAdapter : CursorAdapter
	{
		Activity activity;
        int idIndex = 0;
        int displayNameIndex = 2;
        List<long> selectedItems = new List<long>();

        public MediaCursorAdapter (Activity context, ICursor cursor)
            : base(context, cursor, true)
		{
			activity = context;

            idIndex = cursor.GetColumnIndex(MediaStore.Video.VideoColumns.Id);
            displayNameIndex = cursor.GetColumnIndex(MediaStore.Video.VideoColumns.DisplayName);
		}

        public List<long> SelectedIdList {
            get {
                return selectedItems;
            }
        }

        public override void NotifyDataSetChanged()
        {
            selectedItems.Clear();
            base.NotifyDataSetChanged();
        }

        #region implemented abstract members of CursorAdapter

        public override void BindView(Android.Views.View view, Context context, ICursor cursor)
        {
            long id = cursor.GetLong(idIndex);
            ISharedPreferences pref = activity.GetSharedPreferences(Constants.PrefTag, FileCreationMode.Private);
            bool chk = pref.GetBoolean(Constants.PrefMultiSel, false);
            CheckBox sel = (CheckBox)view.FindViewById(Resource.Id.selectedCheckBox);
            if (sel != null)
            {
                sel.Tag = id;
                sel.Checked = selectedItems.Contains(id);
                if (chk)
                {
                    sel.Visibility = Android.Views.ViewStates.Visible;
                    sel.Enabled = true;
                }
                else
                {
                    sel.Visibility = Android.Views.ViewStates.Gone;
                    sel.Enabled = false;
                }
            }

            string title = cursor.GetString(displayNameIndex);
            TextView tv = (TextView)view.FindViewById(Resource.Id.mediaTitle);
            if (tv != null)
            {
                tv.Text = title;
            }

            BitmapFactory.Options opt = new BitmapFactory.Options();
            opt.InSampleSize = 1;
            Bitmap bmp = MediaStore.Video.Thumbnails.GetThumbnail(activity.ContentResolver, id, VideoThumbnailKind.MicroKind, opt);
            ImageView iv = (ImageView)view.FindViewById(Resource.Id.thumbImage);
            iv.SetImageBitmap(bmp);
        }

        public override Android.Views.View NewView(Context context, ICursor cursor, Android.Views.ViewGroup parent)
        {
            Android.Views.View view = activity.LayoutInflater.Inflate(Resource.Layout.MediaItem, parent, false);
            CheckBox sel = (CheckBox)view.FindViewById(Resource.Id.selectedCheckBox);
            if (sel != null)
            {
                sel.CheckedChange += (s, e) =>
                {
                    CheckBox sels = s as CheckBox;
                    long id = (long)sels.Tag;
                    if (e.IsChecked)
                    {
                        if (!selectedItems.Contains(id))
                        {
                            selectedItems.Add(id);
                        }
                    }
                    else
                    {
                        if (selectedItems.Contains(id))
                        {
                            selectedItems.Remove(id);
                        }
                    }
                };
            }
            return view;
        }

        #endregion
	}
}

