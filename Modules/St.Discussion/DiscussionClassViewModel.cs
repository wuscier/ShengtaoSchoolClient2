using Prism.Commands;
using St.Common;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using Caliburn.Micro;
using Common;
using MenuItem = System.Windows.Controls.MenuItem;
using Serilog;
using Prism.Events;
using St.Common.Commands;
using Action = System.Action;
using LogManager = St.Common.LogManager;
using Timer = System.Threading.Timer;
using MeetingSdk.NetAgent;
using MeetingSdk.Wpf;
using UserInfo = St.Common.UserInfo;
using MeetingSdk.NetAgent.Models;
using System.Linq;
using MeetingSdk.Wpf.Interfaces;
using St.Common.Helper;

namespace St.Discussion
{
    public class DiscussionClassViewModel : ViewModelBase, IExitMeeting
    {
        protected override bool HasErrorMsg(string status, string msg)
        {
            IsMenuOpen = true;

            if (status != "0")
            {
                MessageQueueManager.Instance.AddInfo(msg);
            }

            return status != "0";
        }

        public DiscussionClassViewModel(DiscussionClassView meetingView, Action<bool, string> startMeetingCallback,
            Action<bool, string> exitMeetingCallback)
        {
            _discussionClassView = meetingView;

            UpButtonGotFocusCommand = new DelegateCommand<Button>(menuItem =>
            {
                //Console.WriteLine($"up button got focus, {menuItem}");
                _upFocusedUiElement = menuItem;
            });
            DownButtonGotFocusCommand = new DelegateCommand<Button>((menuItem) =>
            {
                //Console.WriteLine($"down button got focus, {menuItem}");
                _downFocusedUiElement = menuItem;
            });

            InitMenuButtonItems();

            _windowManager = IoC.Get<IMeetingWindowManager>();
            _deviceNameAccessor = IoC.Get<IDeviceNameAccessor>();
            _meetingSdkAgent = IoC.Get<IMeetingSdkAgent>();
            _eventAggregator = IoC.Get<IEventAggregator>();
            _eventAggregator.GetEvent<CommandReceivedEvent>()
                .Subscribe(ExecuteCommand, ThreadOption.PublisherThread, false, command => command.IsIntoClassCommand);

            //_viewLayoutService = IoC.Get<IViewLayout>();
            //_viewLayoutService.ViewFrameList = InitializeViewFrameList(meetingView);

            _bmsService = IoC.Get<IBms>();

            _manualPushLive = IoC.Get<IPushLive>(GlobalResources.LocalPushLive);
            _manualPushLive.ResetStatus();

            _serverPushLiveService = IoC.Get<IPushLive>(GlobalResources.RemotePushLive);
            _serverPushLiveService.ResetStatus();

            _localRecordService = IoC.Get<IRecord>();
            _localRecordService.ResetStatus();

            _startMeetingCallbackEvent = startMeetingCallback;
            _exitMeetingCallbackEvent = exitMeetingCallback;

            //MeetingId = _sdkService.MeetingId;

            _lessonDetail = IoC.Get<LessonDetail>();
            _userInfo = IoC.Get<UserInfo>();
            _userInfos = IoC.Get<List<UserInfo>>();

            MeetingOrLesson = _lessonDetail.Id == 0 ? "会议号:" : "课程号:";

            LoadCommand = DelegateCommand.FromAsyncHandler(LoadAsync);
            OpenExitDialogCommand = DelegateCommand.FromAsyncHandler(OpenExitDialogAsync);
            KickoutCommand = new DelegateCommand<string>(KickoutAsync);
            RecordCommand = new DelegateCommand(RecordAsync);
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

            //TouchDownCommand = new DelegateCommand(() =>
            //{
            //    IsMenuOpen = !IsMenuOpen;

            //});
            RegisterMeetingEvents();
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
                //SpeakItem.Command.Execute(null);
                await SpeakingStatusChangedAsync();
            }
            else if (command.Directive == GlobalCommands.Instance.RecordCommand.Directive)
            {
                RecordAsync();
            }
            else if (command.Directive == GlobalCommands.Instance.PushLiveCommand.Directive)
            {
                MessageQueueManager.Instance.AddInfo("不支持的命令！");
                // do nothing.
            }
            else if (command.Directive == GlobalCommands.Instance.DocCommand.Directive)
            {
                ShareDocItem.Command.Execute(null);
            }
            else if (command.Directive == GlobalCommands.Instance.AverageCommand.Directive)
            {
                //_viewLayoutService.ChangeViewMode(ViewMode.Average);
                //await _viewLayoutService.LaunchLayout();
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

                //_viewLayoutService.SetSpecialView(openedVfs[bigViewIndex], SpecialViewType.Big);
                //await _viewLayoutService.LaunchLayout();

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

                //_viewLayoutService.SetSpecialView(openedVfs[fullViewIndex], SpecialViewType.FullScreen);
                //await _viewLayoutService.LaunchLayout();
            }
            else if (command.Directive == GlobalCommands.Instance.InteractionCommand.Directive)
            {
                MessageQueueManager.Instance.AddInfo("不支持的命令！");

                // do nothing.
            }
            else if (command.Directive == GlobalCommands.Instance.SpeakerCommand.Directive)
            {
                MessageQueueManager.Instance.AddInfo("不支持的命令！");

                // do nothing.
            }
            else if (command.Directive == GlobalCommands.Instance.ShareCommand.Directive)
            {
                MessageQueueManager.Instance.AddInfo("不支持的命令！");

                // do nothing.
            }
        }

