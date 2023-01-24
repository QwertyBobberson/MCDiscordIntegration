using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.Text.RegularExpressions;

namespace MCDiscord
{
    class Program
    {
        private DiscordSocketClient _client;
        private SocketTextChannel channel;

        public static Task Main(string[] args) => new Program().MainAsync(args);

        ulong guildID;
        ulong channelID;

        bool quit = false;


        public async Task MainAsync(string[] args)
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.Guilds,
                LogLevel = LogSeverity.Debug
            });
            string varsDirectory = args[0];
            if(File.Exists(varsDirectory))
            {
                Console.WriteLine($"{varsDirectory} exists");
                StreamReader vars = new StreamReader(File.OpenRead(varsDirectory));
                string logsDirectory = vars.ReadLine();
                string[] whitelist = vars.ReadLine().Split(',');
                string lastMessage;
                string connectMessage = vars.ReadLine();
                string disconnectMessage = vars.ReadLine();
                string token = vars.ReadLine();
                guildID = ulong.Parse(vars.ReadLine());
                channelID = ulong.Parse(vars.ReadLine());

                await _client.LoginAsync(TokenType.Bot, token);
                await _client.StartAsync();
                _client.Ready += () =>
                    {
                        channel = _client.GetGuild(guildID).GetTextChannel(channelID);
                        return Task.CompletedTask;
                    };
                _client.Log += (msg) =>
                    {
                        Console.WriteLine($"{msg.ToString()}");
                        return Task.CompletedTask;
                    };

                if(!File.Exists(logsDirectory))
                {
                    throw new Exception($"{logsDirectory} does not exist");
                }
                else
                {
                    StreamReader logsFile = new StreamReader(File.OpenRead(logsDirectory));
                    lastMessage = logsFile.ReadLine();
                    logsFile.Close();
                    Console.WriteLine(lastMessage);

                    while(!quit)
                    {
                        if(Console.KeyAvailable)
                        {
                            char keypressed = Console.ReadKey(true).KeyChar;
                            if(keypressed == 'q' || keypressed == 'Q')
                            {
                                quit = true;
                            }
                        }
                        logsFile = new StreamReader(File.OpenRead(logsDirectory));

                        string[] logs = logsFile.ReadToEnd().Split('\n');
                        int i = 0;
                        string latestLog;

                        do
                        {
                            i++;
                            latestLog = logs[logs.Length - i].Trim();
                        } while(latestLog == "");

                        // if(channel == null)
                        // {
                        //     continue;
                        // }

                        Console.WriteLine($"Latest Log: {latestLog}\nLast Message: {lastMessage}\n");
                        while(latestLog != lastMessage && i < logs.Length)
                        {
                            for(int j = 0; j < whitelist.Length; j++)
                            {
                                if(latestLog.Contains(whitelist[j]))
                                {
                                    if(Regex.Match(latestLog, disconnectMessage).Success)
                                    {
                                        Console.WriteLine(whitelist[j] + " has disconnected from the server");
                                        // await channel.SendMessageAsync(whitelist[j] + " has disconnected from the server");
                                    }
                                    if(Regex.Match(latestLog, connectMessage).Success)
                                    {
                                        Console.WriteLine(whitelist[j] + " has connected to the server");
                                        // await channel.SendMessageAsync(whitelist[j] + " has connected to the server");
                                    }
                                }
                            }

                            i++;
                            Console.Write($"Read from {latestLog} to ");
                            latestLog = logs[logs.Length - i].Trim();
                            Console.WriteLine(latestLog);
                        }

                        i = 0;

                        do
                        {
                            i++;
                            lastMessage = logs[logs.Length - i].Trim();
                        } while(lastMessage == "");

                        logsFile.Close();
                        System.Threading.Thread.Sleep(1000);
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
            Console.WriteLine("Getting channel");
            Console.WriteLine("Got channel");
        }
    }
}
