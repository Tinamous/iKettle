namespace iKettle.Core
{
    public class KettleStatusEventArgs
    {
        public int Code { get; set; }
        public string Message { get; set; }

        public KettleStatusEventArgs(int code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}