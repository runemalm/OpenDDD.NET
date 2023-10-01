using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using OpenDDD.Infrastructure.Ports.Storage;

namespace OpenDDD.Infrastructure.Ports.Adapters.Storage.Memory
{
    public class MemoryStorage : IStorage
    {
        protected ConcurrentDictionary<string, string> Data;

        public MemoryStorage()
        {
            Truncate();
        }

        public void Put(string key, string value)
        {
            Data[key] = value;
        }

        public string Get(string key)
            => Data.Single(kvp => kvp.Key == key).Value;

        public IEnumerable<string> GetWithPrefix(string prefix)
            => Data.Where(kvp => kvp.Key.StartsWith(prefix)).Select(kvp => kvp.Value);

        public void Erase(string key)
            => Data.Remove(key, out string _);

        public void Truncate()
        {
            Data = new ConcurrentDictionary<string, string>();
        }
    }
}
