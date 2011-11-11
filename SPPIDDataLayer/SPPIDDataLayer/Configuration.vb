Imports System.Collections.Generic
Imports System.Xml.Serialization
Imports System.Runtime.Serialization
Imports System.Linq
Imports System.Text

Imports org.iringtools.library


<DataContract(Name:="document")> _
Public Class SPPIDConfiguration
    <DataMember(Name:="siteConnectionString", Order:=0)> _
    Public Property SiteConnectionString() As String
        Get
            Return m_SiteConnectionString
        End Get
        Set(value As String)
            m_SiteConnectionString = value
        End Set
    End Property
    Private m_SiteConnectionString As String

    <DataMember(Name:="plantConnectionString", Order:=1)> _
    Public Property PlantConnectionString() As String
        Get
            Return m_PlantConnectionString
        End Get
        Set(value As String)
            m_PlantConnectionString = value
        End Set
    End Property
    Private m_PlantConnectionString As String

    <DataMember(Name:="stagingConnectionString", Order:=2)> _
    Public Property StagingConnectionString() As String
        Get
            Return m_StagingConnectionString
        End Get
        Set(value As String)
            m_StagingConnectionString = value
        End Set
    End Property
    Private m_StagingConnectionString As String





End Class



