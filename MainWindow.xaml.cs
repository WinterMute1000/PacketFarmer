using System;
using System.IO;
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
using Microsoft.Win32;
using System.Windows.Threading;

namespace PacketFarmer
{
	public partial class MainWindow : Window
	{
		ProcessingManeger pm = new ProcessingManeger();
		CaptureDeviceList captureDevices;
		String selectedInterface = "";
		String nowProtocol = "";// Using save

		public MainWindow()
		{
			nowProtocol = "TCP";
			pm.PacketInterface = new TCPPacketInterface(pm.NowCatchingDevice);
			PacketInterface.dataSend += this.setTextBlock;
			InitializeComponent();
		}

		private void comboBoxLoad(object sender, RoutedEventArgs e) //Load Interface 
		{
			captureDevices = CaptureDeviceList.Instance;

			if (captureDevices.Count < 1)
			{
				MessageBox.Show("No devices were found on this machine", "Warining", MessageBoxButton.OK);
				return;
			}

			foreach (ICaptureDevice dev in captureDevices)
				select_interface_combo.Items.Add(dev.Name);

			selectedInterface = select_interface_combo.Items.GetItemAt(0).ToString();
			select_interface_combo.Text = select_interface_combo.Items.GetItemAt(0).ToString();
			pm.NowCatchingDevice=captureDevices.ElementAt(0);
		}
		private void menuStartClick(object sender, RoutedEventArgs e)
		{
			show_packet.Text = "";
			pm.openInterface();
			pm.startCaputrePacket();
		}
		private void menuSaveClick(object sender, RoutedEventArgs e) //Save Packet Capturd Data
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter= "Text file(*.txt) | *.txt";
			saveFileDialog.FileName= DateTime.Now.ToString("yyyy-MM-dd-HH-mm")+"-"+nowProtocol+"-"
				+pm.PacketCaptureNum+" packet captured";

			if (saveFileDialog.ShowDialog() == true)
				File.WriteAllText(saveFileDialog.FileName, show_packet.Text);

		}

		private void TCPSelected(object sender, RoutedEventArgs e) //Select Protocol(Processing Sub Menu)
		{
			pm.PacketInterface = new TCPPacketInterface(pm.NowCatchingDevice);
			nowProtocol = "TCP";
		}
		private void menuStopClick(object sender, RoutedEventArgs e) //Stop Packet Capturing
		{
			pm.captureStop();
		}

		private void changeInterface(object sender, SelectionChangedEventArgs e) //OnSelectionChanged
		{
			if (select_interface_combo.SelectedIndex >= 0)
			{
				selectedInterface = select_interface_combo.SelectedItem.ToString();
			}
			else
				MessageBox.Show("Wrong select.", "Warining", MessageBoxButton.OK);
		}
		private void cofirmInterface(object sender, RoutedEventArgs e) //Click Change Button
		{
			/*foreach (ICaptureDevice dev in captureDevices) //Maybe Use Linq?
			{
				if (dev.Name == selectedInterface)
					pm.NowCatchingDevice = dev;
			}*/

			var selectDev = from dev in captureDevices
								where dev.Name == selectedInterface
								select dev;

			pm.NowCatchingDevice = selectDev.ElementAt(0);
		}

		private void numEditLoad(object sender, RoutedEventArgs e)
		{
			packet_num_edit.Text = ProcessingManeger.START_PACKET_NUM.ToString();
		}
		private void changePacketCaptureNum(object sender, RoutedEventArgs e) //Click Num Button
		{
			int textNum = int.Parse(packet_num_edit.Text.ToString());

			if (textNum < 0 || textNum > 1024)
			{
				MessageBox.Show("Capture Length(1~1024)", "Warining", MessageBoxButton.OK);
				return;
			}

			pm.PacketCaptureNum = textNum;
		}

		private void editPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (!(((Key.D0 <= e.Key) && (e.Key <= Key.D9))
				|| ((Key.NumPad0 <= e.Key) && (e.Key <= Key.NumPad9))
				|| e.Key == Key.Back))
			{
				e.Handled = true;
			}
		}

		public void setTextBlock()
		{
			Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
			 { show_packet.Text += pm.PacketInterface.ResultData; }));
		}

	}
}