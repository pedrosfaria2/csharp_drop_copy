using System;
using System.Collections.Generic;

namespace FixClient
{
    public static class MainMenu
    {
        public static void Display(List<FIXClient> clients)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("1. Logon");
                Console.WriteLine("2. Send ResendRequest");
                Console.WriteLine("3. Logout and Exit");
                Console.Write("Enter your choice: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        LogonClients(clients);
                        break;
                    case "2":
                        SendResendRequest(clients);
                        break;
                    case "3":
                        LogoutClients(clients);
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        private static void LogonClients(List<FIXClient> clients)
        {
            foreach (var client in clients)
            {
                client.Logon();
            }

            Console.WriteLine("All clients logged on successfully.");
            Console.ReadLine();
        }

        private static void SendResendRequest(List<FIXClient> clients)
        {
            Console.Write("Enter BeginSeqNo: ");
            if (!int.TryParse(Console.ReadLine(), out int beginSeqNo))
            {
                Console.WriteLine("Invalid BeginSeqNo. Please enter a valid integer.");
                return;
            }

            Console.Write("Enter EndSeqNo (0 for all subsequent messages): ");
            if (!int.TryParse(Console.ReadLine(), out int endSeqNo))
            {
                Console.WriteLine("Invalid EndSeqNo. Please enter a valid integer.");
                return;
            }

            foreach (var client in clients)
            {
                client.SendResendRequest(beginSeqNo, endSeqNo);
            }

            Console.WriteLine("ResendRequest sent to all clients.");
            Console.ReadLine();
        }

        private static void LogoutClients(List<FIXClient> clients)
        {
            foreach (var client in clients)
            {
                client.Logout();
            }

            Console.WriteLine("All clients logged out successfully.");
            Console.ReadLine();
        }
    }
}
