using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ElevatorSimulation.Core;
using ElevatorSimulation.Models;

namespace ElevatorSimulation.UI
{
    /// Draws one elevator shaft: two floors, the floor doors and the moving car with its doors
    public class ShaftView : Control
    {
        private const int FloorSpacing = 170;   // pixels between two floors
        private const int CarHeight = 117;
        private const int ShaftLeft = 55;       // free space on the left for the floor names
        private const int ShaftRight = 15;
        private const int ShaftTop = 24;

        private static readonly Color WallColor = Color.FromArgb(70, 80, 95);

        private readonly int _elevatorId;
        private ElevatorSnapshot _snapshot;

        public ShaftView(int elevatorId)
        {
            _elevatorId = elevatorId;
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.FromArgb(240, 242, 245);
        }

        public void UpdateSnapshot(ElevatorSnapshot snapshot)
        {
            _snapshot = snapshot;
            Invalidate();
        }

        // Y coordinate of the floor level (floor 0 is at the bottom).
        private int LevelY(int floor)
        {
            return Height - 30 - floor * FloorSpacing;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int shaftWidth = Width - ShaftLeft - ShaftRight;
            var shaft = new Rectangle(ShaftLeft, ShaftTop, shaftWidth, LevelY(0) - ShaftTop);

            using (var shaftBrush = new LinearGradientBrush(shaft, Color.FromArgb(200, 206, 215),
                       Color.FromArgb(236, 239, 243), LinearGradientMode.Horizontal))
            using (var wallPen = new Pen(WallColor, 2))
            using (var wallBrush = new SolidBrush(WallColor))
            using (var boldFont = new Font(Font, FontStyle.Bold))
            {
                g.DrawString("Elevator " + _elevatorId, boldFont, wallBrush, ShaftLeft, 3);

                g.FillRectangle(shaftBrush, shaft);
                g.DrawRectangle(wallPen, shaft);

                for (int floor = 0; floor < SimulationSettings.FloorCount; floor++)
                {
                    int y = LevelY(floor);
                    g.FillRectangle(wallBrush, 0, y, Width, 6);                       // the floor
                    g.DrawString("Floor " + floor, boldFont, wallBrush, 3, y - 20);   // its name
                    DrawFloorDoor(g, floor, y, shaftWidth);
                }
            }

            if (_snapshot != null)
            {
                DrawCar(g, shaftWidth);
            }
        }

        private void DrawFloorDoor(Graphics g, int floor, int levelY, int shaftWidth)
        {
            var area = new Rectangle(ShaftLeft + 4, levelY - CarHeight, shaftWidth - 8, CarHeight);

            double open = 0;
            if (_snapshot != null && Math.Abs(_snapshot.Position - floor) < 0.001)
            {
                open = _snapshot.DoorOpenness / 100.0;
            }

            int panelWidth = (int)(area.Width / 2.0 * (1 - open));
            using (var doorBrush = new SolidBrush(Color.FromArgb(105, 115, 130)))
            using (var linePen = new Pen(Color.FromArgb(50, 58, 70), 1))
            {
                g.FillRectangle(doorBrush, area.X, area.Y, panelWidth, area.Height);
                g.FillRectangle(doorBrush, area.Right - panelWidth, area.Y, panelWidth, area.Height);
                g.DrawRectangle(linePen, area);
                if (panelWidth > 0)
                {
                    int middle = area.X + area.Width / 2;
                    g.DrawLine(linePen, middle, area.Y, middle, area.Bottom);   // the gap between the two door halves
                }
            }
        }

        private void DrawCar(Graphics g, int shaftWidth)
        {
            int carBottom = (int)Math.Round(LevelY(0) - _snapshot.Position * FloorSpacing);
            var car = new Rectangle(ShaftLeft + 12, carBottom - CarHeight + 6, shaftWidth - 24, CarHeight - 12);
            var inside = new Rectangle(car.X + 4, car.Y + 4, car.Width - 8, car.Height - 4);

            double open = _snapshot.DoorOpenness / 100.0;
            int panelWidth = (int)(inside.Width / 2.0 * (1 - open));

            using (var bodyBrush = new LinearGradientBrush(car, Color.SteelBlue, Color.DarkSlateBlue, LinearGradientMode.Vertical))
            using (var insideBrush = new LinearGradientBrush(inside, Color.FromArgb(255, 252, 232),
                       Color.FromArgb(235, 225, 190), LinearGradientMode.Vertical))
            using (var doorBrush = new SolidBrush(Color.Silver))
            using (var edgePen = new Pen(Color.DimGray, 1))
            using (var borderPen = new Pen(Color.FromArgb(30, 40, 60), 2))
            {
                g.FillRectangle(bodyBrush, car);
                g.FillRectangle(insideBrush, inside);                                              // inside of the car
                g.FillRectangle(doorBrush, inside.X, inside.Y, panelWidth, inside.Height);         // elevator door (left half)
                g.FillRectangle(doorBrush, inside.Right - panelWidth, inside.Y, panelWidth, inside.Height);   // (right half)
                if (panelWidth > 0)
                {
                    g.DrawLine(edgePen, inside.X + panelWidth, inside.Y, inside.X + panelWidth, inside.Bottom);
                    g.DrawLine(edgePen, inside.Right - panelWidth, inside.Y, inside.Right - panelWidth, inside.Bottom);
                }
                g.DrawRectangle(borderPen, car);
            }

            // Movement indicator above the car.
            int centerX = car.X + car.Width / 2;
            if (_snapshot.Direction == Direction.Up)
            {
                g.FillPolygon(Brushes.ForestGreen, new[]
                {
                    new Point(centerX, car.Y - 15), new Point(centerX - 9, car.Y - 3), new Point(centerX + 9, car.Y - 3)
                });
            }
            else if (_snapshot.Direction == Direction.Down)
            {
                g.FillPolygon(Brushes.Firebrick, new[]
                {
                    new Point(centerX, car.Y - 3), new Point(centerX - 9, car.Y - 15), new Point(centerX + 9, car.Y - 15)
                });
            }
        }
    }
}
