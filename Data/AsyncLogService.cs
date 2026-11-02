using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using ElevatorSimulation.Models;
using ElevatorSimulation.Patterns.Observers;

namespace ElevatorSimulation.Data
{

    public class AsyncLogService : IElevatorEventObserver
    {
        private const int MaxQueuedEvents = 10000;

        private readonly IEventRepository _repository;
        private readonly ConcurrentQueue<ElevatorEvent> _queue = new ConcurrentQueue<ElevatorEvent>();
        private readonly BackgroundWorker _worker = new BackgroundWorker();
        private bool _enabled;

        /// <summary>Raised on the GUI thread when saving failed.</summary>
        public event Action<string> WriteFailed;

        /// <summary>Raised on the GUI thread after a batch of events was saved successfully.</summary>
        public event Action Saved;

        public AsyncLogService(IEventRepository repository)
        {
            _repository = repository;
            _worker.DoWork += OnDoWork;
            _worker.RunWorkerCompleted += OnWorkerCompleted;
        }

        /// Call when the database is ready. Events collected until now are written
        public void Enable()
        {
            _enabled = true;
            StartWorkerIfNeeded();
        }

        public void ElevatorEventOccurred(ElevatorEvent elevatorEvent)
        {
            if (_queue.Count < MaxQueuedEvents)
            {
                _queue.Enqueue(elevatorEvent);
            }
            StartWorkerIfNeeded();
        }

        private void StartWorkerIfNeeded()
        {
            if (_enabled && !_worker.IsBusy && !_queue.IsEmpty)
            {
                _worker.RunWorkerAsync();
            }
        }

        // Runs on a background thread.
        private void OnDoWork(object sender, DoWorkEventArgs e)
        {
            _repository.InsertMany(TakeAll());
        }

        // Runs on the GUI thread again.
        private void OnWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Action<string> handler = WriteFailed;
                if (handler != null)
                {
                    handler("Saving the log failed: " + e.Error.Message);
                }
            }
            else
            {
                Action saved = Saved;
                if (saved != null)
                {
                    saved();
                }
            }
            StartWorkerIfNeeded();
        }

        /// Writes whatever is left in the queue on the calling thread. Used when the program closes
        public void FlushSynchronously()
        {
            if (!_enabled)
            {
                return;
            }

            try
            {
                _repository.InsertMany(TakeAll());
            }
            catch (DatabaseException)
            {
                // The program is closing; there is nobody left to tell.
            }
        }

        private List<ElevatorEvent> TakeAll()
        {
            var batch = new List<ElevatorEvent>();
            ElevatorEvent next;
            while (_queue.TryDequeue(out next))
            {
                batch.Add(next);
            }
            return batch;
        }
    }
}
