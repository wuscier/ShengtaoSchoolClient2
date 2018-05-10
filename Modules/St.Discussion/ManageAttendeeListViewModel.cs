using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WindowsInput.Native;
using Caliburn.Micro;
using Prism.Commands;
using St.Common;
using MeetingSdk.NetAgent;
using MeetingSdk.NetAgent.Models;
using System;
using System.Linq;
using MeetingSdk.Wpf;
using UserInfo = St.Common.UserInfo;
using Common;

namespace St.Discussion
{
    public class ManageAttendeeListViewModel
    {
        private readonly ManageAttendeeListView _manageAttendeeListView;
        public const string SetSpeaking = "指定发言";
        public const string CancelSpeaking = "取消发言";
        private readonly List<UserInfo> _userInfos;
        private readonly IMeetingSdkAgent _meetingSdkAgent;
        private readonly IMeetingWindowManager _windowManager;

        public ManageAttendeeListViewModel(ManageAttendeeListView manageAttendeeListView)
        {
            _manageAttendeeListView = manageAttendeeListView;
            _manageAttendeeListView.Closing += _manageAttendeeListView_Closing;
            _userInfos = IoC.Get<List<UserInfo>>();
            _meetingSdkAgent = IoC.Get<IMeetingSdkAgent>();
            _windowManager = IoC.Get<IMeetingWindowManager>();

            RegisterEvents();

            AttendeeItems = new ObservableCollection<AttendeeItem>();

            LoadedCommand = new DelegateCommand(LoadedAsync);
            WindowKeyDownCommand = new DelegateCommand<object>(WindowKeyDownHandlerAsync);
        }

        private void _manageAttendeeListView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UnregisterEvents();
        }

        private void UnregisterEvents()
        {
            _meetingSdkAgent.OnUserStartSpeakEvent -= UserStartSpeakEventHandler;
            _meetingSdkAgent.OnUserStopSpeakEvent -= UserStopSpeakEventHandler;
        }

        private void RegisterEvents()
        {
            _meetingSdkAgent.OnUserStartSpeakEvent += UserStartSpeakEventHandler;
            _meetingSdkAgent.OnUserStopSpeakEvent += UserStopSpeakEventHandler;
        }

        private void UserStopSpeakEventHandler(MeetingResult<UserSpeakModel> obj)
        {
            var attendeeItem = AttendeeItems.FirstOrDefault(attendee => attendee.Id == obj.Result.Account.AccountId.ToString());
            if (attendeeItem != null) attendeeItem.Content = SetSpeaking;
        }

        private void UserStartSpeakEventHandler(MeetingResult<UserSpeakModel> obj)
        {
            var attendeeItem = AttendeeItems.FirstOrDefault(attendee => attendee.Id == obj.Result.Account.AccountId.ToString());
            if (attendeeItem != null) attendeeItem.Content = CancelSpeaking;
        }

        //private void OtherStopSpeakEventHandler(Participant participant)
        //{
        //    var attendeeItem = AttendeeItems.FirstOrDefault(attendee => attendee.Id == participant.PhoneId);
        //    if (attendeeItem != null) attendeeItem.Content = SetSpeaking;
        //}

        //private void OtherStartSpeakEventHandler(Participant participant)
        //{
        //    var attendeeItem = AttendeeItems.FirstOrDefault(attendee => attendee.Id == participant.PhoneId);
        //    if (attendeeItem != null) attendeeItem.Content = CancelSpeaking;
        //}

        //private void _sdkService_ViewCloseEvent(ParticipantView speakerView)
        //{
        //    var targetAttendee = AttendeeItems.FirstOrDefault(
        //        attendee => attendee.Id == speakerView.Participant.PhoneId && attendee.Hwnd == speakerView.Hwnd);

        //    if (targetAttendee != null) targetAttendee.Content = SetSpeaking;
        //}

        //private void _sdkService_ViewCreateEvent(ParticipantView speakerView)
        //{
        //    var targetAttendee = AttendeeItems.FirstOrDefault(
        //        attendee => attendee.Id == speakerView.Participant.PhoneId && attendee.Hwnd == speakerView.Hwnd);

        //    if (targetAttendee != null) targetAttendee.Content = CancelSpeaking;
        //}

