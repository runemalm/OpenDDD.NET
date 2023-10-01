using System.Collections.Generic;

namespace OpenDDD.Infrastructure.Ports.Storage
{
    public interface IStorage
    {
        void Put(string key, string value);
        string Get(string key);
        IEnumerable<string> GetWithPrefix(string prefix);
        void Erase(string key);
        void Truncate();
    }
}
