using Prism.Commands;
using St.Common;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using Caliburn.Micro;
using Common;
using MeetingSdk.SdkWrapper;
using MeetingSdk.SdkWrapper.MeetingDataModel;
using Prism.Events;
using MenuItem = System.Windows.Controls.MenuItem;
using Serilog;
using St.Common.Commands;
using Action = System.Action;
using LogManager = St.Common.LogManager;
using MeetingSdk.Wpf;
using UserInfo = St.Common.UserInfo;
using MeetingSdk.NetAgent;
using St.Common.Helper;
using MeetingSdk.NetAgent.Models;
using MeetingSdk.Wpf.Interfaces;

namespace St.Meeting
{
    public class MeetingViewModel : ViewModelBase,IExitMeeting
    {
        protected override bool HasErrorMsg(string status, string msg)
        {
            IsMenuOpen = true;
            return base.HasErrorMsg(status, msg);
        }

        public MeetingViewModel(MeetingView meetingView, Action<bool, string> startMeetingCallback,
            Action<bool, string> exitMeetingCallback)
        {
            _meetingView = meetingView;

            //_viewLayoutService = IoC.Get<IViewLayout>();
            //_viewLayoutService.ViewFrameList = InitializeViewFrameList(meetingView);

            _sdkService = IoC.Get<IMeeting>();
            _bmsService = IoC.Get<IBms>();

            _deviceNameAccessor = IoC.Get<IDeviceNameAccessor>();

            _windowManager = IoC.Get<IMeetingWindowManager>();
            _meetingSdkAgent = IoC.Get<IMeetingSdkAgent>();

            _eventAggregator = IoC.Get<IEventAggregator>();
            _eventAggregator.GetEvent<CommandReceivedEvent>()
                .Subscribe(ExecuteCommand, ThreadOption.PublisherThread, false, command => command.IsIntoClassCommand);

            _manualPushLive = IoC.Get<IPushLive>(GlobalResources.LocalPushLive);
            _manualPushLive.ResetStatus();

            _serverPushLiveService = IoC.Get<IPushLive>(GlobalResources.RemotePushLive);
            _serverPushLiveService.ResetStatus();


            _localRecordService = IoC.Get<IRecord>();
            _localRecordService.ResetStatus();

            _startMeetingCallbackEvent = startMeetingCallback;
            _exitMeetingCallbackEvent = exitMeetingCallback;

            //MeetingId = _sdkService.MeetingId;
            SpeakingStatus = IsNotSpeaking;
            //SelfDescription = $"{_sdkService.SelfName}-{_sdkService.SelfPhoneId}";

            _lessonDetail = IoC.Get<LessonDetail>();
            _userInfo = IoC.Get<UserInfo>();
            _userInfos = IoC.Get<List<UserInfo>>();

            MeetingOrLesson = _lessonDetail.Id == 0 ? "会议号:" : "课程号:";
            LessonName = string.IsNullOrEmpty(_lessonDetail.Name)
                ? string.Empty
                : string.Format($"课程名:{_lessonDetail.Name}");



            RegisterMeetingEvents();


            LoadCommand = DelegateCommand.FromAsyncHandler(LoadAsync);
            ModeChangedCommand = new DelegateCommand<string>(MeetingModeChangedAsync);
            SpeakingStatusChangedCommand = DelegateCommand.FromAsyncHandler(SpeakingStatusChangedAsync);
            ExternalDataChangedCommand = DelegateCommand.FromAsyncHandler(ExternalDataChangedAsync);
            SharingDesktopCommand = DelegateCommand.FromAsyncHandler(SharingDesktopAsync);
            CancelSharingCommand = DelegateCommand.FromAsyncHandler(CancelSharingAsync);
            ExitCommand = DelegateCommand.FromAsyncHandler(ExitAsync);
            OpenExitDialogCommand = DelegateCommand.FromAsyncHandler(OpenExitDialogAsync);
            KickoutCommand = new DelegateCommand<string>(KickoutAsync);
            OpenCloseCameraCommand = DelegateCommand.FromAsyncHandler(OpenCloseCameraAsync);
            GetCameraInfoCommand = DelegateCommand<string>.FromAsyncHandler(GetCameraInfoAsync);
            OpenPropertyPageCommand = DelegateCommand<string>.FromAsyncHandler(OpenPropertyPageAsync);
            SetDefaultDataCameraCommand = DelegateCommand<string>.FromAsyncHandler(SetDefaultDataCameraAsync);
            SetDefaultFigureCameraCommand = DelegateCommand<string>.FromAsyncHandler(SetDefaultFigureCameraAsync);
            SetMicStateCommand = DelegateCommand.FromAsyncHandler(SetMicStateAsync);
            ScreenShareCommand = DelegateCommand.FromAsyncHandler(ScreenShareAsync);
            StartSpeakCommand = new DelegateCommand<string>(StartSpeakAsync);
            StopSpeakCommand = new DelegateCommand<string>(StopSpeakAsync);
            RecordCommand = new DelegateCommand(RecordAsync);
            PushLiveCommand = new DelegateCommand(PushLiveAsync);
            WindowKeyDownCommand = new DelegateCommand<object>(WindowKeyDownHandler);

            TopMostTriggerCommand = new DelegateCommand(() =>
            {
                IsTopMost = !IsTopMost;
                string msg = IsTopMost ? "当前窗口已经置顶！" : "取消当前窗口置顶！";
                MessageQueueManager.Instance.AddInfo(msg);
            });
            ShowLogCommand = new DelegateCommand(async () =>
            {
                IsTopMost = false;
                await LogManager.ShowLogAsync();
            });
            ShowHelpCommand = new DelegateCommand(ShowHelp);

            InitializeMenuItems();
        }

        private async void ExecuteCommand(SscCommand command)
        {
            if (!command.Enabled)
            {
                return;
            }

            if (command.Directive == GlobalCommands.Instance.ExitClassCommand.Directive)
            {
                await OpenExitDialogAsync();
            }
            else if (command.Directive == GlobalCommands.Instance.SpeakCommand.Directive)
            {
                await SpeakingStatusChangedAsync();
            }
            else if (command.Directive == GlobalCommands.Instance.RecordCommand.Directive)
            {
                // monitor click operation
                RecordChecked = !RecordChecked;
                RecordAsync();
            }
            else if (command.Directive == GlobalCommands.Instance.PushLiveCommand.Directive)
            {
                PushLiveChecked = !PushLiveChecked;
                PushLiveAsync();
            }
            else if (command.Directive == GlobalCommands.Instance.DocCommand.Directive)
            {
                if (SharingVisibility == Visibility.Visible)
                {
                    var docItem = SharingMenuItems.First(menu => menu.Header.ToString() != "桌面");


                    await ExternalDataChangedAsync();

                    //if (docItem != null && docItem.HasItems)
                    //{
                    //    MenuItem docMenuItem = docItem.Items[0] as MenuItem;
                    //    if (docMenuItem == null)
                    //    {
                    //        return;
                    //    }
                    //    await ExternalDataChangedAsync(docMenuItem.Header.ToString());
                    //}
                    //else
                    //{
                    //    MessageQueueManager.Instance.AddInfo("没有可共享的外接设备！");
                    //}
                }
                else
                {
                    await CancelSharingAsync();
                }
            }
            else if (command.Directive == GlobalCommands.Instance.AverageCommand.Directive)
            {
                //await ViewModeChangedAsync(ViewMode.Average);
            }
            else if (command.Directive == GlobalCommands.Instance.BigSmallsCommand.Directive)
            {
                //var openedVfs = _viewLayoutService.ViewFrameList.Where(vf => vf.IsOpened).ToList();

                //int viewCount = openedVfs.Count;

                //if (viewCount == 0)
                //{
                //    return;
                //}

                //int bigViewIndex = -1;
                //for (int i = 0; i < viewCount; i++)
                //{
                //    if (openedVfs[i].IsBigView)
                //    {
                //        bigViewIndex = i;
                //        break;
                //    }
                //}

                //if (bigViewIndex == -1)
                //{
                //    bigViewIndex = 0;
                //}
                //else if (bigViewIndex + 1 >= openedVfs.Count)
                //{
                //    bigViewIndex = 0;
                //}
                //else
                //{
                //    bigViewIndex += 1;
                //}

                //await BigViewChangedAsync(openedVfs[bigViewIndex]);

            }
            else if (command.Directive == GlobalCommands.Instance.CloseupCommand.Directive)
            {
                //var openedVfs = _viewLayoutService.ViewFrameList.Where(vf => vf.IsOpened).ToList();

                //int viewCount = openedVfs.Count;

                //if (viewCount == 0)
                //{
                //    return;
                //}

                //int fullViewIndex = -1;
                //for (int i = 0; i < viewCount; i++)
                //{
                //    if (openedVfs[i] == _viewLayoutService.FullScreenView)
                //    {
                //        fullViewIndex = i;
                //        break;
                //    }
                //}

                //if (fullViewIndex == -1)
                //{
                //    fullViewIndex = 0;
                //}
                //else if (fullViewIndex + 1 >= openedVfs.Count)
                //{
                //    fullViewIndex = 0;
                //}
                //else
                //{
                //    fullViewIndex += 1;
                //}

                //await FullScreenViewChangedAsync(openedVfs[fullViewIndex]);
            }
            else if (command.Directive == GlobalCommands.Instance.InteractionCommand.Directive)
            {
                //await MeetingModeChangedAsync(Common.MeetingMode.Interaction.ToString());
            }
            else if (command.Directive == GlobalCommands.Instance.SpeakerCommand.Directive)
            {
                //await MeetingModeChangedAsync(Common.MeetingMode.Speaker.ToString());
            }
            else if (command.Directive == GlobalCommands.Instance.ShareCommand.Directive)
            {
                //await MeetingModeChangedAsync(Common.MeetingMode.Sharing.ToString());
            }
        }

