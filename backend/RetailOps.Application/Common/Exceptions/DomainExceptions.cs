namespace RetailOps.Application.Common.Exceptions;

public class InsufficientStockException : Exception
{
    public InsufficientStockException(string message) : base(message) { }
}

public class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(string message) : base(message) { }
}
