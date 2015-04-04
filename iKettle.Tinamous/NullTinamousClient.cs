using System;

namespace iKettle.Tinamous
{
    public class NullTinamousClient : ITinamousClient
    {
        public void Dispose()
        { }

        public event EventHandler<StatusPostReceivedEventArgs> PostReceived;
        public void Connect()
        { }

        public void Disconnect()
        { }

        public void SubscribeToPostsToUser()
        { }

        public void PublishStatus(string message)
        { }
    }
}