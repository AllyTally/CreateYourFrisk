﻿using UnityEngine;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Linq;

public class Misc {
    public string MachineName { get { return System.Environment.UserName; } }

    public void ShakeScreen(float duration, float intensity = 3, bool isIntensityDecreasing = true) {
        Camera.main.GetComponent<GlobalControls>().ShakeScreen(duration, intensity, isIntensityDecreasing);
    }

    public void StopShake() { GlobalControls.stopScreenShake = true; }

    public bool FullScreen {
        get { return Screen.fullScreen; }
        set {
            Screen.fullScreen = value;
            ScreenResolution.SetFullScreen(value, 2);
        }
    }

    public static int WindowWidth {
        get { return Screen.fullScreen && ScreenResolution.wideFullscreen ? Screen.currentResolution.width : (int)ScreenResolution.displayedSize.x; }
    }

    public static int WindowHeight {
        get { return Screen.fullScreen && ScreenResolution.wideFullscreen ? Screen.currentResolution.height : (int)ScreenResolution.displayedSize.y; }
    }

    public static int ScreenWidth {
        get { return Screen.fullScreen && !ScreenResolution.wideFullscreen ? (int)ScreenResolution.displayedSize.x : Screen.currentResolution.width; }
    }

    public static int ScreenHeight {
        get { return Screen.currentResolution.height; }
    }

    public static int MonitorWidth {
        get { return ScreenResolution.lastMonitorWidth; }
    }

    public static int MonitorHeight {
        get { return ScreenResolution.lastMonitorHeight; }
    }

    public void SetWideFullscreen(bool borderless) {
        if (!GlobalControls.isInFight)
            throw new CYFException("SetWideFullscreen is only usable from within battles.");
        ScreenResolution.wideFullscreen = borderless;
        if (Screen.fullScreen)
            ScreenResolution.SetFullScreen(true, 0);
    }

    // Whether or not should the camera's movement affect the debugger's movement. (Think of it as . . . parenting.)
    public static bool isDebuggerAttachedToCamera = true;

    public static float cameraX {
        get { return Camera.main.transform.position.x - 320; }
        set {
            if (UnitaleUtil.IsOverworld && !GlobalControls.isInShop)
                PlayerOverworld.instance.cameraShift.x += value - (Camera.main.transform.position.x - 320);
            else {
                Camera.main.transform.position = new Vector3(value + 320, Camera.main.transform.position.y, Camera.main.transform.position.z);
                if (UserDebugger.instance && isDebuggerAttachedToCamera)
                    UserDebugger.absx = value + UserDebugger.saved_x;
            }
        }
    }

