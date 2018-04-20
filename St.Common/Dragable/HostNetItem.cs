using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dragablz;

namespace St.Common.Dragable
{
    public static class HostNewItem
    {
        public static Func<HeaderedItemViewModel> Factory
        {
            get
            {
                return
                    () =>
                    {
                        var dateTime = DateTime.Now;

                        return new HeaderedItemViewModel()
                        {
                            Header = dateTime.ToLongTimeString(),
                            Content = dateTime.ToString("R")
                        };
                    };
            }
        }
    }
}
