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
		private Thread caputreThread;
		public delegate void startAndStop();
		private volatile bool isThreadRunning;
		private String resultData="";

		public ICaptureDevice NowCatchingDevice
		{
			set
			{
				nowCatchingDevice = value;
				packetInterface.PacketCaptureDevice = value;
			}
		}

		public String ResultData
		{
			get{ return resultData; }
		}

		public bool IsThreadRunning
		{
			get { return isThreadRunning; }
		}


		public int PacketCaptureNum
		{
			set { packetCaptureNum = value; }
			get { return packetCaptureNum; }
		}

		public PacketInterface PacketInterface
		{
			set { packetInterface = value; }
		}
		public ProcessingManeger()
		{
			packetCaptureNum = START_PACKET_NUM;
			caputreThread = new Thread(new ThreadStart(capturePacket));
			isThreadRunning = false;
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
			if (isThreadRunning)
				isThreadRunning = false;

		} //Capture Stop

		public void startCaputrePacket()
		{
			if (!isThreadRunning)
			{
				caputreThread.Start();
				isThreadRunning = true;
			}
		}

		private void capturePacket()
		{
			bool successFlag = false;
			packetInterface.openDevice(DEFAULT_TIMEOUT);

			for (int packetCount = 0; packetCount < packetCaptureNum && isThreadRunning; packetCount++)
			{
				resultData += packetInterface.PacketCapture(ref successFlag);

				if (!successFlag)
					break;
			}

			isThreadRunning = false;

		}
		
	}
}
