using System;
using System.Collections.Generic;
using System.Text;

namespace xdPlayer.Domain.Playback;

public enum PlaybackMode
{
    Normal, // normal audio play, tracks play one after another
    Shuffle // shuffles every track and plays it
}