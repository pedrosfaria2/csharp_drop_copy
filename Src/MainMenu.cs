using System;

namespace FixClient
{
    public static class MainMenu
    {
        public static void Display(List<FIXClient> clients)
        {
            Console.Title = "FIX Client Menu";
            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("===================================");
                Console.WriteLine("        Welcome to FIX Client      ");
                Console.WriteLine("===================================");
                Console.ResetColor();

                Console.WriteLine("Please select an option:");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("1. Logon");
                Console.WriteLine("2. Send ResendRequest");
                Console.WriteLine("3. Logout and Exit");
                Console.ResetColor();

                Console.Write("Your choice: ");
                var choice = Console.ReadKey(true).KeyChar;

                switch (choice)
                {
                    case '1':
                        LogonClients(clients);
                        break;
                    case '2':
                        SendResendRequest(clients);
                        break;
                    case '3':
                        if (ConfirmExit())
                        {
                            LogoutClients(clients);
                            return;
                        }
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid choice. Please try again.");
                        Console.ResetColor();
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

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("All clients logged on successfully.");
            Console.ResetColor();
            Console.ReadLine();
        }

        private static void SendResendRequest(List<FIXClient> clients)
        {
            Console.Write("Enter BeginSeqNo: ");
            if (!int.TryParse(Console.ReadLine(), out int beginSeqNo))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid BeginSeqNo. Please enter a valid integer.");
                Console.ResetColor();
                return;
            }

            Console.Write("Enter EndSeqNo (0 for all subsequent messages): ");
            if (!int.TryParse(Console.ReadLine(), out int endSeqNo))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid EndSeqNo. Please enter a valid integer.");
                Console.ResetColor();
                return;
            }

            foreach (var client in clients)
            {
                client.SendResendRequest(beginSeqNo, endSeqNo);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("ResendRequest sent to all clients.");
            Console.ResetColor();
            Console.ReadLine();
        }

        private static void LogoutClients(List<FIXClient> clients)
        {
            foreach (var client in clients)
            {
                client.Logout();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("All clients logged out successfully.");
            Console.ResetColor();
            Console.ReadLine();
        }

        private static bool ConfirmExit()
        {
            Console.Write("Are you sure you want to exit? (y/n): ");
            var key = Console.ReadKey(true).KeyChar;
            return key == 'y' || key == 'Y';
        }
    }
}
