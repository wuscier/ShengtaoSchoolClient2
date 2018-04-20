using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dragablz;
using Dragablz.Dockablz;

namespace St.Common.Dragable
{
    public class HostViewModel
    {
        public HostViewModel()
        {
            Items = new ObservableCollection<HeaderedItemViewModel>();
        }

        public HostViewModel(params HeaderedItemViewModel[] items)
        {
            Items = new ObservableCollection<HeaderedItemViewModel>(items);
        }

        public ObservableCollection<HeaderedItemViewModel> Items { get; }

        public ObservableCollection<HeaderedItemViewModel> ToolItems { get; } =
            new ObservableCollection<HeaderedItemViewModel>();

        public static Guid TabPartition => new Guid("2AE89D18-F236-4D20-9605-6C03319038E6");

        public IInterTabClient InterTabClient { get; } = new HostInterTabClient();

        public ItemActionCallback ClosingTabItemHandler => ClosingTabItemHandlerImpl;

        private void ClosingTabItemHandlerImpl(ItemActionCallbackArgs<TabablzControl> args)
        {
            var viewModel = args.DragablzItem.DataContext as HeaderedItemViewModel;
            Debug.Assert(viewModel != null);
        }

        public ClosingFloatingItemCallback ClosingFloatingItemHandler => ClosingFloatingItemHandlerImpl;


        private static void ClosingFloatingItemHandlerImpl(ItemActionCallbackArgs<Layout> args)
        {
            var disposable = args.DragablzItem.DataContext as IDisposable;
            disposable?.Dispose();
        }
    }
}
