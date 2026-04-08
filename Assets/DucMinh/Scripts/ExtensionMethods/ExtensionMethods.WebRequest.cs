using UnityEngine.Networking;

namespace DucMinh
{
    public static partial class ExtensionMethods
    {
        public static bool IsError(this UnityWebRequest request)
        {
            var result = request.result;
            return result == UnityWebRequest.Result.ConnectionError || result == UnityWebRequest.Result.ProtocolError ||
                   result == UnityWebRequest.Result.DataProcessingError;
        }
    }
}