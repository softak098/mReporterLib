////////////////////////////////////////////////////////////////////////////////
// File:    LprDataFile.cs
// Purpose: Class to keep details about data files to be printed.
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
    /// Class to hold information about a print job data file.
    /// </summary>
    public class LprData
    {

        #region Private Members

        byte[] _data = null;

        private LprDataFormats format_ = LprDataFormats.Literal;

        #endregion

        #region Constructors

        public LprData(byte[] data)
        {
            _data = data;
        }

        #endregion

        #region Properties

        public byte[] Data { get { return _data; } }
        public string Name { get { return "JOB-1"; } }

        /// <summary>
        /// Get or set the print format of the data file.
        /// </summary>
        public LprDataFormats Format
        {
            get { return format_; }
            set { format_ = value; }
        }

        /// <summary>
        /// Get the binary mode of the assigned data format.
        /// </summary>
        public Boolean IsBinary
        {
            get { return IsDataFormatBinary(format_); }
        }

        /// <summary>
        /// Get the format command string for the data file.
        /// </summary>
        public String FormatCommand
        {
            get { return GetFormatCommand(format_); }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get translation mode used for print format.
        /// </summary>
        /// <param name="format">Data format to be printed.</param>
        /// <returns>True to send raw data; false to send text.</returns>
        public static Boolean IsDataFormatBinary(LprDataFormats format)
        {
            Boolean isBinary = true;

            switch (format)
            {
            case LprDataFormats.CIF: isBinary = false; break;
            case LprDataFormats.Ditroff: isBinary = false; break;
            case LprDataFormats.DVI: isBinary = false; break;
            case LprDataFormats.Formatted: isBinary = false; break;
            case LprDataFormats.Fortran: isBinary = false; break;
            case LprDataFormats.Literal: isBinary = true; break;
            case LprDataFormats.Paginate: isBinary = false; break;
            case LprDataFormats.Plot: isBinary = false; break;
            case LprDataFormats.Postscript: isBinary = false; break;
            case LprDataFormats.Raster: isBinary = true; break;
            case LprDataFormats.Troff: isBinary = false; break;
            }

            return isBinary;
        } // GetDataFormatIsBinary(LprDataFormats format)

        /// <summary>
        /// Get character representing print format.
        /// </summary>
        /// <param name="format">Data format to be printed.</param>
        /// <returns>String containing the format command.</returns>
        public static String GetFormatCommand(LprDataFormats format)
        {
            String command = "l";

            switch (format)
            {
            case LprDataFormats.CIF: command = "c"; break;
            case LprDataFormats.Ditroff: command = "n"; break;
            case LprDataFormats.DVI: command = "d"; break;
            case LprDataFormats.Formatted: command = "f"; break;
            case LprDataFormats.Fortran: command = "r"; break;
            case LprDataFormats.Literal: command = "l"; break;
            case LprDataFormats.Paginate: command = "p"; break;
            case LprDataFormats.Plot: command = "g"; break;
            case LprDataFormats.Postscript: command = "o"; break;
            case LprDataFormats.Raster: command = "v"; break;
            case LprDataFormats.Troff: command = "t"; break;
            }

            return command;
        } // GetDataFormatCommand(LprDataFormats format)

        #endregion

    }
}
