# Smart X Trucks - Logistical Management System
<br />
Youtube : https://youtu.be/dD5ClByrhNY
<br />
PLEASE NOTE: Last moment I realized that my application was missing a stack command history data structure. Once has been incorporated but the demonstration video does not reflect this change
Smart X Trucks is a Dashboard management system revolving trucks and their respective sensor conditions. The dashboard allows users to view a live location and historical GPS data while keeping track of crucial truck conditions such as oil level, tire pressure, speed and fuel amount.

---

## Table of Contents

- [Set-up Instructions](#set-up-instructions)
- [Features](#features)
- [Part 2 Data Structures](#part-2-data-structures)
- [Technical Language Requirements (Part 1)](#technical-language-requirements)
- [References](#references)
- [Declaration of AI](#declaration-of-ai)


---

## Set-up Instructions
Run:
<br /> dotnet restore SmartX.sln to restore dependencies
<br /> 
<br /> 
Run:
<br /> dotnet build SmartX.sln to compile solution.

<br /> 

### Startup
Under configure startup projects, choose Multiple startup projects and choose SmartX.Server and SmartXClient to Start
<br /> 
<br />

## Features

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

## Part 2 Data Structures

| Data Structure | Class | Description |
|---|---|---|
| Priority Queue| EmergencyAlertQueue.cs | orders active dispatch alerts so critical risks appear before lower-severity alerts, with truck ID used to break ties. |
|Dictionary | TruckSensorStateService.cs | maps a truck ID to its telemetry for direct lookups. |
|HashSet | DisconnectedSensorTracker.cs | tracks unique disconnected sensor nodes and detected alert types, helping avoid duplicate alerts and identify recovered conditions. |

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





## Declaration of AI
### Part 1
### https://share.gemini.google/aQmntGv1VmKD 
<img width="942" height="742" alt="image" src="https://github.com/user-attachments/assets/44050356-86b2-4e14-a94b-e39739e5a32d" />

### Part 2
### https://share.gemini.google/QmMLMrJlXoZF
<img width="922" height="821" alt="image" src="https://github.com/user-attachments/assets/c260b318-7f5f-4c50-8e40-ed571f7ea6ab" />


