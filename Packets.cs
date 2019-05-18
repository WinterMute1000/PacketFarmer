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
					TcpPacket packet = (TcpPacket)PacketDotNet.TcpPacket.ParsePacket(capturePacket.LinkLayerType, capturePacket.Data);

					returnString += "Source Port:"+packet.SourcePort.ToString()+" ";
					returnString += "DestinationPort:" + packet.DestinationPort.ToString() + "\n";
					returnString += "SequenceNumber:" + packet.SequenceNumber.ToString() + "\n";
					returnString += "AcknowledgmentNumber:" + packet.AcknowledgmentNumber.ToString() + "\n";
					returnString +="DataOffset:" + packet.DataOffset.ToString()+" ";
					returnString += "Flag:" + packet.AllFlags.ToString() + " ";
					returnString += "Window Size:" + packet.WindowSize.ToString() + "\n";
					returnString += "Checksum:" + packet.Checksum.ToString() + " ";
					returnString += "Urgent Pointer:" + packet.UrgentPointer.ToString() + "\n";

					returnString += "Data" + packet.PayloadData.ToString() + "\n";
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
	}
}
