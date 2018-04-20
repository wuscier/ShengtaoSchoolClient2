using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace St.Common.Dragable
{
    public class CustomHeaderViewModel:BindableBase
    {
        private string _header;
        private bool _isSelected;

        public string Header
        {
            get { return _header; }
            set { SetProperty(ref _header, value); }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }
    }
}
