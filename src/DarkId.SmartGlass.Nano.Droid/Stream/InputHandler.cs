using System;
using Android.Content;
using Android.Runtime;
using Android.Hardware.Input;

namespace DarkId.SmartGlass.Nano.Droid
{
    public class InputHandler
    {
        private StreamActivity _activity;

        public InputHandler(StreamActivity activity)
        {
            _activity = activity;
        }

        public int EnumerateGamepads()
        {
            return 0;
        }

        public bool HandleButtonPress(Android.Views.KeyEvent arg)
        {
            return true;
        }

        public bool HandleAxisMovement(Android.Views.MotionEvent arg)
        {
            return true;
        }

        public void OnInputDeviceAdded(int deviceId)
        {
            throw new NotImplementedException();
        }

        public void OnInputDeviceChanged(int deviceId)
        {
            throw new NotImplementedException();
        }

        public void OnInputDeviceRemoved(int deviceId)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
