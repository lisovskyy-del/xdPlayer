using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using xdPlayer.App.ViewModels;
using xdPlayer.Application.Interfaces;
using xdPlayer.Domain.Entities;

namespace xdPlayer.App.Views;

public partial class EditTrackWindow : Window
{
    public bool Confirmed { get; private set; }

    public EditTrackWindow()
    {
        InitializeComponent();
    }

    public EditTrackWindow(Track track) : this()
    {
        var libraryService = App.Services.GetRequiredService<ILibraryService>();
        var tagService = App.Services.GetRequiredService<ITagService>();

        var vm = new EditTrackViewModel(track, libraryService, tagService);

        vm.RequestClose += () =>
        {
            Confirmed = vm.Confirmed;
            Close();
        };

        vm.RequestChangeCover += async () => await OnChangeCoverAsync(track.Id, vm);

        DataContext = vm;
    }

    private async Task OnChangeCoverAsync(int trackId, EditTrackViewModel vm)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choose cover image",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Images")
                {
                    Patterns = ["*.png", "*.jpg", "*.jpeg", "*.webp"]
                }
            ]
        });

        if (files.Count == 0) return;

        var libraryService = App.Services.GetRequiredService<ILibraryService>();
        var updated = await libraryService.SetTrackCoverAsync(trackId, files[0].Path.LocalPath);

        vm.UpdateCoverPath(updated.CoverImagePath);
    }

    private void OnNewTagKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        if (DataContext is not EditTrackViewModel vm) return;

        vm.AddTagCommand.Execute().Subscribe();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is EditTrackViewModel vm)
            vm.CancelCommand.Execute().Subscribe();
        else
            Close();
    }
}