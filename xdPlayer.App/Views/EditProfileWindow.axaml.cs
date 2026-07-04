using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using System;

namespace xdPlayer.App.Views;

public partial class EditProfileWindow : Window
{
    public string? SelectedAvatarPath { get; private set; }
    public string? ResultDisplayName { get; private set; }
    public bool Confirmed { get; private set; }

    public EditProfileWindow()
    {
        InitializeComponent();
    }

    public EditProfileWindow(string currentName, string? currentAvatarPath) : this()
    {
        NameTextBox.Text = currentName;

        if (!string.IsNullOrWhiteSpace(currentAvatarPath) && System.IO.File.Exists(currentAvatarPath))
        {
            try { AvatarPreview.Source = new Bitmap(currentAvatarPath); }
            catch { }
        }
    }

    private async void OnAvatarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choose avatar",
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

        SelectedAvatarPath = files[0].Path.LocalPath;
        AvatarPreview.Source = new Bitmap(SelectedAvatarPath);
    }

    private void OnSaveClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ResultDisplayName = NameTextBox.Text?.Trim();
        Confirmed = true;
        Close();
    }

    private void OnCancelClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Confirmed = false;
        Close();
    }
}