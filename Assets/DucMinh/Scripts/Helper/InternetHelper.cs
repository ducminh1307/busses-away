using System;
using System.Collections;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace DucMinh
{
    public static class InternetHelper
    {
        private static readonly HttpClient Client = new HttpClient();

        public static async Task<bool> IsInternetAvailableAsync()
        {
            try
            {
                var response = await Client.GetAsync("http://www.google.com");
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsInternetAvailable()
        {
            try
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                    return false;

                using var client = new System.Net.WebClient();
                client.DownloadString("http://www.google.com");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        static IEnumerator CheckInternetCoroutine()
        {
            yield return null;
        }
    }
}