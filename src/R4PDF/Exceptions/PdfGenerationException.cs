namespace R4PDF.Exceptions;

public class PdfGenerationException : Exception
{
    public PdfGenerationException(string message) : base(message)
    {
    }

    public PdfGenerationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}