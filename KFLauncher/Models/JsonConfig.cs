using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KFLauncher.Models
{
    public class JsonConfig
    {
        // TODO: config for other options and patches
        private string _gamePath = String.Empty;
        [Reactive]
        public string GamePath
        {
            get
            {
                return this._gamePath;
            }
            set
            {
                this._gamePath = value;
                this._gamePath = this._gamePath.EndsWith(@"\\") ? this._gamePath.Substring(0, this._gamePath.Length - 2) : this._gamePath;
            }
        }
        
        [Reactive]
        public bool DisableCache { get; set; } = false;
        
        [Reactive]
        public bool OptimizePerformance { get; set; } = true;
        
        [Reactive]
        public bool DisableMusic { get; set; } = false;

        [Reactive]
        public bool SkipIntro { get; set; } = true;

        [Reactive]
        public bool IncreaseCacheLimit { get; set; } = true;
        
        [Reactive]
        public bool UnlockFramerate { get; set; } = true;

        [Reactive]
        public bool FixMouseInput { get; set; } = true;

        [Reactive]
        public bool DisableMovies { get; set; } = false;

        [Reactive]
        public bool QuickHeal { get; set; } = true;
    }
}
