using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.UI.Controls;

public class UIValidatedTextField : UITextField{

    public new Color Line = Color.Teal;
    public new Color LineSelected = Color.LimeGreen;
    public Color ErrLine = Calc.HexToColor("db2323");
    public Color ErrLineSelected = Calc.HexToColor("ffbb33");

    public bool Error;
    private float errLerp;

    public UIValidatedTextField(Font font, int width, string input = "") : base(font, width, input) {}

    protected override void Initialize() {
        base.Initialize();
        errLerp = Error ? 1f : 0f;
    }

    public override void Update(Vector2 position = default) {
        errLerp = Calc.Approach(errLerp, Error ? 1f : 0f, Engine.DeltaTime * 7f);
        base.Line = Color.Lerp(Line, ErrLine, errLerp);
        base.LineSelected = Color.Lerp(LineSelected, ErrLineSelected, errLerp);

        base.Update(position);
    }
}