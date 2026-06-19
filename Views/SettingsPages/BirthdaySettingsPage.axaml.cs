using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using BirthdayIsland.ViewModels;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Enums.SettingsWindow;
using FluentAvalonia.UI.Controls;

namespace BirthdayIsland.Views.SettingsPages;

[SettingsPageInfo(
    "birthdayisland.settings",
    "生日显示插件",
    "🎂", // 选中/未选中都用蛋糕 emoji；这两个参数是字符串、会被当作图标文字显示，传图标名会原样显示成字母
    "🎂",
    SettingsPageCategory.External
)]
public partial class BirthdaySettingsPage : SettingsPageBase
{
    private readonly BirthdaySettingsPageViewModel _viewModel;

    public BirthdaySettingsPage(BirthdaySettingsPageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        // 注入文件选择器与确认对话框（需要 TopLevel，故在加载后再注入）。
        _viewModel.PickFilesAsync = PickFilesAsync;
        _viewModel.ConfirmAsync = ConfirmAsync;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private async Task<IReadOnlyList<IStorageFile>> PickFilesAsync()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null)
            return new List<IStorageFile>();

        return await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "选择生日 Excel 文件",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Excel 工作簿")
                {
                    Patterns = new[] { "*.xlsx", "*.xls" }
                }
            }
        });
    }

    private async Task<bool> ConfirmAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = "确定",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Close
        };
        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }
}