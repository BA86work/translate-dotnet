using System;
using System.Windows.Input;

namespace RealTimeTranslator.Services.Interfaces;

public interface IHotkeyService
{
    event EventHandler<Key> HotkeyPressed;
    void RegisterHotkey(Key key);
    void UnregisterHotkey(Key key);
} 