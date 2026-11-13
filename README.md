# Smart X Trucks - Logistical Management System

Smart-X is a .NET fleet-management demonstration for truck telemetry, sensor status, emergency alerts, fuel-range prediction, historical routes, and IoT mesh topology. The repository contains a Blazor WebAssembly client, an ASP.NET Core API, shared models and services, and an xUnit test project.

> **Demo:** ([Watch The Summative Demo](https://youtu.be/ZadKlynpFNI)) or the [Part 2 Demo](https://youtu.be/dD5ClByrhNY). 
>
> **Important:** This is a demonstration application. Fleet telemetry, mesh nodes, link costs, and ML training examples are simulated. The application does not connect to physical truck or ESP32 hardware.

---

## Table of Contents

- [Requirements](#requirements)
- [Build and run](#build-and-run)
- [Features](#features)
- [Data structures](#data-structures)
- [Prediction model](#prediction-model)
- [References](#references)
- [Declaration of AI](#declaration-of-ai)

## Requirements

- Windows, macOS, or Linux.
- The **.NET 10 SDK**. Confirm with `dotnet --list-sdks`.
- Internet access for the initial NuGet package restore.
- For Visual Studio, open `SmartX.Client\SmartX.Client.slnx` and ensure the .NET development tools are installed.

The projects target `net10.0`. Installing only a .NET runtime is not sufficient to build them; install the .NET 10 SDK.

## Build and run

Run commands from the repository root (the directory containing this README).

### Restore, build, and test
```powershell
dotnet restore .\SmartX.Client\SmartX.Client.slnx
dotnet build .\SmartX.Client\SmartX.Client.slnx
dotnet test .\SmartX.Tests\SmartX.Tests.csproj
```

The solution contains `SmartX.Client`, `SmartX.Server`, `SmartX.Shared`, and `SmartX.Tests`. Build and test must complete successfully before launching the app.

### Run from Visual Studio

1. Open `SmartX.Client\SmartX.Client.slnx`.
2. In startup settings, choose **Multiple startup projects**.
3. Set both `SmartX.Server` and `SmartX.Client` to **Start**.
4. Select the **https** launch profile for each project and start debugging.
5. If Visual Studio prompts about the local HTTPS development certificate, trust it.

The client API is configured to contact the server at `https://localhost:7044/`. The matching HTTPS launch profiles are configured in each project's `Properties\launchSettings.json`.

### Run from a terminal

Open two terminals at the repository root. Start the server first:

```powershell
dotnet run --project .\SmartX.Server\SmartX.Server.csproj --launch-profile https
```
Then start the client in the second terminal:
```powershell
dotnet run --project .\SmartX.Client\SmartX.Client.csproj --launch-profile https
```
The client opens at `https://localhost:7041`. Keep both terminal processes running while using the app. If local HTTPS certificates are not trusted, run `dotnet dev-certs https --trust` and restart the applications.

## Features

### Graphical Visualization of Nodes & Gateway
<img width="1542" height="856" alt="image" src="https://github.com/user-attachments/assets/a8e04d36-4c6b-4eaa-96e4-11808e2c3022" />

### Historical Data Querying
<img width="1507" height="797" alt="image" src="https://github.com/user-attachments/assets/adbf7c36-7f64-4c1e-bb1c-b83387fb75ac" />


### Vehicle Sensor Configuration & Upload
<img width="1876" height="896" alt="image" src="https://github.com/user-attachments/assets/e8ec02cd-62ea-45ac-9eed-982c81c2aa7b" />


### Fuel Range Prediction
<img width="1502" height="202" alt="image" src="https://github.com/user-attachments/assets/520d200a-ea98-44ca-a5f3-78833f9c7544" />


### Priority Alert System
<img width="1490" height="622" alt="image" src="https://github.com/user-attachments/assets/3b9d1078-33b1-424f-af8c-49fb2ba55ce9" />

### Live & Historical Geospatial Tracking
<img width="1497" height="817" alt="image" src="https://github.com/user-attachments/assets/61229613-19d7-437b-a931-3ba2a2d82f72" />

### Environmental and Truck Sensor Dashboard
<img width="1527" height="501" alt="image" src="https://github.com/user-attachments/assets/dc3b8de0-f43e-48ab-90c2-fdf3a24cf118" />

## Data Structures


| Data Structure | Project class/location | Role in the application |
|---|---|---|
| Dictionary and adjacency-list graph | `SmartX.Shared/Services/MeshRoutePlanner.cs` | Maps node IDs (sensor MACs or gateway ID) to node models and neighboring links. Provides efficient node lookup and graph traversal. |
| Queue and HashSet for BFS | `MeshRoutePlanner.PerformBfsReachability` | A FIFO queue explores neighbors level by level; a HashSet tracks visited nodes so each online node is processed once. |
| PriorityQueue and HashSet for Prim | `MeshRoutePlanner.CalculateMinimumSpanningTreePrims` | Prioritizes low-cost links and tracks nodes already added to the spanning backbone. |
| AVL tree, indexed per sensor | `SmartX.Shared/Services/HistoricalTelemetryIndex.cs` | Stores readings by UTC timestamp for range queries. Inserting the same sensor MAC and timestamp replaces the existing reading. |
| Concurrent dictionaries | `SmartX.Server/Services/TelemetryHistoryStore.cs` | Stores a bounded recent history per truck, keyed by timestamp, for safe concurrent access during ingestion. |
| Priority queue and dictionary | `SmartX.Shared/Services/EmergencyAlertQueue.cs` | Orders active alerts by severity and truck ordering; a dictionary maps alert IDs to the current active alert. |
| HashSet | `SmartX.Shared/Services/DisconnectedSensorTracker.cs` | Tracks unique disconnected sensor/truck pairs, preventing duplicate disconnect alerts and allowing recovery to clear the state. |
| Dictionary | `SmartX.Client/Services/TruckSensorStateService.cs` | Provides direct lookup of a truck's current demo telemetry by truck ID. |
| Stack | `SmartX.Client/Services/CommandHistoryService.cs` | Records recent workspace and filter actions in last-in, first-out order, with a 50-entry limit. It is navigation history, not undo/redo. |

The topology API is implemented in `SmartX.Server/Controllers/TopologyController.cs`. Client HTTP calls are grouped in `SmartX.Client/Services/SmartXApiClient.cs`; the UI is in `SmartX.Client/Pages/NetworkTopology.razor`. Telemetry ingestion also inserts readings into the history AVL index through `SmartX.Server/Controllers/TelemetryController.cs`.

## Prediction model

`SmartX.Server/Services/FuelPredictionService.cs` trains an ML.NET **FastTree regression** model when the service is created. Its inputs are speed, current fuel, and payload weight; the output is an estimated remaining range in kilometres. The training examples and range labels are generated synthetically for demonstration purposes. Stationary trucks and trucks with no fuel are handled as 0 km before model prediction. This model is not a calibrated prediction for a particular real truck.

---

## Technical Language Requirements
### Telementry Packet
<img width="1267" height="445" alt="image" src="https://github.com/user-attachments/assets/acd11f59-5a9d-4267-86cb-d27d6fcad920" />
<br /> 
<br /> 

### Operator Overloading
<img width="857" height="195" alt="image" src="https://github.com/user-attachments/assets/b0e97743-ce34-4d83-ad0b-b553521cb023" />
<br /> 
<br /> 

### Advanced Arrays & Lists
<img width="1150" height="567" alt="image" src="https://github.com/user-attachments/assets/d6892339-3276-42e7-800b-aec1408ad62a" />
<br /> 
<br /> 

### Recursion
<img width="757" height="480" alt="image" src="https://github.com/user-attachments/assets/fd8ce663-f3b1-477a-918a-7db3cf1e1d51" />
<br /> 
<br /> 

## References
Revolutionize Your Coding: C# Machine Learning Essentials with ML.NET! 2024. YouTube video, added by Nick Proud. [Online]. Available at: https://www.youtube.com/watch?v=R3kRf7hNVMg [Accessed 14 October 2026].

Microsoft Learn. N/A. PriorityQueue<TElement,TPriority> Class. [Online]. Available at: https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.priorityqueue-2?view=net-8.0 [Accessed 13 October 2026].

Microsoft Learn. N/A. HashSet<T> Class. [Online]. Available at: https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1?view=net-8.0 [Accessed 14 October 2026].

Microsoft Learn. N/A. Train and evaluate a model. [Online]. Available at: https://learn.microsoft.com/en-us/dotnet/machine-learning/how-to-guides/train-machine-learning-model-ml-net [Accessed 14 October 2026].

Ghori Ketan. 2025. Bringing AI to Your .NET Apps with ML.NET — Part 1: Predicting Customer Churn with FastTree. [Online]. https://medium.com/@ghoriketan33/bringing-ai-to-your-net-apps-with-ml-net-part-1-predicting-customer-churn-with-fasttree-312b13c498e6 [Accessed 14 October 2026].

GeeksforGeeks (2026). Prim’s algorithm for minimum spanning tree (MST). [online] GeeksforGeeks. Available at: https://www.geeksforgeeks.org/dsa/prims-minimum-spanning-tree-mst-greedy-algo-5/ [Accessed 13 Nov. 2026].

Skilled.dev (n.d.). Breadth-first search in C#. [online] Skilled.dev. Available at: https://skilled.dev/course/breadth-first-search/csharp [Accessed 13 Nov. 2026].

Varsity Tutors (n.d.). Kruskal’s and prim’s algorithms (conceptual). [online] Varsity Tutors. Available at: https://www.varsitytutors.com/learner/subjects/discrete-math/lessons/kruskals-and-prims-algorithms [Accessed 13 Nov. 2026].






## Declaration of AI
### Part 1
### https://share.gemini.google/aQmntGv1VmKD 
<img width="942" height="742" alt="image" src="https://github.com/user-attachments/assets/44050356-86b2-4e14-a94b-e39739e5a32d" />

### Part 2
### https://share.gemini.google/QmMLMrJlXoZF
<img width="922" height="821" alt="image" src="https://github.com/user-attachments/assets/c260b318-7f5f-4c50-8e40-ed571f7ea6ab" />

### Part 3
### https://share.gemini.google/R0oOsJKGzWnJ
<img width="692" height="710" alt="image" src="https://github.com/user-attachments/assets/f1cb92bd-ea4d-4948-8376-a3a969971efe" />

