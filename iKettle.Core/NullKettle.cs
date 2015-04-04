using System;

namespace iKettle.Core
{
    public class NullKettle : IKettle
    {
        public event EventHandler<KettleStatusEventArgs> StatusChanged;

        public void Connect()
        {
            OnStatusChanged(new KettleStatusEventArgs(0, "iKettle not found. Using NullKettle. No tea today."));
        }

        public void HelloKettle()
        { }

        public void Off()
        { }

        public void Boil()
        { }

        public void GetStatus()
        { }

        protected virtual void OnStatusChanged(KettleStatusEventArgs e)
        {
            var handler = StatusChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void Dispose()
        { }
    }
}