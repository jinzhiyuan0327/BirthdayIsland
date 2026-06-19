using Avalonia.Markup.Xaml;
using BirthdayIsland.Models;
using ClassIsland.Core.Abstractions.Controls;

namespace BirthdayIsland.Views.Components;

// 组件设置控件不需要 [ComponentInfo] 注册信息。
public partial class BirthdayTodayComponentSettingsControl : ComponentBase<BirthdayComponentSettings>
{
    public BirthdayTodayComponentSettingsControl()
    {
        InitializeComponent();

        // Settings 由 ClassIsland 在控件加载后注入；将其设为 DataContext，
        // 使 XAML 中的 {Binding ShowNames} / {Binding MaxNameCount} 能正确解析。
        Loaded += (_, _) => DataContext = Settings;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}