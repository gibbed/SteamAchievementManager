# Understand Anything: Codebase Knowledge Graph

This directory contains an AI-generated architectural knowledge graph for **Local PC Steam Achievement Manager**, produced using [Understand Anything](https://github.com/Egonex-AI/Understand-Anything) and OpenAI Codex.

## Knowledge Graph

The initial analysis examined 84 files across three C# projects and generated:

| Metric | Count |
|---|---:|
| Nodes | 265 |
| Relationships | 662 |
| Architectural layers | 7 |
| Guided exploration steps | 13 |

The graph documents relationships between the application's classes, methods, components, and architectural layers.

## Exploring the Architecture

The repository contains three main C# projects:

- **SAM.Picker:** Discovers locally installed Steam games and presents them through the application interface.
- **SAM.API:** Provides access to Steam client functionality and account information.
- **SAM.Game:** Manages game achievements and statistics after a game is selected.

The generated graph can help developers navigate the application startup sequence, local Steam library discovery, account checks, and interactions between these projects.

## Using Understand Anything

Install [Understand Anything](https://github.com/Egonex-AI/Understand-Anything) for a supported coding assistant, such as OpenAI Codex.

Clone this repository, open Codex in its root directory, and invoke:

`$understand-dashboard`

The dashboard provides an interactive visualization of the repository's architectural relationships.

To regenerate the analysis after modifying the source code, invoke:

`$understand`

## Generated Files

- `knowledge-graph.json`: Structured architectural nodes, relationships, layers, and exploration information.
- `code-lessons.json`: Detailed source-code explanations, when generated.

The knowledge graph is AI-generated documentation. Its descriptions and inferred relationships should be checked against the source code when making engineering decisions.

## Attribution

Knowledge graph generated with [Understand Anything by Egonex AI](https://github.com/Egonex-AI/Understand-Anything), using OpenAI Codex.
