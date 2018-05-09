using System;
using System.IO;
using System.Linq;
using System.Threading;
using Caliburn.Micro;
using MeetingSdk.NetAgent;
using MeetingSdk.NetAgent.Models;
using Serilog;
using St.Common;

namespace St.Meeting
{
    public class LocalPushLiveService : IPushLive
    {
        private static readonly string ConfigFile = Path.Combine(Environment.CurrentDirectory,
            GlobalResources.ConfigPath);

        private readonly IMeetingSdkAgent _meetingService;
        private readonly object _syncRoot = new object();

        public LocalPushLiveService()
        {
            _meetingService = IoC.Get<IMeetingSdkAgent>();
        }

        public bool HasPushLiveSuccessfully { get; set; }

        public int LiveId { get; set; }

        public PublishLiveStreamParameter LiveParam { get; private set; }

        public void ResetStatus()
        {
            LiveId = 0;
            HasPushLiveSuccessfully = false;
        }

        public bool GetLiveParam()
        {
            AggregatedConfig configManager = GlobalData.Instance.AggregatedConfig;

            try
            {
                if (configManager?.LocalLiveStreamInfo == null) return false;
                LiveParam = new PublishLiveStreamParameter
                {
                    LiveParameter = new LiveParameter()
                    {
                        AudioBitrate = 64,
                        BitsPerSample = 16,
                        Channels = 2,
                        IsLive = true,
                        IsRecord = false,
                        SampleRate = 48000,
                        VideoBitrate = configManager.LocalLiveStreamInfo.LiveStreamBitRate,
                        Width = configManager.LocalLiveStreamInfo.LiveStreamDisplayWidth,
                        Height = configManager.LocalLiveStreamInfo.LiveStreamDisplayHeight,
                        Url1 = GlobalData.Instance.AggregatedConfig.LocalLiveStreamInfo.PushLiveStreamUrl,
                    }
                };
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"【get server push live param exception】：{ex}");
                return false;
            }
            return true;
        }

        public MeetingResult RefreshLiveStream(VideoStreamModel[] videoStreamModels, AudioStreamModel[] audioStreamModels)
        {
            if (LiveId == 0)
            {
                return new MeetingResult()
                {
                    Message = "没有可更新的流！",
                    StatusCode = -1,
                };
            }

            Monitor.Enter(_syncRoot);

            MeetingResult updateVideoResult = _meetingService.UpdateLiveStreamVideoInfo(LiveId, videoStreamModels.ToArray());
            MeetingResult updateAudioResult = _meetingService.UpdateLiveStreamAudioInfo(LiveId, audioStreamModels.ToArray());

            MeetingResult mergedResult = new MeetingResult()
            {
                Message = "更新推流成功！",
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

        public MeetingResult StartPushLiveStream(VideoStreamModel[] videoStreamModels, AudioStreamModel[] audioStreamModels, string pushLiveUrl = "")
        {
            if (string.IsNullOrEmpty(pushLiveUrl))
            {
                return new MeetingResult()
                {
                    Message = "推流地址为空！",
                    StatusCode = -1,
                };
            }

            LiveParam.LiveParameter.Url1 = pushLiveUrl;

            if (LiveParam.LiveParameter.Width == 0 || LiveParam.LiveParameter.Height == 0 || LiveParam.LiveParameter.VideoBitrate == 0)
            {
                return new MeetingResult()
                {
                    Message = "推流分辨率或码率未设置！",
                    StatusCode = -1,
                };
            }

            MeetingResult<int> startPushLiveStreamResult = _meetingService.PublishLiveStream(LiveParam);

            if (startPushLiveStreamResult.StatusCode != 0)
            {
                return startPushLiveStreamResult;
            }

            LiveId = startPushLiveStreamResult.Result;

            MeetingResult updateVideoResult = _meetingService.UpdateLiveStreamVideoInfo(LiveId, videoStreamModels.ToArray());
            MeetingResult updateAudioResult = _meetingService.UpdateLiveStreamAudioInfo(LiveId, audioStreamModels.ToArray());

            MeetingResult startLiveStreamResult = _meetingService.StartLiveRecord(LiveId, pushLiveUrl);

            MeetingResult mergedResult = new MeetingResult()
            {
                Message = "推流成功！",
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

            if (startLiveStreamResult.StatusCode != 0)
            {
                mergedResult.StatusCode = -1;
                mergedResult.Message += $" {startLiveStreamResult.Message}";
            }

            HasPushLiveSuccessfully = mergedResult.StatusCode == 0;

            return mergedResult;
        }

        public MeetingResult StopPushLiveStream()
        {
            if (LiveId == 0)
            {
                return new MeetingResult()
                {
                    Message = "没有可停止的流！",
                    StatusCode = -1,
                };
            }

            MeetingResult stopRecordResult = _meetingService.StopLiveRecord(LiveId);
            MeetingResult unpublishLiveResult = _meetingService.UnpublishLiveStream(LiveId);
            LiveId = 0;

            MeetingResult mergedResult = new MeetingResult()
            {
                Message = "停止推流成功！",
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
