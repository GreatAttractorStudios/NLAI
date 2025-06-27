# Natural Language AI (NLAI)

This package provides a powerful and intuitive system for generating NPC behavior trees in Unity using natural language descriptions.

## Core Features

-   **Natural Language to Behavior:** Describe complex AI logic in plain English.
-   **Component-Based:** Create reusable `IAction` and `ISense` MonoBehaviours to define your AI's capabilities.
-   **Editor-First Workflow:** All generation happens in the Unity editor, with no runtime performance cost.
-   **Live Visualizer:** A built-in graphical viewer shows the structure and real-time status of your behavior trees.
-   **Intelligent Feedback:** The system provides guidance when you ask for behaviors that haven't been created yet.

## Getting Started

1.  Open the NLAI Editor via `Window > NLAI Editor`.
2.  Create and/or assign an `NLAISettings` asset.
3.  Add `IAction` and `ISense` components to a GameObject prefab.
4.  Optionally, assign that prefab to the "Context Prefab" field in the editor.
5.  Describe the desired behavior and click "Generate Behavior Tree". 