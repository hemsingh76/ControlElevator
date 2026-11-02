# Two-Elevator Simulation

A small C# Windows Forms app that simulates two elevators in a two-floor building. Each floor has call buttons, each elevator has its own control panel, and every event is saved to a Microsoft Access database and shown in a log on screen.

## Requirements

- Windows 10 or 11
- Visual Studio 2022 (Community is fine) with the ".NET desktop development" workload. The project targets .NET Framework 4.8.
- Microsoft Access Database Engine (ACE OLE DB driver). If you already have Office or Access you're set. Otherwise install the free "Microsoft Access Database Engine 2016 Redistributable" from Microsoft.

The app and the Access driver need to be the same bitness. The project builds as AnyCPU (64-bit). If you have 32-bit Office, open `ElevatorSimulation.csproj` and change the platform settings to:

```xml
<PlatformTarget>x86</PlatformTarget>
```

No NuGet packages needed.

## Running it

1. Unzip the project somewhere, e.g. `C:\Projects\ElevatorSimulation`.
2. Open `ElevatorSimulation.sln` in Visual Studio.
3. Press F5 (or Ctrl+F5 to run without the debugger).

Or from a Developer Command Prompt:

```
cd ElevatorSimulation
dotnet build -c Release
bin\Release\net48\ElevatorSimulation.exe
```

## Database

There's nothing to set up. On first launch the app creates `Data\ElevatorLog.accdb` next to the .exe, along with the `ElevatorLog` table. The status line at the bottom right says "Database ready" or shows the error.

If it can't create the file, make an empty Access database called `ElevatorLog.accdb` in that `Data` folder and the app will add the table. As a last resort, run `Database\schema.sql` in Access.

If you see "provider is not registered", the Access Database Engine is either missing or the wrong bitness (see Requirements).

## Using the app

- **Call buttons** (left of each shaft): call that elevator to a floor. The button stays yellow until the doors open there.
- **Control panel** (under each shaft): press 0 or 1 to send that elevator to a floor. The display shows the current floor and whether the car is moving up, moving down, or stopped.
- **Auto-assign checkbox**: when ticked, any call button goes to a scheduler that picks whichever elevator can arrive first.
- **Show database records**: opens a grid with the newest 500 log entries, which updates live. "Clear log" deletes all records.

Requests that come in mid-trip wait in that elevator's queue and don't interrupt it.

Quick test: press "Call floor 1" next to each elevator and watch both go up. Then tick auto-assign, call floor 0 while one car is busy, and the free one should answer.

## Project layout

```
Models/      data classes (events, snapshots)
Core/        elevator, controller, scheduler, settings
Patterns/    State and Observer pattern classes
Data/        Access database helpers and async logging
UI/          MainForm and custom controls
Database/    schema.sql (manual fallback only)
```

Speeds and door timings can be changed in `Core/SimulationSettings.cs`.