        private void ShowHelp()
        {
            string helpMsg = GlobalResources.HelpMessage;

            SscDialog helpSscDialog = new SscDialog(helpMsg);
            helpSscDialog.ShowDialog();
        }


        private void HandleKeyDown(Key key)
        {
            switch (key)
            {
                case Key.Left:
                    _pressedUpKeyCount = 0;
                    _pressedDownKeyCount = 0;
                    _pressedRightKeyCount = 0;

                    _pressedLeftKeyCount++;

                    break;
                case Key.Right:
                    _pressedUpKeyCount = 0;
                    _pressedDownKeyCount = 0;
                    _pressedLeftKeyCount = 0;

                    _pressedRightKeyCount++;

                    break;
                case Key.Up:
                    _pressedDownKeyCount = 0;
                    _pressedLeftKeyCount = 0;
                    _pressedRightKeyCount = 0;

                    _pressedUpKeyCount++;

                    if (_pressedUpKeyCount == 4)
                    {
                        _pressedUpKeyCount = 0;
                        _sdkService.ShowQosTool();
                    }

                    break;
                case Key.Down:
                    _pressedUpKeyCount = 0;
                    _pressedLeftKeyCount = 0;
                    _pressedRightKeyCount = 0;

                    _pressedDownKeyCount++;

                    if (_pressedDownKeyCount == 4)
                    {
                        _pressedDownKeyCount = 0;
                        _sdkService.CloseQosTool();
                    }

                    break;
                case Key.Enter:
                    IsMenuOpen = true;
                    _pressedUpKeyCount = 0;
                    _pressedDownKeyCount = 0;
                    _pressedLeftKeyCount = 0;
                    _pressedRightKeyCount = 0;
                    break;

                case Key.Escape:
                    IsMenuOpen = false;
                    _pressedUpKeyCount = 0;
                    _pressedDownKeyCount = 0;
                    _pressedLeftKeyCount = 0;
                    _pressedRightKeyCount = 0;
                    break;
                default:
                    _pressedUpKeyCount = 0;
                    _pressedDownKeyCount = 0;
                    _pressedLeftKeyCount = 0;
                    _pressedRightKeyCount = 0;
                    break;
            }
        }

        private  void WindowKeyDownHandler(object obj)
        {
            var keyEventArgs = obj as KeyEventArgs;
            HandleKeyDown(keyEventArgs.Key);
        }

        #region private fields

        private readonly MeetingView _meetingView;

        private delegate Task TaskDelegate();

        private TaskDelegate _cancelSharingAction;

        private readonly IMeetingWindowManager _windowManager;
        private readonly IMeetingSdkAgent _meetingSdkAgent;

        private readonly IDeviceNameAccessor _deviceNameAccessor;
        private readonly IEventAggregator _eventAggregator;
        private readonly IMeeting _sdkService;
        private readonly IBms _bmsService;
        private readonly IPushLive _manualPushLive;
        private readonly IPushLive _serverPushLiveService;
        private readonly IRecord _localRecordService;
        private readonly LessonDetail _lessonDetail;
        private readonly UserInfo _userInfo;
        private readonly List<UserInfo> _userInfos;
        private int _pressedUpKeyCount = 0;
        private int _pressedDownKeyCount = 0;
        private int _pressedLeftKeyCount = 0;
        private int _pressedRightKeyCount = 0;
        private bool _exitByDialog = false;

        private readonly Action<bool, string> _startMeetingCallbackEvent;
        private readonly Action<bool, string> _exitMeetingCallbackEvent;
        private const string IsSpeaking = "取消发言";
        private const string IsNotSpeaking = "发 言";

        #endregion

        #region public properties

        public ViewFrame ViewFrame1 { get; private set; }
        public ViewFrame ViewFrame2 { get; private set; }
        public ViewFrame ViewFrame3 { get; private set; }
        public ViewFrame ViewFrame4 { get; private set; }
        public ViewFrame ViewFrame5 { get; private set; }

        public ObservableCollection<MenuItem> ModeMenuItems { get; set; }
        public ObservableCollection<MenuItem> LayoutMenuItems { get; set; }
        public ObservableCollection<MenuItem> SharingMenuItems { get; set; }


        private string _meetingOrLesson;

        public string MeetingOrLesson
        {
            get { return _meetingOrLesson; }
            set { SetProperty(ref _meetingOrLesson, value); }
        }

        private string _lessonName;

        public string LessonName
        {
            get { return _lessonName; }
            set { SetProperty(ref _lessonName, value); }
        }


        //private string _selfDescription;

        //public string SelfDescription
        //{
        //    get { return _selfDescription; }
        //    set { SetProperty(ref _selfDescription, value); }
        //}

        private int _meetingId;

        public int MeetingId
        {
            get { return _meetingId; }
            set { SetProperty(ref _meetingId, value); }
        }


        private string _pushLiveStreamTips;

        public string PushLiveStreamTips
        {
            get { return _pushLiveStreamTips; }
            set { SetProperty(ref _pushLiveStreamTips, value); }
        }

        private string _recordTips;

        public string RecordTips
        {
            get { return _recordTips; }
            set { SetProperty(ref _recordTips, value); }
        }

        private string _selectedCamera;

        public string SelectedCamera
        {
            get { return _selectedCamera; }
            set { SetProperty(ref _selectedCamera, value); }
        }

        private string _openCloseCameraOperation = "open camera";

        public string OpenCloseCameraOperation
        {
            get { return _openCloseCameraOperation; }
            set { SetProperty(ref _openCloseCameraOperation, value); }
        }

        private string _openCloseDataOperation = "open data";

        public string OpenCloseDataOperation
        {
            get { return _openCloseDataOperation; }
            set { SetProperty(ref _openCloseDataOperation, value); }
        }

        private string _micState = "静音";

        public string MicState
        {
            get { return _micState; }
            set { SetProperty(ref _micState, value); }
        }

        private string _screenShareState = "共享屏幕";

        public string ScreenShareState
        {
            get { return _screenShareState; }
            set { SetProperty(ref _screenShareState, value); }
        }

        private string _phoneId;

        public string PhoneId
        {
            get { return _phoneId; }
            set { SetProperty(ref _phoneId, value); }
        }

        private string _startStopSpeakOperation = "发言";

        public string StartStopSpeakOperation
        {
            get { return _startStopSpeakOperation; }
            set { SetProperty(ref _startStopSpeakOperation, value); }
        }

        private bool _allowedToSpeak = true;

        public bool AllowedToSpeak
        {
            get { return _allowedToSpeak; }
            set { SetProperty(ref _allowedToSpeak, value); }
        }

        private string _phoneIds;

        public string PhoneIds
        {
            get { return _phoneIds; }
            set { SetProperty(ref _phoneIds, value); }
        }

        private bool _recordChecked;

        public bool RecordChecked
        {
            get { return _recordChecked; }
            set
            {
                if (!value)
                {
                    RecordTips = null;
                }
                SetProperty(ref _recordChecked, value);
            }
        }

        private bool _pushLiveChecked;

        public bool PushLiveChecked
        {
            get { return _pushLiveChecked; }
            set
            {
                if (!value)
                {
                    PushLiveStreamTips = null;
                }
                SetProperty(ref _pushLiveChecked, value);
            }
        }

        private string _speakingStatus;

        public string SpeakingStatus
        {
            get { return _speakingStatus; }
            set { SetProperty(ref _speakingStatus, value); }
        }

        private Visibility _sharingVisibility;

        public Visibility SharingVisibility
        {
            get { return _sharingVisibility; }
            set { SetProperty(ref _sharingVisibility, value); }
        }

        private Visibility _cancelSharingVisibility;

        public Visibility CancelSharingVisibility
        {
            get { return _cancelSharingVisibility; }
            set { SetProperty(ref _cancelSharingVisibility, value); }
        }

        //private object _dialogContent;

        //public object DialogContent
        //{
        //    get { return _dialogContent; }
        //    set { SetProperty(ref _dialogContent, value); }
        //}

        //private bool _isDialogOpen;

        //public bool IsDialogOpen
        //{
        //    get { return _isDialogOpen; }
        //    set { SetProperty(ref _isDialogOpen, value); }
        //}

        //private string _dialogMsg;

        //public string DialogMsg
        //{
        //    get { return _dialogMsg; }
        //    set { SetProperty(ref _dialogMsg, value); }
        //}

        private Visibility _isSpeaker;

        public Visibility IsSpeaker
        {
            get { return _isSpeaker; }
            set { SetProperty(ref _isSpeaker, value); }
        }

