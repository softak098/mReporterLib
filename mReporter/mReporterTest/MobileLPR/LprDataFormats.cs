////////////////////////////////////////////////////////////////////////////////
// File:    LprDataFormats.cs
// Purpose: Enumeration used to identify data format for printing.
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
    /// LPR data format codes.
    /// </summary>
    public enum LprDataFormats
    {
        Literal,        // Literal (raw, binary) file.
        CIF,            // Caltech Interchange Format plot file.
        Ditroff,        // ditroff output file.
        DVI,            // DVI (TeX output) file.
        Formatted,      // Plain text file to be formatted.
        Fortran,        // Plain text with FORTRAN carriage control.
        Paginate,       // Text to process through 'pr'.
        Plot,           // Berkely Unix plot file.
        Postscript,     // Standard Postscript file.
        Raster,         // Sun raster image file.
        Troff           // Graphic Systems C/A/T phototypesetter file.
    };

}
