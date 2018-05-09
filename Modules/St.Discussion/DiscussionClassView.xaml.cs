using System;

namespace St.Discussion
{
    /// <summary>
    /// MeetingView.xaml 的交互逻辑
    /// </summary>
    public partial class DiscussionClassView
    {
        public DiscussionClassView(Action<bool, string> startMeetingCallback, Action<bool, string> exitMeetingCallback)
        {
            InitializeComponent();

            DiscussionClassViewModel mvm = new DiscussionClassViewModel(this, startMeetingCallback, exitMeetingCallback);
            DataContext = mvm;
        }
    }
}
