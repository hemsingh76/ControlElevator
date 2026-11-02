using System;
using System.Drawing;
using System.Windows.Forms;
using ElevatorSimulation.Core;
using ElevatorSimulation.Models;

namespace ElevatorSimulation.UI
{
    /// <summary>The control panel inside one car: display window, movement indicator and floor buttons.</summary>
    public class ElevatorControlPanel : GroupBox
    {
        private readonly Label _floorDisplay = new Label();
        private readonly Label _movementLabel = new Label();
        private readonly Button[] _floorButtons = new Button[SimulationSettings.FloorCount];

        /// <summary>Raised with the floor number when a floor button inside the car is pressed.</summary>
        public event Action<int> FloorButtonPressed;

        public ElevatorControlPanel(int elevatorId)
        {
            Text = "Elevator " + elevatorId + " control panel";
            Size = new Size(190, 120);

            // Display window: shows the current floor.
            _floorDisplay.Bounds = new Rectangle(10, 22, 60, 45);
            _floorDisplay.BackColor = Color.FromArgb(20, 25, 30);
            _floorDisplay.ForeColor = Color.LimeGreen;
            _floorDisplay.BorderStyle = BorderStyle.Fixed3D;
            _floorDisplay.Font = new Font(FontFamily.GenericMonospace, 22, FontStyle.Bold);
            _floorDisplay.TextAlign = ContentAlignment.MiddleCenter;
            _floorDisplay.Text = "0";
            Controls.Add(_floorDisplay);

            _movementLabel.Bounds = new Rectangle(80, 22, 100, 45);
            _movementLabel.Font = new Font(Font, FontStyle.Bold);
            _movementLabel.TextAlign = ContentAlignment.MiddleLeft;
            _movementLabel.Text = "Stopped";
            Controls.Add(_movementLabel);

            for (int f = 0; f < SimulationSettings.FloorCount; f++)
            {
                int floor = f; // copy, so every button remembers its own floor
                var button = new Button();
                button.Text = floor.ToString();
                button.Font = new Font(Font.FontFamily, 12, FontStyle.Bold);
                button.Size = new Size(80, 38);
                button.Location = new Point(10 + f * 90, 74);
                ButtonStyle.Apply(button);
                button.Click += (sender, args) =>
                {
                    Action<int> handler = FloorButtonPressed;
                    if (handler != null)
                    {
                        handler(floor);
                    }
                };
                _floorButtons[f] = button;
                Controls.Add(button);
            }
        }

        /// <summary>Refreshes the panel from the latest snapshot sent by the elevator (Observer pattern).</summary>
        public void UpdateFrom(ElevatorSnapshot snapshot)
        {
            _floorDisplay.Text = snapshot.CurrentFloor.ToString();

            switch (snapshot.Direction)
            {
                case Direction.Up:
                    _movementLabel.Text = "Moving up";
                    _movementLabel.ForeColor = Color.ForestGreen;
                    break;
                case Direction.Down:
                    _movementLabel.Text = "Moving down";
                    _movementLabel.ForeColor = Color.Firebrick;
                    break;
                default:
                    _movementLabel.Text = "Stopped";
                    _movementLabel.ForeColor = Color.DimGray;
                    break;
            }

            // The built-in light of a button is on until the elevator reaches that floor.
            for (int f = 0; f < _floorButtons.Length; f++)
            {
                ButtonStyle.SetLit(_floorButtons[f], snapshot.PanelLights[f]);
            }
        }
    }
}
