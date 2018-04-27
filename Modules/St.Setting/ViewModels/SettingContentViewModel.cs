using Prism.Commands;
using St.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;
using System.Windows.Forms;
using Caliburn.Micro;
using MeetingSdk.SdkWrapper;
using MeetingSdk.SdkWrapper.MeetingDataModel;
using Serilog;
using Action = System.Action;
using MeetingSdk.NetAgent;
using MeetingSdk.NetAgent.Models;
using MeetingSdk.Wpf;
using GM.Utilities;

namespace St.Setting
{
    public class SettingContentViewModel : ViewModelBase
    {
        public SettingContentViewModel(SettingContentView meetingConfigView)
        {
            _view = meetingConfigView;
            _sdkService = IoC.Get<IMeeting>();
            _meetingSdkAgent = IoC.Get<IMeetingSdkAgent>();
            _deviceNameAccessor = IoC.Get<IDeviceNameAccessor>();
            _deviceConfigLoader = IoC.Get<IDeviceConfigLoader>();


            _cameraDeviceList = new List<VideoDeviceModel>();
            _docDeviceList = new List<VideoDeviceModel>();

            CameraDeviceList = new ObservableCollection<string>();
            DocDeviceList = new ObservableCollection<string>();

            CameraColorSpaces = new ObservableCollection<VideoFormatModel>();
            DocColorSpaces = new ObservableCollection<VideoFormatModel>();

            VedioParameterVgaList = new ObservableCollection<string>();
            DocParameterVgaList = new ObservableCollection<string>();

            VedioParameterRatesList = new ObservableCollection<int>();

            CheckCameraDeviceCommand = DelegateCommand.FromAsyncHandler(CheckCameraDeviceAsync);
            CheckDocDeviceCommand = DelegateCommand.FromAsyncHandler(CheckDocDeviceAsync);



            Aac = new ObservableCollection<int>();
            SampleRate = new ObservableCollection<int>();
            AudioSource = new ObservableCollection<string>();
            DocAudioSource = new ObservableCollection<string>();
            AudioOutPutDevice = new ObservableCollection<string>();

            CheckPeopleSourceDeviceCommand = new DelegateCommand(CheckPeopleSourceDevice);
            CheckDocSourceDeviceCommand = new DelegateCommand(CheckDocSourceDevice);


            LiveDisplaySource = new ObservableCollection<string>();
            LiveRateSource = new ObservableCollection<int>();



            LoadSettingCommand = new DelegateCommand(LoadSettingAsync);
            UnloadSettingCommand = new DelegateCommand(UnloadSetting);
            //ConfigItemChangedCommand = DelegateCommand<ConfigChangedItem>.FromAsyncHandler(ConfigItemChangedAsync);

            SelectRecordPathCommand = new DelegateCommand(SelectRecordPath);
            LiveUrlChangedCommand = new DelegateCommand(LiveUrlChangedHander);

            //InitializeBindingDataSource();
        }


        //private fields
        private readonly SettingContentView _view;
        private readonly IMeeting _sdkService;
        private readonly IDeviceNameAccessor _deviceNameAccessor;
        private readonly IDeviceConfigLoader _deviceConfigLoader;
        private readonly IMeetingSdkAgent _meetingSdkAgent;

        private AggregatedConfig _configManager = GlobalData.Instance.AggregatedConfig;
        private SettingParameter _settingParameter;

        private string _selectedCameraDevice;
        private string _selectedDocDevice;
        private string _selectedVedioVGA;
        private string _selectedDocVGA;
        private int _selectedVedioRate;
        private int _selectedDocRate;
        private List<VideoDeviceModel> _cameraDeviceList;
        private List<VideoDeviceModel> _docDeviceList;

        private string _audioSource;
        private string _docAudioSource;
        private int _sampleRate;
        private int _aac;
        private string _audioOutPutDevice;


        private string _selectedLiveDisplay;
        private string _selectedRemoteDisplay;
        private int _selectedLiveRate;
        private int _selectedRemoteRate;
        private string _selectedLocalResolution;
        private int _selectedLocalBitrate;
        private string _selectedLocalPath;



