using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;


namespace BoTechnologies.IPC
{
    /// <summary>
    /// SharedMemory
    /// </summary>
    public class SharedMemory : ISharedMemory, IDisposable
    {
        #region DLL Imports
        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public UIntPtr BaseAddress;
            public UIntPtr AllocationBase;
            public uint AllocationProtect;
            public uint RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }


        /// <summary>
        /// Virtuals the query.
        /// </summary>
        /// <param name="lpAddress">The lp address.</param>
        /// <param name="lpBuffer">The lp buffer.</param>
        /// <param name="dwLength">Length of the dw.</param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        internal static extern int VirtualQuery(ref IntPtr lpAddress,
                                                ref MEMORY_BASIC_INFORMATION lpBuffer,
                                                int dwLength
            );



        /// <summary>
        /// Creates the file mapping.
        /// </summary>
        /// <param name="hFile">The h file.</param>
        /// <param name="lpAttributes">The lp attributes.</param>
        /// <param name="flProtect">The fl protect.</param>
        /// <param name="dwMaximumSizeHigh">The dw maximum size high.</param>
        /// <param name="dwMaximumSizeLow">The dw maximum size low.</param>
        /// <param name="lpName">Name of the lp.</param>
        /// <returns></returns>
        [DllImport ("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr CreateFileMapping (IntPtr hFile,
                                          int lpAttributes,
                                          FileMapProtection flProtect,
                                          uint dwMaximumSizeHigh,
                                          uint dwMaximumSizeLow,
                                          string lpName);


        /// <summary>
        /// Opens the file mapping.
        /// </summary>
        /// <param name="dwDesiredAccess">The dw desired access.</param>
        /// <param name="bInheritHandle">if set to <c>true</c> [b inherit handle].</param>
        /// <param name="lpName">Name of the lp.</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr OpenFileMapping(uint dwDesiredAccess, bool bInheritHandle, string lpName);

        /// <summary>
        /// 
        /// </summary>
        [Flags]
        internal enum FileMapProtection : uint
        {
            /// <summary>
            /// 
            /// </summary>
            PageReadonly = 0x02,
            /// <summary>
            /// 
            /// </summary>
            PageReadWrite = 0x04,
            /// <summary>
            /// 
            /// </summary>
            PageWriteCopy = 0x08,
            /// <summary>
            /// 
            /// </summary>
            PageExecuteRead = 0x20,
            /// <summary>
            /// 
            /// </summary>
            PageExecuteReadWrite = 0x40,
            /// <summary>
            /// 
            /// </summary>
            SectionCommit = 0x8000000,
            /// <summary>
            /// 
            /// </summary>
            SectionImage = 0x1000000,
            /// <summary>
            /// 
            /// </summary>
            SectionNoCache = 0x10000000,
            /// <summary>
            /// 
            /// </summary>
            SectionReserve = 0x4000000,
        }

        /// <summary>
        /// Closes the handle.
        /// </summary>
        /// <param name="hObject">The h object.</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(IntPtr hObject);

        /// <summary>
        /// Unmaps the view of file.
        /// </summary>
        /// <param name="lpBaseAddress">The lp base address.</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

        /// <summary>
        /// Maps the view of file.
        /// </summary>
        /// <param name="hFileMappingObject">The h file mapping object.</param>
        /// <param name="dwDesiredAccess">The dw desired access.</param>
        /// <param name="dwFileOffsetHigh">The dw file offset high.</param>
        /// <param name="dwFileOffsetLow">The dw file offset low.</param>
        /// <param name="dwNumberOfBytesToMap">The dw number of bytes to map.</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject,
                                            FileMapAccess dwDesiredAccess,
                                            uint dwFileOffsetHigh,
                                            uint dwFileOffsetLow,
                                            uint dwNumberOfBytesToMap
            );

        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum FileMapAccess : uint
        {
            /// <summary>
            /// 
            /// </summary>
            FileMapCopy = 0x0001,
            /// <summary>
            /// 
            /// </summary>
            FileMapWrite = 0x0002,
            /// <summary>
            /// 
            /// </summary>
            FileMapRead = 0x0004,
            /// <summary>
            /// 
            /// </summary>
            FileMapAllAccess = 0x001f,
            /// <summary>
            /// 
            /// </summary>
            fileMapExecute = 0x0020,
        }
        #endregion

        #region Constants
        const UInt32 STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        const UInt32 SECTION_QUERY = 0x0001;
        const UInt32 SECTION_MAP_WRITE = 0x0002;
        const UInt32 SECTION_MAP_READ = 0x0004;
        const UInt32 SECTION_MAP_EXECUTE = 0x0008;
        const UInt32 SECTION_EXTEND_SIZE = 0x0010;
        const UInt32 SECTION_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED |
                                            SECTION_QUERY |
                                            SECTION_MAP_WRITE |
                                            SECTION_MAP_READ |
                                            SECTION_MAP_EXECUTE |
                                            SECTION_EXTEND_SIZE
                                            );

        const UInt32 FILE_MAP_ALL_ACCESS = SECTION_ALL_ACCESS;

        #endregion

        #region Private

        private SharedMemoryHandle _sharedMemoryHandle;
        private Stream _stream;
        private bool _isThreadCreated;
        private EventWaitHandle _eventWaitHandle;

        /// <summary>
        /// Creates the or open.
        /// </summary>
        private void CreateOrOpen()
        {
            IntPtr hHandle = CreateFileMapping(new IntPtr(-1), 
                                                0, 
                                                FileMapProtection.PageReadWrite,
                                                0, 
                                                Size,
                                                SharedMemoryName);
            _sharedMemoryHandle = new SharedMemoryHandle(hHandle);
            if (_sharedMemoryHandle.IsInvalid)
            {
                throw new InvalidOperationException("Failed to create section object");
            }
        }
        #endregion

        /// <summary>
        /// Occurs when [_receive handler].
        /// </summary>
        private event ReceivedEventHandler<byte[]> _receiveHandler;

        /// <summary>
        /// Occurs when [receive handler].
        /// </summary>
        public event ReceivedEventHandler<byte[]> ReceiveHandler
        {
            add 
            { 
                _receiveHandler += value;
                if (!_isThreadCreated)
                {
                    _isThreadCreated = true;
                    ThreadPool.QueueUserWorkItem(ReceiverCallback);                    
                }
            }
            remove 
            { 
                _receiveHandler -= value;
                if (_receiveHandler == null)
                {
                    _isThreadCreated = false;
                }
            }
        }

        /// <summary>
        /// Receivers the callback.
        /// </summary>
        /// <param name="obj">The obj.</param>
        private void ReceiverCallback(object obj)
        {           
            while (_isThreadCreated)
            {
                try
                {
                    _eventWaitHandle.WaitOne();
                
                    if (_receiveHandler != null)
                    {
                        byte[] buffer = new byte[Size];
                        ShareMemoryStream.Position = 0;
                        ShareMemoryStream.Read(buffer, 0, buffer.Length);
                        _receiveHandler(buffer);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                }
                if (!_eventWaitHandle.SafeWaitHandle.IsClosed)
                {
                    _eventWaitHandle.Reset();
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the shared memory.
        /// </summary>
        /// <value>The name of the shared memory.</value>
        public string SharedMemoryName { get; protected set; }


        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>The size.</value>
        public uint Size { get; protected set; }

        /// <summary>
        /// Gets the share memory stream.
        /// </summary>
        /// <value>The share memory stream.</value>
        internal Stream ShareMemoryStream
        {
            get
            {
                return _stream;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SharedMemory"/> class.
        /// </summary>
        /// <param name="sharedMemoryName">Name of the shared memory.</param>
        /// <param name="numberOfBytes">The number of bytes.</param>
        public SharedMemory(string sharedMemoryName, uint numberOfBytes)
        {
            _eventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, sharedMemoryName + "Event");
            Size = numberOfBytes;
            SharedMemoryName = sharedMemoryName.Trim();
            CreateOrOpen();
            MapView();
        }

        /// <summary>
        /// Maps the view.
        /// </summary>
        private void MapView()
        {
            IntPtr ptr = MapViewOfFile(_sharedMemoryHandle.GetHandle(),
                                    FileMapAccess.FileMapRead | FileMapAccess.FileMapWrite,
                                    (uint)0,
                                    0,
                                    Size);
            if (ptr == IntPtr.Zero)
            {
                throw new InvalidOperationException("Invalid Handle. Filed to map view");
            }
            if (Size == 0)
            {
                MEMORY_BASIC_INFORMATION info = new MEMORY_BASIC_INFORMATION();
                
                VirtualQuery(ref ptr, ref info, (int)System.Runtime.InteropServices.Marshal.SizeOf(info));
                Size = info.RegionSize;
            }

            _stream = new SharedMemoryStream(ptr, Size);
        }

        /// <summary>
        /// Writes the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        public void Write(byte[] buffer)
        {
            try
            {
                ShareMemoryStream.Position = 0;
                ShareMemoryStream.Write(buffer, 0, buffer.Length);
                if (!_eventWaitHandle.SafeWaitHandle.IsClosed)
                {                   
                    _eventWaitHandle.Set();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        #region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _isThreadCreated = false;
                    if (!_eventWaitHandle.SafeWaitHandle.IsClosed)
                    {
                        _eventWaitHandle.Set();
                    }
                    System.Threading.Thread.Sleep(100);
                    _eventWaitHandle.Close();
                    _stream.Close();
                    _sharedMemoryHandle.Close();
                }
            }
            _disposed = true;
        }

        private bool _disposed = false;
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="SharedMemory"/> is reclaimed by garbage collection.
        /// </summary>
        ~SharedMemory()
        {
            Dispose(false);
        }

        #endregion

 
    }
}

/*@}*/