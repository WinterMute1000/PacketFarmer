using System;
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
		private CaptureDeviceList capInstance;
		private ICaptureDevice nowCatchingDevice;
		private PacketInterface packetInterface;

		public CaptureDeviceList CapInstancce
		{
			set { capInstance = value; }
		}
		private UIManager refUIManager;
		public UIManager RefUIManager
		{
			set { refUIManager = value; }
		}
		public ProcessingManeger()
		{
			packetCaptureNum = START_PACKET_NUM;
		}
		public ProcessingManeger(UIManager refUI) :this()
		{
			RefUIManager = refUI;
		}
		public void pcapError() //Error Handling
		{

		}
		public void FindInterface() { } //Interface select

		public void openInterface() //Open Interface;
		{
			packetInterface.openDevice(DEFAULT_TIMEOUT);
		}

		public void startCapturePacket()
		{
			string packetData = null;
			bool successFlag=false;
			//Do Filter register next time.
			for (int packetCount = 0; packetCount < packetCaptureNum; packetCount++)
				packetData=packetInterface.PacketCapture(ref successFlag);
		}
	}
}
