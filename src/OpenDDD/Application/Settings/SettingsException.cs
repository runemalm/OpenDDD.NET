using System;
using OpenDDD.Application.Error;
using ApplicationException = OpenDDD.Application.Error.ApplicationException;

namespace OpenDDD.Application.Settings
{
    public class SettingsException : Error.ApplicationException
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
