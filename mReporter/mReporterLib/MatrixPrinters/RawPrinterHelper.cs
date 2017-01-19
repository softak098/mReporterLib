using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace mReporterLib
{
	public class RawPrinterHelper
	{
		// Structure and API declarions:
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		class DOCINFOA
		{
			[MarshalAs(UnmanagedType.LPStr)]
			public string pDocName;
			[MarshalAs(UnmanagedType.LPStr)]
			public string pOutputFile;
			[MarshalAs(UnmanagedType.LPStr)]
			public string pDataType;
		}
          
		[DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pDefault);
         		[DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		static extern bool ClosePrinter(IntPtr hPrinter);
        		[DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

		[DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		static extern bool EndDocPrinter(IntPtr hPrinter);

		[DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		static extern bool StartPagePrinter(IntPtr hPrinter);

		[DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		static extern bool EndPagePrinter(IntPtr hPrinter);

		[DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);



		// SendBytesToPrinter()
		// When the function is given a printer name and an unmanaged array
		// of bytes, the function sends those bytes to the print queue.
		// Returns true on success, false on failure.
		public static bool SendToPrinter(string printerName, IntPtr pBytes, int dwCount)
		{
			Int32 dwError = 0, dwWritten = 0;
			IntPtr hPrinter = new IntPtr(0);
			DOCINFOA di = new DOCINFOA();
			bool result = false; // Assume failure unless you specifically succeed.

			di.pDocName = "mRreporterLib - RAW document";
			di.pDataType = "RAW";

			// Open the printer.
			if (OpenPrinter(printerName.Normalize(), out hPrinter, IntPtr.Zero)) {
				// Start a document.
				if (StartDocPrinter(hPrinter, 1, di)) {
					// Start a page.
					if (StartPagePrinter(hPrinter)) {
						// Write your bytes.
						result = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
						EndPagePrinter(hPrinter);
					}
					EndDocPrinter(hPrinter);
				}
				ClosePrinter(hPrinter);
			}
			// If you did not succeed, GetLastError may give more information
			// about why not.
			if (result == false) {
				dwError = Marshal.GetLastWin32Error();
			}
			return result;
		}

        public static bool SendToPrinter(string printerName, byte[] buffer)
        {
            bool result = false;

            IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(buffer.Length);
            Marshal.Copy(buffer, 0, pUnmanagedBytes, buffer.Length);
            result = SendToPrinter(printerName, pUnmanagedBytes, buffer.Length);
            Marshal.FreeCoTaskMem(pUnmanagedBytes);
            return result;
        }

	}

}
