using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace AsyncStream
{
    [Serializable]
    public class AsyncStreamReader : IDisposable
    {
        #region Event declarations
        /// <summary>
        /// Occurs when the byte is readed from the System.IO.StreamReader.
        /// </summary>
        public event EventHandler<AsyncReadEventArgs> OnReadedBytes;
        /// <summary>
        /// Occurs when the all bytes are readed from the System.IO.StreamReader.
        /// </summary>
        public event EventHandler<AsyncReadEventArgs> OnEndRead;
        /// <summary>
        /// Occurs when an AsyncStream.AsyncExcpetion is happent.
        /// </summary>
        public event EventHandler<AsyncReadErrorEventArgs> OnError;
        /// <summary>
        /// Occurs when state of AsyncStream.AsyncStreamReader is changed.
        /// </summary>
        public event EventHandler<AsyncStreamStateChangeArgs> OnStateChanged;
        #endregion

        private Thread worker;
        private StreamReader reader;
        private long streamLenght;
        private int byteBuffer;
        private byte[] buffer;

        private AsyncStreamState state = AsyncStreamState.Ready;
        /// <summary>
        /// Gets the current state of AsyncStreamReader.AsyncStreamReader.
        /// </summary>
        public AsyncStreamState State
        {
            get { return state; }
        }

        private string path;
        /// <summary>
        /// Gets the complete file path to be read.
        /// </summary>
        public string Path
        {
            get { return path; }
        }

        /// <summary>
        /// Implements a System.IO.TextReader that reads characters from a byte stream
        /// in a particular encoding.
        /// </summary>
        /// <param name="path">The complete file path to be read.</param>
        public AsyncStreamReader(string path)
        {
            this.path = path;
        }


        /// <summary>
        /// Begins an asynchronous read operation.
        /// </summary>
        public void BeginRead()
        {
            byteBuffer = 0;

            reader = new StreamReader(path);
            streamLenght = reader.BaseStream.Length;

            buffer = new byte[streamLenght];

            worker = new Thread(new ThreadStart(beginRead)); 
            worker.Start();
        }

        /// <summary>
        /// Pause an asynchronous read operation.
        /// </summary>
        public void PauseRead()
        {
            changeState(AsyncStreamState.Paused);
            worker.Suspend();
        }

        /// <summary>
        /// Resume an paused asynchronous read operation.
        /// </summary>
        public void ResumeRead()
        {
            changeState(AsyncStreamState.Started);
            worker.Resume();
        }

        private void beginRead()
        {
            try
            {
                int position = 0;
                changeState(AsyncStreamState.Started);


                while (byteBuffer > -1)
                {
                    byteBuffer = reader.BaseStream.ReadByte();
                    if (byteBuffer != -1)
                    {
                        buffer[position] = (byte)byteBuffer;
                    }

                    if (OnReadedBytes != null && byteBuffer != -1)
                    {
                        OnReadedBytes(null, new AsyncReadEventArgs(position, streamLenght));
                    }
                    else if (OnEndRead != null && byteBuffer == -1)
                    {
                        OnEndRead(null, AsyncReadEventArgs.EndArgs(buffer));

                        reader.Close();
                        reader.Dispose();
                        worker.Abort();
                    }
                    position++;
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                changeState(AsyncStreamState.Error);
                AsyncExcpetion exc = new AsyncExcpetion("Error async reading from: " + path + ".", e);

                if (OnError != null)
                {
                    OnError(null, new AsyncReadErrorEventArgs(exc));
                }
                else
                    throw exc;

                this.StopRead(true);
                changeState(AsyncStreamState.Error);
            }
            finally
            {
                this.readFinished();
            }
        }

        /// <summary>
        /// Stops an asynchronous read operation.
        /// </summary>
        public void StopRead()
        {
            StopRead(false);
        }

        /// <summary>
        /// Stops an asynchronous read operation.
        /// </summary>
        /// <param name="cleareStream">whether to clear strem from memory</param>        
        public void StopRead(bool clearStream)
        {
            changeState(AsyncStreamState.Stoped);
            if (worker != null)
                worker.Abort();

            if (clearStream && reader != null)
            {
                if (reader.BaseStream != null)
                {
                    reader.BaseStream.Flush();
                    reader.BaseStream.Close();
                }

                reader.Close();
                reader.Dispose();
            }
        }

        private void readFinished()
        {
            changeState(AsyncStreamState.Finished);
            if (worker != null)
                worker.Abort();

            if (reader != null)
            {
                if (reader.BaseStream != null)
                    reader.BaseStream.Flush();

                reader.Close();
                reader.Dispose();
            }
        }

        private void changeState(AsyncStreamState state)
        {
            this.state = state;
            if (OnStateChanged != null)
                OnStateChanged(null, new AsyncStreamStateChangeArgs(state));
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
            }
            if (worker != null)
            {
                worker.Abort();
            }
        }

        #endregion
    }
}
