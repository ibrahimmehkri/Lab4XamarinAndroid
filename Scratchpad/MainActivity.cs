using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Content;
using Android.Views;
using Android.Provider;
using Android.Util;

namespace Scratchpad
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private readonly string TAG = "MainActivityLogs";
        TextView textViewEntryCount;
        DBHelper dBHelper;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            FindViewById<Button>(Resource.Id.addEntryBtn).Click += delegate {
                Intent i = new Intent(this, typeof(SecondActivity));
                StartActivity(i);
            };

            dBHelper = new DBHelper(this);
            textViewEntryCount = FindViewById<TextView>(Resource.Id.textViewEntryCount);
            textViewEntryCount.Text = $"Number of Entries: {dBHelper.GetEntryTableRowCount().ToString()}";

            FindViewById<Button>(Resource.Id.buttonDeleteAllEntries).Click += delegate {
                dBHelper.EmptyEntryTable();
                textViewEntryCount.Text = $"Number of Entries: {dBHelper.GetEntryTableRowCount().ToString()}";
                Toast.MakeText(this, "All entries deleted", ToastLength.Long).Show();
            };



            FindViewById<Button>(Resource.Id.buttonSendVATReport).Click += delegate {

                 Intent intent = new Intent(Android.Content.Intent.ActionSend);
                //intent.PutExtra(Intent.ExtraEmail, new string[] { "mehkri_laila@gmail.com" });
                //intent.PutExtra(Intent.ExtraSubject, "Why is this required?");
                intent.PutExtra(Intent.ExtraText, dBHelper.GetTaxReport()); 
                 intent.SetType("text/plain"); 
                 if(intent.ResolveActivity(PackageManager) != null)
                 {
                     Log.Info(TAG, "intent can be resolved");
                     StartActivity(Intent.CreateChooser(intent, "Send email using:"));
                 } else
                 {
                     Log.Info(TAG, "intent cannot be resolved");
                 }

            };


        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnResume()
        {
            base.OnResume();
            Log.Info(TAG, "Resume method was called");
            textViewEntryCount.Text = $"Number of Entries: {dBHelper.GetEntryTableRowCount().ToString()}";
        }

    }
}