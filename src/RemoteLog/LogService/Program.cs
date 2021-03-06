﻿using Agebull.ZeroNet.Core;
using System;

namespace Agebull.ZeroNet.LogService
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello ZeroNet");

            StationProgram.RegisteStation(new RemoteLogRecorder());
            StationProgram.Initialize();
            StationProgram.RunConsole();
        }
    }
}
