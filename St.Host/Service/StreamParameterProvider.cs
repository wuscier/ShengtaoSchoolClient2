using MeetingSdk.NetAgent.Models;
using MeetingSdk.Wpf;
using St.Common;

namespace St.Host.Service
{
    public static class ProviderHelper
    {
        public static AudioInfo GetAudioInfo(string sourceName)
        {
            AudioInfo audioInfo = null;


            AggregatedConfig configManager = GlobalData.Instance.AggregatedConfig;

            if (sourceName == configManager.AudioInfo.AudioSammpleDevice || sourceName == configManager.AudioInfo.DocAudioSammpleDevice)
            {
                return configManager.AudioInfo;
            }

            audioInfo = new AudioInfo()
            {
                AAC = 64000,
                SampleRate = 48000,
            };

            return audioInfo;
        }

        public static VideoInfo GetVideoInfo(string sourceName)
        {
            VideoInfo videoInfo = null;

            AggregatedConfig configManager = GlobalData.Instance.AggregatedConfig;

            if (sourceName == configManager.MainVideoInfo.VideoDevice)
            {
                return configManager.MainVideoInfo;
            }

            if (sourceName == configManager.DocVideoInfo.VideoDevice)
            {
                return configManager.DocVideoInfo;
            }

            if (sourceName == "DesktopCapture")
            {
                videoInfo.DisplayWidth = 1280;
                videoInfo.DisplayHeight = 720;
                videoInfo.VideoBitRate = 1200;
                videoInfo.ColorSpace = (int)VideoColorSpace.YUY2;
                return videoInfo;
            }

            return videoInfo;
        }
    }


    public class PublishMicStreamParameterProvider : IStreamParameterProvider<PublishMicStreamParameter>
    {
        public void Provider(PublishMicStreamParameter parameter, string sourceName)
        {
            AudioInfo audioInfo = ProviderHelper.GetAudioInfo(sourceName);

            parameter.AudioCodeId = AudioCodeId.Aac;
            parameter.CapBitsPerSample = 16;
            parameter.CapChannels = 2;
            parameter.CapSampleRate = audioInfo.SampleRate;
            parameter.CheckRetransSendCount = 1;
            parameter.CheckSendCount = 1;
            parameter.DataResendCount = 1;
            parameter.DataRetransSendCount = 1;
            parameter.DataSendCount = 1;
            parameter.DelayTimeWinsize = 0;
            parameter.EncBitrate = 64;
            parameter.EncBitsPerSample = 16;
            parameter.EncChannels = 2;
            parameter.EncSampleRate = audioInfo.SampleRate;
            parameter.FecCheckCount = 2;
            parameter.FecDataCount = 4;
            parameter.IsMix = 0;
        }
    }



    public class PublishCameraStreamParameterProvider : IStreamParameterProvider<PublishCameraStreamParameter>
    {
        public void Provider(PublishCameraStreamParameter parameter, string sourceName)
        {
            VideoInfo videoInfo = ProviderHelper.GetVideoInfo(sourceName);

            parameter.CapBottom = videoInfo.DisplayHeight;
            parameter.CapFps = 30;
            parameter.CapLeft = 0;
            parameter.CapRight = videoInfo.DisplayWidth;
            parameter.CapTop = 0;
            parameter.DisplayFillMode = DisplayFillMode.RawWithBlack;
            parameter.EncBitrate = videoInfo.VideoBitRate;
            parameter.EncFps = 30;
            parameter.EncHeight = videoInfo.DisplayHeight;
            parameter.EncWidth = videoInfo.DisplayWidth;
            parameter.VideoCodeId = VideoCodeId.H264;
            parameter.VideoCodeLevel = VideoCodeLevel.High;
            parameter.VideoCodeType = VideoCodeType.Soft;
            parameter.VideoColorSpace = (VideoColorSpace)videoInfo.ColorSpace;
            parameter.CheckRetransSendCount = 1;
            parameter.CheckSendCount = 1;
            parameter.DataResendCount = 1;
            parameter.DataRetransSendCount = 1;
            parameter.DataSendCount = 1;
            parameter.DelayTimeWinsize = 0;
            parameter.FecCheckCount = 3;
            parameter.FecDataCount = 6;
        }
    }

