﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andriy.MyChat.Server
{
    using Andriy.Security.Cryptography;

    public class CustomSettings
    {
        private static readonly byte[] SettKey = { 0x21, 0x2F, 0x4F, 0x5E, 0xAD, 0x54, 0x39, 0x33, 0x01, 0x91, 0x1E, 0xD2, 0x33, 0x04, 0x00, 0x29, 0x37, 0xA3, 0x6B, 0xA0, 0xC6, 0x3F, 0xAA, 0x7B, 0x66, 0x70, 0x04, 0x0E, 0x91, 0x44, 0x8E, 0x16 };//32 bytes
        private static readonly byte[] SettIv = { 0x3B, 0x12, 0x5C, 0x30, 0xAC, 0x4F, 0x80, 0xC8, 0x25, 0xA1, 0x33, 0xC3, 0x13, 0x3E, 0xC6, 0xB4 };//16 bytes
        
        public string ConnectionString
        {
            get
            {
                var aes1 = new AESCSPImpl(SettKey, SettIv);
                try
                {
                    return aes1.DecryptStr(ConvertUtils.ToBytes(Properties.Settings.Default.String1));
                }
                finally
                {
                    aes1.Clear();
                }
            }

            set
            {
                var aes1 = new AESCSPImpl(SettKey, SettIv);
                try
                {
                    Properties.Settings.Default.String1 = ConvertUtils.ToString(aes1.EncryptStr(value));
                }
                finally
                {
                    aes1.Clear();
                }
            }
        }
    }
}