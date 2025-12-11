using System;

internal static class ActivatorHelpers
{

    public static extern int Process32Next(IntPtr handle, ref Login.Activator.ProcessEntry32 pe);

   
}