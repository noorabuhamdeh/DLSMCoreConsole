//
// --------------------------------------------------------------------------
//  Gurux Ltd
//
//
//
// Filename:        $HeadURL$
//
// Version:         $Revision$,
//                  $Date$
//                  $Author$
//
// Copyright (c) Gurux Ltd
//
//---------------------------------------------------------------------------
//
//  DESCRIPTION
//
// This file is a part of Gurux Device Framework.
//
// Gurux Device Framework is Open Source software; you can redistribute it
// and/or modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; version 2 of the License.
// Gurux Device Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License for more details.
//
// More information of Gurux products: https://www.gurux.org
//
// This code is licensed under the GNU General Public License v2.
// Full text may be retrieved at http://www.gnu.org/licenses/gpl-2.0.txt
//---------------------------------------------------------------------------

namespace Gurux.DLMS.Objects.Enums
{
    //Security suite Specifies authentication, encryption and key wrapping algorithm.
    public enum SecuritySuite
    {
        /// <summary>
        /// AES-GCM-128 for authenticated encryption and AES-128 for key wrapping.
        /// </summary>
        /// <remarks>
        /// A.K.A Security Suite 0.
        /// </remarks>
        Version0,
        /// <summary>
        /// ECDH-ECDSAAES-GCM-128SHA-256.
        /// </summary>
        /// <remarks>
        /// A.K.A Security Suite 1.
        /// </remarks>
        Version1,
        /// <summary>
        /// ECDH-ECDSAAES-GCM-256SHA-384.
        /// </summary>
        /// <remarks>
        /// A.K.A Security Suite 2.
        /// </remarks>
        Version2
    }
}
