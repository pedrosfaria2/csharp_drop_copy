using QuickFix;
using QuickFix.Fields;
using System;

namespace FixClient
{
    public class FIXApplication(FIXClient client) : IApplication
    {
        private readonly FIXClient _client = client;

        public void OnCreate(SessionID sessionID)
        {
            Console.WriteLine($"Session created: {sessionID}");
        }

        public void OnLogon(SessionID sessionID)
        {
            Console.WriteLine($"Successful logon to session {sessionID}");
        }

        public void OnLogout(SessionID sessionID)
        {
            Console.WriteLine($"Successful logout from session {sessionID}");
        }

        public void ToAdmin(Message message, SessionID sessionID)
        {
            try
            {
                MsgType msgType = new();
                message.Header.GetField(msgType);

                if (msgType.getValue() == MsgType.LOGON)
                {
                    string? rawData = _client.RawData;
                    message.SetField(new RawData(rawData));
                    if (rawData != null)
                    {
                        message.SetField(new RawDataLength(rawData.Length));
                    }
                }

                Console.WriteLine($"To Admin: {message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in ToAdmin: {e.Message}");
            }
        }

        public void FromAdmin(Message message, SessionID sessionID)
        {
            Console.WriteLine($"From Admin: {message}");
        }

        public void ToApp(Message message, SessionID sessionID)
        {
            Console.WriteLine($"To App: {message}");
        }

        public void FromApp(Message message, SessionID sessionID)
        {
            Console.WriteLine($"From App: {message}");
        }
    }
}