@Echo off
Set unidade=%~dp0
%unidade:~0,2%
cd %~dp0
%Windir%\Microsoft.NET\Framework\v4.0.30319\InstallUtil /i ServicoIntegracaoViaFTP.Service.exe
%Windir%\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe ServicoIntegracaoViaFTP.Executor.dll /tlb:ServicoIntegracaoViaFTP.Executor.tlb /codebase
pause