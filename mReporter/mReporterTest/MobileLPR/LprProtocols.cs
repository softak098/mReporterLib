////////////////////////////////////////////////////////////////////////////////
// File:    LprProtocols.cs
// Purpose: Enumeration to identify job transfer protocols.
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
    /// Print job transfer protocols.
    /// </summary>
    public enum LprProtocols
    {
        Direct,     // Direct connection; job control structures.
        LPR,        // RFC 1179 line printer protocol.
        LPRng       // Enhanced line printer protocol.
    };

}
