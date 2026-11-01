#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>Asynchronous native Windows picker; Unity keeps pumping its owner window while the dialog is open.</summary>
public static class WindowsSaveDialog
{
    public static Task<string?> Choose(bool saving)
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        IntPtr owner = GetActiveWindow();
        var completion = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            try
            {
                var path = new StringBuilder(32768);
                int result = ChooseNative(saving ? 1 : 0, owner, path, path.Capacity);
                if (result < 0) throw new InvalidOperationException("Windows file dialog failed (HRESULT " + result.ToString("X8") + ").");
                completion.TrySetResult(result == 1 ? null : path.ToString());
            }
            catch (Exception error) { completion.TrySetException(error); }
        });
        thread.IsBackground = true;
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return completion.Task;
#else
        return Task.FromException<string?>(new PlatformNotSupportedException("Save transfer requires the Windows player."));
#endif
    }
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    [DllImport("user32.dll")] static extern IntPtr GetActiveWindow();
    [DllImport("Forgeborne.Windows", EntryPoint = "ForgeborneChooseSave", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    static extern int ChooseNative(int saving, IntPtr owner, StringBuilder path, int capacity);
#endif
}
