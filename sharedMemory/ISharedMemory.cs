using System;


namespace BoTechnologies.IPC
{
    public delegate void ReceivedEventHandler<T>(T item);
    public interface ISharedMemory
    {
        /// <summary>
        /// Closes this instance.
        /// </summary>
        void Close();
        /// <summary>
        /// Occurs when [receive handler].
        /// </summary>
        event ReceivedEventHandler<byte[]> ReceiveHandler;
        /// <summary>
        /// Gets the name of the shared memory.
        /// </summary>
        /// <value>The name of the shared memory.</value>
        string SharedMemoryName { get; }
        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        uint Size { get; }
        /// <summary>
        /// Writes the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        void Write(byte[] buffer);
    }

}
