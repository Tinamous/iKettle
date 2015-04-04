using System;

namespace iKettle.Tinamous
{
    public interface ITinamousClient : IDisposable
    {
        event EventHandler<StatusPostReceivedEventArgs> PostReceived;
        void Connect();
        void Disconnect();
        void SubscribeToPostsToUser();
        void PublishStatus(string message);
    }
}