   M S B A M L   `   `   `  ����Y  UVisiomex.Projects.LabelingTool, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullO,clr-namespace:Visiomex.Projects.LabelingToolVisiomex.Projects.LabelingTool  5   	   T Phalcondotnet, Version=22.5.0.0, Culture=neutral, PublicKeyToken=4973bed59ddbf2b8A0clr-namespace:HalconDotNet;assembly=halcondotnetHalconDotNet 6D   /    )Visiomex.Projects.LabelingTool.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   >local,clr-namespace:Visiomex.Projects.LabelingTool     5   	   IHalcondotnet0clr-namespace:HalconDotNet;assembly=halcondotnet     6D   _ [MaterialDesignThemes.Wpf, Version=4.2.1.2, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  6�     � WindowState$  Normal=�5   	    � WindowStyle$
 None=�5	   	    � Title$ Etiketleme Aracı��5
   	   $
��1000��6"   $	��720��6/    � Icon$ Images\favicon.ico��6<   .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   6,   �� 5      $��* �6   6)   .+�5   
   � $��0��6    � ColumnDefinitions 5      �� 5      $
��1.1* �6#   �� 5      $��4* �6#   �� 5      $��40 �6#   .+�5      �� $��0��6   $
��Left=�6$   $��Bottom=�6?   $��
10 10 0 10q�6Z   ��5      �� 5       �� 	UriSource$ Images\logo.png?�6"   5      �� 5      $��1��6    �� Orientation$ 
Horizontal=�6)   $
��Left=�6B   $��Bottom=�6]   $��
10 10 0 10q�6x   .+�5      �� -   5         ��Name$ CreateNewProjectButton��5      $��Yeni Proje Oluştur��69   	 � ToolTip$	 Proje Oluştur��5           MaterialDesignOutlinedButton#��[  61   
�������
���   �$��5 0 0 0q�5!   @   $��30��5"      $	��150��6%   $��0q�62   5#      �� 5%      -   5        $ LoadProjectButton��5%      $��Proje Yükle��64   $	 Proje Yükle��5&      #��[  6/   
�������
���   �$��5 0 0 0q�5'   @   $��30��5(      $	��150��6%   $��0q�62   5)      5*      �� 5,      -   5        $ CloseApplicationButton��5,   %   $��2��6   $	 Kapat��6j   $	��Top=�6z   $��Right=�6�   $��30��6�   $��40��6�   ��5-      �� 5.      
 �� 
TargetType"
 ��6   .�5/      ��  �� Property � �� �� Value
 �����67   �� 50       � �� 51      �� 52   "    �� 
TargetType" ��62   .R�53   &   �� #��z��6-   ._�54   *   �� $��Center=�6;   $��Center=�6X   55   '   56   #   58       �� Triggers 59      P� 5:       P� Property �@J�IsMouseOver P� Value . . �5;   "   ��  � ��
 �  ��6?   5<      5>      .��5@      '  !MaterialDesignThemes.Wpf.PackIcon     Kind$ Close=�6*   
���   �$��25��6J   $��25��6V   6c   5A      5B      � 5D   
   $��1��6    5E      �� 5F      $��* �6#   6/   �� 5G      $
��Auto �6#   62   .+�5J      �� ���#C2000B$��3q�6+   $��10q�6?    �� CornerRadius$ 5��6K   ._�5K      �� 
���   �$��3q�6-   $ 5��6A   ._�5L      )  #HalconDotNet.HSmartWindowControlWPF  -   5          Name$ LabellingHWindow��5L   6   $��0��6)   � ContextMenu 5M      �� 5N      .@�5O   "   m� -   5        $��Etiket Çiz��5O   +   6^   5P      5R      5S      5T      � 5V      $��1��6   $��	0 10 0 10q�6#    5W      �� 5X      $
��Auto �6$   64   �� 5Y      $
��Auto �6$   64   �� 5Z      $
��Auto �6$   64   �� 5[      $��* �6$   61   .+�5^      �� $��0��6   $ 
Horizontal=�6*   .+�5_      �� $��Çizim Türü��6    $	��100��62   $��Comic Sans MS)�6>   $��15(�6Y   $��DemiBold"�6g    �� Padding$ 0 5 0 0q�6}   6�   �� 5`      $��:��6    $��Comic Sans MS)�6)   $��15(�6D   $��DemiBold"�6R   $ 0 5 0 0q�6h   6|   �� 5a      -   5         ��Name$ DrawTypeComboBox��5a      $	��100��69   $��	10 0 10 0q�6E   
���  ��$��Comic Sans MS)�6m   $��12(�6�   $��DemiBold"�6�   .@�5b      �� $��Serbest Çizim��6'   
���  ��$��Comic Sans MS)�6T   $��12(�6o   $��DemiBold"�6}   6�   �� 5c      $��Dikdörtgen Çizim��6'   
���  ��$��Comic Sans MS)�6W   $��12(�6r   $��DemiBold"�6�   6�   �� 5d      $��Daire Çizim��6'   
���  ��$��Comic Sans MS)�6R   $��12(�6m   $��DemiBold"�6{   6�   �� 5e      $��Elips Çizim��6'   
���  ��$��Comic Sans MS)�6R   $��12(�6m   $��DemiBold"�6{   6�   5f      5g      �� 5i      $��1��6   $ 
Horizontal=�6*   .+�5j      �� $��Etiket Türü��6    $	��100��63   $��Comic Sans MS)�6?   $��15(�6Z   $��DemiBold"�6h   $ 0 15 0 0q�6~   6�   �� 5k      $��:��6    $��Comic Sans MS)�6)   $��15(�6D   $��DemiBold"�6R   $ 0 15 0 0q�6h   6}   �� 5l      -   5        $ LabelTypeComboBox��5l      $	��100��6:   $��	10 0 10 0q�6F   
���  ��$��Comic Sans MS)�6n   $��12(�6�   $��DemiBold"�6�   6�   �� 5m      -   5        $ AddDeleteLabelTypeButton��5m      $��35��6?   $��25��6J   
���   �$��20(�6i   $��0 0 10 0q�6w   .��5n      �� $	��+/-��6$   $��14(�6/   $ 0 0 0 3q�6=   6Q   ��5o      �� 5p      " ��6.   .R�5q   "   � .+�5r   &   A� 
���   �$��1��6=   6T   �� 5s   &   $��Center=�67   $��Center=�6T   5t   #   5u      5w      5x      �� 5z      $��2��6   $��10 25 10 10q�6*   $ Vertical=�6?   $	��Top=�6V   .+�5{      �� $��ETIKETLENECEK RESIMLER��6    ���#C2000B$��Center=�6S   $��Comic Sans MS)�6p   $��15(�6�   $��DemiBold"�6�   $��0 0 0 20q�6�   6�   �� 5|      -	   5         ��Name$ DropFilesPathBorder��5|      $��3q�6;    J� Opacity$	 0.5Q�6O   $ 3��6]   $��30��6n    J� 	AllowDrop . ��5}      L� 5~       j� Viewport$ 0,0,5,5�6+    j� ViewportUnits$ Absolute=�6>    j� TileMode$
 Tile=�6W    L� Drawing 5   "   I� 5�   &   .��5�   *   �  � Brush
 �   � � Geometry 5�   .   � 5�   2   .��5�   6   ��   �� Rect$  	0,0,50,50�6H   �� 5�   6   $  50,50,50,50�6H   5�   3   5�   +   5�   '   5�      ._�5�      �� -
   5        $ addPhoto��5�   !   $��35��63   $��25��6>   
���   �$��20(�6]   $��	10 0 10 0q�6k   .��5�      �� $��+��6(   $��14(�61   $ 0 0 0 3q�6?   6S   ��5�      �� 5�   "   " ��62   .R�5�   &   � .+�5�   *   A� 
���   �$��1��6A   6W   �� 5�   *   $��Center=�6;   $��Center=�6X   5�   '   5�   #   5�      5�      5�      �� 5�      -   5        ! ��Name$! AllImagesListView��5�      $��3��66    . 6�   5�      5�      5�      5�      