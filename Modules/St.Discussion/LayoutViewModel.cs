using System.Threading.Tasks;
using System.Windows.Input;
using WindowsInput.Native;
using Prism.Commands;
using St.Common;
using MeetingSdk.Wpf;
using Caliburn.Micro;
using St.Common.Helper;

namespace St.Discussion
{
    public class LayoutViewModel
    {
        private readonly LayoutView _layoutView;
        private readonly IMeetingWindowManager _windowManager;

        public LayoutViewModel(LayoutView layoutView)
        {
            _layoutView = layoutView;

            _windowManager = IoC.Get<IMeetingWindowManager>();

            WindowKeyDownCommand = new DelegateCommand<object>(WindowKeyDownHandlerAsync);
            SetAverageLayoutCommand = new DelegateCommand(SetAverageLayoutAsync);
            SelectAttendeeAsBigCommand = new DelegateCommand(SelectAttendeeAsBig);
            SelectAttendeeAsFullCommand = new DelegateCommand(SelectAttendeeAsFull);
            LoadedCommand = new DelegateCommand(() =>
            {
                InputSimulatorManager.Instance.Simulator.Keyboard.KeyPress(VirtualKeyCode.TAB);
            });
        }

        private void SelectAttendeeAsFull()
        {
            SelectAttendeeListView selectAttendeeListView = new SelectAttendeeListView(LayoutRenderType.CloseupLayout);
            selectAttendeeListView.ShowDialog();
        }

        private void SelectAttendeeAsBig()
        {
            SelectAttendeeListView selectAttendeeListView = new SelectAttendeeListView(LayoutRenderType.BigSmallsLayout);
            selectAttendeeListView.ShowDialog();
        }

        private void SetAverageLayoutAsync()
        {
            _windowManager.LayoutChange(WindowNames.MainWindow, LayoutRenderType.AverageLayout);
            _layoutView.Close();
        }

        private void WindowKeyDownHandlerAsync(object args)
        {
            KeyEventArgs keyEventArgs = args as KeyEventArgs;
            if (keyEventArgs != null)
            {
                switch (keyEventArgs.Key)
                {
                    case Key.Escape:
                        _layoutView.Close();
                        break;
                    case Key.Up:
                        InputSimulatorManager.Instance.Simulator.Keyboard.KeyPress(VirtualKeyCode.TAB);
                        break;
                    case Key.Down:
                        InputSimulatorManager.Instance.Simulator.Keyboard.KeyPress(VirtualKeyCode.TAB);
                        break;
                }
            }
        }


        public ICommand WindowKeyDownCommand { get; set; }

        public ICommand SetAverageLayoutCommand { get; set; }
        public ICommand SelectAttendeeAsBigCommand { get; set; }
        public ICommand SelectAttendeeAsFullCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
    }
}