﻿using System;
using System.IO;
using System.Text;
using EemRdx.Networking;
using Sandbox.ModAPI;
using VRage.Game;

namespace EemRdx.Utilities
{
    public interface ILog
    {
        void WriteToLog(string caller, string message, bool showOnHud = false, int duration = 5000, string color = "Green");
        void LogError(string source, string message, Exception Scrap);
        void GetTailMessages();
    }

    public class Log : ILog
    {
		private string LogName { get; set; }
		
		private TextWriter TextWriter { get; set; }

		private static string TimeStamp => DateTime.Now.ToString("MMddyy-HH:mm:ss:ffff");

		private readonly Queue<string> _messageQueue = new Queue<string>(20);

		private const int DefaultIndent = 4;

		private static string Indent { get; } = new string(' ', DefaultIndent);

		public Log(string logName)
		{
			LogName = logName + ".log";
			Init();
		}

		private void Init()
		{
			if (TextWriter != null) return;
			TextWriter = MyAPIGateway.Utilities.WriteFileInLocalStorage(LogName, typeof(Log));
		}

		public void Close()
		{
			TextWriter?.Flush();
			TextWriter?.Close();
			TextWriter = null;
		}

		public void WriteToLog(string caller, string message, bool showOnHud = false, int duration = 3000, string color = MyFontEnum.Green)
		{
			BuildLogLine(caller, message);
			if (!showOnHud) return;
			BuildHudNotification(caller, message, duration, color);
		}

        public void LogError(string source, string message, Exception Scrap)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"Exception occurred in {source}{(!string.IsNullOrEmpty(message) ? $" ({message})" : "")}:");
            builder.AppendLine(Scrap.Message);
            if (Scrap.InnerException != null)
                builder.AppendLine(Scrap.InnerException.Message);
            else
                builder.AppendLine($"No inner exception was provided");
            builder.AppendLine(Scrap.StackTrace);
            WriteToLog(source, builder.ToString());
        }

        public void GetTailMessages()
		{
			//TextReader fileInLocalStorage = MyAPIGateway.Utilities.ReadFileInLocalStorage(LogName, typeof(Logger));
			MyAPIGateway.Utilities.ShowMissionScreen(LogName, "", "", string.Join(Environment.NewLine, _messageQueue.GetQueue()));
			//IMyHudObjectiveLine debugLines = new MyHudObjectiveLine
			//{
			//	Title = LogName,
			//	Objectives = MessageQueue.GetQueue().ToList()
			//};
			//debugLines.Show();
			//MyAPIGateway.Utilities.GetObjectiveLine();
		}

		private static void BuildHudNotification(string caller, string message, int duration, string color)
		{
			Messaging.ShowLocalNotification($"{caller}{Indent}{message}", duration, color);
		}

		private void BuildLogLine(string caller, string message)
		{
			WriteLine($"{TimeStamp}{Indent}{caller}{Indent}{message}");
		}
		
		private void WriteLine(string line)
		{
			_messageQueue.Enqueue(line);
		 	TextWriter.WriteLine(line);
			TextWriter.Flush();
		}
	}
}
