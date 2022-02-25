using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace MCDiscord
{
    class Program
    {
        private DiscordSocketClient _client;
        private SocketTextChannel channel;

        public static Task Main(string[] args) => new Program().MainAsync(args);
        
        ulong guildID;
        ulong channelID;

        public async Task MainAsync(string[] args)
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.Guilds
            });
            string varsDirectory = args[0];
            string serverName = args[1];
            string lastMessage = "[:::]";
            if(File.Exists(varsDirectory))
            {
                Console.WriteLine($"{varsDirectory} exists");
                StreamReader vars = new StreamReader(File.OpenRead(varsDirectory));
                string minecraftDirectory = vars.ReadLine() + $"{serverName}/";
                string logsDirectory = minecraftDirectory + "logs/latest.log";
                string[] whitelist = vars.ReadLine().Split(',');
                string token = vars.ReadLine();
                guildID = ulong.Parse(vars.ReadLine());
                channelID = ulong.Parse(vars.ReadLine());

                await _client.LoginAsync(TokenType.Bot, token);
                await _client.StartAsync();
                _client.Ready += GetChannel;

                if(!File.Exists(logsDirectory))
                {
                    throw new Exception($"Logs do not exist for {serverName}");
                }
                else
                {
                    while(true)
                    {
                        StreamReader logsFile = new StreamReader(File.OpenRead(logsDirectory));
                    
                        string[] logs = logsFile.ReadToEnd().Split('\n');
                        int i = 0;
                        string latestLog;
                        
                        do
                        {  
                            i++;
                            latestLog = logs[logs.Length - i].Trim();
                        } while(latestLog == "");
                        
                        if(channel == null)
                        {
                            continue;
                        }

                        while(latestLog != lastMessage && i < logs.Length)
                        {  
                            for(int j = 0; j < whitelist.Length; j++)
                            {
                                if(latestLog.Contains(whitelist[j]))
                                {
                                    if(latestLog.Contains("left the game"))
                                    {
                                        Console.WriteLine(whitelist[j] + " has disconnected from the server");
                                        await channel.SendMessageAsync(whitelist[j] + " has disconnected from the server");
                                    }
                                    if(latestLog.Contains("joined the game"))
                                    {
                                        Console.WriteLine(whitelist[j] + " has connected to the server");
                                        await channel.SendMessageAsync(whitelist[j] + " has connected to the server");
                                    }
                                }
                            }

                            i++;
                            latestLog = logs[logs.Length - i].Trim();
                        } 
                        
                        i = 0;
                        
                        do
                        {  
                            i++;
                            latestLog = logs[logs.Length - i].Trim();
                        } while(latestLog == "");

                        lastMessage = latestLog;
                        logsFile.Close();
                        Console.WriteLine("Thread Sleeping");
                        System.Threading.Thread.Sleep(10000);
                    }
                }
            }
            else
            {
                Console.WriteLine($"File {varsDirectory} does not exist");
            }
        }

        public async Task GetChannel()
        {
            channel = _client.GetGuild(guildID).GetTextChannel(channelID);
        }
    }
}
