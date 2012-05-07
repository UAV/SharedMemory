using System;
using System.IO;


namespace BoTechnologies.IPC
{
    internal class SharedMemoryStream : UnmanagedMemoryStream
    {
        private IntPtr _ptr;

        /// <summary>
        /// Initializes a new instance of the <see cref="SharedMemoryStream"/> class.
        /// </summary>
        /// <param name="ptr">The PTR.</param>
        /// <param name="size">The size.</param>
        unsafe public SharedMemoryStream(IntPtr ptr, uint size)
            : base((byte*)ptr.ToPointer(), size, size, FileAccess.ReadWrite)
        {            
            _ptr = ptr;
        }

        

        /// <summary>
        /// Unmaps the view of file.
        /// </summary>
        public void UnmapViewOfFile()
        {
            SharedMemory.UnmapViewOfFile(_ptr);
            _ptr = IntPtr.Zero;
        }

        /// <summary>
        /// Closes the current stream and releases any resources (such as sockets and file handles) associated with the current stream.
        /// </summary>
        public override void Close()
        {                    
            base.Close();
        }


        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.IO.UnmanagedMemoryStream"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            UnmapViewOfFile();
            base.Dispose(disposing);
           
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="SharedMemoryStream"/> is reclaimed by garbage collection.
        /// </summary>
        ~SharedMemoryStream()
        {
            Dispose(false);
        }
    };
}

