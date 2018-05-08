using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using St.Common;
using System.IO;
using Caliburn.Micro;
using MeetingSdk.SdkWrapper;
using MeetingSdk.SdkWrapper.MeetingDataModel;
using Serilog;
using MeetingSdk.NetAgent;
using MeetingSdk.NetAgent.Models;
using System.Threading;

namespace St.Meeting
{
    public class LocalRecordService : IRecord
    {
        private readonly IMeetingSdkAgent _meetingService;
        private readonly object _syncRoot = new object();


        public LocalRecordService()
        {
            _meetingService = IoC.Get<IMeetingSdkAgent>();
        }

        public string RecordDirectory { get; private set; }

        public int RecordId { get; set; }

        public PublishLiveStreamParameter RecordParam { get; private set; }

        public void ResetStatus()
        {
            RecordDirectory = string.Empty;
            RecordId = 0;
        }

        public bool GetRecordParam()
        {

            AggregatedConfig configManager = GlobalData.Instance.AggregatedConfig;

            try
            {
                if (configManager?.RecordInfo == null) return false;
                RecordParam = new PublishLiveStreamParameter()
                {
                    LiveParameter = new LiveParameter()
                    {
                        AudioBitrate = 64,
                        BitsPerSample = 16,
                        Channels = 2,
                        SampleRate = configManager.AudioInfo.SampleRate,
                        VideoBitrate = configManager.RecordInfo.RecordBitRate,
                        Width = configManager.RecordInfo.RecordDisplayWidth,
                        Height = configManager.RecordInfo.RecordDisplayHeight,
                        IsRecord = true,
                        IsLive = false,
                        FilePath = configManager.RecordInfo.RecordDirectory,
                    },
                    MediaType = MediaType.StreamMedia,
                    StreamType = StreamType.Live,

                };

                RecordDirectory = configManager.RecordInfo.RecordDirectory;

                return true;
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"【get record param exception】：{ex}");
                return false;
            }
        }

        public MeetingResult RefreshLiveStream(VideoStreamModel[] videoStreamModels, AudioStreamModel[] audioStreamModels)
        {
            if (RecordId == 0)
            {
                return new MeetingResult()
                {
                    Message = "没有可更新的流！",
                    StatusCode = -1,
                };
            }

            Monitor.Enter(_syncRoot);

            MeetingResult updateVideoResult = _meetingService.UpdateLiveStreamVideoInfo(RecordId, videoStreamModels);
            MeetingResult updateAudioResult = _meetingService.UpdateLiveStreamAudioInfo(RecordId, audioStreamModels);

            MeetingResult mergedResult = new MeetingResult()
            {
                Message = "更新录制成功！",
                StatusCode = 0
            };

            if (updateAudioResult.StatusCode != 0)
            {
                mergedResult.StatusCode = -1;
                mergedResult.Message = updateAudioResult.Message;
            }

            if (updateVideoResult.StatusCode != 0)
            {
                mergedResult.StatusCode = -1;
                mergedResult.Message += $" {updateVideoResult.Message}";
            }

            Monitor.Exit(_syncRoot);

            return mergedResult;
        }

        public MeetingResult StartMp4Record(VideoStreamModel[] videoStreamModels, AudioStreamModel[] audioStreamModels)
        {
            if (string.IsNullOrEmpty(RecordDirectory) || !Directory.Exists(RecordDirectory))
            {
                return new MeetingResult()
                {
                    Message = "录制路径未设置！",
                    StatusCode = -1,
                };
            }

            MeetingResult<int> publishLiveResult = _meetingService.PublishLiveStream(RecordParam);

            if (publishLiveResult.StatusCode != 0)
            {
                return publishLiveResult;
            }

            RecordId = publishLiveResult.Result;

            MeetingResult updateVideoResult = _meetingService.UpdateLiveStreamVideoInfo(publishLiveResult.Result, videoStreamModels);
            MeetingResult updateAudioResult = _meetingService.UpdateLiveStreamAudioInfo(publishLiveResult.Result, audioStreamModels);


            string filename = Path.Combine(RecordDirectory, DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss") + ".mp4");

            MeetingResult startRecordResult = _meetingService.StartMp4Record(publishLiveResult.Result, filename);


            MeetingResult mergedResult = new MeetingResult()
            {
                Message = "录制成功！",
                StatusCode = 0
            };

            if (updateVideoResult.StatusCode != 0)
            {
                mergedResult.StatusCode = -1;
                mergedResult.Message = updateVideoResult.Message;
            }

            if (updateAudioResult.StatusCode != 0)
            {
                mergedResult.StatusCode = -1;
                mergedResult.Message += $" {updateAudioResult.Message}";
            }

            if (startRecordResult.StatusCode != 0)
            {
                mergedResult.StatusCode = -1;
                mergedResult.Message += $" {startRecordResult.Message}";
            }

            return mergedResult;
        }

        public MeetingResult StopMp4Record()
        {
            if (RecordId == 0)
            {
                return new MeetingResult()
                {
                    Message = "没有可停止的流！",
                    StatusCode = -1,
                };
            }

            MeetingResult stopRecordResult = _meetingService.StopMp4Record(RecordId);
            MeetingResult unpublishLiveResult = _meetingService.UnpublishLiveStream(RecordId);
            RecordId = 0;


            MeetingResult mergedResult = new MeetingResult()
            {
                Message = "停止录制成功！",
                StatusCode = 0
            };

            if (stopRecordResult.StatusCode != 0)
            {
                mergedResult.StatusCode = -1;
                mergedResult.Message = stopRecordResult.Message;
            }

            if (unpublishLiveResult.StatusCode != 0)
            {
                mergedResult.StatusCode = -1;
                mergedResult.Message += $" {unpublishLiveResult.Message}";
            }

            return mergedResult;
        }
    }
}
