using System;

namespace Xunkong.Widget.Hoyolab
{
    public class HoyolabException : Exception
    {

        public int ReturnCode { get; private set; }

        public HoyolabException(int returnCode, string message) : base($"{message} ({returnCode})")
        {
            ReturnCode = returnCode;
        }

    }
}