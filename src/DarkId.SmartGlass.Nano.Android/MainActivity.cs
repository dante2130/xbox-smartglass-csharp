using Android.App;
using Android.Widget;
using Android.OS;
using Android.Runtime;
using Android.Views;
using System.Collections.Generic;
using Android.Content.PM;

using DarkId.SmartGlass;

namespace DarkId.SmartGlass.Nano.Android
{
    [Activity(Label = "xNano", MainLauncher = true, Icon = "@mipmap/icon",
              ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainActivity : Activity
    {
        private ListView _consoleListView;
        private Button _refreshButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            _consoleListView = FindViewById<ListView>(Resource.Id.lvConsoles);
            _refreshButton = FindViewById<Button>(Resource.Id.btnRefresh);

            _consoleListView.ItemClick += _consoleListView_ItemClick;
            _refreshButton.Click += _refreshButton_Click;
        }

        void _refreshButton_Click(object sender, System.EventArgs e)
        {
        }

        void _consoleListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
        }
    }
}

