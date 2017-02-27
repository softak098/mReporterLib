////////////////////////////////////////////////////////////////////////////////
// File:    LprJob.cs
// Purpose: Main LPR job control code.
// Notice:  Copyright 2010 SunCat Services
// Author:  Edward F Eaglehouse
// 
// THIS WORK IS PROVIDED "AS IS" WITHOUT ANY EXPRESS OR IMPLIED WARRANTIES
// OR CONDITIONS OR GUARANTEES.
//
// This product is licensed under the terms of The Code Project Open
// License (CPOL) 1.02. You are free to use this code in any application,
// private, public, or commercial, subject to this license agreement. A
// copy of this license may be found on the web at
// http://www.codeproject.com/info/cpol10.aspx.
////////////////////////////////////////////////////////////////////////////////
// Revision    Contributor
// 12/08/2010  Edward F Eaglehouse
//      Initial revision.
//      I am very interested in tracking how meaningful a contribution this
//      project has been to the programming community. If you use this code,
//      please email me your name, company, and optionally a contact email
//      address or phone number to mailto:MobileLPR@SunCatServices.com. Your
//      email address and phone number will be kept private.
////////////////////////////////////////////////////////////////////////////////

using System;

using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace MobileLPR
{

    #region Event Handler Delegates

    public delegate void LprJobEventHandler(Object sender, LprJobProgressEventArgs args);

    #endregion

    /// <summary>
    /// Class used to define and submit a print job to a print server.
    /// It supports LPR, LPRng, and Direct protocols.
    /// </summary>
    public class LprJob
    {

        #region Constants

        /// <summary>
        /// Newline string used to terminate server commands.
        /// </summary>
        const String LPR_NEWLINE = "\n";

        /// <summary>
        /// Maximum job number count for assigning job numbers.
        /// </summary>
        const int MAX_JOB_COUNT = 1000;

        /// <summary>
        /// String of letters used to identify data file in job.
        /// </summary>
        const String LPR_INDEXNAMES = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// Size of read buffer used for data files.
        /// </summary>
        const int LPR_BUFSIZE = 2048;

        /// <summary>
        /// Receive timeout in milliseconds.
        /// </summary>
        const int LPR_RECV_TIMEOUT = 15000;

        /// <summary>
        /// Transmission timeout in milliseconds.
        /// </summary>
        const int LPR_SEND_TIMEOUT = 15000;

        #endregion

        #region Enumerations

        /// <summary>
        /// LPR/LPRng server command codes.
        /// </summary>
        public enum ServerCommands : byte
        {
            None = 0,
            PrintWaitingJobs = 1,
            SubmitJob = 2,
            ShortQueueStatus = 3,
            LongQueueStatus = 4,
            RemoveJobs = 5
        };

        /// <summary>
        /// LPR/LPRng server Submit Job subcommand codes.
        /// </summary>
        public enum SubmitJobSubCommands : byte
        {
            None = 0,
            AbortJob = 1,
            SubmitControlFile = 2,
            SubmitDataFile = 3
        };

        #endregion



        #region Static Members

        /// <summary>
        /// Next Job Number to be used (0 <= n < MAX_JOB_COUNT).
        /// </summary>
        private static int nextJobNumber_ = 0;

        /// <summary>
        /// Default Server Name (LPD host) to use when creating a job.
        /// </summary>
        private static String defaultServerName_ = String.Empty;

        /// <summary>
        /// Default Server Port (LPD port) to use when creating a job.
        /// </summary>
        private static Int32 defaultServerPort_ = 515;

        /// <summary>
        /// Default Printer Queue to use when creating a job.
        /// </summary>
        private static String defaultPrinterName_ = "printer";

        /// <summary>
        /// Default Protocol to use when transferring a job.
        /// </summary>
        private static LprProtocols defaultProtocol_ = LprProtocols.LPR;

        #endregion

        #region Private Members

        #region Connection Parameters

        /// <summary>
        /// Protocol selected for transferring job to print server.
        /// </summary>
        private LprProtocols protocol_ = LprProtocols.LPR;

        /// <summary>
        /// Server Name (LPD host) assigned to the print job.
        /// </summary>
        private String serverName_ = String.Empty;

        /// <summary>
        /// Server Port (LPD port) assigned to the print job.
        /// </summary>
        private Int32 serverPort_ = 515;

        #endregion

        #region Job Parameters

        /// <summary>
        /// Printer Name (LPR queue) assigned to the print job.
        /// </summary>
        private String printerName_ = String.Empty;

        /// <summary>
        /// User Name of user creating the job.
        /// </summary>
        private String userName_ = String.Empty;

        /// <summary>
        /// Host Name of the computer sending the job.
        /// </summary>
        private String hostName_ = String.Empty;

        /// <summary>
        /// Job Name to print on Banner page or queue status.
        /// </summary>
        private String jobName_ = String.Empty;

        /// <summary>
        /// Number of spaces represented by tab characters in text files.
        /// </summary>
        private Int32 indentSize_ = 8;

        /// <summary>
        /// Maximum Width of text file to be printed.
        /// </summary>
        private Int32 pageWidth_ = 132;

        /// <summary>
        /// Flag indicating if a Banner Page should be printed.
        /// </summary>
        private Boolean bannerEnabled_ = false;

        /// <summary>
        /// Class Name to print on Banner page.
        /// </summary>
        private String bannerJobClass_ = String.Empty;

        /// <summary>
        /// Title to print on Banner page.
        /// </summary>
        private String pageTitle_ = String.Empty;

        /// <summary>
        /// Flag indicating to send mail when a job is completed.
        /// </summary>
        private Boolean mailEnabled_ = false;

        /// <summary>
        /// Mail recipient of job completion message.
        /// </summary>
        private String mailTo_ = String.Empty;

        /// <summary>
        /// Number of Copies to print of each data file.
        /// </summary>
        private Int32 copies_ = 1;

        /// <summary>
        /// Priority (0-25) for LPRng jobs.
        /// </summary>
        private Int32 priority_ = 0;

        /// <summary>
        /// Collection of data files included in job.
        /// </summary>
        private List<LprData> dataFiles_ = new List<LprData>();

        #endregion

        #region Housekeeping Values

        /// <summary>
        /// Job Number used to identify the print job.
        /// </summary>
        private int jobNumber_ = 0;

        /// <summary>
        /// Total count of local bytes to send for the job.
        /// </summary>
        private Int64 jobSizeTotal_ = 0;

        /// <summary>
        /// Current count of local bytes sent for the job.
        /// </summary>
        private Int64 jobSizeUsed_ = 0;

        /// <summary>
        /// Flag indicating to send control file before data files.
        /// </summary>
        private Boolean sendControlFileFirst_ = true;

        /// <summary>
        /// Current index of data file being transferred.
        /// </summary>
        private Int32 dataFileIndex_ = 0;

        /// <summary>
        /// Current copy index of data file being transferred.
        /// </summary>
        private Int32 copyIndex_ = 0;

        /// <summary>
        /// Object used to translate between client and server encodings.
        /// </summary>
        private Encoding _serverEncoding = null;

        /// <summary>
        /// Connection object used to exchange data with server.
        /// </summary>
        private TcpClient _clientConnection = null;

        /// <summary>
        /// Flag indicating the control file was sent.
        /// </summary>
        private Boolean _sentControlFile = false;

        #endregion

        #region Job Status and Results

        /// <summary>
        /// Synchronization object used to signal end-of-job.
        /// </summary>
        private ManualResetEvent endJob_ = new ManualResetEvent(true);

        /// <summary>
        /// Flag indicating that a connection to server is active.
        /// </summary>
        private bool _isConnected = false;

        /// <summary>
        /// Last Exception encountered when an error occurs.
        /// </summary>
        private Exception lastException_ = null;

        #endregion

        #endregion

        #region Properties

        #region Default Properties

        /// <summary>
        /// Get or set default Server Name to use when creating a job.
        /// </summary>
        public static String DefaultServerName
        {
            get { return defaultServerName_; }
            set { defaultServerName_ = value; }
        }

        /// <summary>
        /// Get or set default Server Port to use when creating a job.
        /// </summary>
        public static Int32 DefaultServerPort
        {
            get { return defaultServerPort_; }
            set { defaultServerPort_ = value; }
        }

        /// <summary>
        /// Get or set default Printer Queue to use when creating a job.
        /// </summary>
        public static String DefaultPrinterName
        {
            get { return defaultPrinterName_; }
            set { defaultPrinterName_ = value; }
        }

        /// <summary>
        /// Get or set default Protocol to use when transferring a job.
        /// </summary>
        public static LprProtocols DefaultProtocol
        {
            get { return defaultProtocol_; }
            set { defaultProtocol_ = value; }
        }

        #endregion

        #region Connection Parameters

        /// <summary>
        /// Get or set Protocol to use when transferring job to print server.
        /// </summary>
        public LprProtocols Protocol
        {
            get { return protocol_; }
            set { protocol_ = value; }
        }

        /// <summary>
        /// Get or set Server Name (LPR host) assigned to the print job.
        /// </summary>
        public String ServerName
        {
            get { return serverName_; }
            set { serverName_ = value; }
        }

        /// <summary>
        /// Get or set Server Port (LPR port) assigned to the print job.
        /// </summary>
        public Int32 ServerPort
        {
            get { return serverPort_; }
            set
            {
                serverPort_ = (value > 0 && value <= 65535) ? value : defaultServerPort_;
            }
        }

        #endregion

        #region Job Parameters

        /// <summary>
        /// Get or set Printer Name (LPR queue) assigned to the print job.
        /// </summary>
        public String PrinterName
        {
            get { return printerName_; }
            set { printerName_ = value; }
        }

        /// <summary>
        /// Get or set User Name of user creating the job.
        /// </summary>
        public String UserName
        {
            get { return userName_; }
            set { userName_ = value; }
        }

        /// <summary>
        /// Get or set Host Name of the computer sending the job.
        /// </summary>
        public String HostName
        {
            get { return hostName_; }
            set { hostName_ = value; }
        }

        /// <summary>
        /// Get or set Job Name to display on Banner page or queue status.
        /// </summary>
        public String JobName
        {
            get { return jobName_; }
            set { jobName_ = value; }
        }

        /// <summary>
        /// Get or set Number of spaces represented by tab characters in text.
        /// </summary>
        public Int32 IndentSize
        {
            get { return indentSize_; }
            set { indentSize_ = (value < 1) ? 8 : value; }
        }

        /// <summary>
        /// Get or set Maximum Width of text file to be printed.
        /// </summary>
        public Int32 PageWidth
        {
            get { return pageWidth_; }
            set { pageWidth_ = (value < 1) ? 1 : value; }
        }

        /// <summary>
        /// Get or set flag indicating if a Banner Page should be printed.
        /// </summary>
        public Boolean BannerEnabled
        {
            get { return bannerEnabled_; }
            set { bannerEnabled_ = value; }
        }

        /// <summary>
        /// Class Name to print on Banner page.
        /// </summary>
        public String BannerJobClass
        {
            get { return bannerJobClass_; }
            set { bannerJobClass_ = value; }
        }

        /// <summary>
        /// Get or set Title to print on Banner page.
        /// </summary>
        public String PageTitle
        {
            get { return pageTitle_; }
            set { pageTitle_ = value; }
        }

        /// <summary>
        /// Get or set flag indicating to send mail when a job is completed.
        /// </summary>
        public Boolean MailEnabled
        {
            get { return mailEnabled_; }
            set { mailEnabled_ = value; }
        }

        /// <summary>
        /// Get or set Mail recipient of job completion message.
        /// </summary>
        public String MailTo
        {
            get { return mailTo_; }
            set { mailTo_ = value; }
        }

        /// <summary>
        /// Get or set Number of Copies to print of each data file.
        /// </summary>
        /// <remarks>
        /// This option is unsupported by LPR protocol and is forced to 1 when
        /// using it.
        /// </remarks>
        public Int32 Copies
        {
            get { return copies_; }
            set { copies_ = (value > 0) ? value : 1; }
        }

        /// <summary>
        /// Get or set Priority (0-25).
        /// </summary>
        /// <remarks>
        /// This option supported by LPRng only. For LPR protocol, it is forced
        /// to 0 (lowest priority). For Direct protocol, it is unused.
        /// </remarks>
        public Int32 Priority
        {
            get { return priority_; }
            set { priority_ = value; }
        }

        /// <summary>
        /// Get collection of data files included in job.
        /// </summary>
        public List<LprData> DataFiles
        {
            get { return dataFiles_; }
        }

        #endregion

        #region Housekeeping Values

        /// <summary>
        /// Get Job Number used to identify the print job.
        /// </summary>
        public Int32 JobNumber
        {
            get { return jobNumber_; }
        }

        /// <summary>
        /// Get Job Size in bytes of local files to transfer.
        /// </summary>
        public Int64 JobSize
        {
            get { jobSizeTotal_ = GetJobSize(); return jobSizeTotal_; }
        }

        #endregion

        #region Job Status and Results

        /// <summary>
        /// Get or set flag indicating that a connection to server is active.
        /// </summary>
        public Boolean IsConnected
        {
            get { return _isConnected; }
            private set { _isConnected = value; }
        }

        /// <summary>
        /// Get flag indicating if job is Completed. True if job submission has
        /// been completed; otherwise false.
        /// </summary>
        public bool IsCompleted
        {
            get { return endJob_.WaitOne(0, false); }
            private set
            {
                if (value == true) {
                    // Set the end-of-job signal.
                    endJob_.Set();
                }
                else {
                    // Clear the end-of-job signal.
                    endJob_.Reset();
                }
            }
        }

        /// <summary>
        /// Get or set Last Exception encountered when an error occurs.
        /// </summary>
        public Exception LastException
        {
            get { return lastException_; }
            private set { lastException_ = value; }
        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the LprJob class.
        /// </summary>
        public LprJob()
        {
            // Default print job.
            jobNumber_ = GetNextJobNumber();
            printerName_ = DefaultPrinterName;
            serverName_ = DefaultServerName;
            serverPort_ = DefaultServerPort;
            protocol_ = DefaultProtocol;
            _serverEncoding = new ASCIIEncoding();
            userName_ = "MobileLPR";
            hostName_ = "WindowsCE";
        }

        #endregion

        #region Static Methods

        public static int GetNextJobNumber()
        {
            int jobNumber;

            jobNumber = nextJobNumber_++;
            if (nextJobNumber_ >= MAX_JOB_COUNT) {
                nextJobNumber_ = 0;
            }

            return jobNumber;
        } // GetNextJobNumber()

        #endregion

        #region Public Methods

        /// <summary>
        /// Set printer parameters from a uniform resource indicator string.
        /// </summary>
        /// <param name="destination">URI string identifying destination printer.</param>
        /// <remarks>
        /// A URI string of the following form can be used to identify the print
        /// server parameters:
        ///     protocol://[user@]hostname[:port][/printer]
        /// </remarks>
        /// <exception cref="UriException">when a URI is missing mandatory
        /// components or is otherwise formatted incorrectly.</exception>
        public void SetPrinterURI(String destination)
        {
            LprDestination dest = new LprDestination(destination);

            Protocol = dest.Protocol;
            if (!String.IsNullOrEmpty(dest.UserName)) {
                UserName = dest.UserName;
            }
            ServerName = dest.ServerName;
            ServerPort = dest.ServerPort;
            if (!String.IsNullOrEmpty(dest.PrinterName)) {
                PrinterName = dest.PrinterName;
            }
        } // SetPrinterURI(String destination)

        /// <summary>
        /// Add a data file to be printed in Literal (raw) format.
        /// </summary>
        /// <param name="pathname">Local pathname of file to print.</param>
        /// <returns>True if data file was added; false if error.</returns>
        public Boolean AddDataFile(byte[] data)
        {
            return AddDataFile(data, LprDataFormats.Literal);
        } // AddDataFile(String pathname)

        /// <summary>
        /// Add a data file to be printed in a specific format.
        /// </summary>
        /// <param name="pathname">Local pathname of file to print.</param>
        /// <param name="format">Format of data file contents.</param>
        /// <returns>True if data file was added; false if error.</returns>
        /// <remarks>
        /// The LPR and LPRng protocols limit the number of data files per job
        /// to a maximum of 52, the number of index letters available. This
        /// method will not add more than this many files. Though not specified,
        /// some LPR servers can handle only one file per job.
        /// </remarks>
        public Boolean AddDataFile(byte[] data, LprDataFormats format)
        {
            LprData file = new LprData(data);
            file.Format = format;
            if ((Protocol == LprProtocols.LPR ||
                    Protocol == LprProtocols.LPRng) &&
                    dataFiles_.Count >= LPR_INDEXNAMES.Length) {
                // Prevent attaching more files than the protocol allows.
                return false;
            }

            dataFiles_.Add(file);
            return true;
        } // AddDataFile(String pathname, LprDataFormats format)

        /// <summary>
        /// Transfer the print job to the print server for processing.
        /// </summary>
        /// <remarks>
        /// This is an asynchronous call. Either subscribe to the <see cref="LprJobCompleted"/>
        /// event, call the <see cref="WaitForCompletion"/> method, or check the
        /// <see cref="IsCompleted"/> property.
        /// </remarks>
        /// <exception cref="LprException">when </exception>
        public void SubmitJob()
        {
            // Start the print job transfer.
            StartJobTransfer();
        } // SubmitJob()

        /// <summary>
        /// Wait for completion of a print job submission.
        /// </summary>
        public void WaitForCompletion()
        {
            // Wait for the End of Job signal.
            endJob_.WaitOne();
        } // WaitForCompletion()

        /// <summary>
        /// Start printing jobs waiting in queue if not already running.
        /// </summary>
        /// <returns>True if command was sent successfully; false if error.</returns>
        public Boolean PrintWaitingJobs()
        {
            Boolean result = false;

            try {
                // Clear the End-of-Job signal.
                IsCompleted = false;

                // Clear exception holder.
                LastException = null;

                // Establish connection to print server.
                StartServerCommand();
                result = SendPrintWaitingJobsCommand();
            }
            catch (Exception ex) {
                // Capture error.
                LastException = ex;
            }
            finally {
                // Terminate the Receive Job command.
                EndServerCommand();

                // Set the End-of-Job signal.
                IsCompleted = true;
            }

            return result;
        } // PrintWaitingJobs()

        /// <summary>
        /// Get short or long status listing of queued jobs.
        /// </summary>
        /// <param name="longListing">True for long listing; false for short.</param>
        /// <param name="joblist">List identifying specific users or jobs.</param>
        /// <returns>String containing response from server.</returns>
        /// <remarks>
        /// Response lines from server are terminated by Unix newlines, for
        /// standards-compliant LPR/LPRng servers.
        /// </remarks>
        public String GetQueueStatus(Boolean longListing, String[] joblist)
        {
            String response = String.Empty;

            try {
                // Clear the End-of-Job signal.
                IsCompleted = false;

                // Clear exception holder.
                LastException = null;

                // Establish connection to print server.
                if (!StartServerCommand()) {
                    throw new LprException("unable to establish connection");
                }
                if (SendQueueStatusCommand(longListing, joblist)) {
                    // Capture responses from print server.
                    response = GetResponseText();
                }
            }
            catch (Exception ex) {
                // Capture error.
                LastException = ex;
            }
            finally {
                // Terminate the Receive Job command.
                EndServerCommand();

                // Set the End-of-Job signal.
                IsCompleted = true;
            }

            // Return response string retrieved from server.
            return response;
        } // GetQueueStatus(Boolean longListing, String[] joblist)

        /// <summary>
        /// Remove the currently running print job or list of jobs.
        /// </summary>
        /// <param name="joblist">List of users or jobs to remove; currently active job
        /// if none listed.</param>
        public Boolean RemoveJobs(String[] joblist)
        {
            Boolean result = false;

            try {
                // Clear the End-of-Job signal.
                IsCompleted = false;

                // Clear exception holder.
                LastException = null;

                // Establish connection to print server.
                StartServerCommand();
                result = SendRemoveJobsCommand(joblist);
            }
            catch (Exception ex) {
                // Capture error.
                LastException = ex;
            }
            finally {
                // Terminate the Receive Job command.
                EndServerCommand();

                // Set the End-of-Job signal.
                IsCompleted = true;
            }

            return result;
        } // RemoveJobs(String[] joblist)

        #endregion

        #region Private Methods

        /// <summary>
        /// Start transferring the job to the print server.
        /// </summary>
        private void StartJobTransfer()
        {
            // Clear the End-of-Job signal.
            IsCompleted = false;

            // Launch a background thread for transfer operations.
            ThreadPool.QueueUserWorkItem(new WaitCallback(SendJobFiles));
        } // StartJobTransfer()

        /// <summary>
        /// Perform cleanup tasks after the print job has been submitted.
        /// </summary>
        private void EndJobTransfer()
        {
            if (_clientConnection != null) {
                // Close connection to the print server.
                if (IsConnected) {
                    // Socket must be closed independently.
                    _clientConnection.GetStream().Close();
                    IsConnected = false;
                }
                _clientConnection.Close();
                _clientConnection = null;
            }

            if (LprJobCompleted != null) {
                // Notify subscribers the submission has ended.
                LprJobProgressEventArgs args = new LprJobProgressEventArgs();
                args.Total = jobSizeTotal_;
                args.Used = jobSizeUsed_;
                args.Command = ServerCommands.SubmitJob;
                LprJobCompleted(this, args);
            }

            // Set the End-of-Job signal.
            IsCompleted = true;
        } // EndJobTransfer()

        /// <summary>
        /// Send all job files to the server using the specified protocol.
        /// </summary>
        /// <remarks>
        /// To submit a job to the LPR/LPRng server, the following steps are
        /// implemented:
        /// 1. Open a new connection to the server.
        /// 2. Send control and data files.
        /// 3. Close the server connection.
        /// 
        /// Steps to send a control file (either first or last file sent), use:
        /// 1. Send a Receive Control File subcommand with length of file.
        /// 2. Receive acknowledgement from server.
        /// 3. Send contents of Control file.
        /// 4. Send zero octet (NUL).
        /// 5. Receive acknowledgement from server.
        /// 
        /// To send a data file, use:
        /// 1. Send a Receive Data File subcommand with length of file.
        /// 2. Receive acknowledgement from server.
        /// 3. Send contents of Data file.
        /// 4. Send zero octet (NUL).
        /// 5. Receive acknowledgement from server.
        /// 
        /// For LPR protocol, copies are implemented by sending the print job
        /// multiple times. For LPRng protocol, copies are implemented by
        /// sending multiple copies of data files for a single print job.
        /// For Direct protocol, the data files are sent multiple times.
        /// </remarks>
        private void SendJobFiles(Object state)
        {
            Boolean isDone = false;
            Boolean needControlFile = false;
            string controlFile = null;

            try {
                // Clear exception holder.
                LastException = null;

                // Determine if we need a control file.
                controlFilename_ = String.Empty;
                needControlFile = (Protocol == LprProtocols.LPR || Protocol == LprProtocols.LPRng);
                if (needControlFile) {
                    // Create the control file to send to the server.
                    controlFile = CreateControlFile();
                }

                // For LPR protocol, implement multiple copies by sending the
                // entire print job multiple times.
                for (int lprCopies = 0; lprCopies < (Protocol == LprProtocols.LPR ? Copies : 1); ++lprCopies) {
                    // Restart next print job.
                    _sentControlFile = false;

                    // Establish connection to print server.
                    StartServerCommand();
                    SendReceiveJobCommand();

                    dataFileIndex_ = 0;
                    copyIndex_ = 0;

                    if (needControlFile && sendControlFileFirst_ && !_sentControlFile) {
                        // Send control file before data files.
                        if (!SendControlFile(controlFile)) {
                            isDone = true;
                        }
                    }

                    // Send multiple copies of data files to be printed.
                    for (dataFileIndex_ = 0;
                            dataFileIndex_ < DataFiles.Count && !isDone;
                            ++dataFileIndex_) {
                        // Send specified number of copies of each data file.
                        // For LPR protocol, send only a single copy.
                        for (copyIndex_ = 0;
                                !isDone &&
                                copyIndex_ < (Protocol == LprProtocols.LPR ? 1 : Copies);
                                ++copyIndex_) {
                            // Send next copy of data file.
                            if (!SendDataFile()) {
                                isDone = true;
                            }
                        } // for (copyIndex_ = 0;
                    } // for (dataFileIndex_ = 0;

                    if (needControlFile && !_sentControlFile && !isDone) {
                        // Send control file after data files.
                        if (!SendControlFile(controlFile)) {
                            isDone = true;
                        }
                    }

                    // Terminate the Receive Job command.
                    EndServerCommand();
                } // for (int extCopies = 0;
            }
            catch (Exception ex) {
                // Capture error.
                LastException = ex;
            }
            finally {
                // Terminate the Receive Job command.
                EndServerCommand();

                // Perform end-of-job cleanup processing.
                EndJobTransfer();
            }
        } // SendJobFiles()

        /// <summary>
        /// Create the control file for the job.
        /// </summary>
        /// <returns>Pathname of control file created.</returns>
        private string CreateControlFile()
        {
            StringBuilder sb = new StringBuilder();

            controlFilename_ = "CONTROL-FILE";

            sb.Append(String.Format("H{0}" + LPR_NEWLINE, HostName));
            sb.Append(String.Format("P{0}" + LPR_NEWLINE, UserName));
            if (BannerEnabled || MailEnabled) {
                if (BannerEnabled) {
                    // Include banner data.
                    sb.Append(String.Format("C{0}" + LPR_NEWLINE, BannerJobClass));
                    sb.Append(String.Format("L{0}" + LPR_NEWLINE, UserName));
                }
                if (MailEnabled) {
                    // Include email address.
                    sb.Append(String.Format("M{0}" + LPR_NEWLINE, MailTo));
                }
            }
            // Include Job Name for display as document name when listing
            // print queue status.
            sb.Append(String.Format("J{0}" + LPR_NEWLINE, JobName));
            sb.Append(String.Format("T{0}" + LPR_NEWLINE, PageTitle));
            sb.Append(String.Format("I{0}" + LPR_NEWLINE, IndentSize));
            sb.Append(String.Format("W{0}" + LPR_NEWLINE, PageWidth));

            // Include all data files that are part of this job.
            for (dataFileIndex_ = 0; dataFileIndex_ < dataFiles_.Count; ++dataFileIndex_) {
                for (copyIndex_ = 0;
                    copyIndex_ < (Protocol == LprProtocols.LPR ? 1 : Copies);
                    ++copyIndex_) {
                    String name = MakeDataFilename(dataFileIndex_, copyIndex_);
                    sb.Append(String.Format("{0}{1}" + LPR_NEWLINE,
                        DataFiles[dataFileIndex_].FormatCommand,
                        name));
                    sb.Append(String.Format("N{0}" + LPR_NEWLINE,
                        DataFiles[dataFileIndex_].Name));
                    sb.Append(String.Format("U{0}" + LPR_NEWLINE, name));
                }
            }
            if (Protocol == LprProtocols.LPRng) {
                // LPRng-specific options.
                // Add unique job identifier.
                sb.Append(String.Format("A{0}@{1}+{2}", UserName, HostName, JobNumber));
                // Add name of original destination printer queue.
                sb.Append(String.Format("Q{0}" + LPR_NEWLINE, PrinterName));
            }

            return sb.ToString();

            //return controlFilename_;
        }


        private String MakeControlFilename(int priority)
        {
            if (priority < 0 || priority >= 26) {
                throw new ArgumentException("priority must be between 0 and 25");
            }

            if (Protocol == LprProtocols.LPR) {
                priority = 0;
            }

            String name = String.Format("cf{0}{1:d3}{2}",
                    LPR_INDEXNAMES.Substring(priority, 1),
                    jobNumber_,
                    HostName
                );

            return name;
        }


        private String MakeDataFilename(int index, int copy)
        {
            String name = String.Format("df{0}{1:d3}{2}{3}",
                    LPR_INDEXNAMES.Substring(index, 1),
                    jobNumber_,
                    HostName,
                    (copy > 0) ? $".C{copy + 1}" : ""
                );

            return name;
        }


        private Boolean SendReceiveJobCommand()
        {
            Boolean result = false;

            if (Protocol == LprProtocols.Direct) {
                // Simply return when sending files directly.
                return true;
            }

            try {
                // Format and send command to the print server.
                String args = String.Format("{0}", PrinterName);
                SendCommand((int)ServerCommands.SubmitJob, args);
                result = true;
            }
            catch (Exception ex) {
                // Capture error.
                LastException = ex;
            }

            return result;
        } // SendReceiveJobCommand()

        private Boolean SendQueueStatusCommand(Boolean longListing, String[] joblist)
        {
            Boolean result = false;

            if (Protocol == LprProtocols.Direct) {
                // Simply return when sending files directly.
                return true;
            }

            try {
                // Format and send command to the print server.
                StringBuilder args = new StringBuilder();

                args.Append(String.Format("{0}", PrinterName));
                if (joblist != null && joblist.Length > 0) {
                    // Append list of users and jobs as operands.
                    foreach (String s in joblist) {
                        if (!String.IsNullOrEmpty(s)) {
                            // Separate operands by whitespace.
                            args.Append(" ").Append(s);
                        }
                    }
                }
                SendCommand((int)(longListing ?
                    ServerCommands.LongQueueStatus :
                    ServerCommands.ShortQueueStatus),
                    args.ToString());
                result = true;
            }
            catch (Exception ex) {
                // Capture error.
                LastException = ex;
            }

            return result;
        }

        private Boolean SendRemoveJobsCommand(String[] joblist)
        {
            Boolean result = false;

            if (Protocol == LprProtocols.Direct) {
                // Simply return when sending files directly.
                return true;
            }

            try {
                // Format and send command to the print server.
                StringBuilder args = new StringBuilder();

                args.Append(String.Format("{0} {1}", PrinterName, UserName));
                if (joblist != null && joblist.Length > 0) {
                    // Append list of users and jobs as operands.
                    foreach (String s in joblist) {
                        if (!String.IsNullOrEmpty(s)) {
                            // Separate operands by whitespace.
                            args.Append(" ").Append(s);
                        }
                    }
                }
                SendCommand((int)ServerCommands.RemoveJobs,
                    args.ToString());
                result = true;
            }
            catch (Exception ex) {
                // Capture error.
                LastException = ex;
            }

            return result;
        }
        private Boolean StartServerCommand()
        {
            Boolean result = false;

            // Open a new connection to the server.
            result = OpenServerConnection();

            return result;
        } // StartServerCommand()

        /// <summary>
        /// Perform post-command processing.
        /// </summary>
        private void EndServerCommand()
        {
            CloseServerConnection();
        } // EndServerCommand()

        /// <summary>
        /// Open the network connection to the server.
        /// </summary>
        private Boolean OpenServerConnection()
        {
            const int WSAEINTR = 10004;
            const int WSAENETDOWN = 10050;
            const int WSAENETUNREACH = 10051;
            const int WSAETIMEDOUT = 10060;
            const int WSAECONNREFUSED = 10061;
            const int WSAEHOSTDOWN = 10064;
            const int WSAEHOSTUNREACH = 10065;
            const int WSAHOST_NOT_FOUND = 11001;


            try {
                // Create a new connection object.
                //IPEndPoint ipLocalEndPoint = new IPEndPoint(IPAddress.Any, localPort);
                _clientConnection = new TcpClient(/*ipLocalEndPoint*/);

                // Set socket options.
                _clientConnection.NoDelay = true;
                //clientConnection_.Client.SetSocketOption(SocketOptionLevel.Socket,SocketOptionName.ReuseAddress, 1);
                // Timeouts are unsupported in the .NET Compact Framework.
                _clientConnection.ReceiveTimeout = LPR_RECV_TIMEOUT;
                _clientConnection.SendTimeout = LPR_SEND_TIMEOUT;

                // Open connection to print server.
                _clientConnection.Connect(ServerName, ServerPort);
                _isConnected = true;
            }
            catch (SocketException ex) {
                // Throw fatal exceptions for caller to handle.
                switch (ex.ErrorCode) {
                    case WSAEINTR:
                    case WSAENETDOWN:
                    case WSAETIMEDOUT:
                    case WSAENETUNREACH:
                    case WSAECONNREFUSED:
                    case WSAEHOSTDOWN:
                    case WSAEHOSTUNREACH:
                    case WSAHOST_NOT_FOUND:
                        throw ex;
                }
            }



            return _isConnected;
        } // OpenServerConnection()

        /// <summary>
        /// Close the network connection to the server.
        /// </summary>
        private void CloseServerConnection()
        {
            if (_clientConnection != null) {
                // Socket must be closed independently.
                if (IsConnected) {
                    _clientConnection.GetStream().Close();
                    IsConnected = false;
                }
                _clientConnection.Close();
                _clientConnection = null;
            }
        } // CloseServerConnection()

        private void SendCommand(SubmitJobSubCommands commandCode, string args)
        {
            Byte[] buffer = new byte[200];
            int byteCount = 0;

            buffer[0] = (byte)commandCode;
            args += LPR_NEWLINE;
            byteCount = _serverEncoding.GetBytes(args, 0, args.Length, buffer, 1);
            _clientConnection.GetStream().Write(buffer, 0, byteCount + 1);
        }

        /// <summary>
        /// Send the job control file to the server.
        /// </summary>
        private Boolean SendControlFile(string fileContent)
        {
            Boolean result = false;

            if (Protocol == LprProtocols.Direct) {
                return true;
            }

            // Generate name of file and calculate size for transfer.
            LprTransferUnit unit = new LprTransferUnit("CONTROL-FILE");
            unit.Content = fileContent;

            if (SendControlFileSubCommand(unit)) {
                // Send file contents.
                result = SendFileContents(unit);

                _sentControlFile = result;
            }

            return result;
        } // SendControlFile()

        /// <summary>
        /// Send Submit Job subcommand to transfer control file contents.
        /// </summary>
        /// <param name="state">State object describing control file.</param>
        /// <returns>True if server accepted command; false if error.</returns>
        private Boolean SendControlFileSubCommand(LprTransferUnit state)
        {
            Boolean result = false;
            int acknowledgement = 0;

            // Format and send Receive Control File subcommand.
            String command = state.ContentSize + " " + state.UnitName;
            SendCommand(SubmitJobSubCommands.SubmitControlFile, command);

            // Get acknowledgement from server.
            acknowledgement = GetResponseCode();
            result = (acknowledgement == 0);
            if (acknowledgement != 0) {
                // Save information about the error.
                StringBuilder message = new StringBuilder(String.Format(
                        "lpr error {0} sending control file subcommand",
                        acknowledgement));
                // Add rejection code interpretation per LPRng.
                String rejection = GetLprngErrorMessage(acknowledgement);
                if (!String.IsNullOrEmpty(rejection)) {
                    message.Append(" (").Append(rejection).Append(")");
                }
                LprException ex = new LprException(message.ToString());
                ex.ErrorCode = acknowledgement;
                LastException = ex;
            } // if (acknowledgement != 0)

            return result;
        } // SendControlFileSubCommand(LprTransferState state)

        /// <summary>
        /// Get command response code from server.
        /// </summary>
        /// <returns>Integer value of response byte from server.</returns>
        private int GetResponseCode()
        {
            return _clientConnection.GetStream().ReadByte();
        } // GetResponseCode()

        /// <summary>
        /// Get textual response string from server.
        /// </summary>
        /// <returns>Raw text response from server.</returns>
        private String GetResponseText()
        {
            StringBuilder text = new StringBuilder();

            while (_clientConnection.GetStream().CanRead) {
                Byte[] bytes = new Byte[1024];
                int bytesRead = _clientConnection.GetStream().Read(bytes, 0, bytes.Length);
                if (bytesRead == 0) {
                    break;
                }

                text.Append(_serverEncoding.GetChars(bytes, 0, bytesRead));
            } // while (clientConnection_.GetStream().CanRead)

            return text.ToString();
        } // GetResponseText()

        /// <summary>
        /// Translate rejection code into an LPRng error message.
        /// </summary>
        /// <param name="error">Integer rejection code.</param>
        /// <returns>String containing error message; empty if not matched.</returns>
        private String GetLprngErrorMessage(int error)
        {
            switch (error) {
                case 1: return "queue not accepting jobs or invalid queue name";
                case 2: return "queue temporarily full, retry later";
                case 3: return "bad job format, do not retry";
            }
            return "";
        }

        /// <summary>
        /// Send a data file to the server.
        /// </summary>
        private bool SendDataFile(LprData dataFile)
        {
            bool result = false;
            LprTransferUnit unit = new LprTransferUnit(MakeDataFilename(dataFileIndex_, copyIndex_));



            //DataFiles[dataFileIndex_].Name;
                );

            // Generate name of file and calculate size for transfer.
            //unit.serverFilename =;
            unit.IsBinary = DataFiles[dataFileIndex_].IsBinary;
            unit.SizeTotal = CalculateServerFileSize(unit);

            if (Protocol == LprProtocols.Direct ||
                    SendDataFileSubCommand(unit)) {
                // Send file contents.
                result = SendFileContents(unit);
            }

            return result;
        } // SendDataFile()

        /// <summary>
        /// Send Submit Job subcommand to transfer data file contents.
        /// </summary>
        /// <param name="state">State object describing data file.</param>
        /// <returns>True if server accepted command; false if error.</returns>
        private Boolean SendDataFileSubCommand(LprTransferUnit state)
        {
            Boolean result = false;
            int acknowledgement = 0;

            // Format and send Receive Data File subcommand.
            String command = state.ContentSize + " " + state.UnitName;
            SendCommand(SubmitJobSubCommands.SubmitDataFile, command);

            // Get acknowledgement from server.
            acknowledgement = GetResponseCode();
            result = (acknowledgement == 0);
            if (acknowledgement != 0) {
                // Save information about the error.
                StringBuilder message = new StringBuilder(String.Format(
                        "lpr error {0} sending data file subcommand",
                        acknowledgement));
                // Add rejection code interpretation per LPRng.
                String rejection = GetLprngErrorMessage(acknowledgement);
                if (!String.IsNullOrEmpty(rejection)) {
                    message.Append(" (").Append(rejection).Append(")");
                }
                LprException ex = new LprException(message.ToString());
                ex.ErrorCode = acknowledgement;
                LastException = ex;
            } // (acknowledgement != 0)

            return result;
        } // SendDataFileSubCommand(LprTransferState state)

    

        /// <summary>
        /// Send contents of a file to the server.
        /// </summary>
        /// <param name="state">Object containing information of file being sent.</param>
        /// <returns>True if successful; false if error.</returns>
        /// <remarks>
        /// Read the file identified in the "state" parameter a block at a time
        /// and send it to the print server over the current connection.
        /// </remarks>
        private Boolean SendFileContents(LprTransferUnit state)
        {
            bool result = false;
            byte[] buffer = new byte[LPR_BUFSIZE];

            /*
            // Read and send file a block at a time.
            int bytesRead = 0;
            byte[] sendBuffer = null;
            int sendCount = 0;
            for ( ; ; )
            {
                bytesRead = state.stream.Read(state.buffer, 0, LPR_BUFSIZE);
                if (bytesRead == 0)
                {
                    // Reached end of current file.
                    break;
                }

                if (state.isBinary)
                {
                    // Use raw data from file.
                    sendBuffer = state.buffer;
                    sendCount = bytesRead;
                }
                else
                {
                    // Translate text from file encoding into ASCII.
                    if (fileEncoding == null)
                    {
                        // Determine encoding for proper decoding of text.
                        if (bytesRead >= 2 &&
                            state.buffer[0] == 0xFE && state.buffer[1] == 0xFF)
                        {
                            // Read UTF-16 Big-endian file.
                            fileEncoding = new UnicodeEncoding(true, true);
                        }
                        else if (bytesRead >= 2 &&
                            state.buffer[0] == 0xFF && state.buffer[1] == 0xFE)
                        {
                            // Read UTF-16 Little-endian file.
                            fileEncoding = new UnicodeEncoding(false, true);
                        }
                        else
                        {
                            // Read UTF-8 file.
                            fileEncoding = new UTF8Encoding();
                        }
                    } // if (encoding == null)

                    // Decode text and encode into ASCII.
                    char[] textin = fileEncoding.GetChars(state.buffer, 0, bytesRead);
                    sendBuffer = serverEncoding_.GetBytes(textin);
                    sendCount = sendBuffer.Length;
                } // if (state.isBinary) ... else

                // Transmit buffer to print server.
                clientConnection_.GetStream().Write(sendBuffer, 0, sendCount);

                // Update transfer counters.
                state.sizeUsed += bytesRead;
                jobSizeUsed_ += bytesRead;
                if (LprJobProgressChanged != null)
                {
                    // Notify subscribers of file transfer progress.
                    LprJobProgressEventArgs args = new LprJobProgressEventArgs();
                    args.Pathname = state.pathname;
                    args.ServerFilename = state.serverFilename;
                    args.Total = jobSizeTotal_;
                    args.Used = jobSizeUsed_;
                    args.Command = ServerCommands.SubmitJob;
                    LprJobProgressChanged(this, args);
                }
            } // for ( ; ; )
            */

            // Transmit buffer to print server.
            _clientConnection.GetStream().Write(state.Data.Data, 0, state.Data.Data.Length);

            if (Protocol == LprProtocols.LPR ||
                    Protocol == LprProtocols.LPRng) {
                // LPR and LPRng require terminating null and acknowledgement.
                // Send terminating zero (null) octet.
                buffer[0] = 0;
                _clientConnection.GetStream().Write(buffer, 0, 1);

                // Get acknowledgement from server.
                int acknowledgement = GetResponseCode();
                result = (acknowledgement == 0);
                if (acknowledgement != 0) {
                    // Save information about the error.
                    LprException ex = new LprException(
                        String.Format("lpr error {0} sending file data",
                        acknowledgement));
                    ex.ErrorCode = acknowledgement;
                    LastException = ex;
                }
            }
            else {
                // Direct protocol requires no additional data.
                result = true;
            }


            return result;
        }

  
        #endregion

    }

    #region Exceptions

    public class LprException : Exception
    {
        public int ErrorCode = 0;
        public String Pathname = "";

        public LprException() { }

        public LprException(String message)
            : base(message)
        {
        }

    }

    #endregion

}
