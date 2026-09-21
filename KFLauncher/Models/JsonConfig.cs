using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace KFLauncher.Models
{
    /// <summary>A saved server.  The query port is rarely the game port, so both are kept.</summary>
    public record Favorite(string Query, ushort GamePort);

    public partial class JsonConfig : ObservableObject
    {
        [ObservableProperty]
        private string gamePath = string.Empty;

        [ObservableProperty]
        private bool disableCache = false;

        [ObservableProperty]
        private bool optimizePerformance = true;

        [ObservableProperty]
        private bool disableMusic = false;

        [ObservableProperty]
        private bool skipIntro = true;

        [ObservableProperty]
        private bool increaseCacheLimit = true;

        [ObservableProperty]
        private bool unlockFramerate = true;

        [ObservableProperty]
        private bool fixMouseInput = true;

        [ObservableProperty]
        private bool disableMovies = false;

        [ObservableProperty]
        private bool quickHeal = true;

        /// <summary>Wine/proton pointer grab, so the cursor cannot wander onto another monitor.</summary>
        [ObservableProperty]
        private bool lockMouse = true;

        [ObservableProperty]
        private bool setResolution = false;

        /// <summary>KF ships 85, which is cropped rather than widened on a widescreen monitor.</summary>
        [ObservableProperty]
        private bool setFov = false;

        [ObservableProperty]
        private string fov = "95";

        [ObservableProperty]
        private string resX = "1920";

        [ObservableProperty]
        private string resY = "1080";

        [ObservableProperty]
        private bool firstLaunch = true;

        /// <summary>0 leave open, 1 minimize, 2 close.  Minimizing stops avalonia painting the
        /// window, and tiling wms like bspwm keep it on screen anyway, so it looks frozen there.</summary>
        [ObservableProperty]
        private int afterLaunch = 0;

        /// <summary>A host serving a cached copy of the list, so no api key is needed.</summary>
        [ObservableProperty]
        private string serverListUrl = ServerBrowser.DefaultListUrl;

        /// <summary>From https://steamcommunity.com/dev/apikey.  Only used without a list url.</summary>
        [ObservableProperty]
        private string steamApiKey = string.Empty;

        /// <summary>Saved servers.  Replaced wholesale when it changes, which is what saves it.</summary>
        [ObservableProperty]
        private List<Favorite> favorites = [];
    }
}
