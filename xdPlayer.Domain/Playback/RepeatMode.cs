using System;
using System.Collections.Generic;
using System.Text;

namespace xdPlayer.Domain.Playback;

public enum RepeatMode
{
    None, // stops the player after all tracks finished
    One, // only repeats one track
    All // if tracks finish, start over
}