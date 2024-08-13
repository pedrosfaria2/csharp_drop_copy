using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FixClient
{
    static class Program
    {
        static void Main()
        {
            const string configPath = "config.yaml";
            var clients = LoadClients(configPath);

            if (clients.Count == 0)
            {
                Console.WriteLine("No clients loaded. Exiting.");
                return;
            }

            MainMenu.Display(clients);
        }

        static List<FIXClient> LoadClients(string configPath)
        {
            var clients = new List<FIXClient>();

            if (!File.Exists(configPath))
            {
                Console.WriteLine($"Configuration file {configPath} not found.");
                return clients;
            }

            try
            {
                var input = File.ReadAllText(configPath);
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                var config = deserializer.Deserialize<Dictionary<string, List<SessionConfig>>>(input);

                if (config.TryGetValue("sessions", out List<SessionConfig>? value))
                {
                    foreach (var session in value)
                    {
                        if (!string.IsNullOrEmpty(session.ConfigFile))
                        {
                            var client = new FIXClient(session.ConfigFile);
                            clients.Add(client);
                        }
                        else
                        {
                            Console.WriteLine("Invalid configuration. ConfigFile is required.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration: {ex.Message}");
            }

            return clients;
        }
    }
}
