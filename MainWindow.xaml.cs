using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Net.Sockets;
using System.Windows.Media;

namespace Subnet_Calculator
{
    public partial class MainWindow : Window
    {
        static string IPAddressString = "";
        static string CIDR_Notation = "";
        static string Netmask = "";
        static string Netmask_Wildcard = "";
        static string Netmask_Binary = "";
        static string NetworkAddressString = "";
        static string BroadcastAddress = "";
        static string UsableHostRange = "";
        static string TotalNumberOfHosts = "";
        static string NumberOfUsableHosts = "";
        static string IPClass = "";
        static string PublicORPrivate = "";
        static string NumberOfHostsToMake = "";
        static bool IPAddressStringGotFocus = false;
        static bool Success = false;
        static bool ResetAll = false;
        static int ipoct1, ipoct2, ipoct3, ipoct4;

        //Main
        public MainWindow()
        {
            InitializeComponent();
        }

        //Header drag
        private void WindowHeader_Mousedown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        //Close button
        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        //Minimize button
        private void Minimize_Button_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        //Reset button
        private void Reset_Button_Click(object sender, RoutedEventArgs e)
        {
            ResetAllInfo();
            UpdateDisplay();
            ipField.Text = "Example: \"192.168.0.16/24\" Hover over to see more";
            ipField.Opacity = 0.5;
            IPAddressStringGotFocus = false;
        }

        //Grab my IP button click
        private void GrabMyIP_Button_Click(object sender, RoutedEventArgs e)
        {
            ResetAllInfo();

            //Find local IPv4 by sending a packet to 8.8.8.8 and recieving it
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {

                try
                {
                    socket.Connect("8.8.8.8", 65530);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"You need access to the internet to use this feature\n\n\"{ex.Message}\"", "ERROR");
                    ResetAllInfo();
                    UpdateDisplay();
                    ipField.Text = "Example: \"192.168.0.16/24\" Hover over to see more";
                    ipField.Opacity = 0.5;
                    IPAddressStringGotFocus = false;
                    return;
                }

                IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;
                IPAddressString = endPoint!.Address.ToString();
                Netmask = Convert.ToString(GetSubnetMask(endPoint.Address))!;
            }

            ipField.Text = $"{IPAddressString} {Netmask}";
            ipField.Opacity = 1.0;
            IPAddressStringGotFocus = true;
            FindIPInformation();
            CalculateNetmask();
            FindNetworkInformation();
            UpdateDisplay();
            Success = true;
        }

        //Calculate button click
        private void Calculate_Button_Click(object sender, RoutedEventArgs e)
        {
            ResetAllInfo();

            //Check if the user has ever pressed inside the field
            if (IPAddressStringGotFocus == true)
            {
                //Set IPAddressString to what the user has entered
                IPAddressString = ipField.Text;
                if (IsItAnIP(IPAddressString) == true)
                {
                    if (FindIPInformation() == true)
                    {
                        if (CalculateNetmask() == true)
                        {
                            if (FindNetworkInformation() == true)
                            {
                                UpdateDisplay();
                                Success = true;
                            }
                            else
                            {
                                //ResetAll = true;
                            }
                        }
                        else
                        {
                            //ResetAll = true;
                        }
                    }
                    else
                    {
                        //ResetAll = true;
                    }
                }
                else
                {
                    //ResetAll = true;
                }
            }
            else
            {
                //ResetAll = true;
                MessageBox.Show("You need to enter something!", "ERROR");
            }
            if (ResetAll == true)
            {
                ResetAllInfo();
                UpdateDisplay();
                ipField.Text = "Example: \"192.168.0.16/24\" Hover over to see more";
                ipField.Opacity = 0.5;
                IPAddressStringGotFocus = false;
            }
        }

        //Previous Network
        private void FindPreviousNetwork()
        {

            List<string> UsableRangeList = NetworkAddressString.Split('.').ToList();

            ipoct1 = Convert.ToInt32(UsableRangeList[0]);
            ipoct2 = Convert.ToInt32(UsableRangeList[1]);
            ipoct3 = Convert.ToInt32(UsableRangeList[2]);
            ipoct4 = Convert.ToInt32(UsableRangeList[3]);

            ipoct4--;
            if (ipoct4 < 0)
            {
                ipoct4 = 255;
                ipoct3--;
                if (ipoct3 < 0)
                {
                    ipoct3 = 255;
                    ipoct2--;
                    if (ipoct2 < 0)
                    {
                        ipoct2 = 255;
                        ipoct1--;
                    }
                }
            }


            IPAddressString = $"{ipoct1}.{ipoct2}.{ipoct3}.{ipoct4}";
            ipField.Text = $"{IPAddressString} {Netmask}";
            NetworkAddressString = IPAddressString;

            int[] _bin = new int[32];
            int[] _NetworkAddress = new int[32];
            int[] _BroadcastAddress = new int[32];
            string[] str = new string[4];

            int _cidr = Convert.ToInt32(CIDR_Notation.Trim('/'));
            str = IPAddressString.Split('.');

            //Convert IP to binary
            _bin = Bina(str);

            //Find NA

            for (int i = 0; i <= (31 - (32 - _cidr)); i++)
            {
                _NetworkAddress[i] = _bin[i];
                _BroadcastAddress[i] = _bin[i];
            }

            for (int i = 31; i > (31 - (32 - _cidr)); i--)
            {
                _NetworkAddress[i] = 0;
            }

            int[] _NetworkAddressDecimal = Deci(_NetworkAddress);

            BroadcastAddress = $"{_NetworkAddressDecimal[0]}.{_NetworkAddressDecimal[1]}.{_NetworkAddressDecimal[2]}.{_NetworkAddressDecimal[3]}";

            UsableHostRange = FindUsableHostRange();
            GetIPClass(ipoct1);
            UpdateDisplay();
        }

