﻿using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

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

        MediaPlayer musicPlayer;

        string bitmapPath = "Resources/Cerberus.png";
        string musicPath = "Resources/Vitality.mp3";
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
        int frame = -1;
        bool isRepeat = true;

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public MainWindow()
        {
            InitializeComponent();

            CreateTray();

            var timer = new DispatcherTimer();

            timer.Interval = TimeSpan.FromSeconds(0.0167 * 3.7);
            timer.Tick += ChangeNextFrame;
            timer.Start();

            MouseDown += (sender, e) =>
            {
                switch (e.ChangedButton)
                {
                    case MouseButton.Left:
                        DragMove();
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

            var selectCharacter = new ToolStripMenuItem()
            {
                Text = "Character"
            };
            
            foreach (string s in imageList)
            {
                var item = new ToolStripMenuItem()
                {
                    Text = s
                };
                item.Click += (sender, e) =>
                {
                    var item = sender as ToolStripMenuItem;
                    bitmapPath = $"Resources/{item.Text}.png";

                    CreateAnimationList();
                };

                selectCharacter.DropDownItems.Add(item);
            }

            selectCharacter.DropDownItems[0].PerformClick();

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

            var menuStrip = new ContextMenuStrip();
            
            menuStrip.Items.Add(selectCharacter);
            menuStrip.Items.Add(selectMusic);
            menuStrip.Items.Add(new ToolStripSeparator());
            menuStrip.Items.Add(toggleAlwaysOnTop);
            menuStrip.Items.Add(toggleMusicRepeat);
            menuStrip.Items.Add(new ToolStripSeparator());
            menuStrip.Items.Add(infoItem);
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

        private void MoveAniBox(object sender, MouseButtonEventArgs e)
        {
            
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
    }
}