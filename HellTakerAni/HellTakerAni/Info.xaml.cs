using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

namespace HellTakerAni
{
    /// <summary>
    /// Info.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Info : Window
    {
        public Info()
        {
            InitializeComponent();

            InfoAppVersionLabel.Content = ETC.appVersion;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            var psi = new ProcessStartInfo
            {
                FileName = (sender as Hyperlink).NavigateUri.ToString(),
                UseShellExecute = true
            };

            Process.Start(psi);

            e.Handled = true;
        }
    }
}
