using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using SharpPcap.AirPcap;
using SharpPcap.WinPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		public void openDevice(int readTimeoutMilliseconds)
		{
			if (isDeviceOpend != true)
			{
				packetCaptureDevice.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds); //need to add error handling
				isDeviceOpend = false;
			}
		}

		public void dataSendEvent()
		{
			if (dataSend != null)
				dataSend();
		}

		//public abstract void PacketCapture(int captureNum, ref bool successFlag);
		public abstract void packetCapture(int captureNum);
		public void stopPacketCapture()
		{
			packetCaptureDevice.StopCapture();
			ResultData += this.NowCaptureNum + " packet captured \n";
			this.NowCaptureNum = 0;
			sendPacketData();
		}

		public void sendPacketData() //Send packetdata to main window
		{
			dataSendEvent();
			resultData = "";
		}
		public abstract void PacketFillter(); //make protocl packet fillter
	}
	class TCPPacketInterface : PacketInterface
	{
		public TCPPacketInterface(ICaptureDevice captureDevice) :base(captureDevice)
		{}

		public override void packetCapture(int captureNum)
		{
			this.CaptureNum = captureNum;
			PacketCaptureDevice.Filter = "tcp";
			PacketCaptureDevice.OnPacketArrival += TcpPacketCapture;
			PacketCaptureDevice.StartCapture();
		}

		public override void PacketFillter()
		{

		}
		public void TcpPacketCapture(object sender, CaptureEventArgs e) //Packet capture and return to string (async)
		{
			RawCapture capturePacket=e.Packet;
			

			this.NowCaptureNum++;

			if (this.NowCaptureNum <= this.CaptureNum)
			{
				var packet = PacketDotNet.Packet.ParsePacket(capturePacket.LinkLayerType, capturePacket.Data);
				TcpPacket tcpPacket=(TcpPacket)packet.Extract(typeof(PacketDotNet.TcpPacket));

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
				int i=1;

				foreach (byte data in tcpPacket.PayloadData)
				{
					ResultData += data+" ";
					if (i % 8 == 0)
						ResultData += "\n";
					i++;
				}
				ResultData += "\n--------------------------------------------\n;";

				if (this.NowCaptureNum == this.CaptureNum)
					ResultData += this.CaptureNum+" packet captured \n";

				sendPacketData();
			}

			else
			{
				PacketCaptureDevice.StopCapture();
				//PacketCaptureDevice.Close();
				this.NowCaptureNum = 0;
			}
		}
	}
}
