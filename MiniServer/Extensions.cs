using System.Text;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
namespace MiniServer
{
    public static class Extensions
    {
        public static async Task SendJSONAsync(this HttpListenerResponse response, object objectToSend)
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(objectToSend));
            response.ContentType = "application/json";
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = data.LongLength;

            // Write out to the response stream (asynchronously), then close it
            await response.OutputStream.WriteAsync(data, 0, data.Length);
            response.Close();
        }
    }
}
