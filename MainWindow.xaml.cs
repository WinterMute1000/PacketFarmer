using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SharpPcap;
using Microsoft.Win32;
using System.Windows.Threading;
using System.Text.RegularExpressions;

namespace PacketFarmer
{
	public partial class MainWindow : Window
	{
		private ProcessingManeger pm = new ProcessingManeger();
		private CaptureDeviceList captureDevices;
		private String selectedInterface = "";
		private String nowProtocol = "";// Using save
		private bool isStarted = false;
		private bool isPacketFilterSet = false;
		private string packetFilter="";

		public MainWindow()
		{
			nowProtocol = "TCP";
			pm.PacketInterface = new TCPPacketInterface(pm.NowCatchingDevice);
			PacketInterface.dataSend += this.SetTextBlock;
			PacketInterface.captureEndHandle += this.CaptureEnd;
			InitializeComponent();
		}

		private void ComboBoxLoad(object sender, RoutedEventArgs e) //Load Interface 
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
		private void MenuStartClick(object sender, RoutedEventArgs e)
		{
			if (isStarted)
				return;

			show_packet.Text = "Start packet Capture.\n";
			isStarted = true;
			pm.OpenInterface();

			if (isPacketFilterSet)
				pm.StartCaputrePacket(packetFilter);

			else
				pm.StartCaputrePacket();
		}
		private void MenuSaveClick(object sender, RoutedEventArgs e) //Save Packet Capturd Data
		{
			if (isStarted)
			{
				MessageBox.Show("Please end packet capturing!", "Warining", MessageBoxButton.OK);
				return;
			}
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter= "Text file(*.txt) | *.txt";
			saveFileDialog.FileName= DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")+"-"+nowProtocol
				+" packet captured";

			if (saveFileDialog.ShowDialog() == true)
			{
				char[] sep = { '\n' };
				File.WriteAllLines(saveFileDialog.FileName,
					show_packet.Text.Split(sep));
			}

		}

		private void SetFilter(object sender, RoutedEventArgs e)
		{
			if (filter_edit.Text == "")
			{
				packetFilter = "";
				isPacketFilterSet = false;
			}
			else
			{
				isPacketFilterSet = true;
				packetFilter = filter_edit.Text;
			}
		}

		private void TCPSelected(object sender, RoutedEventArgs e) //Select Protocol(Processing Sub Menu)
		{
			if (isStarted)
			{
				MessageBox.Show("Please end packet capturing!", "Warining", MessageBoxButton.OK);
				return;
			}
			pm.PacketInterface = new TCPPacketInterface(pm.NowCatchingDevice);
			nowProtocol = "TCP";
		}

		private void IPSelected(object sender, RoutedEventArgs e) //Select Protocol(Processing Sub Menu)
		{
			if (isStarted)
			{
				MessageBox.Show("Please end packet capturing!", "Warining", MessageBoxButton.OK);
				return;
			}
			pm.PacketInterface = new IPPacketInterface(pm.NowCatchingDevice);
			nowProtocol = "IP";
		}

		private void UDPSelected(object sender, RoutedEventArgs e) //Select Protocol(Processing Sub Menu)
		{
			if (isStarted)
			{
				MessageBox.Show("Please end packet capturing!", "Warining", MessageBoxButton.OK);
				return;
			}
			pm.PacketInterface = new UDPPacketInterface(pm.NowCatchingDevice);
			nowProtocol = "UDP";
		}

		private void ARPSelected(object sender, RoutedEventArgs e) //Select Protocol(Processing Sub Menu)
		{
			if (isStarted)
			{
				MessageBox.Show("Please end packet capturing!", "Warining", MessageBoxButton.OK);
				return;
			}
			pm.PacketInterface = new ARPPacketInterface(pm.NowCatchingDevice);
			nowProtocol = "ARP";
		}
		private void MenuStopClick(object sender, RoutedEventArgs e) //Stop Packet Capturing
		{
			pm.CaptureStop();
			show_packet.Text+="Stop packet capture\n";
		}

		private void ChangeInterface(object sender, SelectionChangedEventArgs e) //OnSelectionChanged
		{
			if (isStarted)
			{
				MessageBox.Show("Please end packet capturing!", "Warining", MessageBoxButton.OK);
				return;
			}

			if (select_interface_combo.SelectedIndex >= 0)
			{
				selectedInterface = select_interface_combo.SelectedItem.ToString();
			}
			else
				MessageBox.Show("Wrong select.", "Warining", MessageBoxButton.OK);
		}
		private void CofirmInterface(object sender, RoutedEventArgs e) //Click Change Button
		{
			if (isStarted)
			{
				MessageBox.Show("Please end packet capturing!", "Warining", MessageBoxButton.OK);
				return;
			}

			var selectDev = from dev in captureDevices
								where dev.Name == selectedInterface
								select dev;

			pm.NowCatchingDevice = selectDev.ElementAt(0);
		}

		private void NumEditLoad(object sender, RoutedEventArgs e)
		{
			packet_num_edit.Text = ProcessingManeger.START_PACKET_NUM.ToString();
		}
		private void ChangePacketCaptureNum(object sender, RoutedEventArgs e) //Click Num Button
		{
			if (isStarted)
			{
				MessageBox.Show("Please end packet capturing!", "Warining", MessageBoxButton.OK);
				return;
			}

			try
			{
				int textNum = int.Parse(packet_num_edit.Text.ToString());

				if (textNum < 0 || textNum > 1024)
				{
					MessageBox.Show("Capture Length(1~1024)", "Warining", MessageBoxButton.OK);
					return;
				}

				pm.PacketCaptureNum = textNum;
			}
			catch (FormatException formatException)
			{
				Console.WriteLine(formatException.StackTrace);
				MessageBox.Show("Packet capture num is empty!", "Warining", MessageBoxButton.OK);
				return;
			}
		}

		private void EditPreviewInput(object sender, TextCompositionEventArgs e)
		{
			Regex regex = new Regex("[^0-9]+");
			e.Handled = regex.IsMatch(e.Text);
		}

		public void SetTextBlock()
		{
			Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
			 { show_packet.Text += pm.PacketInterface.ResultData; }));
		}

		public void CaptureEnd(){ isStarted = false; }
	}
}