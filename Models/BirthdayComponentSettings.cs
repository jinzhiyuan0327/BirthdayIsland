using CommunityToolkit.Mvvm.ComponentModel;

namespace BirthdayIsland.Models;

/// <summary>
/// 单个主界面组件实例的设置。ClassIsland 会为每个摆放的组件独立保存此设置。
/// </summary>
public partial class BirthdayComponentSettings : ObservableObject
{
    /// <summary>是否在组件中显示生日者姓名。</summary>
    [ObservableProperty]
    private bool _showNames = true;

    /// <summary>最多显示多少个姓名，超过则只显示人数。</summary>
    [ObservableProperty]
    private int _maxNameCount = 3;
}