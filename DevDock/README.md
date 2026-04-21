# DevDock

DevDock is a Windows desktop productivity application built in **C# with WPF on .NET 8** using the **MVVM pattern**. It brings several everyday productivity tools into one place, including task management, reminders, a Pomodoro timer, a music player, code snippet storage, and notes.

## Overview

The goal of this project was to create a clean, multi-feature desktop app that practices both **software structure** and **user-focused design**. Rather than building a single isolated feature, this app combines several related tools into one consistent workspace.

This project was also an opportunity to strengthen skills in:

- desktop application development with WPF and .NET 8
- UI design and layout with XAML
- organizing code with MVVM
- separating feature logic into view models and services
- state and data management across multiple app sections
- debugging interactions between UI, commands, persistence, and underlying logic
- designing features that feel cohesive rather than disconnected

## Platform

This project is built as a **WPF desktop application** and is intended to run on **Windows**.

## Features

- **Task management**
  - Create, edit, move, and delete tasks
  - Organize tasks in stacked Kanban-style lanes:
    - Backlog
    - To-Do
    - In Progress
    - Code Review
    - Completed
  - Track due dates, priority, estimated completion time, and status
  - Support for subtasks, including completion tracking
  - Edit tasks through a dedicated edit window
  - Persist task and subtask data locally with SQLite and Entity Framework Core

- **Reminders**
  - Displays a banner for items due today
  - Shows all due-today items when there are only a few
  - Simplifies the message when several items are due
  - Allows the reminder banner to be dismissed
  - Updates dynamically based on current task state

- **Pomodoro timer**
  - Supports focus, short break, and long break sessions
  - Uses command-based controls for start, pause, and reset
  - Keeps timer logic separate from shell and navigation logic
  - Structured so timer behavior can later connect to notifications and sound alerts

- **Music player**
  - Local music playback from the app’s `Assets/Music` folder
  - Previous, play/pause, and next track controls
  - True pause/resume behavior instead of restarting the song on every play click
  - Track metadata managed through a JSON file for reliable title, artist, album, and artwork display

- **Code snippets**
  - Store reusable code snippets inside the app
  - Add, edit, search, and delete snippets
  - Edit both snippet titles and content
  - Designed as a lightweight reference space for frequently used code
  - Persisted locally with SQLite and Entity Framework Core

- **Notes**
  - Dedicated notes section for lightweight text storage
  - Notes are saved locally and reload when the app is reopened
  - Included as another integrated productivity surface within the same application

- **System tray behavior**
  - Includes tray support so the app behaves more like a desktop utility
  - Uses a tray service to manage hide/show behavior and quick reopening
  - Built around a compact popup-style workflow rather than a traditional large desktop window

## Tech Stack

- **Language:** C#
- **Framework:** WPF
- **Runtime:** .NET 8
- **Architecture:** MVVM
- **UI:** XAML
- **IDE:** Visual Studio
- **Desktop Features:** `DispatcherTimer`, `NotifyIcon`, `MediaPlayer`
- **Persistence:** SQLite with Entity Framework Core
- **Data Format:** JSON for music metadata

## Project Structure

```text
DevDock/
├── Assets/         # icons, sounds, music, and other bundled app assets
├── Commands/       # reusable command classes such as RelayCommand
├── Data/           # EF Core database context and persistence setup
├── Models/         # data models like tasks, subtasks, notes, snippets, and tracks
├── Services/       # feature services such as tray, storage, sound, and music playback
├── ViewModels/     # UI-facing logic for each feature
├── Views/          # XAML windows and feature views
└── ...
```

## Design Choices
This project uses the MVVM (Model-View-ViewModel) pattern to keep the application organized and maintainable.

- Models store the app's data structures
- Views define the user interface in XAML
- ViewModels handle presentation logic and data binding
- Services manage non-UI behavior such as playback, timers, and persistence
- Commands handle user-triggered actions in a clean, reusable way

A key design decision in this project was to avoid putting all feature logic directly inside `MainViewModel`. Instead:

- `MainViewModel` manages top-level navigation and app-wide coordination
- feature-specific view models manage their own feature state and commands
- services handle non-UI responsibilities such as storage, playback, and tray behavior

### Task Board Design
The task system was designed around a stacked-lane board instead of a traditional table layout. This made it easier to visually separate workflow states while still keeping the board compact enough for a tray-style productivity app.

Tasks are stored with:

- title
- description
- due date
- priority
- estimated minutes
- status
- subtasks

Subtasks are persisted alongside tasks, and the task card UI reflects subtask completion progress. A dedicated edit window was added so task editing would not overcrowd the main popup interface.

