using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using BirthdayIsland.Models;
using BirthdayIsland.Services;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;

namespace BirthdayIsland.Views.Components;

[ComponentInfo(
    "6F3C2B91-6A41-4B7E-9E2C-2C9C4B0B1A77", // 组件唯一 GUID（如复制本项目请自行重新生成）
    "今日生日",
    "\uE9B0",
    "在主界面显示今日过生日的人数，可选显示姓名。"
)]
public partial class BirthdayTodayComponent : ComponentBase<BirthdayComponentSettings>
{
    private readonly BirthdayDataService _dataService;

    public BirthdayTodayComponent(BirthdayDataService dataService)
    {
        InitializeComponent();
        _dataService = dataService;
        _dataService.DataChanged += OnDataChanged;

        // Settings 要到 Loaded 之后才可用；加载完成后再订阅设置变更并刷新一次。
        Loaded += OnComponentLoaded;
        Unloaded += OnComponentUnloaded;
    }

    private void OnComponentLoaded(object? sender, RoutedEventArgs e)
    {
        if (Settings != null)
            Settings.PropertyChanged += OnSettingsChanged;
        UpdateDisplay();
    }

    private void OnComponentUnloaded(object? sender, RoutedEventArgs e)
    {
        _dataService.DataChanged -= OnDataChanged;
        if (Settings != null)
            Settings.PropertyChanged -= OnSettingsChanged;
    }

    private void OnDataChanged(object? sender, EventArgs e) => UpdateDisplay();

    private void OnSettingsChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) => UpdateDisplay();

    private void UpdateDisplay()
    {
        // 数据变更可能来自非 UI 线程，统一切回 UI 线程刷新。
        Dispatcher.UIThread.Post(() =>
        {
            // 用 AvaloniaXamlLoader.Load 手动加载时，x:Name 字段不会被源生成器自动赋值，
            // 必须用 FindControl 在名称作用域里查控件，否则字段为 null 会空引用崩溃。
            var textBlock = this.FindControl<TextBlock>("DisplayTextBlock");
            if (textBlock == null)
                return;

            var today = _dataService.GetTodayBirthdays();
            int count = today.Count;
            bool showNames = Settings?.ShowNames ?? true;
            int maxNames = Settings?.MaxNameCount ?? 3;

            if (count == 0)
                textBlock.Text = "🎂 今日生日：0 人";
            else if (showNames && count <= maxNames)
                textBlock.Text = $"🎂 今日生日：{string.Join("、", today.Select(p => p.Name))}";
            else
                textBlock.Text = $"🎂 今日生日：{count} 人";
        });
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}