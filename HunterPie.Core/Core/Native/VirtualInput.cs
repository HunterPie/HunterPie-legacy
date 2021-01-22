using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HunterPie.Native.Connection;
using HunterPie.Native.Connection.Packets;
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
                    virtualInputTasks[e.inputId].Dispose();

                    virtualInputTasks.Remove(e.inputId);
                }
            }
        }

        /// <summary>
        /// Press a single key for one frame
        /// </summary>
        /// <param name="key">Key Code</param>
        /// <param name="ignoreOriginal">Whether original inputs should be overwritten</param>
        /// <returns>An awaitable task</returns>
        public static Task PressKey(char key, bool ignoreOriginal = true) 
        {
            return Instance.InjectInputRaw(new [] { key }, ignoreOriginalInputs: ignoreOriginal);
        }

        /// <summary>
        /// Press multiple keys for one frame
        /// </summary>
        /// <param name="keys">key codes</param>
        /// <param name="ignoreOriginal">Whether original inputs should be overwritten</param>
        /// <returns>An awaitable task</returns>
        public static Task PressKeys(char[] keys, bool ignoreOriginal = true)
        {
            return Instance.InjectInputRaw(keys, ignoreOriginalInputs: ignoreOriginal);
        }

        /// <summary>
        /// Holds one key for multiple frames
        /// </summary>
        /// <param name="key">key code</param>
        /// <param name="frameCount">Number of frames to hold inputs for</param>
        /// <param name="ignoreOriginal">Whether original inputs should be overwritten</param>
        /// <returns></returns>
        public static Task HoldKey(char key, int frameCount, bool ignoreOriginal = true)
        {
            return Instance.InjectInputRaw(new [] { key }, frameCount, ignoreOriginal);
        }

        /// <summary>
        /// Holds multiple keys for multiple frames
        /// </summary>
        /// <param name="keys">key codes</param>
        /// <param name="frameCount">Number of frames to hold inputs for</param>
        /// <param name="ignoreOriginal">Whether original inputs should be overwritten</param>
        /// <returns></returns>
        public static Task HoldKeys(char[] keys, int frameCount, bool ignoreOriginal = true)
        {
            return Instance.InjectInputRaw(keys, frameCount, ignoreOriginal);
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
                    lock (virtualInputTasks)
                    {
                        foreach (CancellationTokenSource tk in virtualInputTasks.Values)
                        {
                            tk.Cancel();
                            tk.Dispose();
                        }
                    }
                    virtualInputTasks.Clear();

                    Client.Instance.OnQueueInputResponse -= Server_OnQueueInputResponse;
                    instance = null;
                }
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