        private void ShowHelp()
        {
            string helpMsg = GlobalResources.HelpMessage;

            SscDialog helpSscDialog = new SscDialog(helpMsg);
            helpSscDialog.ShowDialog();
        }
        private void InitAutoHideSettings()
        {
            //BindingBase bindingBase = new Binding()
            //{
            //    Source = this,
            //    Mode = BindingMode.OneWayToSource,
            //    Path = new PropertyPath("IsWindowActive")
            //};

            //BindingOperations.SetBinding(_discussionClassView, Window.IsActiveProperty, bindingBase);

            _AutoHideMenuTimer = new Timer((state) =>
            {
                _discussionClassView.Dispatcher.BeginInvoke(new Action(() =>
                {
                    IsWindowActive = _discussionClassView.IsActive;
                }));
                //Console.WriteLine($" is meeting view active, {IsWindowActive}");
                if (DateTime.Now > _autoHideInitialTime.AddSeconds(10) && IsWindowActive)
                {
                    IsMenuOpen = false;
                }
            }, null, 0, 2000);
        }

        private void HandleKeyDown(KeyEventArgs keyEventArgs)
        {
            //Console.WriteLine($"key down, {keyEventArgs.Key}");

            switch (keyEventArgs.Key)
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


                    if (IsMenuOpen)
                    {
                        if (_upFocusedUiElement == null)
                        {
                            _upFocusedUiElement = _discussionClassView.ExitButton;
                        }

                        _upFocusedUiElement.Focus();

                    }

                    _pressedUpKeyCount++;

                    if (_pressedUpKeyCount == 4)
                    {
                        _pressedUpKeyCount = 0;
                        //_sdkService.ShowQosTool();
                    }

                    break;
                case Key.Down:
                    _pressedUpKeyCount = 0;
                    _pressedLeftKeyCount = 0;
                    _pressedRightKeyCount = 0;

                    _pressedDownKeyCount++;

                    if (IsMenuOpen)
                    {
                        if (_downFocusedUiElement == null)
                        {
                            ShowUiBasedOnIsSpeaker();
                        }
                        else
                        {
                            _downFocusedUiElement.Focus();
                        }
                    }

                    if (_pressedDownKeyCount == 4)
                    {
                        _pressedDownKeyCount = 0;
                        //_sdkService.CloseQosTool();
                    }


                    break;
                case Key.Enter:
                case Key.Apps:
                    if (!IsMenuOpen)
                    {
                        IsMenuOpen = true;
                        _downFocusedUiElement?.Focus();
                        keyEventArgs.Handled = true;
                    }

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

        private void WindowKeyDownHandler(object obj)
        {
            _autoHideInitialTime = DateTime.Now;
            var keyEventArgs = obj as KeyEventArgs;
            HandleKeyDown(keyEventArgs);
        }

        #region private fields

        private readonly DiscussionClassView _discussionClassView;
        private UIElement _downFocusedUiElement;
        private UIElement _upFocusedUiElement;


        private readonly IMeetingWindowManager _windowManager;

        private readonly IMeetingSdkAgent _meetingSdkAgent;
        private readonly IDeviceNameAccessor _deviceNameAccessor;

        private readonly IEventAggregator _eventAggregator;
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
        private Timer _AutoHideMenuTimer;
        private DateTime _autoHideInitialTime = DateTime.Now;
        private bool _exitByDialog = false;

        private readonly Action<bool, string> _startMeetingCallbackEvent;
        private readonly Action<bool, string> _exitMeetingCallbackEvent;
        private const string OpenDoc = "打开课件";
        private const string CloseDoc = "关闭课件";
        private const string IsSpeaking = "取消发言";
        private const string IsNotSpeaking = "发 言";
        private const string ListenMode = "听课模式";
        private const string DiscussionMode = "评课模式";
        private const string StartRecord = "开启录制";
        private const string StopRecord = "关闭录制";

        #endregion

        #region public properties

        public ViewFrame ViewFrame1 { get; private set; }
        public ViewFrame ViewFrame2 { get; private set; }
        public ViewFrame ViewFrame3 { get; private set; }
        public ViewFrame ViewFrame4 { get; private set; }
        public ViewFrame ViewFrame5 { get; private set; }

        public ObservableCollection<MenuItem> LayoutMenuItems { get; set; }
        public ObservableCollection<MenuItem> SharingMenuItems { get; set; }



        public bool IsWindowActive { get; set; }

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


        private string _meetingOrLesson;

        public string MeetingOrLesson
        {
            get { return _meetingOrLesson; }
            set { SetProperty(ref _meetingOrLesson, value); }
        }

        private int _meetingId;

        public int MeetingId
        {
            get { return _meetingId; }
            set { SetProperty(ref _meetingId, value); }
        }


        private string _recordTips;

        public string RecordTips
        {
            get { return _recordTips; }
            set { SetProperty(ref _recordTips, value); }
        }

        private string _phoneId;

        public string PhoneId
        {
            get { return _phoneId; }
            set { SetProperty(ref _phoneId, value); }
        }

        private string _recordMsg = StartRecord;
        public string RecordMsg
        {
            get { return _recordMsg; }
            set
            {
                if (value == StartRecord)
                {
                    RecordTips = null;
                }
                SetProperty(ref _recordMsg, value);
            }
        }

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

        private string _classMode;

        public string ClassMode
        {
            get { return _classMode; }
            set { SetProperty(ref _classMode, value); }
        }


        #endregion

        #region Commands

        public ICommand LoadCommand { get; set; }
        public ICommand OpenExitDialogCommand { get; set; }
        public ICommand KickoutCommand { get; set; }
        public ICommand SetMicStateCommand { get; set; }
        public ICommand StartSpeakCommand { get; set; }
        public ICommand StopSpeakCommand { get; set; }
        public ICommand RecordCommand { get; set; }
        public ICommand TopMostTriggerCommand { get; set; }
        public ICommand ShowLogCommand { get; set; }
        public ICommand TriggerMenuCommand { get; set; }
        public ICommand WindowKeyDownCommand { get; set; }
        public ICommand TouchDownCommand { get; set; }
        public ICommand DownButtonGotFocusCommand { get; set; }
        public ICommand UpButtonGotFocusCommand { get; set; }
        public ICommand ShowHelpCommand { get; set; }


        private void InitMenuButtonItems()
        {
            ClassMode = DiscussionMode;

            ClassModeItem = new MenuButtonItem()
            {
                Visibility = Visibility.Collapsed,
                Content = ListenMode,
                GotFocusCommand = DownButtonGotFocusCommand,
                Command = DelegateCommand.FromAsyncHandler(async () =>
                {
                    _autoHideInitialTime = DateTime.Now;
                    _downFocusedUiElement = _discussionClassView.ClassModeButton;

                    //var participants = _sdkService.GetParticipants();

                    //List<string> listeners = new List<string>();

                    //participants.ForEach(participant =>
                    //{
                    //    if (participant.PhoneId != _sdkService.CreatorPhoneId)
                    //    {
                    //        listeners.Add(participant.PhoneId);
                    //    }
                    //});

                    switch (ClassMode)
                    {
                        case DiscussionMode: //Goto 听课模式,禁言所有听讲教室。

                            ManageListenersItem.Visibility = Visibility.Collapsed;
                            LayoutItem.Visibility = Visibility.Collapsed;

                            ClassMode = ListenMode;
                            ClassModeItem.Content = DiscussionMode;

                            _windowManager.ModeChange(ModeDisplayerType.ShareMode);

                            MessageQueueManager.Instance.AddInfo(Messages.InfoGotoListenerMode);

                            //foreach (var listener in listeners)
                            //{
                            //    _sdkService.RequireUserStopSpeak(listener);
                            //}
                            int listeningMode = (int)UiMessage.ListeningMode;
                            await _meetingSdkAgent.AsynSendUIMsg(listeningMode, 0, "");

                            break;
                        case ListenMode: //Goto 评课模式，

                            ManageListenersItem.Visibility = Visibility.Visible;
                            LayoutItem.Visibility = Visibility.Visible;

                            ClassMode = DiscussionMode;
                            ClassModeItem.Content = ListenMode;


                            _windowManager.ModeChange(ModeDisplayerType.InteractionMode);

                            MessageQueueManager.Instance.AddInfo(Messages.InfoGotoDiscussionMode);
                            //foreach (var listener in listeners)
                            //{
                            //    _sdkService.RequireUserSpeak(listener);
                            //}

                            int discussionMode = (int)UiMessage.DiscussionMode;
                            await _meetingSdkAgent.AsynSendUIMsg(discussionMode, 0, "");


                            break;
                    }

                    _downFocusedUiElement.Focus();
                })
            };

            ShareDocItem = new MenuButtonItem()
            {
                Visibility = Visibility.Collapsed,
                Content = OpenDoc,
                GotFocusCommand = DownButtonGotFocusCommand,
                Command = DelegateCommand.FromAsyncHandler(async () =>
                {
                    _autoHideInitialTime = DateTime.Now;

                    _downFocusedUiElement = _discussionClassView.ShareDocButton;
                    await OpenCloseDocAsync();
                    _downFocusedUiElement.Focus();
                })
            };

            ManageListenersItem = new MenuButtonItem()
            {
                Visibility = Visibility.Collapsed,
                Content = "评课管理",
                GotFocusCommand = DownButtonGotFocusCommand,
                Command = new DelegateCommand(() =>
                {
                    _autoHideInitialTime = DateTime.Now;

                    _downFocusedUiElement = _discussionClassView.ManageListenersButton;
                    ManageAttendeeListView attendeeListView = new ManageAttendeeListView();
                    attendeeListView.ShowDialog();
                    _downFocusedUiElement.Focus();
                    _autoHideInitialTime = DateTime.Now;

                })
            };

            LayoutItem = new MenuButtonItem()
            {
                Visibility = Visibility.Collapsed,
                Content = "画面布局",
                GotFocusCommand = DownButtonGotFocusCommand,
                Command = new DelegateCommand(() =>
                {
                    _autoHideInitialTime = DateTime.Now;

                    _downFocusedUiElement = _discussionClassView.LayoutButton;
                    //if (!CheckIsUserSpeaking(true))
                    //{
                    //    return;
                    //}

                    LayoutView layoutView = new LayoutView();
                    layoutView.ShowDialog();
                    _downFocusedUiElement.Focus();
                    _autoHideInitialTime = DateTime.Now;

                })
            };

            SpeakItem = new MenuButtonItem()
            {
                Visibility = Visibility.Collapsed,
                Content = IsSpeaking,
                GotFocusCommand = DownButtonGotFocusCommand,
                Command = DelegateCommand.FromAsyncHandler(async () =>
                {
                    _downFocusedUiElement = _discussionClassView.SpeakButton;
                    await SpeakingStatusChangedAsync();
                    //_downFocusedUiElement.Focus();
                })
            };
        }


        public MenuButtonItem ClassModeItem { get; set; }
        public MenuButtonItem ShareDocItem { get; set; }
        public MenuButtonItem ManageListenersItem { get; set; }
        public MenuButtonItem LayoutItem { get; set; }
        public MenuButtonItem SpeakItem { get; set; }
        public bool IsCreator
        {
            get
            {
                return _userInfo.UserId == _lessonDetail.MasterUserId;
            }
        }

        #endregion

        #region Command Handlers

        private void ChangeWindowStyleInDevMode()
        {
            if (GlobalData.Instance.RunMode == RunMode.Development)
            {
                IsTopMost = false;
                _discussionClassView.UseNoneWindowStyle = false;
                _discussionClassView.ResizeMode = ResizeMode.CanResize;
                _discussionClassView.WindowStyle = WindowStyle.SingleBorderWindow;
                _discussionClassView.IsWindowDraggable = true;
                _discussionClassView.ShowMinButton = true;
                _discussionClassView.ShowMaxRestoreButton = true;
                _discussionClassView.ShowCloseButton = false;
                _discussionClassView.IsCloseButtonEnabled = false;
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
                    string err = joinResult.StatusCode == -2014 ? "该课程已经结束！" : "加入课程失败！";

                    _exitByDialog = true;
                    _discussionClassView.Close();
                    _startMeetingCallbackEvent(false, err);
                }
            }
            else
            {
                _exitByDialog = true;
                _discussionClassView.Close();
                _startMeetingCallbackEvent(false, "课程号无效！");
            }


            SetScreenSize();

            await _windowManager.Join(meetingId, false, IsCreator);

            _startMeetingCallbackEvent(true, "");

            IsMenuOpen = true;

            GlobalCommands.Instance.SetCommandsStateInDiscussionClass();

            if (_lessonDetail.Id > 0)
            {
                ResponseResult result = await
                    _bmsService.UpdateMeetingStatus(_lessonDetail.Id, _userInfo.UserId,
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        string.Empty);

                HasErrorMsg(result.Status, result.Message);
            }
            ShowUiBasedOnIsSpeaker();

            InitAutoHideSettings();


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
            //    _discussionClassView.Close();
            //    _startMeetingCallbackEvent(false, joinMeetingResult.Message);

            //}
            //else
            //{
            //    IsMenuOpen = true;

            //    GlobalCommands.Instance.SetCommandsStateInDiscussionClass();

            //    //if join meeting successfully, then make main view invisible
            //    _startMeetingCallbackEvent(true, "");

            //    if (_lessonDetail.Id > 0)
            //    {
            //        ResponseResult result = await
            //            _bmsService.UpdateMeetingStatus(_lessonDetail.Id, _userInfo.UserId,
            //                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //                string.Empty);

            //        HasErrorMsg(result.Status, result.Message);
            //    }
            //    ShowUiBasedOnIsSpeaker();
            //    //if (_sdkService.IsCreator)
            //    //{
            //    InitAutoHideSettings();
            //    //}
            //}

        }

