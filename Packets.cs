using PacketDotNet;
using SharpPcap;
using System;
using System.ComponentModel;
using System.Reflection;
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

					foreach (byte data in tcpPacket.PayloadData)
					{
						ResultData += Convert.ToString(data, 16) + " ";
						if (i % 8 == 0)
							ResultData += "\n";
						i++;
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
	class IPPacketInterface : PacketInterface
	{
		public IPPacketInterface(ICaptureDevice captureDevice) : base(captureDevice)
		{ }

		public override void PacketCapture(int captureNum)
		{
			this.CaptureNum = captureNum;
			PacketCaptureDevice.Filter = "ip";
			ResultData = "";
			PacketCaptureDevice.OnPacketArrival += IpPacketCapture;
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
				PacketCaptureDevice.OnPacketArrival += IpPacketCapture;
				PacketCaptureDevice.StartCapture();
			}
			catch (PcapException wrongFilter)
			{
				Console.WriteLine(wrongFilter.StackTrace);
				MessageBox.Show("Please reset filter and start.", "Wrong filter!", MessageBoxButton.OK);
			}
		}
		public void IpPacketCapture(object sender, CaptureEventArgs e) //Packet capture and return to string (async)
		{
			RawCapture capturePacket = e.Packet;


			this.NowCaptureNum++;

			try
			{
				if (this.NowCaptureNum <= this.CaptureNum)
				{
					var packet = PacketDotNet.Packet.ParsePacket(capturePacket.LinkLayerType, capturePacket.Data);
					IpPacket IpPacket = (IpPacket)packet.Extract(typeof(PacketDotNet.IpPacket));
					ResultData += "Version:" + IpPacket.Version+" ";
					ResultData += "Header Length:" + IpPacket.HeaderLength + " ";
					//ResultData += "QOS:" + IpPacket.TypeOfService + " ";
					ResultData += "Packet Length:"+IpPacket.PayloadLength+"\n";
					//ResultData += "Identifier:" + IpPacket.Id + " ";
					//ResultData += "Flags:" + IpPacket.FragmentFlags + " ";
					//ResultData += "Fragment Offset:" + IpPacket.FragmentOffset + "\n";
					ResultData += "TTL:" + IpPacket.TimeToLive + " ";
					ResultData += "Protocol:" + IpPacket.Protocol + " ";
					//ResultData += "CheckSum:" + IpPacket.Checksum + "\n";
					ResultData += "Source:" + IpPacket.SourceAddress + "\n";
					ResultData += "Destination:" + IpPacket.DestinationAddress + "\n";
					int i = 1;

					if (IpPacket.PayloadData != null)
					{
						foreach (byte data in IpPacket.PayloadData)
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
				PacketCaptureDevice.OnPacketArrival -= IpPacketCapture;
				StopPacketCapture();
				//PacketCaptureDevice.Close();
			}
		}

		public override void RemoveHanlder()
		{
			PacketCaptureDevice.OnPacketArrival -= IpPacketCapture;
		}
	}
}
