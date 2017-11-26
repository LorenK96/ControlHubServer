﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Grpc.Core;
using System.Drawing;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Service;

namespace ControlHubServer
{
    public enum InputType
    {
        STANDARD,
        DIRECTINPUT,
        XBOX
    }

    class Settings // Global app settings
    {
        public readonly static bool BUTTON_TOGGLE = false;
        public readonly static InputType INPUT_TYPE = InputType.XBOX;
    }


    public class ControlHubServer
    {
        public string Host { get; set; }
        public int Port { get; private set; }

        private Server server;
        public Xbox360Controller X360Controller { get; set; }

        public ControlHubServer(string host = "localhost", int port = 50051)
        {
            this.Host = host;
            this.Port = port;
        }

        public void Start(InputType inputType)
        {
            if (inputType == InputType.XBOX)
            {
                // TODO: Only connect when prompted
                var client = new ViGEmClient();
                X360Controller = new Xbox360Controller(client);
                X360Controller.Connect();

                server = new Server
                {
                    Services = {
                        XboxButtons.BindService(new XboxImpl(X360Controller))
                    },
                    Ports = { new ServerPort(Host, Port, ServerCredentials.Insecure) }
                };
            }
            else if (inputType == InputType.STANDARD || inputType == InputType.DIRECTINPUT)
            {
                server = new Server
                {
                    Services = {
                        StandardInput.BindService(new StandardInputImpl(inputType))
                    },
                    Ports = { new ServerPort(Host, Port, ServerCredentials.Insecure) }
                };
            }

            server.Start();
        }

        public async void Stop()
        {
            X360Controller.Disconnect();
            await server.ShutdownAsync();
        }
    }
}