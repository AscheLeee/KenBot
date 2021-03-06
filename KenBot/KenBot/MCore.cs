﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace KenBotClient
{
    public class MCore
    {
        public MChannel Channel = new MChannel();
        public MBot Bot = new MBot();
        public MIO IO = new MIO();
        public MStreamer Streamer = new MStreamer();
        public MOTD MessageOfTheDay = new MOTD();

        public TimeSpan JoinChannelTimeoutDuration = new TimeSpan(0, 0, 10);
        public bool DebugMode = false;

        public static string Command = string.Empty;

        public string ConnectToIRC()
        {
            if (string.IsNullOrWhiteSpace(Bot.OAuthToken))
                return "'User.OAuthToken' in MCore.ConnectToIRC is NULL/empty.";
            if (string.IsNullOrWhiteSpace(Bot.UserName))
                return "'User.Name' in MCore.ConnectToIRC is NULL/empty.";

            Command = string.Format("PASS {0}\r\nNICK {1}\r\n", Bot.OAuthToken, Bot.UserName);
            IO.AttemptWrite(Command);

            if (DebugMode)
            {
                return string.Concat(Command, "\r\n", IO.AttemptRead());
            }
            else
                return IO.AttemptRead();
        }

        public string JoinChannel(string _Channel, MIO IO)
        {
            Command = string.Empty;
            if (!_Channel[0].Equals('#'))
                _Channel.Insert(0, "#");
            if (string.IsNullOrWhiteSpace(_Channel))
                return "'_Channel' in MCore.JoinChannel is NULL/empty.";

            Channel.Name = _Channel;
            Command = string.Format("JOIN {0}\r\n", _Channel); //Channel.Name must be prefixed with '#'.
            IO.AttemptWrite(Command);
            //Update(Response);
            if (DebugMode)
            {
                return string.Concat(Command, "\r\n", IO.AttemptRead());
            }
            string Response = IO.AttemptRead();

            Stopwatch TimeoutTimer = new Stopwatch();
            TimeoutTimer.Start();

            while (!Response.Contains("353")
                && TimeoutTimer.Elapsed.Seconds <= JoinChannelTimeoutDuration.Seconds)
            {
                string Temp = IO.AttemptRead();
                if (!Temp.Substring(0, 2).Equals("No"))
                    Response += IO.AttemptRead();
                Thread.Sleep(100);
            }

            TimeoutTimer.Stop();
            return Response;
        }

        /*public void GatherChannelInfo(string JoinChannelResponse)
        {
            string[] lines =
                //JoinChannelResponse.Remove(0, JoinChannelResponse.IndexOf("353:"))
                //.Remove(JoinChannelResponse.IndexOf("366:"))
                JoinChannelResponse.Split("\r\n".ToCharArray())
                .ToArray();
            //353: = #cn firstUserHere1
            //01234567  8
            int startIndex = 8 + Channel.Name.Length;
            string[] Viewers;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[0].Substring(0, 3).Equals("353"))
                {
                    Viewers = lines[i].Remove(0, startIndex).Split(' ');
                    for (int j = 0; j < Viewers.Length; j++)
                    {
                        Channel.Viewers.Add(Viewers[j]);
                    }
                }
            }
        }*/

        public string ListMods()
        {
            Command = string.Format("PRIVMSG {0} :{1}\r\n", Channel.Name, ".mods");
            IO.AttemptWrite(Command);

            Stopwatch TimeoutTimer = new Stopwatch();
            TimeSpan TimeoutDuration = new TimeSpan(0, 0, 5);

            string Response = IO.AttemptRead();
            List<string> ResponseParts = Response.Split("\r\n".ToArray()).ToList();

            TimeoutTimer.Start();

            //:tmi.twitch.tv NOTICE #moeyyas :The moderators of this room are: kenbawt
            while (ResponseParts.Last().Length > 57
                && !ResponseParts.Last().Substring(25 + Channel.Name.Length, 32).Equals("The moderators of this room are:")
                && TimeoutTimer.Elapsed.Seconds <= TimeoutDuration.Seconds)
            {
                string LastMessage = IO.AttemptRead();
                if (!LastMessage.Equals("No data available in NetworkStream to read."))
                {
                    Response += IO.AttemptRead();
                    ResponseParts.Concat(Response.Split("\r\n".ToArray()).ToList());
                }
            }

            TimeoutTimer.Stop();

            if (DebugMode)
                return string.Concat(Command, "\r\n", Response);
            return Response;
        }

        public void SendMessage(string _Message, string _ChannelName)
        {
            Command = string.Empty;
            if (!_ChannelName[0].Equals('#')) //Channel.Name must be prefixed with '#'.
                _ChannelName.Insert(0, "#");
            /*
            if (string.IsNullOrWhiteSpace(_Message))
                return "'_Message' in MCore.SendChannelMessage is NULL/empty.";
            if (string.IsNullOrWhiteSpace(_ChannelName))
                return "'_Recipient' in MCore.SendChannelMessage is NULL/empty.";
            */
            if (!string.IsNullOrWhiteSpace(_Message)
             && !string.IsNullOrWhiteSpace(_ChannelName))
                Command = string.Format("PRIVMSG {0} :{1}\r\n", _ChannelName, _Message);
            IO.AttemptWrite(Command);
            //return Command;
        }

        public void SendMessage(string _Message, string _ChannelName, string _Recipient)
        {
            Command = string.Empty;
            if (!_ChannelName[0].Equals('#')) //Channel.Name must be prefixed with '#'.
                _ChannelName.Insert(0, "#");
            /*
            if (string.IsNullOrWhiteSpace(_Message))
                return "'_Message' in MCore.SendPrivateMessage is NULL/empty.";
            if (string.IsNullOrWhiteSpace(_Recipient))
                return "'_Recipient' in MCore.SendPrivateMessage is NULL/empty.";
            */
            if (!string.IsNullOrWhiteSpace(_Message)
             && !string.IsNullOrWhiteSpace(_ChannelName)
             && !string.IsNullOrWhiteSpace(_Recipient))
                Command = string.Format("PRIVMSG {0} :/w {1} {2}\r\n", _ChannelName, _Recipient, _Message);
            IO.AttemptWrite(Command);
            //return Command;
        }
    }
}
