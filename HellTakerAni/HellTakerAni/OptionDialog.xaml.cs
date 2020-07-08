using System;
using System.Windows;
using System.Windows.Controls;

using static HellTakerAni.Properties.HTASetting;
using static HellTakerAni.ETC;

namespace HellTakerAni
{
    /// <summary>
    /// VolumeDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OptionDialog : Window
    {
        public OptionDialog()
        {
            InitializeComponent();

            HTAVolumeLevelSlider.ValueChanged += HTAVolumeLevelSlider_ValueChanged;
            HTAVolumeLevelSlider.Maximum = Default.ApplyExtendVolume ? 200 : 100;
            HTAVolumeLevelSlider.Minimum = 0;
            HTAVolumeLevelSlider.Value = Default.Volume;
            HTAVolumeExtendOption.IsChecked = Default.ApplyExtendVolume;

            HTAFrameLevelSlider.ValueChanged += HTAFrameLevelSlider_ValueChanged;
            HTAFrameLevelSlider.Value = Default.FrameSpeedSeed;
            HTAFrameLevelLabel.Content = $"Frame Level : {Default.FrameSpeedSeed}";
        }

        private void HTAVolumeLevelSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var vSlider = sender as Slider;

            vSlider.ValueChanged -= HTAVolumeLevelSlider_ValueChanged;

            int level = Convert.ToInt32(e.NewValue);

            HTAVolumeLevelLabel.Content = $"Volume : {level}";
            vSlider.Value = Default.Volume = level;
            musicPlayer.Volume = level / 200.0;

            Default.Save();

            vSlider.ValueChanged += HTAVolumeLevelSlider_ValueChanged;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            HTAVolumeLevelSlider.Maximum = 200;
            Default.ApplyExtendVolume = true;

            Default.Save();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            HTAVolumeLevelSlider.Maximum = 100;
            HTAVolumeLevelSlider.Value = (Default.Volume > 100) ? 100 : Default.Volume;
            Default.ApplyExtendVolume = false;

            Default.Save();
        }

        private void HTAFrameLevelSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var fSlider = sender as Slider;

            fSlider.ValueChanged -= HTAFrameLevelSlider_ValueChanged;

            int level = Convert.ToInt32(e.NewValue);

            HTAFrameLevelLabel.Content = $"Frame Level : {level}";
            fSlider.Value = Default.FrameSpeedSeed = level;
            frameTimer.Interval = TimeSpan.FromSeconds((1.0 / level));

            Default.Save();

            fSlider.ValueChanged += HTAFrameLevelSlider_ValueChanged;
        }
    }
}
