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
            //ConfigItemChangedCommand = DelegateCommand<ConfigChangedItem>.FromAsyncHandler(ConfigItemChangedAsync);

            //SelectRecordPathCommand = new DelegateCommand(SelectRecordPath);
            //LiveUrlChangedCommand = new DelegateCommand(LiveUrlChangedHander);

            //InitializeBindingDataSource();
        }

        //private fields
        private readonly SettingContentView _view;
        private readonly IMeeting _sdkService;
        private readonly IDeviceNameAccessor _deviceNameAccessor;
        private readonly IDeviceConfigLoader _deviceConfigLoader;
        private readonly IMeetingSdkAgent _meetingSdkAgent;

        private AggregatedConfig _configManager = GlobalData.Instance.AggregatedConfig;


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


        // commands
        public ICommand CheckCameraDeviceCommand { get; set; }
        public ICommand CheckDocDeviceCommand { get; set; }

        public ICommand CheckPeopleSourceDeviceCommand { get; set; }
        public ICommand CheckDocSourceDeviceCommand { get; set; }

        public ICommand SelectRecordPathCommand { get; set; }


        private ConfigItemTag configItemTag;

        public ConfigItemTag ConfigItemTag
        {
            get { return configItemTag; }
            set { SetProperty(ref configItemTag, value); }
        }

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

        //private string _pushUrlExplanation;
        //public string PushUrlExplanation
        //{
        //    get { return _pushUrlExplanation; }
        //    set { SetProperty(ref _pushUrlExplanation, value); }
        //}

        //commands
        public ICommand LoadSettingCommand { get; set; }
        public ICommand ConfigItemChangedCommand { get; set; }
        public ICommand LiveUrlChangedCommand { get; set; }

        //command handlers
        private void LoadSettingAsync()
        {
            try
            {
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

        private void Init_Live_Record_Settings()
        {
        }

        private void Init_Audio_Settings()
        {
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

            var settingLocalData = GetParametersAsync();

            if (settingLocalData != null)
            {
                var rateList = settingLocalData.VedioParameterRates;
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

        //private async Task ConfigItemChangedAsync(ConfigChangedItem configChangedItem)
        //{
        //    if (string.IsNullOrEmpty(configChangedItem.value))
        //    {
        //        return;
        //    }

        //    await Task.Run(async () =>
        //    {

        //        switch (configChangedItem.key)
        //        {
        //            case ConfigItemKey.MainCamera:
        //                await RefreshExclusiveDataSourceAsync(ConfigItemKey.MainCamera, configChangedItem.value);
        //                _sdkService.SetDefaultDevice(1, configChangedItem.value);


        //                await _view.Dispatcher.BeginInvoke(new Action(() =>
        //                {
        //                    RefreshResolutionsAsync(configChangedItem);
        //                }));
        //                break;

        //            case ConfigItemKey.MainCameraResolution:
        //                string[] mainResolution = configChangedItem.value.Split('*');
        //                _sdkService.SetVideoResolution(1, int.Parse(mainResolution[0]), int.Parse(mainResolution[1]));
        //                break;

        //            case ConfigItemKey.MainCameraCodeRate:
        //                _sdkService.SetVideoBitRate(1, int.Parse(configChangedItem.value));

        //                break;
        //            case ConfigItemKey.SecondaryCamera:
        //                await RefreshExclusiveDataSourceAsync(ConfigItemKey.SecondaryCamera, configChangedItem.value);
        //                _sdkService.SetDefaultDevice(2, configChangedItem.value);

        //                await _view.Dispatcher.BeginInvoke(new Action(() =>
        //                {
        //                    RefreshResolutionsAsync(configChangedItem);
        //                }));
        //                break;
        //            case ConfigItemKey.SecondaryCameraResolution:
        //                string[] secondaryResolution = configChangedItem.value.Split('*');
        //                _sdkService.SetVideoResolution(2, int.Parse(secondaryResolution[0]),
        //                    int.Parse(secondaryResolution[1]));
        //                break;
        //            case ConfigItemKey.SecondaryCameraCodeRate:
        //                _sdkService.SetVideoBitRate(2, int.Parse(configChangedItem.value));
        //                break;
        //            case ConfigItemKey.MainMicrophone:
        //                await RefreshExclusiveDataSourceAsync(ConfigItemKey.MainMicrophone, configChangedItem.value);
        //                _sdkService.SetDefaultDevice(3, configChangedItem.value);

        //                break;
        //            case ConfigItemKey.SecondaryMicrophone:
        //                await
        //                    RefreshExclusiveDataSourceAsync(ConfigItemKey.SecondaryMicrophone, configChangedItem.value);
        //                _sdkService.SetDefaultDevice(5, configChangedItem.value);

        //                break;
        //            case ConfigItemKey.Speaker:
        //                _sdkService.SetDefaultDevice(4, configChangedItem.value);
        //                break;
        //            case ConfigItemKey.AudioSampleRate:
        //                _sdkService.SetAudioSampleRate(int.Parse(configChangedItem.value));
        //                break;
        //            case ConfigItemKey.AudioCodeRate:
        //                _sdkService.SetAudioBitRate(int.Parse(configChangedItem.value));
        //                break;
        //            case ConfigItemKey.LiveResolution:

        //                break;
        //            case ConfigItemKey.LiveCodeRate:

        //                break;
        //            case ConfigItemKey.Unknown:
        //            default:
        //                break;
        //        }

        //        SaveConfig();
        //    });
        //}

        //private void RefreshResolutionsAsync(ConfigChangedItem configChangedItem)
        //{
        //    if (configChangedItem.value == NonExclusiveItem)
        //    {
        //        return;
        //    }

        //    Camera videoDeviceInfo = _sdkService.GetCameraInfo(configChangedItem.value);


        //    if (configChangedItem.key == ConfigItemKey.MainCamera)
        //    {
        //        if (videoDeviceInfo.CameraParameters.Count > 0)
        //        {
        //            MeetingConfigParameter.UserCameraSetting.ResolutionList.Clear();

        //            CameraParameter cameraParameter = videoDeviceInfo.CameraParameters[0];

        //            foreach (Size t in cameraParameter.VideoSizes)
        //            {
        //                string resolution = $"{t.Width}*{t.Height}";

        //                if (!MeetingConfigParameter.UserCameraSetting.ResolutionList.Contains(resolution))
        //                {
        //                    MeetingConfigParameter.UserCameraSetting.ResolutionList.Add(resolution);
        //                }
        //            }

        //        }

        //        if (
        //            MeetingConfigParameter.UserCameraSetting.ResolutionList.Contains(
        //                GlobalData.Instance.AggregatedConfig.MainCamera.Resolution))
        //        {
        //            MeetingConfigResult.MainCamera.Resolution =
        //                GlobalData.Instance.AggregatedConfig.MainCamera.Resolution;
        //        }
        //        else if (MeetingConfigParameter.UserCameraSetting.ResolutionList.Count > 0)
        //        {
        //            MeetingConfigResult.MainCamera.Resolution =
        //                MeetingConfigParameter.UserCameraSetting.ResolutionList[0];
        //        }
        //    }

        //    if (configChangedItem.key == ConfigItemKey.SecondaryCamera)
        //    {
        //        if (videoDeviceInfo.CameraParameters.Count > 0)
        //        {
        //            MeetingConfigParameter.DataCameraSetting.ResolutionList.Clear();

        //            CameraParameter cameraParameter = videoDeviceInfo.CameraParameters[0];

        //            foreach (Size t in cameraParameter.VideoSizes)
        //            {
        //                string resolution = $"{t.Width}*{t.Height}";

        //                if (!MeetingConfigParameter.DataCameraSetting.ResolutionList.Contains(resolution))
        //                {
        //                    MeetingConfigParameter.DataCameraSetting.ResolutionList.Add(resolution);
        //                }
        //            }
        //        }

        //        if (
        //            MeetingConfigParameter.DataCameraSetting.ResolutionList.Contains(
        //                GlobalData.Instance.AggregatedConfig.SecondaryCamera.Resolution))
        //        {
        //            MeetingConfigResult.SecondaryCamera.Resolution =
        //                GlobalData.Instance.AggregatedConfig.SecondaryCamera.Resolution;
        //        }
        //        else if (MeetingConfigParameter.DataCameraSetting.ResolutionList.Count > 0)
        //        {
        //            MeetingConfigResult.SecondaryCamera.Resolution =
        //                MeetingConfigParameter.DataCameraSetting.ResolutionList[0];
        //        }
        //    }
        //}

        //private void SelectRecordPath()
        //{
        //    FolderBrowserDialog fbd = new FolderBrowserDialog {SelectedPath = Environment.CurrentDirectory};
        //    if (fbd.ShowDialog() == DialogResult.OK)
        //    {
        //        MeetingConfigResult.RecordConfig.RecordPath = fbd.SelectedPath;
        //        SaveConfig();
        //    }
        //}

        //private void LiveUrlChangedHander()
        //{
        //    if (
        //        !string.IsNullOrEmpty(
        //            MeetingConfigResult.LocalLiveConfig.PushLiveStreamUrl) &&
        //        Uri.IsWellFormedUriString(MeetingConfigResult.LocalLiveConfig.PushLiveStreamUrl,
        //            UriKind.Absolute))
        //    {
        //        LiveUrlColor = "White";
        //        SaveConfig();
        //    }
        //    else
        //    {
        //        LiveUrlColor = "Red";
        //    }
        //}


        //methods
        //public void InitializeBindingDataSource()
        //{
        //    CachedCameras = new List<VideoDeviceModel>();
        //    CachedMicrophones = new List<string>();
        //    CachedSpeakers = new List<string>();

        //    MainCameras = new ObservableCollection<string>();
        //    SecondaryCameras = new ObservableCollection<string>();

        //    MainColorspaces = new ObservableCollection<VideoFormatModel>();
        //    SecondaryColorspaces = new ObservableCollection<VideoFormatModel>();

        //    MainMicrophones = new ObservableCollection<string>();
        //    SecondaryMicrophones = new ObservableCollection<string>();
        //    Speakers = new ObservableCollection<string>();
        //    MeetingConfigResult = new AggregatedConfig();
        //    MeetingConfigParameter = new MeetingSetting();

        //    ConfigItemTag = new ConfigItemTag();
        //}

        //private async Task CacheDeviceListAsync()
        //{
        //    await Task.Run(() =>
        //    {
        //        var cameraList = _meetingSdkAgent.GetVideoDevices();

        //        if (cameraList.Result != null)
        //        {
        //            CachedCameras = cameraList.Result.ToList();
        //        }


        //        var micList = _meetingSdkAgent.GetMicrophones();

        //        if (micList.Result != null)
        //        {
        //            CachedMicrophones = micList.Result.ToList();
        //        }

        //        CachedMicrophones.Add(NonExclusiveItem);

        //        var speakerList = _meetingSdkAgent.GetLoudSpeakers();

        //        if (speakerList.Result != null)
        //        {
        //            CachedSpeakers = speakerList.Result.ToList();
        //        }

        //        CachedSpeakers.Add(NonExclusiveItem);
        //    });
        //}


        //private async Task SetupDeviceListAsync()
        //{
        //    await _meetingConfigView.Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        ClearDeviceList();
        //        CachedCameras.ForEach((camera) =>
        //        {
        //            MainCameras.Add(camera.DeviceName);
        //            SecondaryCameras.Add(camera.DeviceName);
        //        });

        //        CachedMicrophones.ForEach((microphone) =>
        //        {
        //            MainMicrophones.Add(microphone);
        //            SecondaryMicrophones.Add(microphone);
        //        });

        //        CachedSpeakers.ForEach((speaker) =>
        //        {
        //            Speakers.Add(speaker);
        //        });
        //    }));
        //}

        //private async Task GetConfigFromGlobalConfigAsync()
        //{
        //    MeetingConfigResult.CloneConfig(GlobalData.Instance.AggregatedConfig);

        //    await AutoSelectConfigAsync();
        //}


        //private async Task AutoSelectConfigAsync()
        //{
        //    await _meetingConfigView.Dispatcher.BeginInvoke(new Action(async () =>
        //    {
        //        if (string.IsNullOrEmpty(MeetingConfigResult.MainCamera.Resolution))
        //        {
        //            MeetingConfigResult.MainCamera.Resolution =
        //                MeetingConfigParameter.UserCameraSetting.ResolutionList.Count > 0
        //                    ? MeetingConfigParameter.UserCameraSetting.ResolutionList[0]
        //                    : null;
        //        }

        //        if (string.IsNullOrEmpty(MeetingConfigResult.MainCamera.CodeRate))
        //        {
        //            MeetingConfigResult.MainCamera.CodeRate =
        //                MeetingConfigParameter.UserCameraSetting.BitRateList.Count > 0
        //                    ? MeetingConfigParameter.UserCameraSetting.BitRateList[0]
        //                    : null;

        //        }
        //        if (string.IsNullOrEmpty(MeetingConfigResult.SecondaryCamera.Resolution))
        //        {
        //            MeetingConfigResult.SecondaryCamera.Resolution =
        //                MeetingConfigParameter.DataCameraSetting.ResolutionList.Count > 0
        //                    ? MeetingConfigParameter.DataCameraSetting.ResolutionList[0]
        //                    : null;

        //        }
        //        if (string.IsNullOrEmpty(MeetingConfigResult.SecondaryCamera.CodeRate))
        //        {
        //            MeetingConfigResult.SecondaryCamera.CodeRate =
        //                MeetingConfigParameter.DataCameraSetting.BitRateList.Count > 0
        //                    ? MeetingConfigParameter.DataCameraSetting.BitRateList[0]
        //                    : null;

        //        }
        //        if (string.IsNullOrEmpty(MeetingConfigResult.AudioConfig.Speaker))
        //        {
        //            MeetingConfigResult.AudioConfig.Speaker = Speakers.Count > 0 ? Speakers[0] : null;

        //        }
        //        if (string.IsNullOrEmpty(MeetingConfigResult.AudioConfig.SampleRate))
        //        {
        //            MeetingConfigResult.AudioConfig.SampleRate = MeetingConfigParameter.Audio.SampleRateList.Count > 0
        //                ? MeetingConfigParameter.Audio.SampleRateList[0]
        //                : null;

        //        }
        //        if (string.IsNullOrEmpty(MeetingConfigResult.AudioConfig.CodeRate))
        //        {
        //            MeetingConfigResult.AudioConfig.CodeRate = MeetingConfigParameter.Audio.BitRateList.Count > 0
        //                ? MeetingConfigParameter.Audio.BitRateList[0]
        //                : null;

        //        }
        //        if (string.IsNullOrEmpty(MeetingConfigResult.LocalLiveConfig.Resolution))
        //        {
        //            MeetingConfigResult.LocalLiveConfig.Resolution = MeetingConfigParameter.Live.ResolutionList.Count >
        //                                                             0
        //                ? MeetingConfigParameter.Live.ResolutionList[0]
        //                : null;

        //        }
        //        if (string.IsNullOrEmpty(MeetingConfigResult.LocalLiveConfig.CodeRate))
        //        {
        //            MeetingConfigResult.LocalLiveConfig.CodeRate = MeetingConfigParameter.Live.BitRateList.Count > 0
        //                ? MeetingConfigParameter.Live.BitRateList[0]
        //                : null;

        //        }
        //        if (string.IsNullOrEmpty(MeetingConfigResult.RemoteLiveConfig.Resolution))
        //        {
        //            MeetingConfigResult.RemoteLiveConfig.Resolution = MeetingConfigParameter.Live.ResolutionList.Count >
        //                                                              0
        //                ? MeetingConfigParameter.Live.ResolutionList[0]
        //                : null;

        //        }
        //        if (string.IsNullOrEmpty(MeetingConfigResult.RemoteLiveConfig.CodeRate))
        //        {
        //            MeetingConfigResult.RemoteLiveConfig.CodeRate = MeetingConfigParameter.Live.BitRateList.Count > 0
        //                ? MeetingConfigParameter.Live.BitRateList[0]
        //                : null;

        //        }

        //        if (string.IsNullOrEmpty(MeetingConfigResult.RecordConfig.Resolution))
        //        {
        //            MeetingConfigResult.RecordConfig.Resolution = MeetingConfigParameter.Live.ResolutionList.Count > 0
        //                ? MeetingConfigParameter.Live.ResolutionList[0]
        //                : null;

        //        }
        //        if (string.IsNullOrEmpty(MeetingConfigResult.RecordConfig.CodeRate))
        //        {
        //            MeetingConfigResult.RecordConfig.CodeRate = MeetingConfigParameter.Live.BitRateList.Count > 0
        //                ? MeetingConfigParameter.Live.BitRateList[0]
        //                : null;

        //        }


        //        //exclusive1
        //        if (string.IsNullOrEmpty(MeetingConfigResult.MainCamera.Name) && MainCameras.Count > 0)
        //        {
        //            MeetingConfigResult.MainCamera.Name = MainCameras[0];

        //            RefreshResolutionsAsync(new ConfigChangedItem()
        //            {
        //                key = ConfigItemKey.MainCamera,
        //                value = MainCameras[0]
        //            });

        //            if (MainCameras[0] != NonExclusiveItem)
        //            {
        //                SecondaryCameras.Remove(MainCameras[0]);
        //            }
        //        }

        //        //exclusive1
        //        if (string.IsNullOrEmpty(MeetingConfigResult.SecondaryCamera.Name) && SecondaryCameras.Count > 0)
        //        {
        //            MeetingConfigResult.SecondaryCamera.Name = SecondaryCameras[0];

        //            RefreshResolutionsAsync(new ConfigChangedItem()
        //            {
        //                key = ConfigItemKey.SecondaryCamera,
        //                value = SecondaryCameras[0]
        //            });

        //            if (SecondaryCameras[0] != NonExclusiveItem)
        //            {
        //                MainCameras.Remove(SecondaryCameras[0]);
        //            }
        //        }

        //        //exclusive2
        //        if (string.IsNullOrEmpty(MeetingConfigResult.AudioConfig.MainMicrophone) && MainMicrophones.Count > 0)
        //        {
        //            MeetingConfigResult.AudioConfig.MainMicrophone = MainMicrophones[0];

        //            if (MainMicrophones[0] != NonExclusiveItem)
        //            {
        //                SecondaryMicrophones.Remove(MainMicrophones[0]);
        //            }
        //        }

        //        //exclusive2
        //        if (string.IsNullOrEmpty(MeetingConfigResult.AudioConfig.SecondaryMicrophone) &&
        //            SecondaryMicrophones.Count > 0)
        //        {
        //            MeetingConfigResult.AudioConfig.SecondaryMicrophone = SecondaryMicrophones[0];

        //            if (SecondaryMicrophones[0] != NonExclusiveItem)
        //            {
        //                MainMicrophones.Remove(SecondaryMicrophones[0]);
        //            }
        //        }


        //        if (MainCameras.Contains(MeetingConfigResult.MainCamera.Name))
        //        {
        //            await
        //                RefreshExclusiveDataSourceAsync(ConfigItemKey.MainCamera, MeetingConfigResult.MainCamera.Name);

        //            RefreshResolutionsAsync(new ConfigChangedItem()
        //            {
        //                key = ConfigItemKey.MainCamera,
        //                value = MeetingConfigResult.MainCamera.Name
        //            });
        //        }

        //        if (SecondaryCameras.Contains(MeetingConfigResult.SecondaryCamera.Name))
        //        {
        //            await
        //                RefreshExclusiveDataSourceAsync(ConfigItemKey.SecondaryCamera,
        //                    MeetingConfigResult.SecondaryCamera.Name);

        //            RefreshResolutionsAsync(new ConfigChangedItem()
        //            {
        //                key = ConfigItemKey.SecondaryCamera,
        //                value = MeetingConfigResult.SecondaryCamera.Name
        //            });
        //        }

        //        if (MainMicrophones.Contains(MeetingConfigResult.AudioConfig.MainMicrophone))
        //        {
        //            await
        //                RefreshExclusiveDataSourceAsync(ConfigItemKey.MainMicrophone,
        //                    MeetingConfigResult.AudioConfig.MainMicrophone);
        //        }

        //        if (SecondaryMicrophones.Contains(MeetingConfigResult.AudioConfig.SecondaryMicrophone))
        //        {
        //            await
        //                RefreshExclusiveDataSourceAsync(ConfigItemKey.SecondaryMicrophone,
        //                    MeetingConfigResult.AudioConfig.SecondaryMicrophone);
        //        }

        //        if (string.IsNullOrEmpty(MeetingConfigResult.RecordConfig.RecordPath))
        //        {
        //            MeetingConfigResult.RecordConfig.RecordPath =
        //                Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        //        }

        //        SaveConfig();
        //    }));
        //}

        //private async Task SetDefaultConfigResultsAsync()
        //{
        //    await Task.Run(() =>
        //    {
        //        if (MeetingConfigResult.MainCamera.Name != NonExclusiveItem)
        //        {
        //            _sdkService.SetDefaultDevice(1, MeetingConfigResult.MainCamera.Name);
        //            string[] mainResolution = MeetingConfigResult.MainCamera.Resolution.Split('*');
        //            _sdkService.SetVideoResolution(1, int.Parse(mainResolution[0]), int.Parse(mainResolution[1]));
        //            _sdkService.SetVideoBitRate(1, int.Parse(MeetingConfigResult.MainCamera.CodeRate));
        //        }

        //        if (MeetingConfigResult.SecondaryCamera.Name != NonExclusiveItem)
        //        {
        //            _sdkService.SetDefaultDevice(2, MeetingConfigResult.SecondaryCamera.Name);
        //            string[] secondaryResolution = MeetingConfigResult.SecondaryCamera.Resolution.Split('*');
        //            _sdkService.SetVideoResolution(2, int.Parse(secondaryResolution[0]),
        //                int.Parse(secondaryResolution[1]));
        //            _sdkService.SetVideoBitRate(2, int.Parse(MeetingConfigResult.SecondaryCamera.CodeRate));
        //        }

        //        if (MeetingConfigResult.AudioConfig.MainMicrophone != NonExclusiveItem)
        //        {
        //            _sdkService.SetDefaultDevice(3, MeetingConfigResult.AudioConfig.MainMicrophone);
        //        }

        //        if (MeetingConfigResult.AudioConfig.SecondaryMicrophone != NonExclusiveItem)
        //        {
        //            _sdkService.SetDefaultDevice(5, MeetingConfigResult.AudioConfig.SecondaryMicrophone);
        //        }

        //        _sdkService.SetDefaultDevice(4, MeetingConfigResult.AudioConfig.Speaker);

        //        _sdkService.SetAudioSampleRate(int.Parse(MeetingConfigResult.AudioConfig.SampleRate));
        //        _sdkService.SetAudioBitRate(int.Parse(MeetingConfigResult.AudioConfig.CodeRate));
        //    });
        //}

        //private void SaveConfig()
        //{
        //    try
        //    {
        //        string configResultPath = Path.Combine(Environment.CurrentDirectory, Common.GlobalResources.ConfigPath);

        //        GlobalData.Instance.AggregatedConfig.CloneConfig(MeetingConfigResult);

        //        string json = JsonConvert.SerializeObject(MeetingConfigResult, Formatting.Indented);

        //        File.WriteAllText(configResultPath, json, Encoding.UTF8);
        //        //SscUpdateManager.WriteConfigToVersionFolder(json);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Logger.Error($"【save config exception in setting page】：{ex}");
        //    }
        //}

        //private async Task RefreshExclusiveDataSourceAsync(ConfigItemKey configItemKey, string exclusiveItem)
        //{
        //    await _meetingConfigView.Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        switch (configItemKey)
        //        {
        //            case ConfigItemKey.MainCamera:

        //                string tempSecondaryCamera = MeetingConfigResult.SecondaryCamera.Name;

        //                SecondaryCameras.Clear();

        //                CachedCameras.ForEach((camera) =>
        //                {
        //                    if (camera != exclusiveItem)
        //                    {
        //                        SecondaryCameras.Add(camera);
        //                    }
        //                });
        //                if (!SecondaryCameras.Contains(NonExclusiveItem))
        //                {
        //                    SecondaryCameras.Add(NonExclusiveItem);
        //                }
        //                MeetingConfigResult.SecondaryCamera.Name = tempSecondaryCamera;

        //                break;
        //            case ConfigItemKey.SecondaryCamera:

        //                string tempMainCamera = MeetingConfigResult.MainCamera.Name;
        //                MainCameras.Clear();
        //                CachedCameras.ForEach((camera) =>
        //                {
        //                    if (camera != exclusiveItem)
        //                    {
        //                        MainCameras.Add(camera);
        //                    }
        //                });

        //                if (!MainCameras.Contains(NonExclusiveItem))
        //                {
        //                    MainCameras.Add(NonExclusiveItem);
        //                }
        //                MeetingConfigResult.MainCamera.Name = tempMainCamera;

        //                break;
        //            case ConfigItemKey.MainMicrophone:
        //                string tempSecondaryMic = MeetingConfigResult.AudioConfig.SecondaryMicrophone;

        //                SecondaryMicrophones.Clear();

        //                CachedMicrophones.ForEach((mic) =>
        //                {
        //                    if (mic != exclusiveItem)
        //                    {
        //                        SecondaryMicrophones.Add(mic);
        //                    }
        //                });

        //                if (!SecondaryMicrophones.Contains(NonExclusiveItem))
        //                {
        //                    SecondaryMicrophones.Add(NonExclusiveItem);
        //                }

        //                MeetingConfigResult.AudioConfig.SecondaryMicrophone = tempSecondaryMic;
        //                break;
        //            case ConfigItemKey.SecondaryMicrophone:
        //                string tempMainMic = MeetingConfigResult.AudioConfig.MainMicrophone;

        //                MainMicrophones.Clear();
        //                CachedMicrophones.ForEach((mic) =>
        //                {
        //                    if (mic != exclusiveItem)
        //                    {
        //                        MainMicrophones.Add(mic);
        //                    }
        //                });

        //                if (!MainMicrophones.Contains(NonExclusiveItem))
        //                {
        //                    MainMicrophones.Add(NonExclusiveItem);
        //                }
        //                MeetingConfigResult.AudioConfig.MainMicrophone = tempMainMic;
        //                break;
        //            case ConfigItemKey.Unknown:
        //            default:
        //                break;
        //        }

        //    }));
        //}

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

        //private async Task CloneParameters(MeetingSetting newParameters)
        //{
        //    await _view.Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        MeetingConfigParameter.Audio.Clear();
        //        MeetingConfigParameter.UserCameraSetting.ResolutionList.Clear();
        //        MeetingConfigParameter.UserCameraSetting.BitRateList.Clear();
        //        MeetingConfigParameter.DataCameraSetting.ResolutionList.Clear();
        //        MeetingConfigParameter.DataCameraSetting.BitRateList.Clear();
        //        MeetingConfigParameter.Live.ResolutionList.Clear();
        //        MeetingConfigParameter.Live.BitRateList.Clear();

        //        newParameters.Audio.BitRateList.ToList().ForEach(bitrate =>
        //        {
        //            MeetingConfigParameter.Audio.BitRateList.Add(bitrate);
        //        });

        //        newParameters.Audio.SampleRateList.ToList().ForEach(samplerate =>
        //        {
        //            MeetingConfigParameter.Audio.SampleRateList.Add(samplerate);
        //        });

        //        newParameters.UserCameraSetting.BitRateList.ToList().ForEach(bitrate =>
        //        {
        //            MeetingConfigParameter.UserCameraSetting.BitRateList.Add(bitrate);
        //        });

        //        newParameters.UserCameraSetting.ResolutionList.ToList().ForEach(resolution =>
        //        {
        //            MeetingConfigParameter.UserCameraSetting.ResolutionList.Add(resolution);
        //        });

        //        newParameters.DataCameraSetting.BitRateList.ToList().ForEach(bitrate =>
        //        {
        //            MeetingConfigParameter.DataCameraSetting.BitRateList.Add(bitrate);
        //        });

        //        newParameters.DataCameraSetting.ResolutionList.ToList().ForEach(resolution =>
        //        {
        //            MeetingConfigParameter.DataCameraSetting.ResolutionList.Add(resolution);
        //        });

        //        newParameters.Live.BitRateList.ToList().ForEach(bitrate =>
        //        {
        //            MeetingConfigParameter.Live.BitRateList.Add(bitrate);
        //        });

        //        newParameters.Live.ResolutionList.ToList().ForEach(resolution =>
        //        {
        //            MeetingConfigParameter.Live.ResolutionList.Add(resolution);
        //        });
        //    }));
        //}

        //private void ClearDeviceList()
        //{
        //    MainCameras.Clear();
        //    MainColorspaces.Clear();
        //    SecondaryCameras.Clear();
        //    SecondaryColorspaces.Clear();
        //    MainMicrophones.Clear();
        //    SecondaryMicrophones.Clear();
        //    Speakers.Clear();
        //}
    }
}
