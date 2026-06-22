using xdPlayer.Application.Interfaces;
using xdPlayer.Presentation.ViewModels;
using xdPlayer.Infrastructure.Services;

namespace xdPlayer.Presentation.Design;

// reason why this exists is because it is required to rig avalonia previewer and be able to design
// the app inside of presentation project.
public static class DesignViewModelFactory
{
    public static PlayerViewModel CreatePlayer()
    {
        return new PlayerViewModel(
            new FakePlaybackManager(),
            new FakeAudioPlayerService()
        );
    }
}