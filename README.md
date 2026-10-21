# Gaugr

**A lightweight Windows PC monitoring dashboard built with WPF and C#.**

Gaugr brings your PC's hardware readings and performance history into one clean, dark desktop interface. It is an actively developed project, with a focus on useful monitoring, readable visualizations, and a polished native Windows experience.

> **Status:** Early development. Features and hardware sensor support may vary by system.

## Features

- **Overview** — at-a-glance CPU and GPU temperatures, memory usage, and system-drive capacity.
- **Performance** — live CPU, GPU, and RAM history charts with 60-second and 5-minute views.
- **Hardware** — detected processor, graphics, memory, motherboard, clock, and fan information where available.
- **Storage** — capacity, free space, and usage for every detected drive.
- **Processes** — searchable process list with CPU and memory usage, sorting, and periodic refresh.
- **Hardware monitoring** — sensor readings collected with LibreHardwareMonitor.

Gaugr requests administrator rights on launch, because Windows only exposes CPU temperatures and fan speeds to elevated processes. When running from a terminal or Visual Studio, start that tool as administrator.

**CPU temperature needs the PawnIO driver.** Gaugr reads sensors through LibreHardwareMonitor, which uses the open-source [PawnIO](https://pawnio.eu) driver for low-level CPU access. If PawnIO is not installed, the CPU gauge shows "--" and asks you to install it. Install PawnIO once and restart Gaugr.

**Windows SmartScreen:** Gaugr's installer and executable are not code-signed yet, so Windows may warn that the publisher is unknown. Choose **More info → Run anyway** if you trust the download.

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
- [x] Add a dedicated storage page for all detected drives
- [ ] Add configurable monitoring and alert settings
- [ ] Improve release packaging and documentation

## Feedback

Bug reports, feature suggestions, and constructive feedback are welcome through GitHub Issues. Please include your Windows version and relevant hardware details when reporting a hardware-reading issue. Do not include private information in issue reports.

## License and permitted use

Gaugr is **proprietary software**, not an open-source-licensed project. The source is published for transparency and review. Under the [Gaugr Proprietary License](LICENSE), individuals may inspect, compile, and run Gaugr for personal, non-commercial use. Commercial use, redistribution, and broader reuse require prior written permission.

Third-party libraries have their own licenses; see their respective projects and package notices. The Gaugr license does not override those licenses.

For commercial licensing or other permissions, contact [@zaidwh0x](https://github.com/zaidwh0x).
