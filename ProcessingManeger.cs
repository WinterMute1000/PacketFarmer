using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using SharpPcap.AirPcap;
using SharpPcap.WinPcap;
using System.Windows;
//This class's all method is async maybe

namespace PacketFarmer
{
	class ProcessingManeger
	{
		public static readonly int START_PACKET_NUM=5;
		public static readonly int DEFAULT_TIMEOUT = 1000;
		private int packetCaptureNum;
		private ICaptureDevice nowCatchingDevice;
		private PacketInterface packetInterface;

		public ICaptureDevice NowCatchingDevice
		{
			set
			{
				nowCatchingDevice = value;
				packetInterface.PacketCaptureDevice = value;
			}
			get
			{ return nowCatchingDevice; }
		}

		public int PacketCaptureNum
		{
			set { packetCaptureNum = value; }
			get { return packetCaptureNum; }
		}

		public PacketInterface PacketInterface
		{
			get { return packetInterface; }
			set { packetInterface = value; }
		}
		public ProcessingManeger()
		{
			packetCaptureNum = START_PACKET_NUM;
			//isThreadRunning = false;
		}

		public void OpenInterface() //Open Interface
		{
			packetInterface.OpenDevice(DEFAULT_TIMEOUT);
		}

		public void CaptureStop()
		{
			packetInterface.stopPacketCapture();

		} //Capture Stop

		public void StartCaputrePacket() //Packet capture thread start
		{

			packetInterface.OpenDevice(DEFAULT_TIMEOUT);
			packetInterface.PacketCapture(packetCaptureNum);
		}

		public void StartCaputrePacket(string filter) //Packet capture thread start
		{

			packetInterface.OpenDevice(DEFAULT_TIMEOUT);
			packetInterface.PacketCapture(packetCaptureNum,filter);
		}

	}
}
