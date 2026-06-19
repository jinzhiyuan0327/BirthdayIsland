using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using BirthdayIsland.Models;
using BirthdayIsland.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BirthdayIsland.ViewModels;

public partial class BirthdaySettingsPageViewModel : ObservableObject
{
    private readonly BirthdayDataService _dataService;
    private readonly ExcelImportService _excelImportService;

    /// <summary>由设置页在加载后注入，用于弹出系统文件选择器。</summary>
    public Func<Task<IReadOnlyList<IStorageFile>>>? PickFilesAsync { get; set; }

    /// <summary>由设置页在加载后注入，用于弹出确认对话框。返回 true 表示确认。</summary>
    public Func<string, string, Task<bool>>? ConfirmAsync { get; set; }

    public ObservableCollection<BirthdayPerson> People => _dataService.People;

    public ObservableCollection<BirthdayPerson> TodayBirthdays { get; } = new();

    public int TotalCount => People.Count;

    public int TodayBirthdayCount => TodayBirthdays.Count;

    public string? LastImportFileName => _dataService.LastImportFileName;

    public string LastImportTimeText =>
        _dataService.LastImportTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "（尚未导入）";

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _importErrors = new();

    public BirthdaySettingsPageViewModel(
        BirthdayDataService dataService,
        ExcelImportService excelImportService)
    {
        _dataService = dataService;
        _excelImportService = excelImportService;

        _dataService.DataChanged += (_, _) => Refresh();
        Refresh();
    }

    [RelayCommand]
    private async Task ImportExcelAsync()
    {
        if (PickFilesAsync == null)
        {
            StatusMessage = "无法打开文件选择器。";
            return;
        }

        var files = await PickFilesAsync();
        var file = files?.FirstOrDefault();
        if (file == null)
            return; // 用户取消

        string path = file.TryGetLocalPath() ?? file.Path.LocalPath;

        BirthdayImportResult result;
        try
        {
            result = _excelImportService.Import(path);
        }
        catch (Exception ex)
        {
            StatusMessage = $"导入失败：{ex.Message}";
            return;
        }

        ImportErrors = new ObservableCollection<string>(
            result.Errors.Select(e => $"第 {e.RowNumber} 行：{e.Reason}"));

        if (result.Fatal)
        {
            StatusMessage = result.Message ?? "导入失败。";
            return;
        }

        // 覆盖导入并保存
        _dataService.AddOrReplacePeople(result.People, file.Name);
        StatusMessage = result.Message ?? $"导入完成：成功 {result.SuccessCount} 条，失败 {result.ErrorCount} 条。";
    }

    [RelayCommand]
    private async Task ClearAsync()
    {
        bool confirmed = ConfirmAsync == null
            ? true
            : await ConfirmAsync("清空数据", "确定要清空全部生日数据吗？此操作不可撤销。");

        if (!confirmed)
            return;

        _dataService.Clear();
        ImportErrors = new ObservableCollection<string>();
        StatusMessage = "已清空全部数据。";
    }

    [RelayCommand]
    private void DeletePerson(BirthdayPerson? person)
    {
        if (person == null)
            return;

        _dataService.RemovePerson(person);
        StatusMessage = $"已删除：{person.Name}";
    }

    [RelayCommand]
    private void Reload()
    {
        _dataService.Load();
        StatusMessage = "已从本地文件重新读取数据。";
    }

    private void Refresh()
    {
        TodayBirthdays.Clear();
        foreach (var person in _dataService.GetTodayBirthdays())
            TodayBirthdays.Add(person);

        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(TodayBirthdayCount));
        OnPropertyChanged(nameof(LastImportFileName));
        OnPropertyChanged(nameof(LastImportTimeText));
    }
}