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

using Gurux.DLMS.ASN.Enums;
using Gurux.DLMS.Ecdsa;
using Gurux.DLMS.Internal;
using Gurux.DLMS.Secure;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Gurux.DLMS.ASN
{
    /// <summary>
    /// Pkcs8 certification request. Private key is saved using this format.
    /// </summary>
    /// <remarks>
    /// https://tools.ietf.org/html/rfc5208
    /// </remarks>
    public class GXPkcs8
    {
        /// <summary>
        /// Private key version.
        /// </summary>
        public CertificateVersion Version
        {
            get;
            set;
        }

        /// <summary>
        /// Algorithm.
        /// </summary>
        private object Algorithm
        {
            get;
            set;
        }

        /// <summary>
        /// Private key.
        /// </summary>
        public GXPrivateKey PrivateKey
        {
            get;
            private set;
        }

        /// <summary>
        /// Public key.
        /// </summary>
        public GXPublicKey PublicKey
        {
            get;
            private set;
        }

        public byte[] Encoded
        {
            get
            {
                GXAsn1Sequence d = new GXAsn1Sequence();
                d.Add((sbyte)Version);
                GXAsn1Sequence d1 = new GXAsn1Sequence();
                d1.Add(new GXAsn1ObjectIdentifier(X9ObjectIdentifierConverter.GetString((X9ObjectIdentifier)Algorithm)));
                d1.Add(new GXAsn1ObjectIdentifier("1.2.840.10045.3.1.7"));
                d.Add(d1);
                GXAsn1Sequence d2 = new GXAsn1Sequence();
                d2.Add((sbyte)1);
                d2.Add(PrivateKey.RawValue);
                d.Add(d2);
                return GXAsn1Converter.ToByteArray(d);
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public GXPkcs8()
        {
            Version = CertificateVersion.Version1;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="data">Base64 string. </param>
        public GXPkcs8(string data)
        {
            string tmp = data.Replace("-----BEGIN PRIVATE KEY-----", "");
            tmp = tmp.Replace("-----END PRIVATE KEY-----", "");
            tmp = tmp.Replace("-----BEGIN EC PRIVATE KEY-----", "");
            tmp = tmp.Replace("-----END EC PRIVATE KEY-----", "");
            Init(GXCommon.FromBase64(tmp.Trim()));
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="data">Encoded bytes. </param>
        public GXPkcs8(byte[] data)
        {
            Init(data);
        }

        private void Init(byte[] data)
        {
            GXAsn1Sequence seq = (GXAsn1Sequence)GXAsn1Converter.FromByteArray(data);
            if (seq.Count < 3)
            {
                throw new System.ArgumentException("Wrong number of elements in sequence.");
            }
            Version = (CertificateVersion)seq[0];
            GXAsn1Sequence tmp = (GXAsn1Sequence)seq[1];
            Algorithm = X9ObjectIdentifierConverter.FromString(tmp[0].ToString());
            if ((X9ObjectIdentifier)Algorithm == X9ObjectIdentifier.None)
            {
                Algorithm = PkcsObjectIdentifierConverter.FromString(tmp[0].ToString());
            }
            PrivateKey = GXPrivateKey.FromRawBytes((byte[])((GXAsn1Sequence)seq[2])[1]);
            if (PrivateKey == null)
            {
                throw new Exception("Invalid private key.");
            }
            PublicKey = PrivateKey.GetPublicKey();
        }

        public override sealed string ToString()
        {
            StringBuilder bb = new StringBuilder();
            bb.Append("PKCS #8:");
            bb.Append("\n");
            bb.Append("Version: ");
            bb.Append(Version.ToString());
            bb.Append("\n");
            bb.Append("Algorithm: ");
            if (Algorithm != null)
            {
                bb.Append(Algorithm.ToString());
            }
            bb.Append("\r\n");
            return bb.ToString();
        }

        /// <summary>Load private key from the PEM file.
        /// </summary>
        /// <param name="path">File path. </param>
        /// <returns> Created GXPkcs8 object. </returns>
        public static GXPkcs8 Load(string path)
        {
            return new GXPkcs8(File.ReadAllText(path));
        }

        ///
        ///     <summary>Save private key to PEM file.
        /// </summary>
        ///  <param name="path">
        ///           File path. </param>
        ///
        public virtual void Save(string path)
        {
            StringBuilder sb = new StringBuilder();
            if (PrivateKey != null)
            {
                sb.Append("-----BEGIN EC PRIVATE KEY-----" + Environment.NewLine);
                sb.Append(ToPem());
                sb.Append(Environment.NewLine + "-----END EC PRIVATE KEY-----");
                File.WriteAllText(path, sb.ToString());
            }
            else
            {
                throw new System.ArgumentException("Public or private key is not set.");
            }
        }

        /// <summary>
        /// Private key in PEM format.
        /// </summary>
        /// <returns>Private key as in PEM string.</returns>
        public string ToPem()
        {
            return GXCommon.ToBase64(Encoded);
        }
    }

}