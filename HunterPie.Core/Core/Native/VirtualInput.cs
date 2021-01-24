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

        private readonly Dictionary<uint, TaskCompletionSource<bool>> pendingRequests = new Dictionary<uint, TaskCompletionSource<bool>>();

        private VirtualInput()
        {
            Client.Instance.OnQueueInputResponse += Server_OnQueueInputResponse;
        }

        private void Server_OnQueueInputResponse(object sender, S_QUEUE_INPUT e)
        {
            lock (pendingRequests)
            {
                if (pendingRequests.ContainsKey(e.inputId))
                {
                    pendingRequests[e.inputId].SetResult(true);

                    pendingRequests.Remove(e.inputId);
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

        private async Task<bool> InjectInputRaw(
            char[] keys,
            int numberOfFrames = 1,
            bool ignoreOriginalInputs = true)
        {
            uint newInjId;

            TaskCompletionSource<bool> src = new TaskCompletionSource<bool>();
            
            lock (pendingRequests)
            {
                do
                {
                    newInjId = (uint)(new Random().NextDouble() * uint.MaxValue);
                } while (pendingRequests.ContainsKey(newInjId));

                pendingRequests.Add(newInjId, src);
            }

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

            return await src.Task;
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
                    Client.Instance.OnQueueInputResponse -= Server_OnQueueInputResponse;

                    lock (pendingRequests)
                    {
                        foreach (TaskCompletionSource<bool> tk in pendingRequests.Values)
                        {
                            tk.SetResult(false);
                        }
                    }
                    pendingRequests.Clear();

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
