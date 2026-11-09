@echo off
rem Double-click launcher for Perch Sort on Windows; forwards all arguments to run.ps1.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run.ps1" %*
