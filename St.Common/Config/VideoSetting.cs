using MeetingSdk.NetAgent.Models;
using System.Collections.ObjectModel;

namespace St.Common
{
    public class VideoSetting
    {
        public string VideoType { get; set; }

        public ObservableCollection<VideoFormatModel> Colorspaces { get; set; }
        public ObservableCollection<string> BitRateList { get; set; }
        public ObservableCollection<string> ResolutionList { get; set; }
    }
}