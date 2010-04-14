@echo off
@set PATH=c:\Windows\Microsoft.NET\Framework\v3.5;c:\Windows\Microsoft.NET\Framework\v2.0.50727;%PATH%
cd %~dp0%
edmgen.exe /mode:fullgeneration /connectionstring:"data source=.\SQLEXPRESS; Initial Catalog=Camelot_Test; User Id=camelot; Password=camelot;" /provider:System.Data.SqlClient /entitycontainer:Entities /namespace:org.ids_adi.iring.adapter.dataLayer.Model /language:CSharp /outssdl:Model.ssdl /outcsdl:Model.csdl /outmsl:Model.msl /outobjectlayer:..\App_Code\Model.cs /outviews:..\App_Code\Views.cs
pause
