namespace LegalDocumentComparator.Domain.ValueObjects;

public class TextPosition
{
    public int PageNumber { get; private set; }
    public double X { get; private set; }
    public double Y { get; private set; }
    public double Width { get; private set; }
    public double Height { get; private set; }

    private TextPosition() { }

    public TextPosition(int pageNumber, double x, double y, double width, double height)
    {
        if (pageNumber <= 0)
            throw new ArgumentException("Page number must be positive", nameof(pageNumber));

        PageNumber = pageNumber;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not TextPosition other)
            return false;

        return PageNumber == other.PageNumber &&
               Math.Abs(X - other.X) < 0.01 &&
               Math.Abs(Y - other.Y) < 0.01;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PageNumber, X, Y);
    }
}
