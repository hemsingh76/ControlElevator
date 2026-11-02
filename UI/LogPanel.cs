using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using ElevatorSimulation.Data;

namespace ElevatorSimulation.UI
{

    public class LogPanel : GroupBox
    {
        private readonly IEventRepository _repository;
        private readonly DataGridView _grid = new DataGridView();
        private readonly Button _clearButton = new Button();
        private readonly Label _statusLabel = new Label();
        private readonly BackgroundWorker _loadWorker = new BackgroundWorker();
        private readonly BackgroundWorker _clearWorker = new BackgroundWorker();

        private bool _ready;
        private bool _reloadAgain;

        public LogPanel(IEventRepository repository)
        {
            _repository = repository;
            Text = "Database records";

            _grid.Dock = DockStyle.Fill;
            _grid.ReadOnly = true;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AllowUserToResizeRows = false;
            _grid.RowHeadersVisible = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.BackgroundColor = Color.White;

            var gridHolder = new Panel();
            gridHolder.Dock = DockStyle.Fill;
            gridHolder.Controls.Add(_grid);

            var bottom = new Panel();
            bottom.Dock = DockStyle.Bottom;
            bottom.Height = 35;

            _clearButton.Text = "Clear log";
            _clearButton.Bounds = new Rectangle(0, 6, 80, 25);
            _clearButton.Click += OnClearClicked;

            _statusLabel.Bounds = new Rectangle(90, 6, 400, 25);
            _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            _statusLabel.Text = "Waiting for the database...";

            bottom.Controls.Add(_clearButton);
            bottom.Controls.Add(_statusLabel);

            Controls.Add(gridHolder);
            Controls.Add(bottom);

            _loadWorker.DoWork += (s, e) => e.Result = _repository.GetAll();
            _loadWorker.RunWorkerCompleted += OnLoadCompleted;

            _clearWorker.DoWork += (s, e) => _repository.DeleteAll();
            _clearWorker.RunWorkerCompleted += OnClearCompleted;
        }

        /// <summary>Call once the database is ready.</summary>
        public void SetReady()
        {
            _ready = true;
            Reload();
        }

        public void ShowMessage(string text)
        {
            _statusLabel.Text = text;
        }

        /// <summary>When the panel is shown, the records are loaded by themselves.</summary>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
            {
                Reload();
            }
        }

        /// <summary>Loads the records in the background. Safe to call at any time. Does nothing while the panel is hidden.</summary>
        public void Reload()
        {
            if (!_ready || !Visible)
            {
                return;
            }

            if (_loadWorker.IsBusy || _clearWorker.IsBusy)
            {
                _reloadAgain = true; // do it again as soon as the current job is finished
                return;
            }

            _loadWorker.RunWorkerAsync();
        }

        private void OnLoadCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                _statusLabel.Text = "Could not load the log: " + e.Error.Message;
            }
            else
            {
                var table = (DataTable)e.Result;
                _grid.DataSource = table;
                FormatGrid();
                _statusLabel.Text = table.Rows.Count + " record(s), newest first";
            }

            RunPendingReload();
        }

        private void FormatGrid()
        {
            if (_grid.Columns["Log ID"] != null)
            {
                _grid.Columns["Log ID"].Visible = false;
            }
            if (_grid.Columns["Event Time"] != null)
            {
                _grid.Columns["Event Time"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";
                _grid.Columns["Event Time"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            foreach (string name in new[] { "Elevator", "Current Floor", "Destination" })
            {
                if (_grid.Columns[name] != null)
                {
                    _grid.Columns[name].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
            }
            if (_grid.Columns["Event Description"] != null)
            {
                _grid.Columns["Event Description"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        private void OnClearClicked(object sender, EventArgs e)
        {
            if (!_ready || _clearWorker.IsBusy)
            {
                return;
            }

            DialogResult answer = MessageBox.Show(this, "Delete all log records?", "Clear log",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer == DialogResult.Yes)
            {
                _statusLabel.Text = "Deleting...";
                _clearWorker.RunWorkerAsync();
            }
        }

        private void OnClearCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                _statusLabel.Text = "Could not clear the log: " + e.Error.Message;
                RunPendingReload();
                return;
            }

            _reloadAgain = false;
            Reload();
        }

        private void RunPendingReload()
        {
            if (_reloadAgain)
            {
                _reloadAgain = false;
                Reload();
            }
        }
    }
}
