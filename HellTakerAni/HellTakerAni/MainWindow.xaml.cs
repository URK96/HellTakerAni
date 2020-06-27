using IWshRuntimeLibrary;

using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using static HellTakerAni.Properties.HTASetting;

namespace HellTakerAni
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        enum MusicState { Stop, Play, Pause }

        MusicState mState = MusicState.Stop;

        Bitmap original;
        Bitmap[] frames = new Bitmap[12];
        ImageSource[] imgFrame = new ImageSource[12];
        System.Windows.Controls.Image[] aniBoxes;

        internal MediaPlayer musicPlayer;

        System.Timers.Timer resizeTimer;
        internal DispatcherTimer frameTimer;

        readonly string startupPath = @"C:\Users\chlwl\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup";
        string bitmapPath = @"Resources/Cerberus.png";
        string musicPath = @"Resources/Vitality.mp3";
        string[] imageList =
        {
            "Azazel",
            "Cerberus",
            "Cerberus_Full",
            "Hero",
            "Hero_Cook",
            "Judgement",
            "Justice",
            "Lucifer",
            "Lucifer_Cook",
            "Malina",
            "Modeus",
            "Pandemonica",
            "Skeleton",
            "Zdrada"
        };
        string[] musicList =
        {
            "Vitality",
            "Vitality_VIP",
            "Vitality_SayMaxWell_Remix"
        };
        string[] sizeList =
        {
            "50x50",
            "75x75",
            "100x100",
            "125x125",
            "150x150"
        };
        int frame = -1;
        int aniBoxCount = 1;
        bool isRepeat = true;

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public MainWindow()
        {
            SetStartPositionSize();

            InitializeComponent();

            aniBoxes = new System.Windows.Controls.Image[]
            {
                aniBox,
                aniBox1,
                aniBox2
            };

            CreateTray();

            resizeTimer = new System.Timers.Timer(1000);
            resizeTimer.Elapsed += delegate { ResizeMode = ResizeMode.NoResize; };

            frameTimer = new DispatcherTimer();

            frameTimer.Interval = TimeSpan.FromSeconds((1 / Default.FrameSpeedSeed));
            frameTimer.Tick += ChangeNextFrame;
            frameTimer.Start();

            MouseDown += (sender, e) =>
            {
                switch (e.ChangedButton)
                {
                    case MouseButton.Left:
                        DragMove();

                        Default.StartPositionX = Left;
                        Default.StartPositionY = Top;

                        Default.Save();
                        break;
                    case MouseButton.Right:
                        musicPlayer.Stop();
                        mState = MusicState.Stop;
                        break;
                }
            };
            MouseDoubleClick += (sender, e) =>
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    switch (mState)
                    {
                        case MusicState.Play:
                            musicPlayer.Pause();
                            mState = MusicState.Pause;
                            break;
                        case MusicState.Pause:
                        case MusicState.Stop:
                            if (musicPlayer.Source != null)
                            {
                                musicPlayer.Play();
                                mState = MusicState.Play;
                            }
                            break;
                    }
                }
            };

            musicPlayer = new MediaPlayer();
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
        }

        void CreateTray()
        {
            // Program Item

            var exitItem = new ToolStripMenuItem()
            {
                Text = "Exit"
            };
            exitItem.Click += delegate { System.Windows.Application.Current.Shutdown(); };

            var infoItem = new ToolStripMenuItem()
            {
                Text = "Info"
            };
            infoItem.Click += delegate
            {
                var infoWindow = new Info();
                infoWindow.Show();
            };

            var tipItem = new ToolStripMenuItem()
            {
                Text = "Tip"
            };
            tipItem.Click += delegate
            {
                System.Windows.MessageBox.Show(Properties.Resources.TipMessage, "HellTakerAni Tip", MessageBoxButton.OK);
            };


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

                if (item.Checked)
                {
                    var shell = new WshShell();
                    var shortcut = shell.CreateShortcut(shortcutFile) as IWshShortcut;

                    shortcut.WorkingDirectory = Environment.CurrentDirectory;
                    shortcut.IconLocation = shortcut.TargetPath = Path.Combine(Environment.CurrentDirectory, $"{appName}.exe");

                    shortcut.Save();
                }
                else
                {
                    if (System.IO.File.Exists(shortcutFile))
                    {
                        System.IO.File.Delete(shortcutFile);
                    }
                }
            };
            toggleStartUp.Checked = System.IO.File.Exists(shortcutFile);

            var toggleAlwaysOnTop = new ToolStripMenuItem()
            {
                Text = "Always On Top"
            };
            toggleAlwaysOnTop.Click += (sender, e) =>
            {
                var item = sender as ToolStripMenuItem;

                item.Checked = !item.Checked;
                Topmost = item.Checked;
            };
            toggleAlwaysOnTop.Checked = true;

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

            var selectCharacter = new ToolStripMenuItem()
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

                // Cerberus_Full
                if (i == 2)
                {
                    item.Click += (sender, e) =>
                    {
                        var item = sender as ToolStripMenuItem;

                        bitmapPath = $"Resources/{item.Text[..8]}.png";
                        Default.CharacterIndex = (int)item.Tag;

                        Default.Save();

                        CreateAnimationList();
                        SetAniBoxes(3);

                        foreach (ToolStripMenuItem menu in selectCharacter.DropDownItems)
                        {
                            menu.Checked = false;
                        }

                        item.Checked = true;
                    };
                }
                else
                {
                    item.Click += (sender, e) =>
                    {
                        var item = sender as ToolStripMenuItem;

                        bitmapPath = $"Resources/{item.Text}.png";
                        Default.CharacterIndex = (int)item.Tag;

                        Default.Save();

                        CreateAnimationList();
                        SetAniBoxes(1);

                        foreach (ToolStripMenuItem menu in selectCharacter.DropDownItems)
                        {
                            menu.Checked = false;
                        }

                        item.Checked = true;
                    };
                }

                selectCharacter.DropDownItems.Add(item);
            }

            selectCharacter.DropDownItems[Default.CharacterIndex].PerformClick();

            var selectSize = new ToolStripMenuItem()
            {
                Text = "Preset Size"
            };

            for (int i = 0; i < sizeList.Length; ++i)
            {
                var item = new ToolStripMenuItem()
                {
                    Text = sizeList[i],
                    Tag = i
                };
                item.Click += (sender, e) =>
                {
                    var item = sender as ToolStripMenuItem;
                    int index = (int)item.Tag;
                    int size = 50 + 25 * index;

                    Width = Height = size;

                    Default.WindowWidth = Default.WindowHeight = size;

                    Default.Save();
                };

                selectSize.DropDownItems.Add(item);
            }

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

            var selectVolume = new ToolStripMenuItem()
            {
                Text = "Volume"
            };
            selectVolume.Click += delegate
            {
                var vDialog = new VolumeDialog(this);
                vDialog.Show();
            };

            var selectSpeed = new ToolStripMenuItem()
            {
                Text = "Speed"
            };
            selectSpeed.Click += delegate
            {
                var oDialog = new ControlSpeedDialog(this);
                oDialog.Show();
            };


            // Add menu items

            var menuStrip = new ContextMenuStrip();

            menuStrip.Items.Add(selectCharacter);
            menuStrip.Items.Add(selectSize);
            menuStrip.Items.Add(selectMusic);
            menuStrip.Items.Add(selectVolume);
            //menuStrip.Items.Add(selectSpeed);
            menuStrip.Items.Add(new ToolStripSeparator());
            menuStrip.Items.Add(toggleStartUp);
            menuStrip.Items.Add(toggleAlwaysOnTop);
            menuStrip.Items.Add(toggleMusicRepeat);
            menuStrip.Items.Add(new ToolStripSeparator());
            menuStrip.Items.Add(infoItem);
            menuStrip.Items.Add(tipItem);
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

        private void CreateAnimationList()
        {
            original = Image.FromFile(bitmapPath) as Bitmap;

            for (int i = 0; i < 12; ++i)
            {
                frames[i] = new Bitmap(100, 100);

                using (var g = Graphics.FromImage(frames[i]))
                {
                    g.DrawImage(original, new Rectangle(0, 0, 100, 100), new Rectangle(100 * i, 0, 100, 100), GraphicsUnit.Pixel);
                }

                var handle = frames[i].GetHbitmap();

                try
                {
                    imgFrame[i] = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
                finally
                {
                    DeleteObject(handle);
                }
            }
        }

        private void SetStartPositionSize()
        {
            double x = Default.StartPositionX;
            double y = Default.StartPositionY;

            if ((x == -1) || (y == -1))
            {
                Default.StartPositionX = Left;
                Default.StartPositionY = Top;

                Default.Save();
            }
            else
            {
                Left = x;
                Top = y;
            }

            Width = Default.WindowWidth;
            Height = Default.WindowHeight;
        }

        private void ChangeNextFrame(object sender, EventArgs e)
        {
            frame = (frame + 1) % 12;

            for (int i = 0; i < aniBoxCount; ++i)
            {
                aniBoxes[i].Source = imgFrame[frame];
            }
        }

        private void SetAniBoxes(int count)
        {
            aniBoxCount = count;
            HTAWindow.Width = HTAWindow.Height * count;

            foreach (System.Windows.Controls.Image control in aniBoxContainer.Children)
            {
                control.Visibility = Visibility.Hidden;
                control.Source = null;
            }

            for (int i = 0; i < count; ++i)
            {
                aniBoxes[i].Visibility = Visibility.Visible;
            }
        }


        #region Window styles

        [Flags]
        public enum ExtendedWindowStyles
        {
            WS_EX_TOOLWINDOW = 0x00000080,
        }

        public enum GetWindowLongFields
        {
            GWL_EXSTYLE = (-20),
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            int error = 0;
            IntPtr result = IntPtr.Zero;

            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                int tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if ((result == IntPtr.Zero) && (error != 0))
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern int IntSetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);

        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var wndHelper = new WindowInteropHelper(this);

            int exStyle = (int)GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);

            exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
        }

        private async void Window_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            await Task.Delay(100);

            ResizeMode = ResizeMode.CanResizeWithGrip;
        }

        private async void Window_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            await Task.Delay(2000);

            ResizeMode = ResizeMode.NoResize;
        }

        private void HTAWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Default.WindowWidth = e.NewSize.Width;
            Default.WindowHeight = e.NewSize.Height;

            Default.Save();
        }
    }
}
