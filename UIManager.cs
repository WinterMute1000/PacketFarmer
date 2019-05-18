using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//This class's all method is async maybe
namespace PacketFarmer
{
	class UIManager
	{
		private ProcessingManeger refProcessingManeger;
		public ProcessingManeger RefProcessingManeger
		{
			set { refProcessingManeger = value; }
		}
		public void saveResult() //Save Result(selected path)
		{
		}
		public void selectProtocol()
		{

		}//Send event to Processing Maneger for change protocol

		public void stopCapture()
		{

		} // Stop packet capture event send to Processing Maneger
		public void setPacketCaptureNum ()
		{

		} //Send packet num to Processing Maneger

		public void updateScrollView() //Add result
		{

		}
		public void exitProgram() //End Program
		{
		}
	}
}
