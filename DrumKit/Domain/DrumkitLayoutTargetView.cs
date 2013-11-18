using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrumKit
{
    [Flags]
    public enum DrumkitLayoutTargetView
    {
        None = 0,
        Snapped = 1,
        Filled = 2,
        Landscape = 4,
        Portrait = 8,
        All = Snapped | Filled | Landscape | Portrait
    }
}
