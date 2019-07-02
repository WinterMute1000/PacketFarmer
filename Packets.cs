using PacketDotNet;
using SharpPcap;
using System;
using System.Windows;

namespace PacketFarmer //Make Packetcapture function event
{
	abstract class PacketInterface
	{
		private ICaptureDevice packetCaptureDevice;
		private bool isDeviceOpend;
		private int captureNum = 0;
		private int nowCaptureNum = 0;
		public delegate void captureSend();
		public static event captureSend dataSend;
		public delegate void captureEnd();
		public static event captureEnd captureEndHandle;
		private String resultData = "";

		public ICaptureDevice PacketCaptureDevice //PacketInterface capturing interfece
		{
			set
			{
				packetCaptureDevice = value;
				isDeviceOpend = false;
			}
			get { return packetCaptureDevice; }
		}

		public String ResultData
		{
			get { return resultData; }
			set { resultData = value; }
		}

		public int CaptureNum
		{
			set
			{
				captureNum = value;
			}
			get
			{
				return captureNum;
			}
		}

		public int NowCaptureNum
		{
			set
			{
				nowCaptureNum = value;
			}
			get
			{
				return nowCaptureNum;
			}
		}

		public PacketInterface(ICaptureDevice captureDevice)
		{
			PacketCaptureDevice = captureDevice;
			isDeviceOpend = false;
		}
		public void OpenDevice(int readTimeoutMilliseconds)
		{
			if (isDeviceOpend != true)
			{
				packetCaptureDevice.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds); //need to add error handling
				isDeviceOpend = false;
			}
		}

		public void DataSendEvent()
		{
			if (dataSend != null)
				dataSend();
		}

		public void CaptureEndEvent()
		{
			if (captureEndHandle != null)
				captureEndHandle();
		}

		public abstract void PacketCapture(int captureNum);
		public abstract void PacketCapture(int captureNum, string filter);
		public abstract void RemoveHanlder();
		public void StopPacketCapture()
		{
			RemoveHanlder();
			packetCaptureDevice.StopCapture();
			ResultData += this.NowCaptureNum + " packet captured \n";
			this.NowCaptureNum = 0;
			SendPacketData();
			CaptureEndEvent();
		}

