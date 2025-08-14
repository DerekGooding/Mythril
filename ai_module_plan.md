# AI Module Plan (Enhanced for Gemini-CLI & Jules)

## 1. Overview

This module allows an AI agent (Gemini-CLI, Jules, or similar) to directly interact with and test the **Mythril MonoGame** project.  
It supports generating commands, executing them in the running game, and returning screenshots for validation.  
The architecture is **transport-agnostic**, meaning we can switch between TCP, named pipes, standard I/O, or even direct function calls depending on the environment.

The goal is to enable a reliable, iterative testing loop between the AI and the game, without locking ourselves into a single IPC method.

---

## 2. Architecture

**Core components:**

1. **Game Instrumentation** – Hooks inside `Mythril` to process structured commands and return responses.
2. **Controller Application** – A console app that launches and controls the game process, manages the IPC transport, and mediates between the game and the AI agent.
3. **Transport Layer** – Pluggable IPC implementation (TCP, named pipes, stdio) selected at runtime.
4. **Agent Adapter** – Handles quirks of different AI agents (Gemini-CLI, Jules).

**Simplified Flow:**  
LLM / AI Agent → Agent Adapter → Controller App → Transport Layer → Mythril Game

---

## 3. Transport Layer

We define a simple interface:

```csharp
public interface ICommandTransport
{
    Task SendAsync(string message);
    Task<string> ReceiveAsync();
}
```

**Implementations:**
- **StdIoTransport** → For agents that run the process interactively in the same shell (Gemini-CLI, Jules).
- **NamedPipeTransport** → Local-only IPC, faster and more secure than TCP.
- **TcpTransport** → For remote or cross-machine scenarios.

Default: `StdIoTransport` for Gemini-CLI and Jules.

---

## 4. Command Protocol

All communication uses **structured JSON**, not raw strings.  
Example:
```json
{
    "action": "CLICK_BUTTON",
    "target": "Settings",
    "args": {}
}
```

**Benefits:**
- Easy to extend.
- Easier for LLMs to generate valid commands.
- Safer to parse and validate.

---

## 5. Game Instrumentation

### Command Listener
- The game runs a background listener on the chosen transport.
- Validates and queues incoming commands.

### Command Executor
- Executes supported commands:
    - `CLICK_BUTTON <label>` – Clicks a UI button by text.
    - `CLICK_COORDS <x> <y>` – Clicks a specific point.
    - `WAIT <seconds>` – Pauses execution.
    - `SCREENSHOT [inline|path]` – Sends screenshot (base64 if inline).
    - `PING` – Confirms game is alive.
    - `EXIT` – Shuts down game.

---

## 6. Screenshot Handling

Two modes:
1. **Path Mode** – Saves to file, returns path (for local/human testers).
2. **Inline Mode** – Encodes screenshot as base64, sends directly over transport (for remote/AI agents).

**Implementation:**  
- Use `RenderTarget2D` → `Texture2D.SaveAsPng` → `FileStream` or in-memory stream.

---

## 7. Controller Application

### Process Management
- Starts the game as a child process.
- Handles cleanup if the game crashes or stalls.

### AI Agent Integration
Interface:
```csharp
public interface IAgentAdapter
{
    Task<string[]> GetCommandsAsync(string prompt);
    Task SendImageAsync(byte[] imageData);
}
```
Adapters:
- **GeminiCliAdapter**
- **JulesAdapter**

### Workflow
1. Detect which agent is active.
2. Pick the right `ICommandTransport`.
3. Launch the game with appropriate IPC mode.
4. Loop:
    - Get commands from AI.
    - Send to game.
    - Receive screenshots/responses.
    - Send results back to AI.
5. Exit gracefully.

---

## 8. Configuration

**`ai_config.json` example:**
```json
{
    "transport": "stdio",
    "screenshotMode": "inline",
    "pipeName": "mythril_ai",
    "tcpPort": 5555
}
```

---

## 9. Best Practices

- Default to **StdIO** for Gemini-CLI and Jules (no networking setup, safest).
- Only enable TCP if you actually need remote testing, and bind to `127.0.0.1`.
- Validate all incoming JSON before executing commands.
- Include heartbeat checks so the controller knows the game is responsive.
- Allow mid-run feedback from the AI, not just run-once-then-exit.
