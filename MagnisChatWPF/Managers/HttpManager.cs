using Common.DTO.Account;
using Common.DTO.Communication;
using MagnisChatWPF.Extentions;
using MagnisChatWPF.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MagnisChatWPF.Managers
{
    [Export(typeof(IHttpManager))]
    public class HttpManager : IHttpManager
    {
        private readonly string _host;
        private string _token { get; set; }

        public HttpManager()
        {           
            _host = Properties.Settings.Default["HttpApi"].ToString();
        }

        public void Authorize(string token)
        {
            _token = token;
        }

        public async Task<ResponseDTO<T>> GetAsync<T>(string route)
        {
            try
            {
                WebRequest request = WebRequest.Create(_host + route);
                request.Headers.Add("Authorization", "Bearer " + _token);
                request.Method = "GET";

                HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return new ResponseDTO<T>() { Error = new Error(401, "Wrong token or user unauthorized") };
                }

                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();

                if (string.IsNullOrEmpty(responseFromServer))
                {
                    return new ResponseDTO<T>() { Error = { ErrorDescription = "Unknown error" } };
                }
                var data = JsonConvert.DeserializeObject<ResponseDTO<T>>(responseFromServer);
                return data;
            }
            catch (WebException)
            {                
                return new ResponseDTO<T>() { Error = new Error(500, "Couldn't connect to the remote server") };
            }
            catch (Exception)
            {                
                return new ResponseDTO<T>() { Error = new Error(500, "Unknown error") };
            }
        }

        public async Task<ResponseDTO<T>> PutAsync<T>(string path)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(_host + path);
                request.Method = "PUT";
                request.ContentType = "application/json";
                request.ContentLength = 0;
                request.Headers.Add("Authorization", "Bearer " + _token);

                var response = await request.GetResponseNoException();
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return new ResponseDTO<T>() { Error = new Error(401, "Wrong token or user unauthorized") };
                }

                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();

                if (string.IsNullOrEmpty(responseFromServer))
                {
                    return new ResponseDTO<T>() { Error = { ErrorDescription = "Unknown error" } };
                }
                var data = JsonConvert.DeserializeObject<ResponseDTO<T>>(responseFromServer);
                return data;
            }
            catch (WebException)
            {                
                return new ResponseDTO<T>() { Error = new Error(500, "Couldn't connect to the remote server") };
            }
            catch (Exception)
            {               
                return new ResponseDTO<T>() { Error = new Error(500, "Unknown error") };
            }
        }       

        public async Task<ResponseDTO<T>> PostAsync<T>(string path, object body = null)
        {
            try
            {          
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(_host + path);
                request.Method = "POST";
                request.Headers.Add("Authorization", "Bearer " + _token);
                request.ContentType = "application/json";

                if(body == null)
                {
                    request.ContentLength = 0;
                }
                else
                {
                    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        string json = JsonConvert.SerializeObject(body);
                        streamWriter.Write(json);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                }            
           
                var response = await request.GetResponseNoException();

                if(response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return new ResponseDTO<T>() { Error = new Error(401, "Wrong token or user unauthorized") };
                }

                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();

                if (string.IsNullOrEmpty(responseFromServer))
                {
                    return new ResponseDTO<T>() { Error = { ErrorDescription = "Unknown error" } };
                }
                var data = JsonConvert.DeserializeObject<ResponseDTO<T>>(responseFromServer);
                return data;
            }
            catch (WebException)
            {               
                return new ResponseDTO<T>() { Error = new Error(500, "Couldn't connect to the remote server") };
            }
            catch (Exception)
            {                
                return new ResponseDTO<T>() { Error = new Error(500, "Unknown error") };
            }
        }
        
    }
}
