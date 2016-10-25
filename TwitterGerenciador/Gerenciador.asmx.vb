Imports System.Web.Services
Imports System.ComponentModel
Imports System.Xml
Imports Autenticador
Imports TwitterVB2

' To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
' <System.Web.Script.Services.ScriptService()> _
<System.Web.Services.WebService(Namespace:="http://tempuri.org/")> _
<System.Web.Services.WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<ToolboxItem(False)> _
Public Class Gerenciador
    Inherits System.Web.Services.WebService

    <WebMethod()> _
    Public Function EnviaMensagemDireta(ByVal pParaUsuario As String, ByVal pMensagem As String, _
                                        ByVal pToken As String, ByVal pTokenSecret As String) As Boolean

        Try
            Dim wrkUrl As String = String.Format("http://twitter.com/direct_messages/new.xml?user={0}&text={1}", pParaUsuario, pMensagem)
            Dim oAuth As New oAuthTwitter
            oAuth.Token = pToken
            oAuth.TokenSecret = pTokenSecret

            Dim wrkResquest As String = oAuth.oAuthWebRequest(oAuthTwitter.Method.POST, wrkUrl, String.Empty)

            If wrkResquest = String.Empty Then
                Return False
            End If

            Return True

        Catch wrkerro As Exception
            Return False
        End Try

    End Function

    ''' <summary>
    ''' Método de busca de mensagens diretas utilizado XML
    ''' </summary>
    ''' <param name="pSinceId"></param>
    ''' <param name="pCount"></param>
    ''' <param name="pToken"></param>
    ''' <param name="pTokenSecret"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <WebMethod()> _
    Public Function BuscaMensagensDiretas(ByVal pSinceId As String, ByVal pCount As Integer, _
                                          ByVal pToken As String, ByVal pTokenSecret As String) As String
        Try
            Dim tw As New TwitterAPI
            tw.AuthenticateWith("zCAWzBY7NZroHtjW9O1Q", "idHgDe8ClVZuihHYP5pFGEYp8rogB7rWTk4ph8iv3Q", pToken, pTokenSecret)

            ' Specify that we want 50 statuses instead of the default 20
            Dim tp As New TwitterParameters
            tp.Add(TwitterParameterNames.Count, pCount)
            tp.Add(TwitterParameterNames.SinceID, pSinceId)

            Dim wrkRetorno As New StringBuilder

            For Each Message As TwitterDirectMessage In tw.DirectMessages(tp)
                wrkRetorno.Append(Message.ID.ToString.Replace(";", ".") & ";")
                wrkRetorno.Append(Message.Text.Replace(";", ".") & ";")
                wrkRetorno.Append(Message.SenderScreenName.Replace(";", ".") & "-")
            Next

            If String.IsNullOrEmpty(wrkRetorno.ToString) Then
                Return "null"
            Else
                Return wrkRetorno.ToString()
            End If

            'Dim wrkUrl As String = String.Format("http://api.twitter.com/1/direct_messages.xml?since_id={0}&count={1}&", pSinceId, pCount)

            'Dim oAuth As New oAuthTwitter
            'oAuth.Token = pToken
            'oAuth.TokenSecret = pTokenSecret


            'Dim wrkResquest As String = oAuth.oAuthWebRequest(oAuthTwitter.Method.GET, wrkUrl, String.Empty)

            'Dim wrkXmlDocument As New XmlDocument
            'wrkXmlDocument.LoadXml(wrkResquest)

            'Dim wrkLista As XmlNodeList
            'wrkLista = wrkXmlDocument.SelectNodes("//direct_message")

            'Dim wrkRetorno As New StringBuilder

            'Dim wrkNode As XmlNode
            'For Each wrkNode In wrkLista
            '    wrkRetorno.Append(wrkNode.SelectSingleNode("id").InnerText.ToString().Replace(";", ".") & ";")
            '    wrkRetorno.Append(wrkNode.SelectSingleNode("text").InnerText.ToString().Replace(";", ".") & ";")
            '    wrkRetorno.Append(wrkNode.SelectSingleNode("sender_screen_name").InnerText.ToString().Replace(";", ".") & "-")
            'Next

            'If String.IsNullOrEmpty(wrkRetorno.ToString) Then
            '    Return "null"
            'Else
            '    Return wrkRetorno.ToString()
            'End If

        Catch wrkErro As Exception
            Return "Erro</direct-messages>"
            'Return BuscaMensagensDiretasTeste(pSinceId, pCount, pToken, pTokenSecret)
        End Try

    End Function

    ''' <summary>
    ''' Método de busca de mensagens diretas utilizado JSON
    ''' </summary>
    ''' <param name="pSinceId"></param>
    ''' <param name="pCount"></param>
    ''' <param name="pToken"></param>
    ''' <param name="pTokenSecret"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <WebMethod()> _
    Public Function BuscaMensagensDiretasJson(ByVal pSinceId As String, ByVal pCount As Integer, _
                                          ByVal pToken As String, ByVal pTokenSecret As String) As String
        Try
            Dim wrkUrl As String = String.Format("http://api.twitter.com/1/direct_messages.json?since_id={0}&count={1}&", pSinceId, pCount)

            Dim oAuth As New oAuthTwitter
            oAuth.Token = pToken
            oAuth.TokenSecret = pTokenSecret

            Dim wrkResquest As String = oAuth.oAuthWebRequest(oAuthTwitter.Method.GET, wrkUrl, String.Empty)

            Dim wrkXmlDocument As New XmlDocument
            wrkXmlDocument.LoadXml(wrkResquest)

            Dim wrkLista As XmlNodeList
            wrkLista = wrkXmlDocument.SelectNodes("//direct_message")

            Dim wrkRetorno As New StringBuilder

            Dim wrkNode As XmlNode
            For Each wrkNode In wrkLista
                wrkRetorno.Append(wrkNode.SelectSingleNode("id").InnerText.ToString().Replace(";", ".") & ";")
                wrkRetorno.Append(wrkNode.SelectSingleNode("text").InnerText.ToString().Replace(";", ".") & ";")
                wrkRetorno.Append(wrkNode.SelectSingleNode("sender_screen_name").InnerText.ToString().Replace(";", ".") & "-")
            Next

            If String.IsNullOrEmpty(wrkRetorno.ToString) Then
                Return "null"
            Else
                Return wrkRetorno.ToString()
            End If

        Catch wrkErro As Exception
            Return "Erro</direct-messages>"
            'Return BuscaMensagensDiretasTeste(pSinceId, pCount, pToken, pTokenSecret)
        End Try

    End Function

    ''' <summary>
    ''' Método somente utilizado para fins demonstrativos quando o sistema do Twitter não estiver On-line
    ''' </summary>
    ''' <param name="pSinceId"></param>
    ''' <param name="pCount"></param>
    ''' <param name="pToken"></param>
    ''' <param name="pTokenSecret"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <WebMethod()> _
    Public Function BuscaMensagensDiretasTeste(ByVal pSinceId As String, ByVal pCount As Integer, _
                                          ByVal pToken As String, ByVal pTokenSecret As String) As String
        Try
            Dim wrkRetorno As New StringBuilder

            wrkRetorno.Append(pSinceId & ";")
            wrkRetorno.Append("comando tumon" & ";")
            wrkRetorno.Append("casabot" & "-")

            If String.IsNullOrEmpty(wrkRetorno.ToString) Then
                Return "null"
            Else
                Return wrkRetorno.ToString()
            End If

        Catch wrkErro As Exception
            Return "Erro</direct-messages>"
        End Try

    End Function

End Class