        private void SetScreenSize()
        {
            MeetingSdk.Wpf.IScreen screen = _windowManager.VideoBoxManager as MeetingSdk.Wpf.IScreen;
            screen.Size = new System.Windows.Size(_discussionClassView.ActualWidth, _discussionClassView.ActualHeight);
        }

        //command handlers
        private async Task LoadAsync()
        {
            string errMsg = DeviceSettingsChecker.Instance.IsVideoAudioSettingsValid();

            if (!string.IsNullOrEmpty(errMsg))
            {
                _exitByDialog = true;
                _discussionClassView.Close();
                _startMeetingCallbackEvent(false, errMsg);

                return;
            }

            if (GlobalData.VideoControl == null)
            {
                GlobalData.VideoControl = new VideoControl();
            }

            _discussionClassView.Grid.Children.Add(GlobalData.VideoControl);


            GlobalData.Instance.CurWindowHwnd = new WindowInteropHelper(_discussionClassView).Handle;
            ChangeWindowStyleInDevMode();
            await JoinMeetingAsync();
        }


        private void ShowUiBasedOnIsSpeaker()
        {
            //if not speaker, then clear mode menu items
            if (IsCreator)
            {
                IsSpeaker = Visibility.Visible;


                ClassModeItem.Visibility = Visibility.Visible;
                ShareDocItem.Visibility = Visibility.Visible;
                ManageListenersItem.Visibility = Visibility.Visible;
                LayoutItem.Visibility = Visibility.Visible;

                SpeakItem.Visibility = Visibility.Collapsed;

                _downFocusedUiElement = _discussionClassView.ClassModeButton;

            }
            else
            {
                IsSpeaker = Visibility.Collapsed;

                ClassModeItem.Visibility = Visibility.Collapsed;
                ShareDocItem.Visibility = Visibility.Collapsed;
                ManageListenersItem.Visibility = Visibility.Collapsed;

                LayoutItem.Visibility = Visibility.Visible;

                if (ClassMode == ListenMode)
                {
                    SpeakItem.Visibility = Visibility.Collapsed;

                }
                if (ClassMode == DiscussionMode)
                {
                    SpeakItem.Visibility = Visibility.Visible;

                }

                _downFocusedUiElement = _discussionClassView.LayoutButton;
            }

            _downFocusedUiElement.Focus();
            
        }

