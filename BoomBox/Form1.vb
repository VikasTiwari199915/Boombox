'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
'''''''''''''''''''''''''''''''''''''Project:-BoomBox''''''''''''''''''''''''''''''''''''
'' Author :- Vikas Tiwari                                                              ''
'' Date Of Creation :-17th October 2018                                                ''
'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
Imports System.Drawing.Imaging
Imports System.IO
Imports Un4seen
Imports Un4seen.Bass
Imports Un4seen.Bass.AddOn.Sfx
Imports Un4seen.Bass.AddOn.Sfx.BassSfx

Public Class Form1
    Dim artwork As Bitmap
    Dim CommandFile As String
    Dim folder As String
    Dim fullFileName As String
    Dim spectrum As New Un4seen.Bass.Misc.Visuals
    Public albumArtImg As Bitmap = My.Resources.Music_icon
    Public albumart2 As Bitmap
    Public songFile As String
    Public artist As String
    Public stream As Integer
    Public last_Title As String
    Dim mouseOffset As Point
    Dim sf As Long
    Dim hSFX3 As Long
    Dim hSFX4 As Long
    Dim volpoint As Integer
    Dim selectedCustomPlugin As String

    Sub BlurBitmap(ByRef image As Bitmap, Optional ByVal BlurForce As Integer = 1)
        'We get a graphics object from the image
        Dim g As Graphics = Graphics.FromImage(image)
        'declare an ImageAttributes to use it when drawing
        Dim att As New ImageAttributes
        'declare a ColorMatrix
        Dim m As New ColorMatrix
        ' set Matrix33 to 0.5, which represents the opacity. so the drawing will be semi-trasparent.
        m.Matrix33 = 0.5F
        'Setting this ColorMatrix to the ImageAttributes.
        att.SetColorMatrix(m)
        'drawing the image on it self, but not in the same coordinates, in a way that every pixel will be drawn on the pixels arround it.
        For x = -BlurForce To BlurForce
            For y = -BlurForce To BlurForce
                'Drawing image on it self using out ImageAttributes to draw it semi-transparent.
                g.DrawImage(image, New Rectangle(x, y, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, att)
            Next
        Next
        'disposing ImageAttributes and Graphics. the effect is then applied. 
        att.Dispose()
        g.Dispose()
    End Sub
    Private Sub Me_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs) Handles MyBase.MouseDown
        mouseOffset = New Point(-e.X, -e.Y)
    End Sub
    Private Sub Me_MouseMove(ByVal sender As Object, ByVal e As MouseEventArgs) Handles MyBase.MouseMove
        If e.Button = MouseButtons.Left Then
            Dim mousePos = Control.MousePosition
            mousePos.Offset(mouseOffset.X, mouseOffset.Y)
            Location = mousePos
        End If
    End Sub
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        VolumeBar.Value = My.Settings.volume * 100
        TaskbarAssistant1.Assign(Me)
        BassNet.Registration("hesawisa@hotmail.com", "2X3123320312422")
        Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero)
        BassSfx.BASS_SFX_Init(System.Diagnostics.Process.GetCurrentProcess().Handle, Me.Handle)
        CommandFile = Command$()
        If Not CommandFile = String.Empty Then
            CommandFile = Replace(CommandFile, Chr(34), String.Empty)
            fullFileName = CommandFile
            folder = CommandFile.Remove(CommandFile.LastIndexOf("\"))
            updatePlaylist()

            'ListBox1.SelectedIndex = ListBox1.Items.IndexOf(fullFileName)
            Try
                stream = Bass.BASS_StreamCreateFile(CommandFile, 0, 0, BASSFlag.BASS_DEFAULT)
                Bass.BASS_ChannelPlay(stream, False)
                refresh_Tags()
                Timer1.Start()
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        Else

        End If

    End Sub
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PlayButton.Click
        PlayPause()
    End Sub
    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        Me.Close()
    End Sub
    Public Sub timertick(ByVal sender As Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        GTrackBar1.Enabled = True
        GTrackBar1.MaxValue = Bass.BASS_ChannelGetLength(stream)
        GTrackBar1.Value = Bass.BASS_ChannelGetPosition(stream)
        Try
            TaskbarAssistant1.ProgressMode = DevExpress.Utils.Taskbar.Core.TaskbarButtonProgressMode.Normal
            TaskbarAssistant1.ProgressMaximumValue = Bass.BASS_ChannelGetLength(stream)
            TaskbarAssistant1.ProgressCurrentValue = Bass.BASS_ChannelGetPosition(stream)
        Catch ex As Exception
        End Try

        Dim len As Long = Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetLength(stream))
        Dim pos As Long = Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetPosition(stream))
        If len = 0 And pos > len Then
            Songlength.Visible = False
            LiveLabel.Visible = True
        Else
            Songlength.Visible = True
            LiveLabel.Visible = False

        End If
        Songlength.Text = String.Format(" {0:#0.00}", Utils.FixTimespan(len, "MMSS"))
        SongPosition.Text = String.Format(" {0:#0.00}", Utils.FixTimespan(pos, "MMSS"))

        If Bass.BASS_ChannelIsActive(stream) = BASSActive.BASS_ACTIVE_STOPPED Then
            Try
                ListBox2.SelectedIndex = ListBox2.SelectedIndex + 1
                ListBox1.SelectedIndex = ListBox2.SelectedIndex
                Try
                    Bass.BASS_ChannelStop(stream)
                Catch ex As Exception

                End Try
                stream = Bass.BASS_StreamCreateFile(ListBox2.SelectedItem, 0, 0, BASSFlag.BASS_DEFAULT)
                Bass.BASS_ChannelPlay(stream, False)
                refresh_Tags()
                Timer1.Start()
            Catch ex As Exception
                Try
                    ListBox2.SelectedIndex = 0
                    Bass.BASS_ChannelPlay(stream, False)
                Catch ex2 As Exception

                End Try
            End Try
        ElseIf Bass.BASS_ChannelIsActive(stream) = BASSActive.BASS_ACTIVE_PAUSED Then
            Try
                TaskbarAssistant1.ProgressMode = DevExpress.Utils.Taskbar.Core.TaskbarButtonProgressMode.Paused
            Catch ex As Exception

            End Try

        ElseIf Bass.BASS_ChannelIsActive(stream) = BASSActive.BASS_ACTIVE_PLAYING Then
            Try
                TaskbarAssistant1.ProgressMode = DevExpress.Utils.Taskbar.Core.TaskbarButtonProgressMode.Normal
            Catch ex As Exception

            End Try


        End If
        'If sf <> -1 Then
        '    AddOn.Sfx.BassSfx.BASS_SFX_PluginRender(sf, stream, PictureBox1.Handle)
        'End If


    End Sub
    Private Sub GTrackBar1_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles GTrackBar1.MouseDown
        Timer1.Stop()
    End Sub
    Private Sub GTrackBar1_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles GTrackBar1.MouseUp
        'Bass.BASS_ChannelSetPosition(stream, GTrackBar1.Value)
        Timer1.Start()
    End Sub
    Function refresh_Tags()
        Try
            Dim TI As New Un4seen.Bass.AddOn.Tags.TAG_INFO()
            Un4seen.Bass.AddOn.Tags.BassTags.BASS_TAG_GetFromFile(stream, TI)
            Dim channelinf As Un4seen.Bass.BASS_CHANNELINFO = Bass.BASS_ChannelGetInfo(stream)

            'Select playing file in playlist
            Try
                songFile = channelinf.filename
                ListBox2.SelectedItem = songFile
                ListBox1.SelectedIndex = ListBox2.SelectedIndex
            Catch ex As Exception
            End Try

            If Not TI.title = "" Then
                Try
                    NotifyIcon1.BalloonTipText = TI.title
                    NotifyIcon1.ShowBalloonTip(2000)
                Catch ex As Exception

                End Try
            End If


            Try

                'last_Title = TI.title
                If Not TI.title = "" Then
                    Text = TI.title

                Else
                    Text = "BoomBox"
                    titlelabel.Text = "Title:Not Found"
                End If

                If TI.title.Length > 30 Then
                    LabelControl1.Text = TI.title.Substring(0, 30) + "..."
                    titlelabel.Text = TI.title
                Else
                    LabelControl1.Text = TI.title
                    titlelabel.Text = TI.title
                End If
            Catch ex As Exception
                LabelControl1.Text = "BoomBox"
                titlelabel.Text = "Title : Not Found"
                Text = "BoomBox"
            End Try
            Try
                If Not TI.album = "" Then
                    albumLabel.Text = TI.album
                Else
                    albumLabel.Text = "Album:Not Found"
                End If
            Catch ex As Exception
                albumLabel.Text = "No Data"
            End Try
            Try
                If Not TI.year = "" Then
                    YearLabel.Text = TI.year
                Else
                    YearLabel.Text = ""
                End If
            Catch ex As Exception
                YearLabel.Text = "No Data"
            End Try
            Try
                If Not TI.artist = String.Empty Then
                    ArtistLabel.Text = TI.artist
                Else
                    ArtistLabel.Text = "Artist:Not Found"
                End If
            Catch ex As Exception
                ArtistLabel.Text = "No Data"
            End Try


            Try
                Dim abort As System.Drawing.Image.GetThumbnailImageAbort
                PictureBox1.Image = TI.PictureGetImage(0).GetThumbnailImage(207, 192, abort, IntPtr.Zero)
                artwork = TI.PictureGetImage(0)
                albumArtImg = TI.PictureGetImage(0).GetThumbnailImage(207, 192, abort, IntPtr.Zero)
                Me.BackgroundImage = TI.PictureGetImage(0).GetThumbnailImage(207, 192, abort, IntPtr.Zero)
                Try
                    BlurBitmap(Me.BackgroundImage, 3)
                Catch ex As Exception
                    Me.BackgroundImage = My.Resources.blurred_background_1034_792
                End Try
            Catch ex As Exception
                Me.BackgroundImage = My.Resources.blurred_background_1034_792
                PictureBox1.Image = My.Resources.Music_icon
                artwork = My.Resources.Music_icon
                albumArtImg = My.Resources.Music_icon
            End Try

            LabelControl1.Text = "[" & ListBox1.SelectedIndex + 1 & "/" & ListBox1.Items.Count & "] " & TI.title
        Catch ex As Exception

        End Try


    End Function
    Function updatePlaylist()
        Dim a = My.Computer.FileSystem.GetDirectoryInfo(folder)
        Dim b = a.GetFiles("*.mp3", SearchOption.TopDirectoryOnly).Length
        Dim c = a.GetFiles("*.wma", SearchOption.TopDirectoryOnly).Length
        GTrackBar2.MaxValue = b + c
        GTrackBar2.Value = 0
        For Each files In My.Computer.FileSystem.GetFiles(folder, FileIO.SearchOption.SearchTopLevelOnly, "*.mp3")

            Label1.Visible = True
            GTrackBar2.Visible = True

            Try
                GTrackBar2.Value = GTrackBar2.Value + 1
                ListBox1.Items.Add(files.Remove(0, files.LastIndexOf("\") + 1))
                ListBox2.Items.Add(files)
                Label1.Text = "Loading : " & files.ToString
                'System.Threading.Thread.Sleep(10)
                GTrackBar2.Refresh()
                Label1.Refresh()
            Catch ex As Exception

            End Try
        Next
        For Each files2 In My.Computer.FileSystem.GetFiles(folder, FileIO.SearchOption.SearchTopLevelOnly, "*.wma")

            Label1.Visible = True
            GTrackBar2.Visible = True

            Try
                GTrackBar2.Value = GTrackBar2.Value + 1
                ListBox1.Items.Add(files2.Remove(0, files2.LastIndexOf("\") + 1))
                ListBox2.Items.Add(files2)
                Label1.Text = "Loading : " & files2.ToString
                'System.Threading.Thread.Sleep(10)
                GTrackBar2.Refresh()
                Label1.Refresh()
            Catch ex As Exception

            End Try
        Next
        Label1.Text = "Loading Complete !!"
        Label1.Refresh()
        Threading.Thread.Sleep(500)
        GTrackBar2.Visible = False
        Label1.Visible = False

        Return (0)
    End Function
    Private Sub GTrackBar1_Scroll(ByVal sender As Object, ByVal e As System.Windows.Forms.ScrollEventArgs) Handles GTrackBar1.Scroll
        If Not Bass.BASS_ChannelIsActive(stream) = BASSActive.BASS_ACTIVE_STOPPED Then
            Bass.BASS_ChannelSetPosition(stream, GTrackBar1.Value)
        End If
    End Sub
    Private Sub Button6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button6.Click
        FlyoutPanel1.ShowBeakForm()
        ListBox1.Focus()
    End Sub
    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        NextSong()
    End Sub
    Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click
        PreSong()
    End Sub
    Private Sub Form1_MouseHover(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.MouseHover

    End Sub
    Private Sub Form1_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.MouseLeave, Button6.MouseLeave, Button4.MouseLeave, MyBase.Leave, Button2.Leave
        GTrackBar2.Focus()
    End Sub
    Private Sub ListBox1_MouseDoubleClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles ListBox1.MouseDoubleClick
        ListBox2.SelectedIndex = ListBox1.SelectedIndex
        Try
            Try
                Bass.BASS_ChannelStop(stream)
            Catch ex As Exception

            End Try
            stream = Bass.BASS_StreamCreateFile(ListBox2.SelectedItem, 0, 0, BASSFlag.BASS_DEFAULT)
            Bass.BASS_ChannelPlay(stream, False)
            refresh_Tags()
            Timer1.Start()
        Catch ex As Exception
            Try
                ListBox2.SelectedIndex = 0
                Bass.BASS_ChannelPlay(stream, False)
                refresh_Tags()
            Catch ex2 As Exception

            End Try
        End Try

    End Sub
    Private Sub ListBox2_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListBox2.SelectedIndexChanged
        ListBox1.SelectedIndex = ListBox2.SelectedIndex
    End Sub
    Private Sub ExitToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitToolStripMenuItem.Click
        Me.Close()
    End Sub
    Private Sub PlaylistToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PlaylistToolStripMenuItem.Click
        FlyoutPanel1.ShowPopup()
    End Sub
    Private Sub GoBackToPlayerToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles GoBackToPlayerToolStripMenuItem.Click
        FlyoutPanel1.HideBeakForm()
        FlyoutPanel1.HidePopup()

    End Sub
    Private Sub Button1_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs)
        FlyoutPanel2.ShowPopup()

    End Sub
    Private Sub ShowPlayerToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ShowPlayerToolStripMenuItem.Click, NotifyIcon1.BalloonTipClicked
        Me.Show()
    End Sub
    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        '#########Me.Hide()
        If Height = 461 Then
            Size = New Size(280, 80)
            VolumeBar.Visible = False
            LabelControl1.Visible = False
            PictureBox1.Location = New Point(0, 0)
            PictureBox1.Size = New Size(80, 80)

            Button5.Size = New Size(40, 40)
            Button5.Location = New Point(101, 40)

            PlayButton.Size = New Size(40, 40)
            PlayButton.Location = New Point(141, 40)

            Button2.Size = New Size(40, 40)
            Button2.Location = New Point(181, 40)

            Button6.Size = New Size(40, 40)
            Button6.Location = New Point(240, 40)

            GTrackBar1.Size = New Size(200, 23)
            GTrackBar1.Location = New Point(81, 17)

            titlelabel.Location = New Point(81, 0)
            titlelabel.Size = New Size(155, 17)

            Button3.Location = New Point(235, 0)
            Button4.Location = New Point(263, 0)

        Else
            Size = New Size(275, 461)
            VolumeBar.Visible = True
            LabelControl1.Visible = True
            PictureBox1.Location = New Point(34, 52)
            PictureBox1.Size = New Size(207, 192)

            Button5.Size = New Size(68, 72)
            Button5.Location = New Point(0, 388)

            PlayButton.Size = New Size(77, 79)
            PlayButton.Location = New Point(99, 385)

            Button2.Size = New Size(68, 72)
            Button2.Location = New Point(206, 388)

            Button6.Size = New Size(31, 28)
            Button6.Location = New Point(241, 216)

            GTrackBar1.Size = New Size(286, 23)
            GTrackBar1.Location = New Point(-6, 365)

            titlelabel.Location = New Point(0, 247)
            titlelabel.Size = New Size(273, 13)

            Button3.Location = New Point(222, 1)
            Button4.Location = New Point(256, 1)


        End If


    End Sub
    Public Function NextSong()
        Try
            ListBox2.SelectedIndex = ListBox2.SelectedIndex + 1
            ListBox1.SelectedIndex = ListBox2.SelectedIndex
            Try
                Bass.BASS_ChannelStop(stream)
            Catch ex As Exception

            End Try
            stream = Bass.BASS_StreamCreateFile(ListBox2.SelectedItem, 0, 0, BASSFlag.BASS_DEFAULT)
            Bass.BASS_ChannelPlay(stream, False)
            refresh_Tags()
            Timer1.Start()
        Catch ex2 As Exception


            ListBox2.SelectedIndex = 0
            ListBox1.SelectedIndex = 0
            Try
                Bass.BASS_ChannelStop(stream)
            Catch ex3 As Exception

            End Try
            stream = Bass.BASS_StreamCreateFile(ListBox2.SelectedItem, 0, 0, BASSFlag.BASS_DEFAULT)
            Bass.BASS_ChannelPlay(stream, False)
            refresh_Tags()
            Timer1.Start()

        End Try
    End Function
    Public Function PreSong()
        Try
            ListBox2.SelectedIndex = ListBox2.SelectedIndex - 1
            ListBox1.SelectedIndex = ListBox2.SelectedIndex
            Try
                Bass.BASS_ChannelStop(stream)
            Catch ex As Exception

            End Try
            stream = Bass.BASS_StreamCreateFile(ListBox2.SelectedItem, 0, 0, BASSFlag.BASS_DEFAULT)
            Bass.BASS_ChannelPlay(stream, False)
            refresh_Tags()
            Timer1.Start()
        Catch ex As Exception
            Try
                ListBox2.SelectedIndex = ListBox2.Items.Count - 1
                ListBox1.SelectedIndex = ListBox2.SelectedIndex
                Try
                    Bass.BASS_ChannelStop(stream)
                Catch ex1 As Exception

                End Try
                stream = Bass.BASS_StreamCreateFile(ListBox2.SelectedItem, 0, 0, BASSFlag.BASS_DEFAULT)
                Bass.BASS_ChannelPlay(stream, False)
                refresh_Tags()
                Timer1.Start()
            Catch ex2 As Exception

            End Try
        End Try

    End Function
    Private Sub NextSongToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NextSongToolStripMenuItem.Click
        NextSong()
    End Sub
    Private Sub PreviousSongToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PreviousSongToolStripMenuItem.Click
        PreSong()
    End Sub
    Private Sub ClosePlayerToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ClosePlayerToolStripMenuItem.Click
        Close()
    End Sub
    Private Sub Timer2_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer2.Tick
        If WhatsappToolStripMenuItem.Checked = True Then
            PictureBox1.Image = spectrum.CreateWaveForm(stream, PictureBox1.Width, PictureBox1.Height, Color.White, Color.Empty, Color.Empty, Color.Transparent, 1, True, True, True)
        ElseIf BarToolStripMenuItem.Checked = True Then
            PictureBox1.Image = spectrum.CreateSpectrum(stream, PictureBox1.Width, PictureBox1.Height, Color.Black, Color.White, Color.Empty, False, False, False)
        ElseIf LineToolStripMenuItem.Checked = True Then
            PictureBox1.Image = spectrum.CreateSpectrumLine(stream, PictureBox1.Width, PictureBox1.Height, Color.White, Color.Black, Color.Empty, 8, 1, True, False, True)
        ElseIf PeakToolStripMenuItem.Checked = True Then
            PictureBox1.Image = spectrum.CreateSpectrumLinePeak(stream, PictureBox1.Width, PictureBox1.Height, Color.Black, Color.Black, Color.White, Color.Empty, 8, 1, 1, 50, True, False, True)
        ElseIf CustomPluginToolStripMenuItem.Checked = True Then
            If (hSFX3 <> -1) Then
                Dim g As Graphics = Graphics.FromHwnd(PictureBox1.Handle)
                BASS_SFX_PluginRender(hSFX3, stream, g.GetHdc)
                g.Dispose()
            End If
        ElseIf WindowsMediaPlayerToolStripMenuItem.Checked = True Then
            If (hSFX4 <> -1) Then
                Dim g As Graphics = Graphics.FromHwnd(PictureBox1.Handle)
                BASS_SFX_PluginRender(hSFX4, stream, g.GetHdc)
                g.Dispose()

            End If
        End If

    End Sub
    Private Sub WhatsappToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles WhatsappToolStripMenuItem.Click
        AlbumArtToolStripMenuItem.Checked = False
        BarToolStripMenuItem.Checked = False
        LineToolStripMenuItem.Checked = False
        PeakToolStripMenuItem.Checked = False
        CustomPluginToolStripMenuItem.Checked = False
        WhatsappToolStripMenuItem.Checked = True
        WindowsMediaPlayerToolStripMenuItem.Checked = False
        BASS_SFX_PluginStop(hSFX3)
        BASS_SFX_PluginStop(hSFX4)
        Timer2.Start()
    End Sub
    Private Sub BarToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BarToolStripMenuItem.Click
        AlbumArtToolStripMenuItem.Checked = False
        BarToolStripMenuItem.Checked = True
        LineToolStripMenuItem.Checked = False
        PeakToolStripMenuItem.Checked = False
        CustomPluginToolStripMenuItem.Checked = False
        WhatsappToolStripMenuItem.Checked = False
        WindowsMediaPlayerToolStripMenuItem.Checked = False
        BASS_SFX_PluginStop(hSFX3)
        BASS_SFX_PluginStop(hSFX4)
        Timer2.Start()
    End Sub
    Private Sub LineToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LineToolStripMenuItem.Click
        AlbumArtToolStripMenuItem.Checked = False
        BarToolStripMenuItem.Checked = False
        LineToolStripMenuItem.Checked = True
        PeakToolStripMenuItem.Checked = False
        CustomPluginToolStripMenuItem.Checked = False
        WhatsappToolStripMenuItem.Checked = False
        WindowsMediaPlayerToolStripMenuItem.Checked = False
        BASS_SFX_PluginStop(hSFX3)
        BASS_SFX_PluginStop(hSFX4)
        Timer2.Start()
    End Sub
    Private Sub PeakToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PeakToolStripMenuItem.Click
        AlbumArtToolStripMenuItem.Checked = False
        BarToolStripMenuItem.Checked = False
        LineToolStripMenuItem.Checked = False
        PeakToolStripMenuItem.Checked = True
        CustomPluginToolStripMenuItem.Checked = False
        WhatsappToolStripMenuItem.Checked = False
        WindowsMediaPlayerToolStripMenuItem.Checked = False
        BASS_SFX_PluginStop(hSFX3)
        BASS_SFX_PluginStop(hSFX4)
        Timer2.Start()
    End Sub
    Private Sub AlbumArtToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AlbumArtToolStripMenuItem.Click
        AlbumArtToolStripMenuItem.Checked = True
        BarToolStripMenuItem.Checked = False
        LineToolStripMenuItem.Checked = False
        PeakToolStripMenuItem.Checked = False
        CustomPluginToolStripMenuItem.Checked = False
        WhatsappToolStripMenuItem.Checked = False
        WindowsMediaPlayerToolStripMenuItem.Checked = False
        BASS_SFX_PluginStop(hSFX3)
        BASS_SFX_PluginStop(hSFX4)
        Timer2.Stop()
        PictureBox1.Image = albumArtImg
    End Sub
    Private Sub CustomPluginToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CustomPluginToolStripMenuItem.Click
        Timer2.Stop()
        If OpenFileDialog2.ShowDialog = Windows.Forms.DialogResult.OK Then

            BASS_SFX_PluginStop(hSFX3)
            selectedCustomPlugin = OpenFileDialog2.FileName
            hSFX3 = BASS_SFX_PluginCreate(OpenFileDialog2.FileName, PictureBox1.Handle, PictureBox1.Width, PictureBox1.Height, BASSSFXFlag.BASS_SFX_DEFAULT)
            BASS_SFX_PluginSetStream(hSFX3, stream)
            BASS_SFX_PluginStart(hSFX3)
            Timer2.Start()
            AlbumArtToolStripMenuItem.Checked = False
            BarToolStripMenuItem.Checked = False
            LineToolStripMenuItem.Checked = False
            PeakToolStripMenuItem.Checked = False
            CustomPluginToolStripMenuItem.Checked = True
            WindowsMediaPlayerToolStripMenuItem.Checked = False
            WhatsappToolStripMenuItem.Checked = False
        Else
            Timer2.Start()
        End If
    End Sub
    Private Sub Button7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Timer1.Stop()

        Bass.BASS_ChannelStop(stream)
        PlayButton.BackgroundImage = My.Resources.Media_Controls_Play_icon
        Bass.BASS_ChannelSetPosition(stream, 0)
        GTrackBar1.Value = 0
        GTrackBar1.Refresh()
        spectrum.ClearPeaks()
        Timer2.Stop()
    End Sub
    Private Sub WindowsMediaPlayerToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles WindowsMediaPlayerToolStripMenuItem.Click
        Timer2.Stop()
        BASS_SFX_PluginStop(hSFX4)
        hSFX4 = BASS_SFX_PluginCreate("0AA02E8D-F851-4CB0-9F64-BBA9BE7A983D", PictureBox1.Handle, PictureBox1.Width, PictureBox1.Height, BASSSFXFlag.BASS_SFX_DEFAULT)
        BASS_SFX_PluginSetStream(hSFX4, stream)
        BASS_SFX_PluginStart(hSFX4)
        Timer2.Start()
        AlbumArtToolStripMenuItem.Checked = False
        BarToolStripMenuItem.Checked = False
        LineToolStripMenuItem.Checked = False
        PeakToolStripMenuItem.Checked = False
        CustomPluginToolStripMenuItem.Checked = False
        WhatsappToolStripMenuItem.Checked = False
        WindowsMediaPlayerToolStripMenuItem.Checked = True
    End Sub

    '_____________Screen Edge Snapping....(Beta)_______________
    Private Sub Form1_LocationChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.LocationChanged
        If Height = 80 Then
            If My.Computer.Screen.Bounds.Right - (Location.X + 275) < 30 And Location.Y < 30 Then
                Location = New System.Drawing.Point(My.Computer.Screen.Bounds.Right - 275, 0)


            ElseIf My.Computer.Screen.Bounds.Right - (Location.X + 275) < 30 Then
                Location = New System.Drawing.Point(My.Computer.Screen.Bounds.Right - 275, Location.Y)
            ElseIf Location.X < 30 Then
                Location = New System.Drawing.Point(0, Location.Y)
            ElseIf Location.Y < 30 Then
                Location = New System.Drawing.Point(Location.X, 0)
            ElseIf My.Computer.Screen.Bounds.Bottom - (Location.Y + 80) < 30 Then
                Location = New System.Drawing.Point(Location.X, My.Computer.Screen.Bounds.Bottom - 80)
            End If
        End If
    End Sub
    Private Sub Form1_KeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles MyBase.KeyDown, VolumeBar.KeyDown, PlayButton.KeyDown, GTrackBar2.KeyDown, GTrackBar1.KeyDown, Button6.KeyDown, Button5.KeyDown, Button3.KeyDown, Button2.KeyDown
        If e.KeyCode = Keys.MediaPlayPause Then
            PlayPause()
        ElseIf e.KeyCode = Keys.Space Then
            PlayPause()
        ElseIf e.KeyCode = Keys.PageDown Then
            NextSong()
        ElseIf e.KeyCode = Keys.PageUp Then
            PreSong()
        ElseIf e.KeyCode = Keys.MediaNextTrack Then
            NextSong()
        ElseIf e.KeyCode = Keys.MediaPreviousTrack Then
            PreSong()
        ElseIf e.KeyCode = Keys.MediaPlayPause Then
            PlayPause()
        End If
    End Sub
    Function PlayPause()
        If stream <> 0 Then
            If Bass.BASS_ChannelIsActive(stream) = BASSActive.BASS_ACTIVE_PLAYING Then
                Bass.BASS_ChannelPause(stream)
                ThumbnailButton2.Image = My.Resources.Media_Controls_Play_icon
            Else
                Bass.BASS_ChannelPlay(stream, False)
                ThumbnailButton2.Image = My.Resources.Media_Controls_Pause_icon
                Timer1.Start()
                Timer2.Start()
            End If
        Else
            OpenFileDialog1.FileName = ""
            OpenFileDialog1.Filter = "Mp3 Files|*.mp3|All Files|*.*"
            If OpenFileDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
                fullFileName = OpenFileDialog1.FileName
                folder = OpenFileDialog1.FileName.Remove(OpenFileDialog1.FileName.LastIndexOf("\"))
                updatePlaylist()
                ListBox1.SelectedIndex = ListBox1.Items.IndexOf(fullFileName)
                ListBox2.SelectedIndex = ListBox1.SelectedIndex
                stream = Bass.BASS_StreamCreateFile(OpenFileDialog1.FileName, 0, 0, BASSFlag.BASS_DEFAULT)
                Bass.BASS_ChannelPlay(stream, False)
                refresh_Tags()
                Timer1.Start()
            End If
        End If
    End Function
    Private Sub OpenUrlToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OpenUrlToolStripMenuItem.Click
        Open_Link.Show()
    End Sub
    Public Function playlink(ByVal link As String)
        Try
            Timer1.Stop()
            Bass.BASS_ChannelStop(stream)
            stream = Bass.BASS_StreamCreateURL(link, 0, BASSFlag.BASS_DEFAULT, Nothing, IntPtr.Zero)
            Bass.BASS_ChannelPlay(stream, False)
            Timer1.Start()
        Catch ex As Exception
            MsgBox("Provided Link Could Not Be Played!!")
            ' Timer1.Start()
        End Try


    End Function

    Private Sub PlayButton_MouseHover(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PlayButton.MouseHover
        If Bass.BASS_ChannelIsActive(stream) = BASSActive.BASS_ACTIVE_PLAYING Then
            PlayButton.BackgroundImage = My.Resources.pause_focused1
        Else
            PlayButton.BackgroundImage = My.Resources.play_focused1
        End If
        GTrackBar2.Focus()
    End Sub

    Private Sub PlayButton_MouseLeave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PlayButton.MouseLeave
        If Bass.BASS_ChannelIsActive(stream) = BASSActive.BASS_ACTIVE_PLAYING Then
            PlayButton.BackgroundImage = My.Resources.pause_normal
        Else
            PlayButton.BackgroundImage = My.Resources.play_normal
        End If
        GTrackBar2.Focus()
    End Sub

    Private Sub PlayButton_MouseDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles PlayButton.MouseDown
        If Bass.BASS_ChannelIsActive(stream) = BASSActive.BASS_ACTIVE_PLAYING Then
            PlayButton.BackgroundImage = My.Resources.pause_pressed
        Else
            PlayButton.BackgroundImage = My.Resources.play_pressed
        End If
        GTrackBar2.Focus()
    End Sub

    Private Sub PlayButton_MouseUp(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles PlayButton.MouseUp
        If Bass.BASS_ChannelIsActive(stream) = BASSActive.BASS_ACTIVE_PLAYING Then
            PlayButton.BackgroundImage = My.Resources.pause_normal
        Else
            PlayButton.BackgroundImage = My.Resources.play_normal
        End If
        GTrackBar2.Focus()
    End Sub

    Private Sub Button2_MouseHover(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.MouseHover
        Button2.BackgroundImage = My.Resources.next_focused1
        GTrackBar2.Focus()
    End Sub

    Private Sub Button2_MouseLeave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.MouseLeave
        Button2.BackgroundImage = My.Resources.next_normal
        GTrackBar2.Focus()
    End Sub

    Private Sub Button2_MouseDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Button2.MouseDown
        Button2.BackgroundImage = My.Resources.next_pressed
        GTrackBar2.Focus()
    End Sub

    Private Sub Button2_MouseUp(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Button2.MouseUp
        Button2.BackgroundImage = My.Resources.next_normal
        GTrackBar2.Focus()
    End Sub

    Private Sub Button5_MouseHover(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.MouseHover
        Button5.BackgroundImage = My.Resources.previous_pressed
        GTrackBar2.Focus()
    End Sub

    Private Sub Button5_MouseUp(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Button5.MouseUp
        Button5.BackgroundImage = My.Resources.previous_normal
        GTrackBar2.Focus()
    End Sub

    Private Sub Button5_MouseLeave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.MouseLeave
        Button5.BackgroundImage = My.Resources.previous_normal
        GTrackBar2.Focus()
    End Sub

    Private Sub Button5_MouseDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Button5.MouseDown
        Button5.BackgroundImage = My.Resources.previous_focused
        GTrackBar2.Focus()
    End Sub

    Private Sub VolumeBar_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles VolumeBar.MouseMove
        If e.Button.Left Then
            Bass.BASS_SetVolume(VolumeBar.Value / 100)
            My.Settings.volume = VolumeBar.Value / 100
        End If
    End Sub
    Private Sub VolumeBar_MouseUp(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles VolumeBar.MouseUp

    End Sub

    Private Sub SaveAlbumArtAsPictureToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SaveAlbumArtAsPictureToolStripMenuItem.Click
        SaveFileDialog1.FileName = titlelabel.Text + ".jpg"
        If SaveFileDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
            artwork.Save(SaveFileDialog1.FileName)
        End If
    End Sub

    Private Sub ThumbnailButton3_Click(ByVal sender As System.Object, ByVal e As DevExpress.Utils.Taskbar.ThumbButtonClickEventArgs) Handles ThumbnailButton3.Click
        PreSong()
    End Sub

    Private Sub ThumbnailButton2_Click(ByVal sender As System.Object, ByVal e As DevExpress.Utils.Taskbar.ThumbButtonClickEventArgs) Handles ThumbnailButton2.Click
        PlayPause()
    End Sub

    Private Sub ThumbnailButton1_Click(ByVal sender As System.Object, ByVal e As DevExpress.Utils.Taskbar.ThumbButtonClickEventArgs) Handles ThumbnailButton1.Click
        NextSong()
    End Sub

    Private Sub BarButtonItem5_ItemClick(ByVal sender As System.Object, ByVal e As DevExpress.XtraBars.ItemClickEventArgs)
        Me.Close()
    End Sub

    Private Sub BarButtonItem4_ItemClick(ByVal sender As System.Object, ByVal e As DevExpress.XtraBars.ItemClickEventArgs)
        Me.Show()
    End Sub

    Private Sub BarLargeButtonItem1_ItemClick(ByVal sender As System.Object, ByVal e As DevExpress.XtraBars.ItemClickEventArgs)
        PreSong()
    End Sub

    Private Sub BarButtonItem1_ItemClick(ByVal sender As System.Object, ByVal e As DevExpress.XtraBars.ItemClickEventArgs)
        NextSong()
    End Sub
    Private Sub AboutMeToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutMeToolStripMenuItem.Click
        Dialog1.Show()
    End Sub

    Private Sub FilesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles FilesToolStripMenuItem.Click, AddSongsToolStripMenuItem.Click
        OpenFileDialog1.Multiselect = True
        OpenFileDialog1.Title = "Select Files To Add.."
        OpenFileDialog1.Filter = "Mp3 Files|*.mp3|All Files|*.*"
        If OpenFileDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
            For Each song In OpenFileDialog1.FileNames
                ListBox2.Items.Add(song)
            Next
            For Each songname In OpenFileDialog1.SafeFileNames
                ListBox1.Items.Add(songname)
            Next

        End If
    End Sub

    Private Sub VolumeBar_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles VolumeBar.ValueChanged
        If VolumeBar.Focused = True Then
            Bass.BASS_SetVolume(VolumeBar.Value / 100)
            My.Settings.volume = VolumeBar.Value / 100
        End If
    End Sub

    Private Sub FolderToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles FolderToolStripMenuItem.Click, AddFolderToolStripMenuItem.Click
        If FolderBrowserDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
            folder = FolderBrowserDialog1.SelectedPath
            updatePlaylist()
        End If
    End Sub

    Private Sub OpenFileToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OpenFileToolStripMenuItem.Click
        OpenFileDialog1.Multiselect = False
        If OpenFileDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
            ListBox1.Items.Clear()
            ListBox2.Items.Clear()
            ListBox1.Items.Add(OpenFileDialog1.SafeFileName)
            ListBox2.Items.Add(OpenFileDialog1.FileName)
            Try
                ListBox2.SelectedIndex = 0
                ListBox1.SelectedIndex = 0
                Try
                    Bass.BASS_ChannelStop(stream)
                Catch ex As Exception

                End Try
                stream = Bass.BASS_StreamCreateFile(ListBox2.SelectedItem, 0, 0, BASSFlag.BASS_DEFAULT)
                Bass.BASS_ChannelPlay(stream, False)
                refresh_Tags()
                Timer1.Start()
            Catch ex As Exception
                Try
                    ListBox2.SelectedIndex = 0
                    Bass.BASS_ChannelPlay(stream, False)
                Catch ex2 As Exception

                End Try
            End Try
        End If
    End Sub

    Private Sub OpenFolderToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OpenFolderToolStripMenuItem.Click
        If FolderBrowserDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
            ListBox1.Items.Clear()
            ListBox2.Items.Clear()
            folder = FolderBrowserDialog1.SelectedPath
            updatePlaylist()
            Try
                ListBox2.SelectedIndex = 0
                ListBox1.SelectedIndex = 0
                Try
                    Bass.BASS_ChannelStop(stream)
                Catch ex As Exception

                End Try
                stream = Bass.BASS_StreamCreateFile(ListBox2.SelectedItem, 0, 0, BASSFlag.BASS_DEFAULT)
                Bass.BASS_ChannelPlay(stream, False)
                refresh_Tags()
                Timer1.Start()
            Catch ex As Exception
                Try
                    ListBox2.SelectedIndex = 0
                    Bass.BASS_ChannelPlay(stream, False)
                Catch ex2 As Exception

                End Try
            End Try
        End If

    End Sub

  
    Private Sub RemoveSongToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RemoveSongToolStripMenuItem.Click
        Dim itemtoremove As Integer = ListBox1.SelectedIndex
        ListBox2.Items.RemoveAt(itemtoremove)
        ListBox1.Items.RemoveAt(itemtoremove)
        refresh_Tags()

    End Sub

    Private Sub ClearAllToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ClearAllToolStripMenuItem.Click
        ListBox1.Items.Clear()
        ListBox2.Items.Clear()
    End Sub

    Private Sub GTrackBar1_MouseHover(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles GTrackBar1.MouseHover
        If stream = 0 Then
            GTrackBar1.Enabled = False
        Else
            GTrackBar1.Enabled = True
        End If
    End Sub

    Private Sub Form1_SizeChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.SizeChanged
        'If Height = 461 Then
        '    TaskbarAssistant1.ThumbnailClipRegion = New Rectangle(34, 52, 240, 245)
        'Else
        '    TaskbarAssistant1.ThumbnailClipRegion = New Rectangle(PictureBox1.Location, PictureBox1.Size)
        'End If
    End Sub

    Private Sub PictureBox1_SizeChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBox1.SizeChanged
        If Height = 461 Then
            TaskbarAssistant1.ThumbnailClipRegion = New Rectangle(34, 52, 240, 244)
        Else
            TaskbarAssistant1.ThumbnailClipRegion = New Rectangle(PictureBox1.Location, PictureBox1.Size)
        End If

        If WindowsMediaPlayerToolStripMenuItem.Checked = True Then
            Timer2.Stop()
            BASS_SFX_PluginStop(hSFX4)
            hSFX4 = BASS_SFX_PluginCreate("0AA02E8D-F851-4CB0-9F64-BBA9BE7A983D", PictureBox1.Handle, PictureBox1.Width, PictureBox1.Height, BASSSFXFlag.BASS_SFX_DEFAULT)
            BASS_SFX_PluginSetStream(hSFX4, stream)
            BASS_SFX_PluginStart(hSFX4)
            Timer2.Start()
        ElseIf CustomPluginToolStripMenuItem.Checked = True Then
            BASS_SFX_PluginStop(hSFX3)
            hSFX3 = BASS_SFX_PluginCreate(OpenFileDialog2.FileName, PictureBox1.Handle, PictureBox1.Width, PictureBox1.Height, BASSSFXFlag.BASS_SFX_DEFAULT)
            BASS_SFX_PluginSetStream(hSFX3, stream)
            BASS_SFX_PluginStart(hSFX3)
            Timer2.Start()
        End If
        'MsgBox(PictureBox1.Location.ToString & "\" & PictureBox1.Size.ToString)

    End Sub

    Private Sub ToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripMenuItem1.Click
        If ToolStripMenuItem1.Checked = True Then
            ToolStripMenuItem1.Checked = False
            Me.TopMost = False
        Else
            ToolStripMenuItem1.Checked = True
            Me.TopMost = True
        End If
    End Sub
End Class