        //collection properties
        public ObservableCollection<string> CameraDeviceList { get; set; }
        public ObservableCollection<string> DocDeviceList { get; set; }
        public ObservableCollection<VideoFormatModel> CameraColorSpaces { get; set; }
        public ObservableCollection<VideoFormatModel> DocColorSpaces { get; set; }
        public ObservableCollection<string> VedioParameterVgaList { get; set; }
        public ObservableCollection<string> DocParameterVgaList { get; set; }
        public ObservableCollection<int> VedioParameterRatesList { get; set; }

        public ObservableCollection<string> AudioSource { get; set; }
        public ObservableCollection<string> DocAudioSource { get; set; }
        public ObservableCollection<string> AudioOutPutDevice { get; set; }
        public ObservableCollection<int> SampleRate { get; set; }
        public ObservableCollection<int> Aac { get; set; }

        public ObservableCollection<string> LiveDisplaySource { get; set; }
        public ObservableCollection<int> LiveRateSource { get; set; }




        // selected properties
        public string SelectedCameraDevice
        {
            get { return _selectedCameraDevice; }
            set
            {

                if (SetProperty(ref _selectedCameraDevice, value))
                {
                    _deviceNameAccessor.SetName(DeviceName.Camera, "");
                    _deviceNameAccessor.SetName(DeviceName.Camera, value, "first");

                    if (!string.IsNullOrEmpty(SelectedDocDevice))
                    {
                        _deviceNameAccessor.SetName(DeviceName.Camera, SelectedDocDevice, "second");
                    }

                    UpdateCameraColorSpace();
                }
            }
        }
        public string SelectedDocDevice
        {
            get { return _selectedDocDevice; }
            set
            {
                if (SetProperty(ref _selectedDocDevice, value))
                {
                    _deviceNameAccessor.SetName(DeviceName.Camera, "");
                    _deviceNameAccessor.SetName(DeviceName.Camera, value, "second");
                    if (!string.IsNullOrEmpty(SelectedCameraDevice))
                    {
                        _deviceNameAccessor.SetName(DeviceName.Camera, SelectedCameraDevice, "first");
                    }

                    UpdateDocColorSpace();
                }

            }
        }
        private VideoFormatModel _selectedCameraColorSpace;
        public VideoFormatModel SelectedCameraColorSpace
        {
            get { return _selectedCameraColorSpace; }
            set
            {
                if (SetProperty(ref _selectedCameraColorSpace, value))
                {
                    UpdateCameraVgaSource();
                }
            }
        }
        private VideoFormatModel _selectedDocColorSpace;
        public VideoFormatModel SelectedDocColorSpace
        {
            get { return _selectedDocColorSpace; }
            set
            {
                if (SetProperty(ref _selectedDocColorSpace, value))
                {
                    UpdateDocVgaSource();
                }
            }
        }
        public string SelectedVedioVGA
        {
            get { return _selectedVedioVGA; }
            set { SetProperty(ref _selectedVedioVGA, value); }
        }
        public string SelectedDocVGA
        {
            get { return _selectedDocVGA; }
            set { SetProperty(ref _selectedDocVGA, value); }
        }
        public int SelectedVedioRate
        {
            get { return _selectedVedioRate; }
            set { SetProperty(ref _selectedVedioRate, value); }
        }
        public int SelectedDocRate
        {
            get { return _selectedDocRate; }
            set { SetProperty(ref _selectedDocRate, value); }
        }

