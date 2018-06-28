using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MagnisChatWPF.Extentions
{
    public static class HttpWebResponseExtention
    {
        public async static Task<HttpWebResponse> GetResponseNoException(this HttpWebRequest request)
        {
            try
            {
                return (HttpWebResponse) await request.GetResponseAsync();
            }
            catch (WebException we)
            {
                var resp = we.Response as HttpWebResponse;
                if (resp == null)
                    throw;
                return resp;
            }
        }
    }
}
