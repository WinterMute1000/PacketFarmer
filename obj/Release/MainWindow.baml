   M S B A M L   `   `   `  ���� G  CPacketFarmer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:PacketFarmerPacketFarmer      PacketFarmer.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     ,localclr-namespace:PacketFarmer       � Title$  PacketFarmer��$	��800��$
��1000��.���  � RowDefinitions �� $��3* ��� $��5* ��� $��5* ��� $��5* ��� $��5* ��� $	��45* � � ColumnDefinitions �� $	��25* ��� $	��25* ��� $	��25* ��� $	��25* �.+�o� $��0��$��4��.@�m� -   $��Start��m� -   $
��Save��m� $��SelectProtocol��.@�m� -   $	��TCP��m� -   $
��IPv4��m� -   $
��IPv6��m� -   $	��UDP��m� -   $	��ARP��m� -   $
��ICMP��m� -	   $
��Stop���� $��Select Interface��$��25(�$��1��$��0��$��Center=�$��Center=��� -
   $��select_interface_combo��$��2��$��3�� �� 
IsEditable .  �� -   $��confirm_change��$��2��$��3��$	��100��.��	Confirm�� $��3��$��0��$(��"Please input packet capture number��$��Center=�$��Center=�$��10(��� -   $��packet_num_edit��$��3��$��1���� -   $��confirm_number��$��3��$��2��$	��100��.��	Confirm�� $��4��$��0��$��Filter��$��Center=�$��Center=�$��20(��� -   $��filter_edit��$��4��$��1��$��2���� -   $��confirm_filter��$��4��$��3��$	��100��.��	Confirm�� $��5��$��4��$��Visible=�$
��Auto=�.���� .+��� -   $��show_packet��$
��Auto��$
��Auto�� �� TextAlignment$
 Left=�$��10(