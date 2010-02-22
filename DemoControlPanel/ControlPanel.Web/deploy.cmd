xcopy *.svc    ..\..\..\Deliverables\DemoControlPanel /Y
xcopy *.aspx   ..\..\..\Deliverables\DemoControlPanel /Y
xcopy *.cs     ..\..\..\Deliverables\DemoControlPanel /Y
xcopy *.js     ..\..\..\Deliverables\DemoControlPanel /Y

xcopy Bin\*.*       ..\..\..\Deliverables\DemoControlPanel\Bin /Y
xcopy ClientBin\*.* ..\..\..\Deliverables\DemoControlPanel\ClientBin /Y
xcopy App_Code\*.*  ..\..\..\Deliverables\DemoControlPanel\App_Code /e /Y
xcopy XML\*.*       ..\..\..\Deliverables\DemoControlPanel\XML /Y