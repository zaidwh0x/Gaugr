# Gaugr

**A lightweight Windows PC monitoring dashboard built with WPF and C#.**

Gaugr brings your PC's hardware readings and performance history into one clean, dark desktop interface. It is an actively developed project, with a focus on useful monitoring, readable visualizations, and a polished native Windows experience.

> **Status:** Early development. Features and hardware sensor support may vary by system.

## Features

- **Overview** — at-a-glance CPU and GPU temperatures, memory usage, and system-drive capacity.
- **Performance** — live CPU, GPU, and RAM history charts with 60-second and 5-minute views.
- **Hardware** — detected processor, graphics, memory, motherboard, clock, and fan information where available.
- **Processes** — searchable process list with CPU and memory usage, sorting, and periodic refresh.
- **Hardware monitoring** — sensor readings collected with LibreHardwareMonitor.

Some readings depend on hardware, drivers, and sensor support. Gaugr does not yet expose every metric on every PC.

## Built with

- C# and .NET 10
- WPF and XAML
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)
- [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor)
- [LiveCharts2](https://github.com/Live-Charts/LiveCharts2)

## Getting started

### Requirements

- Windows
- Visual Studio with the **.NET desktop development** workload
- The .NET 10 SDK

### Run from Visual Studio

1. Clone or download this repository.
2. Open `Gaugr/Gaugr.slnx` in Visual Studio.
3. Allow NuGet packages to restore.
4. Set `Gaugr.App` as the startup project if needed.
5. Build and run the app.

### Build from the command line

From the repository root:

```powershell
dotnet build .\Gaugr\Gaugr.App\Gaugr.App.csproj
dotnet run --project .\Gaugr\Gaugr.App\Gaugr.App.csproj
```

## Project structure

```text
Gaugr/
└── Gaugr/
    ├── Gaugr.slnx
    └── Gaugr.App/
        ├── Controls/       # Reusable UI controls
        ├── Services/       # Hardware and process sampling
        ├── ViewModels/     # UI state and presentation logic
        ├── Views/          # WPF pages
        ├── App.xaml
        └── MainWindow.xaml
```

## Roadmap

- [ ] Improve missing/unavailable sensor handling
- [ ] Refine temperature chart scaling and metric presentation
- [ ] Add a dedicated storage page for all detected drives
- [ ] Add configurable monitoring and alert settings
- [ ] Improve release packaging and documentation

## Contributing

Gaugr is being developed in the open as a personal software project. Bug reports, ideas, and constructive feedback are welcome. If you report a hardware-reading issue, include your Windows version and relevant hardware details.

## License

No license has been added yet. Until a license is included, all rights remain with the copyright holder and reuse is not automatically permitted.