		public void SendPacketData() //Send packetdata to main window
		{
			DataSendEvent();
			resultData="";
		}
		public void SetPacketFillter(string filter) //make protocl packet fillter
		{
			PacketCaptureDevice.Filter = "and" + filter;
		}
	}
	class TCPPacketInterface : PacketInterface
	{
		public TCPPacketInterface(ICaptureDevice captureDevice) : base(captureDevice)
		{ }

		public override void PacketCapture(int captureNum)
		{
			this.CaptureNum = captureNum;
			PacketCaptureDevice.Filter = "tcp";
			ResultData = "";
			PacketCaptureDevice.OnPacketArrival += TcpPacketCapture;
			PacketCaptureDevice.StartCapture();
		}

		public override void PacketCapture(int captureNum, string filter)
		{
			this.CaptureNum = captureNum;
			PacketCaptureDevice.Filter = "tcp";
			ResultData = "";
			try
			{
				SetPacketFillter(filter);
				PacketCaptureDevice.OnPacketArrival += TcpPacketCapture;
				PacketCaptureDevice.StartCapture();
			}
			catch (PcapException wrongFilter)
			{
				Console.WriteLine(wrongFilter.StackTrace);
				MessageBox.Show("Please reset filter and start.", "Wrong filter!", MessageBoxButton.OK);
			}
		}
		public void TcpPacketCapture(object sender, CaptureEventArgs e) //Packet capture and return to string (async)
		{
			RawCapture capturePacket = e.Packet;


			this.NowCaptureNum++;

			try
			{
				if (this.NowCaptureNum <= this.CaptureNum)
				{
					var packet = PacketDotNet.Packet.ParsePacket(capturePacket.LinkLayerType, capturePacket.Data);
					TcpPacket tcpPacket = (TcpPacket)packet.Extract(typeof(PacketDotNet.TcpPacket));
					ResultData += "Source Port:" + tcpPacket.SourcePort.ToString() + " ";
					ResultData += "DestinationPort:" + tcpPacket.DestinationPort.ToString() + "\n";
					ResultData += "SequenceNumber:" + tcpPacket.SequenceNumber.ToString() + "\n";
					ResultData += "AcknowledgmentNumber:" + tcpPacket.AcknowledgmentNumber.ToString() + "\n";
					ResultData += "DataOffset:" + tcpPacket.DataOffset.ToString() + " ";
					ResultData += "Flag:" + tcpPacket.AllFlags.ToString() + " ";
					ResultData += "Window Size:" + tcpPacket.WindowSize.ToString() + "\n";
					ResultData += "Checksum:" + tcpPacket.Checksum.ToString() + " ";
					ResultData += "Urgent Pointer:" + tcpPacket.UrgentPointer.ToString() + "\n";
					ResultData += "Data\n";
					int i = 1;

					if(tcpPacket.PayloadData!=null)
					{
						foreach (byte data in tcpPacket.PayloadData)
						{
							ResultData += Convert.ToString(data, 16) + " ";

							if (i % 8 == 0)
								ResultData += "\n";
							i++;
						}
					}
					ResultData += "\n--------------------------------------------\n";

					if (this.NowCaptureNum == this.CaptureNum)
						StopPacketCapture();

					SendPacketData();
				}

				else
				{
					StopPacketCapture();
					//PacketCaptureDevice.Close();
					this.NowCaptureNum = 0;
					CaptureEndEvent();
				}
			}
			catch (NullReferenceException nullException)
			{
				Console.WriteLine(nullException.StackTrace);
				MessageBox.Show("Can't packet extracted. \n Are you set others protocol in filter?"
					, "Warining", System.Windows.MessageBoxButton.OK);
				StopPacketCapture();
				//PacketCaptureDevice.Close();
			}
		}

		public override void RemoveHanlder()
		{
			PacketCaptureDevice.OnPacketArrival -= TcpPacketCapture;
		}
	}
	class IPv4PacketInterface : PacketInterface
	{
		public IPv4PacketInterface(ICaptureDevice captureDevice) : base(captureDevice)
		{ }

		public override void PacketCapture(int captureNum)
		{
			this.CaptureNum = captureNum;
			PacketCaptureDevice.Filter = "ip";
			ResultData = "";
			PacketCaptureDevice.OnPacketArrival += Ipv4PacketCapture;
			PacketCaptureDevice.StartCapture();
		}

		public override void PacketCapture(int captureNum, string filter)
		{
			this.CaptureNum = captureNum;
			PacketCaptureDevice.Filter = "ip";
			ResultData = "";
			try
			{
				SetPacketFillter(filter);
				PacketCaptureDevice.OnPacketArrival += Ipv4PacketCapture;
				PacketCaptureDevice.StartCapture();
			}
			catch (PcapException wrongFilter)
			{
				Console.WriteLine(wrongFilter.StackTrace);
				MessageBox.Show("Please reset filter and start.", "Wrong filter!", MessageBoxButton.OK);
			}
		}
		public void Ipv4PacketCapture(object sender, CaptureEventArgs e) //Packet capture and return to string (async)
		{
			RawCapture capturePacket = e.Packet;
			try
			{
				if (this.NowCaptureNum <= this.CaptureNum)
				{
					var packet = PacketDotNet.Packet.ParsePacket(capturePacket.LinkLayerType, capturePacket.Data);
					IpPacket ipPacket = (IpPacket)packet.Extract(typeof(PacketDotNet.IpPacket));

					if (ipPacket.Version != IpVersion.IPv4)
						return;

					IPv4Packet ipv4Packet = (IPv4Packet)ipPacket.Extract(typeof(PacketDotNet.IPv4Packet));
					this.NowCaptureNum++;
					ResultData += "Version:" + ipv4Packet.Version+" ";
					ResultData += "Header Length:" + ipv4Packet.HeaderLength + " ";
					ResultData += "QOS:" + ipv4Packet.TypeOfService + " ";
					ResultData += "Packet Length:"+ipv4Packet.PayloadLength+"\n";
					ResultData += "Identifier:" + ipv4Packet.Id + " ";
					ResultData += "Flags:" + ipv4Packet.FragmentFlags + " ";
					ResultData += "Fragment Offset:" + ipv4Packet.FragmentOffset + "\n";
					ResultData += "TTL:" + ipv4Packet.TimeToLive + " ";
					ResultData += "Protocol:" + ipv4Packet.Protocol + " ";
					ResultData += "CheckSum:" + ipv4Packet.Checksum + "\n";
					ResultData += "Source:" + ipv4Packet.SourceAddress + "\n";
					ResultData += "Destination:" + ipv4Packet.DestinationAddress + "\n";
					int i = 1;

					if (ipv4Packet.PayloadData != null)
					{
						foreach (byte data in ipv4Packet.PayloadData)
						{
							ResultData += Convert.ToString(data, 16) + " ";
							if (i % 8 == 0)
								ResultData += "\n";
							i++;
						}
					}
					ResultData += "\n--------------------------------------------\n";

					if (this.NowCaptureNum == this.CaptureNum)
						StopPacketCapture();
					SendPacketData();
				}

				else
				{
					StopPacketCapture();
					//PacketCaptureDevice.Close();
					CaptureEndEvent();
				}
			}
			catch (NullReferenceException nullException)
			{
				Console.WriteLine(nullException.StackTrace);
				MessageBox.Show("Can't packet extracted. \n Are you set others protocol in filter?"
					, "Warining", System.Windows.MessageBoxButton.OK);
				StopPacketCapture();
				//PacketCaptureDevice.Close();
			}
		}

		public override void RemoveHanlder()
		{
			PacketCaptureDevice.OnPacketArrival -= Ipv4PacketCapture;
		}
	}

	class IPv6PacketInterface : PacketInterface
	{
		public IPv6PacketInterface(ICaptureDevice captureDevice) : base(captureDevice)
		{ }

		public override void PacketCapture(int captureNum)
		{
			this.CaptureNum = captureNum;
			PacketCaptureDevice.Filter = "ip";
			ResultData = "";
			PacketCaptureDevice.OnPacketArrival += Ipv6PacketCapture;
			PacketCaptureDevice.StartCapture();
		}

		public override void PacketCapture(int captureNum, string filter)
		{
			this.CaptureNum = captureNum;
			PacketCaptureDevice.Filter = "ip";
			ResultData = "";
			try
			{
				SetPacketFillter(filter);
				PacketCaptureDevice.OnPacketArrival += Ipv6PacketCapture;
				PacketCaptureDevice.StartCapture();
			}
			catch (PcapException wrongFilter)
			{
				Console.WriteLine(wrongFilter.StackTrace);
				MessageBox.Show("Please reset filter and start.", "Wrong filter!", MessageBoxButton.OK);
			}
		}
		public void Ipv6PacketCapture(object sender, CaptureEventArgs e) //Packet capture and return to string (async)
		{
			RawCapture capturePacket = e.Packet;
			try
			{
				if (this.NowCaptureNum <= this.CaptureNum)
				{
					var packet = PacketDotNet.Packet.ParsePacket(capturePacket.LinkLayerType, capturePacket.Data);
					IpPacket ipPacket = (IpPacket)packet.Extract(typeof(PacketDotNet.IpPacket));

					if (ipPacket.Version != IpVersion.IPv6)
						return;

					IPv6Packet ipv6Packet = (IPv6Packet)ipPacket.Extract(typeof(PacketDotNet.IPv6Packet));
					this.NowCaptureNum++;

					ResultData += "Version:" + ipv6Packet.Version + " ";
					ResultData += "Traffic Class:" + ipv6Packet.TrafficClass + " ";
					ResultData += "Flow Label:" +ipv6Packet.FlowLabel +"\n";
					ResultData += "Payload Length:" + ipv6Packet.PayloadLength + " ";
					ResultData += "Next Header:" + ipv6Packet.NextHeader + " ";
					ResultData += "Hop Limit:" + ipv6Packet.HopLimit + "\n";
					ResultData += "Source Address:" + ipv6Packet.SourceAddress + "\n";
					ResultData += "Destination Address:" + ipv6Packet.DestinationAddress + "\n";

					int i = 1;

					if (ipv6Packet.PayloadData != null)
					{
						foreach (byte data in ipv6Packet.PayloadData)
						{
							ResultData += Convert.ToString(data, 16) + " ";
							if (i % 8 == 0)
								ResultData += "\n";
							i++;
						}
					}
					ResultData += "\n--------------------------------------------\n";

					if (this.NowCaptureNum == this.CaptureNum)
						StopPacketCapture();
					SendPacketData();
				}

				else
				{
					StopPacketCapture();
					//PacketCaptureDevice.Close();
					CaptureEndEvent();
				}
			}
			catch (NullReferenceException nullException)
			{
				Console.WriteLine(nullException.StackTrace);
				MessageBox.Show("Can't packet extracted. \n Are you set others protocol in filter?"
					, "Warining", System.Windows.MessageBoxButton.OK);
				StopPacketCapture();
				//PacketCaptureDevice.Close();
			}
		}

		public override void RemoveHanlder()
		{
			PacketCaptureDevice.OnPacketArrival -= Ipv6PacketCapture;
		}
	}
	class UDPPacketInterface : PacketInterface
	{
		public UDPPacketInterface(ICaptureDevice captureDevice) : base(captureDevice)
		{ }

		public override void PacketCapture(int captureNum)
		{
			this.CaptureNum = captureNum;
			PacketCaptureDevice.Filter = "udp";
			ResultData = "";
			PacketCaptureDevice.OnPacketArrival += UdpPacketCapture;
			PacketCaptureDevice.StartCapture();
		}

		public override void PacketCapture(int captureNum, string filter)
		{
			this.CaptureNum = captureNum;
			PacketCaptureDevice.Filter = "udp";
			ResultData = "";
			try
			{
				SetPacketFillter(filter);
				PacketCaptureDevice.OnPacketArrival += UdpPacketCapture;
				PacketCaptureDevice.StartCapture();
			}
			catch (PcapException wrongFilter)
			{
				Console.WriteLine(wrongFilter.StackTrace);
				MessageBox.Show("Please reset filter and start.", "Wrong filter!", MessageBoxButton.OK);
			}
		}
		public void UdpPacketCapture(object sender, CaptureEventArgs e) //Packet capture and return to string (async)
		{
			RawCapture capturePacket = e.Packet;


			this.NowCaptureNum++;

			try
			{
				if (this.NowCaptureNum <= this.CaptureNum)
				{
					var packet = PacketDotNet.Packet.ParsePacket(capturePacket.LinkLayerType, capturePacket.Data);
					UdpPacket udpPacket = (UdpPacket)packet.Extract(typeof(PacketDotNet.UdpPacket));

					ResultData += "Source Port:" +udpPacket.SourcePort +" ";
					ResultData += "Destination Port:" + udpPacket.DestinationPort + "\n";
					ResultData += "Length:" + udpPacket.Length + " ";
					ResultData +="CheckSum:" +udpPacket.Checksum +"\n";

					int i = 1;

					if (udpPacket.PayloadData != null)
					{
						foreach (byte data in udpPacket.PayloadData)
						{
							ResultData += Convert.ToString(data, 16) + " ";
							if (i % 8 == 0)
								ResultData += "\n";
							i++;
						}
					}
					ResultData += "\n--------------------------------------------\n";

					if (this.NowCaptureNum == this.CaptureNum)
						StopPacketCapture();
					SendPacketData();
				}

				else
				{
					StopPacketCapture();
					//PacketCaptureDevice.Close();
					CaptureEndEvent();
				}
			}
			catch (NullReferenceException nullException)
			{
				Console.WriteLine(nullException.StackTrace);
				MessageBox.Show("Can't packet extracted. \n Are you set others protocol in filter?"
					, "Warining", System.Windows.MessageBoxButton.OK);
				StopPacketCapture();
				//PacketCaptureDevice.Close();
			}
		}

		public override void RemoveHanlder()
		{
			PacketCaptureDevice.OnPacketArrival -= UdpPacketCapture;
		}
	}

	class ARPPacketInterface : PacketInterface
	{
		public ARPPacketInterface(ICaptureDevice captureDevice) : base(captureDevice)
		{ }

		public override void PacketCapture(int captureNum)
		{
			this.CaptureNum = captureNum;
			PacketCaptureDevice.Filter = "arp";
			ResultData = "";
			PacketCaptureDevice.OnPacketArrival += ArpPacketCapture;
			PacketCaptureDevice.StartCapture();
		}

		public override void PacketCapture(int captureNum, string filter)
		{
			this.CaptureNum = captureNum;
			PacketCaptureDevice.Filter = "arp";
			ResultData = "";
			try
			{
				SetPacketFillter(filter);
				PacketCaptureDevice.OnPacketArrival += ArpPacketCapture;
				PacketCaptureDevice.StartCapture();
			}
			catch (PcapException wrongFilter)
			{
				Console.WriteLine(wrongFilter.StackTrace);
				MessageBox.Show("Please reset filter and start.", "Wrong filter!", MessageBoxButton.OK);
			}
		}
		public void ArpPacketCapture(object sender, CaptureEventArgs e) //Packet capture and return to string (async)
		{
			RawCapture capturePacket = e.Packet;


			this.NowCaptureNum++;

			try
			{
				if (this.NowCaptureNum <= this.CaptureNum)
				{
					var packet = PacketDotNet.Packet.ParsePacket(capturePacket.LinkLayerType, capturePacket.Data);
					ARPPacket arpPacket = (ARPPacket)packet.Extract(typeof(PacketDotNet.ARPPacket));

					ResultData += "Hardware Type:" + arpPacket.HardwareAddressType + " ";
					ResultData += "Protocol TYpe:"+arpPacket.ProtocolAddressType+"\n";
					ResultData += "Hardware Address Length:" + arpPacket.HardwareAddressLength + " ";
					ResultData += "Protocol Address Length:" + arpPacket.ProtocolAddressLength + " ";
					ResultData += "Operation:" + arpPacket.Operation + "\n";
					ResultData += "Sender Hardware Address" +arpPacket.SenderHardwareAddress +"\n";
					ResultData += "Sender Protocol Address" + arpPacket.SenderProtocolAddress + "\n";
					ResultData += "Target Hardware Address" + arpPacket.TargetHardwareAddress + "\n";
					ResultData += "Target Protocol Address" + arpPacket.TargetProtocolAddress + "\n";

					int i = 1;

					if (arpPacket.PayloadData != null)
					{
						foreach (byte data in arpPacket.PayloadData)
						{
							ResultData += Convert.ToString(data, 16) + " ";
							if (i % 8 == 0)
								ResultData += "\n";
							i++;
						}
					}
					ResultData += "\n--------------------------------------------\n";

					if (this.NowCaptureNum == this.CaptureNum)
						StopPacketCapture();
					SendPacketData();
				}

				else
				{
					StopPacketCapture();
					//PacketCaptureDevice.Close();
					CaptureEndEvent();
				}
			}
			catch (NullReferenceException nullException)
			{
				Console.WriteLine(nullException.StackTrace);
				MessageBox.Show("Can't packet extracted. \n Are you set others protocol in filter?"
					, "Warining", System.Windows.MessageBoxButton.OK);
				StopPacketCapture();
				//PacketCaptureDevice.Close();
			}
		}

		public override void RemoveHanlder()
		{
			PacketCaptureDevice.OnPacketArrival -= ArpPacketCapture;
		}
	}

	class ICMPPacketInterface : PacketInterface
	{
		public ICMPPacketInterface(ICaptureDevice captureDevice) : base(captureDevice)
		{ }

		public override void PacketCapture(int captureNum)
		{
			this.CaptureNum = captureNum;
			PacketCaptureDevice.Filter = "ip";
			ResultData = "";
			PacketCaptureDevice.OnPacketArrival += IcmpPacketCapture;
			PacketCaptureDevice.StartCapture();
		}

		public override void PacketCapture(int captureNum, string filter)
		{
			this.CaptureNum = captureNum;
			PacketCaptureDevice.Filter = "ip";
			ResultData = "";
			try
			{
				SetPacketFillter(filter);
				PacketCaptureDevice.OnPacketArrival += IcmpPacketCapture;
				PacketCaptureDevice.StartCapture();
			}
			catch (PcapException wrongFilter)
			{
				Console.WriteLine(wrongFilter.StackTrace);
				MessageBox.Show("Please reset filter and start.", "Wrong filter!", MessageBoxButton.OK);
			}
		}
		public void IcmpPacketCapture(object sender, CaptureEventArgs e) //Packet capture and return to string (async)
		{
			RawCapture capturePacket = e.Packet;
			try
			{
				if (this.NowCaptureNum <= this.CaptureNum)
				{
					var packet = PacketDotNet.Packet.ParsePacket(capturePacket.LinkLayerType, capturePacket.Data);
					IpPacket ipPacket = (IpPacket)packet.Extract(typeof(PacketDotNet.IpPacket));

					if (ipPacket.Version != IpVersion.IPv4 || ipPacket.Protocol!=IPProtocolType.ICMP)
						return;

					ICMPv4Packet icmpPacket = (ICMPv4Packet)ipPacket.Extract(typeof(PacketDotNet.ICMPv4Packet));
					this.NowCaptureNum++;

					ResultData += "Header:" + icmpPacket.Header + "\n";
					
					int i = 1;

					if (icmpPacket.PayloadData != null)
					{
						foreach (byte data in icmpPacket.PayloadData)
						{
							ResultData += Convert.ToString(data, 16) + " ";
							if (i % 8 == 0)
								ResultData += "\n";
							i++;
						}
					}
					ResultData += "\n--------------------------------------------\n";

					if (this.NowCaptureNum == this.CaptureNum)
						StopPacketCapture();
					SendPacketData();
				}

				else
				{
					StopPacketCapture();
					//PacketCaptureDevice.Close();
					CaptureEndEvent();
				}
			}
			catch (NullReferenceException nullException)
			{
				Console.WriteLine(nullException.StackTrace);
				MessageBox.Show("Can't packet extracted. \n Are you set others protocol in filter?"
					, "Warining", System.Windows.MessageBoxButton.OK);
				StopPacketCapture();
				//PacketCaptureDevice.Close();
			}
		}

		public override void RemoveHanlder()
		{
			PacketCaptureDevice.OnPacketArrival -= IcmpPacketCapture;
		}
	}

	class IGMPPacketInterface : PacketInterface
	{
		public IGMPPacketInterface(ICaptureDevice captureDevice) : base(captureDevice)
		{ }

		public override void PacketCapture(int captureNum)
		{
			this.CaptureNum = captureNum;
			PacketCaptureDevice.Filter = "ip";
			ResultData = "";
			PacketCaptureDevice.OnPacketArrival += IgmpPacketCapture;
			PacketCaptureDevice.StartCapture();
		}

		public override void PacketCapture(int captureNum, string filter)
		{
			this.CaptureNum = captureNum;
			PacketCaptureDevice.Filter = "ip";
			ResultData = "";
			try
			{
				SetPacketFillter(filter);
				PacketCaptureDevice.OnPacketArrival += IgmpPacketCapture;
				PacketCaptureDevice.StartCapture();
			}
			catch (PcapException wrongFilter)
			{
				Console.WriteLine(wrongFilter.StackTrace);
				MessageBox.Show("Please reset filter and start.", "Wrong filter!", MessageBoxButton.OK);
			}
		}
		public void IgmpPacketCapture(object sender, CaptureEventArgs e) //Packet capture and return to string (async)
		{
			RawCapture capturePacket = e.Packet;
			try
			{
				if (this.NowCaptureNum <= this.CaptureNum)
				{
					var packet = PacketDotNet.Packet.ParsePacket(capturePacket.LinkLayerType, capturePacket.Data);
					IpPacket ipPacket = (IpPacket)packet.Extract(typeof(PacketDotNet.IpPacket));

					if (ipPacket.Version != IpVersion.IPv4 || ipPacket.Protocol != IPProtocolType.IGMP)
						return;

					IGMPv2Packet igmpPacket = (IGMPv2Packet)ipPacket.Extract(typeof(PacketDotNet.IGMPv2Packet));
					this.NowCaptureNum++;

					ResultData += "Header:" + igmpPacket.Header + "\n";

					int i = 1;

					if (igmpPacket.PayloadData != null)
					{
						foreach (byte data in igmpPacket.PayloadData)
						{
							ResultData += Convert.ToString(data, 16) + " ";
							if (i % 8 == 0)
								ResultData += "\n";
							i++;
						}
					}
					ResultData += "\n--------------------------------------------\n";

					if (this.NowCaptureNum == this.CaptureNum)
						StopPacketCapture();
					SendPacketData();
				}

				else
				{
					StopPacketCapture();
					//PacketCaptureDevice.Close();
					CaptureEndEvent();
				}
			}
			catch (NullReferenceException nullException)
			{
				Console.WriteLine(nullException.StackTrace);
				MessageBox.Show("Can't packet extracted. \n Are you set others protocol in filter?"
					, "Warining", System.Windows.MessageBoxButton.OK);
				StopPacketCapture();
				//PacketCaptureDevice.Close();
			}
		}

		public override void RemoveHanlder()
		{
			PacketCaptureDevice.OnPacketArrival -= IgmpPacketCapture;
		}
	}
}
