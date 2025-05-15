// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("EZKck6MRkpmREZKSkzffkIdYJMDxOSEaB82MBQOsMgXRa7/nkwOb2aMRkrGjnpWauRXbFWSekpKSlpOQ5EndVLJWVuwqFMYwiK2ZhyTRdeiLuu2swcsJtM3ZUq9Tx0v58olEcIN8yjbg7FhMgjR2wZR4NP573HCwx6EiaVFFrLGz7Fi7B0z/vIWP+6cB2ted63t0gt6pjW/M+ozTs+n8Wz7/sMEifWvCDR5/yZ4afyydJ3V/2CNBNPtSlzDHIA95i0EdSgQzwpWzu30aZwpd9BMomtVw4ns9AbIm/9SNQc0HInjAoWF7RV3vLZJ2X2UbuZPIFFIEFCDm7Ooz6/HOPBm/fuPn8v2uyX1L1k69gzgC+3hGSLL6BM11HluuSg44hJGQkpOS");
        private static int[] order = new int[] { 1,4,4,10,4,7,7,11,9,12,13,12,12,13,14 };
        private static int key = 147;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
