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

namespace PacketFarmer
{
	abstract class PacketInterface
	{
		private ICaptureDevice packetCaptureDevice;
		public ICaptureDevice PacketCaptureDevice //PacketInterface capturing interfece
		{
			set { packetCaptureDevice = value; }
			get { return packetCaptureDevice; }
		}

		public PacketInterface() { }
		public PacketInterface(ICaptureDevice captureDevice)
		{
			PacketCaptureDevice = captureDevice;
		}
		public void openDevice(int readTimeoutMilliseconds)
		{
			packetCaptureDevice.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds); //need to add error handling
		}
		public abstract String PacketCapture(ref bool successFlag);
		//public abstract void PacketFillter(); make protocl packet fillter
	}
	class TCPPacketInterface : PacketInterface
	{
		public TCPPacketInterface() { }
		public TCPPacketInterface(ICaptureDevice captureDevice)
		{
		}
		public override string PacketCapture(ref bool successFlag) //Packet capture and return to string (async)
		{
			RawCapture capturePacket;
			String returnString="";

			successFlag = false;
			try
			{
				if ((capturePacket = PacketCaptureDevice.GetNextPacket()) != null)
				{
					Packet packet = (Packet)PacketDotNet.TcpPacket.ParsePacket(capturePacket.LinkLayerType, capturePacket.Data);
					TranslationPacketHeader(packet, ref returnString);
				}
				else
					throw new Exception();

				return returnString;
			}
			catch (Exception e)
			{
				successFlag = true;
				returnString = "Can't Not Captured Next Packet.\n";
				return returnString;
			}
		}
	    private void TranslationPacketHeader(Packet packet,ref string returnString) //Translation packet header to string
		{
			TcpPacket tcpPacket = (TcpPacket)packet;

			//Do next time.
		}
	}
}
