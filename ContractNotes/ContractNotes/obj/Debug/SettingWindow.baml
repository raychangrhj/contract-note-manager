   M S B A M L   `   `   `  ����H  DContractNotes, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:ContractNotesContractNotes  5   	   O Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem 5   	   !    ContractNotes.SettingWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -localclr-namespace:ContractNotes     5   	   9System&clr-namespace:System;assembly=mscorlib     5   	   -   5          � Title$  Rule Setting��5	   	   $	��300��6   $	��300��6+    � WindowStartupLocation$ CenterScreen=�67    � Icon$ res/settings.png��6\   $	��850��6t   $	��500��6�   .��5
      � .+�5   
   &   System.Windows.Controls.DataGrid  -   5          Name$ dataGrid��5         
IsReadOnly . $
��Left=�67   $��
10,140,0,0q�6R   $	��Top=�6f   $	��300��6~   $	��810��6�      SelectionMode$ Single=�6�      Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   Header$ Instance��6%   1  +System.Windows.Controls.DataGridBoundColumn   Binding 67   �� *
Instance+  5      $ Source��6%    65   �� *Source+  5      $ Destination��6%    6:   �� *Destination+5      �� 5   
   -   5        	 ��Name$	 	addButton��5      $��Add Rule��6$   $
��Left=�67   $��	10,10,0,0q�6R   $	��Top=�6e   $	��100��6}   $��30��6�   6�   �� 5   
   -   5        $	 deleteButton��5      $��Delete Rule��6'   $
��Left=�6=   $��
120,10,0,0q�6X   $	��Top=�6l   $	��100��6�   $��30��6�   {�.  6�   �� 5   
   -   5        $	 
saveButton��5      $
��Save��6%   $
��Left=�64   $��
340,10,0,0q�6O   $	��Top=�6c   $	��100��6{   $��30��6�   {�.  6�   �� 5   
   -   5        $	 closeButton��5      $��Close��6&   $
��Left=�66   $��
450,10,0,0q�6Q   $	��Top=�6e   $	��100��6}   $��30��6�   6�   �� 5   
   $��Instance��6   $
��Left=�6#   $��	15,50,0,0q�6>   $	��Top=�6Q   $��90��6i   $��25��6t   $��Right=�6�   $��Center=�6�   �� 5   
   -   5        
 ��Name$
 instanceTextBox��5      $
��Left=�6+   $��25��6F   $��
110,50,0,0q�6R    �� TextWrapping$
 Wrap=�6f   $	��Top=�6z   $	��100��6�   $��Center=�6�   �� 5   
   $��Source��6   $
��Left=�6!   $��	15,80,0,0q�6<   $	��Top=�6O   $��90��6g   $��25��6r   $��Right=�6~   $��Center=�6�   �� 5   
   -   5        $
 sourceTextBox��5      $
��Left=�6)   $��25��6D   $��
110,80,0,0q�6P   $
 Wrap=�6d   $	��Top=�6x   $	��650��6�   $��Center=�6�    �� 
IsReadOnly . 6�   �� 5   
   -	   5        $	 browseSourceButton��5      $��. . .��6-   $
��Left=�6=   $��
760,80,0,0q�6X   $	��Top=�6l   $��25��6�   $��25��6�   6�   �� 5   
   $��Destination��6   $
��Left=�6&   $��
15,110,0,0q�6A   $	��Top=�6U   $��90��6m   $��25��6x   $��Right=�6�   $��Center=�6�   �� 5   
   -
   5        $
 destinationTextBox��5      $
��Left=�6.   $��25��6I   $��110,110,0,0q�6U   $
 Wrap=�6j   $	��Top=�6~   $	��650��6�   $��Center=�6�    . 6�   �� 5   
   -   5        $	 browseDestinationButton��5      $��. . .��62   $
��Left=�6B   $��760,110,0,0q�6]   $	��Top=�6r   $��25��6�   $��25��6�   6�   �� 5   
   -   5        $	 updateButton��5      $��Update Rule��6'   $
��Left=�6=   $��
230,10,0,0q�6X   $	��Top=�6l   $	��100��6�   $��30��6�   {�.  6�   5      5       