        private string _curModeName;

        public string CurModeName
        {
            get { return _curModeName; }
            set { SetProperty(ref _curModeName, value); }
        }

        private string _curLayoutName;

        public string CurLayoutName
        {
            get { return _curLayoutName; }
            set { SetProperty(ref _curLayoutName, value); }
        }

        private bool _isMenuOpen;
        public bool IsMenuOpen
        {
            get { return _isMenuOpen; }
            set { SetProperty(ref _isMenuOpen, value); }
        }

        private bool _isTopMost = true;

        public bool IsTopMost
        {
            get { return _isTopMost; }
            set
            {
                if (!value)
                {
                    IsMenuOpen = false;
                }
                SetProperty(ref _isTopMost, value);
            }
        }


        #endregion

        #region Commands

        public ICommand LoadCommand { get; set; }
        public ICommand ModeChangedCommand { get; set; }
        public ICommand SpeakingStatusChangedCommand { get; set; }
        public ICommand ExternalDataChangedCommand { get; set; }
        public ICommand SharingDesktopCommand { get; set; }
        public ICommand CancelSharingCommand { get; set; }
        public ICommand ExitCommand { get; set; }
        public ICommand OpenExitDialogCommand { get; set; }
        public ICommand KickoutCommand { get; set; }
        public ICommand OpenCloseCameraCommand { get; set; }
        public ICommand GetCameraInfoCommand { get; set; }
        public ICommand OpenPropertyPageCommand { get; set; }
        public ICommand SetDefaultFigureCameraCommand { get; set; }
        public ICommand SetDefaultDataCameraCommand { get; set; }
        public ICommand SetMicStateCommand { get; set; }
        public ICommand ScreenShareCommand { get; set; }
        public ICommand StartSpeakCommand { get; set; }
        public ICommand StopSpeakCommand { get; set; }
        public ICommand StartDoubleScreenCommand { get; set; }
        public ICommand StopDoubleScreenCommand { get; set; }
        public ICommand StartMonitorStreamCommand { get; set; }
        public ICommand StopMonitorStreamCommand { get; set; }
        public ICommand RecordCommand { get; set; }
        public ICommand PushLiveCommand { get; set; }
        public ICommand TopMostTriggerCommand { get; set; }
        public ICommand ShowLogCommand { get; set; }
        public ICommand TriggerMenuCommand { get; set; }
        public ICommand WindowKeyDownCommand { get; set; }
        public ICommand ShowHelpCommand { get; set; }
        public bool IsCreator
        {
            get
            {
                return _userInfo.UserId == _lessonDetail.MasterUserId;
                //return GlobalData.TryGet(CacheKey.HostId).ToString() == _windowManager.Participant.Account.AccountId.ToString();
            }
        }

        #endregion

        #region Command Handlers


        private void ChangeWindowStyleInDevMode()
        {
            if (GlobalData.Instance.RunMode == RunMode.Development)
            {
                IsTopMost = false;
                _meetingView.UseNoneWindowStyle = false;
                _meetingView.ResizeMode = ResizeMode.CanResize;
                _meetingView.WindowStyle = WindowStyle.SingleBorderWindow;
                _meetingView.IsWindowDraggable = true;
                _meetingView.ShowMinButton = true;
                _meetingView.ShowMaxRestoreButton = true;
                _meetingView.ShowCloseButton = false;
                _meetingView.IsCloseButtonEnabled = false;
            }
        }

        private async Task JoinMeetingAsync()
        {
            int meetingId = 0;
            object meetingIdObj = GlobalData.TryGet(CacheKey.MeetingId);

            if (meetingIdObj != null && int.TryParse(meetingIdObj.ToString(), out meetingId))
            {
                MeetingId = meetingId;

                var joinResult = await _meetingSdkAgent.JoinMeeting(meetingId, true);

                if (joinResult.StatusCode != 0)
                {
                    if (joinResult.StatusCode == -2014)
                    {
                        HasErrorMsg("-1", "该课程已经结束！");
                    }
                    else
                    {
                        HasErrorMsg("-1", "加入课程失败！");
                    }
                }
            }
            else
            {
                _exitByDialog = true;
                _meetingView.Close();
                _startMeetingCallbackEvent(false, "课程号无效！");
            }

            SetScreenSize();

            await _windowManager.Join(meetingId, false, IsCreator);

            _startMeetingCallbackEvent(true, "");

            GlobalData.Instance.ViewArea = new ViewArea()
            {
                Width = _meetingView.ActualWidth,
                Height = _meetingView.ActualHeight
            };


            //if not speaker, then clear mode menu items
            if (!IsCreator)
            {
                ModeMenuItems.Clear();
                IsSpeaker = Visibility.Collapsed;
            }
            else
            {
                IsSpeaker = Visibility.Visible;
            }

            if (_lessonDetail.Id > 0)
            {
                ResponseResult result = await
                    _bmsService.UpdateMeetingStatus(_lessonDetail.Id, _userInfo.UserId,
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        string.Empty);

                HasErrorMsg(result.Status, result.Message);
            }


            //uint[] uint32SOfNonDataArray =
            //{
            //    (uint) _meetingView.PictureBox1.Handle.ToInt32(),
            //    (uint) _meetingView.PictureBox2.Handle.ToInt32(),
            //    (uint) _meetingView.PictureBox3.Handle.ToInt32(),
            //    (uint) _meetingView.PictureBox4.Handle.ToInt32(),
            //};

            //foreach (var hwnd in uint32SOfNonDataArray)
            //{
            //    Log.Logger.Debug($"【figure hwnd】：{hwnd}");
            //}

            //uint[] uint32SOfDataArray = { (uint)_meetingView.PictureBox5.Handle.ToInt32() };

            //foreach (var hwnd in uint32SOfDataArray)
            //{
            //    Log.Logger.Debug($"【data hwnd】：{hwnd}");
            //}

            //AsyncCallbackMsg joinMeetingResult =
            //    await
            //        _sdkService.JoinMeeting(MeetingId, uint32SOfNonDataArray, uint32SOfNonDataArray.Length,
            //            uint32SOfDataArray,
            //            uint32SOfDataArray.Length);

            ////if failed to join meeting, needs to roll back
            //if (joinMeetingResult.Status != 0)
            //{
            //    Log.Logger.Error(
            //        $"【join meeting result】：result={joinMeetingResult.Status}, msg={joinMeetingResult.Message}");


            //    _exitByDialog = true;
            //    _meetingView.Close();
            //    _startMeetingCallbackEvent(false, joinMeetingResult.Message);
            //}
            //else
            //{
            //    GlobalCommands.Instance.SetCommandsStateInNonDiscussionClass(_sdkService.IsCreator);

            //    //if join meeting successfully, then make main view invisible
            //    _startMeetingCallbackEvent(true, "");



        }

        private void SetScreenSize()
        {
            MeetingSdk.Wpf.IScreen screen = _windowManager.VideoBoxManager as MeetingSdk.Wpf.IScreen;
            screen.Size = new System.Windows.Size(_meetingView.ActualWidth, _meetingView.ActualHeight);
        }

        //command handlers
        private async Task LoadAsync()
        {
            string errMsg = DeviceSettingsChecker.Instance.IsVideoAudioSettingsValid();
            if (!string.IsNullOrEmpty(errMsg))
            {
                _exitByDialog = true;
                _meetingView.Close();
                _startMeetingCallbackEvent(false, errMsg);

                return;
            }

            if (GlobalData.VideoControl == null)
            {
                GlobalData.VideoControl = new VideoControl();
            }

            _meetingView.Grid.Children.Add(GlobalData.VideoControl);


            GlobalData.Instance.CurWindowHwnd = new WindowInteropHelper(_meetingView).Handle;

            ChangeWindowStyleInDevMode();
            await JoinMeetingAsync();
        }

        private void MeetingModeChangedAsync(string meetingMode)
        {
            if (!_windowManager.Participant.IsSpeaking)
            {
                HasErrorMsg("-1", Messages.WarningYouAreNotSpeaking);

                return;
            }

            var targetMode = (ModeDisplayerType)Enum.Parse(typeof(ModeDisplayerType), meetingMode);

            _windowManager.ModeChange(targetMode);

            //if (!CheckIsUserSpeaking(true))
            //{
            //    return;
            //}

            //if (meetingMode == Common.MeetingMode.Speaker.ToString() &&
            //    !_viewLayoutService.ViewFrameList.Any(
            //        v => v.PhoneId == _sdkService.CreatorPhoneId && v.ViewType == 1))
            //{
            //    //如果选中的模式条件不满足，则回滚到之前的模式，
            //    //没有主讲者视图无法设置主讲模式，没有共享无法共享模式，没有发言无法设置任何模式

            //    HasErrorMsg("-1", Messages.WarningNoSpeaderView);
            //    return;
            //}

            //if (meetingMode == Common.MeetingMode.Sharing.ToString() &&
            //    !_viewLayoutService.ViewFrameList.Any(
            //        v => v.PhoneId == _sdkService.CreatorPhoneId && v.ViewType == 2))
            //{
            //    //如果选中的模式条件不满足，则回滚到之前的模式，
            //    //没有主讲者视图无法设置主讲模式，没有共享无法共享模式，没有发言无法设置任何模式

            //    HasErrorMsg("-1", Messages.WarningNoSharingView);
            //    return;
            //}

            //var newMeetingMode = (Common.MeetingMode) Enum.Parse(typeof(Common.MeetingMode), meetingMode);

            //_viewLayoutService.ChangeMeetingMode(newMeetingMode);

            //_viewLayoutService.ResetAsAutoLayout();

            //await _viewLayoutService.LaunchLayout();
        }

