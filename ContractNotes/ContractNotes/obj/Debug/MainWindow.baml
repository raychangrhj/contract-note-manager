   M S B A M L   `   `   `  ����H  DContractNotes, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:ContractNotesContractNotes  5   	       ContractNotes.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -localclr-namespace:ContractNotes     5   	   -   5          � Title$  ContractNoteManager 1.0��5   	   $	��820��6)    � WindowStartupLocation$ CenterScreen=�66    � Icon$ res/contractNote.ico��6[   $
��1100��6�   $	��750��6�    � WindowState$ 	Maximized=�6�   $
��1100��6  .��5	      � .+�5
   
   �� -   5         ��Name$ 
scanButton��5
      $
��Left=�6%   $��	10,10,0,0q�6@   $	��Top=�6S   $��90��6�   $��30��6�   .��5      ��  �� Orientation$ 
Horizontal=�6   .+�5      �� -   5         ��Name$ 	scanImage��5      $��res/scan.png��6+   $��0q�6A   $��16��6L   $��20��6W   �� 5      -   5         ��Name$ scanTextBlock��5      $
��Scan��63   $��0q�6?   $��65��6J   $��20��6U    �� TextAlignment$ Center=�6a   ���	#FF0064C8$��14(�6�   $
��Bold"�6�   5      5      �� 5   
   -   5        $ approveButton��5      $
��Left=�6(   $��
105,10,0,0q�6C   $	��Top=�6W   $��90��6o   $��30��6z   .��5      �� $ 
Horizontal=�6   .+�5      �� -   5        $ approveImage��5      $��res/approve.png��6.   $��0q�6G   $��16��6R   $��20��6]   �� 5      -   5        $ approveTextBlock��5      $��Approve��66   $��0q�6E   $��65��6P   $��20��6[   $ Center=�6g   ���	#FF0064C8$��14(�6�   $
��Bold"�6�   5      5      �� 5   
   -   5        $ 
skipButton��5      $
��Left=�6%   $��
200,10,0,0q�6@   $	��Top=�6T   $��90��6l   $��30��6w   .��5      �� $ 
Horizontal=�6   .+�5      �� -	   5        $ 	skipImage��5      $��0q�6+   $��16��66   $��20��6A   $��res/skip.png��6M   �� 5      -
   5        $ skipTextBlock��5      $
��Skip��63   $��0q�6?   $��65��6J   $��20��6U   $ Center=�6a   ���	#FF0064C8$��14(�6�   $
��Bold"�6�   5      5      �� 5   
   -   5        $ exportButton��5      $
��Left=�6'   $��
295,10,0,0q�6B   $	��Top=�6V   $��90��6n   $��30��6y   .��5      �� $ 
Horizontal=�6   .+�5      �� -   5        $ exportImage��5      $��res/saveFile.png��6-   $��0q�6G   $��16��6R   $��20��6]   �� 5      -   5        $ exportTextBlock��5      $��Export��65   $��0q�6C   $��65��6N   $��20��6Y   $ Center=�6e   ���	#FF0064C8$��14(�6�   $
��Bold"�6�   5       5!      �� 5"   
   -   5        $ zoomInButton��5"      $
��Left=�6'   $��
390,10,0,0q�6B   $	��Top=�6V   $��90��6n   $��30��6y   .��5#      �� $ 
Horizontal=�6   .+�5$      �� -   5        $ zoomInImage��5$      $��res/zoomIn.png��6-   $��0q�6E   $��16��6P   $��20��6[   �� 5%      -   5        $ zoomInTextBlock��5%      $��ZoomIn��65   $��0q�6C   $��65��6N   $��20��6Y   $ Center=�6e   ���	#FF0064C8$��14(�6�   $
��Bold"�6�   5&      5'      �� 5(   
   -   5        $ zoomOutButton��5(      $
��Left=�6(   $��
485,10,0,0q�6C   $	��Top=�6W   $��90��6o   $��30��6z   .��5)      �� $ 
Horizontal=�6   .+�5*      �� -   5        $ zoomOutImage��5*      $��res/zoomOut.png��6.   $��0q�6G   $��16��6R   $��20��6]   �� 5+      -   5        $ zoomOutTextBlock��5+      $��ZoomOut��66   $��0q�6E   $��65��6P   $��20��6[   $ Center=�6g   ���	#FF0064C8$��14(�6�   $
��Bold"�6�   5,      5-      �� 5.   
   -   5        $ prevPageButton��5.      $
��Left=�6)   $��
580,10,0,0q�6D   $	��Top=�6X   $��90��6p   $��30��6{   .��5/      �� $ 
Horizontal=�6   .+�50      �� -   5        $ prevPageImage��50      $��res/prevPage.png��6/   $��0q�6I   $��16��6T   $��20��6_   �� 51      -   5        $ prevPageTextBlock��51      $��Previous��67   $��0q�6G   $��65��6R   $��20��6]   $ Center=�6i   ���	#FF0064C8$��14(�6�   $
��Bold"�6�   52      53      �� 54   
   -   5        $ nextPageButton��54      $
��Left=�6)   $��
675,10,0,0q�6D   $	��Top=�6X   $��90��6p   $��30��6{   .��55      �� $ 
Horizontal=�6   .+�56      �� -   5        $ nextPageImage��56      $��res/nextPage.png��6/   $��0q�6I   $��16��6T   $��20��6_   �� 57      -   5        $ nextPageTextBlock��57      $
��Next��67   $��0q�6C   $��65��6N   $��20��6Y   $ Center=�6e   ���	#FF0064C8$��14(�6�   $
��Bold"�6�   58      59      �� 5:   
   -   5        $ settingButton��5:      $
��Left=�6(   $��
770,10,0,0q�6C   $	��Top=�6W   $��90��6o   $��30��6z   .��5;      �� $ 
Horizontal=�6   .+�5<      �� -   5        $ settingImage��5<      $��res/settings.png��6.   $��0q�6H   $��16��6S   $��20��6^   �� 5=      -   5        $ settingTextBlock��5=      $��Setting��66   $��0q�6E   $��65��6P   $��20��6[   $ Center=�6g   ���	#FF0064C8$��14(�6�   $
��Bold"�6�   5>      5?      �� 5@   
   -   5        $ 
helpButton��5@      $
��Left=�6%   $��
865,10,0,0q�6@   $	��Top=�6T   $��90��6l   $��30��6w   .��5A      �� $ 
Horizontal=�6   .+�5B      �� -   5        $ 	helpImage��5B      $��res/help.png��6+   $��0q�6A   $��16��6L   $��20��6W   �� 5C      -   5        $ helpTextBlock��5C      $
��Help��63   $��0q�6?   $��65��6J   $��20��6U   $ Center=�6a   ���	#FF0064C8$��14(�6�   $
��Bold"�6�   5D      5E      �� 5F   
   -    5        $ 
quitButton��5F      $
��Left=�6%   $��
960,10,0,0q�6@   $	��Top=�6T   $��90��6l   $��30��6w   .��5G      �� $ 
Horizontal=�6   .+�5H      �� -!   5        $ 	quitImage��5H      $��res/quit.png��6+   $��0q�6A   $��16��6L   $��20��6W   �� 5I      -"   5        $ quitTextBlock��5I      $
��Quit��63   $��0q�6?   $��65��6J   $��20��6U   $ Center=�6a   ���	#FF0064C8$��14(�6�   $
��Bold"�6�   5J      5K      �� 5L   
   -#   5        	 ��Name$	 resultCanvasBorder��5L      
���   �$��1q�6A   $
��Left=�6U   $	��575��6p   $��	11,48,0,0q�6}   $	��Top=�6�   $	��382��6�   ._�5M      �� -$   5        
 ��Name$
 resultCanvas��5M      $
��Auto=�6+   $
��Auto=�6]   5N      �� 5O   
   -%   5         ��Name$ listBox��5O      $
��Left=�6#   $	��150��6>   $��
11,628,0,0q�6K   $	��Top=�6_   $	��382��6w   6�   �� 5P   
   -&   5        $	 scrollViewerBorder��5P      
���   �$��1q�6A   $
��Left=�6U   $	��730��6p   $��
398,48,0,0q�6}   $	��Top=�6�   $	��684��6�    �� Padding$ 0q�6�   ._�5Q      �� -'   5         ��Name$ scrollViewer��5Q      $
��Auto=�61   $
��Auto=�6V   .��5R      �� -(   5        $ image��5R      $
��None=�6'   5S      5T      5U      5V      