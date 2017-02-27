using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace mReporterTest
{

    class LPDPrinterHelper
    {
        #region variables

        private string phost;
        private string pqueue;
        private string puser;

        private string errormsg = "";

        private const int PORT = 515;          // hard coded LPR/LPD port number

        private string controlfile;             // not really a file but ok
                                                // Example: "HPC1\nProb\nfdfA040PC1\nUdfA040PC1\nNtimerH.ps\n"
                                                // H PC1   			=> responsible host (mandatory)
                                                // P rob 			=> responsible user (mandatory)
                                                // f dfA040PC1 		=> Print formatted file
                                                // U dfA040PC1      => Unlink data file (indicates that sourcefile is no longer needed!)
                                                // N timerH.ps      => Name of source file

        #endregion variables

        #region constructor

        /// <summary>
        /// Constructor for an instance of an printer that can communicate 
        /// with LPR, LPQ and LPRM.
        /// </summary>
        /// <param name="host">idem</param>
        /// <param name="queueName">idem</param>
        /// <param name="userName">idem</param>
        public LPDPrinterHelper(string host, string queueName, string userName="rvdt")
        {
            phost = host;
            pqueue = queueName;
            puser = userName;
        }

        #endregion constructor

        #region properties

        public string Host
        {
            get
            {
                return phost;
            }
        }

        public string Queue
        {
            get
            {
                return pqueue;
            }
            set
            {
                pqueue = value;
            }
        }

        public string User
        {
            get
            {
                return puser;
            }
        }

        public string ErrorMsg
        {
            get
            {
                return errormsg;
            }
        }



        #endregion properties

        #region Restart

        public void Restart()
        {
            ProcessRestart();
        }

        /// <summary>
        /// This command starts the printing process if it not already running.
        /// </summary>
        private void ProcessRestart()
        {
            errormsg = "";

            ////////////////////////////////////////////////////////
            /// PREPARE TCPCLIENT STUF
            ///
            TcpClient tc = new TcpClient();
            tc.Connect(phost, PORT);
            NetworkStream nws = tc.GetStream();
            if (!nws.CanWrite) {
                errormsg = "-1: cannot write to network stream";
                nws.Close();
                tc.Close();
                return;
            }

            ////////////////////////////////////////////////////////
            /// LOCAL VARIABLES
            ///
            const int BUFSIZE = 1024;               // 1KB buffer 
            byte[] buffer = new byte[BUFSIZE];
            byte[] ack = new byte[4];               // for acknowledge
            int cnt;                                // for read acknowledge

            ////////////////////////////////////////////////////////
            /// COMMAND: RESTART
            ///      +----+-------+----+
            ///      | 01 | Queue | LF |
            ///      +----+-------+----+
            ///      Command code - 1
            ///      Operand - Printer queue name
            /// 

            int pos = 0;
            buffer[pos++] = 1;
            for (int i = 0; i < pqueue.Length; i++) {
                buffer[pos++] = (byte)pqueue[i];
            }
            buffer[pos++] = (byte)'\n';

            nws.Write(buffer, 0, pos);
            nws.Flush();

            /////////////////////////////////////////////////////////
            /// READ ACK
            cnt = nws.Read(ack, 0, 4);
            if (ack[0] != 0) {
                errormsg = "-2: no ACK on RESTART";
                nws.Close();
                tc.Close();
                return;
            }
            nws.Close();
            tc.Close();
        }

        #endregion Restart

        #region LPR

        public void LPR(byte[] data)
        {

            SendFile(data);


            return;
        }



        /// <summary>
        /// Internal worker to send the file over LPR. If the sending succeeds 
        /// and the del flag is TRUE the file <para>fname</para> will be deleted.
        /// If any error occurs the file will not be deleted.  
        /// </summary>
        /// <param name="fname">filename to send</param>
        /// <param name="del">flag delete after print</param>
        private void SendFile(byte[] data)
        {
            errormsg = "";

            ////////////////////////////////////////////////////////
            /// PREPARE TCPCLIENT
            ///
            TcpClient tc = new TcpClient();
            tc.Connect(phost, PORT);
            NetworkStream nws = tc.GetStream();
            if (!nws.CanWrite) {
                errormsg = "-20: cannot write to network stream";
                nws.Close();
                tc.Close();
                return;
            }

            ////////////////////////////////////////////////////////
            /// SOME LOCAL VARIABLES
            ///
            string localhost = Dns.GetHostName();
            int jobID = GetJobId();
            string dname = String.Format("dfA{0}{1}", jobID, localhost);
            string cname = String.Format("cfA{0}{1}", jobID, localhost);
            controlfile = String.Format("H{0}\nP{1}\nf{2}\nU{3}\nN{4}\n",
                                        localhost, puser, dname, dname, "RAW-DATA");

            const int BUFSIZE = 4 * 1024;           // 4KB buffer
            byte[] buffer = new byte[BUFSIZE];      // 
            byte[] ack = new byte[4];               // for the acknowledges
            int cnt;                                // for read acknowledge

            ////////////////////////////////////////////////////////
            /// COMMAND: RECEIVE A PRINTJOB
            ///      +----+-------+----+
            ///      | 02 | Queue | LF |
            ///      +----+-------+----+
            ///
            int pos = 0;
            buffer[pos++] = 2;
            for (int i = 0; i < pqueue.Length; i++) {
                buffer[pos++] = (byte)pqueue[i];
            }
            buffer[pos++] = (byte)'\n';

            nws.Write(buffer, 0, pos);
            nws.Flush();

            /////////////////////////////////////////////////////////
            /// READ ACK
            cnt = nws.Read(ack, 0, 4);
            if (ack[0] != 0) {
                errormsg = "-21: no ACK on COMMAND 02.";
                nws.Close();
                tc.Close();
                return;
            }

            /////////////////////////////////////////////////////////
            /// SUBCMD: RECEIVE CONTROL FILE
            ///
            ///      +----+-------+----+------+----+
            ///      | 02 | Count | SP | Name | LF |
            ///      +----+-------+----+------+----+
            ///      Command code - 2
            ///      Operand 1 - Number of bytes in control file
            ///      Operand 2 - Name of control file
            ///
            pos = 0;
            buffer[pos++] = 2;
            string len = controlfile.Length.ToString();
            for (int i = 0; i < len.Length; i++) {
                buffer[pos++] = (byte)len[i];
            }
            buffer[pos++] = (byte)' ';
            for (int i = 0; i < cname.Length; i++) {
                buffer[pos++] = (byte)cname[i];
            }
            buffer[pos++] = (byte)'\n';

            nws.Write(buffer, 0, pos);
            nws.Flush();

            /////////////////////////////////////////////////////////
            /// READ ACK
            cnt = nws.Read(ack, 0, 4);
            if (ack[0] != 0) {
                errormsg = "-22: no ACK on SUBCMD 2";
                nws.Close();
                tc.Close();
                return;
            }

            /////////////////////////////////////////////////////////
            /// ADD CONTENT OF CONTROLFILE
            pos = 0;
            for (int i = 0; i < controlfile.Length; i++) {
                buffer[pos++] = (byte)controlfile[i];
            }
            buffer[pos++] = 0;

            nws.Write(buffer, 0, pos);
            nws.Flush();

            /////////////////////////////////////////////////////////
            /// READ ACK
            cnt = nws.Read(ack, 0, 4);
            if (ack[0] != 0) {
                errormsg = "-23: no ACK on CONTROLFILE";
                nws.Close();
                tc.Close();
                return;
            }

            /////////////////////////////////////////////////////////
            /// SUBCMD: RECEIVE DATA FILE
            ///
            ///      +----+-------+----+------+----+
            ///      | 03 | Count | SP | Name | LF |
            ///      +----+-------+----+------+----+
            ///      Command code - 3
            ///      Operand 1 - Number of bytes in data file
            ///      Operand 2 - Name of data file
            ///
            pos = 0;
            buffer[pos++] = 3;

            len = data.Length.ToString();

            for (int i = 0; i < len.Length; i++) {
                buffer[pos++] = (byte)len[i];
            }
            buffer[pos++] = (byte)' ';
            for (int i = 0; i < dname.Length; i++) {
                buffer[pos++] = (byte)dname[i];
            }
            buffer[pos++] = (byte)'\n';

            nws.Write(buffer, 0, pos);
            nws.Flush();

            /////////////////////////////////////////////////////////
            /// READ ACK
            cnt = nws.Read(ack, 0, 4);
            if (ack[0] != 0) {
                errormsg = "-24: no ACK on SUBCMD 3";
                nws.Close();
                tc.Close();
                return;
            }

            /////////////////////////////////////////////////////////
            /// ADD CONTENT OF DATAFILE


            nws.Write(data, 0, data.Length);
            nws.Flush();

            // close data file with a 0 ..
            pos = 0;
            buffer[pos++] = 0;
            nws.Write(buffer, 0, pos);
            nws.Flush();

            /////////////////////////////////////////////////////////
            /// READ ACK
            cnt = nws.Read(ack, 0, 4);
            if (ack[0] != 0) {
                errormsg = "-25: no ACK on DATAFILE";
                nws.Close();
                tc.Close();
                return;
            }

            nws.Close();
            tc.Close();

        }

        #endregion LPR

        #region LPQ

        /// <summary>
        /// LPQ requests the device for the queue content in a short format. 
        /// </summary>
        /// <returns>LPQ returns a printer specific string representing the queue</returns>
        public string LPQ()
        {
            return LPQ(false);
        }

        /// <summary>
        /// LPQ requests the device for the queue content. 
        /// If the parameter longlist is false it is requested in a short 
        /// format otherwise in a log format. Note that these formats are 
        /// not defined by the rfc1179 and are therefor printer or printserver 
        /// specific.
        /// </summary>
        /// <param name="longlist">boolean indicating a long (true) or a short (false) listing</param>
        /// <returns>LPQ returns a printer specific string representing the queue</returns>
        public string LPQ(bool longlist)
        {
            string rv = "";
            try {
                rv = ProcessLPQ(longlist);
            }
            catch {
                errormsg = "-30:  Could not request queue";
                rv = errormsg;
            }
            return rv;
        }

        /// <summary>
        /// LPQ internal worker 
        /// </summary>
        /// <param name="longlist">long (true) or a short (false) listing</param>
        /// <returns>string representing the queue contents.</returns>
        private string ProcessLPQ(bool longlist)
        {
            errormsg = "";

            ////////////////////////////////////////////////////////
            /// PREPARE TCPCLIENT STUF
            ///
            TcpClient tc = new TcpClient();
            tc.Connect(phost, PORT);
            NetworkStream nws = tc.GetStream();
            if (!nws.CanWrite) {
                errormsg = "-40: cannot write to network stream";
                nws.Close();
                tc.Close();
                return "";
            }

            ////////////////////////////////////////////////////////
            /// SOME LOCAL VARS
            ///
            const int BUFSIZE = 1024;               // 1KB buffer 
            byte[] buffer = new byte[BUFSIZE];      // fat buffer
            int cnt;                                // for read acknowledge

            ////////////////////////////////////////////////////////
            /// COMMAND: SEND QUEUE STATE
            ///     +----+-------+----+------+----+
            ///	    | 03 | Queue | SP | List | LF |
            ///		+----+-------+----+------+----+
            /// 	Command code - 3 for short que listing 4 for long que listing
            ///		Operand 1 - Printer queue name
            ///		Other operands - User names or job numbers
            ///
            int pos = 0;
            buffer[pos++] = (byte)(longlist ? 3 : 4);
            for (int i = 0; i < pqueue.Length; i++) {
                buffer[pos++] = (byte)pqueue[i];
            }
            buffer[pos++] = (byte)' ';
            // ask all users

            // for (int i = 0; i < user.Length; i++)
            // {
            // 		buffer[pos++] = (byte)user[i];
            // }
            buffer[pos++] = (byte)'\n';

            nws.Write(buffer, 0, pos);
            nws.Flush();

            /////////////////////////////////////////////////////////
            /// READ LPQ OUTPUT
            /// 
            string rv = "";
            cnt = nws.Read(buffer, 0, BUFSIZE);
            while (cnt > 0) {
                rv += System.Text.Encoding.ASCII.GetString(buffer);
                cnt = nws.Read(buffer, 0, BUFSIZE);
            }

            nws.Close();
            tc.Close();

            return rv.Replace("\n", "\r\n");
        }

        #endregion LPQ

        #region LPRM

        /// <summary>
        /// LPRM removes the job with jobID from the queue. If the jobID is 
        /// ommitted, the active job will be removed. Note that only jobs from 
        /// the current user can be removed. Check with LPQ if jobs are really 
        /// removed.
        /// </summary>
        /// <param name="jobID">
        /// String representing the jobID to be removed from the queue.
        /// jobID might contain multiple jobID's that are separated by spaces
        /// (at least for the winXP LPD daemon)
        /// </param>
        public void LPRM(string jobID)
        {
            try {
                ProcessLPRM(jobID);
            }
            catch {
                errormsg = "-50: Could not remove job";
            }
            return;
        }

        /// <summary>
        /// LPRM internal worker
        /// </summary>
        /// <returns></returns>
        private void ProcessLPRM(string jobID)
        {
            errormsg = "";

            ////////////////////////////////////////////////////////
            /// PREPARE TCPCLIENT STUF
            ///
            TcpClient tc = new TcpClient();
            tc.Connect(phost, PORT);
            NetworkStream nws = tc.GetStream();
            if (!nws.CanWrite) {
                errormsg = "-51: cannot write to network stream";
                nws.Close();
                tc.Close();
                return;
            }

            ////////////////////////////////////////////////////////
            /// LOCAL VARIABLES
            ///
            const int BUFSIZE = 1024;               // 1KB buffer
            byte[] buffer = new byte[BUFSIZE];      // buffer
            int cnt;                                // for read acknowledge
            byte[] ack = new byte[4];               // for the acknowledges

            ////////////////////////////////////////////////////////
            /// COMMAND: REMOVE JOBS 
            ///      +----+-------+----+-------+----+------+----+
            ///      | 05 | Queue | SP | Agent | SP | List | LF |
            ///      +----+-------+----+-------+----+------+----+
            ///      Command code - 5
            ///      Operand 1 - Printer queue name
            ///      Operand 2 - User name making request (the agent)
            ///      Other operands - User names or job numbers
            ///

            int pos = 0;
            buffer[pos++] = 5;
            for (int i = 0; i < pqueue.Length; i++) {
                buffer[pos++] = (byte)pqueue[i];
            }
            buffer[pos++] = (byte)' ';

            // for current user
            for (int i = 0; i < puser.Length; i++) {
                buffer[pos++] = (byte)puser[i];
            }

            // identified job, might be an empty string
            buffer[pos++] = (byte)' ';
            for (int i = 0; i < jobID.Length; i++) {
                buffer[pos++] = (byte)jobID[i];
            }
            buffer[pos++] = (byte)'\n';

            nws.Write(buffer, 0, pos);
            nws.Flush();

            /////////////////////////////////////////////////////////
            /// READ LPRM OUTPUT
            cnt = nws.Read(ack, 0, 4);
            if (ack[0] != 0) {
                errormsg = "-52: no ACK on COMMAND 05";
                nws.Close();
                tc.Close();
                return;
            }

            nws.Close();
            tc.Close();

            return;
        }

        #endregion LPRM

        static int _counter = 0;
        #region misc
        /// <summary>
        /// GetCounter returns the next jobid for the LPR protocol
        /// which must be between 0 and 999.
        /// The jobid is incremented every call but will be wrapped to 0 when 
        /// larger than 999. 
        /// </summary>
        /// <returns>next number</returns>
        
        private int GetJobId()
        {
            if (_counter > 999) _counter = 0;
            return _counter++;
        }

        #endregion misc
    }




}
