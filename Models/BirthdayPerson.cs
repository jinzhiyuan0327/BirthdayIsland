using System;

namespace BirthdayIsland.Models;

public class BirthdayPerson
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Excel 中读取到的原始生日文本，便于排查解析问题。</summary>
    public string RawBirthday { get; set; } = string.Empty;

    /// <summary>
    /// 解析后的生日日期。若 Excel 中只有月日，则使用默认年份 2000。
    /// 闰日（2/29）会原样保留为 2000-02-29（2000 为闰年）。
    /// </summary>
    public DateTime Birthday { get; set; }

    public string? ClassName { get; set; }

    public string? Remark { get; set; }
}