        public string SelectedAudioSource
        {
            get { return _audioSource; }
            set
            {
                if (SetProperty(ref _audioSource, value))
                {
                    _deviceNameAccessor.SetName(DeviceName.Microphone, "");
                    _deviceNameAccessor.SetName(DeviceName.Microphone, value, "first");
                    if (!string.IsNullOrEmpty(SelectedDocAudioSource))
                    {
                        _deviceNameAccessor.SetName(DeviceName.Microphone, SelectedDocAudioSource, "second");
                    }
                }
            }
        }
        public string SelectedDocAudioSource
        {
            get { return _docAudioSource; }
            set
            {
                if (SetProperty(ref _docAudioSource, value))
                {
                    _deviceNameAccessor.SetName(DeviceName.Microphone, "");
                    _deviceNameAccessor.SetName(DeviceName.Microphone, value, "second");
                    if (!string.IsNullOrEmpty(SelectedAudioSource))
                    {
                        _deviceNameAccessor.SetName(DeviceName.Microphone, SelectedAudioSource, "first");
                    }

                }
            }
        }
        public int SelectedSampleRate
        {
            get { return _sampleRate; }
            set { SetProperty(ref _sampleRate, value); }
        }
        public int SelectedAac
        {
            get { return _aac; }
            set { SetProperty(ref _aac, value); }
        }
        public string SelectedAudioOutPutDevice
        {
            get { return _audioOutPutDevice; }
            set
            {
                if (SetProperty(ref _audioOutPutDevice, value))
                {
                    _deviceNameAccessor.SetName(DeviceName.Speaker, "");
                    _deviceNameAccessor.SetName(DeviceName.Speaker, value);
                }
            }
        }

        public string SelectedLiveDisplay
        {
            get { return _selectedLiveDisplay; }
            set { SetProperty(ref _selectedLiveDisplay, value); }
        }
        public string SelectedRemoteDisplay
        {
            get { return _selectedRemoteDisplay; }
            set { SetProperty(ref _selectedRemoteDisplay, value); }
        }
        public int SelectedLiveRate
        {
            get { return _selectedLiveRate; }
            set { SetProperty(ref _selectedLiveRate, value); }
        }
        public int SelectedRemoteRate
        {
            get { return _selectedRemoteRate; }
            set { SetProperty(ref _selectedRemoteRate, value); }
        }
        public string SelectedLocalResolution
        {
            get { return _selectedLocalResolution; }
            set { SetProperty(ref _selectedLocalResolution, value); }
        }
        public int SelectedLocalBitrate
        {
            get { return _selectedLocalBitrate; }
            set { SetProperty(ref _selectedLocalBitrate, value); }
        }
        public string SelectedLocalPath
        {
            get { return _selectedLocalPath; }
            set { SetProperty(ref _selectedLocalPath, value); }
        }

        private string _manualPushLiveStreamUrl;

        public string ManualPushLiveStreamUrl
        {
            get { return _manualPushLiveStreamUrl; }
            set { SetProperty(ref _manualPushLiveStreamUrl, value); }
        }


