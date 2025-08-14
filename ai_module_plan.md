# AI Module Plan

## 1. Overview

This document outlines a plan for creating a module that allows an LLM to interact with and test the Mythril MonoGame project. The module will enable the LLM to generate a series of test commands, have the game execute them, and then receive one or more screenshots of the game's state for review. This will create a powerful automated testing and validation loop.

## 2. Architecture

The module will consist of two main components:

1.  **Game Instrumentation:** Code added to the existing `Mythril` project to allow it to be controlled by an external process.
2.  **Controller Application:** A new console application within the `Mythril` solution that orchestrates the testing process.

Here is a diagram of the proposed architecture:

```
+----------------------+
|      LLM             |
+----------------------+
          ^
          | (3. Get Commands, 7. Send Screenshot)
          v
+----------------------+
| Controller App       |
| (C# Console)         |
+----------------------+
          ^
          | (4. Send Commands, 6. Receive Screenshot Path)
          v
+----------------------+
| Mythril Game         |
| (MonoGame)           |
+----------------------+
```

## 3. Game Instrumentation

### Command Listener

The game will listen for commands on a local TCP socket. This provides a simple and effective way for the controller application to send commands to the running game.

*   **Implementation:** Use `System.Net.Sockets.TcpListener` to create a simple server within the `Game1` class.
*   **Threading:** The listener will run on a separate thread to avoid blocking the game loop.

### Command Parser

The game will parse incoming command strings. The command format will be simple, with one command per line.

*   **Format:** `COMMAND [ARG1] [ARG2] ...`
*   **Example:** `CLICK_BUTTON Settings`

### Command Executor

The game will execute the parsed commands. This will be handled by a new `CommandExecutor` class.

*   **Supported Commands:**
    *   `CLICK_BUTTON <button_text>`: Clicks a UI button with the specified text.
    *   `WAIT <seconds>`: Pauses execution for a specified number of seconds.
    *   `SCREENSHOT <filename>`: Takes a screenshot and saves it to the specified file.
    *   `EXIT`: Closes the game.

## 4. Screenshot Utility

The game will be able to take a screenshot of its current state.

*   **Implementation:** Use `Microsoft.Xna.Framework.Graphics.RenderTarget2D` to render the current scene to a texture, then use `Texture2D.SaveAsPng` to save it to a file.
*   **File Path:** The controller will specify the file path for the screenshot.

## 5. Controller Application

The controller application will be a new C# console project in the `Mythril` solution.

### Process Management

The controller will start the `Mythril` game as a child process.

*   **Implementation:** Use `System.Diagnostics.Process` to start and manage the game process.

### LLM Integration

The controller will need to communicate with an LLM to get test commands. This will be abstracted behind an `ILlmProvider` interface.

*   **Interface:**
    ```csharp
    public interface ILlmProvider
    {
        Task<string[]> GetTestCommandsAsync(string prompt);
        Task SendScreenshotAsync(string screenshotPath);
    }
    ```

### Command Generation

The controller will get commands from the LLM and send them to the game via the TCP socket.

### Screenshot Retrieval

The controller will receive the path to the saved screenshot from the game and then pass it to the LLM.

## 6. Workflow

1.  The user starts the **Controller Application**.
2.  The controller starts the **Mythril Game** as a child process.
3.  The controller sends a prompt to the **LLM** to get a set of test commands.
4.  The controller sends the commands to the game one by one over the TCP socket.
5.  The game executes the commands. When it encounters a `SCREENSHOT` command, it takes a screenshot and saves it to a file.
6.  When the game has finished executing all commands (or receives an `EXIT` command), it closes.
7.  The controller sends the saved screenshot(s) to the **LLM** for review.

## 7. Implementation Details

### Project Structure

*   A new project, `Mythril.Controller`, will be added to the `Mythril.sln` solution.
*   New classes for command handling will be added to the `Mythril` project, likely in a new `GameLogic/AI` namespace.

### Dependencies

*   No new external dependencies are anticipated.

### Configuration

*   A new configuration file (e.g., `ai_config.json`) will be created to store settings for the controller application, such as the port for the TCP listener and any LLM API keys.
