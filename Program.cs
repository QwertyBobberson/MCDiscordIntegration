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
        
        long guildID;
        long channelID;

        public async Task MainAsync(string[] args)
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.Guilds
            });
            Console.WriteLine("Started");
            string varsDirectory = args[0];
            string serverName = args[1];
            string lastTimestamp = "[:::]";
            if(File.Exists(varsDirectory))
            {
                StreamReader vars = new StreamReader(File.OpenRead(varsDirectory));
                string minecraftDirectory = vars.ReadLine() + $"{serverName}/";
                string logsDirectory = minecraftDirectory + "logs/latest.log";
                string[] whitelist = vars.ReadLine().Split(',');
                string token = vars.ReadLine();
                guildID = long.Parse(vars.ReadLine());
                channelID = long.Parse(vars.ReadLine());

                await _client.LoginAsync(TokenType.Bot, token);
                await _client.StartAsync();

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

                        if(!latestLog.Contains(lastTimestamp))
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
                            lastTimestamp = latestLog.Substring(0, 10);
                        }

                        logsFile.Close();
                    }
                }
            }
            else
            {
                Console.WriteLine($"File {varsDirectory} does not exist");
            }
        }
    }
}
