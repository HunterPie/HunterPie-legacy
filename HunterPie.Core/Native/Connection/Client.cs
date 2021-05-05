using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using HunterPie.Logger;
using HunterPie.Native.Connection.Packets;
using static HunterPie.Memory.Address;
using System.Collections.Generic;

namespace HunterPie.Native.Connection
{
    public class Client : IDisposable
    {
        private static Client instance;

        public static Client Instance
        {
            get { return instance ?? (instance = new Client()); }
        }

        private const string Address = "127.0.0.1";
        private const int Port = 16969;

        private TcpClient socket;
        private bool disposedValue;

        public bool IsConnected => socket?.Connected ?? false;

        private NetworkStream stream => IsConnected ? socket?.GetStream() : null;

        private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        #region Events

        public event EventHandler<S_QUEUE_INPUT> OnQueueInputResponse;
        public event EventHandler<S_DEAL_DAMAGE> OnDamageDealResponse;

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

            try
            {
                await socket.ConnectAsync(Address, Port);
            } catch (Exception err)
            {
                Debugger.Error(err);
                return false;
            }

            if (IsConnected)
            {
                Debugger.Write($"[Socket] Connected to HunterPie.Native!", "#FFFF59E6");
                Listen();

                await SendAsync(
                        GenerateCConnect()
                    );
            }

            return IsConnected;
        }

        private C_CONNECT GenerateCConnect()
        {
            C_CONNECT connectPacket = new C_CONNECT()
            {
                header = new Header() { opcode = OPCODE.Connect, version = 1 },
                addresses = new UIntPtr[128]
            };

            string[] names =
            {
                "FUN_GAME_INPUT",
                "GAME_INPUT_OFFSET",
                "GAME_HUD_INFO_OFFSET",
                "GAME_CHAT_OFFSET",
                "FUN_CHAT_SYSTEM",
                "FUN_DEAL_DAMAGE"
            };

            for (int i = 0; i < names.Length; i++)
                connectPacket.addresses[i] = (UIntPtr)GetAbsoluteAddress(names[i]);

            return connectPacket;
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
                    await stream.ReadAsync(buffer, 0, buffer.Length);

                    HandlePackets(buffer.Clone() as byte[]);
                }
            });
        }

        private async void HandlePackets(byte[] buffer)
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
                        await SendAsync(enableHooks);
                    }

                    return;
                }
                    
                case OPCODE.Disconnect:
                {
                    Log("Received a S_DISCONNECT");
                    break;
                }

                case OPCODE.QueueInput:
                {
                    Log("Received S_QUEUE_INPUT");
                    S_QUEUE_INPUT pkt = PacketParser.Deserialize<S_QUEUE_INPUT>(buffer);
                    OnQueueInputResponse?.Invoke(this, pkt);
                    break;
                }

                case OPCODE.DealDamage:
                {
                    Log("Received S_DEAL_DAMAGE");
                    S_DEAL_DAMAGE pkt = PacketParser.Deserialize<S_DEAL_DAMAGE>(buffer);
                    OnDamageDealResponse?.Invoke(this, pkt);
                    break;
                }
            }
        }

        internal void Disconnect()
        {
            try
            {
                SendAsync(new C_DISCONNECT
                {
                    header = new Header
                    {
                        opcode = OPCODE.Disconnect,
                        version = 1
                    },
                    message = "<STYL MOJI_LIGHTBLUE_DEFAULT><ICON SLG_NEWS>HunterPie Native</STYL>\nDisconnected.",
                    unk1 = -1,
                    unk2 = 0,
                    unk3 = 0
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    stream?.Close();
                    socket?.Close();
                    socket?.Dispose();
                    instance = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
