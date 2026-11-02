using System.Drawing;
using System.Windows.Forms;

namespace ElevatorSimulation.UI
{
    /// <summary>One place for the look of the elevator buttons: flat, white, blue border, yellow when lit.</summary>
    internal static class ButtonStyle
    {
        private static readonly Color Normal = Color.White;
        private static readonly Color Lit = Color.Gold;
        private static readonly Color Border = Color.SteelBlue;
        private static readonly Color Text = Color.FromArgb(40, 60, 90);

        public static void Apply(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Border;
            button.FlatAppearance.BorderSize = 2;
            button.UseVisualStyleBackColor = false;
            button.BackColor = Normal;
            button.ForeColor = Text;
            button.Cursor = Cursors.Hand;
        }

        /// <summary>The "built-in light" of a button: yellow while the request is waiting.</summary>
        public static void SetLit(Button button, bool lit)
        {
            button.BackColor = lit ? Lit : Normal;
        }
    }
}
