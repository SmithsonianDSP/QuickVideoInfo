using System;
using Android.Content.PM;
using Java.Security;

namespace YTII.Droid.App
{
    internal static class SignatureVerification
    {
        //ORIGINAL LINE: public static String getSignature(@NonNull PackageManager pm, @NonNull String packageName)
        public static string GetSignature(PackageManager pm, string packageName)
        {
            try
            {
                PackageInfo packageInfo = pm.GetPackageInfo(packageName, PackageInfoFlags.Signatures);
                if (packageInfo == null || packageInfo.Signatures == null || packageInfo.Signatures.Count == 0 || packageInfo.Signatures[0] == null)
                {
                    return null;
                }
                return SignatureDigest(packageInfo.Signatures[0]);
            }
            catch (PackageManager.NameNotFoundException)
            {
                return null;
            }
        }

        private static string SignatureDigest(Android.Content.PM.Signature sig)
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