using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using HunterPie.Logger;
using HunterPie.Native.Connection.Packets;
using HunterPie.Core.Native;

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

        private NetworkStream stream => IsConnected ? socket?.GetStream() : null;

        private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        #region Events

        public event EventHandler<S_QUEUE_INPUT> OnQueueInputResponse;

        #endregion

        #region Wrappers

        internal static async Task<bool> Initialize()
        {
            return await Instance.Connect();
        }

        public static async Task<bool> ToServer<T>(T data) where T : struct
        {
            return await Instance.SendAsync(data);
        }

        #endregion

        private async Task<bool> Connect()
        {
            if (IsConnected)
                return true;

            socket = new TcpClient();

            await socket.ConnectAsync(address, port);

            if (IsConnected)
            {
                Debugger.Write($"[Socket] Connected to HunterPie.Native!", "#FFFF59E6");
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
            } else
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
            } catch (Exception err)
            {
                Debugger.Error(err);
            } finally
            {
                semaphoreSlim.Release();
            }

            return false;
        }

        private void Listen()
        {
            Task.Run(async () =>
            {
                byte[] buffer = new byte[8192];
                while (IsConnected)
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
                {
                    Log("Received a S_CONNECT");
                    S_CONNECT pkt = PacketParser.Deserialize<S_CONNECT>(buffer);
                    
                    if (Injector.CheckIfCRCBypassExists())
                    {
                        C_ENABLE_HOOKS enableHooks = new C_ENABLE_HOOKS
                        {
                            header = new Header { opcode = OPCODE.EnableHooks, version = 1}
                        };
                        SendAsync(enableHooks);
                    }

                    return;
                }
                    
                case OPCODE.Disconnect:
                    Log("Received a S_DISCONNECT");
                    break;
                case OPCODE.QueueInput:
                {
                    Log("Received S_QUEUE_INPUT");
                    S_QUEUE_INPUT pkt = PacketParser.Deserialize<S_QUEUE_INPUT>(buffer);
                    OnQueueInputResponse?.Invoke(this, pkt);
                    break;
                }
                    
            }
        }

        internal void Disconnect()
        {
            try
            {
                Chat.SystemMessage("<SIZE 30><STYL MOJI_YELLOW_DEFAULT>HunterPie Native</STYL>\nDisconnected.", -1, 0, 0);
                SendAsync(new C_DISCONNECT
                {
                    header = new Header
                    {
                        opcode = OPCODE.Disconnect,
                        version = 1
                    }
                }).RunSynchronously();
            } catch {}
            finally
            {
                stream?.Close();
                socket?.Close();
                socket?.Dispose();
            }
            
        }

        private void Log(object message)
        {
            #if DEBUG
            Debugger.Write($"[Socket] {message}", "#FFFF59E6");
            #endif
        }
    }
}
