PipeBench
=========

Simple straightforward test for the efficiency of BlockingCollection. The control run is on .NET Core which has roughly the same performance as the normal desktop .NET on Windows.

There's something wrong with Mono and it has been tested at least with versions 4.2.4 (EL7 EPEL), 4.4.2 (Fedora 25) and 4.6.2 (Xamarin MonoDevelop flatpak).

Slow run on Mono
----------------
```
$ mono --version
Mono JIT compiler version 4.2.4 (tarball Mon Sep 19 02:09:55 UTC 2016)
Copyright (C) 2002-2014 Novell, Inc, Xamarin Inc and Contributors. www.mono-project.com
	TLS:           __thread
	SIGSEGV:       altstack
	Notifications: epoll
	Architecture:  amd64
	Disabled:      none
	Misc:          softdebug 
	LLVM:          supported, not enabled.
	GC:            sgen
$ mono bin/Release/PipeBench.exe 
BlockingPipeStream test starting.
BlockingPipeStream test wrote 65536000000 bytes in 1000000 iterations.
BlockingPipeStream test read 65536000000 bytes in 10.291 seconds.
BufferingPipeStream test starting.
BufferingPipeStream test wrote 65536000000 bytes in 1000000 iterations.
BufferingPipeStream test read 65536000000 bytes in 22.026 seconds.
CompatBufferingPipeStream test starting.
CompatBufferingPipeStream test wrote 65536000000 bytes in 1000000 iterations.
CompatBufferingPipeStream test read 65536000000 bytes in 22.903 seconds.

```

Fast run on .NET Core
---------------------
```
$ dotnet --version
1.0.0-preview2-1-003177
$ dotnet bin/Release/netcoreapp1.1/PipeBench.dll 
BlockingPipeStream test starting.
BlockingPipeStream test wrote 65536000000 bytes in 1000000 iterations.
BlockingPipeStream test read 65536000000 bytes in 7.848 seconds.
BufferingPipeStream test starting.
BufferingPipeStream test wrote 65536000000 bytes in 1000000 iterations.
BufferingPipeStream test read 65536000000 bytes in 7.723 seconds.
CompatBufferingPipeStream test starting.
CompatBufferingPipeStream test wrote 65536000000 bytes in 1000000 iterations.
CompatBufferingPipeStream test read 65536000000 bytes in 6.986 seconds.

```

Slow run on Mono on Windows 10 VM
---------------------------------
```
>"C:\Program Files\Mono\bin\mono.exe" PipeBench.exe
BlockingPipeStream test starting.
BlockingPipeStream test wrote 65536000000 bytes in 1000000 iterations.
BlockingPipeStream test read 65536000000 bytes in 15.778 seconds.
BufferingPipeStream test starting.
BufferingPipeStream test wrote 65536000000 bytes in 1000000 iterations.
BufferingPipeStream test read 65536000000 bytes in 23.576 seconds.
CompatBufferingPipeStream test starting.
CompatBufferingPipeStream test wrote 65536000000 bytes in 1000000 iterations.
CompatBufferingPipeStream test read 65536000000 bytes in 23.819 seconds.
```

Fast run on .NET 4 on Windows 10 VM
-----------------------------------
```
>PipeBench.exe
BlockingPipeStream test starting.
BlockingPipeStream test wrote 65536000000 bytes in 1000000 iterations.
BlockingPipeStream test read 65536000000 bytes in 12,975 seconds.
BufferingPipeStream test starting.
BufferingPipeStream test wrote 65536000000 bytes in 1000000 iterations.
BufferingPipeStream test read 65536000000 bytes in 8,244 seconds.
CompatBufferingPipeStream test starting.
CompatBufferingPipeStream test wrote 65536000000 bytes in 1000000 iterations.
CompatBufferingPipeStream test read 65536000000 bytes in 6,93 seconds.
```
