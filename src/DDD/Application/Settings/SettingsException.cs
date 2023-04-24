using System;
using DDD.Application.Error;
using ApplicationException = DDD.Application.Error.ApplicationException;

namespace DDD.Application.Settings
{
    public class SettingsException : ApplicationException
    {
        public static SettingsException Invalid(string spec)
            => new SettingsException(ApplicationError.Settings_InvalidSettings(spec));
        
        public static SettingsException Invalid(string spec, Exception inner)
            => new SettingsException(ApplicationError.Settings_InvalidSettings(spec), inner);
        
        public static SettingsException AuthEnabledButNoPrivateKey()
            => new SettingsException(ApplicationError.Settings_AuthEnabledButNoPrivateKey());

        public SettingsException(IApplicationError error) : base(error)
        {
            
        }
        
        public SettingsException(IApplicationError error, Exception inner) : base(error, inner)
        {
            
        }
    }
}
