namespace Gateway.Models.Exceptions;

public class ServiceInvocationException : Exception
{
    public ServiceInvocationException(string message) 
        : base(message)
    {
    }
}