using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;


namespace BoTechnologies.IPC
{
    class SharedMemoryHandle : CriticalHandleMinusOneIsInvalid
    {
        /// <summary>
        /// Closes the handle.
        /// </summary>
        /// <param name="hObject">The h object.</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        /// <summary>
        /// When overridden in a derived class, executes the code required to free the handle.
        /// </summary>
        /// <returns>
        /// true if the handle is released successfully; otherwise, in the event of a catastrophic failure, false. In this case, it generates a ReleaseHandleFailed Managed Debugging Assistant.
        /// </returns>
        protected override bool ReleaseHandle()
        {
            return CloseHandle(handle) ? true : false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SharedMemoryHandle"/> class.
        /// </summary>
        /// <param name="h">The h.</param>
        public SharedMemoryHandle(IntPtr h)
        {            
            handle = h;
        }

        /// <summary>
        /// Gets a value that indicates whether the handle is invalid.
        /// </summary>
        /// <value></value>
        /// <returns>true if the handle is not valid; otherwise, false.</returns>
        public override bool IsInvalid
        {
            get
            {
                return handle == IntPtr.Zero;
            }
        }

        /// <summary>
        /// Gets the handle.
        /// </summary>
        /// <returns></returns>
        internal IntPtr GetHandle()
        {
            return handle;
        }
    }
}