        private async Task SpeakingStatusChangedAsync()
        {
            if (SpeakingStatus == IsSpeaking)
            {
                var stopSpeakMsg = await _meetingSdkAgent.AskForStopSpeak();
                HasErrorMsg(stopSpeakMsg.StatusCode.ToString(), stopSpeakMsg.Message);
                return;
                //will change SpeakStatus in StopSpeakCallbackEventHandler.
            }

            if (SpeakingStatus == IsNotSpeaking)
            {
                var applyToSpeakMsg = await _meetingSdkAgent.AskForSpeak();

                HasErrorMsg(applyToSpeakMsg.StatusCode.ToString(), applyToSpeakMsg.Message);

                //if (!HasErrorMsg(result.Status.ToString(), result.Message))
                //{
                //    // will change SpeakStatus in callback???
                //    SpeakingStatus = IsSpeaking;
                //}
            }
        }

        private async Task ExternalDataChangedAsync()
        {
            if (!_windowManager.Participant.IsSpeaking)
            {
                HasErrorMsg("-1", Messages.WarningYouAreNotSpeaking);
                return;
            }


            try
            {
                MeetingResult<IList<VideoDeviceModel>> videoDeviceResult = _meetingSdkAgent.GetVideoDevices();

                MeetingResult<IList<string>> micResult = _meetingSdkAgent.GetMicrophones();

                AggregatedConfig configManager = GlobalData.Instance.AggregatedConfig;

                IEnumerable<string> docCameras;
                if (!_deviceNameAccessor.TryGetName(DeviceName.Camera, (devName) => { return devName.Option == "second"; }, out docCameras) || !videoDeviceResult.Result.Any(vdm => vdm.DeviceName == docCameras.FirstOrDefault()))
                {
                    HasErrorMsg("-1", "课件摄像头未配置！");
                    return;
                }


                if (configManager.DocVideoInfo?.DisplayWidth == 0 || configManager.DocVideoInfo?.DisplayHeight == 0 || configManager.DocVideoInfo?.VideoBitRate == 0)
                {
                    HasErrorMsg("-1", "课件采集参数未设置！");
                    return;
                }


                IEnumerable<string> docMics;
                if (!_deviceNameAccessor.TryGetName(DeviceName.Microphone, (devName) => { return devName.Option == "second"; }, out docMics) || !micResult.Result.Any(mic => mic == docMics.FirstOrDefault()))
                {
                    HasErrorMsg("-1", "课件麦克风未配置！");
                    return;
                }

                if (!_windowManager.Participant.IsSpeaking)
                {
                    HasErrorMsg("-1", "发言状态才可以进行课件分享！");
                    return;
                }

                MeetingResult<int> publishDocCameraResult = await _windowManager.Publish(MeetingSdk.NetAgent.Models.MediaType.VideoDoc, docCameras.FirstOrDefault());
                MeetingResult<int> publishDocMicResult = await _windowManager.Publish(MeetingSdk.NetAgent.Models.MediaType.AudioDoc, docMics.FirstOrDefault());

                if (publishDocCameraResult.StatusCode != 0 || publishDocMicResult.StatusCode != 0)
                {
                    HasErrorMsg("-1", "打开课件失败！");
                    return;
                }

                _cancelSharingAction = async () =>
                {
                    AsyncCallbackMsg result = await _sdkService.CloseSharedCamera();
                    if (!HasErrorMsg(result.Status.ToString(), result.Message))
                    {
                        SharingVisibility = Visibility.Visible;
                        CancelSharingVisibility = Visibility.Collapsed;
                    }
                };

                SharingVisibility = Visibility.Collapsed;
                CancelSharingVisibility = Visibility.Visible;

            }
            catch (Exception)
            {
            }
            finally
            {
                _eventAggregator.GetEvent<LayoutChangedEvent>().Publish(_windowManager.LayoutRendererStore.CurrentLayoutRenderType);
            }
        }

        private async Task SharingDesktopAsync()
        {
            if (!_windowManager.Participant.IsSpeaking)
            {
                HasErrorMsg("-1", Messages.WarningYouAreNotSpeaking);

                return;
            }

            MeetingResult<int> publishDocCameraResult = await _windowManager.Publish(MeetingSdk.NetAgent.Models.MediaType.VideoCaptureCard, "DesktopCapture");

            if (publishDocCameraResult.StatusCode != 0)
            {
                HasErrorMsg("-1", "共享桌面失败！");
                return;
            }

            _cancelSharingAction = async () =>
            {
                AsyncCallbackMsg result = await _sdkService.StopScreenSharing();
                if (!HasErrorMsg(result.Status.ToString(), result.Message))
                {
                    SharingVisibility = Visibility.Visible;
                    CancelSharingVisibility = Visibility.Collapsed;
                }
            };
            SharingVisibility = Visibility.Collapsed;
            CancelSharingVisibility = Visibility.Visible;
        }

        private async Task CancelSharingAsync()
        {
            await _cancelSharingAction();
        }

