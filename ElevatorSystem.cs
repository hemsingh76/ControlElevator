using ElevatorSimulation.Core;
using ElevatorSimulation.Data;

namespace ElevatorSimulation
{

    public class ElevatorSystem
    {
        private readonly DatabaseConfig _databaseConfig;
        private readonly IElevator[] _elevators;
        private readonly ElevatorAnimator[] _animators;

        public IElevator[] Elevators { get { return _elevators; } }
        public ElevatorController Controller { get; }
        public IDatabaseInitializer DatabaseInitializer { get; }
        public IEventRepository Repository { get; }
        public AsyncLogService LogService { get; }

        public ElevatorSystem()
        {
            // Database side.
            _databaseConfig = new DatabaseConfig();
            var database = new AccessDatabase(_databaseConfig);
            DatabaseInitializer = new AccessDatabaseInitializer(_databaseConfig, database);
            Repository = new AccessEventRepository(database);
            LogService = new AsyncLogService(Repository);

            // Elevators: both start on the bottom floor. Each one gets its own animator (timer + delegate),

            _elevators = new IElevator[SimulationSettings.ElevatorCount];
            _animators = new ElevatorAnimator[SimulationSettings.ElevatorCount];
            for (int i = 0; i < SimulationSettings.ElevatorCount; i++)
            {
                var elevator = new Elevator(i + 1, 0);
                elevator.AttachEventObserver(LogService);   // the database logger observes the elevator
                _elevators[i] = elevator;
                _animators[i] = new ElevatorAnimator(elevator.Tick, SimulationSettings.TickMilliseconds);
            }

            Controller = new ElevatorController(_elevators, new ShortestResponseTimeScheduler());
        }


        public string DatabaseDescription
        {
            get { return "Microsoft Access (" + _databaseConfig.Provider + ")"; }
        }

        /// <summary>Announces the elevators to their observers and starts their animation timers.</summary>
        public void Start()
        {
            foreach (IElevator elevator in _elevators)
            {
                elevator.Start();
            }
            foreach (ElevatorAnimator animator in _animators)
            {
                animator.Start();
            }
        }

        /// <summary>Stops the timers and writes any events that are still waiting to be saved.</summary>
        public void Shutdown()
        {
            foreach (ElevatorAnimator animator in _animators)
            {
                animator.Stop();
                animator.Dispose();
            }
            LogService.FlushSynchronously();
        }
    }
}