        private void WindowKeyDownHandlerAsync(object args)
        {
            KeyEventArgs keyEventArgs = args as KeyEventArgs;
            if (keyEventArgs != null)
            {
                switch (keyEventArgs.Key)
                {
                    case Key.Escape:
                        _manageAttendeeListView.Close();
                        break;
                    //case Key.Up:
                    //    InputSimulatorManager.Instance.Simulator.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    //    break;
                    //case Key.Down:
                    //    InputSimulatorManager.Instance.Simulator.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    //    break;
                }
            }
        }

        private void LoadedAsync()
        {
            List<Participant> participants = new List<Participant>();

            participants.Add(_windowManager.Participant);

            foreach (var participant in _windowManager.Participants)
            {
                participants.Add(participant);
            }

            participants.ForEach(view =>
            {
                AttendeeItems.Add(new AttendeeItem()
                {
                    Text =
                        view.Account.AccountId.ToString() == GlobalData.TryGet(CacheKey.HostId).ToString()
                            ? "我"
                            : _userInfos.FirstOrDefault(user => user.GetNube() == view.Account.AccountId.ToString())?.Name,
                    Content = view.IsSpeaking ? CancelSpeaking : SetSpeaking,
                    Id = view.Account.AccountId.ToString(),
                    ButtonCommand = view.Account.AccountId.ToString() == GlobalData.TryGet(CacheKey.HostId).ToString()
                        ? new DelegateCommand<AttendeeItem>(async (self) =>
                        {
                            switch (self.Content)
                            {
                                case CancelSpeaking:
                                    var stopSpeakMsg = await _meetingSdkAgent.AskForStopSpeak();
                                    if (stopSpeakMsg.StatusCode != 0)
                                    {
                                        self.Content = CancelSpeaking;
                                        MessageQueueManager.Instance.AddInfo(stopSpeakMsg.Message);
                                    }
                                    else
                                    {
                                        self.Content = SetSpeaking;
                                    }
                                    break;

                                case SetSpeaking:
                                    var startSpeakMsg = await _meetingSdkAgent.AskForSpeak();
                                    if (startSpeakMsg.StatusCode != 0)
                                    {
                                        self.Content = SetSpeaking;
                                        MessageQueueManager.Instance.AddInfo(startSpeakMsg.Message);
                                    }
                                    else
                                    {
                                        self.Content = CancelSpeaking;
                                    }
                                    break;
                            }
                        })
                        : new DelegateCommand<AttendeeItem>(async (attendeeItem) =>
                        {

                            switch (attendeeItem.Content)
                            {
                                case CancelSpeaking:

                                    //AsyncCallbackMsg stopCallbackMsg = _sdkService.RequireUserStopSpeak(attendeeItem.Id);
                                    int stopSpeakCmd = (int)UiMessage.BannedToSpeak;
                                    var sendStopSpeakMsg = await _meetingSdkAgent.AsynSendUIMsg(stopSpeakCmd, int.Parse(attendeeItem.Id), "");

                                    if (sendStopSpeakMsg.StatusCode != 0)
                                    {
                                        attendeeItem.Content = CancelSpeaking;
                                        MessageQueueManager.Instance.AddInfo(sendStopSpeakMsg.Message);
                                    }
                                    else
                                    {
                                        attendeeItem.Content = SetSpeaking;
                                    }

                                    break;
                                case SetSpeaking:
                                    //AsyncCallbackMsg startCallbackMsg = _sdkService.RequireUserSpeak(attendeeItem.Id);

                                    int startSpeakCmd = (int)UiMessage.AllowToSpeak;
                                    var sendStartSpeakMsg = await _meetingSdkAgent.AsynSendUIMsg(startSpeakCmd, int.Parse(attendeeItem.Id),"");


                                    if (sendStartSpeakMsg.StatusCode != 0)
                                    {
                                        attendeeItem.Content = SetSpeaking;
                                        MessageQueueManager.Instance.AddInfo(sendStartSpeakMsg.Message);
                                    }
                                    else
                                    {
                                        attendeeItem.Content = CancelSpeaking;
                                    }

                                    break;
                            }
                        })
                });
            });

            //InputSimulatorManager.Instance.Simulator.Keyboard.KeyPress(VirtualKeyCode.TAB);
            InputSimulatorManager.Instance.Simulator.Keyboard.KeyPress(VirtualKeyCode.TAB);
        }

        public ObservableCollection<AttendeeItem> AttendeeItems { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand WindowKeyDownCommand { get; set; }
    }
}