using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using HunterPie.Logger;
using HunterPie.Native.Connection.Packets;

namespace HunterPie.Native.Connection
{
    public class Client
    {
        private static Client instance;

        public static Client Instance
        {
            get { return instance ?? (instance = new Client()); }
        }

        private const string address = "127.0.0.1";
        private const int port = 16969;

        private TcpClient socket;

        public bool IsConnected => socket?.Connected ?? false;

        private NetworkStream stream => socket?.GetStream();

        private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public async Task<bool> Connect()
        {
            if (IsConnected)
                return true;

            socket = new TcpClient();

            await socket.ConnectAsync(address, port);

            Listen();

            if (IsConnected)
            {
                Log("Connected to HunterPie.Native!");
                Listen();
                C_CONNECT connectPacket = new C_CONNECT()
                {
                    header = new Header() {opcode = OPCODE.Connect, version = 1},
                    hunterpiePath = AppDomain.CurrentDomain.BaseDirectory
                };

                await SendAsync(connectPacket);
            }

            return IsConnected;
        }

        public async Task<bool> SendAsync<T>(T packet) where T : struct
        {
            FieldInfo packetType = packet.GetType().GetField("header");

            if (packetType?.FieldType == typeof(Header))
            {
                byte[] buffer = PacketParser.Serialize(packet);
                return await SendRawAsync(buffer);
            }
            else
            {
                Debugger.Error("Invalid packet");
                return false;
            }
        }

        private async Task<bool> SendRawAsync(byte[] buffer)
        {
            if (!IsConnected)
                return false;

            try
            {
                await semaphoreSlim.WaitAsync();

                await stream.WriteAsync(buffer, 0, buffer.Length);
                
                return true;
            }
            catch (Exception err)
            {
                Debugger.Error(err);
            }
            finally
            {
                semaphoreSlim.Release();
            }

            return false;
        }

        private void Listen()
        {
            Log("Listening for data!");
            Task.Run(async () =>
            {
                byte[] buffer = new byte[8192];
                while (true)
                {
                    if (stream.DataAvailable)
                    {
                        await stream.ReadAsync(buffer, 0, buffer.Length);
                        HandlePackets(buffer);
                        Array.Clear(buffer, 0, buffer.Length);
                    }
                    await Task.Delay(16);
                }
            });
        }

        private void HandlePackets(byte[] buffer)
        {
            Header packetHeader = PacketParser.Deserialize<Header>(buffer);

            switch (packetHeader.opcode)
            {
                case OPCODE.Connect:
                    Log("Received a S_CONNECT");
                    return;
            }
        }

        private void Log(object message)
        {
            Debugger.Write($"[Socket] {message}", "#FFFF59E6");
        }
    }
}
