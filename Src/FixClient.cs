using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using QuickFix.Transport;
using System;

namespace FixClient
{
    public class FIXClient
    {
        private readonly string _configFilePath;
        private readonly FIXApplication _application;
        private SessionSettings _settings = null!;
        private QuickFix.Transport.SocketInitiator? _initiator;
        public string? RawData { get; private set; }

        public FIXClient(string configFilePath)
        {
            _configFilePath = configFilePath ?? throw new ArgumentNullException(nameof(configFilePath));
            _application = new FIXApplication(this);

            InitializeSettings();
        }

        private void InitializeSettings()
        {
            try
            {
                _settings = new SessionSettings(_configFilePath);
                var storeFactory = new FileStoreFactory(_settings);
                var logFactory = new FileLogFactory(_settings);

                foreach (var sessionID in _settings.GetSessions())
                {
                    var sessionSettings = _settings.Get(sessionID);

                    if (sessionSettings.Has("rawdata"))
                    {
                        RawData = sessionSettings.GetString("rawdata");
                        Console.WriteLine($"RawData successfully read: {RawData}");
                    }
                    else
                    {
                        Console.WriteLine($"Key 'rawdata' not found in session {sessionID}");
                    }
                }

                if (string.IsNullOrEmpty(RawData))
                {
                    throw new Exception("RawData not found in any session.");
                }

                _initiator = new SocketInitiator(_application, storeFactory, _settings, logFactory);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error initializing FIX Client: {e.Message}");
                throw;
            }
        }

        public void Logon()
        {
            if (_initiator == null)
            {
                Console.WriteLine("FIX Client not initialized. Cannot logon.");
                return;
            }

            Console.WriteLine("Attempting logon with the primary host...");
            if (TryLogon())
            {
                Console.WriteLine("FIX Client logged on successfully to the primary host.");
                return;
            }

            Console.WriteLine("Primary host logon failed. Attempting logon with the secondary host...");
            SwitchToSecondaryHost();

            if (TryLogon())
            {
                Console.WriteLine("FIX Client logged on successfully to the secondary host.");
            }
            else
            {
                Console.WriteLine("Logon failed for both primary and secondary hosts.");
            }
        }

        private bool TryLogon()
        {
            try
            {
                _initiator?.Start();

                for (int i = 0; i < 10; i++)
                {
                    if (_initiator?.IsLoggedOn == true)
                    {
                        return true;
                    }
                    System.Threading.Thread.Sleep(1000);
                }

                _initiator?.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Logon attempt failed: {e.Message}");
            }

            return false;
        }

        private void SwitchToSecondaryHost()
        {
            foreach (var sessionID in _settings.GetSessions())
            {
                var sessionSettings = _settings.Get(sessionID);

                if (sessionSettings.Has("socketconnecthostsecondary"))
                {
                    var secondaryHost = sessionSettings.GetString("socketconnecthostsecondary");
                    sessionSettings.SetString("SocketConnectHost", secondaryHost);
                    Console.WriteLine($"Switched to secondary host: {secondaryHost}");
                }
                else
                {
                    Console.WriteLine($"No secondary host found in session {sessionID}");
                }
            }
        }

        public void Logout()
        {
            if (_initiator == null)
            {
                Console.WriteLine("FIX Client not initialized. Cannot logout.");
                return;
            }

            _initiator.Stop();
            Console.WriteLine("FIX Client logged out successfully.");
        }

        public void SendResendRequest(int beginSeqNo, int endSeqNo)
        {
            try
            {
                var resendRequest = new ResendRequest();
                resendRequest.SetField(new BeginSeqNo(beginSeqNo));
                resendRequest.SetField(new EndSeqNo(endSeqNo));

                SendMessage(resendRequest);
                Console.WriteLine($"ResendRequest sent for sequence numbers {beginSeqNo} to {endSeqNo}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error sending ResendRequest from {beginSeqNo} to {endSeqNo}: {e.Message}");
                throw;
            }
        }

        private void SendMessage(QuickFix.FIX44.Message message)
        {
            try
            {
                var sessionID = _initiator?.GetSessionIDs().ToArray()[0];
                if (sessionID == null)
                {
                    Console.WriteLine("SessionID not found. Cannot send message.");
                    return;
                }

                MsgType msgType = new();
                message.Header.GetField(msgType);

                var sendingTime = new SendingTime(DateTime.ParseExact(DateTime.UtcNow.ToString("yyyyMMdd-HH:mm:ss.fff"), "yyyyMMdd-HH:mm:ss.fff", null));
                message.Header.SetField(sendingTime);

                if (msgType.getValue() == MsgType.SEQUENCE_RESET || msgType.getValue() == MsgType.RESEND_REQUEST)
                {
                    var origSendingTime = new OrigSendingTime(DateTime.ParseExact(DateTime.UtcNow.ToString("yyyyMMdd-HH:mm:ss.fff"), "yyyyMMdd-HH:mm:ss.fff", null));
                    message.SetField(origSendingTime);

                    message.SetField(new PossDupFlag(true));
                }

                Session.SendToTarget(message, sessionID);
                Console.WriteLine($"Message sent to target: {message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error sending message: {e.Message}");
                throw;
            }
        }
    }
}