    public static float cameraY {
        get { return Camera.main.transform.position.y - 240; }
        set {
            if (UnitaleUtil.IsOverworld && !GlobalControls.isInShop)
                PlayerOverworld.instance.cameraShift.y += value - (Camera.main.transform.position.y - 240);
            else {
                Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, value + 240, Camera.main.transform.position.z);
                if (UserDebugger.instance && isDebuggerAttachedToCamera)
                    UserDebugger.absy = value + UserDebugger.saved_y;
            }
        }
    }

    public static void MoveCamera(float x, float y) {
        cameraX += x;
        cameraY += y;
    }

    public static void MoveCameraTo(float x, float y) {
        cameraX = x;
        cameraY = y;
    }

    public static void ResetCamera() {
        if (UnitaleUtil.IsOverworld && !GlobalControls.isInShop)
            PlayerOverworld.instance.cameraShift = Vector2.zero;
        else
            MoveCameraTo(0f, 0f);
    }

    public LuaSpriteShader ScreenShader {
        get { return CameraShader.luashader; }
    }

    public static void DestroyWindow() { Application.Quit(); }

    // TODO: When OW is reworked, add 3rd argument to open a file in any of "mod", "map" or "default" locations
    public static LuaFile OpenFile(string path, string mode = "rw") { return new LuaFile(path, mode); }

    public bool FileExists(string path) {
        if (path.Contains(".."))
            throw new CYFException("You cannot check for a file outside of a mod folder. The use of \"..\" is forbidden.");
        return File.Exists((FileLoader.ModDataPath + "/" + path).Replace('\\', '/'));
    }

    public bool DirExists(string path) {
        if (path.Contains(".."))
            throw new CYFException("You cannot check for a directory outside of a mod folder. The use of \"..\" is forbidden.");
        return Directory.Exists((FileLoader.ModDataPath + "/" + path).Replace('\\', '/'));
    }

    public bool CreateDir(string path) {
        if (path.Contains(".."))
            throw new CYFException("You cannot create a directory outside of a mod folder. The use of \"..\" is forbidden.");

        if (Directory.Exists((FileLoader.ModDataPath + "/" + path).Replace('\\', '/'))) return false;
        Directory.CreateDirectory(FileLoader.ModDataPath + "/" + path);
        return true;
    }

    private static bool PathValid(string path) { return path != " " && path != "" && path != "/" && path != "\\" && path != "." && path != "./" && path != ".\\"; }

    public bool MoveDir(string path, string newPath) {
        if (path.Contains("..") || newPath.Contains(".."))
            throw new CYFException("You cannot move a directory outside of a mod folder. The use of \"..\" is forbidden.");

        if (!DirExists(path) || DirExists(newPath) || !PathValid(path)) return false;
        Directory.Move(FileLoader.ModDataPath + "/" + path, FileLoader.ModDataPath + "/" + newPath);
        return true;
    }

    public bool RemoveDir(string path, bool force = false) {
        if (path.Contains(".."))
            throw new CYFException("You cannot remove a directory outside of a mod folder. The use of \"..\" is forbidden.");

        if (!Directory.Exists((FileLoader.ModDataPath + "/" + path).Replace('\\', '/'))) return false;
        try { Directory.Delete(FileLoader.ModDataPath + "/" + path, force); }
        catch { /* ignored */ }

        return false;
    }

    public string[] ListDir(string path, bool getFolders = false) {
        if (path == null)        throw new CYFException("Cannot list a directory with a nil path.");
        if (path.Contains("..")) throw new CYFException("You cannot list directories outside of a mod folder. The use of \"..\" is forbidden.");

        path = (FileLoader.ModDataPath + "/" + path).Replace('\\', '/');
        if (!Directory.Exists(path))
            throw new CYFException("Invalid path:\n\n\"" + path + "\"");

        DirectoryInfo d = new DirectoryInfo(path);
        System.Collections.Generic.List<string> retval = new System.Collections.Generic.List<string>();
        retval.AddRange(!getFolders ? d.GetFiles().Select(fi => Path.GetFileName(fi.ToString()))
                                    : d.GetDirectories().Select(di => di.Name));
        return retval.ToArray();
    }

    public static string OSType {
        get {
            switch (Application.platform) {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer: return "Windows";
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.LinuxPlayer:   return "Linux";
                default:                            return "Mac";
            }
        }
    }
    
    public static float debuggerX {
        get {
            if (UserDebugger.instance)
                return UserDebugger.x;
            throw new CYFException("Misc.debuggerX cannot be used outside of a function.");
        }
        set {
            if (UserDebugger.instance)
                UserDebugger.x = value;
            throw new CYFException("Misc.debuggerX cannot be used outside of a function.");
        }
    }
    
    public static float debuggerY {
        get {
            if (UserDebugger.instance) return UserDebugger.y;
            else throw new CYFException("Misc.debuggerY cannot be used outside of a function.");
        }
        set {
            if (UserDebugger.instance) UserDebugger.y = value;
            else throw new CYFException("Misc.debuggerY cannot be used outside of a function.");
        }
    }
    
    public static float debuggerAbsX {
        get {
            if (UserDebugger.instance) return UserDebugger.absx;
            else throw new CYFException("Misc.debuggerAbsX cannot be used outside of a function.");
        }
        set {
            if (UserDebugger.instance) UserDebugger.absx = value;
            else throw new CYFException("Misc.debuggerAbsX cannot be used outside of a function.");
        }
    }
    
    public static float debuggerAbsY {
        get {
            if (UserDebugger.instance) return UserDebugger.absy;
            else throw new CYFException("Misc.debuggerAbsY cannot be used outside of a function.");
        }
        set {
            if (UserDebugger.instance) UserDebugger.absy = value;
            else throw new CYFException("Misc.debuggerAbsY cannot be used outside of a function.");
        }
    }
    
    // Moves the debugger relative to its current position.
    public static void MoveDebugger(float x, float y) {
        if (UserDebugger.instance) UserDebugger.Move(x, y);
        else throw new CYFException("Misc.MoveDebugger cannot be used outside of a function.");
    }
    
    // Moves the debugger relative to the camera's position. The default position is (300, 480). The debugger's width is 320 and its height is 140. The debugger's pivot is the top-left corner.
    public static void MoveDebuggerTo(float x, float y) {
        if (UserDebugger.instance) UserDebugger.MoveTo(x, y);
        else throw new CYFException("Misc.MoveDebuggerTo cannot be used outside of a function.");
    }
    
    // Moves the debugger relative to the game's (0, 0) position.
    public static void MoveDebuggerToAbs(float x, float y) {
        if (UserDebugger.instance) UserDebugger.MoveToAbs(x, y);
        else throw new CYFException("Misc.MoveDebuggerToAbs cannot be used outside of a function.");
    }

    #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        [DllImport("user32.dll")]
        private static extern int GetActiveWindow();
        public static int window = GetActiveWindow();

        public static void RetargetWindow() { window = GetActiveWindow(); }

        [DllImport("user32.dll")]
        public static extern int FindWindow(string className, string windowName);
        [DllImport("user32.dll")]
        private static extern int MoveWindow(int hwnd, int x, int y, int nWidth, int nHeight, int bRepaint);
        [DllImport("user32.dll", EntryPoint = "GetWindowText", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(int hwnd, StringBuilder lpWindowText, int nMaxCount);
        [DllImport("user32.dll", EntryPoint = "SetWindowText")]
        public static extern bool SetWindowText(int hwnd, string text);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(int hWnd, out RECT lpRect);
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public static string WindowName {
            get {
                StringBuilder strbTitle = new StringBuilder(9999);
                GetWindowText(window, strbTitle, strbTitle.Capacity + 1);
                return strbTitle.ToString();
            }
            set { SetWindowText(window, value); }
        }

        public static int WindowX {
                get {
                    Rect size = GetWindowRect();
                    return (int)size.x;
                }
                set {
                     Rect size = GetWindowRect();
                     MoveWindow(window, value, (int)size.y, (int)size.width, (int)size.height, 1);
                }
            }

        public static int WindowY {
            get {
                Rect size = GetWindowRect();
                return Screen.currentResolution.height - (int)size.y - (int)size.height;
            }
            set {
                 Rect size = GetWindowRect();
                 MoveWindow(window, (int)size.x, Screen.currentResolution.height - value - (int)size.height, (int)size.width, (int)size.height, 1);
            }
        }

        public static void MoveWindow(int X, int Y) {
            Rect size = GetWindowRect();
            if (!Screen.fullScreen)
                MoveWindow(window, (int)size.x + X, (int)size.y - Y, (int)size.width, (int)size.height, 1);
        }

        public static void MoveWindowTo(int X, int Y) {
            Rect size = GetWindowRect();
            if (!Screen.fullScreen)
                MoveWindow(window, X, Screen.currentResolution.height - Y - (int)size.height, (int)size.width, (int)size.height, 1);
        }

        private static Rect GetWindowRect() {
            RECT r;
            GetWindowRect(window, out r);
            return new Rect(r.Left, r.Top, Mathf.Abs(r.Right - r.Left), Mathf.Abs(r.Top - r.Bottom));
        }
    #else
        public static string WindowName {
            get {
                UnitaleUtil.DisplayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here.");
                return "";
            }
            set { UnitaleUtil.DisplayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here."); }
        }

        public static int WindowX {
            get {
                UnitaleUtil.DisplayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here.");
                return 0;
            }
            set { UnitaleUtil.DisplayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here."); }
        }

        public static int WindowY {
            get {
                UnitaleUtil.DisplayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here.");
                return 0;
            }
            set { UnitaleUtil.DisplayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here."); }
        }

        public static void MoveWindowTo(int X, int Y) {
            UnitaleUtil.DisplayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here.");
            return;
        }

        public static void MoveWindow(int X, int Y) {
            UnitaleUtil.DisplayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here.");
            return;
        }

        public static Rect GetWindowRect() {
            UnitaleUtil.DisplayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here.");
            return new Rect();
        }
    #endif
}