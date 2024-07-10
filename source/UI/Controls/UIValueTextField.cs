using System;
using System.Collections.Generic;

namespace Snowberry.UI.Controls;

public class UIValueTextField<T> : UIValidatedTextField {

    public Action<T> OnValidInputChange;
    public new T Value { get; private set; }

    private static readonly HashSet<char> integerChars = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-'];
    private static readonly HashSet<char> floatChars = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-', '.', ',', 'e'];

    public UIValueTextField(Font font, int width, string input = "") : base(font, width, input) {
        CharacterWhitelist = Type.GetTypeCode(typeof(T)) switch {
            TypeCode.Int32 or TypeCode.Int64 => integerChars,
            TypeCode.Single or TypeCode.Double => floatChars,
            _ => null
        };
    }

    protected override void OnInputUpdate(string input) {
        base.OnInputUpdate(input);
        try{
            Value = (T)Convert.ChangeType(input, typeof(T));
            OnValidInputChange?.Invoke(Value);
            Error = false;
        }catch{
            Value = default;
            Error = true;
        }
    }
}