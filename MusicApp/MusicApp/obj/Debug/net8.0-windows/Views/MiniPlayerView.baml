   M S B A M L   `   `   `  ����  MusicApp#    MusicApp.Views.MiniPlayerView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      $
��Auto��5      $
��Auto��6   .��5      � -   5        ���#222222  � ColumnDefinitions  5      �� 5	      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5      �� $��40��62   $��40��6=   $��5q�6I   $��Center=�6T   $��0��6o   ��6
   �� *CurrentSongThumbnail+ J� Clip 5      �� 5       �� Rect$ 	0,0,40,40�6    �� RadiusX$ 5Q�6)    �� RadiusY$ 5Q�65   5      �� 5       �� Orientation$ Vertical=�6   $��Center=�6&   $
��10,0q�6A   $��1��6O   .+�5      �� 
�������$
��Bold"�6C   ��6   �� *CurrentSongName+6C   �� 5      
���������6   �� *CurrentArtistName+62   5      �� 5      $ Vertical=�6   $��Stretch=�6&   $��Center=�6D   $��2��6_   $��	10,0,10,0q�6o   .+�5       �� $ 
Horizontal=�6   $��Center=�6)   $��Center=�6F   $��0,5,0,0q�6a   .+�5!      �� -   5        $��Previous��5!      $��75��6    ��6+   �� *PreviousCommand+6O   �� 5"      -   5        $��75��5"   /   $	��5,0q�6_   ��6   �� *PlayPauseText+��6:   �� *PlayPauseCommand+6l   �� 5#      -   5        $
��Next��5#      $��75��6   ��6'   �� *NextCommand+6G   5$      � 5%      $��Stretch=�6
   $��Center=�6(   $��	10,0,10,0q�6C     5&      �� 5'      $
��Auto �6   �� 5(      $��* �6   �� 5)      $
��Auto �6   .+�5+      �� $��50��6   
�������$��0��6J   ��6   �� *CurrentTime+6J   �� 5,      -   5        $��Stretch=�5-      $��1��53      $	��5,0q�54       � Value 5.      �� *SliderValue+ �� Mode$ TwoWay=� � Maximum 5/      �� *	TotalTime+$ OneWay=�54      �� 55      $��50��6   
�������$��2��6Q   ��6   �� *FormattedTotalTime+6Q   56      57      58      59      