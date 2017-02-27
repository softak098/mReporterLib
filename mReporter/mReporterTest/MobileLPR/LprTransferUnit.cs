////////////////////////////////////////////////////////////////////////////////
// File:    LprTransferState.cs
// Purpose: Class containing file transfer information.
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
using System.IO;

namespace MobileLPR
{
    public class LprTransferUnit
    {

        #region Private Members

        public bool IsBinary = false;


        public LprData Data;

        public string UnitName { get; private set; }
        public string Content = null;
        public int ContentSize => Content?.Length ?? 0;

        #endregion

        public LprTransferUnit(string unitName)
        {

            UnitName = unitName;

        }

        public LprTransferUnit(LprData lprData)
        {
            Data = lprData;
            IsBinary = true;

            //this.pathname = pathname;
            //FileInfo f = new FileInfo(pathname);
            //this.sizeTotal = f.Length;
        }

    }
}
