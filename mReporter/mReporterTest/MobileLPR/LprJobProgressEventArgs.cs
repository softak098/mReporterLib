////////////////////////////////////////////////////////////////////////////////
// File:    LprJobProgressEventArgs.cs
// Purpose: Class containing print job progress event data.
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
////////////////////////////////////////////////////////////////////////////////

using System;

using System.Collections.Generic;
using System.Text;

namespace MobileLPR
{
    /// <summary>
    /// Class containing print job progress event data.
    /// </summary>
    public class LprJobProgressEventArgs : EventArgs
    {

        #region Private Members

        /// <summary>
        /// Local pathname of file being transferred.
        /// </summary>
        private String pathname_ = String.Empty;

        /// <summary>
        /// Server name of file being transferred.
        /// </summary>
        private String serverFilename_ = String.Empty;

        /// <summary>
        /// Total number of local bytes to be sent for the job.
        /// </summary>
        private Int64 total_ = 0;

        /// <summary>
        /// Current number of bytes sent for the job.
        /// </summary>
        private Int64 used_ = 0;
        
        /// <summary>
        /// Type of command for which progress is being reported.
        /// </summary>
        private LprJob.ServerCommands command_ = LprJob.ServerCommands.None;

        #endregion

        #region Properties

        /// <summary>
        /// Get or set local pathname of file being transferred.
        /// </summary>
        public String Pathname
        {
            get { return pathname_; }
            set { pathname_ = value; }
        }

        /// <summary>
        /// Get or set server name of file being transferred.
        /// </summary>
        public String ServerFilename
        {
            get { return serverFilename_; }
            set { serverFilename_ = value; }
        }

        /// <summary>
        /// Get or set total number of local bytes to be sent for the job.
        /// </summary>
        public Int64 Total
        {
            get { return total_; }
            set { total_ = value; }
        }

        /// <summary>
        /// Get or set current number of bytes sent for the job.
        /// </summary>
        public Int64 Used
        {
            get { return used_; }
            set { used_ = value; }
        }

        /// <summary>
        /// Get or set type of command for which progress is being reported.
        /// </summary>
        public LprJob.ServerCommands Command
        {
            get { return command_; }
            set { command_ = value; }
        }

        #endregion

    }

}
