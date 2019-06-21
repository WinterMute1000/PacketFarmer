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
		//private Thread caputreThread;
		//private String resultData = "";
		//private volatile bool isThreadRunning;

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

		/*public String ResultData
		{
			get{ return resultData; }
		}*/

		/*public bool IsThreadRunning
		{
			get { return isThreadRunning; }
		}*/


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
		public void pcapError() //Error Handling
		{
		}

		public void openInterface() //Open Interface
		{
			packetInterface.openDevice(DEFAULT_TIMEOUT);
		}

		public void captureStop()
		{
			/*if (isThreadRunning)
				isThreadRunning = false;*/

			packetInterface.stopPacketCapture();

		} //Capture Stop

		public void startCaputrePacket() //Packet capture thread start
		{
			/*if (!isThreadRunning)
			{
				caputreThread = new Thread(new ThreadStart(capturePacket));
				caputreThread.Start();
				isThreadRunning = true;
			}*/

			packetInterface.openDevice(DEFAULT_TIMEOUT);
			packetInterface.packetCapture(packetCaptureNum);
		}

		/*private void capturePacket()
		{
			//bool successFlag = false;
			packetInterface.openDevice(DEFAULT_TIMEOUT);
			packetInterface.packetCapture(packetCaptureNum);
		}*/
		
	}
}
