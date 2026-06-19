using System;
using System.Linq;
using BirthdayIsland.Models;
using BirthdayIsland.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BirthdayIsland.ViewModels;

public partial class BirthdayTodayComponentViewModel : ObservableObject
{
    private readonly BirthdayDataService _dataService;
    private BirthdayComponentSettings? _settings;

    [ObservableProperty]
    private string _displayText = "🎂 今日生日：0 人";

    public BirthdayTodayComponentViewModel(BirthdayDataService dataService)
    {
        _dataService = dataService;
        _dataService.DataChanged += OnDataChanged;
        Refresh();
    }

    /// <summary>由组件在加载完成后传入组件级设置。</summary>
    public void ApplySettings(BirthdayComponentSettings? settings)
    {
        if (_settings != null)
            _settings.PropertyChanged -= OnSettingsChanged;

        _settings = settings;

        if (_settings != null)
            _settings.PropertyChanged += OnSettingsChanged;

        Refresh();
    }

    private void OnSettingsChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) => Refresh();

    private void OnDataChanged(object? sender, EventArgs e) => Refresh();

    public void Refresh()
    {
        var today = _dataService.GetTodayBirthdays();
        int count = today.Count;

        bool showNames = _settings?.ShowNames ?? true;
        int maxNames = _settings?.MaxNameCount ?? 3;

        if (count == 0)
        {
            DisplayText = "🎂 今日生日：0 人";
            return;
        }

        if (showNames && count <= maxNames)
        {
            string names = string.Join("、", today.Select(p => p.Name));
            DisplayText = $"🎂 今日生日：{names}";
        }
        else
        {
            DisplayText = $"🎂 今日生日：{count} 人";
        }
    }
}