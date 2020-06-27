using System;
using System.Windows;

using static HellTakerAni.Properties.HTASetting;

namespace HellTakerAni
{
    /// <summary>
    /// ControlSpeedDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ControlSpeedDialog : Window
    {
        MainWindow parent;

        public ControlSpeedDialog(Window parent)
        {
            this.parent = parent as MainWindow;

            InitializeComponent();

            //HTAFrameSlider.Value = Default.FrameSpeedSeed;
            HTAFrameLabel.Content = $"Frame : {Default.FrameSpeedSeed}";
        }

        private void HTAFrameIntervalSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            HTAFrameLabel.Content = $"Frame : {e.NewValue}";
            parent.frameTimer.Interval = TimeSpan.FromSeconds(1 / e.NewValue);
            Default.FrameSpeedSeed = e.NewValue;

            Default.Save();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            double frame = 20;

            parent.frameTimer.Interval = TimeSpan.FromSeconds(1 / frame);
            //HTAFrameSlider.Value = Default.FrameSpeedSeed = frame;

            Default.Save();
        }
    }
}
