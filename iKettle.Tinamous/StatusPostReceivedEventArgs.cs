using System;

namespace iKettle.Tinamous
{
    public class StatusPostReceivedEventArgs : EventArgs
    {
        public StatusPostReceivedEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}