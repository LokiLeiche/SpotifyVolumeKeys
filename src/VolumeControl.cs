using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers; // For general headers
using System.Text.Json;
using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;
using System.Net.NetworkInformation;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


public class VolumeControl
{
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
    [DllImport("kernel32")]
    private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
    [DllImport("kernel32")]
    private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);


    private static string refreshToken = "";
    private static string authToken = "";
    private static int volume = 50;
    private static string path = "./config.ini";
    private static int VolUpBtn = 175;
    private static int VolDownBtn = 174;
    private static int increment = 5;
    private static string client_id = "";
    private static string client_secret = "";

    // INI FILE TEST
    public static void IniFile(string INIPath)
    {
        path = INIPath;
    }

    public static void IniWriteValue(string Section, string Key, string Value)
    {
        WritePrivateProfileString(Section, Key, Value, path);
    }

    public static string IniReadValue(string Section, string Key)
    {
        StringBuilder temp = new StringBuilder(255);
        int i = GetPrivateProfileString(Section, Key, "", temp, 255, path);
        return temp.ToString();
    }



    public static void LoadConfig()
    {
        string vol = IniReadValue("configuration", "default_volume");
        if (!string.IsNullOrEmpty(vol))
        {
            volume = int.Parse(vol);
        }
        else
        {
            IniWriteValue("configuration", "default_volume", "50");
        }

        string btn1 = IniReadValue("configuration", "volume_up_button");
        if (!string.IsNullOrEmpty (btn1))
        {
            VolUpBtn = int.Parse(btn1);
        }
        else
        {
            IniWriteValue("configuration", "volume_up_button", "175");
        }

        string btn2 = IniReadValue("configuration", "volume_down_button");
        if (!string.IsNullOrEmpty(btn2))
        {
            VolDownBtn = int.Parse(btn2);
        }
        else
        {
            IniWriteValue("configuration", "volume_down_button", "174");
        }

        string vol_increment = IniReadValue("configuration", "volume_increment");
        if (!string.IsNullOrEmpty(vol_increment))
        {
            increment = int.Parse(vol_increment);
        }
        else
        {
            IniWriteValue("configuration", "volume_increment", "5");
        }
    }


    // main script
    public static async Task Main()
    {
        IniFile("./config.ini");
        LoadConfig();

        string id = IniReadValue("tokens", "client_id");
        if (id == "")
        {
            string? newid = "";
            while (newid == null || newid == "")
            {
                Console.WriteLine("No client_id found in config.ini, enter your spotify app client_id here:");
                newid = Console.ReadLine(); // Reads a line of text from the console
                if (!string.IsNullOrEmpty(newid))
                {
                    client_id = newid;
                    IniWriteValue("tokens", "client_id", newid);
                    Console.WriteLine("client_id saved!");
                }
                else
                {
                    Console.WriteLine("You did not provide a client_id");
                }
            }
        }
        else
        {
            client_id = id;
        }

        string secret = IniReadValue("tokens", "client_secret");
        if (secret == "")
        {
            string? newSecret = "";
            while (string.IsNullOrEmpty(newSecret))
            {
                Console.WriteLine("No client_secret found in config.ini, enter your spotify app client_secret here:");
                newSecret = Console.ReadLine(); // Reads a line of text from the console
                if (!string.IsNullOrEmpty(newSecret))
                {
                    client_secret = newSecret;
                    IniWriteValue("tokens", "client_secret", newSecret);
                    Console.WriteLine("client_secret saved!");
                }
                else
                {
                    Console.WriteLine("You did not provide a client_secret");
                }
            }
        }
        else
        {
            client_secret = secret;
        }


        string token = IniReadValue("tokens", "refresh_token");
        if (token == "")
        {
            string? response = "";
            while (response != "y" && response != "Y" && response != "n" && response != "N")
            {
                Console.WriteLine("No refresh_token found in config.ini - Do you already have a refresh_token generated? (y/n)");
                response = Console.ReadLine();
            }

            if (response == "y" || response == "Y")
            {
                string? newToken = "";
                while (string.IsNullOrEmpty(newToken))
                {
                    Console.WriteLine("No refresh_token found in config.ini, enter your spotify api refresh_token here:");
                    newToken = Console.ReadLine(); // Reads a line of text from the console
                    if (!string.IsNullOrEmpty(newToken))
                    {
                        refreshToken = newToken;
                        IniWriteValue("tokens", "refresh_token", newToken);
                        Console.WriteLine("Token saved!");
                    }
                    else
                    {
                        Console.WriteLine("You did not provide a token");
                    }
                }
            }
            else
            {
                await GetInitialToken();
            }
        }
        else
        {
            refreshToken = token;
        }

        authToken = await GetNewToken();
        _ = Refresh();

        await GetInitialVolume();

        Console.WriteLine("Application now running and listening for your inputs. You can minimize this window now. Look at the config.ini for configuration of keys etc");
        while (true)
        {
            Thread.Sleep(1);
            int previousVolume = volume;
            short volUpState = GetAsyncKeyState(VolUpBtn);
            short volDownState = GetAsyncKeyState(VolDownBtn);
            if (volUpState != 0)
            {
                volume = volume + increment;
            }
            else if (volDownState != 0)
            {
                volume = volume - increment;
            }
            if (volume < 0) volume = 0; else if (volume > 100) volume = 100;
            if (volume != previousVolume)
            {
                await Request(volume);
            }
        }
    }

    public static async Task Refresh()
    {
        while (true)
        {
            authToken = await GetNewToken();
            await Task.Delay(1000 * 60 * 50);
        }
    }


     public static async Task Request(int arg)
     {
        string url = $"https://api.spotify.com/v1/me/player/volume?volume_percent={arg}";

        // 2. Create an HttpClient
        using (HttpClient client = new())
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Put, url))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                    HttpResponseMessage response = await client.SendAsync(request);

                    if (! response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}\n{response}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        Console.WriteLine($"Changed volume to {arg}");
     }

    public static async Task GetInitialVolume()
    {
        string url = "https://api.spotify.com/v1/me/player";

        // 2. Create an HttpClient
        using (HttpClient client = new())
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                    HttpResponseMessage response = await client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}\n{response}");
                    }
                    else
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        //Console.WriteLine($"{content}");
                        using (JsonDocument document = JsonDocument.Parse(content))
                        {
                            JsonElement root = document.RootElement;
                            if (root.TryGetProperty("device", out JsonElement element))
                            {
                                JsonDocument device = JsonDocument.Parse(element.GetRawText());
                                JsonElement deviceElem = device.RootElement;
                                if (deviceElem.TryGetProperty("volume_percent", out JsonElement vol))
                                {
                                    int newVol = vol.GetInt16();
                                    volume = newVol;
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }


    public static async Task<string> GetNewToken()
    {
        using (HttpClient client = new())
        {
            string tokenUrl = "https://accounts.spotify.com/api/token";
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken },
                { "client_id", client_id },
                { "client_secret", client_secret },
            });

            try
            {
                HttpResponseMessage response = await client.PostAsync(tokenUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();


                    using (JsonDocument document = JsonDocument.Parse(responseJson))
                    {
                        JsonElement root = document.RootElement;

                        if (root.TryGetProperty("refresh_token", out JsonElement refresh_tokenElement))
                        {
                            string? newToken = refresh_tokenElement.GetString();
                            if (!string.IsNullOrEmpty(newToken))
                            {
                                refreshToken = newToken;
                                Console.WriteLine("New Refresh-Token generated!");
                                IniWriteValue("tokens", "refresh_token", newToken);
                            }
                        }

                        if (root.TryGetProperty("access_token", out JsonElement access_tokenElement))
                        {
                            string? newauthToken = access_tokenElement.GetString();
                            if (!string.IsNullOrEmpty(newauthToken))
                            {
                                return newauthToken;
                            }
                            else
                            {
                                return "";
                            }
                            
                        }
                        else
                        {
                            Console.WriteLine("No access_token found in response!");
                            return "";
                        }

                    }

                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    return "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "";
            }
        }
    }


    public static async Task<string> GetInitialToken()
    {
        using (HttpClient client = new())
        {
            string? code = "";
            while (string.IsNullOrEmpty(code))
            {
                Console.WriteLine("Enter the code you got after the authorization process here:");
                code = Console.ReadLine();
            }
            string? redirect = "";
            while (string.IsNullOrEmpty(redirect))
            {
                Console.WriteLine("Enter the redirect URL you used for authorization here:");
                redirect = Console.ReadLine();
            }
            string tokenUrl = "https://accounts.spotify.com/api/token";
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "client_id", client_id },
                { "client_secret", client_secret },
                { "code", code },
                { "redirect_uri", redirect },
            });

            try
            {
                HttpResponseMessage response = await client.PostAsync(tokenUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();


                    using (JsonDocument document = JsonDocument.Parse(responseJson))
                    {
                        JsonElement root = document.RootElement;

                        if (root.TryGetProperty("refresh_token", out JsonElement refresh_tokenElement))
                        {
                            string? newToken = refresh_tokenElement.GetString();
                            if (! string.IsNullOrEmpty(newToken))
                            {
                                refreshToken = newToken;
                                Console.WriteLine("New Refresh-Token generated!");
                                IniWriteValue("tokens", "refresh_token", newToken);
                            }
                            else
                            {
                                Console.WriteLine("There was an error with the request, failed to obtain refresh_token");
                                Environment.Exit(1);
                            }
                        }

                        if (root.TryGetProperty("access_token", out JsonElement access_tokenElement))
                        {
                            string? newauthToken = access_tokenElement.GetString();
                            if (newauthToken != null)
                            {
                                return newauthToken;
                            }
                            else
                            {
                                string tok = await GetNewToken();
                                return tok;
                            }

                        }
                        else
                        {
                            string tok = await GetNewToken();
                            return tok;
                        }

                    }

                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    return "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "";
            }
        }
    }
}
