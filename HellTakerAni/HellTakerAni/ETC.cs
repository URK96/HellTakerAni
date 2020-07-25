using System.Windows.Media;
using System.Windows.Threading;
using System;

namespace HellTakerAni
{
    internal static class ETC
    {
        internal enum MusicState { Stop, Play, Pause }

        internal static MusicState mState = MusicState.Stop;

        internal static readonly string appVersion = "v2.2";

        internal static readonly string startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);

        internal static MediaPlayer musicPlayer;
        internal static string musicPath = @"Resources/Vitality.mp3";
        internal static string[] musicList =
        {
            "Apropos",
            "Epitomize",
            "Luminescent",
            "Vitality",
            "Vitality_VIP",
            "Vitality_SayMaxWell_Remix",
            "Vitality_t+pazolite_Remix",
            "Vitality_Axez_Remix"
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
            "Bellzebub",
            "Cerberus",
            "Cerberus_Full",
            "Glorious_Left",
            "Glorious_Right",
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
        
        internal static bool isRepeat = true;
        internal static int mainFrameCount = -1;
    }
}
