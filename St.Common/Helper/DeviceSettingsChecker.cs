using Caliburn.Micro;
using MeetingSdk.NetAgent;
using MeetingSdk.NetAgent.Models;
using MeetingSdk.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace St.Common.Helper
{
    public class DeviceSettingsChecker
    {
        private readonly IMeetingSdkAgent _meetingSdkAgent;
        private AggregatedConfig _configManager = GlobalData.Instance.AggregatedConfig;

        private DeviceSettingsChecker()
        {
            _meetingSdkAgent = IoC.Get<IMeetingSdkAgent>();

        }

        public readonly static DeviceSettingsChecker Instance = new DeviceSettingsChecker();

        public string IsVideoAudioSettingsValid()
        {
            MeetingResult<IList<VideoDeviceModel>> videoDeviceResult = _meetingSdkAgent.GetVideoDevices();

            MeetingResult<IList<string>> micResult = _meetingSdkAgent.GetMicrophones();

            MeetingResult<IList<string>> speakerResult = _meetingSdkAgent.GetLoudSpeakers();

            IDeviceNameAccessor deviceNameAccessor = IoC.Get<IDeviceNameAccessor>();

            IEnumerable<string> cameraDeviceName;
            if (videoDeviceResult.Result.Count == 0 || string.IsNullOrEmpty(_configManager.MainVideoInfo?.VideoDevice) || !deviceNameAccessor.TryGetName(DeviceName.Camera, new Func<DeviceName, bool>(d => { return d.Option == "first"; }), out cameraDeviceName) || !videoDeviceResult.Result.Any(vdm => vdm.DeviceName == cameraDeviceName.FirstOrDefault()))
            {
                return Messages.WarningNoCamera;
            }

            if (_configManager.MainVideoInfo?.DisplayWidth == 0 || _configManager.MainVideoInfo?.DisplayHeight == 0 || _configManager.MainVideoInfo?.VideoBitRate == 0)
            {
                return Messages.WarningWrongVideoParams;
            }

            IEnumerable<string> micDeviceName;
            if (micResult.Result.Count == 0 || string.IsNullOrEmpty(_configManager.AudioInfo?.AudioSammpleDevice) || !deviceNameAccessor.TryGetName(DeviceName.Microphone, new Func<DeviceName, bool>(d => { return d.Option == "first"; }), out micDeviceName) || !micResult.Result.Any(mic => mic == micDeviceName.FirstOrDefault()))
            {
                return Messages.WarningNoMicrophone;
            }

            if (_configManager.AudioInfo?.SampleRate == 0 || _configManager.AudioInfo?.AAC == 0)
            {
                return Messages.WarningWrongMicParams;
            }

            string audioOutputDeviceName;
            if (speakerResult.Result.Count == 0 || string.IsNullOrEmpty(_configManager.AudioInfo?.AudioOutPutDevice) || !deviceNameAccessor.TryGetSingleName(DeviceName.Speaker, out audioOutputDeviceName) || !speakerResult.Result.Any(speaker => speaker == audioOutputDeviceName))
            {
                return Messages.WarningNoSpeaker;
            }

            if (string.IsNullOrEmpty(_configManager.RecordInfo.RecordDirectory))
            {
                return Messages.WarningRecordDirectoryNotSet;
            }

            if (!Directory.Exists(_configManager.RecordInfo.RecordDirectory))
            {
                return Messages.WarningRecordDirectoryNotExist;
            }

            return "";
        }
    }
}
