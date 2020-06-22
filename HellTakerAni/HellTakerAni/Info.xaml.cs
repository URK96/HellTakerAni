using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            var uri = (sender as Hyperlink).NavigateUri;

            var psi = new ProcessStartInfo
            {
                FileName = uri.ToString(),
                UseShellExecute = true
            };
            Process.Start(psi);

            e.Handled = true;
        }
    }
}
