'Imports Llama;


Public Class LamaFactory

    'Public Shared Function Llama.LMADataSource CreateDataSource(string siteNode, string projectStr)
    '    Dim ds As Llama.LMADataSource = New Llama.LMADataSource()
    '    ds.ProjectNumber = projectStr;
    '    ds.set_SiteNode(siteNode);
    '    Return ds

    Public Shared Function CreateDataSource(ByVal siteNode As String, ByVal projectStr As String) As LMADataSourceSubstitute
        Dim ds As LMADataSourceSubstitute = New LMADataSourceSubstitute()
        ds.ProjectNumber = projectStr
        ds.set_SiteNode(siteNode)
        Return ds
    End Function

End Class

'Had to use this as I did not have the Llama.dll available
'Should be the Llama.LMADataSource instead
Public Class LMADataSourceSubstitute

    Public Property ProjectNumber() As String

    Public Sub set_SiteNode(ByVal siteNode As String)

    End Sub

End Class