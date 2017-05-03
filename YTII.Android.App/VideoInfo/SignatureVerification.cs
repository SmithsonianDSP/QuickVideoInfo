#region file_header

// QuickVideoInfo - YTII.Android.App - SignatureVerification.cs
// 
// Licensed to the Apache Software Foundation (ASF) under one or more contributor license agreements.  
// See the NOTICE file distributed with this work for additional information regarding copyright ownership.  
// The ASF licenses this file to you under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software distributed under the License is 
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express 
// or implied.  See the License for the specific language governing permissions and limitations under the License.
//  

#endregion

using System;
using Android.Content.PM;
using Java.Security;
using Signature = Android.Content.PM.Signature;

namespace YTII.Droid.App
{
    /// <summary>
    ///     This utility class is used for getting the application's SHA1 signature fingerprint for authentication purposes
    /// </summary>
    internal static class SignatureVerification
    {
        //ORIGINAL LINE: public static String getSignature(@NonNull PackageManager pm, @NonNull String packageName)
        public static string GetSignature(PackageManager pm, string packageName)
        {
            try
            {
                var packageInfo = pm.GetPackageInfo(packageName, PackageInfoFlags.Signatures);
                if (packageInfo?.Signatures == null || packageInfo.Signatures.Count == 0 || packageInfo.Signatures[0] == null)
                    return null;
                return SignatureDigest(packageInfo.Signatures[0]);
            }
            catch (PackageManager.NameNotFoundException)
            {
                return null;
            }
        }

        static string SignatureDigest(Signature sig)
        {
            var signature = sig.ToByteArray();
            try
            {
                var md = MessageDigest.GetInstance("SHA1");
                var digest = md.Digest(signature);
                return ByteArrayToString(digest);
            }
            catch (NoSuchAlgorithmException)
            {
                return null;
            }

            string ByteArrayToString(byte[] ba)
            {
                var hex = BitConverter.ToString(ba);
                hex = hex.Replace("-", ":");
                return hex;
            }
        }
    }
}