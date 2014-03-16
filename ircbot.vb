Imports System.Net
Imports System.Text
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Threading
Namespace ircclient

    Class MainClass
        Public Shared Sub Main(ByVal args As String())
            Dim R As IO.StreamReader
            Dim buf As String, nick As String, owner As String, chan As String, reddituser As String, fromuser As String
            Dim sock As New System.Net.Sockets.TcpClient()
            Dim input As System.IO.TextReader
            Dim output As System.IO.TextWriter
            chan = "#channel"
            owner = "bcpu"
            nick = "ircbot"
            'Connect to irc server and get input and output text streams from TcpClient.
            sock.Connect("irc.server.net", "6667")
            If Not sock.Connected Then
                Console.WriteLine("Failed to connect!")
                Return
            End If
            input = New System.IO.StreamReader(sock.GetStream())
            output = New System.IO.StreamWriter(sock.GetStream())

            'Starting USER and NICK login commands 
            output.Write("USER " & nick & " 0 * :" & owner & vbCr & vbLf & "NICK " & nick & vbCr & vbLf)
            output.Flush()

            'Process each line received from irc server
            buf = input.ReadLine()
            While True

                'Display received irc message
                Console.WriteLine(buf)

                'Send pong reply to any ping messages
                If buf.StartsWith("PING ") Then
                    output.Write(buf.Replace("PING", "PONG") & vbCr & vbLf)
                    output.Flush()
                End If
                If buf(0) <> ":"c Then
                    Continue While
                End If

    'After server sends 001 command, we can set mode to bot and join a channel
                If buf.Split(" "c)(1) = "001" Then
                    output.Write("MODE " & nick & " +B" & vbCr & vbLf & "JOIN " & chan & vbCr & vbLf)
                    output.Flush()
                End If
                buf = input.ReadLine()

                ' reply back to user who said yodel bot
                If buf.Contains("yodel bot") Then
                    Dim sourcestring As String = buf
                    Dim re As Regex = New Regex(":(.*?)!")
                    Dim mc As MatchCollection = re.Matches(sourcestring)
                    Dim mIdx As Integer = 0
                    For Each m As Match In mc
                        For groupIdx As Integer = 0 To m.Groups.Count - 1
                            ' Console.WriteLine("[{0}][{1}] = {2}", mIdx, re.GetGroupNames(groupIdx), m.Groups(groupIdx).Value)
                        Next
                        mIdx = mIdx + 1
                    Next
                 '   Console.WriteLine(mc(0).Groups(1).Value)
                    fromuser = (mc(0).Groups(1).Value)
                    output.Write("PRIVMSG " & chan & " :not much " & fromuser & vbCr & vbLf)

                    output.Flush()
                End If

                ' grab reddit karma of user
                If buf.Contains("!karma") Then
                    Dim sourcestring As String = buf
                    Dim re As Regex = New Regex("karma(.*?)$")
                    Dim mc As MatchCollection = re.Matches(sourcestring)
                    Dim mIdx As Integer = 0
                    For Each m As Match In mc
                        For groupIdx As Integer = 0 To m.Groups.Count - 1
                            'Console.WriteLine("[{0}][{1}] = {2}", mIdx, re.GetGroupNames(groupIdx), m.Groups(groupIdx).Value)
                        Next
                        mIdx = mIdx + 1
                    Next
                    'Console.WriteLine(mc(0).Groups(1).Value)
                    reddituser = (mc(0).Groups(1).Value)
                    reddituser = reddituser.Replace(" ", "")

                    '====Grab karma values from reddit!!!!!!!!!
                    Console.WriteLine("Grabbing karma for " & reddituser)
                    Dim linkkarma As String
                    Dim commentkarma As String
                    Dim request As HttpWebRequest = DirectCast(WebRequest.Create("http://www.reddit.com/user/" & reddituser), HttpWebRequest)
                    Dim tempCookies As New CookieContainer
                    Dim encoding As New UTF8Encoding
                    request.CookieContainer = tempCookies
                    request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; ru; rv:1.9.2.3) Gecko/20100401 Firefox/4.0 (.NET CLR 3.5.30729)"
                    Dim response As HttpWebResponse = DirectCast(request.GetResponse(), HttpWebResponse)
                    Dim reader As New StreamReader(response.GetResponseStream())
                    Dim theusercp As String = reader.ReadToEnd
                    Dim postresponse As HttpWebResponse
                    postresponse = DirectCast(request.GetResponse(), HttpWebResponse)
                    tempCookies.Add(postresponse.Cookies)

                    'regex link karma
                    Dim sourcestring2 As String = theusercp
                    Dim re3 As Regex = New Regex("class=""karma"">(.*?)<")
                    Dim mc3 As MatchCollection = re3.Matches(sourcestring2)
                    Dim mIdx3 As Integer = 0
                    For Each m As Match In mc3
                        For groupIdx As Integer = 0 To m.Groups.Count - 1
                            Console.WriteLine("[{0}][{1}] = {2}", mIdx3, re.GetGroupNames(groupIdx), m.Groups(groupIdx).Value)
                        Next
                        mIdx3 = mIdx3 + 1
                    Next
                    Console.WriteLine("{0} matches found in:", mc3.Count)

                    'regex comment karma
                    Dim re2 As Regex = New Regex("comment-karma"">(.*?)<")
                    Dim mc2 As MatchCollection = re2.Matches(sourcestring2)
                    Dim mIdx2 As Integer = 0
                    For Each m As Match In mc2
                        For groupIdx As Integer = 0 To m.Groups.Count - 1
                            Console.WriteLine("[{0}][{1}] = {2}", mIdx2, re2.GetGroupNames(groupIdx), m.Groups(groupIdx).Value)
                        Next
                        mIdx2 = mIdx2 + 1
                    Next
                    Console.WriteLine("{0} matches found in:", mc.Count)
                    linkkarma = (mc3(0).Groups(1).Value)
                    commentkarma = (mc2(0).Groups(1).Value)
                    Console.WriteLine("Link Karma = " & linkkarma)
                    Console.WriteLine("Comment Karma = " & commentkarma)
                    output.Write("PRIVMSG " & chan & " :" & reddituser & " has " & linkkarma & " link & " & commentkarma & " comment karma." & vbCr & vbLf)
                    output.Flush()
                End If
            End While
        End Sub
        
    End Class
End Namespace
