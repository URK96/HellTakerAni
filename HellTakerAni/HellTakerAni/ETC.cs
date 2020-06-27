using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Threading;

namespace HellTakerAni
{
    internal static class ETC
    {
        internal enum MusicState { Stop, Play, Pause }

        internal static MusicState mState = MusicState.Stop;

        internal static readonly string appVersion = "v1.5";

        internal static readonly string startupPath = @"C:\Users\chlwl\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup";

        internal static MediaPlayer musicPlayer;
        internal static string musicPath = @"Resources/Vitality.mp3";
        internal static string[] musicList =
        {
            "Vitality",
            "Vitality_VIP",
            "Vitality_SayMaxWell_Remix"
        };

        internal static string[] sizeList =
        {
            "50x50",
            "75x75",
            "100x100",
            "125x125",
            "150x150"
        };

        internal static string[] imageList =
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

        internal static DispatcherTimer frameTimer;

        internal static int frameCount = -1;
        internal static bool isRepeat = true;
        internal static bool isFirst = true;
    }
}