        //Next Network
        private void FindNextNetwork()
        {

            List<string> UsableRangeList = BroadcastAddress.Split('.').ToList();

            ipoct1 = Convert.ToInt32(UsableRangeList[0]);
            ipoct2 = Convert.ToInt32(UsableRangeList[1]);
            ipoct3 = Convert.ToInt32(UsableRangeList[2]);
            ipoct4 = Convert.ToInt32(UsableRangeList[3]);

            ipoct4++;
            if (ipoct4 > 255)
            {
                ipoct4 = 0;
                ipoct3++;
                if (ipoct3 > 255)
                {
                    ipoct3 = 0;
                    ipoct2++;
                    if (ipoct2 > 255)
                    {
                        ipoct2 = 0;
                        ipoct1++;
                    }
                }
            }


            IPAddressString = $"{ipoct1}.{ipoct2}.{ipoct3}.{ipoct4}";
            ipField.Text = $"{IPAddressString} {Netmask}";
            NetworkAddressString = IPAddressString;


            string[] str = new string[4];
            int[] _bin = new int[32];
            str = IPAddressString.Split('.');
            //Convert IP to binary
            _bin = Bina(str);


            int[] _BroadcastAddress = new int[32];
            int _cidr = Convert.ToInt32(CIDR_Notation.Trim('/'));


            for (int i = 0; i <= (31 - (32 - _cidr)); i++)
            {
                _BroadcastAddress[i] = _bin[i];
            }

            //Find BC
            for (int i = 31; i > (31 - (32 - _cidr)); i--)
            {
                _BroadcastAddress[i] = 1;
            }
            int[] _BroadcastAddressDecimal = Deci(_BroadcastAddress);

            BroadcastAddress = $"{_BroadcastAddressDecimal[0]}.{_BroadcastAddressDecimal[1]}.{_BroadcastAddressDecimal[2]}.{_BroadcastAddressDecimal[3]}";

            UsableHostRange = FindUsableHostRange();
            GetIPClass(ipoct1);
            UpdateDisplay();
        }

