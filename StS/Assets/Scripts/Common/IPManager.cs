using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

public abstract class IPManager
{
    public static string GetIP(ADDRESSFAM Addfam)
    {
        //Return null if ADDRESSFAM is Ipv6 but Os does not support it
        if (Addfam == ADDRESSFAM.IPv6 && !Socket.OSSupportsIPv6)
        {
            return null;
        }

        string output = "";

        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            NetworkInterfaceType _type1 = NetworkInterfaceType.Wireless80211;
            NetworkInterfaceType _type2 = NetworkInterfaceType.Ethernet;

            if ((item.NetworkInterfaceType == _type1 || item.NetworkInterfaceType == _type2) && item.OperationalStatus == OperationalStatus.Up)
#endif 
            {
                foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                {
                    //IPv4
                    if (Addfam == ADDRESSFAM.IPv4)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                        }
                    }

                    //IPv6
                    else if (Addfam == ADDRESSFAM.IPv6)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            output = ip.Address.ToString();
                        }
                    }
                }
            }
        }
        return output;
    }

    public static bool ValidateIPv4(string ipString)
    {
        if (String.IsNullOrWhiteSpace(ipString))
        {
            return false;
        }

        string[] splitValues = ipString.Split('.');
        if (splitValues.Length != 4)
        {
            return false;
        }

        byte tempForParsing;

        return splitValues.All(r => byte.TryParse(r, out tempForParsing));
    }

    public static string EncryptIPtoHexa(string ipAddress)
    {
        string dotsPosition = "";
        string encryptedIP = "";

        char[] arr;
        arr = ipAddress.ToCharArray();
        
        for (int i = 0; i < arr.Count(); i++)
        {
            if (arr[i] == Convert.ToChar("."))
            {
                dotsPosition += i;
            } else
            {
                encryptedIP += arr[i];
            }
        }

        encryptedIP = dotsPosition + encryptedIP;

        // TO DO

        return encryptedIP;
    }

    public static string ConvertToBase36(ulong value)
    {
        string result = "";
        ulong Base = 36;
        string Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        while (value > 0)
        {
            int index = Convert.ToInt32(value % Base);
            result = Chars[index] + result; // use StringBuilder for better performance
            value /= Base;
        }

        return result;
    }

    public static string ConvertToBase10(int value)
    {
        string result = "";
        int Base = 10;
        string Chars = "0123456789";

        while (value > 0)
        {
            result = Chars[value % Base] + result; // use StringBuilder for better performance
            value /= Base;
        }

        return result;
    }
}

public enum ADDRESSFAM
{
    IPv4, IPv6
}