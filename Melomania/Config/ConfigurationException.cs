using System;

namespace Melomania.Config
{
    public class ConfigurationException : Exception
    {
        public ConfigurationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}