        //Previous button
        private void Previous_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Success != false)
            {
                FindPreviousNetwork();
                FindNextNetwork();
                FindPreviousNetwork();
                FindPreviousNetwork();
                FindNextNetwork();
                FindNextNetwork();
            }
            else
            {
                MessageBox.Show("You need to enter something!", "ERROR");
            }
        }

        //Next button
        private void Next_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Success != false)
            {
                FindNextNetwork();
            }
            else
            {
                MessageBox.Show("You need to enter something!", "ERROR");
            }
        }

        //Lightmode button
        private void LightMode_Button_Click(object sender, RoutedEventArgs e)
        {
            if (LightModeButton.Content.ToString()!.ToLower().Equals("dark mode"))
            {
                EnableDarkmode();
            }
            else
            {
                EnableLightmode();
            }
        }

        //Enable darkmode
        private void EnableDarkmode()
        {
            Grid.Background = Brushes.DarkGray;
            LightModeBorder.BorderBrush = Brushes.LightGray;
            LightModeButton.Background = Brushes.LightGray;
            LightModeButton.Foreground = Brushes.DarkSlateGray;
            ipField.Background = Brushes.LightGray;
            LightModeButton.Content = "Light Mode";
        }

        //Enable lightmode
        private void EnableLightmode()
        {
            Grid.Background = Brushes.GhostWhite;
            LightModeBorder.BorderBrush = Brushes.DarkSlateGray;
            LightModeButton.Background = Brushes.DarkSlateGray;
            LightModeButton.Foreground = Brushes.FloralWhite;
            ipField.Background = Brushes.GhostWhite;
            LightModeButton.Content = "Dark Mode";
        }

        //ipField GotFocus event handler
        private void IpField_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!(IPAddressStringGotFocus == true))
            {
                ipField.Text = "";
                ipField.Opacity = 1.0;
                IPAddressStringGotFocus = true;
            }
        }

        //ipField KeyDown event handler
        private void IpField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ResetAllInfo();

                //Check if the user has ever pressed inside the field
                if (IPAddressStringGotFocus == true)
                {
                    //Set IPAddressString to what the user has entered
                    IPAddressString = ipField.Text;
                    if (IsItAnIP(IPAddressString) == true)
                    {
                        if (FindIPInformation() == true)
                        {
                            if (CalculateNetmask() == true)
                            {
                                if (FindNetworkInformation() == true)
                                {
                                    UpdateDisplay();
                                    Success = true;
                                }
                                else
                                {
                                    //ResetAll = true;
                                }
                            }
                            else
                            {
                                //ResetAll = true;
                            }
                        }
                        else
                        {
                            //ResetAll = true;
                        }
                    }
                    else
                    {
                        //ResetAll = true;
                    }
                }
                else
                {
                    //ResetAll = true;
                    MessageBox.Show("You need to enter something!", "ERROR");
                }
                if (ResetAll == true)
                {
                    ResetAllInfo();
                    UpdateDisplay();
                    ipField.Text = "Example: \"192.168.0.16/24\" Hover over to see more";
                    ipField.Opacity = 0.5;
                    IPAddressStringGotFocus = false;
                }
            }
        }

        //Find the local Subnetmask
        public static IPAddress GetSubnetMask(IPAddress address)
        {
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (address.Equals(unicastIPAddressInformation.Address))
                        {
                            return unicastIPAddressInformation.IPv4Mask;
                        }
                    }
                }
            }
            throw new ArgumentException(string.Format("Can't find subnetmask for IP address '{0}'", address));
        }

        private bool IsItAnIP(string _IPAddressString)
        {
            //Try to seperate IP from netmask
            //Check if IP is empty
            if (_IPAddressString.Equals(""))
            {
                MessageBox.Show("Your IP field is empty!", "ERROR");
                return false;
            }

            //Try to find the CIDR Notation
            if (_IPAddressString.Contains('/'))
            {
                try
                {
                    int temp = Convert.ToInt32(_IPAddressString.Substring(_IPAddressString.IndexOf('/')));
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return false;
                }
                //Get the index of '/'
                try
                {
                    int _IndexOfNetmask;
                    _IndexOfNetmask = _IPAddressString.IndexOf('/');
                    CIDR_Notation = _IPAddressString.Substring(_IndexOfNetmask);

                    //If there is nothing after the '/'
                    if (CIDR_Notation.Equals("/"))
                    {
                        MessageBox.Show("You must be missing something", "ERROR");
                        return false;
                    }

                    //Remove the '/' and everything after it from the IP
                    CIDR_Notation = CIDR_Notation.Trim('/');
                    IPAddressString = _IPAddressString.Remove(_IndexOfNetmask);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"An error occured: \"{e.Message}\"\n\nTry again or contact an administrator", "ERROR");
                    return false;
                }
                //Try to find a netmask or wildcard
            }
            else if (_IPAddressString.Contains(' '))
            {
                if (_IPAddressString.Contains('#'))
                {

                    try
                    {
                        int _indexOf = _IPAddressString.IndexOf('#');
                        NumberOfHostsToMake = _IPAddressString.Substring(_indexOf);

                        //If there is nothing after the '#' sign
                        if (NumberOfHostsToMake.Equals("#"))
                        {
                            MessageBox.Show("You must be missing something", "ERROR");
                            return false;
                        }

                        //Trim the '#' at the beginning to hopefulle get only a number (2 - 1073741824)
                        NumberOfHostsToMake = NumberOfHostsToMake.Trim('#');
                        IPAddressString = _IPAddressString.Remove(_indexOf);

                        try
                        {
                            long _TEMP = Convert.ToInt64(NumberOfHostsToMake);

                            if (_TEMP > 1073741824 || _TEMP < 2)
                            {
                                MessageBox.Show($"\"{_TEMP}\", is not a valid number. It needs to be within 2 and 4,294,967,294", "ERROR");
                                return false;
                            }

                        }
                        catch (Exception e)
                        {
                            MessageBox.Show($"An error occured: \"{e.Message}\"\n\nTry again or contact an administrator", "ERROR");
                            return false;
                        }

                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"An error occured: \"{e.Message}\"\n\nTry again or contact an administrator", "ERROR");
                        return false;
                    }

                    return true;
                }

                try
                {
                    int _IndexOfNetmask = _IPAddressString.IndexOf(' ');
                    Netmask = _IPAddressString.Substring(_IndexOfNetmask);

                    Netmask = Netmask.Trim();

                    if (Netmask.Equals(""))
                    {
                        MessageBox.Show("You must be missing something", "ERROR");
                        return false;
                    }
                    else if (Netmask.StartsWith("0") || (Netmask.StartsWith("1")) || (Netmask.StartsWith("3")) || (Netmask.StartsWith("7")) || (Netmask.StartsWith("15")) || (Netmask.StartsWith("31")) || (Netmask.StartsWith("63")) || (Netmask.StartsWith("127")))
                    {
                        Netmask_Wildcard = Netmask;
                        Netmask = "";
                    }

                    IPAddressString = _IPAddressString.Remove(_IndexOfNetmask);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"An error occured: \"{e.Message}\"\n\nTry again or contact an administrator", "ERROR");
                    return false;
                }
                //Try to find a subnet definition ('#xx')
            }
            else if (IPAddressString.Contains('#'))
            {

                try
                {
                    int _indexOf = _IPAddressString.IndexOf('#');
                    NumberOfHostsToMake = _IPAddressString.Substring(_indexOf);

                    //If there is nothing after the '#' sign
                    if (NumberOfHostsToMake.Equals("#"))
                    {
                        MessageBox.Show("You must be missing something", "ERROR");
                        return false;
                    }

                    //Trim the '#' at the beginning to hopefulle get only a number (2 - 1073741824)
                    NumberOfHostsToMake = NumberOfHostsToMake.Trim('#');
                    IPAddressString = _IPAddressString.Remove(_indexOf);

                    try
                    {
                        long _TEMP = Convert.ToInt64(NumberOfHostsToMake);

                        if (_TEMP > 1073741824 || _TEMP < 2)
                        {
                            MessageBox.Show($"\"{_TEMP}\", is not a valid number. It needs to be within 2 and 4,294,967,294", "ERROR");
                            return false;
                        }

                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"An error occured: \"{e.Message}\"\n\nTry again or contact an administrator", "ERROR");
                        return false;
                    }

                }
                catch (Exception e)
                {
                    MessageBox.Show($"An error occured: \"{e.Message}\"\n\nTry again or contact an administrator", "ERROR");
                    return false;
                }

            }
            else
            {
                //If theres no ' ', '/' or '#' there is probably no netmask subnet definition
                MessageBox.Show("You must be missing something", "ERROR");
                return false;
            }


            //Return true if nothing is wrong
            return true;
        }

        //Find information about the given IP
        private bool FindIPInformation()
        {
            if (ValidateIP() != true)
            {
                return false;
            }

            return true;
        }

        //ValidateIP
        private bool ValidateIP()
        {
            //Get the netmask
            string _IPAddressString = IPAddressString;
            int oct1, oct2, oct3, oct4;

            //Check each octet
            //Octet 1
            int _tempIndex = _IPAddressString.IndexOf('.');
            try
            {
                oct1 = Convert.ToInt32(_IPAddressString.Substring(0, _tempIndex));
            }
            catch (Exception e)
            {
                MessageBox.Show($"An error occured: \"{e.Message}\"\n\nTry again or contact an administrator", "ERROR");
                return false;
            }

            //Octet 2
            _IPAddressString = _IPAddressString.Substring(_tempIndex + 1);
            _tempIndex = _IPAddressString.IndexOf('.');
            try
            {
                oct2 = Convert.ToInt32(_IPAddressString.Substring(0, _tempIndex));
            }
            catch (Exception e)
            {
                MessageBox.Show($"An error occured: \"{e.Message}\"\n\nTry again or contact an administrator", "ERROR");
                return false;
            }

            //Octet 3
            _IPAddressString = _IPAddressString.Substring(_tempIndex + 1);
            _tempIndex = _IPAddressString.IndexOf('.');
            try
            {
                oct3 = Convert.ToInt32(_IPAddressString.Substring(0, _tempIndex));
            }
            catch (Exception e)
            {
                MessageBox.Show($"An error occured: \"{e.Message}\"\n\nTry again or contact an administrator", "ERROR");
                return false;
            }

            //Octet 4
            _IPAddressString = _IPAddressString.Substring(_tempIndex + 1);
            try
            {
                oct4 = Convert.ToInt32(_IPAddressString.Substring(0, _IPAddressString.Length));
            }
            catch (Exception e)
            {
                MessageBox.Show($"An error occured: \"{e.Message}\"\n\nTry again or contact an administrator", "ERROR");
                return false;
            }

            if (oct1 >= 0 && oct1 <= 255)
            {
                if (oct2 >= 0 && oct2 <= 255)
                {
                    if (oct3 >= 0 && oct3 <= 255)
                    {
                        if (oct4 >= 0 && oct4 <= 255)
                        {
                            ipoct1 = oct1;
                            ipoct2 = oct2;
                            ipoct3 = oct3;
                            ipoct4 = oct4;
                        }
                        else
                        {
                            MessageBox.Show($"Your fourth octet is out of bounds", "ERROR");
                            return false;
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Your third octet is out of bounds", "ERROR");
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show($"Your second octet is out of bounds", "ERROR");
                    return false;
                }
            }
            else
            {
                MessageBox.Show($"Your first octet is out of bounds", "ERROR");
                return false;
            }

            if (!(GetIPClass(oct1) == true))
            {
                return false;
            }

            return true;
        }

        //Get the class of the ipv4
        private bool GetIPClass(int _oct1)
        {
            if (_oct1 >= 0 && _oct1 <= 127)
            {
                IPClass = "A (0.0.0.0 - 127.255.255.255)";
            }
            else if (_oct1 >= 128 && _oct1 <= 191)
            {
                IPClass = "B (128.0.0.0 - 191.255.255.255)";
            }
            else if (_oct1 >= 192 && _oct1 <= 223)
            {
                IPClass = "C (192.0.0.0 - 223.255.255.255)";
            }
            else if (_oct1 >= 224 && _oct1 <= 239)
            {
                IPClass = "D (224.0.0.0 - 239.255.255.255)";
            }
            else if (_oct1 >= 240 && _oct1 <= 255)
            {
                IPClass = "E (240.0.0.0 - 255.255.255.255)";
            }
            else
            {
                MessageBox.Show("Class is not recognized", "ERROR");
                return false;
            }
            return true;
        }

        //Reset all prior known information
        private void ResetAllInfo()
        {
            IPAddressString = "";
            CIDR_Notation = "";
            Netmask = "";
            Netmask_Wildcard = "";
            Netmask_Binary = "";
            NetworkAddressString = "";
            BroadcastAddress = "";
            UsableHostRange = "";
            TotalNumberOfHosts = "";
            NumberOfUsableHosts = "";
            NumberOfHostsToMake = "";
            IPClass = "";
            PublicORPrivate = "";
            Success = false;
        }

        //Remove trailing character
        private void RemoveTrailing(string character)
        {
            if (Netmask.EndsWith(character))
            {
                Netmask = Netmask.Substring(0, Netmask.Length - 1);
            }
            if (Netmask_Wildcard.EndsWith(character))
            {
                Netmask_Wildcard = Netmask_Wildcard.Substring(0, Netmask_Wildcard.Length - 1);
            }
        }

        private bool Validate(string _name)
        {
            int _oct1, _oct2, _oct3, _oct4;
            switch (_name.ToLower())
            {
                case "netmask":
                    string _NetmaskCopy = Netmask;

                    //Netmask validation
                    //First check if its all numbers
                    try
                    {
                        long _temp = Convert.ToInt64(Netmask.Replace(".", ""));
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"An error occured: \"{e.Message}\"\n\nTry again or contact an administrator", "ERROR");
                        return false;
                    }
                    //Next check if its within range (0-255)

                    //Octet 1
                    try
                    {
                        _oct1 = Convert.ToInt32(Netmask.Substring(0, Netmask.IndexOf(".")));
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"An error occured: \"{e.Message}\"\n\nTry again or contact an administrator", "ERROR");
                        return false;
                    }

                    //Check if numbers are correct
                    if (!(_oct1 == 0 || _oct1 == 128 || _oct1 == 192 || _oct1 == 224 || _oct1 == 240 || _oct1 == 248 || _oct1 == 252 || _oct1 == 254 || _oct1 == 255))
                    {
                        MessageBox.Show("Incorrect mask(1), possible values are: 0, 128, 192, 224, 240, 248, 252, 254 & 255", "ERROR");
                        return false;
                    }

                    if (!(_oct1 >= 0 && _oct1 <= 255))
                    {
                        MessageBox.Show($"Your subnet is out of range, the valid range is 0-255 (First octet)", "ERROR");
                        return false;
                    }

                    Netmask_Wildcard += $"{255 - _oct1}.";

                    //Octet 2
                    Netmask = Netmask.Substring(Netmask.IndexOf(".") + 1);
                    try
                    {
                        _oct2 = Convert.ToInt32(Netmask.Substring(0, Netmask.IndexOf(".")));
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"An error occured: \"{e.Message}\"\n\nTry again or contact an administrator", "ERROR");
                        return false;
                    }

                    //Check if numbers are correct
                    if (!(_oct2 == 0 || _oct2 == 128 || _oct2 == 192 || _oct2 == 224 || _oct2 == 240 || _oct2 == 248 || _oct2 == 252 || _oct2 == 254 || _oct2 == 255))
                    {
                        MessageBox.Show("Incorrect mask(2), possible values are: 0, 128, 192, 224, 240, 248, 252, 254 & 255", "ERROR");
                        return false;
                    }

                    if (!(_oct2 >= 0 && _oct2 <= 255))
                    {
                        MessageBox.Show($"Your subnet is out of range, the valid range is 0-255 (Second octet)", "ERROR");
                        return false;
                    }
                    Netmask_Wildcard += $"{255 - _oct2}.";

                    //Octet 3
                    Netmask = Netmask.Substring(Netmask.IndexOf(".") + 1);
                    try
                    {
                        _oct3 = Convert.ToInt32(Netmask.Substring(0, Netmask.IndexOf(".")));
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"An error occured: \"{e.Message}\"\n\nTry again or contact an administrator", "ERROR");
                        return false;
                    }

                    //Check if numbers are correct
                    if (!(_oct3 == 0 || _oct3 == 128 || _oct3 == 192 || _oct3 == 224 || _oct3 == 240 || _oct3 == 248 || _oct3 == 252 || _oct3 == 254 || _oct3 == 255))
                    {
                        MessageBox.Show("Incorrect mask(3), possible values are: 0, 128, 192, 224, 240, 248, 252, 254 & 255", "ERROR");
                        return false;
                    }

                    if (!(_oct3 >= 0 && _oct3 <= 255))
                    {
                        MessageBox.Show($"Your subnet is out of range, the valid range is 0-255 (Third octet)", "ERROR");
                        return false;
                    }
                    Netmask_Wildcard += $"{255 - _oct3}.";

                    //Octet 4
                    Netmask = Netmask.Substring(Netmask.IndexOf(".") + 1);
                    try
                    {
                        _oct4 = Convert.ToInt32(Netmask.Substring(0));
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"An error occured: \"{e.Message}\"\n\nTry again or contact an administrator", "ERROR");
                        return false;
                    }

                    //Check if numbers are correct
                    if (!(_oct4 == 0 || _oct4 == 128 || _oct4 == 192 || _oct4 == 224 || _oct4 == 240 || _oct4 == 248 || _oct4 == 252 || _oct4 == 254 || _oct4 == 255))
                    {
                        MessageBox.Show("Incorrect mask(4), possible values are: 0, 128, 192, 224, 240, 248, 252, 254 & 255", "ERROR");
                        return false;
                    }

                    if (!(_oct4 >= 0 && _oct4 <= 255))
                    {
                        MessageBox.Show($"Your subnet is out of range, the valid range is 0-255 (Fourth octet)", "ERROR");
                        return false;
                    }
                    Netmask_Wildcard += $"{255 - _oct4}";

                    //Check if they increment correctly
                    if (_oct1 < _oct2 || _oct1 < _oct3 || _oct1 < _oct4 || _oct2 < _oct3 || _oct2 < _oct4 || _oct3 < _oct4)
                    {
                        MessageBox.Show("Your netmask is not in order (Please check it)", "ERROR");
                        return false;
                    }
                    else if (_oct1 < 255 && _oct2 > 0 || _oct2 < 255 && _oct3 > 0 || _oct3 < 255 && _oct4 > 0)
                    {
                        MessageBox.Show("Your netmask is not in order (Please check it)", "ERROR");
                        return false;
                    }

                    //Retrieve the copy
                    Netmask = _NetmaskCopy;

                    //Remove trailing dots at the end of both wildcard and netmask if any
                    RemoveTrailing(".");

                    return true;
                case "netmask_wildcard":
                    string _Netmask_Wildcard_Copy = Netmask_Wildcard;

                    //Wildcard validation
                    //First check if its all numbers
                    try
                    {
                        long _temp = Convert.ToInt64(Netmask_Wildcard.Replace(".", ""));
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"An error occured: \"{e.Message}\"\n\nTry again or contact an administrator", "ERROR");
                        return false;
                    }
                    //Next check if its within range (0-255)

                    //Octet 1
                    try
                    {
                        _oct1 = Convert.ToInt32(Netmask_Wildcard.Substring(0, Netmask_Wildcard.IndexOf(".")));
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"An error occured: \"{e.Message}\"\n\nTry again or contact an administrator", "ERROR");
                        return false;
                    }

                    //Check if numbers are correct
                    if (!(_oct1 == 0 || _oct1 == 1 || _oct1 == 3 || _oct1 == 7 || _oct1 == 15 || _oct1 == 31 || _oct1 == 63 || _oct1 == 127 || _oct1 == 255))
                    {
                        MessageBox.Show("Incorrect mask(1), possible values are: 0, 1, 3, 7, 15, 31, 63, 127 & 255", "ERROR");
                        return false;
                    }

                    if (!(_oct1 >= 0 && _oct1 <= 255))
                    {
                        MessageBox.Show($"Your subnet is out of range, the valid range is 0-255 (First octet)", "ERROR");
                        return false;
                    }
                    Netmask += $"{255 - _oct1}.";

                    //Octet 2
                    Netmask_Wildcard = Netmask_Wildcard.Substring(Netmask_Wildcard.IndexOf(".") + 1);
                    try
                    {
                        _oct2 = Convert.ToInt32(Netmask_Wildcard.Substring(0, Netmask_Wildcard.IndexOf(".")));
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"An error occured: \"{e.Message}\"\n\nTry again or contact an administrator", "ERROR");
                        return false;
                    }

                    //Check if numbers are correct
                    if (!(_oct2 == 0 || _oct2 == 1 || _oct2 == 3 || _oct2 == 7 || _oct2 == 15 || _oct2 == 31 || _oct2 == 63 || _oct2 == 127 || _oct2 == 255))
                    {
                        MessageBox.Show("Incorrect mask(2), possible values are: 0, 1, 3, 7, 15, 31, 63, 127 & 255", "ERROR");
                        return false;
                    }

                    if (!(_oct2 >= 0 && _oct2 <= 255))
                    {
                        MessageBox.Show($"Your subnet is out of range, the valid range is 0-255 (Second octet)", "ERROR");
                        return false;
                    }
                    Netmask += $"{255 - _oct2}.";

                    //Octet 3
                    Netmask_Wildcard = Netmask_Wildcard.Substring(Netmask_Wildcard.IndexOf(".") + 1);
                    try
                    {
                        _oct3 = Convert.ToInt32(Netmask_Wildcard.Substring(0, Netmask_Wildcard.IndexOf(".")));
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"An error occured: \"{e.Message}\"\n\nTry again or contact an administrator", "ERROR");
                        return false;
                    }

                    //Check if numbers are correct
                    if (!(_oct3 == 0 || _oct3 == 1 || _oct3 == 3 || _oct3 == 7 || _oct3 == 15 || _oct3 == 31 || _oct3 == 63 || _oct3 == 127 || _oct3 == 255))
                    {
                        MessageBox.Show("Incorrect mask(3), possible values are: 0, 1, 3, 7, 15, 31, 63, 127 & 255", "ERROR");
                        return false;
                    }

                    if (!(_oct3 >= 0 && _oct3 <= 255))
                    {
                        MessageBox.Show($"Your subnet is out of range, the valid range is 0-255 (Third octet)", "ERROR");
                        return false;
                    }
                    Netmask += $"{255 - _oct3}.";

                    //Octet 4
                    Netmask_Wildcard = Netmask_Wildcard.Substring(Netmask_Wildcard.IndexOf(".") + 1);
                    try
                    {
                        _oct4 = Convert.ToInt32(Netmask_Wildcard.Substring(0));
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"An error occured: \"{e.Message}\"\n\nTry again or contact an administrator", "ERROR");
                        return false;
                    }

                    //Check if numbers are correct
                    if (!(_oct4 == 0 || _oct4 == 1 || _oct4 == 3 || _oct4 == 7 || _oct4 == 15 || _oct4 == 31 || _oct4 == 63 || _oct4 == 127 || _oct4 == 255))
                    {
                        MessageBox.Show("Incorrect mask(4), possible values are: 0, 1, 3, 7, 15, 31, 63, 127 & 255", "ERROR");
                        return false;
                    }

                    if (!(_oct4 >= 0 && _oct4 <= 255))
                    {
                        MessageBox.Show($"Your subnet is out of range, the valid range is 0-255 (Fourth octet)", "ERROR");
                        return false;
                    }
                    Netmask += $"{255 - _oct4}";

                    //Check if they increment correctly
                    if (_oct1 > _oct2 || _oct1 > _oct3 || _oct1 > _oct4 || _oct2 > _oct3 || _oct2 > _oct4 || _oct3 > _oct4)
                    {
                        MessageBox.Show("Your netmask is not in order (Please check it)", "ERROR");
                        return false;
                    }
                    else if (_oct4 < 255 && _oct3 > 0 || _oct3 < 255 && _oct2 > 0 || _oct2 < 255 && _oct1 > 0)
                    {
                        MessageBox.Show("Your netmask is not in order (Please check it)", "ERROR");
                        return false;
                    }

                    //Retrieve the copy
                    Netmask_Wildcard = _Netmask_Wildcard_Copy;

                    //Remove trailing dots at the end of both wildcard and netmask if any
                    RemoveTrailing(".");

                    return true;
            }

            //Defaults to false
            return false;
        }

        //Get CIDR from netmask
        private string getCIDR()
        {
            int _CIDR_INT = 0;
            int oct1, oct2, oct3, oct4;

            //Get the netmask
            string _Netmask = Netmask;

            //Octet 1
            int _tempIndex = _Netmask.IndexOf('.');
            oct1 = Convert.ToInt32(_Netmask.Substring(0, _tempIndex));

            //Octet 2
            _Netmask = _Netmask.Substring(_tempIndex + 1);
            _tempIndex = _Netmask.IndexOf('.');
            oct2 = Convert.ToInt32(_Netmask.Substring(0, _tempIndex));

            //Octet 3
            _Netmask = _Netmask.Substring(_tempIndex + 1);
            _tempIndex = _Netmask.IndexOf('.');
            oct3 = Convert.ToInt32(_Netmask.Substring(0, _tempIndex));

            //Octet 4
            _Netmask = _Netmask.Substring(_tempIndex + 1);
            oct4 = Convert.ToInt32(_Netmask.Substring(0, _Netmask.Length));

            if (oct4 > 252)
            {
                MessageBox.Show("The smallest network possible is a /30", "ERROR");
                return "ERROR";
            }

            //Octet 1
            switch (oct1)
            {
                case 0:
                    _CIDR_INT += 0;
                    break;

                case 128:
                    _CIDR_INT += 1;
                    break;

                case 192:
                    _CIDR_INT += 2;
                    break;

                case 224:
                    _CIDR_INT += 3;
                    break;

                case 240:
                    _CIDR_INT += 4;
                    break;

                case 248:
                    _CIDR_INT += 5;
                    break;

                case 252:
                    _CIDR_INT += 6;
                    break;

                case 254:
                    _CIDR_INT += 7;
                    break;

                case 255:
                    _CIDR_INT += 8;
                    break;
            }

            //Octet 2
            switch (oct2)
            {
                case 0:
                    _CIDR_INT += 0;
                    break;

                case 128:
                    _CIDR_INT += 1;
                    break;

                case 192:
                    _CIDR_INT += 2;
                    break;

                case 224:
                    _CIDR_INT += 3;
                    break;

                case 240:
                    _CIDR_INT += 4;
                    break;

                case 248:
                    _CIDR_INT += 5;
                    break;

                case 252:
                    _CIDR_INT += 6;
                    break;

                case 254:
                    _CIDR_INT += 7;
                    break;

                case 255:
                    _CIDR_INT += 8;
                    break;
            }

            //Octet 3
            switch (oct3)
            {
                case 0:
                    _CIDR_INT += 0;
                    break;

                case 128:
                    _CIDR_INT += 1;
                    break;

                case 192:
                    _CIDR_INT += 2;
                    break;

                case 224:
                    _CIDR_INT += 3;
                    break;

                case 240:
                    _CIDR_INT += 4;
                    break;

                case 248:
                    _CIDR_INT += 5;
                    break;

                case 252:
                    _CIDR_INT += 6;
                    break;

                case 254:
                    _CIDR_INT += 7;
                    break;

                case 255:
                    _CIDR_INT += 8;
                    break;
            }

            //Octet 4
            switch (oct4)
            {
                case 0:
                    _CIDR_INT += 0;
                    break;

                case 128:
                    _CIDR_INT += 1;
                    break;

                case 192:
                    _CIDR_INT += 2;
                    break;

                case 224:
                    _CIDR_INT += 3;
                    break;

                case 240:
                    _CIDR_INT += 4;
                    break;

                case 248:
                    _CIDR_INT += 5;
                    break;

                case 252:
                    _CIDR_INT += 6;
                    break;

                case 254:
                    _CIDR_INT += 7;
                    break;

                case 255:
                    _CIDR_INT += 8;
                    break;
            }

            return $"{_CIDR_INT}";
        }

        //Calculate the network mask
        private bool CalculateNetmask()
        {
            //Figure out what was entered along with the IP.
            if (!(CIDR_Notation.Equals("")))
            {

                //Trim CIDR and convert to an integer
                int _CIDR_Notation = Convert.ToInt32(CIDR_Notation.Trim('/'));

                //Check if the newly created int CIDR is within range
                if (_CIDR_Notation >= 0 && _CIDR_Notation <= 30)
                {

                    //Iterate through 4 times
                    for (int i = 0; i < 4; i++)
                    {

                        //Do some basic math
                        int _rest = 0;
                        while (_CIDR_Notation > 8)
                        {
                            _rest++;
                            _CIDR_Notation--;
                        }

                        //Once basic math has been done, capture the current value and output the netmask and wildcard
                        switch (_CIDR_Notation)
                        {
                            case 8:
                                Netmask += "255.";
                                Netmask_Wildcard += "0.";
                                _CIDR_Notation -= 8;
                                break;
                            case 7:
                                Netmask += "254.";
                                Netmask_Wildcard += "1.";
                                _CIDR_Notation -= 7;
                                break;
                            case 6:
                                Netmask += "252.";
                                Netmask_Wildcard += "3.";
                                _CIDR_Notation -= 6;
                                break;
                            case 5:
                                Netmask += "248.";
                                Netmask_Wildcard += "7.";
                                _CIDR_Notation -= 5;
                                break;
                            case 4:
                                Netmask += "240.";
                                Netmask_Wildcard += "15.";
                                _CIDR_Notation -= 4;
                                break;
                            case 3:
                                Netmask += "224.";
                                Netmask_Wildcard += "31.";
                                _CIDR_Notation -= 3;
                                break;
                            case 2:
                                Netmask += "192.";
                                Netmask_Wildcard += "63.";
                                _CIDR_Notation -= 2;
                                break;
                            case 1:
                                Netmask += "128.";
                                Netmask_Wildcard += "127.";
                                _CIDR_Notation -= 1;
                                break;
                            case 0:
                                Netmask += "0.";
                                Netmask_Wildcard += "255.";
                                break;
                        }

                        //Add the rest to cidr and reset the rest, ready for another round
                        _CIDR_Notation += _rest;
                    }

                    //Remove trailing dots at the end of both wildcard and netmask
                    RemoveTrailing(".");

                }
                else
                {
                    MessageBox.Show("The smallest network possible is a /30", "ERROR");
                    return false;
                }

            }
            else if (!(Netmask.Equals("")))
            {

                //Validate the netmask
                if (Validate("Netmask") == true)
                {

                    //Get the CIDR from the netmask
                    if (getCIDR().Equals("ERROR"))
                    {
                        return false;
                    }
                    CIDR_Notation = getCIDR();
                }
                else
                {
                    return false;
                }
            }
            else if (!(Netmask_Wildcard.Equals("")))
            {

                //Validate the wildcard mask
                if (Validate("Netmask_Wildcard") == true)
                {

                    //Get the CIDR from the Wildcard mask
                    CIDR_Notation = getCIDR();
                }
                else
                {
                    return false;
                }
            }
            else if (!(NumberOfHostsToMake.Equals("")))
            {
                long _NumberOfHostsToMake = Convert.ToInt64(NumberOfHostsToMake.Trim('#'));

                //Get the required CIDR to create the subnet
                for (int i = 0; i < 30; i++)
                {
                    long _temp = (long)Math.Pow(2, i);
                    if (_temp >= _NumberOfHostsToMake)
                    {
                        CIDR_Notation = Convert.ToString(32 - i);
                        break;
                    }
                }

                if (!(CIDR_Notation.Equals("")))
                {

                    //Trim CIDR and convert to an integer
                    int _CIDR_Notation = Convert.ToInt32(CIDR_Notation.Trim('/'));

                    //Check if the newly created int CIDR is within range
                    if (_CIDR_Notation >= 0 && _CIDR_Notation <= 30)
                    {

                        //Iterate through 4 times
                        for (int i = 0; i < 4; i++)
                        {

                            //Do some basic math
                            int _rest = 0;
                            while (_CIDR_Notation > 8)
                            {
                                _rest++;
                                _CIDR_Notation--;
                            }

                            //Once basic math has been done, capture the current value and output the netmask and wildcard
                            switch (_CIDR_Notation)
                            {
                                case 8:
                                    Netmask += "255.";
                                    Netmask_Wildcard += "0.";
                                    _CIDR_Notation -= 8;
                                    break;
                                case 7:
                                    Netmask += "254.";
                                    Netmask_Wildcard += "1.";
                                    _CIDR_Notation -= 7;
                                    break;
                                case 6:
                                    Netmask += "252.";
                                    Netmask_Wildcard += "3.";
                                    _CIDR_Notation -= 6;
                                    break;
                                case 5:
                                    Netmask += "248.";
                                    Netmask_Wildcard += "7.";
                                    _CIDR_Notation -= 5;
                                    break;
                                case 4:
                                    Netmask += "240.";
                                    Netmask_Wildcard += "15.";
                                    _CIDR_Notation -= 4;
                                    break;
                                case 3:
                                    Netmask += "224.";
                                    Netmask_Wildcard += "31.";
                                    _CIDR_Notation -= 3;
                                    break;
                                case 2:
                                    Netmask += "192.";
                                    Netmask_Wildcard += "63.";
                                    _CIDR_Notation -= 2;
                                    break;
                                case 1:
                                    Netmask += "128.";
                                    Netmask_Wildcard += "127.";
                                    _CIDR_Notation -= 1;
                                    break;
                                case 0:
                                    Netmask += "0.";
                                    Netmask_Wildcard += "255.";
                                    break;
                            }

                            //Add the rest to cidr and reset the rest, ready for another round
                            _CIDR_Notation += _rest;
                        }

                        //Remove trailing dots at the end of both wildcard and netmask
                        RemoveTrailing(".");

                    }
                    else
                    {
                        MessageBox.Show("The smallest network possible is a /30", "ERROR");
                        return false;
                    }

                }
            }

            //Calculate the binary netmask
            if (CalculteBinary() != true)
            {
                return false;
            }

            return true;
        }

        //Calculate binary netmask
        private bool CalculteBinary()
        {
            int oct1, oct2, oct3, oct4;

            //Get the netmask
            string _Netmask = Netmask;

            //Octet 1
            int _tempIndex = _Netmask.IndexOf('.');
            oct1 = Convert.ToInt32(_Netmask.Substring(0, _tempIndex));

            //Octet 2
            _Netmask = _Netmask.Substring(_tempIndex + 1);
            _tempIndex = _Netmask.IndexOf('.');
            oct2 = Convert.ToInt32(_Netmask.Substring(0, _tempIndex));

            //Octet 3
            _Netmask = _Netmask.Substring(_tempIndex + 1);
            _tempIndex = _Netmask.IndexOf('.');
            oct3 = Convert.ToInt32(_Netmask.Substring(0, _tempIndex));

            //Octet 4
            _Netmask = _Netmask.Substring(_tempIndex + 1);
            oct4 = Convert.ToInt32(_Netmask.Substring(0, _Netmask.Length));

            //Convert to binary and add leading zeros, to make it look pretty
            string _bin1 = Convert.ToString(oct1, 2).PadLeft(8, '0');
            string _bin2 = Convert.ToString(oct2, 2).PadLeft(8, '0');
            string _bin3 = Convert.ToString(oct3, 2).PadLeft(8, '0');
            string _bin4 = Convert.ToString(oct4, 2).PadLeft(8, '0');
            Netmask_Binary = $"{_bin1}.{_bin2}.{_bin3}.{_bin4}";

            return true;
        }

        //Find Network Information
        private bool FindNetworkInformation()
        {
            //Get the ip and network mask
            int _ipoct1 = ipoct1;
            int _ipoct2 = ipoct2;
            int _ipoct3 = ipoct3;
            int _ipoct4 = ipoct4;

            //Convert to binary and add leading zeros, to make it look pretty
            string _ipbin1 = Convert.ToString(ipoct1, 2).PadLeft(8, '0');
            string _ipbin2 = Convert.ToString(ipoct2, 2).PadLeft(8, '0');
            string _ipbin3 = Convert.ToString(ipoct3, 2).PadLeft(8, '0');
            string _ipbin4 = Convert.ToString(ipoct4, 2).PadLeft(8, '0');
            string _ipbin = $"{_ipbin1}{_ipbin2}{_ipbin3}{_ipbin4}";

            //Get cidr
            int _cidr = 0;
            try
            {
                _cidr = Convert.ToInt32(CIDR_Notation.Trim('/'));
            }
            catch (Exception)
            {
                return false;
            }


            int[] _bin = new int[32];
            int[] _NetworkAddress = new int[32];
            int[] _BroadcastAddress = new int[32];

            string[] str = new string[4];
            str = IPAddressString.Split('.');

            //Convert IP to binary
            _bin = Bina(str);

            //Find NA

            for (int i = 0; i <= (31 - (32 - _cidr)); i++)
            {
                _NetworkAddress[i] = _bin[i];
                _BroadcastAddress[i] = _bin[i];
            }

            for (int i = 31; i > (31 - (32 - _cidr)); i--)
            {
                _NetworkAddress[i] = 0;
            }

            //Find BC
            for (int i = 31; i > (31 - (32 - _cidr)); i--)
            {
                _BroadcastAddress[i] = 1;
            }

            //Convert to Decimal
            int[] _NetworkAddressDecimal = Deci(_NetworkAddress);
            int[] _BroadcastAddressDecimal = Deci(_BroadcastAddress);

            //Save
            NetworkAddressString = $"{_NetworkAddressDecimal[0]}.{_NetworkAddressDecimal[1]}.{_NetworkAddressDecimal[2]}.{_NetworkAddressDecimal[3]}";
            BroadcastAddress = $"{_BroadcastAddressDecimal[0]}.{_BroadcastAddressDecimal[1]}.{_BroadcastAddressDecimal[2]}.{_BroadcastAddressDecimal[3]}";

            //Find private or public?
            PublicORPrivate = FindPublicORPrivate();

            //Find Usable host range
            UsableHostRange = FindUsableHostRange();

            //Find Total Number of Hosts
            TotalNumberOfHosts = FindTotalHosts(32 - _cidr);

            //Find number of usable hosts
            NumberOfUsableHosts = $"{Convert.ToInt64(TotalNumberOfHosts) - 2}";

            return true;
        }

        //Find total number of hosts
        private string FindTotalHosts(int hostbits)
        {

            return $"{Math.Pow(2, hostbits)}";
        }

        //Find usable host range
        private string FindUsableHostRange()
        {
            string[] _NetworkAddress = NetworkAddressString.Split('.');
            string[] _BroadcastAddress = BroadcastAddress.Split('.');

            int oct4 = Convert.ToInt32(_NetworkAddress[3]);
            _NetworkAddress[3] = $"{oct4 + 1}";

            oct4 = Convert.ToInt32(_BroadcastAddress[3]);
            _BroadcastAddress[3] = $"{oct4 - 1}";


            return $"{_NetworkAddress[0]}.{_NetworkAddress[1]}.{_NetworkAddress[2]}.{_NetworkAddress[3]} - {_BroadcastAddress[0]}.{_BroadcastAddress[1]}.{_BroadcastAddress[2]}.{_BroadcastAddress[3]}";
        }

        //Find Public or private
        private string FindPublicORPrivate()
        {
            if ((ipoct1 == 10) || (ipoct1 == 172 && ipoct2 >= 16 && ipoct2 < 32) || (ipoct1 == 192 && ipoct2 == 168))
            {
                return "Private";
            }
            return "Public";
        }

        //Convert decimal to Binary
        private static int[] Bina(string[] str)
        {
            int[] re = new int[32];
            int a, b, c, d, i, rem;
            a = b = c = d = 1;
            Stack<int> st = new Stack<int>();

            // Separate each number of the IP address 
            if (str != null)
            {
                a = int.Parse(str[0]);
                b = int.Parse(str[1]);
                c = int.Parse(str[2]);
                d = int.Parse(str[3]);
            }

            // convert first number to Binary 
            for (i = 0; i <= 7; i++)
            {
                rem = a % 2;
                st.Push(rem);
                a = a / 2;
            }

            // Obtain First octet 
            for (i = 0; i <= 7; i++)
            {
                re[i] = st.Pop();
            }

            // convert second number to Binary 
            for (i = 8; i <= 15; i++)
            {
                rem = b % 2;
                st.Push(rem);
                b = b / 2;
            }

            // Obtain Second octet 
            for (i = 8; i <= 15; i++)
            {
                re[i] = st.Pop();
            }

            // convert Third number to Binary 
            for (i = 16; i <= 23; i++)
            {
                rem = c % 2;
                st.Push(rem);
                c = c / 2;
            }

            // Obtain Third octet 
            for (i = 16; i <= 23; i++)
            {
                re[i] = st.Pop();
            }

            // convert fourth number to Binary 
            for (i = 24; i <= 31; i++)
            {
                rem = d % 2;
                st.Push(rem);
                d = d / 2;
            }

            // Obtain Fourth octet 
            for (i = 24; i <= 31; i++)
            {
                re[i] = st.Pop();
            }

            return (re);
        }

        //Convert Binary to decimal
        private static int[] Deci(int[] bi)
        {

            int[] arr = new int[4];
            int a, b, c, d, i, j;
            a = b = c = d = 0;
            j = 7;

            for (i = 0; i < 8; i++)
            {

                a = a + (int)(Math.Pow(2, j)) * bi[i];
                j--;
            }

            j = 7;
            for (i = 8; i < 16; i++)
            {

                b = b + bi[i] * (int)(Math.Pow(2, j));
                j--;
            }

            j = 7;
            for (i = 16; i < 24; i++)
            {

                c = c + bi[i] * (int)(Math.Pow(2, j));
                j--;
            }

            j = 7;
            for (i = 24; i < 32; i++)
            {

                d = d + bi[i] * (int)(Math.Pow(2, j));
                j--;
            }

            arr[0] = a;
            arr[1] = b;
            arr[2] = c;
            arr[3] = d;
            return arr;
        }

        //Update the display
        private void UpdateDisplay()
        {
            if (CIDR_Notation.Equals(""))
            {
                TextResults_CIDR.Text = $"CIDR Notation:";
            }
            else
            {
                TextResults_CIDR.Text = $"CIDR Notation: /{CIDR_Notation.Trim()}";
            }

            if (PublicORPrivate.Equals(""))
            {
                TextResults_IPAddress.Text = $"IP Address:";
            }
            else
            {
                TextResults_IPAddress.Text = $"IP Address: {IPAddressString.Trim()} ({PublicORPrivate.Trim()})";
            }

            if (NumberOfHostsToMake.Equals(""))
            {
                TextResults_UsableHosts.Text = $"Number of Usable Hosts: {NumberOfUsableHosts.Trim()}";
            }
            else
            {
                TextResults_UsableHosts.Text = $"Number of Usable Hosts: {NumberOfUsableHosts.Trim()} (Requested: {NumberOfHostsToMake})";
            }

            TextResults_Netmask.Text = $"Netmask: {Netmask.Trim()}";
            TextResults_Netmask_Wildcard.Text = $"Wildcard Mask: {Netmask_Wildcard.Trim()}";
            TextResults_Netmask_Binary.Text = $"Binary Netmask: {Netmask_Binary.Trim()}";
            TextResults_NetworkAddress.Text = $"Network Address (NA): {NetworkAddressString.Trim()}";
            TextResults_BroadcastAddress.Text = $"Broadcast Address (BC): {BroadcastAddress.Trim()}";
            TextResults_UsableHostRange.Text = $"Usable Host Range: {UsableHostRange.Trim()}";
            TextResults_TotalHosts.Text = $"Total Number of Hosts: {TotalNumberOfHosts.Trim()}";
            TextResults_IPClass.Text = $"IP Class: {IPClass.Trim()}";
        }
        //END
    }
}