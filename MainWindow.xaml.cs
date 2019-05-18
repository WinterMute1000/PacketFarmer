using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using SharpPcap.AirPcap;
using SharpPcap.WinPcap;

namespace PacketFarmer
{
	public partial class MainWindow : Window
	{
		UIManager um;
		ProcessingManeger pm;
		public MainWindow()
		{
			UIManager um = new UIManager();
			ProcessingManeger pm = new ProcessingManeger(um);

			um.RefProcessingManeger = pm;
			InitializeComponent();
		}

		private void comboBox_Load(object sender, RoutedEventArgs e) //Load Interface 
		{
			CaptureDeviceList captureDevices = CaptureDeviceList.Instance;
			pm.CapInstancce = captureDevices;

			foreach (ICaptureDevice dev in captureDevices)
				select_interface_combo.Items.Add(dev.Name);
		}
		private void menuSaveClick(object sender, RoutedEventArgs e) //Save Packet Capturd Data
		{
		}

		private void menuSelectClick(object sender, RoutedEventArgs e) //Select Protocol(Processing Sub Menu)
		{

		}
		private void menuStopClick (object sender, RoutedEventArgs e) //Stop Packet Capturing
		{

		}

		private void changeInterface(object sender, SelectionChangedEventArgs e) //OnSelectionChanged
		{

		}

		private void cofirmInterface(object sender, RoutedEventArgs e) //Click Change Button
		{

		}

		private void changePacketNum(object sender, RoutedEventArgs e) //Click Num Button
		{

		}
	}
}
