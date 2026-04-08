namespace R4PDF.Models;

public class AutoPaginationSettings
{
    public bool Enabled { get; set; }
    public bool RepeatHeaderOnContinuation { get; set; } = true;
    public bool RepeatFooterOnContinuation { get; set; } = true;
    public bool SplitParagraphs { get; set; } = true;
    public bool SplitTables { get; set; } = true;
}