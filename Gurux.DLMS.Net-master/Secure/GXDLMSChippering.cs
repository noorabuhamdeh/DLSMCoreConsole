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

using System;
using System.Linq;
using Gurux.DLMS.Enums;
using Gurux.DLMS.Internal;
using Gurux.DLMS.Objects.Enums;

namespace Gurux.DLMS.Secure
{
    internal class GXDLMSChippering
    {
        /// <summary>
        /// Get Nonse from frame counter and system title.
        /// </summary>
        /// <param name="invocationCounter">Invocation counter.</param>
        /// <param name="systemTitle">System title.</param>
        /// <returns></returns>
        static byte[] GetNonse(UInt32 invocationCounter, byte[] systemTitle)
        {
            byte[] nonce = new byte[12];
            systemTitle.CopyTo(nonce, 0);
            byte[] tmp = BitConverter.GetBytes(invocationCounter).Reverse().ToArray();
            tmp.CopyTo(nonce, 8);
            return nonce;
        }

        static internal byte[] EncryptAesGcm(AesGcmParameter param, byte[] plainText)
        {
            System.Diagnostics.Debug.WriteLine("Encrypt settings: " + param.ToString());
            param.CountTag = null;
            GXByteBuffer data = new GXByteBuffer();
            if (param.Type == CountType.Packet)
            {
                data.SetUInt8((byte)param.Security);
            }
            byte[] tmp = BitConverter.GetBytes((UInt32)param.InvocationCounter).Reverse().ToArray();
            byte[] aad = GetAuthenticatedData(param, plainText);
            GXDLMSChipperingStream gcm = new GXDLMSChipperingStream(param.Security, true, param.BlockCipherKey,
                    aad, GetNonse((UInt32)param.InvocationCounter, param.SystemTitle), null);
            // Encrypt the secret message
            if (param.Security != (byte)Security.Authentication)
            {
                gcm.Write(plainText);
            }
            byte[] ciphertext = gcm.FlushFinalBlock();
            if (param.Security == (byte)Security.Authentication)
            {
                if (param.Type == CountType.Packet)
                {
                    data.Set(tmp);
                }
                if ((param.Type & CountType.Data) != 0)
                {
                    data.Set(plainText);
                }
                if ((param.Type & CountType.Tag) != 0)
                {
                    param.CountTag = gcm.GetTag();
                    data.Set(param.CountTag);
                }
            }
            else if (param.Security == (byte)Security.Encryption)
            {
                if (param.Type == CountType.Packet)
                {
                    data.Set(tmp);
                }
                data.Set(ciphertext);
            }
            else if (param.Security == (byte)Security.AuthenticationEncryption)
            {
                if (param.Type == CountType.Packet)
                {
                    data.Set(tmp);
                }
                if ((param.Type & CountType.Data) != 0)
                {
                    data.Set(ciphertext);
                }
                if ((param.Type & CountType.Tag) != 0)
                {
                    param.CountTag = gcm.GetTag();
                    data.Set(param.CountTag);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException("security");
            }
            if (param.Type == CountType.Packet)
            {
                GXByteBuffer tmp2 = new GXByteBuffer((ushort)(10 + data.Size));
                tmp2.SetUInt8(param.Tag);
                if (param.Tag == (int)Command.GeneralGloCiphering ||
                    param.Tag == (int)Command.GeneralDedCiphering ||
                    param.Tag == (int)Command.DataNotification)
                {
                    if (!param.IgnoreSystemTitle)
                    {
                        GXCommon.SetObjectCount(param.SystemTitle.Length, tmp2);
                        tmp2.Set(param.SystemTitle);
                    }
                    else
                    {
                        tmp2.SetUInt8(0);
                    }
                }
                GXCommon.SetObjectCount(data.Size, tmp2);
                tmp2.Set(data.Array());
                return tmp2.Array();
            }
            byte[] crypted = data.Array();
            System.Diagnostics.Debug.WriteLine("Crypted: " + GXCommon.ToHex(crypted, true));
            return crypted;
        }

        private static byte[] GetAuthenticatedData(AesGcmParameter p, byte[] plainText)
        {
            if (p.Security == (byte)Security.Authentication)
            {
                GXByteBuffer tmp2 = new GXByteBuffer();
                tmp2.SetUInt8((byte)p.Security);
                tmp2.Set(p.AuthenticationKey);
                tmp2.Set(plainText);
                return tmp2.Array();
            }
            else if (p.Security == (byte)Security.Encryption)
            {
                return p.AuthenticationKey;
            }
            else if (p.Security == (byte) Security.AuthenticationEncryption)
            {
                GXByteBuffer tmp2 = new GXByteBuffer();
                tmp2.SetUInt8((byte)p.Security);
                tmp2.Set(p.AuthenticationKey);
                return tmp2.Array();
            }
            return null;
        }

        /// <summary>
        /// Decrypt data.
        /// </summary>
        /// <param name="p">Decryption parameters</param>
        /// <returns>Decrypted data.</returns>
        public static byte[] DecryptAesGcm(AesGcmParameter p, GXByteBuffer data)
        {
            if (data == null || data.Size < 2)
            {
                throw new ArgumentOutOfRangeException("cryptedData");
            }
            byte[] tmp;
            int len;
            Command cmd = (Command)data.GetUInt8();
            switch (cmd)
            {
                case Command.GeneralGloCiphering:
                case Command.GeneralDedCiphering:
                    len = GXCommon.GetObjectCount(data);
                    if (len != 0)
                    {
                        p.SystemTitle = new byte[len];
                        data.Get(p.SystemTitle);
                        if (p.Xml != null && p.Xml.Comments)
                        {
                            p.Xml.AppendComment(GXCommon.SystemTitleToString(Standard.DLMS, p.SystemTitle, true));
                        }
                    }
                    if (p.SystemTitle == null || p.SystemTitle.Length != 8)
                    {
                        if (p.Xml == null)
                        {
                            throw new ArgumentNullException("Invalid sender system title.");
                        }
                        else
                        {
                            p.Xml.AppendComment("Invalid sender system title.");
                        }
                    }
                    break;
                case Command.GeneralCiphering:
                case Command.GloInitiateRequest:
                case Command.GloInitiateResponse:
                case Command.GloReadRequest:
                case Command.GloReadResponse:
                case Command.GloWriteRequest:
                case Command.GloWriteResponse:
                case Command.GloGetRequest:
                case Command.GloGetResponse:
                case Command.GloSetRequest:
                case Command.GloSetResponse:
                case Command.GloMethodRequest:
                case Command.GloMethodResponse:
                case Command.GloEventNotification:
                case Command.DedInitiateRequest:
                case Command.DedInitiateResponse:
                case Command.DedGetRequest:
                case Command.DedGetResponse:
                case Command.DedSetRequest:
                case Command.DedSetResponse:
                case Command.DedMethodRequest:
                case Command.DedMethodResponse:
                case Command.DedEventNotification:
                case Command.DedReadRequest:
                case Command.DedReadResponse:
                case Command.DedWriteRequest:
                case Command.DedWriteResponse:
                case Command.GloConfirmedServiceError:
                case Command.DedConfirmedServiceError:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("cryptedData");
            }
            int value = 0;
            UInt64 transactionId = 0;
            if (cmd == Command.GeneralCiphering)
            {
                len = GXCommon.GetObjectCount(data);
                tmp = new byte[len];
                data.Get(tmp);
                GXByteBuffer t = new GXByteBuffer(tmp);
                transactionId = t.GetUInt64();
                len = GXCommon.GetObjectCount(data);
                if (len != 0)
                {
                    tmp = new byte[len];
                    data.Get(tmp);
                    p.SystemTitle = tmp;
                }
                if (p.SystemTitle == null || p.SystemTitle.Length != 8)
                {
                    if (p.Xml == null)
                    {
                        throw new ArgumentNullException("Invalid sender system title.");
                    }
                    else
                    {
                        p.Xml.AppendComment("Invalid sender system title.");
                    }
                }
                len = GXCommon.GetObjectCount(data);
                tmp = new byte[len];
                data.Get(tmp);
                p.RecipientSystemTitle = tmp;
                // Get date time.
                len = GXCommon.GetObjectCount(data);
                if (len != 0)
                {
                    tmp = new byte[len];
                    data.Get(tmp);
                    p.DateTime = tmp;
                }
                // other-information
                len = data.GetUInt8();
                if (len != 0)
                {
                    tmp = new byte[len];
                    data.Get(tmp);
                    p.OtherInformation = tmp;
                }
                // KeyInfo OPTIONAL
                len = data.GetUInt8();
                // AgreedKey CHOICE tag.
                data.GetUInt8();
                // key-parameters
                len = data.GetUInt8();
                value = data.GetUInt8();
                p.KeyParameters = value;
                if (value == 1)
                {
                    // KeyAgreement.ONE_PASS_DIFFIE_HELLMAN
                    // key-ciphered-data
                    len = GXCommon.GetObjectCount(data);
                    tmp = new byte[len];
                    data.Get(tmp);
                    p.KeyCipheredData = tmp;
                }
                else if (value == 2)
                {
                    // KeyAgreement.STATIC_UNIFIED_MODEL
                    len = GXCommon.GetObjectCount(data);
                    if (len != 0)
                    {
                        throw new ArgumentException("Invalid key parameters");
                    }
                }
                else
                {
                    throw new ArgumentException("key-parameters");
                }
            }
            len = GXCommon.GetObjectCount(data);
            p.CipheredContent = data.Remaining();
            byte sc = (byte)data.GetUInt8();
            Enums.Security security = (Enums.Security)(sc & 0x30);
            if ((sc & 0x80) != 0)
            {
                System.Diagnostics.Debug.WriteLine("Compression is used.");
            }
            if ((sc & 0x40) != 0)
            {
                System.Diagnostics.Debug.WriteLine("Error: Key_Set is used.");
            }
            if ((sc & 0x20) != 0)
            {
                System.Diagnostics.Debug.WriteLine("Encryption is applied.");
            }

            SecuritySuite ss = (SecuritySuite)(sc & 0x3);
            p.Security = (byte)security;
            UInt32 invocationCounter = data.GetUInt32();
            p.InvocationCounter = invocationCounter;
            if (ss == SecuritySuite.Version2)
            {
                throw new NotImplementedException("Security Suite 2 is not implemented.");
            }
            System.Diagnostics.Debug.WriteLine("Decrypt settings: " + p.ToString());
            System.Diagnostics.Debug.WriteLine("Encrypted: " + GXCommon.ToHex(data.Data,
                    false, data.Position, data.Size - data.Position));
            byte[] tag = new byte[12];
            byte[] encryptedData;
            int length;
            if (security == Enums.Security.Authentication)
            {
                length = data.Size - data.Position - 12;
                encryptedData = new byte[length];
                data.Get(encryptedData);
                data.Get(tag);
                // Check tag.
                EncryptAesGcm(p, encryptedData);
                if (!GXDLMSChipperingStream.TagsEquals(tag, p.CountTag))
                {
                    if (transactionId != 0)
                    {
                        p.InvocationCounter = transactionId;
                    }
                    if (p.Xml == null)
                    {
                        throw new GXDLMSException("Decrypt failed. Invalid tag.");
                    }
                    else
                    {
                        p.Xml.AppendComment("Decrypt failed. Invalid tag.");
                    }
                }
                return encryptedData;
            }
            byte[] ciphertext = null;
            if (security == Enums.Security.Encryption)
            {
                length = data.Size - data.Position;
                ciphertext = new byte[length];
                data.Get(ciphertext);
            }
            else if (security == Enums.Security.AuthenticationEncryption)
            {
                length = data.Size - data.Position - 12;
                ciphertext = new byte[length];
                data.Get(ciphertext);
                data.Get(tag);
            }
            byte[] aad = GetAuthenticatedData(p, ciphertext),
                    iv = GetNonse(invocationCounter, p.SystemTitle);
            GXDLMSChipperingStream gcm = new GXDLMSChipperingStream((byte)security, true,
                    p.BlockCipherKey, aad, iv, tag);
            gcm.Write(ciphertext);
            if (transactionId != 0)
            {
                p.InvocationCounter = transactionId;
            }
            return gcm.FlushFinalBlock();
        }
    }
}
