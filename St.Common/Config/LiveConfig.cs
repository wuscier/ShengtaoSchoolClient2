using Prism.Mvvm;

namespace St.Common
{


    public class LiveStreamInfo:BindableBase
    {
        //public string Description { get; set; }

        public int LiveStreamDisplayWidth { get; set; }

        public int LiveStreamDisplayHeight { get; set; }

        public int LiveStreamBitRate { get; set; }

        private string pushLiveStreamUrl;

        public string PushLiveStreamUrl
        {
            get { return pushLiveStreamUrl; }
            set { SetProperty(ref pushLiveStreamUrl, value); }
        }

        //public bool IsEnabled { get; set; }
    }

}