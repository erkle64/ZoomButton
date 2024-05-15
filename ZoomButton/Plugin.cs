using C3.ModKit;
using HarmonyLib;
using System.Reflection;
using Unfoundry;
using UnityEngine;

namespace ZoomButton
{
    [UnfoundryMod(Plugin.GUID)]
    public class Plugin : UnfoundryPlugin
    {
        public const string
            MODNAME = "ZoomButton",
            AUTHOR = "erkle64",
            GUID = AUTHOR + "." + MODNAME,
            VERSION = "0.2.0";

        public static LogSource log;

        private static TypedConfigEntry<KeyCode> zoomKey;
        private static TypedConfigEntry<float> zoomFactor;

        private static bool _shouldZoom = false;
        private static bool _isZoomed = false;
        private static float _previousFOV = 90.0f;
        private static float _targetFOV = 90.0f;
        private static float _zoomRate = 0.0f;

        public Plugin()
        {
            log = new LogSource(MODNAME);

            new Config(GUID)
                .Group("General")
                    .Entry(out zoomFactor, "zoomFactor", 7.0f)
                .EndGroup()
                .Group("Keys",
                    "Key Codes: Backspace, Tab, Clear, Return, Pause, Escape, Space, Exclaim,",
                    "DoubleQuote, Hash, Dollar, Percent, Ampersand, Quote, LeftParen, RightParen,",
                    "Asterisk, Plus, Comma, Minus, Period, Slash,",
                    "Alpha0, Alpha1, Alpha2, Alpha3, Alpha4, Alpha5, Alpha6, Alpha7, Alpha8, Alpha9,",
                    "Colon, Semicolon, Less, Equals, Greater, Question, At,",
                    "LeftBracket, Backslash, RightBracket, Caret, Underscore, BackQuote,",
                    "A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,",
                    "LeftCurlyBracket, Pipe, RightCurlyBracket, Tilde, Delete,",
                    "Keypad0, Keypad1, Keypad2, Keypad3, Keypad4, Keypad5, Keypad6, Keypad7, Keypad8, Keypad9,",
                    "KeypadPeriod, KeypadDivide, KeypadMultiply, KeypadMinus, KeypadPlus, KeypadEnter, KeypadEquals,",
                    "UpArrow, DownArrow, RightArrow, LeftArrow, Insert, Home, End, PageUp, PageDown,",
                    "F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15,",
                    "Numlock, CapsLock, ScrollLock,",
                    "RightShift, LeftShift, RightControl, LeftControl, RightAlt, LeftAlt, RightApple, RightApple,",
                    "LeftCommand, LeftCommand, LeftWindows, RightWindows, AltGr,",
                    "Help, Print, SysReq, Break, Menu,",
                    "Mouse0, Mouse1, Mouse2, Mouse3, Mouse4, Mouse5, Mouse6")
                    .Entry(out zoomKey, "zoomKey", KeyCode.BackQuote)
                .EndGroup()
                .Load()
                .Save();
        }

        public override void Load(Mod mod)
        {
            log.Log($"Loading {MODNAME}");
        }

        [HarmonyPatch]
        public static class Patch
        {
            private static FieldInfo _renderCamera = typeof(GameCamera).GetField("renderCamera", BindingFlags.NonPublic | BindingFlags.Instance);

            [HarmonyPatch(typeof(GameCamera), nameof(GameCamera.Update))]
            [HarmonyPrefix]
            public static void GameCamera_Update(GameCamera __instance)
            {
                var renderCamera = (Camera)_renderCamera.GetValue(__instance);
                if (renderCamera == null) return;

                _shouldZoom = Input.GetKey(zoomKey.Get()) && !GlobalStateManager.checkIfCursorIsRequired();

                if (_shouldZoom)
                {
                    if (!_isZoomed)
                    {
                        _isZoomed = true;

                        _previousFOV = renderCamera.fieldOfView;
                        _targetFOV = _previousFOV / zoomFactor.Get();
                        _zoomRate = 0.0f;
                    }

                    renderCamera.fieldOfView = Mathf.SmoothDamp(renderCamera.fieldOfView, _targetFOV, ref _zoomRate, 0.1f, 1500.0f, Time.deltaTime);
                }
                else if(_isZoomed)
                {
                    _isZoomed = false;
                    renderCamera.fieldOfView = _previousFOV;
                }
            }
        }
    }
}


