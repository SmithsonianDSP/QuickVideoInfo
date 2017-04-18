using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ACM = Android.Content.PM;
using Java.Security;

namespace YTII.Droid.App
{
    internal static class SignatureVerification
    {
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: public static String getSignature(@NonNull PackageManager pm, @NonNull String packageName)
        public static string GetSignature(ACM.PackageManager pm, string packageName)
        {
            try
            {
                ACM.PackageInfo packageInfo = pm.GetPackageInfo(packageName, ACM.PackageInfoFlags.Signatures);
                if (packageInfo == null || packageInfo.Signatures == null || packageInfo.Signatures.Count == 0 || packageInfo.Signatures[0] == null)
                {
                    return null;
                }
                return SignatureDigest(packageInfo.Signatures[0]);
            }
            catch (ACM.PackageManager.NameNotFoundException)
            {
                return null;
            }
        }

        private static string SignatureDigest(ACM.Signature sig)
        {
            byte[] signature = sig.ToByteArray();
            try
            {
                MessageDigest md = MessageDigest.GetInstance("SHA1");
                byte[] digest = md.Digest(signature);
                return ByteArrayToString(digest);
            }
            catch (NoSuchAlgorithmException)
            {
                return null;
            }

            string ByteArrayToString(byte[] ba)
            {
                string hex = BitConverter.ToString(ba);
                hex = hex.Replace("-", ":");
                return hex;
            }
        }

    }
}