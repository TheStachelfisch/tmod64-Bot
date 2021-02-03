using System;

namespace tMod64Bot.Services.Config
{
    public class TypeMismatchException : Exception
    {
        public TypeMismatchException(string message) : base(message) { }
    }
}