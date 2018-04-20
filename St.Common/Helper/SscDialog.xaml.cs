using System.Windows;

namespace St.Common
{
    /// <summary>
    /// UpdateConfirmView.xaml 的交互逻辑
    /// </summary>
    public partial class SscDialog
    {
        public SscDialog(string msg) : this()
        {
            TbMsg.Text = msg;
        }

        public SscDialog()
        {
            InitializeComponent();
        }

        private void BtnOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void SscDialog_OnLoaded(object sender, RoutedEventArgs e)
        {
            BtnOk.Focus();
        }
    }
}
