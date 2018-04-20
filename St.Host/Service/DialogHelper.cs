using St.Common;
using St.Common.Contract;

namespace St.Host
{
    public class DialogHelper : IDialogHelper
    {
        public bool Show(string msg)
        {
            SscDialog sscDialog = new SscDialog(msg);
            bool? value = sscDialog.ShowDialog();

            return value.HasValue && value.Value;
        }
    }
}
