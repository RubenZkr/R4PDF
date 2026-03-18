using R4PDF.Models;

namespace R4PDF.Fluent.Builders;

/// <summary>
///     Builder for PDF document metadata (title, author, etc.).
/// </summary>
public class MetadataBuilder
{
    internal readonly DocumentMetadata Metadata = new();

    public MetadataBuilder Title(string title)
    {
        Metadata.Title = title;
        return this;
    }

    public MetadataBuilder Author(string author)
    {
        Metadata.Author = author;
        return this;
    }

    public MetadataBuilder Subject(string subject)
    {
        Metadata.Subject = subject;
        return this;
    }

    public MetadataBuilder Keywords(string keywords)
    {
        Metadata.Keywords = keywords;
        return this;
    }
}