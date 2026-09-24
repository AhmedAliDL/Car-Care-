namespace App.Domain.Exceptions;

public class DomainException : Exception
{
    public string Title { get; }

    public DomainException(string title, string message) : base(message)
    {
        Title = title;
    }
}