        private async Task OpenCloseDocAsync()
        {
            if (!_windowManager.Participant.IsSpeaking)
            {
                MessageQueueManager.Instance.AddInfo(Messages.WarningYouAreNotSpeaking);
                return;
            }

            if (ShareDocItem.Content == OpenDoc)
            {
                //if (_sdkService.IsSharedDocOpened())
                //{
                //    MessageQueueManager.Instance.AddInfo(Messages.DocumentAlreadyOpened);
                //    ShareDocItem.Content = CloseDoc;
                //    return;
                //}


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

                MeetingResult<int> publishDocCameraResult = await _windowManager.Publish(MeetingSdk.NetAgent.Models.MediaType.VideoDoc, docCameras.FirstOrDefault());
                MeetingResult<int> publishDocMicResult = await _windowManager.Publish(MeetingSdk.NetAgent.Models.MediaType.AudioDoc, docMics.FirstOrDefault());

                if (publishDocCameraResult.StatusCode != 0 || publishDocMicResult.StatusCode != 0)
                {
                    ShareDocItem.Content = OpenDoc;
                    MessageQueueManager.Instance.AddInfo("打开课件失败！");

                    return;
                }

                ShareDocItem.Content = CloseDoc;
            }
            else if (ShareDocItem.Content == CloseDoc)
            {
                int? docCameraResourceId = 0;
                int? docAudioResourceId = 0;

                docCameraResourceId = _windowManager.Participant.Resources.FirstOrDefault(res => res.MediaType == MediaType.VideoDoc)?.ResourceId;
                docAudioResourceId = _windowManager.Participant.Resources.FirstOrDefault(res => res.MediaType == MediaType.AudioDoc)?.ResourceId;

                if (docCameraResourceId.HasValue && docCameraResourceId.Value != 0)
                {
                    MeetingResult stopShareCameraResult = await _windowManager.Unpublish(MeetingSdk.NetAgent.Models.MediaType.VideoDoc, docCameraResourceId.Value);
                }

                if (docAudioResourceId.HasValue && docAudioResourceId.Value != 0)
                {
                    MeetingResult stopShareMicResult = await _windowManager.Unpublish(MeetingSdk.NetAgent.Models.MediaType.AudioDoc, docAudioResourceId.Value);
                }

                ShareDocItem.Content = OpenDoc;
            }
        }

