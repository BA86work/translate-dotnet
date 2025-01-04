using System;
using System.Collections.Generic;
using System.Windows.Input;
using RealTimeTranslator.Services.Interfaces;

namespace RealTimeTranslator.Services.Implementations;

public class HotkeyService : IHotkeyService
{
    private readonly Dictionary<Key, bool> _registeredHotkeys = new();

    public event EventHandler<Key> HotkeyPressed;

    public void RegisterHotkey(Key key)
    {
        if (!_registeredHotkeys.ContainsKey(key))
        {
            _registeredHotkeys[key] = true;
        }
    }

    public void UnregisterHotkey(Key key)
    {
        if (_registeredHotkeys.ContainsKey(key))
        {
            _registeredHotkeys.Remove(key);
        }
    }

    internal void OnHotkeyPressed(Key key)
    {
        if (_registeredHotkeys.ContainsKey(key))
        {
            HotkeyPressed?.Invoke(this, key);
        }
    }
} 