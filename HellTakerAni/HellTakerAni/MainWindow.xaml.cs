using IWshRuntimeLibrary;

using Microsoft.Win32;

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

using static HellTakerAni.ETC;

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

        System.Timers.Timer resizeTimer;

        readonly string startupPath = @"C:\Users\chlwl\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup";
        string bitmapPath = @"Resources/Cerberus.png";
        string musicPath = @"Resources/Vitality.mp3";
        string[] imageList =
        {
            "Azazel",
            "Cerberus",
            "Judgement",
            "Justice",
            "Lucifer",
            "Lucifer_Cook",
            "Malina",
            "Modeus",
            "Pandemonica",
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
        bool isRepeat = true;

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public MainWindow()
        {
            SetStartPositionSize();

            InitializeComponent();

            CreateTray();

            resizeTimer = new System.Timers.Timer(1000);
            resizeTimer.Elapsed += delegate { ResizeMode = ResizeMode.NoResize; };

            var timer = new DispatcherTimer();

            timer.Interval = TimeSpan.FromSeconds((1 / 60.0) * 3);
            timer.Tick += ChangeNextFrame;
            timer.Start();

            MouseDown += (sender, e) =>
            {
                switch (e.ChangedButton)
                {
                    case MouseButton.Left:
                        DragMove();

                        Properties.HTASetting.Default.StartPositionX = Left;
                        Properties.HTASetting.Default.StartPositionY = Top;

                        Properties.HTASetting.Default.Save();
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
                item.Click += (sender, e) =>
                {
                    var item = sender as ToolStripMenuItem;

                    bitmapPath = $"Resources/{item.Text}.png";
                    Properties.HTASetting.Default.CharacterIndex = (int)item.Tag;

                    Properties.HTASetting.Default.Save();

                    CreateAnimationList();

                    foreach (ToolStripMenuItem menu in selectCharacter.DropDownItems)
                    {
                        menu.Checked = false;
                    }

                    item.Checked = true;
                };

                selectCharacter.DropDownItems.Add(item);
            }

            selectCharacter.DropDownItems[Properties.HTASetting.Default.CharacterIndex].PerformClick();

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

                    Properties.HTASetting.Default.WindowWidth = Properties.HTASetting.Default.WindowHeight = size;

                    Properties.HTASetting.Default.Save();
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
                var vDialog = new VolumeDialog();
                vDialog.Show();
            };


            // Add menu items

            var menuStrip = new ContextMenuStrip();
            
            menuStrip.Items.Add(selectCharacter);
            menuStrip.Items.Add(selectSize);
            menuStrip.Items.Add(selectMusic);
            menuStrip.Items.Add(selectVolume);
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
                Icon = new System.Drawing.Icon("Resources/logo.ico"),
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
                    g.DrawImage(original, new System.Drawing.Rectangle(0, 0, 100, 100), new System.Drawing.Rectangle(100 * i, 0, 100, 100), GraphicsUnit.Pixel);
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
            double x = Properties.HTASetting.Default.StartPositionX;
            double y = Properties.HTASetting.Default.StartPositionY;

            if ((x == -1) || (y == -1))
            {
                Properties.HTASetting.Default.StartPositionX = Left;
                Properties.HTASetting.Default.StartPositionY = Top;

                Properties.HTASetting.Default.Save();
            }
            else
            {
                Left = x;
                Top = y;
            }

            Width = Properties.HTASetting.Default.WindowWidth;
            Height = Properties.HTASetting.Default.WindowHeight;
        }

        private void ChangeNextFrame(object sender, EventArgs e)
        {
            frame = (frame + 1) % 12;
            aniBox.Source = imgFrame[frame]; 
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
            Properties.HTASetting.Default.WindowWidth = e.NewSize.Width;
            Properties.HTASetting.Default.WindowHeight = e.NewSize.Height;

            Properties.HTASetting.Default.Save();
        }
    }
}
