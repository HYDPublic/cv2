#r "Newtonsoft.Json"

using System.Net.Http.Headers;
using System.Configuration;



public async static Task<string> Run(Stream myBlob, string name, TraceWriter log)
{       
    log.Info($"Calling computer vision for {name}...");

    var array = await ToByteArrayAsync(myBlob);

    HttpClient client = new HttpClient();
    var key = ConfigurationManager.AppSettings["SubscriptionKey"];
    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

    HttpContent payload = new ByteArrayContent(array);
    payload.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/octet-stream");
    
    var endpoint = "https://westus.api.cognitive.microsoft.com/vision/v1.0/";//ConfigurationManager.AppSettings["VisionEndpoint"];
    var results = await client.PostAsync(endpoint + "/analyze?visualFeatures=ImageType,Faces,Adult,Categories,Color,Tags,Description", payload);
    log.Info("Status code "+ results.StatusCode);
    var obj = await results.Content.ReadAsAsync<object>();
    var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
    log.Info($"Results for {name} : {json}");
    return json;
}

// Converts a stream to a byte array
private async static Task<byte[]> ToByteArrayAsync(Stream stream)
{
    Int32 length = stream.Length > Int32.MaxValue ? Int32.MaxValue : Convert.ToInt32(stream.Length);
    byte[] buffer = new Byte[length];
    await stream.ReadAsync(buffer, 0, length);
    stream.Position = 0;
    return buffer;
}
