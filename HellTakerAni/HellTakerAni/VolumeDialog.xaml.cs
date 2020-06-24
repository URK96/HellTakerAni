﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using static HellTakerAni.ETC;
using static HellTakerAni.Properties.HTASetting;

namespace HellTakerAni
{
    /// <summary>
    /// VolumeDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class VolumeDialog : Window
    {
        public VolumeDialog()
        {
            InitializeComponent();

            HTAVolumeLevelSlider.Maximum = Default.ApplyExtendVolume ? 200 : 100;
            HTAVolumeLevelSlider.Minimum = 0;
            HTAVolumeLevelSlider.Value = Default.Volume;

            HTAVolumeExtendOption.IsChecked = Default.ApplyExtendVolume;
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
    }
}