    public class PublishDataCardStreamParameterProvider : IStreamParameterProvider<PublishDataCardStreamParameter>
    {
        public void Provider(PublishDataCardStreamParameter parameter, string sourceName)
        {
            VideoInfo videoInfo = ProviderHelper.GetVideoInfo(sourceName);

            parameter.CapBottom = videoInfo.DisplayHeight;
            parameter.CapFps = 30;
            parameter.CapLeft = 0;
            parameter.CapRight = videoInfo.DisplayWidth;
            parameter.CapTop = 0;
            parameter.DisplayFillMode = DisplayFillMode.RawWithBlack;
            parameter.EncBitrate = videoInfo.VideoBitRate;
            parameter.EncFps = 30;
            parameter.EncHeight = videoInfo.DisplayHeight;
            parameter.EncWidth = videoInfo.DisplayWidth;
            parameter.VideoCodeId = VideoCodeId.H264;
            parameter.VideoCodeLevel = VideoCodeLevel.High;
            parameter.VideoCodeType = VideoCodeType.Soft;
            parameter.VideoColorSpace = (VideoColorSpace)videoInfo.ColorSpace;
            parameter.CheckRetransSendCount = 1;
            parameter.CheckSendCount = 1;
            parameter.DataResendCount = 1;
            parameter.DataRetransSendCount = 1;
            parameter.DataSendCount = 1;
            parameter.DelayTimeWinsize = 0;
            parameter.FecCheckCount = 3;
            parameter.FecDataCount = 6;

        }
    }

    public class PublishWinCaptureStreamParameterProvider : IStreamParameterProvider<PublishWinCaptureStreamParameter>
    {
        public void Provider(PublishWinCaptureStreamParameter parameter, string sourceName)
        {
            VideoInfo videoInfo = ProviderHelper.GetVideoInfo(sourceName);

            parameter.CapBottom = videoInfo.DisplayHeight;
            parameter.CapFps = 30;
            parameter.CapLeft = 0;
            parameter.CapRight = videoInfo.DisplayWidth;
            parameter.CapTop = 0;
            parameter.DisplayFillMode = DisplayFillMode.RawWithBlack;
            parameter.EncBitrate = videoInfo.VideoBitRate;
            parameter.EncFps = 30;
            parameter.EncHeight = videoInfo.DisplayHeight;
            parameter.EncWidth = videoInfo.DisplayWidth;
            parameter.VideoCodeId = VideoCodeId.H264;
            parameter.VideoCodeLevel = VideoCodeLevel.High;
            parameter.VideoCodeType = VideoCodeType.Soft;
            parameter.VideoColorSpace = (VideoColorSpace)videoInfo.ColorSpace;
            parameter.CheckRetransSendCount = 1;
            parameter.CheckSendCount = 1;
            parameter.DataResendCount = 1;
            parameter.DataRetransSendCount = 1;
            parameter.DataSendCount = 1;
            parameter.DelayTimeWinsize = 0;
            parameter.FecCheckCount = 3;
            parameter.FecDataCount = 6;
        }
    }

    public class SubscribeMicStreamParameterProvider : IStreamParameterProvider<SubscribeMicStreamParameter>
    {
        public void Provider(SubscribeMicStreamParameter parameter, string sourceName)
        {
            parameter.CheckRetransSendCount = 1;
            parameter.CheckSendCount = 1;
            parameter.DataResendCount = 1;
            parameter.DataRetransSendCount = 1;
            parameter.DataSendCount = 1;
            parameter.DelayTimeWinsize = 500;
            parameter.FecCheckCount = 2;
            parameter.FecDataCount = 4;
            parameter.IsMix = 0;
        }
    }

    public class SubscribeCameraStreamParameterProvider : IStreamParameterProvider<SubscribeCameraStreamParameter>
    {
        public void Provider(SubscribeCameraStreamParameter parameter, string sourceName)
        {
            parameter.CheckRetransSendCount = 1;
            parameter.CheckSendCount = 1;
            parameter.DataResendCount = 1;
            parameter.DataRetransSendCount = 1;
            parameter.DataSendCount = 1;
            parameter.DelayTimeWinsize = 500;
            parameter.FecCheckCount = 3;
            parameter.FecDataCount = 6;
            parameter.DisplayFillMode = DisplayFillMode.RawWithBlack;
        }
    }

    public class SubscribeDataCardStreamParameterProvider : IStreamParameterProvider<SubscribeDataCardStreamParameter>
    {
        public void Provider(SubscribeDataCardStreamParameter parameter, string sourceName)
        {
            parameter.CheckRetransSendCount = 1;
            parameter.CheckSendCount = 1;
            parameter.DataResendCount = 1;
            parameter.DataRetransSendCount = 1;
            parameter.DataSendCount = 1;
            parameter.DelayTimeWinsize = 500;
            parameter.FecCheckCount = 3;
            parameter.FecDataCount = 6;
            parameter.DisplayFillMode = DisplayFillMode.RawWithBlack;
        }
    }

    public class SubscribeWinCaptureStreamParameterProvider : IStreamParameterProvider<SubscribeWinCaptureStreamParameter>
    {
        public void Provider(SubscribeWinCaptureStreamParameter parameter, string sourceName)
        {
            parameter.CheckRetransSendCount = 1;
            parameter.CheckSendCount = 1;
            parameter.DataResendCount = 1;
            parameter.DataRetransSendCount = 1;
            parameter.DataSendCount = 1;
            parameter.DelayTimeWinsize = 500;
            parameter.FecCheckCount = 3;
            parameter.FecDataCount = 6;
            parameter.DisplayFillMode = DisplayFillMode.RawWithBlack;
        }
    }
}
