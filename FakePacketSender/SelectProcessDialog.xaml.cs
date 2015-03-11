using System.Windows;

namespace FakePacketSender
{
    public partial class SelectProcessDialog : Window
    {
        public SelectProcessDialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
