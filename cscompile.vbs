If WScript.Arguments.Count = 0 Then
    MsgBox "CSファイルをこのVBSにドラッグ&ドロップしてください。"
    WScript.Quit
End If

Dim csFile, outputExe, cscPath

csFile = WScript.Arguments(0)

If LCase(Right(csFile, 3)) <> ".cs" Then
    MsgBox "CSファイルをドロップしてください。"
    WScript.Quit
End If

Dim fso
Set fso = CreateObject("Scripting.FileSystemObject")
outputExe = fso.GetParentFolderName(csFile) & "\" & fso.GetBaseName(csFile) & ".exe"

cscPath = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"

Dim shell
Set shell = CreateObject("WScript.Shell")

Dim cmd
cmd = """" & cscPath & """" & _
      " /r:System.Windows.Forms.dll /r:System.Drawing.dll" & _
      " /out:""" & outputExe & """" & _
      " """ & csFile & """"

Dim result
result = shell.Run("cmd /k """ & cmd & """", 1, True)

If result = 0 Then
    MsgBox "コンパイル成功！" & vbCrLf & outputExe
Else
    MsgBox "コンパイル失敗。エラーコード: " & result
End If

Set fso = Nothing
Set shell = Nothing
