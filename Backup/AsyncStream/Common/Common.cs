using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsyncStream
{
    #region State enum

    /// <summary>
    /// Specifies identifiers to indicate the the state of AsyncStream.AsyncStreamReader.
    /// </summary>
    public enum AsyncStreamState
    { 
        None,
        /// <summary>
        /// This state occurs when AsyncStream.AsyncStreamReader is ready to start read.
        /// </summary>
        Ready,
        /// <summary>
        /// This state occurs when AsyncStream.AsyncStreamReader read is started.
        /// </summary>
        Started,
        /// <summary>
        /// This state occurs when AsyncStream.AsyncStreamReader read is paused.
        /// </summary>
        Paused,
        /// <summary>
        /// This state occurs when AsyncStream.AsyncStreamReader read is stoped.
        /// </summary>
        Stoped,
        /// <summary>
        /// This state occurs when AsyncStream.AsyncStreamReader read is finished.
        /// </summary>
        Finished,
        /// <summary>
        /// This state occurs when an AsyncStream.AsyncStreamReader exception is happent.
        /// </summary>
        Error
    }

    #endregion

    #region Event args

    /// <summary>
    /// Provides data for the AsyncStream.AsyncStreamReader class event on state change.
    /// </summary>
    public class AsyncStreamStateChangeArgs : EventArgs
    {
        private AsyncStreamState currentState = AsyncStreamState.Ready;
        /// <summary>
        /// Gets current object AsyncStream.AsyncStreamReader state.
        /// </summary>
        public AsyncStreamState CurrentState
        {
            get { return currentState; }
        }
        /// <summary>
        /// Initializes a new instance of AsyncStream.AsyncStreamStateChangeArgs class.
        /// </summary>
        /// <param name="state">The state that have been changed.</param>
        public AsyncStreamStateChangeArgs(AsyncStreamState state)
        {
            this.currentState = state;
        }
    }

    /// <summary>
    /// Provides data for the AsyncStream.AsyncReadEventArgs class event on async read.
    /// </summary>
    public class AsyncReadEventArgs : EventArgs
    {
        private long bytesReaded;
        /// <summary>
        /// Gets the number of bytes that have been readed.
        /// </summary>
        public long BytesReaded
        {
            get { return bytesReaded; }
        }

        private int percentReaded;
        /// <summary>
        /// Gets percent of current readed bytes.
        /// </summary>
        public int PercentReaded
        {
            get { return percentReaded; }
        }

        private long length;
        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        public long Length
        {
            get { return length; }
        }

        private bool isComplete;
        /// <summary>
        /// Gets value that indicating whether the currend stream read is complete.
        /// </summary>
        public bool IsComplete
        {
            get { return isComplete; }
            set { isComplete = value; }
        }

        private byte[] result;
        /// <summary>
        /// Gets current readed strem byte array.
        /// </summary>
        public byte[] Result
        {
            get { return result; }
            set { result = value; }
        }

        /// <summary>
        /// Initializes a new instance of AsyncStream.AsyncReadEventArgs class.
        /// </summary>
        /// <param name="bytesReaded">How many bytes have been readed till now.</param>
        /// <param name="length">Total stream length.</param>
        public AsyncReadEventArgs(long bytesReaded, long length)
        {
            this.bytesReaded = bytesReaded;
            this.length = length;
            this.isComplete = false;

            try
            {
                int p = (int)length / 100;
                this.percentReaded = (int)bytesReaded / p;
                if (this.percentReaded > 100)
                    this.percentReaded = 100;
            }
            catch (DivideByZeroException) { this.percentReaded = 100; }
        }

        /// <summary>
        /// Initializes a new instance of AsyncStream.AsyncReadEventArgs class with result when readed is finished.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static AsyncReadEventArgs EndArgs(byte[] result)
        {
            AsyncReadEventArgs args = new AsyncReadEventArgs(0, 0);
            args.IsComplete = true;
            args.Result = result;
            return args;
        }
    }

    /// <summary>
    /// Provides data for the AsyncStream.AsyncStreamReader class event on read error.
    /// </summary>
    public class AsyncReadErrorEventArgs : EventArgs
    {
        private AsyncExcpetion error;
        /// <summary>
        /// Gets AsyncExcpetion excpetion data.
        /// </summary>
        public AsyncExcpetion Error
        {
            get { return error; }
        }
        /// <summary>
        /// Initializes a new instance of AsyncStream.AsyncReadErrorEventArgs class.
        /// </summary>
        /// <param name="e">AsyncException that have been happen.</param>
        public AsyncReadErrorEventArgs(AsyncExcpetion e)
        {
            error = e;
        }
    }

    #endregion

    /// <summary>
    /// Represents errors that occur during reading.
    /// </summary>
    public class AsyncExcpetion : Exception
    {
        public AsyncExcpetion() : base() { }
        public AsyncExcpetion(string message) : base(message) { }
        public AsyncExcpetion(string message, Exception innerException) : base(message, innerException) { }
    }

    
}
