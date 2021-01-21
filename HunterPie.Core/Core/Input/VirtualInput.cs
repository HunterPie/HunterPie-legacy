using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HunterPie.Memory;
using HunterPie.Native.Connection;
using HunterPie.Native.Connection.Packets;
using HunterPie.Logger;
using static HunterPie.Memory.WindowsHelper;
// ReSharper disable All

namespace HunterPie.Core.Input
{
    public class VirtualInput : IDisposable
    {
        private static VirtualInput instance;
        private bool disposedValue;

        public static VirtualInput Instance
        {
            get { return instance ?? (instance = new VirtualInput());}
        }

        private readonly Dictionary<uint, CancellationTokenSource> virtualInputTasks = new Dictionary<uint, CancellationTokenSource>();

        private VirtualInput()
        {
            Client.Instance.OnQueueInputResponse += Server_OnQueueInputResponse;
        }

        private void Server_OnQueueInputResponse(object sender, S_QUEUE_INPUT e)
        {
            lock (virtualInputTasks)
            {
                if (virtualInputTasks.ContainsKey(e.inputId))
                {
                    virtualInputTasks[e.inputId].Cancel();

                    virtualInputTasks.Remove(e.inputId);
                }
            }
            Debugger.Log($"Finished injecting {e.inputId}");
        }

        /// <summary>
        /// Sends a string to Monster Hunter: World
        /// </summary>
        /// <param name="characters">Text to be sent</param>
        public static void SendText(string characters)
        {
            if (characters is null)
                return;
            
            foreach (char c in characters)
                SendMessage(Kernel.WindowHandle, WMessages.WM_CHAR, c, IntPtr.Zero);
        }

        public Task PressKey(char key) 
        {
            return InjectInputRaw(new [] { key });
        }

        private Task InjectInputRaw(
            char[] keys,
            int numberOfFrames = 1,
            bool ignoreOriginalInputs = true)
        {
            uint newInjId = (uint)(new Random().NextDouble() * uint.MaxValue);
            CancellationTokenSource src = new CancellationTokenSource();
            
            lock (virtualInputTasks)
            {
                do
                {
                    newInjId = (uint)(new Random().NextDouble() * uint.MaxValue);
                } while (virtualInputTasks.ContainsKey(newInjId));

                virtualInputTasks.Add(newInjId, src);
            }

            Task newTsk = Task.Run(async () => {
                C_QUEUE_INPUT packet = new C_QUEUE_INPUT()
                {
                    header = new Header() { opcode = OPCODE.QueueInput, version = 1 },
                    inputs = new input()
                    {
                        inputArray = CalculateInputs(keys),
                        nFrames = numberOfFrames,
                        injectionId = newInjId,
                        ignoreOriginalInputs = ignoreOriginalInputs
                    }
                };
                await Client.ToServer(packet);
                
                try
                {
                    await Task.Delay(-1, src.Token);
                } catch { }

            });

            return newTsk;
        }

        private static byte[] CalculateInputs(char[] keys)
        {
            byte[] bits = new byte[32];
            foreach (char key in keys)
            {
                bits[key >> 3] |= (byte)(1 << (key % 8));
            }
            return bits;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
