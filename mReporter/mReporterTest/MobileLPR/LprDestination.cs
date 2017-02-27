////////////////////////////////////////////////////////////////////////////////
// File:    LprDestination.cs
// Purpose: Class to parse components of a printer URI.
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
using System.Text.RegularExpressions;

namespace MobileLPR
{
    /// <summary>
    /// Helper class to parse a printer URI. Valid printer URI syntax is:
    ///     [protocol://][username@]servername[:serverport][/printername]
    /// </summary>
    public class LprDestination
    {

        #region Private Members

        private Uri uri_ = null;

        private LprProtocols protocol_ = LprProtocols.LPR;

        private String userInfo_ = String.Empty;

        private String serverName_ = String.Empty;

        private Int32 serverPort_ = 0;

        private String printerName_ = String.Empty;

        #endregion

        #region Properties

        public LprProtocols Protocol
        {
            get { return protocol_; }
            set { protocol_ = value; }
        }

        public String UserName
        {
            get { return userInfo_; }
            set { userInfo_ = value; }
        }

        public String ServerName
        {
            get { return serverName_; }
            set { serverName_ = value; }
        }

        public Int32 ServerPort
        {
            get { return serverPort_; }
            set { serverPort_ = value; }
        }

        public String PrinterName
        {
            get { return printerName_; }
            set { printerName_ = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize an instance of the LprDestination class.
        /// </summary>
        /// <param name="destination">Uniform Resource Indicator to be parsed.</param>
        /// <exception cref="UriException">when URI string is invalid.</exception>
        /// <remarks>
        /// Parses a valid URI string into components usable for print jobs. A
        /// valid URI is parsed to set these properties:
        ///     Protocol: Protocol to use when transferring job to print server.
        ///     UserName: User Name of user creating the job.
        ///     ServerName: Server Name (LPD host) assigned to the print job.
        ///     ServerPort: Server Port (LPD port) assigned to the print job.
        ///     PrinterName: Printer Name (LPD queue) assigned to the print job.
        /// Valid URI string for the print job destination is:
        ///     [protocol://][username@]servername[:serverport][/printername]
        /// </remarks>
        public LprDestination(String destination)
        {
            uri_ = new Uri(destination);

            protocol_ = GetProtocol();
            userInfo_ = GetUserInfo();
            serverName_ = GetServerName();
            serverPort_ = GetServerPort();
            printerName_ = GetPrinterName();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Convert URI scheme to a Protocol code.
        /// </summary>
        /// <returns>Specified or default protocol code.</returns>
        private LprProtocols GetProtocol()
        {
            switch (uri_.Scheme)
            {
            case "lprng":   return LprProtocols.LPRng;

            case "lpr":
            case "lpd":     return LprProtocols.LPR;
            
            case "direct":
            case "raw":     return LprProtocols.Direct;
            }

            return LprProtocols.LPR;
        } // GetProtocol(String scheme)

        /// <summary>
        /// Get server name portion of URI.
        /// </summary>
        /// <returns>Server name string.</returns>
        private String GetServerName()
        {
            return uri_.Host;
        } // GetServerName()

        /// <summary>
        /// Get user info portion of URI.
        /// </summary>
        /// <returns>User info string.</returns>
        private String GetUserInfo()
        {
            return uri_.UserInfo;
        } // GetUserInfo()

        /// <summary>
        /// Get server port portion of URI.
        /// </summary>
        /// <returns>Specified or default port number for protocol.</returns>
        private Int32 GetServerPort()
        {
            if (uri_.IsDefaultPort)
            {
                // User default port when none was specified.
                LprProtocols protocol = GetProtocol();

                switch (protocol)
                {
                case LprProtocols.LPR:      return 515;
                case LprProtocols.LPRng:    return 515;
                case LprProtocols.Direct:   return 9100;
                }
            }

            return uri_.Port;
        } // GetServerPort()

        /// <summary>
        /// Get printer name portion of URI.
        /// </summary>
        /// <returns>Specified printer name.</returns>
        private String GetPrinterName()
        {
            String temp = uri_.AbsolutePath;

            // Remove leading path separators.
            if (temp.StartsWith("/"))
            {
                temp = temp.Substring(1);
            }

            return temp;
        } // GetPrinterName()

        #endregion

    }
}
