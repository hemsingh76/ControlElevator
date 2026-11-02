using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ElevatorSimulation.Core;
using ElevatorSimulation.Models;
using ElevatorSimulation.Patterns.Observers;

namespace ElevatorSimulation.UI
{
   
    public class MainForm : Form, IElevatorChangeObserver, IRequestPanelObserver
    {
        private const int WidthWithoutLog = 600;
        private const int WidthWithLog = 1120;

        private readonly ElevatorSystem _system;

        // GUI controls
        private readonly ShaftView[] _shaftViews = new ShaftView[SimulationSettings.ElevatorCount];
        private readonly ElevatorControlPanel[] _controlPanels = new ElevatorControlPanel[SimulationSettings.ElevatorCount];
        // _requestButtons[elevatorIndex, floor] = the call button of that elevator on that floor
        private readonly Button[,] _requestButtons = new Button[SimulationSettings.ElevatorCount, SimulationSettings.FloorCount];
        private readonly CheckBox _autoAssignCheckBox = new CheckBox();
        private readonly Button _recordsButton = new Button();
        private readonly Label _databaseStatusLabel = new Label();
        private LogPanel _logPanel;

        public MainForm(ElevatorSystem system)
        {
            _system = system;

            Text = "Elevator Simulation";
            ClientSize = new Size(WidthWithoutLog, 615);
            BackColor = Color.FromArgb(240, 242, 245);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            BuildInterface();
            ConnectToSystem();

            Load += OnFormLoad;
            FormClosing += OnFormClosing;
        }

        //-------- building the window

        private void BuildInterface()
        {
            // Each elevator column: floor request buttons | shaft with the car | control panel below the shaft.
            for (int i = 0; i < SimulationSettings.ElevatorCount; i++)
            {
                int elevatorId = i + 1;
                int columnLeft = 10 + i * 293;

                for (int f = 0; f < SimulationSettings.FloorCount; f++)
                {
                    int floor = f;
                    var button = new Button();
                    button.Text = "Call" + Environment.NewLine + "floor " + floor;
                    button.Font = new Font(Font, FontStyle.Bold);
                    button.Size = new Size(90, 45);
                    button.Location = new Point(columnLeft, f == 1 ? 124 : 299);   // floor 1 on top, floor 0 at the bottom
                    ButtonStyle.Apply(button);
                    button.Click += (sender, args) => _system.Controller.PressExternalButton(elevatorId, floor);
                    _requestButtons[i, f] = button;
                    Controls.Add(button);
                }

                var shaft = new ShaftView(elevatorId);
                shaft.Bounds = new Rectangle(columnLeft + 95, 10, 190, 400);
                _shaftViews[i] = shaft;
                Controls.Add(shaft);

                var panel = new ElevatorControlPanel(elevatorId);
                panel.Location = new Point(columnLeft + 95, 415);
                panel.FloorButtonPressed += floor => _system.Controller.PressInternalButton(elevatorId, floor);
                _controlPanels[i] = panel;
                Controls.Add(panel);
            }

            // The database records. Hidden until the button is pressed.
            _logPanel = new LogPanel(_system.Repository);
            _logPanel.Bounds = new Rectangle(615, 10, 495, 525);
            _logPanel.Visible = false;
            Controls.Add(_logPanel);

            _recordsButton.Text = "Show database records";
            _recordsButton.Font = new Font(Font, FontStyle.Bold);
            _recordsButton.Bounds = new Rectangle(10, 545, 200, 30);
            ButtonStyle.Apply(_recordsButton);
            _recordsButton.Click += OnRecordsButtonClick;
            Controls.Add(_recordsButton);

            _autoAssignCheckBox.Text = "Auto-assign calls to the nearest elevator";
            _autoAssignCheckBox.Bounds = new Rectangle(225, 545, 365, 30);
            _autoAssignCheckBox.CheckedChanged += (sender, args) => _system.Controller.AutoAssign = _autoAssignCheckBox.Checked;
            Controls.Add(_autoAssignCheckBox);

            _databaseStatusLabel.Bounds = new Rectangle(10, 580, WidthWithoutLog - 20, 30);
            _databaseStatusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _databaseStatusLabel.ForeColor = Color.DimGray;
            _databaseStatusLabel.Text = "Database: not started";
            Controls.Add(_databaseStatusLabel);
        }

        /// <summary>Registers this window as an observer. After this, the elevators push changes to the window.</summary>
        private void ConnectToSystem()
        {
            foreach (IElevator elevator in _system.Elevators)
            {
                elevator.AttachChangeObserver(this);
            }
            _system.Controller.Attach(this);

            _system.LogService.WriteFailed += SetDatabaseStatus;
            _system.LogService.Saved += () => _logPanel.Reload();   // new records appear by themselves
        }

        // ------------------------------------------------------------------ start / stop

        private void OnFormLoad(object sender, EventArgs e)
        {
            StartDatabaseSetup();
            _system.Start();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            _system.Shutdown();
        }

        /// <summary>Creates/opens the Access file in a BackgroundWorker so the window never freezes.</summary>
        private void StartDatabaseSetup()
        {
            SetDatabaseStatus("Database: preparing...");

            var worker = new BackgroundWorker();
            worker.DoWork += (s, args) => _system.DatabaseInitializer.Initialize();
            worker.RunWorkerCompleted += (s, args) =>
            {
                if (args.Error != null)
                {
                    SetDatabaseStatus("Database ERROR: " + args.Error.Message);
                    _logPanel.ShowMessage("Database not available - events are not saved.");
                    return;
                }

                SetDatabaseStatus("Database ready: " + _system.DatabaseDescription);
                _system.LogService.Enable();   // writes everything collected so far
                _logPanel.SetReady();          // loads the grid if it is visible
            };
            worker.RunWorkerAsync();
        }

        private void SetDatabaseStatus(string text)
        {
            _databaseStatusLabel.Text = text;
        }

        /// <summary>Shows or hides the database records. When shown, they load and update by themselves.</summary>
        private void OnRecordsButtonClick(object sender, EventArgs e)
        {
            bool show = !_logPanel.Visible;

            ClientSize = new Size(show ? WidthWithLog : WidthWithoutLog, ClientSize.Height);
            _logPanel.Visible = show;   // showing the panel starts loading the records
            _recordsButton.Text = show ? "Hide database records" : "Show database records";
        }

        // ------------------------------------------------------------------ Observer callbacks

        public void ElevatorChanged(ElevatorSnapshot snapshot)
        {
            int index = snapshot.ElevatorId - 1;
            _shaftViews[index].UpdateSnapshot(snapshot);
            _controlPanels[index].UpdateFrom(snapshot);
        }

        public void ExternalLightsChanged(int elevatorId, bool[] lights)
        {
            // The built-in light of a call button is on until that elevator arrives.
            for (int f = 0; f < lights.Length; f++)
            {
                ButtonStyle.SetLit(_requestButtons[elevatorId - 1, f], lights[f]);
            }
        }
    }
}
