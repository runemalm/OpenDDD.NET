using System;

namespace DDD.Application.Error
{
    public class TransactionalException : ApplicationException
    {
        public static TransactionalException Failed(string reason)
            => new TransactionalException(ApplicationError.Transactional_Failed(reason));
        
        public static TransactionalException NotRegistered()
            => new TransactionalException(ApplicationError.Transactional_NotRegistered());
        
        public static TransactionalException NotRegistered(Exception inner)
            => new TransactionalException(ApplicationError.Transactional_NotRegistered(), inner);
        
        public TransactionalException(IApplicationError error) : base(error)
        {
            
        }
        
        public TransactionalException(IApplicationError error, Exception inner) : base(error, inner)
        {
            
        }
    }
}
