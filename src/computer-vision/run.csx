#r "Newtonsoft.Json"

using System.Net.Http.Headers;
using System.Configuration;
using System.Threading.Tasks;

private static readonly string key = ConfigurationManager.AppSettings["SubscriptionKey"];
private static readonly string endpoint = ConfigurationManager.AppSettings["Url"];

public async static Task<string> Run(Stream input, string filename, TraceWriter log)
{
    log.Info($"Calling computer vision for {filename}...");

    var client = new HttpClient();
    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

    var array = await ToByteArrayAsync(input);

    var payload = new ByteArrayContent(array);
    payload.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/octet-stream");
    
    var queryParams = "mode=Handwritten";
    
    var results = await client.PostAsync(endpoint + "?" + queryParams, payload);
    string operationLocation;
    log.Info("url"+ endpoint);
    log.Info("sentto "+"?"+queryParams);
    log.Info("Status code " + results.StatusCode);
    log.Info("the key is "+key);

    System.Threading.Thread.Sleep(1000);
    if(results.IsSuccessStatusCode)
    {
        log.Info("all is good");
        operationLocation = results.Headers.GetValues("Operation-Location").FirstOrDefault();
        log.Info("op location is " + operationLocation);
   
    string contentString;
    //
    int i=0;
    do{
        System.Threading.Thread.Sleep(1000);

    results = await client.GetAsync(operationLocation);
    log.Info("status code 2 is "+ results.StatusCode);
    contentString = await results.Content.ReadAsStringAsync();
    log.Info("----- " + contentString);


    var obj = await results.Content.ReadAsAsync<object>();
    var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
 
    log.Info($"Results {i} for {filename} : {json}");
    }
    while(i<10&& contentString.IndexOf("\"status\":\"Succeeded\"")==-1);
    return ("w1");
    }

    else return("wonderful");
}

// Converts a stream to a byte array
private async static Task<byte[]> ToByteArrayAsync(Stream stream)
{
    var length = stream.Length > Int32.MaxValue ? Int32.MaxValue : Convert.ToInt32(stream.Length);
    var buffer = new Byte[length];
    await stream.ReadAsync(buffer, 0, length);
    stream.Position = 0;
    return buffer;
}
