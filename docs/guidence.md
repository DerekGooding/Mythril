# AI Guidance System

## Purpose
This system allows AI agents to seek architectural, project, and strategic direction from human developers. When an agent is unsure of the project's direction or faces a high-level decision, it must document its questions here.

## Process for Agents
1. **Identify Ambiguity:** When a task requires a decision that falls outside established mandates or project patterns.
2. **Create Request:** Create a new Markdown file in `docs/guidence/` following the naming convention `YYYY-MM-DD_topic.md`.
3. **Fill Template:** Use the [Guidance Template](#guidance-template) below. Ensure questions are specific and context is provided.
4. **Monitor for Response:** Check for response in the `Human Guidance` section.
5. **Distill:** Once a human response is provided:
    - Extract the Q&A into `docs/guidence_knowledge_base.md`.
    - Format it as a clear, queryable entry.
    - Delete the original file from `docs/guidence/`.

## Guidance Template
```markdown
# Guidance Request: [Topic]

**Date:** YYYY-MM-DD
**Agent:** Gemini CLI

## Context
[Explain the current situation, the decision to be made, and any proposed options]

## Questions
1. [Primary question?]
2. [Secondary question?]

## Human Guidance
[BLANK - This section is for the human developer to fill out]
```

## Knowledge Base
The cumulative results of these interactions are stored in [docs/guidence_knowledge_base.md](guidence_knowledge_base.md) and must be consulted by all agents before starting new features.
