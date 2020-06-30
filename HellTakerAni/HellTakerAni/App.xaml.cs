using IWshRuntimeLibrary;

using System;
using System.Drawing;
using System.IO;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;

using static HellTakerAni.ETC;
using static HellTakerAni.Properties.HTASetting;

namespace HellTakerAni
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public App()
        {
            musicPlayer = new System.Windows.Media.MediaPlayer();
            musicPlayer.MediaEnded += delegate
            {
                if (isRepeat)
                {
                    musicPlayer.Position = TimeSpan.Zero;
                    musicPlayer.Play();
                }
                else
                {
                    mState = MusicState.Stop;
                }
            };

            frameTimer = new DispatcherTimer();

            frameTimer.Interval = TimeSpan.FromSeconds((1 / Default.FrameSpeedSeed));
            frameTimer.Tick += delegate { mainFrameCount = (mainFrameCount + 1) % 24; };
            frameTimer.Start();

            CreateTray();
        }

        void CreateTray()
        {
            // Program Item

            var exitItem = new ToolStripMenuItem()
            {
                Text = "Exit"
            };
            exitItem.Click += delegate { Current.Shutdown(); };

            var infoItem = new ToolStripMenuItem()
            {
                Text = "Info"
            };
            infoItem.Click += delegate
            {
                var infoWindow = new Info();
                infoWindow.Show();
            };

            /*var tipItem = new ToolStripMenuItem()
            {
                Text = "Tip"
            };
            tipItem.Click += delegate
            {
                System.Windows.MessageBox.Show(Properties.Resources.TipMessage, "HellTakerAni Tip", MessageBoxButton.OK);
            };*/


            // Util Item

            string appName = AppDomain.CurrentDomain.FriendlyName;
            string shortcutFile = Path.Combine(startupPath, $"{appName}.lnk");

            var toggleStartUp = new ToolStripMenuItem()
            {
                Text = "Run At Startup"
            };
            toggleStartUp.Click += (sender, e) =>
            {
                var item = sender as ToolStripMenuItem;

                item.Checked = !item.Checked;

                if (!Directory.Exists(startupPath))
                {
                    Directory.CreateDirectory(startupPath);
                }

                if (System.IO.File.Exists(shortcutFile))
                {
                    System.IO.File.Delete(shortcutFile);
                }

                if (item.Checked)
                {
                    var shell = new WshShell();
                    var shortcut = shell.CreateShortcut(shortcutFile) as IWshShortcut;

                    shortcut.WorkingDirectory = Environment.CurrentDirectory;
                    shortcut.IconLocation = shortcut.TargetPath = Path.Combine(Environment.CurrentDirectory, $"{appName}.exe");

                    shortcut.Save();
                }
            };
            toggleStartUp.Checked = System.IO.File.Exists(shortcutFile);

            var toggleMusicRepeat = new ToolStripMenuItem()
            {
                Text = "Repeat Music"
            };
            toggleMusicRepeat.Click += (sender, e) =>
            {
                var item = sender as ToolStripMenuItem;

                item.Checked = !item.Checked;
                isRepeat = item.Checked;
            };
            toggleMusicRepeat.Checked = isRepeat;


            // Display & Audio Item

            var selectNewCharacter = new ToolStripMenuItem()
            {
                Text = "Character"
            };

            for (int i = 0; i < imageList.Length; ++i)
            {
                var item = new ToolStripMenuItem()
                {
                    Text = imageList[i],
                    Tag = i
                };
                item.Click += (sender, e) =>
                {
                    var item = sender as ToolStripMenuItem;

                    var mWindow = new MainWindow((int)item.Tag);
                    MainWindow.Show();
                };

                selectNewCharacter.DropDownItems.Add(item);
            }

            selectNewCharacter.DropDownItems[Default.CharacterIndex].PerformClick();

            var selectMusic = new ToolStripMenuItem()
            {
                Text = "Music"
            };

            foreach (string s in musicList)
            {
                var item = new ToolStripMenuItem()
                {
                    Text = s
                };
                item.Click += (sender, e) =>
                {
                    var item = sender as ToolStripMenuItem;
                    musicPath = $"Resources/{item.Text}.mp3";

                    musicPlayer.Open(new Uri(musicPath, UriKind.Relative));
                    musicPlayer.Play();
                    mState = MusicState.Play;
                };

                selectMusic.DropDownItems.Add(item);
            }

            var selectOption = new ToolStripMenuItem()
            {
                Text = "Options"
            };
            selectOption.Click += delegate
            {
                var oDialog = new OptionDialog();
                oDialog.Show();
            };


            // Add menu items

            var menuStrip = new ContextMenuStrip();

            menuStrip.Items.Add(selectNewCharacter);
            menuStrip.Items.Add(selectMusic);
            menuStrip.Items.Add(new ToolStripSeparator());
            menuStrip.Items.Add(toggleStartUp);
            menuStrip.Items.Add(toggleMusicRepeat);
            menuStrip.Items.Add(selectOption);
            menuStrip.Items.Add(new ToolStripSeparator());
            menuStrip.Items.Add(infoItem);
            menuStrip.Items.Add(exitItem);


            // Create tray icon

            var tray = new NotifyIcon()
            {
                Icon = new Icon("Resources/logo.ico"),
                Visible = true,
                Text = "HellTakerAni",
                ContextMenuStrip = menuStrip
            };
        }

    }
}