        public async Task ExitAsync()
        {
            try
            {

                UnRegisterMeetingEvents();


                MeetingResult meetingResult = await _meetingSdkAgent.LeaveMeeting();

                await _windowManager.Leave();

                if (meetingResult.StatusCode != 0)
                {
                    HasErrorMsg("-1", meetingResult.Message);
                    Log.Logger.Error(meetingResult.Message);
                }



                _meetingView.Grid.Children.Remove(GlobalData.VideoControl);


                StopAllLives();

                await UpdateExitTime();

                await _meetingView.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _exitByDialog = true;
                    _meetingView.Close();

                    _exitMeetingCallbackEvent(true, "");

                }));
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"ExitAsync => {ex}");
            }
        }

        private void StopAllLives()
        {
            _manualPushLive.StopPushLiveStream();
            _serverPushLiveService.StopPushLiveStream();
            _localRecordService.StopMp4Record();
        }

        private async Task UpdateExitTime()
        {
            if (_lessonDetail.Id > 0)
            {
                ResponseResult updateResult = await
                    _bmsService.UpdateMeetingStatus(_lessonDetail.Id, _userInfo.UserId,
                        string.Empty, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    );

                HasErrorMsg(updateResult.Status, updateResult.Message);
            }
        }

        private async Task OpenExitDialogAsync()
        {
            await _meetingView.Dispatcher.BeginInvoke(new Action(async () =>
            {
                IsMenuOpen = false;

                YesNoDialog yesNoDialog = new YesNoDialog("确定退出？");
                bool? result = yesNoDialog.ShowDialog();

                if (result.HasValue && result.Value)
                {
                    await ExitAsync();
                }
                else
                {
                    IsMenuOpen = true;
                }
            }));
        }

        private void KickoutAsync(string userPhoneId)
        {
             _sdkService.HostKickoutUser(userPhoneId);
        }

        private async Task OpenCloseCameraAsync()
        {
            if (OpenCloseCameraOperation == "open camera")
            {
                AsyncCallbackMsg result = await _sdkService.OpenCamera(SelectedCamera);
                if (!HasErrorMsg(result.Status.ToString(), result.Message))
                {
                    OpenCloseCameraOperation = "close camera";
                }

            }
            else
            {
                AsyncCallbackMsg result = await _sdkService.CloseCamera();
                if (!HasErrorMsg(result.Status.ToString(), result.Message))
                {
                    OpenCloseCameraOperation = "open camera";
                }

            }
        }

        private async Task OpenPropertyPageAsync(string cameraName)
        {
            await _meetingView.Dispatcher.BeginInvoke(new Action(() =>
            {
                AsyncCallbackMsg result = _sdkService.ShowCameraPropertyPage(cameraName);
            }));
        }

        private async Task GetCameraInfoAsync(string cameraName)
        {
            await Task.Run(() =>
            {
                Camera videoDeviceInfo = _sdkService.GetCameraInfo(cameraName);
            });
        }

        private async Task SetDefaultFigureCameraAsync(string cameraName)
        {
            AsyncCallbackMsg result = await _sdkService.SetDefaultCamera(1, cameraName);
            HasErrorMsg(result.Status.ToString(), result.Message);
        }

        private async Task SetDefaultDataCameraAsync(string cameraName)
        {
            AsyncCallbackMsg result = await _sdkService.SetDefaultCamera(2, cameraName);
            HasErrorMsg(result.Status.ToString(), result.Message);
        }

        private async Task ScreenShareAsync()
        {

            if (ScreenShareState == "共享屏幕")
            {
                AsyncCallbackMsg result = await _sdkService.StartScreenSharing();
                if (!HasErrorMsg(result.Status.ToString(), result.Message))
                {
                    ScreenShareState = "取消屏幕共享";
                }
            }
            else
            {
                AsyncCallbackMsg result = await _sdkService.StopScreenSharing();
                if (!HasErrorMsg(result.Status.ToString(), result.Message))
                {
                    ScreenShareState = "共享屏幕";
                }
            }

        }

        private async Task SetMicStateAsync()
        {
            if (MicState == "静音")
            {
                AsyncCallbackMsg result = await _sdkService.SetMicMuteState(1);
                if (!HasErrorMsg(result.Status.ToString(), result.Message))
                {
                    MicState = "取消静音";
                }
            }
            else
            {
                AsyncCallbackMsg result = await _sdkService.SetMicMuteState(0);
                if (!HasErrorMsg(result.Status.ToString(), result.Message))
                {
                    MicState = "静音";
                }
            }
        }

        private void StartSpeakAsync(string userPhoneId)
        {
             _sdkService.RequireUserSpeak(userPhoneId);
        }

        private void StopSpeakAsync(string userPhoneId)
        {
            _sdkService.RequireUserStopSpeak(userPhoneId);
        }

        private void PushLiveAsync()
        {
            try
            {
                if (!HasStreams())
                {
                    HasErrorMsg("-1", "当前没有音视频流，无法推流！");
                    PushLiveChecked = !PushLiveChecked;
                    return;
                }

                if (PushLiveChecked)
                {
                    var pushrt = _manualPushLive.GetLiveParam();

                    if (!pushrt)
                    {
                        HasErrorMsg("-1", "推流参数未正确配置！");

                        PushLiveChecked = false;
                        PushLiveStreamTips = null;

                        return;
                    }

                    IGetLiveVideoCoordinate liveVideoCoordinate = _windowManager as IGetLiveVideoCoordinate;
                    System.Windows.Size size = new System.Windows.Size()
                    {
                        Width = _manualPushLive.LiveParam.LiveParameter.Width,
                        Height = _manualPushLive.LiveParam.LiveParameter.Height,
                    };

                    VideoStreamModel[] videoStreamModels = liveVideoCoordinate.GetVideoStreamModels(size);

                    MeetingResult result =
                            _manualPushLive.StartPushLiveStream(videoStreamModels, GetAudioStreamModels(),
                            _manualPushLive.LiveParam.LiveParameter.Url1);

                    if (result.StatusCode != 0)
                    {
                        HasErrorMsg("-1", result.Message);

                        PushLiveChecked = false;
                        PushLiveStreamTips = null;
                    }
                    else
                    {
                        PushLiveStreamTips =
                            string.Format(
                                $"分辨率：{_manualPushLive.LiveParam.LiveParameter.Width}*{_manualPushLive.LiveParam.LiveParameter.Height}\r\n" +
                                $"码率：{_manualPushLive.LiveParam.LiveParameter.VideoBitrate}\r\n" +
                                $"推流地址：{_manualPushLive.LiveParam.LiveParameter.Url1}");
                    }
                }
                else
                {
                    MeetingResult result = _manualPushLive.StopPushLiveStream();
                    PushLiveStreamTips = null;

                    if (result.StatusCode != 0)
                    {
                        HasErrorMsg("-1", result.Message);

                        PushLiveChecked = true;
                    }
                }
            }
            catch (Exception)
            {

            }
        }


        private void RecordAsync()
        {
            if (!HasStreams())
            {
                HasErrorMsg("-1", "当前没有音视频流，无法录制！");
                RecordChecked = false;
                return;
            }

            if (RecordChecked)
            {
                var recordRt = _localRecordService.GetRecordParam();

                if (!recordRt)
                {
                    HasErrorMsg("-1", "录制参数未正确配置！");
                    RecordTips = null;
                    RecordChecked = false;
                    return;
                }

                IGetLiveVideoCoordinate liveVideoCoordinate = _windowManager as IGetLiveVideoCoordinate;

                System.Windows.Size size = new System.Windows.Size()
                {
                    Width = _localRecordService.RecordParam.LiveParameter.Width,
                    Height = _localRecordService.RecordParam.LiveParameter.Height,
                };

                VideoStreamModel[] videoStreamModels = liveVideoCoordinate.GetVideoStreamModels(size);

                AudioStreamModel[] audioStreamModels = GetAudioStreamModels();
                MeetingResult result = _localRecordService.StartMp4Record(videoStreamModels, audioStreamModels);

                if (result.StatusCode != 0)
                {
                    HasErrorMsg("-1", result.Message);
                    RecordTips = null;
                    RecordChecked = false;
                }
                else
                {
                    RecordTips =
                            string.Format(
                                $"分辨率：{_localRecordService.RecordParam.LiveParameter.Width}*{_localRecordService.RecordParam.LiveParameter.Height}\r\n" +
                                $"码率：{_localRecordService.RecordParam.LiveParameter.VideoBitrate}\r\n" +
                                $"录制路径：{_localRecordService.RecordDirectory}");
                }
            }
            else
            {
                MeetingResult result = _localRecordService.StopMp4Record();
                RecordTips = null;

                if (result.StatusCode != 0)
                {
                    HasErrorMsg("-1", result.Message);
                    RecordChecked = true;
                }
            }
        }

        private AudioStreamModel[] GetAudioStreamModels()
        {
            List<AudioStreamModel> audioStreamModels = new List<AudioStreamModel>();

            var selfAudioResources = _windowManager.Participant.Resources.Where(res => res.IsUsed = true &&
            (res.MediaType == MediaType.AudioCaptureCard || res.MediaType == MediaType.AudioDoc || res.MediaType == MediaType.Microphone));

            foreach (var selfAudioRes in selfAudioResources)
            {
                AudioStreamModel audioStreamModel = new AudioStreamModel()
                {
                    AccountId = _windowManager.Participant.Account.AccountId.ToString(),
                    StreamId = selfAudioRes.ResourceId,
                    BitsPerSameple = 16,
                    Channels = 2,
                    SampleRate = 48000,
                };

                audioStreamModels.Add(audioStreamModel);
            }


            foreach (var participant in _windowManager.Participants)
            {
                var otherAudioResources = participant.Resources.Where(res => res.IsUsed &&
                (res.MediaType == MediaType.AudioCaptureCard || res.MediaType == MediaType.AudioDoc || res.MediaType == MediaType.Microphone));

                foreach (var otherAudioRes in otherAudioResources)
                {
                    AudioStreamModel audioStreamModel = new AudioStreamModel()
                    {
                        AccountId = participant.Account.AccountId.ToString(),
                        StreamId = otherAudioRes.ResourceId,
                        BitsPerSameple = 16,
                        Channels = 2,
                        SampleRate = 48000,
                    };

                    audioStreamModels.Add(audioStreamModel);
                }
            }

            return audioStreamModels.ToArray();
        }


        private bool HasStreams()
        {
            return _windowManager.VideoBoxManager.Items.Any(v => v.Visible);
        }


        //dynamic commands
        //private async Task ViewModeChangedAsync(ViewMode viewMode)
        //{
        //if (!CheckIsUserSpeaking(true))
        //{
        //    return;
        //}

        //_viewLayoutService.ChangeViewMode(viewMode);
        //await _viewLayoutService.LaunchLayout();
        //}

        //private async Task FullScreenViewChangedAsync(ViewFrame fullScreenView)
        //{
        //    if (!CheckIsUserSpeaking(true))
        //    {
        //        return;
        //    }


        //    if (!CheckIsUserSpeaking(fullScreenView, true))
        //    {
        //        return;
        //    }

        //    _viewLayoutService.SetSpecialView(fullScreenView, SpecialViewType.FullScreen);

        //    await _viewLayoutService.LaunchLayout();
        //}

        //private async Task BigViewChangedAsync(ViewFrame bigView)
        //{
        //    if (!CheckIsUserSpeaking(true))
        //    {
        //        return;
        //    }

        //    if (_viewLayoutService.ViewFrameList.Count(viewFrame => viewFrame.IsOpened) < 2)
        //    {
        //        //一大多小至少有两个视图，否则不予设置

        //        HasErrorMsg("-1", Messages.WarningBigSmallLayoutNeedsTwoAboveViews);
        //        return;
        //    }

        //    if (!CheckIsUserSpeaking(bigView, true))
        //    {
        //        return;
        //    }

        //    var bigSpeakerView =
        //        _viewLayoutService.ViewFrameList.FirstOrDefault(
        //            v => v.PhoneId == bigView.PhoneId && v.Hwnd == bigView.Hwnd);

        //    if (bigSpeakerView == null)
        //    {
        //        //LOG ViewFrameList may change during this period.
        //    }

        //    _viewLayoutService.SetSpecialView(bigSpeakerView, SpecialViewType.Big);

        //    await _viewLayoutService.LaunchLayout();
        //}

        #endregion

        #region Methods

        //private List<ViewFrame> InitializeViewFrameList(MeetingView meetingView)
        //{
        //    List<ViewFrame> viewFrames = new List<ViewFrame>();

        //    ViewFrame1 = new ViewFrame(meetingView.PictureBox1.Handle, meetingView.PictureBox1, meetingView.Label1);
        //    ViewFrame2 = new ViewFrame(meetingView.PictureBox2.Handle, meetingView.PictureBox2, meetingView.Label2);
        //    ViewFrame3 = new ViewFrame(meetingView.PictureBox3.Handle, meetingView.PictureBox3, meetingView.Label3);
        //    ViewFrame4 = new ViewFrame(meetingView.PictureBox4.Handle, meetingView.PictureBox4, meetingView.Label4);
        //    ViewFrame5 = new ViewFrame(meetingView.PictureBox5.Handle, meetingView.PictureBox5, meetingView.Label5);

        //    viewFrames.Add(ViewFrame1);
        //    viewFrames.Add(ViewFrame2);
        //    viewFrames.Add(ViewFrame3);
        //    viewFrames.Add(ViewFrame4);
        //    viewFrames.Add(ViewFrame5);

        //    return viewFrames;
        //}

        public void RefreshLayoutMenu(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = e.OriginalSource as MenuItem;
            if (menuItem.Header is StackPanel)
            {
                RefreshLayoutMenuItems();
            }
            else
            {
                e.Handled = true;
            }
        }

        private void InitializeMenuItems()
        {
            LoadModeMenuItems();
            RefreshLayoutMenuItems();
            RefreshExternalData();
        }

        private void RegisterMeetingEvents()
        {
            _eventAggregator.GetEvent<StartSpeakEvent>().Subscribe(StartSpeakEventHandler);
            _eventAggregator.GetEvent<StopSpeakEvent>().Subscribe(StopSpeakEventHandler);
            _eventAggregator.GetEvent<UserJoinEvent>().Subscribe(OtherJoinMeetingEventHandler);
            _eventAggregator.GetEvent<UserLeaveEvent>().Subscribe(OtherExitMeetingEventHandler);
            _eventAggregator.GetEvent<TransparentMsgReceivedEvent>().Subscribe(UIMessageReceivedEventHandler);
            _eventAggregator.GetEvent<HostKickoutUserEvent>().Subscribe(KickedByHostEventHandler);

            _eventAggregator.GetEvent<DeviceLostNoticeEvent>().Subscribe(DeviceLostNoticeEventHandler);
            _eventAggregator.GetEvent<DeviceStatusChangedEvent>().Subscribe(DeviceStatusChangedEventHandler);
            _eventAggregator.GetEvent<LockStatusChangedEvent>().Subscribe(LockStatusChangedEventHandler);
            _eventAggregator.GetEvent<MeetingManageExceptionEvent>().Subscribe(MeetingManageExceptionEventHandler);
            _eventAggregator.GetEvent<SdkCallbackEvent>().Subscribe(SdkCallbackEventHandler);
            _eventAggregator.GetEvent<UiTransparentMsgReceivedEvent>().Subscribe(UiTransparentMsgReceivedEventHandler);

            _eventAggregator.GetEvent<ModeDisplayerTypeChangedEvent>().Subscribe(ClassModeChangedEventHandler);
            _eventAggregator.GetEvent<LayoutChangedEvent>().Subscribe(LayoutChangedEventHandler);
            _eventAggregator.GetEvent<RefreshCanvasEvent>().Subscribe(RefreshViewContainerBackground);


            _eventAggregator.GetEvent<ParticipantCollectionChangeEvent>().Subscribe(ParticipantCollectionChangeEventHandler);

            _eventAggregator.GetEvent<RemoveVideoControlEvent>().Subscribe(RemoveVideoControlEventHandler, ThreadOption.UIThread, true);

            _meetingView.LocationChanged += _meetingView_LocationChanged;
            _meetingView.Deactivated += _meetingView_Deactivated;
            _meetingView.Closing += _meetingView_Closing;


            //_sdkService.ViewCreatedEvent += ViewCreateEventHandler;
            //_sdkService.ViewClosedEvent += ViewCloseEventHandler;
            //_sdkService.StartSpeakEvent += StartSpeakEventHandler;
            //_sdkService.StopSpeakEvent += StopSpeakEventHandler;
            //_viewLayoutService.MeetingModeChangedEvent += MeetingModeChangedEventHandler;
            //_viewLayoutService.ViewModeChangedEvent += ViewModeChangedEventHandler;
            //_sdkService.OtherJoinMeetingEvent += OtherJoinMeetingEventHandler;
            //_sdkService.OtherExitMeetingEvent += OtherExitMeetingEventHandler;
            //_sdkService.TransparentMessageReceivedEvent += UIMessageReceivedEventHandler;
            //_sdkService.ErrorMsgReceivedEvent += ErrorMsgReceivedEventHandler;
            //_sdkService.KickedByHostEvent += KickedByHostEventHandler;
            //_sdkService.DiskSpaceNotEnoughEvent += DiskSpaceNotEnoughEventHandler;
        }

        private void ParticipantCollectionChangeEventHandler(IEnumerable<MeetingSdk.Wpf.Participant> obj)
        {
            foreach (var participant in _windowManager.Participants)
            {
                var userInfo = _userInfos.FirstOrDefault(cls => cls.GetNube() == participant.Account.AccountId.ToString());
                if (userInfo != null)
                {
                    participant.Account.AccountName = userInfo.Name;
                }
            }
        }

        private void RefreshViewContainerBackground()
        {
        }

        private void LayoutChangedEventHandler(LayoutRenderType obj)
        {
            CurLayoutName = EnumHelper.GetDescription(typeof(LayoutRenderType), obj);
            CurModeName = EnumHelper.GetDescription(typeof(ModeDisplayerType), _windowManager.ModeDisplayerStore.CurrentModeDisplayerType);

            RefreshLocalRecordLive();

            RefreshRemotePushLive();
            RefreshPushLive();

            StartAutoLiveAsync();
        }


        private void RefreshPushLive()
        {
            if (_manualPushLive.LiveId != 0)
            {
                IGetLiveVideoCoordinate liveVideoCoordinate = _windowManager as IGetLiveVideoCoordinate;
                System.Windows.Size size = new System.Windows.Size()
                {
                    Width = _manualPushLive.LiveParam.LiveParameter.Width,
                    Height = _manualPushLive.LiveParam.LiveParameter.Height,
                };

                VideoStreamModel[] videoStreamModels = liveVideoCoordinate.GetVideoStreamModels(size);

                _manualPushLive.RefreshLiveStream(videoStreamModels, GetAudioStreamModels());
            }
        }

        private void StartAutoLiveAsync()
        {
            if (_windowManager.VideoBoxManager.Items.Count(viewFrame => viewFrame.Visible) > 0)
            {
                if (IsCreator && !_serverPushLiveService.HasPushLiveSuccessfully&&_lessonDetail.Live)
                {
                    _serverPushLiveService.HasPushLiveSuccessfully = true;

                    var pushRt = _serverPushLiveService.GetLiveParam();

                    if (!string.IsNullOrEmpty(_userInfo.PushStreamUrl) && pushRt)
                    {
                        IGetLiveVideoCoordinate liveVideoCoordinate = _windowManager as IGetLiveVideoCoordinate;
                        System.Windows.Size size = new System.Windows.Size()
                        {
                            Width = _serverPushLiveService.LiveParam.LiveParameter.Width,
                            Height = _serverPushLiveService.LiveParam.LiveParameter.Height,
                        };

                        VideoStreamModel[] videoStreamModels = liveVideoCoordinate.GetVideoStreamModels(size);

                        MeetingResult result =
                                _serverPushLiveService.StartPushLiveStream(videoStreamModels, GetAudioStreamModels(),
                                _userInfo.PushStreamUrl);
                    }
                }
            }
        }


        private void RefreshRemotePushLive()
        {
            if (_serverPushLiveService.LiveId != 0)
            {
                IGetLiveVideoCoordinate liveVideoCoordinate = _windowManager as IGetLiveVideoCoordinate;
                System.Windows.Size size = new System.Windows.Size()
                {
                    Width = _serverPushLiveService.LiveParam.LiveParameter.Width,
                    Height = _serverPushLiveService.LiveParam.LiveParameter.Height,
                };

                VideoStreamModel[] videoStreamModels = liveVideoCoordinate.GetVideoStreamModels(size);

                _serverPushLiveService.RefreshLiveStream(videoStreamModels, GetAudioStreamModels());
            }

        }

        private void RefreshLocalRecordLive()
        {
            if (_localRecordService.RecordId != 0)
            {
                IGetLiveVideoCoordinate liveVideoCoordinate = _windowManager as IGetLiveVideoCoordinate;
                System.Windows.Size size = new System.Windows.Size()
                {
                    Width = _localRecordService.RecordParam.LiveParameter.Width,
                    Height = _localRecordService.RecordParam.LiveParameter.Height,
                };

                VideoStreamModel[] videoStreamModels = liveVideoCoordinate.GetVideoStreamModels(size);

                _localRecordService.RefreshLiveStream(videoStreamModels, GetAudioStreamModels());
            }
        }


        private async void ClassModeChangedEventHandler(ModeDisplayerType obj)
        {
            if (IsCreator)
            {
                await SyncClassMode();
            }

            RefreshLocalRecordLive();

            RefreshRemotePushLive();
            RefreshPushLive();


            CurModeName = EnumHelper.GetDescription(typeof(ModeDisplayerType), obj);
            CurLayoutName = EnumHelper.GetDescription(typeof(LayoutRenderType), _windowManager.LayoutRendererStore.CurrentLayoutRenderType);
        }

        private async Task SyncClassMode()
        {
            MeetingResult sendUiMsgResult = await _meetingSdkAgent.AsynSendUIMsg((int)_windowManager.ModeDisplayerStore.CurrentModeDisplayerType, 0, "");
            HasErrorMsg(sendUiMsgResult.StatusCode.ToString(), sendUiMsgResult.Message);
        }

        private void UiTransparentMsgReceivedEventHandler(UiTransparentMsg obj)
        {
            if (obj.MsgId < 3)
            {
                GlobalData.AddOrUpdate(CacheKey.HostId, obj.TargetAccountId);

                var classMode = (ModeDisplayerType)obj.MsgId;


                if (_windowManager.ModeChange(classMode))
                {
                }
            }
        }

        private void SdkCallbackEventHandler(SdkCallbackModel obj)
        {
        }

        private void MeetingManageExceptionEventHandler(ExceptionModel obj)
        {
        }

        private void LockStatusChangedEventHandler(MeetingResult obj)
        {
        }

        private void DeviceStatusChangedEventHandler(DeviceStatusModel obj)
        {
        }

        private void DeviceLostNoticeEventHandler(ResourceModel obj)
        {
        }

        private void KickedByHostEventHandler(KickoutUserModel obj)
        {
        }

        private void UIMessageReceivedEventHandler(TransparentMsg obj)
        {
        }

        private void OtherExitMeetingEventHandler(AccountModel obj)
        {
            object hostIdObj = GlobalData.TryGet(CacheKey.HostId);

            int hostId;
            if (hostIdObj != null && int.TryParse(hostIdObj.ToString(), out hostId))
            {
                if (hostId == obj.AccountId)
                {
                    if (_windowManager.ModeChange(ModeDisplayerType.InteractionMode))
                    {

                    }
                }
            }

        }

        private void OtherJoinMeetingEventHandler(AccountModel obj)
        {
            if (IsCreator)
            {
                _meetingSdkAgent.AsynSendUIMsg((int)_windowManager.ModeDisplayerStore.CurrentModeDisplayerType, obj.AccountId, "");
            }
        }

        private void StopSpeakEventHandler(SpeakModel obj)
        {
            if (IsCreator)
            {
                if (_windowManager.ModeChange(ModeDisplayerType.InteractionMode))
                {
                }
            }

            SpeakingStatus = IsNotSpeaking;
        }


        private void StartSpeakEventHandler(SpeakModel obj)
        {
            SpeakingStatus = IsSpeaking;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        private void _meetingView_Deactivated(object sender, EventArgs e)
        {
            IsMenuOpen = false;
        }

        private void _meetingView_LocationChanged(object sender, EventArgs e)
        {
            try
            {
                MethodInfo methodInfo = typeof(Popup).GetMethod("UpdatePosition",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (IsMenuOpen)
                {
                    methodInfo.Invoke(_meetingView.TopMenu, null);
                    methodInfo.Invoke(_meetingView.BottomMenu, null);
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"_meetingView_LocationChanged => {ex}");
            }
        }

        private void DiskSpaceNotEnoughEventHandler(AsyncCallbackMsg msg)
        {
            HasErrorMsg(msg.Status.ToString(), msg.Message);
        }

        private async void _meetingView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //UnRegisterMeetingEvents();
            if (GlobalData.Instance.RunMode == RunMode.Development && !_exitByDialog)
            {
                e.Cancel = true;
                await OpenExitDialogAsync();
            }
        }

        private void UnRegisterMeetingEvents()
        {

            _eventAggregator.GetEvent<CommandReceivedEvent>().Unsubscribe(ExecuteCommand);


            _eventAggregator.GetEvent<StartSpeakEvent>().Unsubscribe(StartSpeakEventHandler);
            _eventAggregator.GetEvent<StopSpeakEvent>().Unsubscribe(StopSpeakEventHandler);

            //CurrentLayoutInfo.Instance.ClassModeChangedEvent -= MeetingModeChangedEventHandler;
            //CurrentLayoutInfo.Instance.PictureModeChangedEvent -= ViewModeChangedEventHandler;
            //CurrentLayoutInfo.Instance.LayoutChangedEvent -= _viewLayoutService_LayoutChangedEvent;


            _eventAggregator.GetEvent<UserJoinEvent>().Unsubscribe(OtherJoinMeetingEventHandler);
            _eventAggregator.GetEvent<UserLeaveEvent>().Unsubscribe(OtherExitMeetingEventHandler);
            _eventAggregator.GetEvent<TransparentMsgReceivedEvent>().Unsubscribe(UIMessageReceivedEventHandler);
            _eventAggregator.GetEvent<HostKickoutUserEvent>().Unsubscribe(KickedByHostEventHandler);

            _eventAggregator.GetEvent<DeviceLostNoticeEvent>().Unsubscribe(DeviceLostNoticeEventHandler);
            _eventAggregator.GetEvent<DeviceStatusChangedEvent>().Unsubscribe(DeviceStatusChangedEventHandler);
            _eventAggregator.GetEvent<LockStatusChangedEvent>().Unsubscribe(LockStatusChangedEventHandler);
            _eventAggregator.GetEvent<MeetingManageExceptionEvent>().Unsubscribe(MeetingManageExceptionEventHandler);
            _eventAggregator.GetEvent<SdkCallbackEvent>().Unsubscribe(SdkCallbackEventHandler);
            _eventAggregator.GetEvent<UiTransparentMsgReceivedEvent>().Unsubscribe(UiTransparentMsgReceivedEventHandler);

            _eventAggregator.GetEvent<ModeDisplayerTypeChangedEvent>().Unsubscribe(ClassModeChangedEventHandler);
            _eventAggregator.GetEvent<LayoutChangedEvent>().Unsubscribe(LayoutChangedEventHandler);
            _eventAggregator.GetEvent<RefreshCanvasEvent>().Unsubscribe(RefreshViewContainerBackground);


            _eventAggregator.GetEvent<ParticipantCollectionChangeEvent>().Unsubscribe(ParticipantCollectionChangeEventHandler);

            _eventAggregator.GetEvent<RemoveVideoControlEvent>().Unsubscribe(RemoveVideoControlEventHandler);



            //_sdkService.ViewCreatedEvent -= ViewCreateEventHandler;
            //_sdkService.ViewClosedEvent -= ViewCloseEventHandler;
            //_sdkService.StartSpeakEvent -= StartSpeakEventHandler;
            //_sdkService.StopSpeakEvent -= StopSpeakEventHandler;
            //_viewLayoutService.MeetingModeChangedEvent -= MeetingModeChangedEventHandler;
            //_viewLayoutService.ViewModeChangedEvent -= ViewModeChangedEventHandler;
            //_sdkService.OtherJoinMeetingEvent -= OtherJoinMeetingEventHandler;
            //_sdkService.OtherExitMeetingEvent -= OtherExitMeetingEventHandler;
            //_sdkService.TransparentMessageReceivedEvent -= UIMessageReceivedEventHandler;
            //_sdkService.ErrorMsgReceivedEvent -= ErrorMsgReceivedEventHandler;
            //_sdkService.KickedByHostEvent -= KickedByHostEventHandler;
            //_sdkService.DiskSpaceNotEnoughEvent -= DiskSpaceNotEnoughEventHandler;
        }


        private void RemoveVideoControlEventHandler()
        {
            _meetingView.Grid.Children.Remove(GlobalData.VideoControl);
        }

        private void KickedByHostEventHandler()
        {
            _meetingView.Dispatcher.BeginInvoke(new Action(() =>
            {
                _exitByDialog = true;

                _meetingView.Close();


                _exitMeetingCallbackEvent(true, "");

                //MetroWindow mainView = App.SSCBootstrapper.Container.ResolveKeyed<MetroWindow>("MainView");
                //mainView.GlowBrush = new SolidColorBrush(Colors.Purple);
                //mainView.NonActiveGlowBrush = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#FF999999"));
                //mainView.Visibility = Visibility.Visible;
            }));
        }

        private void ErrorMsgReceivedEventHandler(AsyncCallbackMsg error)
        {
            HasErrorMsg("-1", error.Message);
        }

        private void UIMessageReceivedEventHandler(TransparentMessage message)
        {
            //if (message.MessageId < 3)
            //{
            //    _sdkService.CreatorPhoneId = message.Sender.PhoneId;

            //    Common.MeetingMode meetingMode = (Common.MeetingMode) message.MessageId;
            //    _viewLayoutService.ChangeMeetingMode(meetingMode);

            //    _viewLayoutService.LaunchLayout();
            //}
            //else
            //{
            //    if (message.MessageId == (int) UiMessage.BannedToSpeak)
            //    {
            //        AllowedToSpeak = false;
            //    }
            //    if (message.MessageId == (int) UiMessage.AllowToSpeak)
            //    {
            //        AllowedToSpeak = true;
            //    }
            //}
        }

        private void OtherExitMeetingEventHandler(MeetingSdk.SdkWrapper.MeetingDataModel.Participant contactInfo)
        {
            //var attendee = _userInfos.FirstOrDefault(userInfo => userInfo.GetNube() == contactInfo.m_szPhoneId);

            //string displayName = string.Empty;
            //if (!string.IsNullOrEmpty(attendee?.Name))
            //{
            //    displayName = attendee.Name + " - ";
            //}

            //string exitMsg = $"{displayName}{contactInfo.m_szPhoneId}退出会议！";
            //HasErrorMsg("-1", exitMsg);

            if (contactInfo.PhoneId == _sdkService.CreatorPhoneId)
            {
                //
            }
        }

        private void OtherJoinMeetingEventHandler(MeetingSdk.SdkWrapper.MeetingDataModel.Participant contactInfo)
        {
            //var attendee = _userInfos.FirstOrDefault(userInfo => userInfo.GetNube() == contactInfo.PhoneId);

            ////string displayName = string.Empty;
            ////if (!string.IsNullOrEmpty(attendee?.Name))
            ////{
            ////    displayName = attendee.Name + " - ";
            ////}

            ////string joinMsg = $"{displayName}{contactInfo.m_szPhoneId}加入会议！";
            ////HasErrorMsg("-1", joinMsg);

            ////speaker automatically sends a message(with creatorPhoneId) to nonspeakers
            ////!!!CAREFUL!!! ONLY speaker will call this
            //if (_sdkService.IsCreator)
            //{
            //    _sdkService.SendMessage((int) _viewLayoutService.MeetingMode,
            //        _viewLayoutService.MeetingMode.ToString(), _viewLayoutService.MeetingMode.ToString().Length, null);
            //}
        }

        private async void ViewCloseEventHandler(ParticipantView speakerView)
        {
            //await _viewLayoutService.HideViewAsync(speakerView);
        }

        private void StopSpeakEventHandler()
        {
            //_viewLayoutService.ChangeViewMode(ViewMode.Auto);

            //if (_sdkService.IsCreator)
            //{
            //    _viewLayoutService.ChangeMeetingMode(Common.MeetingMode.Interaction);
            //}

            //SpeakingStatus = IsNotSpeaking;
            //SharingVisibility = Visibility.Visible;
            //CancelSharingVisibility = Visibility.Collapsed;

            //_meetingView.Dispatcher.BeginInvoke(new Action(RefreshExternalData));
            ////reload menus
        }

        private void StartSpeakEventHandler()
        {
            SpeakingStatus = IsSpeaking;
        }

        private async void ViewCreateEventHandler(ParticipantView speakerView)
        {
            //await _viewLayoutService.ShowViewAsync(speakerView);
        }


        //private bool CheckIsUserSpeaking(bool showMsgBar = false)
        //{
        //    //return true;

        //    _windowManager.Participant.is

        //    List<MeetingSdk.SdkWrapper.MeetingDataModel.Participant> participants = _sdkService.GetParticipants();

        //    var self = participants.FirstOrDefault(p => p.PhoneId == _sdkService.SelfPhoneId);

        //    if (self != null && (showMsgBar && !self.IsSpeaking))
        //    {
        //        HasErrorMsg("-1", Messages.WarningYouAreNotSpeaking);
        //    }

        //    return self != null && self.IsSpeaking;
        //}

        //private bool CheckIsUserSpeaking(ViewFrame speakerView, bool showMsgBar = false)
        //{
        //    //return true;

        //    List<MeetingSdk.SdkWrapper.MeetingDataModel.Participant> participants = _sdkService.GetParticipants();

        //    var speaker = participants.FirstOrDefault(p => p.PhoneId == speakerView.PhoneId);

        //    bool isUserNotSpeaking = string.IsNullOrEmpty(speaker.PhoneId) || !speaker.IsSpeaking;

        //    if (isUserNotSpeaking && showMsgBar)
        //    {
        //        HasErrorMsg("-1", Messages.WarningUserNotSpeaking);
        //    }

        //    return !isUserNotSpeaking;
        //}

        private void RefreshExternalData()
        {
            if (SharingMenuItems == null)
            {
                SharingMenuItems = new ObservableCollection<MenuItem>();

            }
            else
            {
                SharingMenuItems.Clear();
            }

            var sharings = Enum.GetNames(typeof(Sharing));
            foreach (var sharing in sharings)
            {
                var newSharingMenu = new MenuItem();
                newSharingMenu.Header = EnumHelper.GetDescription(typeof(Sharing), Enum.Parse(typeof(Sharing), sharing));

                if (sharing == Sharing.Desktop.ToString())
                {
                    newSharingMenu.Command = SharingDesktopCommand;
                }

                if (sharing == Sharing.ExternalData.ToString())
                {
                    newSharingMenu.Command = ExternalDataChangedCommand;

                    //var videoDevices = _meetingSdkAgent.GetVideoDevices();
                    //foreach (var camera in videoDevices.Result)
                    //{
                    //    if (!string.IsNullOrEmpty(camera.DeviceName) && camera.DeviceName != GlobalData.Instance.AggregatedConfig.MainVideoInfo.VideoDevice)
                    //    {
                    //        newSharingMenu.Items.Add(
                    //            new MenuItem()
                    //            {
                    //                Header = camera.DeviceName,
                    //                Command = ExternalDataChangedCommand,
                    //                CommandParameter = camera.DeviceName
                    //            });
                    //    }
                    //}
                }

                SharingMenuItems.Add(newSharingMenu);
            }
        }

        private void LoadModeMenuItems()
        {
            if (ModeMenuItems == null)
            {
                ModeMenuItems = new ObservableCollection<MenuItem>();
            }
            else
            {
                ModeMenuItems.Clear();
            }

            var modes = Enum.GetNames(typeof(ModeDisplayerType));
            foreach (var mode in modes)
            {
                var newModeMenu = new MenuItem();
                newModeMenu.Header = EnumHelper.GetDescription(typeof(ModeDisplayerType),
                    Enum.Parse(typeof(ModeDisplayerType), mode));
                newModeMenu.Command = ModeChangedCommand;
                newModeMenu.CommandParameter = mode;

                ModeMenuItems.Add(newModeMenu);
            }
            CurModeName = EnumHelper.GetDescription(typeof(ModeDisplayerType), _windowManager.ModeDisplayerStore.CurrentModeDisplayerType);

        }

        private void RefreshLayoutMenuItems()
        {
            if (LayoutMenuItems == null)
            {
                LayoutMenuItems = new ObservableCollection<MenuItem>();
            }
            else
            {
                LayoutMenuItems.Clear();
            }

            var layouts = Enum.GetNames(typeof(LayoutRenderType));
            foreach (var layout in layouts)
            {
                var newLayoutMenu = new MenuItem();
                newLayoutMenu.Header = EnumHelper.GetDescription(typeof(LayoutRenderType), Enum.Parse(typeof(LayoutRenderType), layout));
                newLayoutMenu.Tag = layout;

                if (layout == LayoutRenderType.BigSmallsLayout.ToString() || layout == LayoutRenderType.CloseupLayout.ToString())
                {
                    foreach (var speakerView in _windowManager.VideoBoxManager.Items)
                    {
                        if (speakerView.Visible)
                        {
                            newLayoutMenu.Items.Add(new MenuItem()
                            {
                                Header =
                                    string.IsNullOrEmpty(speakerView.Name)
                                        ? speakerView.AccountResource.AccountModel.AccountId.ToString()
                                        : (speakerView.AccountResource.AccountModel.AccountName + " - " + speakerView.AccountResource.AccountModel.AccountId),
                                Tag = speakerView
                            });
                        }
                    }
                }

                newLayoutMenu.Click += LayoutChangedEventHandler;

                LayoutMenuItems.Add(newLayoutMenu);
            }

            CurLayoutName = EnumHelper.GetDescription(typeof(LayoutRenderType), _windowManager.LayoutRendererStore.CurrentLayoutRenderType);
        }

        private void LayoutChangedEventHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            MenuItem sourceMenuItem = e.OriginalSource as MenuItem;

            string header = menuItem.Tag.ToString();

            LayoutRenderType layout = (LayoutRenderType)Enum.Parse(typeof(LayoutRenderType), header);


            switch (layout)
            {
                case LayoutRenderType.AutoLayout:
                case LayoutRenderType.AverageLayout:

                    if (_windowManager.LayoutChange(WindowNames.MainWindow, layout))
                    {
                    }

                    break;
                case LayoutRenderType.CloseupLayout:
                case LayoutRenderType.BigSmallsLayout:

                    VideoBox videoBox = sourceMenuItem.Tag as VideoBox;

                    var specialView = _windowManager.VideoBoxManager.Items.FirstOrDefault(v => v.AccountResource != null && v.AccountResource.AccountModel.AccountId == videoBox.AccountResource.AccountModel.AccountId && v.Handle == videoBox.Handle);

                    if (specialView == null)
                    {
                        HasErrorMsg("-1", "找不到该视图！");
                        return;
                    }

                    _windowManager.VideoBoxManager.SetProperty(layout.ToString(), specialView.Name);

                    if (!_windowManager.LayoutChange(WindowNames.MainWindow, layout))
                    {
                        HasErrorMsg("-1", "无法设置一大一小画面模式！");
                    }

                    break;
            }

            if (layout != _windowManager.LayoutRendererStore.CurrentLayoutRenderType)
            {
                CurLayoutName = EnumHelper.GetDescription(typeof(LayoutRenderType), layout);
            }
        }

        #endregion
    }
}