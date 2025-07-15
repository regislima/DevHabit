using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace DevHabit.Api.Services;

public sealed class InMemoryETagStore
{
    private static readonly ConcurrentDictionary<string, string> ETags = new();

    public string GetETag(string resourceUri) => ETags.GetOrAdd(resourceUri, _ => string.Empty);

    public void SetETag(string resourceUri, object resource) =>
        ETags.AddOrUpdate(resourceUri, GenerateETag(resource), (_, _) => GenerateETag(resource));

    public void RemoveETag(string resourceUri) => ETags.TryRemove(resourceUri, out _);
    
    private static string GenerateETag(object resource)
    {
        var content = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(resource));
        var hash = SHA512.HashData(content);

        return Convert.ToHexString(hash);
    }
}
