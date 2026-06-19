using System.Collections.Generic;

namespace BirthdayIsland.Models;

public class BirthdayImportResult
{
    public List<BirthdayPerson> People { get; set; } = new();

    public List<BirthdayImportError> Errors { get; set; } = new();

    public int SuccessCount => People.Count;

    public int ErrorCount => Errors.Count;

    /// <summary>整体是否失败（例如找不到表头列、文件无法打开）。</summary>
    public bool Fatal { get; set; }

    /// <summary>致命错误或概要提示信息。</summary>
    public string? Message { get; set; }
}

public class BirthdayImportError
{
    public int RowNumber { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string? RawName { get; set; }

    public string? RawBirthday { get; set; }
}