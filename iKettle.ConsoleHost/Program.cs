using System;
using System.Diagnostics;
using iKettle.Core;
using iKettle.Tinamous;

namespace iKettle.ConsoleHost
{
    class Program
    {
        private static IKettle _kettle;
        private static ITinamousClient _tinamousClient;

        static void Main(string[] args)
        {
            using (_tinamousClient = CreateTinamousClient())
            {
                using (_kettle = KettleFinder.Find())
                {
                    _kettle.StatusChanged += kettle_StatusChanged;

                    _kettle.Connect();

                    ShowUserCommandOptions();

                    ProcessUserCommand();
                }
            }
        }

        private static ITinamousClient CreateTinamousClient()
        {
            try
            {
                var client = new TinamousClient();
                client.Connect();
                client.SubscribeToPostsToUser();
                client.PostReceived += tinamousClient_PostReceived;
                return client;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error connecting to Tinamous: " + ex);
                return new NullTinamousClient();
            }
        }

        private static void ShowUserCommandOptions()
        {
            Console.WriteLine("Press B to boil");
            Console.WriteLine("Press H for HelloKettle");
            Console.WriteLine("Press O to switch off");
            Console.WriteLine("Press S for status");
            Console.WriteLine("Press X to quit");
        }

        private static void ProcessUserCommand()
        {
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey();

                switch (key.KeyChar)
                {
                    case 'B':
                    case 'b':
                        _kettle.Boil();
                        break;
                    case 'H':
                    case 'h':
                        _kettle.HelloKettle();
                        break;
                    case 'O':
                    case 'o':
                        _kettle.Off();
                        break;
                    case 'S':
                    case 's':
                        _kettle.GetStatus();
                        break;
                    case 'X':
                    case 'x':
                        return;
                }
            }
        }

        static void tinamousClient_PostReceived(object sender, StatusPostReceivedEventArgs e)
        {
            string message = e.Message.ToLower();
            ProcessMessage(message);
        }

        private static void ProcessMessage(string message)
        {
            if (message.ToLower().Contains("boil"))
            {
                Trace.WriteLine("Received boil command");
                _kettle.Boil();
            }
            else if (message.Contains("hello"))
            {
                Trace.WriteLine("Received Hello command");
                _kettle.HelloKettle();
            }
            else
            {
                _tinamousClient.PublishStatus("Unknown command.");
            }
        }

        static void kettle_StatusChanged(object sender, KettleStatusEventArgs e)
        {
            Console.WriteLine("Kettle status changed: " + e.Message);
            _tinamousClient.PublishStatus(e.Message);
        }
    }
}
