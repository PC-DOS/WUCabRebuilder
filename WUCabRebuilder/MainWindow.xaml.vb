Imports System.IO
Imports System.Windows.Forms
Imports System.Windows.Window
Imports System.Xml
Class MainWindow
    Dim InputDirectory As String
    Dim OutputDiectory As String
    Dim EmptyList As New List(Of String)
    Dim MessageList As New List(Of String)
    Sub RefreshMessageList()
        lstMessage.ItemsSource = EmptyList
        lstMessage.ItemsSource = MessageList
        DoEvents()
    End Sub
    Sub AddMessage(MessageText As String)
        MessageList.Add(MessageText)
        RefreshMessageList()
        lstMessage.SelectedIndex = lstMessage.Items.Count - 1
        lstMessage.ScrollIntoView(lstMessage.SelectedItem)
    End Sub
    Sub LockUI()
        txtInputDir.IsEnabled = False
        txtOutputDir.IsEnabled = False
        btnBrowseInput.IsEnabled = False
        btnBrowseOutput.IsEnabled = False
        btnStart.IsEnabled = False
    End Sub
    Sub UnlockUI()
        txtInputDir.IsEnabled = True
        txtOutputDir.IsEnabled = True
        btnBrowseInput.IsEnabled = True
        btnBrowseOutput.IsEnabled = True
        btnStart.IsEnabled = True
    End Sub
    Private Sub SetTaskbarProgess(MaxValue As Integer, MinValue As Integer, CurrentValue As Integer, Optional State As Shell.TaskbarItemProgressState = Shell.TaskbarItemProgressState.Normal)
        If MaxValue <= MinValue Or CurrentValue < MinValue Or CurrentValue > MaxValue Then
            Exit Sub
        End If
        TaskbarItem.ProgressValue = (CurrentValue - MinValue) / (MaxValue - MinValue)
        TaskbarItem.ProgressState = State
    End Sub
    Function GetPathFromFile(FilePath As String) As String
        If FilePath.Trim = "" Then
            Return ""
        End If
        If FilePath(FilePath.Length - 1) = "\" Then
            Return FilePath
        End If
        Try
            Return FilePath.Substring(0, FilePath.LastIndexOf("\"))
        Catch ex As Exception
            Return ""
        End Try
    End Function
    Private Sub btnBrowseInput_Click(sender As Object, e As RoutedEventArgs) Handles btnBrowseInput.Click
        Dim FolderBrowser As New FolderBrowserDialog
        With FolderBrowser
            .Description = "请指定已解压缩的 Windows Update CAB 文件的位置，然后单击""确定""按钮。"
        End With
        If FolderBrowser.ShowDialog() = Forms.DialogResult.OK Then
            InputDirectory = FolderBrowser.SelectedPath
            If InputDirectory(InputDirectory.Length - 1) <> "\" Then
                InputDirectory = InputDirectory & "\"
            End If
            txtInputDir.Text = InputDirectory
            If Not File.Exists(InputDirectory & "update.mum") Then
                MessageBox.Show("目录结构重建所必须的文件""update.mum""不存在，请检查您选择的目录。", "关键文件丢失", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            End If
        End If
    End Sub

    Private Sub btnBrowseOutput_Click(sender As Object, e As RoutedEventArgs) Handles btnBrowseOutput.Click
        Dim FolderBrowser As New FolderBrowserDialog
        With FolderBrowser
            .Description = "请指定重建完成的目录结构要输出的位置，然后单击""确定""按钮。"
        End With
        If FolderBrowser.ShowDialog() = Forms.DialogResult.OK Then
            OutputDiectory = FolderBrowser.SelectedPath
            If OutputDiectory(OutputDiectory.Length - 1) <> "\" Then
                OutputDiectory = OutputDiectory & "\"
            End If
            txtOutputDir.Text = OutputDiectory
        End If
    End Sub

    Private Sub btnStart_Click(sender As Object, e As RoutedEventArgs) Handles btnStart.Click
        LockUI()
        If Not File.Exists(InputDirectory & "update.mum") Then
            MessageBox.Show("目录结构重建所必须的文件""update.mum""不存在，请检查您选择的目录。", "关键文件丢失", MessageBoxButtons.OK, MessageBoxIcon.Error)
            UnlockUI()
            Exit Sub
        End If
        If Not Directory.Exists(OutputDiectory) Then
            Try
                Directory.CreateDirectory(OutputDiectory)
            Catch ex As Exception
                MessageBox.Show("试图创建输出目录""" & OutputDiectory & """时发生错误: " & vbCrLf & ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error)
                UnlockUI()
                Exit Sub
            End Try
        End If
        With prgProgress
            .Minimum = 0
            .Maximum = 100
            .Value = 0
        End With
        MessageList.Clear()
        RefreshMessageList()
        Dim UpdateInfoFile As New XmlDocument
        AddMessage("正在打开包描述文件""" & InputDirectory & "update.mum""。")
        RefreshMessageList()
        Try
            UpdateInfoFile.Load(InputDirectory & "update.mum")
        Catch ex As Exception
            MessageBox.Show("无法打开包描述文件""" & InputDirectory & "update.mum""，发生错误: " & vbCrLf & ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error)
            AddMessage("无法打开包描述文件""" & InputDirectory & "update.mum""，发生错误: " & ex.Message)
            AddMessage("发生错误，取消操作。")
            UnlockUI()
            Exit Sub
        End Try
        AddMessage("成功打开包描述文件""" & InputDirectory & "update.mum""。")
        Dim nsMgr As New XmlNamespaceManager(UpdateInfoFile.NameTable)
        nsMgr.AddNamespace("ns", "urn:schemas-microsoft-com:asm.v3")
        Dim CustomInformationNode As XmlNode = UpdateInfoFile.SelectSingleNode("/ns:assembly/ns:package/ns:customInformation", nsMgr)
        AddMessage("正在定位 XML 节点""/assembly/package/customInformation""。")
        If IsNothing(CustomInformationNode) Then
            MessageBox.Show("XML 节点定位失败。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error)
            AddMessage("XML 节点定位失败。")
            AddMessage("发生错误，取消操作。")
            UnlockUI()
            Exit Sub
        End If
        AddMessage("XML 节点""assembly/package/customInformation""定位成功，共有 " & CustomInformationNode.ChildNodes.Count & " 条记录。")
        With prgProgress
            .Minimum = 0
            .Maximum = CustomInformationNode.ChildNodes.Count
            .Value = 0
        End With
        Dim TempFileInfo As New WindowsUpdatePackageFileNodeProperties
        Dim FileList As XmlNodeList = CustomInformationNode.ChildNodes
        Dim nSuccess As UInteger = 0
        Dim nFail As UInteger = 0
        Dim nIgnored As UInteger = 0
        For Each FileNode As XmlNode In FileList
            Dim FileElement As XmlElement = FileNode
            If FileElement.Name <> "file" Then
                AddMessage("已忽略一个节点，因为它的类型是""" & FileElement.Name & """而不是""file""。")
                nIgnored += 1
                prgProgress.Value += 1
                SetTaskbarProgess(prgProgress.Maximum, 0, prgProgress.Value)
                Continue For
            End If
            Try
                With TempFileInfo
                    .Name = FileElement.GetAttribute("name").ToString
                    .CabPath = FileElement.GetAttribute("cabpath").ToString
                End With
                If Not TempFileInfo.Name.StartsWith("$(runtime.system32)\") And Not TempFileInfo.Name.StartsWith("$(runtime.bootdrive)\") Then
                    AddMessage("已忽略一个文件节点，因为它没有描述文件复制信息。")
                    nIgnored += 1
                    prgProgress.Value += 1
                    SetTaskbarProgess(prgProgress.Maximum, 0, prgProgress.Value)
                    Continue For
                End If
                With TempFileInfo
                    If .Name.StartsWith("$(runtime.system32)\") Then
                        .Name = .Name.Replace("$(runtime.system32)\", OutputDiectory & "Windows\System32\")
                    End If
                    If .Name.StartsWith("$(runtime.bootdrive)\") Then
                        .Name = .Name.Replace("$(runtime.bootdrive)\", OutputDiectory)
                    End If
                    .CabPath = InputDirectory & .CabPath
                End With
                Dim CopyDest As String = GetPathFromFile(TempFileInfo.Name)
                If Not Directory.Exists(CopyDest) Then
                    Directory.CreateDirectory(CopyDest)
                End If
                If File.Exists(TempFileInfo.Name) Then
                    File.Delete(TempFileInfo.Name)
                End If
                File.Copy(TempFileInfo.CabPath, TempFileInfo.Name)
                AddMessage("已成功从""" & TempFileInfo.CabPath & """复制文件到""" & TempFileInfo.Name & """。")
                nSuccess += 1
                prgProgress.Value += 1
                SetTaskbarProgess(prgProgress.Maximum, 0, prgProgress.Value)
                DoEvents()
            Catch ex As Exception
                AddMessage("已忽略一个文件节点，因为发生错误: " & ex.Message)
                nFail += 1
                prgProgress.Value += 1
                SetTaskbarProgess(prgProgress.Maximum, 0, prgProgress.Value)
                Continue For
            End Try
        Next
        MessageBox.Show("操作完成，共有 " & nSuccess.ToString & "个文件被成功复制，有 " & nIgnored.ToString & " 个文件被忽略，处理 " & nFail.ToString & " 个文件时出错。", "大功告成!", MessageBoxButtons.OK, MessageBoxIcon.Information)
        UnlockUI()
        With prgProgress
            .Minimum = 0
            .Maximum = 100
            .Value = 0
        End With
        SetTaskbarProgess(100, 0, 0)
    End Sub
End Class
