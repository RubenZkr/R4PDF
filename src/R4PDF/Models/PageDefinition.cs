namespace R4PDF.Models;

public class PageDefinition
{
    public PageSettings? Settings { get; set; }
    public SectionDefinition? Header { get; set; }
    public SectionDefinition Body { get; set; } = new();
    public SectionDefinition? Footer { get; set; }
}