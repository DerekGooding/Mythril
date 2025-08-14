# Jules Agent Configuration

## Project Overview
This project is built with **.NET 9** and requires the .NET SDK to be installed before building, testing, or running.  
All code is written in C# and unit tests use **MSTest**.

## Environment Setup
Run the provided setup script before performing any other commands:

```bash
./setup.sh
```

## AI Control Module

This project includes an AI control module that allows agents like Jules and Gemini-CLI to interact with the running game. This is done by sending JSON commands over standard I/O.

### Screenshot Capability

One of the key features is the ability to take screenshots of the game. This is done using the `SCREENSHOT` command.

**Command:**
```json
{
  "action": "SCREENSHOT",
  "args": {
    "filename": "my_screenshot.png",
    "inline": true
  }
}
```

*   `filename`: The name of the file to save the screenshot to.
*   `inline`: If `true`, the game will return the screenshot as a base64-encoded string in the response. This is useful for agents that can view images directly.

The agent can use this functionality to get visual feedback from the game state.