        private async Task SpeakingStatusChangedAsync()
        {
            if (SpeakItem.Content == IsSpeaking)
            {
                await _meetingSdkAgent.AskForStopSpeak();

                return;
                //will change SpeakStatus in StopSpeakCallbackEventHandler.
            }

            if (SpeakItem.Content == IsNotSpeaking)
            {
                var result = await _meetingSdkAgent.AskForSpeak();
                if (!HasErrorMsg(result.StatusCode.ToString(), result.Message))
                {
                    // will change SpeakStatus in callback???
                    SpeakItem.Content = IsSpeaking;
                }
            }
        }

        public async Task ExitAsync()
        {
            try
            {
                _autoHideInitialTime = DateTime.Now;

                StopAllLives();

                MeetingResult meetingResult = await _meetingSdkAgent.LeaveMeeting();

                await _windowManager.Leave();

                Log.Logger.Debug($"【exit meeting】：result={meetingResult.StatusCode}, msg={meetingResult.Message}");
                HasErrorMsg(meetingResult.StatusCode.ToString(), meetingResult.Message);

                _discussionClassView.Grid.Children.Remove(GlobalData.VideoControl);

                _AutoHideMenuTimer?.Dispose();

                UnRegisterMeetingEvents();

                await UpdateExitTime();


                await _discussionClassView.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _exitByDialog = true;
                    _discussionClassView.Close();

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
            await _discussionClassView.Dispatcher.BeginInvoke(new Action(async () =>
            {
                IsMenuOpen = false;
                _upFocusedUiElement = _discussionClassView.ExitButton;


                YesNoDialog yesNoDialog = new YesNoDialog("确定退出？");
                bool? result = yesNoDialog.ShowDialog();
                _autoHideInitialTime = DateTime.Now;

                if (result.HasValue && result.Value)
                {
                    await ExitAsync();
                }
                else
                {
                    IsMenuOpen = true;
                    _upFocusedUiElement.Focus();
                }

            }));
        }

        private void KickoutAsync(string userPhoneId)
        {
            //_sdkService.HostKickoutUser(userPhoneId);
        }

        private void RecordAsync()
        {
            _autoHideInitialTime = DateTime.Now;

            _upFocusedUiElement = _discussionClassView.RecordButton;

            if (!HasStreams())
            {
                HasErrorMsg("-1", "当前没有音视频流，无法录制！");
                return;
            }


            switch (RecordMsg)
            {
                case StartRecord:



                    var recordRt = _localRecordService.GetRecordParam();

                    if (!recordRt)
                    {
                        HasErrorMsg("-1", "录制参数未正确配置！");
                        RecordTips = null;
                        RecordMsg = StartRecord;
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
                        RecordMsg = StartRecord;
                    }
                    else
                    {
                        RecordMsg = StopRecord;
                        RecordTips =
                                string.Format(
                                    $"分辨率：{_localRecordService.RecordParam.LiveParameter.Width}*{_localRecordService.RecordParam.LiveParameter.Height}\r\n" +
                                    $"码率：{_localRecordService.RecordParam.LiveParameter.VideoBitrate}\r\n" +
                                    $"录制路径：{_localRecordService.RecordDirectory}");
                    }

                    break;
                case StopRecord:
                    MeetingResult stopResult = _localRecordService.StopMp4Record();
                    RecordTips = null;

                    if (stopResult.StatusCode != 0)
                    {
                        HasErrorMsg("-1", stopResult.Message);
                        RecordMsg = StopRecord;
                    }

                    RecordMsg = StartRecord;

                    break;
            }

            _upFocusedUiElement.Focus();
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

        #endregion

        #region Methods

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



            _discussionClassView.LocationChanged += _discussionClassView_LocationChanged;
            _discussionClassView.Deactivated += _discussionClassView_Deactivated;
            _discussionClassView.Closing += _meetingView_Closing;


            //_sdkService.ViewCreatedEvent += ViewCreateEventHandler;
            //_sdkService.ViewClosedEvent += ViewCloseEventHandler;
            //_sdkService.CloseSharedDocEvent += _sdkService_CloseSharedDocEvent;
            //_sdkService.StartSpeakEvent += StartSpeakEventHandler;
            //_sdkService.StopSpeakEvent += StopSpeakEventHandler;
            ////_viewLayoutService.MeetingModeChangedEvent += MeetingModeChangedEventHandler;
            ////_viewLayoutService.ViewModeChangedEvent += ViewModeChangedEventHandler;
            //_sdkService.OtherJoinMeetingEvent += OtherJoinMeetingEventHandler;
            //_sdkService.OtherExitMeetingEvent += OtherExitMeetingEventHandler;
            //_sdkService.TransparentMessageReceivedEvent += UIMessageReceivedEventHandler;
            //_sdkService.ErrorMsgReceivedEvent += ErrorMsgReceivedEventHandler;
            //_sdkService.KickedByHostEvent += KickedByHostEventHandler;
            //_sdkService.DiskSpaceNotEnoughEvent += DiskSpaceNotEnoughEventHandler;
        }

        private void _discussionClassView_LocationChanged(object sender, EventArgs e)
        {
            try
            {
                MethodInfo methodInfo = typeof(Popup).GetMethod("UpdatePosition",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (IsMenuOpen)
                {
                    methodInfo.Invoke(_discussionClassView.TopMenu, null);
                    methodInfo.Invoke(_discussionClassView.BottomMenu, null);
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"_discussionClassView_LocationChanged => {ex}");
            }
        }

        private void _discussionClassView_Deactivated(object sender, EventArgs e)
        {
            IsMenuOpen = false;
        }

        //private void _sdkService_CloseSharedDocEvent(AsyncCallbackMsg asyncCallbackMsg)
        //{
        //    ShareDocItem.Content = OpenDoc;
        //}

        //private void DiskSpaceNotEnoughEventHandler(AsyncCallbackMsg msg)
        //{
        //    HasErrorMsg(msg.Status.ToString(), msg.Message);
        //}

        private async void _meetingView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
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
            //_sdkService.CloseSharedDocEvent -= _sdkService_CloseSharedDocEvent;
            //_sdkService.StartSpeakEvent -= StartSpeakEventHandler;
            //_sdkService.StopSpeakEvent -= StopSpeakEventHandler;
            ////_viewLayoutService.MeetingModeChangedEvent -= MeetingModeChangedEventHandler;
            ////_viewLayoutService.ViewModeChangedEvent -= ViewModeChangedEventHandler;
            //_sdkService.OtherJoinMeetingEvent -= OtherJoinMeetingEventHandler;
            //_sdkService.OtherExitMeetingEvent -= OtherExitMeetingEventHandler;
            //_sdkService.TransparentMessageReceivedEvent -= UIMessageReceivedEventHandler;
            //_sdkService.ErrorMsgReceivedEvent -= ErrorMsgReceivedEventHandler;
            //_sdkService.KickedByHostEvent -= KickedByHostEventHandler;
            //_sdkService.DiskSpaceNotEnoughEvent -= DiskSpaceNotEnoughEventHandler;
        }




        private void RemoveVideoControlEventHandler()
        {
            _discussionClassView.Grid.Children.Remove(GlobalData.VideoControl);
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
                if (IsCreator && !_serverPushLiveService.HasPushLiveSuccessfully && _lessonDetail.Live)
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


        private void LayoutChangedEventHandler(LayoutRenderType obj)
        {
            CurLayoutName = EnumHelper.GetDescription(typeof(LayoutRenderType), obj);
            CurModeName = EnumHelper.GetDescription(typeof(ModeDisplayerType), _windowManager.ModeDisplayerStore.CurrentModeDisplayerType);

            RefreshLocalRecordLive();

            RefreshRemotePushLive();
            RefreshPushLive();

            StartAutoLiveAsync();
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


        private async void UiTransparentMsgReceivedEventHandler(UiTransparentMsg obj)
        {


            if (obj.MsgId < 3)
            {
                GlobalData.AddOrUpdate(CacheKey.HostId, obj.TargetAccountId);

                var classMode = (ModeDisplayerType)obj.MsgId;


                if (_windowManager.ModeChange(classMode))
                {
                }
            }
            else
            {
                if (obj.MsgId == (int)UiMessage.BannedToSpeak)
                {

                   await _meetingSdkAgent.AskForStopSpeak();
                    return;
                }

                if (obj.MsgId == (int)UiMessage.AllowToSpeak)
                {
                    var result = await _meetingSdkAgent.AskForSpeak();
                    if (!HasErrorMsg(result.StatusCode.ToString(), result.Message))
                    {
                        // will change SpeakStatus in callback???
                        SpeakItem.Content = IsSpeaking;
                    }

                    return;
                }

                if (obj.MsgId == (int)UiMessage.ListeningMode)
                {
                    ClassMode = ListenMode;
                    SpeakItem.Visibility = Visibility.Collapsed;
                    MessageQueueManager.Instance.AddInfo(Messages.InfoGotoListenerMode);

                    await _meetingSdkAgent.AskForStopSpeak();
                    return;

                }
                if (obj.MsgId == (int)UiMessage.DiscussionMode)
                {
                    GotoDiscussionMode();
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
                    GotoDiscussionMode();
                    //if (_windowManager.ModeChange(ModeDisplayerType.InteractionMode))
                    //{

                    //}
                }
            }

        }

        private void OtherJoinMeetingEventHandler(AccountModel obj)
        {
            if (IsCreator)
            {
                _meetingSdkAgent.AsynSendUIMsg((int)_windowManager.ModeDisplayerStore.CurrentModeDisplayerType, obj.AccountId, "");


                // send a message to sync new attendee's class mode
                int messageId = (int)UiMessage.DiscussionMode;
                if (ClassMode == ListenMode)
                {
                    messageId = (int)UiMessage.ListeningMode;
                    //_sdkService.RequireUserStopSpeak(contactInfo.PhoneId);

                }
                else if (ClassMode == DiscussionMode)
                {
                    messageId = (int)UiMessage.DiscussionMode;
                }

                _meetingSdkAgent.AsynSendUIMsg(messageId, obj.AccountId, "");
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

            SpeakItem.Content = IsNotSpeaking;
        }


        private void StartSpeakEventHandler(SpeakModel obj)
        {
            SpeakItem.Content = IsSpeaking;
        }
















        private void KickedByHostEventHandler()
        {
            _discussionClassView.Dispatcher.BeginInvoke(new Action(() =>
            {
                _exitByDialog = true;

                _discussionClassView.Close();


                _exitMeetingCallbackEvent(true, "");

                //MetroWindow mainView = App.SSCBootstrapper.Container.ResolveKeyed<MetroWindow>("MainView");
                //mainView.GlowBrush = new SolidColorBrush(Colors.Purple);
                //mainView.NonActiveGlowBrush = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#FF999999"));
                //mainView.Visibility = Visibility.Visible;
            }));
        }

        //private void ViewModeChangedEventHandler(ViewMode viewMode)
        //{
        //    CurLayoutName = EnumHelper.GetDescription(typeof(ViewMode), viewMode);
        //}

        //private void MeetingModeChangedEventHandler(MeetingMode meetingMode)
        //{
        //    CurModeName = EnumHelper.GetDescription(typeof(MeetingMode), meetingMode);

        //    if (_sdkService.IsCreator)
        //    {
        //        AsyncCallbackMsg result =
        //            _sdkService.SendMessage((int) _viewLayoutService.MeetingMode,
        //                _viewLayoutService.MeetingMode.ToString(), _viewLayoutService.MeetingMode.ToString().Length,
        //                null);
        //        HasErrorMsg(result.Status.ToString(), result.Message);
        //    }
        //}

        //private void ErrorMsgReceivedEventHandler(AsyncCallbackMsg error)
        //{
        //    HasErrorMsg("-1", error.Message);
        //}

        //private async void UIMessageReceivedEventHandler(TransparentMessage message)
        //{
        //    Log.Logger.Debug(
        //        $"UIMessageReceivedEventHandler => msgId={message.MessageId}, senderPhoneId={message.Sender?.PhoneId}");

        //    //if (message.MessageId < 3)
        //    //{
        //    //    _sdkService.CreatorPhoneId = message.Sender.PhoneId;

        //    //    MeetingMode meetingMode = (MeetingMode) message.MessageId;
        //    //    _viewLayoutService.ChangeMeetingMode(meetingMode);

        //    //    await _viewLayoutService.LaunchLayout();
        //    //}
        //    //else
        //    //{
        //    //    if (message.MessageId == (int) UiMessage.BannedToSpeak)
        //    //    {

        //    //        AsyncCallbackMsg stopSucceeded = await _sdkService.StopSpeak();
        //    //        return;
        //    //    }

        //    //    if (message.MessageId == (int) UiMessage.AllowToSpeak)
        //    //    {
        //    //        AsyncCallbackMsg result = await _sdkService.ApplyToSpeak();
        //    //        if (!HasErrorMsg(result.Status.ToString(), result.Message))
        //    //        {
        //    //            // will change SpeakStatus in callback???
        //    //            SpeakItem.Content = IsSpeaking;
        //    //        }

        //    //        return;
        //    //    }

        //    //    if (message.MessageId == (int) UiMessage.ListeningMode)
        //    //    {
        //    //        ClassMode = ListenMode;
        //    //        SpeakItem.Visibility = Visibility.Collapsed;
        //    //        MessageQueueManager.Instance.AddInfo(Messages.InfoGotoListenerMode);

        //    //        await _sdkService.StopSpeak();
        //    //        return;

        //    //    }
        //    //    if (message.MessageId == (int) UiMessage.DiscussionMode)
        //    //    {
        //    //       await GotoDiscussionMode();
        //    //    }
        //    //}
        //}

        private void GotoDiscussionMode()
        {
            _windowManager.ModeChange(ModeDisplayerType.InteractionMode);

            ClassMode = DiscussionMode;
            SpeakItem.Visibility = Visibility.Visible;
            MessageQueueManager.Instance.AddInfo(Messages.InfoGotoDiscussionMode);
        }

        //private async void OtherExitMeetingEventHandler(Participant contactInfo)
        //{
        //    //var attendee = _userInfos.FirstOrDefault(userInfo => userInfo.GetNube() == contactInfo.m_szPhoneId);

        //    //string displayName = string.Empty;
        //    //if (!string.IsNullOrEmpty(attendee?.Name))
        //    //{
        //    //    displayName = attendee.Name + " - ";
        //    //}

        //    //string exitMsg = $"{displayName}{contactInfo.m_szPhoneId}退出会议！";
        //    //HasErrorMsg("-1", exitMsg);

        //    if (contactInfo.PhoneId == _sdkService.CreatorPhoneId)
        //    {
        //        await GotoDiscussionMode();
        //    }
        //}

        //private void OtherJoinMeetingEventHandler(Participant contactInfo)
        //{
        //    //var attendee = _userInfos.FirstOrDefault(userInfo => userInfo.GetNube() == contactInfo.PhoneId);

        //    ////string displayName = string.Empty;
        //    ////if (!string.IsNullOrEmpty(attendee?.Name))
        //    ////{
        //    ////    displayName = attendee.Name + " - ";
        //    ////}

        //    ////string joinMsg = $"{displayName}{contactInfo.m_szPhoneId}加入会议！";
        //    ////HasErrorMsg("-1", joinMsg);

        //    ////speaker automatically sends a message(with creatorPhoneId) to nonspeakers
        //    ////!!!CAREFUL!!! ONLY speaker will call this

        //    //Log.Logger.Debug($"OtherJoinMeetingEventHandler => phoneId={contactInfo.PhoneId}, name={contactInfo.Name}");

        //    //if (_sdkService.IsCreator)
        //    //{
        //    //    //var newView =
        //    //    //    _viewLayoutService.ViewFrameList.FirstOrDefault(
        //    //    //        v => v.PhoneId == contactInfo.PhoneId && v.ViewType == 1);

        //    //    //if (newView != null) newView.IsIntoMeeting = true;

        //    //    //_sdkService.SendUIMessage((int) _viewLayoutService.MeetingMode,
        //    //    //    _viewLayoutService.MeetingMode.ToString(), _viewLayoutService.MeetingMode.ToString().Length, null);


        //    //    int meetingModeCmd = (int) _viewLayoutService.MeetingMode;
        //    //    AsyncCallbackMsg sendMeetingModeMsg = _sdkService.SendMessage(meetingModeCmd,
        //    //        meetingModeCmd.ToString(), meetingModeCmd.ToString().Length,
        //    //        contactInfo.PhoneId);

        //    //    Log.Logger.Debug(
        //    //        $"sendMeetingModeMsg => msgId={meetingModeCmd}, targetPhoneId={contactInfo.PhoneId}, result={sendMeetingModeMsg.Status}");

        //    //    // send a message to sync new attendee's class mode
        //    //    int messageId = (int) UiMessage.DiscussionMode;
        //    //    if (ClassMode == ListenMode)
        //    //    {
        //    //        messageId = (int) UiMessage.ListeningMode;
        //    //        //_sdkService.RequireUserStopSpeak(contactInfo.PhoneId);

        //    //    }
        //    //    else if (ClassMode == DiscussionMode)
        //    //    {
        //    //        messageId = (int) UiMessage.DiscussionMode;
        //    //    }

        //    //    AsyncCallbackMsg sendClassModeMsg = _sdkService.SendMessage(messageId, messageId.ToString(),
        //    //        messageId.ToString().Length,
        //    //        contactInfo.PhoneId);

        //    //    Log.Logger.Debug(
        //    //        $"sendClassModeMsg => msgId={messageId}, targetPhoneId={contactInfo.PhoneId}, result={sendClassModeMsg.Status}");

        //    //}
        //}

        //private async void ViewCloseEventHandler(ParticipantView speakerView)
        //{
        //    //await _viewLayoutService.HideViewAsync(speakerView);
        //}

        private void StopSpeakEventHandler()
        {
            ////_viewLayoutService.ChangeViewMode(ViewMode.Auto);

            //if (_sdkService.IsCreator)
            //{
            //    _viewLayoutService.ChangeMeetingMode(MeetingMode.Interaction);
            //}

            //SpeakItem.Content = IsNotSpeaking;

            ////reload menus
        }

        private void StartSpeakEventHandler()
        {
            SpeakItem.Content = IsSpeaking;
        }

        //private async void ViewCreateEventHandler(ParticipantView speakerView)
        //{
        //    //await _viewLayoutService.ShowViewAsync(speakerView);
        //}

        private bool CheckIsUserSpeaking(bool showMsgBar = false)
        {
            //var self =
            //    _viewLayoutService.ViewFrameList.FirstOrDefault(p => p.PhoneId == _sdkService.SelfPhoneId && p.IsOpened);

            //if (showMsgBar && self == null)
            //{
            //    MessageQueueManager.Instance.AddInfo(Messages.WarningYouAreNotSpeaking);
            //}

            //return self != null;

            return true;
        }

        

        #endregion
    }
}