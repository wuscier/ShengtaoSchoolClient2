using System.Collections.Generic;
using System.Threading.Tasks;
using MeetingSdk.NetAgent;
using MeetingSdk.NetAgent.Models;
using MeetingSdk.SdkWrapper.MeetingDataModel;

namespace St.Common
{
    public interface IRecord
    {
        int RecordId { get; }
        string RecordDirectory { get; }
        PublishLiveStreamParameter RecordParam { get; }

        void ResetStatus();

        bool GetRecordParam();

        MeetingResult RefreshLiveStream(VideoStreamModel[] videoStreamModels, AudioStreamModel[] audioStreamModels);
        MeetingResult StartMp4Record(VideoStreamModel[] videoStreamModels, AudioStreamModel[] audioStreamModels);
        MeetingResult StopMp4Record();
    }
}
