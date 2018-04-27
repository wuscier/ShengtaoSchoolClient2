using Prism.Mvvm;

namespace St.Common
{

    public class VideoInfo : BindableBase
    {
        private string _videoDevice;
        public string VideoDevice
        {
            get { return _videoDevice; }
            set { SetProperty(ref _videoDevice, value); }
        }


        private int _displayWidth;
        public int DisplayWidth
        {
            get { return _displayWidth; }
            set { SetProperty(ref _displayWidth, value); }
        }

        private int _displayHeight;
        public int DisplayHeight
        {
            get { return _displayHeight; }
            set { SetProperty(ref _displayHeight, value); }
        }

        private int _videoBitRate;
        public int VideoBitRate
        {
            get { return _videoBitRate; }
            set { SetProperty(ref _videoBitRate, value); }
        }

        private int _colorSpace;
        public int ColorSpace
        {
            get
            {
                return _colorSpace;
            }
            set
            {
                SetProperty(ref _colorSpace, value);
            }
        }
    }
}