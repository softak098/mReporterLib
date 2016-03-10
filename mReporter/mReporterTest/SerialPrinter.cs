using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mReporterLib.Tests
{
    class SerialPrinter
    {
        string _data;
        Encoding _encoding;

        public SerialPrinter(string data,Encoding encoding)
        {
            _data = data;
            _encoding = encoding;
        }

        public void Print(string comPort)
        {
            SerialPort port = new SerialPort(comPort);

            byte[] data = _encoding.GetBytes(_data);

            if (port.IsOpen) port.Close();
            port.Open();
            try {
                port.Write(data, 0, data.Length);

            }
            finally {
                port.Close();
            }

        }

    }
}
