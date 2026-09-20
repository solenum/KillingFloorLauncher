using CommunityToolkit.Mvvm.ComponentModel;

namespace KFLauncher.Models
{
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

        /// <summary>From https://steamcommunity.com/dev/apikey, needed for the server list.</summary>
        [ObservableProperty]
        private string steamApiKey = string.Empty;
    }
}
