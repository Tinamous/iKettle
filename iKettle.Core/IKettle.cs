using System;

namespace iKettle.Core
{
    public interface IKettle : IDisposable
    {
        event EventHandler<KettleStatusEventArgs> StatusChanged;
        void Connect();
        void HelloKettle();
        void Off();
        void Boil();
        void GetStatus();
    }
}