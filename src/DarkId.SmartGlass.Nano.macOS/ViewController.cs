using System;
using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace DarkId.SmartGlass.Nano.macOS
{
    public partial class ViewController : NSViewController
    {
        private AVPlayer _player;
        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _player = new AVPlayer();
            _player.SetView(this.View);
            Task.Run(() => _player.CreateClient());
        }

        public override void ViewDidAppear()
        {
            base.ViewDidAppear();
        }

        public override NSObject RepresentedObject
        {
            get
            {
                return base.RepresentedObject;
            }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
        }
    }
}
