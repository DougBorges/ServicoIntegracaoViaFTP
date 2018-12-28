@Echo off
Set unidade=%~dp0
%unidade:~0,2%
cd %~dp0
%Windir%\Microsoft.NET\Framework\v4.0.30319\InstallUtil /u ServicoIntegracaoViaFTP.Service.exe
%Windir%\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe /unregister ServicoIntegracaoViaFTP.Executor.dll
del /f /q ServicoIntegracaoViaFTP.Executor.tlb
pause