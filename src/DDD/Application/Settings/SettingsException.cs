using System;
using System.Collections.Generic;
using DDD.Domain.Model.Error;

namespace DDD.Application.Settings
{
	public class SettingsException : DomainException
	{
		public SettingsException(IDomainError error) : base(error)
		{
			
		}

		public SettingsException(IEnumerable<IDomainError> errors) : base(errors)
		{
			
		}
		
		public SettingsException(string message) : base(message)
		{
            
		}
        
		public SettingsException(string message, Exception inner) : base(message, inner)
		{
            
		}
	}
}
