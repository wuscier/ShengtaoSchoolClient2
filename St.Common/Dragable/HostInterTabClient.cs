using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dragablz;

namespace St.Common.Dragable
{
    public class HostInterTabClient : IInterTabClient
    {
        public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            var view = new HostView();
            var viewModel = new HostViewModel();
            view.DataContext = viewModel;

            return new NewTabHost<Window>(view, view.InitialTabablzControl);
        }

        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            return TabEmptiedResponse.CloseWindowOrLayoutBranch;
        }
    }
}
