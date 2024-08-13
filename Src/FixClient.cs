using QuickFix;
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

            _initiator.Start();
            Console.WriteLine("FIX Client logged on successfully.");
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
    }
}