### Reminder Design
Reminder logic is driven by task state rather than static text. The reminder banner checks which tasks are due on the current day and adjusts its message depending on how many items are due. This made the reminder system more useful and also helped the task board and app shell feel more connected.

### Pomodoro Timer Design
The Pomodoro timer was designed with its own dedicated view model so the timer state, mode switching, and command logic are isolated from the rest of the application. This keeps concerns separated and makes it easier to later add:

- notifications
- completion sounds
- automatic session switching
- custom timer lengths

### Code Snippets Design
The snippet system was designed differently from notes because snippets benefit from structure. Each snippet is stored as its own record with a title and content, which allows the app to support selection, editing, deletion, and search. This makes the feature more practical as a small personal reference library instead of just another plain text box.

### Notes Design
The notes feature was intentionally kept simple. Instead of managing multiple separate note files, the app stores a single notes record that can be edited directly from the main interface. This kept the feature lightweight while still making it useful as a quick scratch space within the app.

### Music Player Design
The music player was structured around a dedicated music service and track model rather than raw file names alone. This made it easier to support:

- track navigation
- true play/pause behavior
- display of title and artist
- album art
- metadata loaded from a curated JSON file

Using a JSON metadata file instead of relying only on MP3 metadata was a deliberate design choice because local files may not always contain consistent embedded metadata.

### Asset Management Design
Assets such as tray icons, sound files, and music files are stored in the project’s Assets folder and copied into the output directory through the project configuration. This made runtime file loading more reliable and was important for getting features like the tray icon and sound playback working correctly.

### Persistence Design
Task data is persisted locally using [**SQLite with Entity Framework Core**](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/?tabs=dotnet-core-cli). This was a stronger fit than simple file-based storage because the app manages related structured data such as tasks and subtasks, and it made updating, reloading, and organizing task state more reliable as the feature set grew.

## Challenges and Improvements
A major part of development involved refining feature behavior and fixing edge cases across connected parts of the app. Some of the key areas worked through included:

- stabilizing task editing behavior
- fixing bugs involving subtask additions not saving correctly
- preventing existing subtasks from disappearing after edits
- resolving EF Core persistence issues around updating child subtask collections
- improving reminder display logic for multiple due items
- adding empty-state behavior for task lanes
- fixing styling inconsistencies in the edit window and task actions menu
- correcting command and interaction behavior for tray-style UI elements
- debugging runtime asset loading for tray icons and audio files
- refining the music player so pause/resume behaves like a real player
- adding local persistence for notes
- building searchable, editable code snippet storage
- separating responsibilities between view models and services as features became more complex

These improvements helped make the project feel more complete and usable.

## How to Run

### Prerequisites
- Windows
- .NET 8 SDK
- Visual Studio with WPF/.NET desktop development support

### Project Dependencies
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Sqlite`
- `Microsoft.EntityFrameworkCore.Tools`
- Windows Forms support (used for system tray integration via `NotifyIcon`)

### Steps
1. Clone this repository
2. Open the solution in Visual Studio
3. Restore NuGet packages if prompted
4. Build the project
5. Run the application

### Notes
Ensure the Assets folder is included in the build output
Music files should be placed in Assets/Music
Music metadata can be managed through a tracks.json file in Assets/Music
The task system uses local SQLite storage, so the database file will be created at runtime
If the database schema changes during development, deleting the local database file may be necessary so it can be recreated with the latest tables

## Future Improvements
Possible next steps for the project include:

- search and filtering for tasks
- richer code editing experience for snippets
- notifications for reminders
- data export/import support
- unit tests for view models and services
- additional UI polish and theming options

## Screenshots
### Task Dashboard
<img width="349" height="526" alt="image" src="https://github.com/user-attachments/assets/5fa063d8-fe0b-4aa3-aed4-7b18211a5fcd" />

### Timer Dashboard
<img width="344" height="526" alt="image" src="https://github.com/user-attachments/assets/10cf7d67-df9a-4ce9-a674-373cb6137cd3" />

### Code Snippets Dashboard
<img width="341" height="524" alt="image" src="https://github.com/user-attachments/assets/de71f3da-421d-47c6-9ff8-a4e6d0b84a18" />

### Notes Dashboard
<img width="343" height="524" alt="image" src="https://github.com/user-attachments/assets/2370715f-c512-451a-ad7d-1de0f63066a2" />

### Music Dashboard
<img width="340" height="521" alt="image" src="https://github.com/user-attachments/assets/44b85a6a-0ef3-4c4f-9604-1be028dfaea1" />


## What I Learned
Through this project, I gained more experience with:

- structuring desktop applications with MVVM
- coordinating multiple features in one application
- debugging UI and data-binding issues
- designing for both functionality and usability
- iterating on a project until it feels polished, not just functional
