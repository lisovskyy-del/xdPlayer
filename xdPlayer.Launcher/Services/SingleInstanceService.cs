using System;
using System.Collections.Generic;
using System.Text;

namespace xdPlayer.Launcher.Services;

public static class SingleInstanceService
{
    private static Mutex? _mutex;

    public static bool TryAcquire()
    {
        _mutex = new Mutex(
            initiallyOwned: true,
            name: @"Local\xdPlayerLauncher",
            createdNew: out bool createdNew);

        return createdNew;
    }
}