        // methods
        private void UpdateCameraVgaSource()
        {
            VedioParameterVgaList.Clear();
            var cameraVgaList = VgaList(SelectedCameraColorSpace);
            cameraVgaList.ForEach(v => { VedioParameterVgaList.Add(v); });
        }
        private void UpdateDocVgaSource()
        {
            DocParameterVgaList.Clear();
            var docVgaList = VgaList(SelectedDocColorSpace);
            docVgaList.ForEach(v => { DocParameterVgaList.Add(v); });
        }
        private void UpdateCameraColorSpace()
        {
            VideoDeviceModel videoDeviceModel = _cameraDeviceList.FirstOrDefault(camera => camera.DeviceName == SelectedCameraDevice);

            if (videoDeviceModel != null)
            {
                CameraColorSpaces.Clear();

                videoDeviceModel.VideoFormatModels.ForEach(vfm =>
                {
                    CameraColorSpaces.Add(vfm);
                });

                SelectedCameraColorSpace = videoDeviceModel.VideoFormatModels.FirstOrDefault();
            }
            else
            {
                SelectedCameraColorSpace = null;
            }
        }
        private void UpdateDocColorSpace()
        {
            VideoDeviceModel videoDeviceModel = _docDeviceList.FirstOrDefault(camera => camera.DeviceName == SelectedDocDevice);
            if (videoDeviceModel != null)
            {
                DocColorSpaces.Clear();

                videoDeviceModel.VideoFormatModels.ForEach(vfm =>
                {
                    DocColorSpaces.Add(vfm);
                });

                SelectedDocColorSpace = videoDeviceModel.VideoFormatModels.FirstOrDefault();
            }
            else
            {
                SelectedDocColorSpace = null;
            }
        }
        private List<string> VgaList(VideoFormatModel videoFormatModel)
        {
            var vgaList = new List<string>();

            if (videoFormatModel == null)
            {
                return vgaList;
            }

            if (videoFormatModel.SizeModels.Count == 0)
            {
                return vgaList;
            }

            videoFormatModel.SizeModels.ForEach(size =>
            {
                vgaList.Add($"{size.Width}*{size.Height}");
            });

            return vgaList.Distinct().ToList();
        }
        private async Task CheckCameraDeviceAsync()
        {
            await _view.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (SelectedCameraDevice == SelectedDocDevice)
                    SelectedDocDevice = string.Empty;
            }));
        }
        private async Task CheckDocDeviceAsync()
        {
            await _view.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (SelectedCameraDevice == SelectedDocDevice)
                    SelectedCameraDevice = string.Empty;
            }));
        }

        private void CheckPeopleSourceDevice()
        {
            if (SelectedAudioSource == SelectedDocAudioSource)
                SelectedDocAudioSource = string.Empty;

        }
        private void CheckDocSourceDevice()
        {
            if (SelectedAudioSource == SelectedDocAudioSource)
                SelectedAudioSource = string.Empty;

        }

        private SettingParameter GetParametersAsync()
        {
            string configParameterPath = Path.Combine(Environment.CurrentDirectory, Common.GlobalResources.SettingPath);

            try
            {
                if (!File.Exists(configParameterPath)) return null;
                object obj;
                SerializeHelper.Deserialize(configParameterPath, typeof(SettingParameter), out obj);
                var parameter = obj as SettingParameter;
                return parameter;
            }
            catch (Exception ex)
            {
                // Logger.WriteErrorFmt("配置文件", ex, "配置文件处理异常：{0}", ex.Message);
            }
            return null;
        }

        private void SetVideoDefaultSetting()
        {
            SelectedCameraDevice = _configManager.MainVideoInfo.VideoDevice;
            SelectedDocDevice = _configManager.DocVideoInfo.VideoDevice;

            SelectedCameraColorSpace = _cameraDeviceList.FirstOrDefault(vdm => vdm.DeviceName == SelectedCameraDevice)?.VideoFormatModels.FirstOrDefault(vfm => vfm.Colorsapce == _configManager.MainVideoInfo.ColorSpace);
            SelectedDocColorSpace = _docDeviceList.FirstOrDefault(vdm => vdm.DeviceName == SelectedDocDevice)?.VideoFormatModels.FirstOrDefault(vfm => vfm.Colorsapce == _configManager.DocVideoInfo.ColorSpace);

            SelectedVedioVGA = $"{_configManager.MainVideoInfo.DisplayWidth}*{_configManager.MainVideoInfo.DisplayHeight}";
            SelectedDocVGA = $"{_configManager.DocVideoInfo.DisplayWidth}*{_configManager.DocVideoInfo.DisplayHeight}";

            SelectedVedioRate = _configManager.MainVideoInfo.VideoBitRate;
            SelectedDocRate = _configManager.DocVideoInfo.VideoBitRate;
        }
        private void SetDefaultAudioSetting()
        {
            SelectedAudioSource = _configManager.AudioInfo.AudioSammpleDevice;
            SelectedAac = _configManager.AudioInfo.AAC;
            SelectedAudioOutPutDevice = _configManager.AudioInfo.AudioOutPutDevice;
            SelectedDocAudioSource = _configManager.AudioInfo.DocAudioSammpleDevice;
            SelectedSampleRate = _configManager.AudioInfo.SampleRate;
        }
        private void SetDefaultLiveRecordSetting()
        {
            //本地保存的配置
            if (_configManager.LocalLiveStreamInfo == null) _configManager.LocalLiveStreamInfo = new LiveStreamInfo();
            if (_configManager.RecordInfo == null) _configManager.RecordInfo = new RecordInfo();
            if (_configManager.RemoteLiveStreamInfo == null) _configManager.RemoteLiveStreamInfo = new LiveStreamInfo();

            SelectedLiveDisplay =
                $"{_configManager.LocalLiveStreamInfo.LiveStreamDisplayWidth}*{_configManager.LocalLiveStreamInfo.LiveStreamDisplayHeight}";
            SelectedLiveRate = _configManager.LocalLiveStreamInfo.LiveStreamBitRate;
            SelectedRemoteDisplay =
                $"{_configManager.RemoteLiveStreamInfo.LiveStreamDisplayWidth}*{_configManager.RemoteLiveStreamInfo.LiveStreamDisplayHeight}";
            SelectedRemoteRate = _configManager.RemoteLiveStreamInfo.LiveStreamBitRate;
            SelectedLocalResolution =
                $"{_configManager.RecordInfo.RecordDisplayWidth}*{_configManager.RecordInfo.RecordDisplayHeight}";
            SelectedLocalBitrate = _configManager.RecordInfo.RecordBitRate;
            SelectedLocalPath = _configManager.RecordInfo.RecordDirectory;
        }

        private void SaveVideoSettings()
        {
            _deviceConfigLoader.SaveConfig(_deviceNameAccessor);

            var cameraDeviceName = SelectedCameraDevice;
            var docDeviceName = SelectedDocDevice;

            var videoVga = SelectedVedioVGA;
            var docVga = SelectedDocVGA;

            var videoRate = SelectedVedioRate;
            var videoDocRate = SelectedDocRate;

            _configManager.MainVideoInfo = new VideoInfo();
            _configManager.DocVideoInfo = new VideoInfo();

            if (string.IsNullOrEmpty(SelectedCameraDevice) && string.IsNullOrEmpty(SelectedDocDevice))
            {
                HasErrorMsg("-1", "未选择任何视频采集源！");
            }

            if (!string.IsNullOrEmpty(SelectedCameraDevice))
            {
                if (!string.IsNullOrEmpty(videoVga))
                {
                    var videoVgaWith = int.Parse(videoVga.Split('*')[0]);
                    var videoVgaHeight = int.Parse(videoVga.Split('*')[1]);

                    _configManager.MainVideoInfo.DisplayWidth = videoVgaWith;
                    _configManager.MainVideoInfo.DisplayHeight = videoVgaHeight;
                }

                _configManager.MainVideoInfo.VideoDevice = cameraDeviceName;
                _configManager.MainVideoInfo.VideoBitRate = videoRate;
                _configManager.MainVideoInfo.ColorSpace = SelectedCameraColorSpace.Colorsapce;
            }

            if (!string.IsNullOrEmpty(SelectedDocDevice))
            {
                if (!string.IsNullOrEmpty(docVga))
                {
                    var docVgaWith = int.Parse(docVga.Split('*')[0]);
                    var docVgaHeight = int.Parse(docVga.Split('*')[1]);

                    _configManager.DocVideoInfo.DisplayHeight = docVgaHeight;
                    _configManager.DocVideoInfo.DisplayWidth = docVgaWith;

                }

                var docRate = SelectedDocRate;

                _configManager.DocVideoInfo.VideoBitRate = docRate;
                _configManager.DocVideoInfo.VideoDevice = docDeviceName;
                _configManager.DocVideoInfo.ColorSpace = SelectedDocColorSpace.Colorsapce;
            }

            if (!string.IsNullOrEmpty(SelectedDocDevice) || !string.IsNullOrEmpty(SelectedCameraDevice))
                Common.ConfigManager.WriteConfig();
        }

        private void SaveAudioSettings()
        {
            _deviceConfigLoader.SaveConfig(_deviceNameAccessor);

            //保存设置到本地配置文件
            _configManager.AudioInfo.AAC = SelectedAac;
            _configManager.AudioInfo.AudioOutPutDevice = SelectedAudioOutPutDevice;
            _configManager.AudioInfo.AudioSammpleDevice = SelectedAudioSource;
            _configManager.AudioInfo.SampleRate = SelectedSampleRate;
            _configManager.AudioInfo.DocAudioSammpleDevice = SelectedDocAudioSource;


            Common.ConfigManager.WriteConfig();
        }

        private void SaveLiveRecordSettings()
        {
            _configManager.LocalLiveStreamInfo.LiveStreamBitRate = SelectedLiveRate;
            _configManager.LocalLiveStreamInfo.LiveStreamDisplayHeight = int.Parse(SelectedLiveDisplay.Split('*')[1]);
            _configManager.LocalLiveStreamInfo.LiveStreamDisplayWidth = int.Parse(SelectedLiveDisplay.Split('*')[0]);

            _configManager.RemoteLiveStreamInfo.LiveStreamBitRate = SelectedRemoteRate;
            _configManager.RemoteLiveStreamInfo.LiveStreamDisplayHeight =
                int.Parse(SelectedRemoteDisplay.Split('*')[1]);
            _configManager.RemoteLiveStreamInfo.LiveStreamDisplayWidth = int.Parse(SelectedRemoteDisplay.Split('*')[0]);


            _configManager.RecordInfo.RecordBitRate = SelectedLocalBitrate;
            _configManager.RecordInfo.RecordDirectory = SelectedLocalPath;
            _configManager.RecordInfo.RecordDisplayWidth = int.Parse(SelectedLocalResolution.Split('*')[0]);
            _configManager.RecordInfo.RecordDisplayHeight = int.Parse(SelectedLocalResolution.Split('*')[1]);

            Common.ConfigManager.WriteConfig();
        }

        private void SelectRecordPath()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog { SelectedPath = Environment.CurrentDirectory };
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                SelectedLocalPath = fbd.SelectedPath;
            }
        }

        private void LiveUrlChangedHander()
        {
            if (
                !string.IsNullOrEmpty(
                    ManualPushLiveStreamUrl) &&
                Uri.IsWellFormedUriString(ManualPushLiveStreamUrl,
                    UriKind.Absolute))
            {
                LiveUrlColor = "White";
            }
            else
            {
                LiveUrlColor = "Red";
            }
        }

        private void SaveConfig()
        {
            try
            {
                SaveVideoSettings();
                SaveAudioSettings();
                SaveLiveRecordSettings();
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"【save config exception in setting page】：{ex}");
            }
        }



        // commands
        public ICommand CheckCameraDeviceCommand { get; set; }
        public ICommand CheckDocDeviceCommand { get; set; }

        public ICommand CheckPeopleSourceDeviceCommand { get; set; }
        public ICommand CheckDocSourceDeviceCommand { get; set; }

        public ICommand SelectRecordPathCommand { get; set; }
        public ICommand LiveUrlChangedCommand { get; set; }

        public ICommand LoadSettingCommand { get; set; }
        public ICommand UnloadSettingCommand { get; set; }

        private bool isMainCameraExpanded;

        public bool IsMainCameraExpanded
        {
            get { return isMainCameraExpanded; }
            set
            {
                ManageExpanderStatue(value);
                SetProperty(ref isMainCameraExpanded, value);
            }
        }

        private bool isSecondaryCameraExpanded;

        public bool IsSecondaryCameraExpanded
        {
            get { return isSecondaryCameraExpanded; }
            set
            {
                ManageExpanderStatue(value);
                SetProperty(ref isSecondaryCameraExpanded, value);
            }
        }

        private bool isAudioExpanded;

        public bool IsAudioExpanded
        {
            get { return isAudioExpanded; }
            set
            {
                ManageExpanderStatue(value);
                SetProperty(ref isAudioExpanded, value);
            }
        }

        private bool isLiveExpanded;

        public bool IsLiveExpanded
        {
            get { return isLiveExpanded; }
            set
            {
                ManageExpanderStatue(value);
                SetProperty(ref isLiveExpanded, value);
            }
        }
        private bool isServerLiveExpanded;

        public bool IsServerLiveExpanded
        {
            get { return isServerLiveExpanded; }
            set
            {
                ManageExpanderStatue(value);
                SetProperty(ref isServerLiveExpanded, value);
            }
        }

        private bool isRecordExpanded;

        public bool IsRecordExpanded
        {
            get { return isRecordExpanded; }
            set
            {
                ManageExpanderStatue(value);
                SetProperty(ref isRecordExpanded, value);
            }
        }

        private string liveUrlColor = "White";

        public string LiveUrlColor
        {
            get { return liveUrlColor; }
            set { SetProperty(ref liveUrlColor, value); }
        }

        //command handlers
        private void LoadSettingAsync()
        {
            try
            {
                _settingParameter = GetParametersAsync();

                Init_Video_Settings();
                Init_Audio_Settings();
                Init_Live_Record_Settings();
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"【load setting page exception】：{ex}");
                string errorMsg = ex.InnerException?.Message.Replace("\r\n", string.Empty) ??
                                  ex.Message.Replace("\r\n", string.Empty);
                HasErrorMsg("-1", errorMsg);
            }
        }

        private void UnloadSetting()
        {
            SaveConfig();
        }


        private void Init_Live_Record_Settings()
        {
            _settingParameter.LiveParameterVGAs.ForEach(v => { LiveDisplaySource.Add(v.LiveDisplayWidth); });
            _settingParameter.LiveParameterRates.ForEach(r => { LiveRateSource.Add(r.LiveBitRate); });
            SetDefaultLiveRecordSetting();
        }

        private void Init_Audio_Settings()
        {

            AudioSource.Clear();
            DocAudioSource.Clear();
            AudioOutPutDevice.Clear();
            Aac.Clear();
            SampleRate.Clear();

            //设备

            var microphones = _meetingSdkAgent.GetMicrophones();

            var audioSourceList = microphones;
            var docSourceList = microphones;

            var audioOutPutList = _meetingSdkAgent.GetLoudSpeakers();

            var sampleRateList = _settingParameter.AudioParameterSampleRates;
            var aac = _settingParameter.AudioParameterAACs;
            //装载数据源
            audioSourceList.Result.ToList().ForEach(a => { AudioSource.Add(a); });
            docSourceList.Result.ToList().ForEach(d => { DocAudioSource.Add(d); });

            audioOutPutList.Result.ToList().ForEach(o => { AudioOutPutDevice.Add(o); });
            aac.ForEach(o => { Aac.Add(o.AAC); });
            sampleRateList.ForEach(o => { SampleRate.Add(o.SampleRate); });
            AudioSource.Add(string.Empty);
            DocAudioSource.Add(string.Empty);

            //设置默认选项
            SetDefaultAudioSetting();

            if (audioSourceList.Result.All(o => o != SelectedAudioSource))
                SelectedAudioSource = string.Empty;
            if (docSourceList.Result.All(o => o != SelectedDocAudioSource))
                SelectedDocAudioSource = string.Empty;
            if (audioOutPutList.Result.All(o => o != SelectedAudioOutPutDevice))
                SelectedAudioOutPutDevice = string.Empty;

        }

        private void Init_Video_Settings()
        {
            CameraDeviceList.Clear();
            DocDeviceList.Clear();

            CameraColorSpaces.Clear();
            DocColorSpaces.Clear();

            VedioParameterVgaList.Clear();
            DocParameterVgaList.Clear();

            VedioParameterRatesList.Clear();


            var cameraList = _meetingSdkAgent.GetVideoDevices();

            if (cameraList.Result == null)
            {
                HasErrorMsg("-1", "无法获取本机视频设备信息！");
                return;
            }

            _cameraDeviceList = cameraList.Result.ToList();
            _docDeviceList = cameraList.Result.ToList();

            if (_settingParameter != null)
            {
                var rateList = _settingParameter.VedioParameterRates;
                rateList.ForEach(v => { VedioParameterRatesList.Add(v.VideoBitRate); });
            }
            _cameraDeviceList.ForEach(c => { CameraDeviceList.Add(c.DeviceName); });
            _docDeviceList.ForEach(d => { DocDeviceList.Add(d.DeviceName); });

            CameraDeviceList.Add("");
            DocDeviceList.Add("");
            SetVideoDefaultSetting();


            if (_cameraDeviceList.All(o => o.DeviceName != SelectedCameraDevice))
                SelectedCameraDevice = string.Empty;
            if (_docDeviceList.All(o => o.DeviceName != SelectedDocDevice))
                SelectedDocDevice = string.Empty;

        }


        //only one expander can be expanded at one time
        private void ManageExpanderStatue(bool isExpanded)
        {
            if (isExpanded)
            {
                if (IsMainCameraExpanded | IsSecondaryCameraExpanded | IsAudioExpanded | IsLiveExpanded |
                    IsServerLiveExpanded | IsRecordExpanded)
                {
                    IsMainCameraExpanded = false;
                    IsSecondaryCameraExpanded = false;
                    IsAudioExpanded = false;
                    IsLiveExpanded = false;
                    IsServerLiveExpanded = false;
                    IsRecordExpanded = false;
                }
            }
        }
    }
}
