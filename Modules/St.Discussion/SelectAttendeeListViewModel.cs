using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;
using MeetingSdk.Wpf;
using Prism.Commands;
using St.Common;
using St.Common.Helper;
using WindowsInput.Native;
using UserInfo = St.Common.UserInfo;

namespace St.Discussion
{
    public class SelectAttendeeListViewModel
    {
        private readonly SelectAttendeeListView _selectAttendeeListView;
        private readonly List<UserInfo> _userInfos;
        private readonly LayoutRenderType _targetSpecialViewType;
        private readonly IMeetingWindowManager _windowManager;

        public SelectAttendeeListViewModel(SelectAttendeeListView selectAttendeeListView, LayoutRenderType specialViewType)
        {
            _windowManager = IoC.Get<IMeetingWindowManager>();

            _selectAttendeeListView = selectAttendeeListView;
            _targetSpecialViewType = specialViewType;

            _userInfos = IoC.Get<List<UserInfo>>();

            AttendeeItems = new ObservableCollection<AttendeeItem>();

            LoadedCommand = new DelegateCommand(LoadedAsync);
            WindowKeyDownCommand = new DelegateCommand<object>(WindowKeyDownHandlerAsync);
        }

        private void WindowKeyDownHandlerAsync(object args)
        {
            KeyEventArgs keyEventArgs = args as KeyEventArgs;
            if (keyEventArgs != null)
            {
                switch (keyEventArgs.Key)
                {
                    case Key.Escape:
                        _selectAttendeeListView.Close();
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
            var videoboxs = _windowManager.VideoBoxManager.Items;

            var attendees = from videobox in videoboxs
                            select new AttendeeItem()
                            {
                                Text = videobox.Name,
                                Id = videobox.AccountResource.AccountModel.AccountId.ToString(),
                                Hwnd = videobox.Handle,
                                ButtonCommand = DelegateCommand<AttendeeItem>.FromAsyncHandler(async (attendeeItem) =>
                                {

                                    _windowManager.VideoBoxManager.SetProperty(_targetSpecialViewType.ToString(), attendeeItem.Text);


                                    _windowManager.LayoutChange(WindowNames.MainWindow, _targetSpecialViewType);
                                    _selectAttendeeListView.Close();
                                })
                            };

            attendees.ToList().ForEach(attendee =>
            {
                AttendeeItems.Add(attendee);
            });

            InputSimulatorManager.Instance.Simulator.Keyboard.KeyPress(VirtualKeyCode.TAB);
            //InputSimulatorManager.Instance.Simulator.Keyboard.KeyPress(VirtualKeyCode.TAB);

        }


        private bool CheckIsUserSpeaking(ViewFrame speakerView, bool showMsgBar = false)
        {
            ////return true;


            //var speaker =
            //    _viewLayoutService.ViewFrameList.FirstOrDefault(
            //        p => p.PhoneId == speakerView.PhoneId && p.Hwnd == speakerView.Hwnd && p.IsOpened);

            //bool isUserNotSpeaking = speaker == null;

            //if (isUserNotSpeaking && showMsgBar)
            //{
            //    MessageQueueManager.Instance.AddInfo(Messages.WarningUserNotSpeaking);
            //}

            //return !isUserNotSpeaking;

            return true;
        }


        public ObservableCollection<AttendeeItem> AttendeeItems { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand WindowKeyDownCommand { get; set; }
    }
}