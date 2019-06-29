
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
using Android.Provider;
using Android.Graphics;
using Java.IO;
using Android.Support.V4.Content;
using Android.Util;
using Android;
using Android.Content.PM;
using Android.Database.Sqlite; 


namespace Scratchpad
{
    [Activity(Label = "SecondActivity")]
    public class SecondActivity : Activity
    {

        ImageView imageView;
        Bitmap bitmap;
        const int REQUEST_IMAGE_CAPTURE = 1;
        const int REQUEST_WRITE_PERMISSION = 2;
        const string TAG = "MyLogs";
        DBHelper dBHelper;
        bool pictureAdded = false;
        string imageFilePath;
        EditText editTextEntryName;
        int rowCount;


        protected override void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_second);

            editTextEntryName = FindViewById<EditText>(Resource.Id.editTextEntryName);

            FindViewById<Button>(Resource.Id.take_photo_btn).Click += delegate {
                DispatchPictureIntent();
            };

            imageView = FindViewById<ImageView>(Resource.Id.imageView1);
            imageView.SetImageResource(Resource.Drawable.image_placeholder);

            FindViewById<Button>(Resource.Id.save_entry_btn).Click += delegate {
                SaveEntryInDatabase();
            };

            dBHelper = new DBHelper(Android.App.Application.Context);
            rowCount = GetCountEntryTable(); 
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == REQUEST_IMAGE_CAPTURE && resultCode == Result.Ok)
            {
                Log.Info(TAG, string.Join(", ", data.Extras.KeySet()));
                Log.Info(TAG, data.Extras.Get("data").ToString());
                bitmap = (Bitmap)data.Extras.Get("data");
                imageView.SetImageBitmap(bitmap);
                pictureAdded = true;
            }
        }

        private void DispatchPictureIntent()
        {

            Intent intent = new Intent(MediaStore.ActionImageCapture);

            if (intent.ResolveActivity(PackageManager) != null)
            {
                StartActivityForResult(intent, REQUEST_IMAGE_CAPTURE);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode == REQUEST_WRITE_PERMISSION)
            {
                Log.Info(TAG, "Result for permission request to write received.");

                if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                {
                    Log.Info(TAG, "Permission to write granted.");
                    SaveEntryInDatabase();
                } else
                {
                    Log.Info(TAG, "Permission not granted.");
                }
            }
        }

        private void SaveEntryInDatabase()
        {
            string fileName = $"demoImage{rowCount+1}";
            string entryName = editTextEntryName.Text;
            string amount = FindViewById<EditText>(Resource.Id.editTextAmount).Text; 

            if (entryName.Length > 0 && amount.Length > 0)
            {

                bool mounted = Android.OS.Environment.ExternalStorageState.Equals(Android.OS.Environment.MediaMounted);
                bool permissionGranted = CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Android.Content.PM.Permission.Granted;

                if (mounted && permissionGranted)
                {
                    Log.Info(TAG, "External storage is mounted & Permission to write is granted.");
                    if(pictureAdded)
                    {
                        string dirPictures = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath;
                        imageFilePath = System.IO.Path.Combine(dirPictures, fileName);
                        System.IO.FileStream stream = new System.IO.FileStream(imageFilePath, System.IO.FileMode.Create);
                        bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
                        stream.Close();
                    }
                    else
                    {
                        imageFilePath = "Not added"; 
                    }

                    SQLiteDatabase db = dBHelper.WritableDatabase;
                    ContentValues values = new ContentValues();
                    values.Put(DBContract.Entry.COLUMN_NAME_NAME, entryName);
                    values.Put(DBContract.Entry.COLUMN_NAME_FILEPATH, imageFilePath);
                    values.Put(DBContract.Entry.COLUMN_NAME_DATE, DateTime.Now.ToShortDateString());


                    string expense = "-";
                    if(FindViewById<RadioButton>(Resource.Id.radioButtonExpense).Checked)
                    {
                        expense += amount;
                        values.Put(DBContract.Entry.COLUMN_NAME_AMOUNT, double.Parse(expense));
                    } else
                    {
                        values.Put(DBContract.Entry.COLUMN_NAME_AMOUNT, double.Parse(amount));
                    }

                    long newRowId = db.Insert(DBContract.Entry.TABLE_NAME, null, values);

                    if(newRowId != -1)
                    {
                        StartActivity(new Intent(this, typeof(MainActivity))); 
                    } else
                    {
                        Toast.MakeText(this, "Error adding entry", ToastLength.Long).Show(); 
                    }

                }
                else if (mounted)
                {
                    Log.Info(TAG, "External storage is mounted. Need to make a permission request to write.");
                    string[] permissionsRequired = { Manifest.Permission.WriteExternalStorage };
                    RequestPermissions(permissionsRequired, REQUEST_WRITE_PERMISSION);
                }
            }
            else
            {
                Log.Info(TAG, "File name or Entry name is missing.");
                Toast.MakeText(this, "File name or Entry name is missing.", ToastLength.Long).Show(); 
            }
        }


        private int GetCountEntryTable()
        {
            SQLiteDatabase db = dBHelper.ReadableDatabase;
            if (db != null)
            {
                string[] columns = { DBContract.ID, DBContract.Entry.COLUMN_NAME_NAME, DBContract.Entry.COLUMN_NAME_FILEPATH };
                Android.Database.ICursor cursor = db.Query(DBContract.Entry.TABLE_NAME, columns, null, null, null, null, null);
                return cursor.Count;
            } else
            {
                return -1;
            }
        }
        /*
         * private void EmptyEntryTable()
        {
            dBHelper.OnUpgrade(dBHelper.WritableDatabase, DBHelper.DATABASE_VERSION, DBHelper.DATABASE_VERSION++);
            textViewRowCount.Text = $"Number of rows: {rowCount}";
        }       
        */


    }

}
