using System.Windows;
using MeetingSdk.Wpf;
using St.Common;

namespace St.Discussion
{
    /// <summary>
    /// MeetingView.xaml 的交互逻辑
    /// </summary>
    public partial class SelectAttendeeListView
    {
        public SelectAttendeeListView(LayoutRenderType specialViewType)
        {
            InitializeComponent();

            DataContext = new SelectAttendeeListViewModel(this, specialViewType);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}