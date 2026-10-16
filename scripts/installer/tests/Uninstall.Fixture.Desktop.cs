// 独立卸载夹具的专用桌面与精确 HWND 互操作；不扫描用户桌面。
using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;
public static class CodeWFUninstallFixtureDesktop {
 [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)] public struct StartupInfo { public int cb; public string reserved,desktop,title; public int x,y,cx,cy,charsX,charsY,fill,flags; public short show,reserved2; public IntPtr reservedPtr,input,output,error; }
 [StructLayout(LayoutKind.Sequential)] public struct ProcessInfo { public IntPtr process,thread; public int processId,threadId; }
 [StructLayout(LayoutKind.Sequential)] public struct Rect { public int left,top,right,bottom; }
 public delegate bool EnumProc(IntPtr window,IntPtr data);
 [DllImport("user32.dll",CharSet=CharSet.Unicode,SetLastError=true)] public static extern IntPtr CreateDesktop(string name,IntPtr device,IntPtr mode,int flags,uint access,IntPtr security);
 [DllImport("user32.dll")] public static extern bool CloseDesktop(IntPtr desktop);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] public static extern bool CreateProcess(string application,StringBuilder command,IntPtr processAttr,IntPtr threadAttr,bool inherit,uint flags,IntPtr environment,string directory,ref StartupInfo startup,out ProcessInfo process);
 [DllImport("kernel32.dll")] public static extern bool CloseHandle(IntPtr handle);
 [DllImport("kernel32.dll")] public static extern IntPtr OpenProcess(uint access,bool inherit,uint processId);
 [DllImport("kernel32.dll")] public static extern bool TerminateProcess(IntPtr process,uint exitCode);
 [DllImport("kernel32.dll")] public static extern uint WaitForSingleObject(IntPtr handle,uint milliseconds);
 [DllImport("kernel32.dll")] public static extern bool GetExitCodeProcess(IntPtr process,out uint exitCode);
 [DllImport("user32.dll")] public static extern bool EnumDesktopWindows(IntPtr desktop,EnumProc callback,IntPtr data);
 [DllImport("user32.dll")] public static extern bool EnumChildWindows(IntPtr parent,EnumProc callback,IntPtr data);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] public static extern int GetClassName(IntPtr window,StringBuilder text,int count);
 [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr window,out Rect rect);
 [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr window);
 [DllImport("user32.dll")] public static extern bool IsWindow(IntPtr window);
 [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr window,IntPtr dc,uint flags);
 [DllImport("user32.dll",CharSet=CharSet.Unicode,SetLastError=true)] public static extern IntPtr SendMessageTimeout(IntPtr window,uint message,IntPtr wp,IntPtr lp,uint flags,uint timeout,out UIntPtr result);
 [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr window,out uint processId);
 [DllImport("user32.dll")] public static extern int GetWindowRgn(IntPtr window,IntPtr region);
 [DllImport("gdi32.dll")] public static extern IntPtr CreateRectRgn(int left,int top,int right,int bottom);
 [DllImport("gdi32.dll")] public static extern bool DeleteObject(IntPtr region);
 public static string Text(IntPtr window) { if(!IsWindow(window))return "";IntPtr buffer=Marshal.AllocHGlobal(16384); try {Marshal.WriteInt16(buffer,0);try{Send(window,13,8192,buffer);}catch(InvalidOperationException){if(!IsWindow(window))return "";throw;}return Marshal.PtrToStringUni(buffer) ?? "";} finally {Marshal.FreeHGlobal(buffer);} }
 public static string Class(IntPtr window) { var text=new StringBuilder(512); GetClassName(window,text,text.Capacity); return text.ToString(); }
 public static List<IntPtr> Windows(IntPtr desktop) { var windows=new List<IntPtr>(); EnumDesktopWindows(desktop,(w,d)=>{windows.Add(w);return true;},IntPtr.Zero); return windows; }
 public static List<IntPtr> Children(IntPtr parent) { var windows=new List<IntPtr>(); EnumChildWindows(parent,(w,d)=>{windows.Add(w);return true;},IntPtr.Zero); return windows; }
 public static ulong Send(IntPtr window,uint message) { return Send(window,message,0,IntPtr.Zero); }
 public static ulong Send(IntPtr window,uint message,long wp) { return Send(window,message,wp,IntPtr.Zero); }
 public static ulong Send(IntPtr window,uint message,long wp,IntPtr lp) { UIntPtr result; if(SendMessageTimeout(window,message,new IntPtr(wp),lp,2,5000,out result)==IntPtr.Zero) throw new InvalidOperationException("Fixture window did not respond."); return result.ToUInt64(); }
 public static void Capture(IntPtr window,string path) { Rect r;if(!GetWindowRect(window,out r) || r.right<=r.left || r.bottom<=r.top) throw new InvalidOperationException("Fixture window bounds invalid.");using(var b=new Bitmap(r.right-r.left,r.bottom-r.top)){using(var g=Graphics.FromImage(b)){IntPtr dc=g.GetHdc();try {if(!PrintWindow(window,dc,2)) throw new InvalidOperationException("PrintWindow failed.");} finally {g.ReleaseHdc(dc);}} b.Save(path);} }
}
