
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using static HellTakerAni.ETC;
using static HellTakerAni.Properties.HTASetting;

namespace HellTakerAni
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Bitmap original;
        List<Bitmap> frames = new List<Bitmap>(12);
        List<ImageSource> imgFrame = new List<ImageSource>(12);
        System.Windows.Controls.Image[] aniBoxes;

        int frame = 0;
        int frameCount = -1;
        int aniBoxCount = 1;
        int characterIndex = 0;

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public MainWindow(int startCharacterIndex)
        {
            SetStartPositionSize();

            InitializeComponent();
            SetMouseEvent();
            InitContextMenuItems();

            Topmost = true;

            aniBoxes = new System.Windows.Controls.Image[]
            {
                aniBox,
                aniBox1,
                aniBox2
            };

            (MainContextMenu_Character.Items[startCharacterIndex] as MenuItem).RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            frameTimer.Tick += ChangeNextFrame;
        }

        private void SetMouseEvent()
        {
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
        }

        private void InitContextMenuItems()
        {
            // Character Menu

            for (int i = 0; i < imageList.Length; ++i)
            {
                var characterItem = new MenuItem()
                {
                    Header = imageList[i],
                    Tag = i
                };

                if (i == 2)
                {
                    characterItem.Click += (sender, e) =>
                    {
                        var item = sender as MenuItem;
                        var id = (int)item.Tag;

                        Default.CharacterIndex = id;

                        Default.Save();

                        CreateAnimationList($"{item.Header}"[..8]);
                        SetAniBoxes(3);
                        ChangeCheckCharacterSelect(id);
                    };
                }
                else
                {
                    characterItem.Click += (sender, e) =>
                    {
                        var item = sender as MenuItem;
                        var id = (int)item.Tag;

                        Default.CharacterIndex = id;

                        Default.Save();

                        CreateAnimationList(item.Header.ToString());
                        SetAniBoxes(1);
                        ChangeCheckCharacterSelect(id);
                    };
                }

                MainContextMenu_Character.Items.Add(characterItem);
            }


            // Size Menu

            for (int i = 0; i < sizeList.Length; ++i)
            {
                var item = new MenuItem()
                {
                    Header = sizeList[i],
                    Tag = i
                };
                item.Click += (sender, e) =>
                {
                    var item = sender as MenuItem;
                    int size = 50 + 25 * (int)item.Tag;

                    Width = size * aniBoxCount;
                    Height = size;
                };

                MainContextMenu_Size.Items.Add(item);
            }


            // Toggle Topmost Menu

            MainContextMenu_ToggleTopmost.Click += (sender, e) =>
            {
                (sender as MenuItem).IsChecked = Topmost = !Topmost;
            };


            // Remove Menu

            MainContextMenu_Remove.Click += delegate
            {
                frameTimer.Tick -= ChangeNextFrame;
                Close();
                GC.Collect();
            };
        }

        private void ChangeCheckCharacterSelect(int index)
        {
            characterIndex = index;

            foreach (MenuItem menuItem in MainContextMenu_Character.Items)
            {
                menuItem.IsChecked = (int)menuItem.Tag == index;
            }
        }

        private void CreateAnimationList(string fileName)
        {
            string bitmapPath = @$"Resources/{fileName}.png";

            original = System.Drawing.Image.FromFile(bitmapPath) as Bitmap;

            frame = original.Width / 100;

            frames.Clear();
            imgFrame.Clear();

            for (int i = 0; i < frame; ++i)
            {
                frames.Add(new Bitmap(100, 100));

                using (var g = Graphics.FromImage(frames[i]))
                {
                    g.DrawImage(original, new Rectangle(0, 0, 100, 100), new Rectangle(100 * i, 0, 100, 100), GraphicsUnit.Pixel);
                }

                var handle = frames[i].GetHbitmap();

                try
                {
                    imgFrame.Add(Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
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
            frameCount = (characterIndex == 3) || (characterIndex == 4) ? mainFrameCount : mainFrameCount % 12;

            for (int i = 0; i < aniBoxCount; ++i)
            {
                aniBoxes[i].Source = imgFrame[frameCount];
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
