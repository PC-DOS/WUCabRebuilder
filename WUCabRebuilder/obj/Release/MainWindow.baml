   M S B A M L   `   `   `  ����   WUCabRebuilder    WUCabRebuilder.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$1  +Windows Update CAB 目录结构重建工具��$	��400��$	��600��$	��525��$	��350�� � Icon$ Assets/Icon/Icon.ico��.��� $��10q� � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $��* ��� $
��Auto �.+�� $��0��.+��� $z��t本工具根据已解压缩的 Windows Update CAB 文件中 update.mum 文件的描述，重构系统目录结构。��� $��1��$��0,10,0,0q� �� $
��Auto ��� $
��Auto �.+��� $��0��$6��0已解压缩的 Windows Update CAB 文件路径:��� $��1��$��0,5,0,0q� � ColumnDefinitions �� $��* ��� $
��Auto �.+��� -    ��Name$ txtInputDir��$��0�� �� 
IsReadOnly .  �� IsReadOnlyCaretVisible . $��0,0,5,0q��� -    ��Name$ btnBrowseInput��$��1��$��浏览(_B)...��$��55��� $��2��$��0,5,0,0q� �� $
��Auto ��� $
��Auto �.+��� $��0��$"��重建的目录结构路径:��� $��1��$��0,5,0,0q� �� $��* ��� $
��Auto �.+��� -   $ txtOutputDir��$��0�� .  . $��0,0,5,0q��� -   $ btnBrowseOutput��$��1��$��浏览(_B)...��$��55��� $��3��$��0,10,0,0q� �� $
��Auto ��� $��* �.+��� -   $ btnStart��$��0��$��
开始(_S)��$��25��$��50���� -    ��Name$ 
lstMessage��$��4��$��0,10,0,0q�� -   	 �Name$	 prgProgress��$��5��
 � Minimum$
 0Q� � Maximum$	 100Q�$��10��$��0,10,0,0q� � TaskbarItemInfo *  $System.Windows.Shell.TaskbarItemInfo  -   TaskbarItem��