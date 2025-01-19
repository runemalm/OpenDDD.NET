namespace OpenDDD.Domain.Model.Exception
{
    public class ValidationException : System.Exception
    {
        public ValidationException() 
        { 
        }

        public ValidationException(string message) 
            : base(message) 
        { 
        }

        public ValidationException(string message, System.Exception innerException) 
            : base(message, innerException) 
        { 
        }
    }
}
