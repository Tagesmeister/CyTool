   M S B A M L   `   `   `  ����
  CyTool    CyTool.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$$  CyTool - Cybersecurity Toolbox��5   	   $	��900��60   $	��900��6=    � WindowStartupLocation$ CenterScreen=�5   	    � WindowStyle$ SingleBorderWindow=�5   	   ���	#FF202124 � 	Resources 5         CyberButton �� 
TargetType �� Property �� Value �� 
TargetType �� CornerRadius	 J� SnapsToDevicePixels
 �� Orientation �� Triggers P� Property P� Value��%H  &	        �� 5	      " ��6   .�5
      ��  � �� �	#FF4285F46"   �� 5       � ��
 �����6"   �� 5       � ��$ 16(�6    �� 5       � ��$ SemiBold"�6"   �� 5       �@�Cursor$
 Hand��6   �� 5       � ��$ 8q�6   �� 5       � ��$ 	12,0,12,0q�6   �� 5       � ��$	 300��6   �� 5       � ��$ 80��6   �� 5       � ��$ 0q�6'   �� 5       � ��$
 Left=�62   �� 5       � �� 5      �� 5      " ��6   .R�5      �� #��z��6   $ 8��5   !   $
	 True��5   !   ._�5   	   �� $
 
Horizontal=�6   $
��Left=�5   )   $��Center=�5   )   $��10,0,0,0q�5   )   .+�5   
   �� $��28��6   $��28��6   $��0,0,10,0q�6'   �� 5    
   #��z��6   $��18(�5!   ,   
�������$��Center=�5#   ,   5$   
   5%   	    5&      P� 5'   	    �@J�IsMouseOver$
 True��6(   . �5(   
   ��  � �� �	#FF66A5FF6'   5)   
   P� 5*   	    � ��$
 True��6&   . �5+   
   ��  � �� �	#FF316AC56'   5,   
   5.      50      51      52      .��54      � ��55      �� 56       �� 
StartPoint$	 0,0&�6    �� EndPoint$	 1,1&�6*   .��57      �  � Color$ 	#FF202124��6    � Offset$	 0.0Q�6%   � 58      $ 	#FF15171A��6   $	 1.0Q�6%   59      .+�5<      �� ���	#FF2C2E33$ 12��5=       �� Padding$ 30q�5>      $��1q�5?      ���	#FF3A3B3C$��Center=�5A      $��Center=�5B       J� Effect 5C      3  -System.Windows.Media.Effects.DropShadowEffect  5D         Color$ Black��6      
BlurRadius$ 10Q�6%      ShadowDepth$ 2Q�65      Opacity$	 0.5Q�6E   ._�5F      �  � RowDefinitions 5G      �� 5H      $
��Auto �6   �� 5I      $��* �6   .+�5L      �� $)��#🔐 CyTool - Cybersecurity Toolbox��6   $��32(�5M      $
��Bold"�5N      
������� �� TextAlignment$ Center=�5P      $��Center=�5Q      $��10q�5R      �� 5T      $��1��6   $��Center=�5U      $��Center=�5V      $��20q�5W      .+�5X      �� $��📡 Analyse Network��6   #��[  6+   �� 5Y      $��	🚀 DDoX��6   #��[  6    �� 5Z      $��🛡️ PenTest Suite��6   #��[  6*   �� 5[      $��🔍 Scan Ports��6   #��[  6&   �� 5\      $��📊 Traffic Monitor��6   #��[  6+   5]      5^      5_      5`      5a      