# Learning a Codebase with Codex CLI and Understand Anything

**A Windows PowerShell walkthrough, troubleshooting record, and project retrospective**  
**Case study:** [Local PC Steam Achievement Manager](https://github.com/zacw-243L/LocalPCSteamAchievementManager)  
**Session date:** 14 November 2026  
**Tooling used:** OpenAI Codex CLI v0.169.0, Understand Anything, Git, Node.js, pnpm, and a local Vite dashboard

> This guide is based on one completed, hands-on session. Commands and project paths are written for Windows PowerShell. The upstream tool may evolve, so check its [official repository](https://github.com/Egonex-AI/Understand-Anything) if a future version behaves differently.

## 1. What this workflow does

[Codex CLI](https://github.com/openai/codex) is a coding agent that can inspect a local source repository, answer questions about it, and work with files and tools subject to permissions. [Understand Anything](https://github.com/Egonex-AI/Understand-Anything) adds skills that analyze the repository and create a structured **knowledge graph**. The graph describes files, classes, methods, dependencies, architectural layers, and a guided tour. A browser dashboard lets you explore those relationships interactively.

The practical learning loop is:

1. Open Codex in the **specific repository** you want to study.
2. Run `$understand` to build its graph.
3. Review the generated explanations and graph in `$understand-dashboard`.
4. Ask targeted questions, inspect the actual source, and verify any surprising claims.
5. Save the graph and a human-readable learning report for later review.

A useful distinction: Codex can build working *context* about a project during analysis. That does not, by itself, mean the underlying AI model permanently retrains on the repository.

## 2. Prerequisites on Windows

Install **Git**, **Node.js 22 or newer**, and **Codex CLI**. Understand Anything's analysis build requires **pnpm 10 or newer**. Check your tools in PowerShell:

```powershell
 git --version
 node --version
 npm --version
```

Install Codex CLI if necessary:

```powershell
npm install -g @openai/codex
```

Start it once with `codex` and sign in when prompted. If you are still at `C:\Windows\System32`, exit and later relaunch from your actual code repository. Do not analyze a Windows system directory as if it were a project.

Install pnpm and confirm its version:

```powershell
npm install -g pnpm@10
pnpm --version
```

## 3. Install Understand Anything for Codex

The tool's Windows installer supports Codex. If the default location is acceptable, review the [install script](https://github.com/Egonex-AI/Understand-Anything/blob/main/install.ps1), then run:

```powershell
iwr -useb https://raw.githubusercontent.com/Egonex-AI/Understand-Anything/main/install.ps1 | iex
```

At the platform selection prompt, choose **2) codex**. Restart Codex after installation. The installation makes the `understand` skills discoverable through the user's `.agents\skills` directory.

### The custom location used in this session

We kept the Understand Anything **tool** on D:, and the actual **source-code projects** on C:. These locations do not need to be on the same drive.

```text
D:\Utilities\Understand-Anything-Codex\  # Installed tool
C:\Github staging area\               # Existing local repositories
```

For a **fresh** install to D:, choose an empty destination and set the installer variable before running the installer:

```powershell
New-Item -ItemType Directory -Force 'D:\Utilities' | Out-Null
$env:UA_DIR = 'D:\Utilities\Understand-Anything-Codex'
iwr -useb https://raw.githubusercontent.com/Egonex-AI/Understand-Anything/main/install.ps1 | iex
```

Pick `2) codex`. The installer uses `UA_DIR` here for **its own clone destination**. The generated `.ua` directory inside each analyzed project is a separate concept. There is no need to persist `UA_DIR` system-wide for ordinary use.

**Lesson learned:** moving the original Git checkout between C: and D: with `Move-Item` triggered a permissions error involving its `.git` directory. Installing a new copy directly at the desired location was simpler than forcibly moving a partially installed checkout. Avoid deleting either copy until the replacement works.

### Build the plugin core if required

Our initial run recognized the skill but stopped because its JavaScript core was unbuilt and `pnpm` was missing. Once pnpm is installed, build from the **plugin workspace directory**:

```powershell
cd 'D:\Utilities\Understand-Anything-Codex\understand-anything-plugin'
pnpm install
pnpm --filter @understand-anything/core build
```

This path is the installation used in this session. If you installed in the default location, locate the `understand-anything-plugin` directory containing `pnpm-workspace.yaml` and run the commands there. The skill may also perform this build automatically during preflight.

## 4. Launch Codex inside a repository

The working example was a C# fork of Steam Achievement Manager. The folder name contains spaces, so quote the path:

```powershell
cd 'C:\Github staging area\LocalPCSteamAchievementManager'
codex
```

Alternatively, launch directly into that directory:

```powershell
codex -C 'C:\Github staging area\LocalPCSteamAchievementManager'
```

Once inside the Codex prompt, enter:

```text
$understand
```

**Codex uses `$` to invoke these skills.** The `/plugin marketplace add`, `/plugin install`, and `/understand` commands seen in some other tool instructions are not the Codex installation and invocation sequence. `$understand --full` forces a rebuild when needed.

## 5. What to expect during graph generation

On the first run, the skill checks the repository and generates or reads `.ua/.understandignore`. This file selects what should be excluded from analysis. **The skill deliberately pauses for your confirmation.** Review its proposed exclusions and then say something like:

```text
Confirmed. Continue with the full Understand Anything analysis,
following the remaining phases in SKILL.md.
Generate knowledge-graph.json and report when complete.
```

The workflow scans files, forms batches, analyzes source, assembles and checks graph relationships, identifies architectural layers, creates an exploration tour, and saves the graph. Larger repositories can take time and require several permission checks. Codex may display a lot of temporary JavaScript, JSON filenames, and progress information. That terminal output is mainly an execution trace, rather than a finished explanation intended for reading.

**Permission prompts:** during the case study, the sandbox blocked a Node script from launching Git to read the current commit. Codex requested permission to run `.ua/tmp/prepare-save.cjs`. Approve a one-time command only after checking that it belongs to the analysis you requested; inspect generated scripts if you are unsure. Granting permanent permission is generally unnecessary for a one-off exploration.

**Source analysis limitation:** the bundled C# import resolver found no internal edges in the first scan. Codex then inspected source references to infer runtime dependencies. Such relationships are useful hypotheses, but deserve verification against methods, process boundaries, and tests.

## 6. Open the interactive graph

After generation finishes, check that the repository contains:

```text
LocalPCSteamAchievementManager/
└── .ua/
    ├── knowledge-graph.json
    ├── code-lessons.json    # when lesson generation succeeds
    ├── meta.json            # graph generation metadata
    ├── intermediate/        # temporary pipeline artifacts
    └── tmp/                 # possible temporary scripts
```

Inside Codex, run:

```text
$understand-dashboard
```

The dashboard runs a **local web server** and prints a URL. In this session it worked at `http://127.0.0.1:5173/`. Some versions print an access-token URL such as `http://127.0.0.1:5173/?token=...`; use the exact URL displayed by your terminal.

`127.0.0.1` is your own computer and `5173` is the Vite server's port. Starting the dashboard with `--host 127.0.0.1` restricts it to the loopback interface. Stop the terminal server when finished, typically with `Ctrl+C`. The saved graph persists after the server stops.

**Viewer fallback observed:** npm rejected an attempted download of the packaged viewer with `EALLOWREMOTE` (remote packages disabled). Codex then successfully launched the installed local Vite dashboard. This was a **viewer-launch problem**, not a graph-generation failure.

## 7. Real output from the Steam Achievement Manager case study

The completed run reported:

| Result | Value |
|---|---:|
| Inventoried/analyzed files | 84 |
| C# projects | 3 |
| Knowledge-graph nodes | **265** |
| Relationships/edges | **662** |
| Architectural layers | **7** |
| Guided tour steps | **13** |

All 84 inventoried files had generated lessons or overviews. The graph was saved and validated before the dashboard started.

The main components mapped in the source were:

- **`SAM.Picker`**: starts the client, discovers local games, checks account access, filters the results, and displays the list.
- **`SteamLibraryScanner`** (inside `SAM.Picker`): looks for Steam libraries, reads `libraryfolders.vdf` and `appmanifest_*.acf`, and extracts local AppIDs and names.
- **`SAM.API`**: the shared bridge to Steam client APIs, used for initialization, account checks, callbacks, and game metadata.
- **`SAM.Game`**: a separate executable launched for a chosen AppID to manage that game's achievements/statistics.

The interpreted startup route was:

```text
SAM.Picker starts
  → Steam client initialization
  → SteamLibraryScanner reads local libraries/manifests
  → subscription check and preferred name lookup
  → picker list is filtered and displayed
  → user selects a game
  → SAM.Game starts for that AppID
```

Codex highlighted a possible integration issue: `SteamLibraryScanner` can use the `SAM_STEAM_PATH` environment variable, whereas `SAM.API` reportedly locates `steamclient.dll` through a different registry-based path. The analysis suggested the client could fail before scanning in an unusual Steam installation. **This was an analysis finding, not a confirmed runtime bug.** The correct next step would be source review and a targeted test.

The app's existing README explains why the fork exists: a repeated failure downloading a central 1.84 MB Steam game catalogue motivated a local-manifest-based alternative.

## 8. Use the graph to learn, not just admire it

The strongest study pattern is to use each node as a starting point for a more precise question. After finding an interesting connection in the dashboard, ask Codex:

```text
Using the existing Understand Anything graph, explain the path
from SteamLibraryScanner to the GamePicker list. Cite the exact
source files and methods for each relationship. Mark any inferred
runtime relationships separately. Do not change the code.
```

Then drill into a suspected fault:

```text
Compare SteamLibraryScanner's Steam root discovery with
SAM.API.Steam.GetInstallPath. Describe the failure conditions,
identify the source evidence, and propose the smallest test to
confirm the behavior. Do not modify files.
```

Other supported skills discovered in this session included `$understand-chat`, `$understand-explain`, `$understand-diff`, and `$understand-onboard`. They can make the graph useful for questions, deeper file explanations, change-impact exploration, and onboarding. The original code remains the authority when the AI's interpretation is uncertain.

## 9. Save a readable report instead of relying on terminal scrollback

This was an important lesson from the experiment: the generated JSON and busy terminal trace are poor substitutes for a written explanation. The standard graph workflow does not guarantee an exhaustive plain-text transcript. Ask Codex to create a **separate human-readable report** after the graph is saved:

```text
Using .ua/knowledge-graph.json, .ua/code-lessons.json (if present),
and the current repository, create .ua/architecture-report.md.
Explain the component architecture, startup/data flow, key
relationships, sources for important findings, limitations, and
unverified hypotheses. Include a short troubleshooting appendix.
Do not change application source code or overwrite graph files.
```

A Markdown file is searchable in VS Code, readable on GitHub, and easy to convert to `.txt`. It preserves the *findings* rather than pretending to reproduce the agent's inaccessible internal reasoning. If you want a chronological audit trail too, keep the run's available tool logs, generated intermediate reports, and permission decisions separately. Those may be incomplete and should be reviewed before publication.

## 10. Commit the graph to GitHub safely

The official project documentation recommends versioning the `.ua` data while excluding scratch files. Review the generated content first, especially before publishing a private repository's architecture.

Useful `.gitignore` entries:

```gitignore
.ua/intermediate/
.ua/diff-overlay.json
.ua/tmp/
.ua/viewer-cache/
```

For a simple public share, stage the core graph and optional lessons explicitly:

```powershell
cd 'C:\Github staging area\LocalPCSteamAchievementManager'
git status --short
git add -- .ua/knowledge-graph.json
if (Test-Path '.ua/code-lessons.json') { git add -- .ua/code-lessons.json }
git diff --cached --stat
git commit -m 'docs: add Understand Anything knowledge graph'
git push origin master
```

This repo's default branch was `master`; substitute your current branch in other projects. You may also choose to commit graph metadata, fingerprints, configuration, and documentation after reviewing them, consistent with upstream sharing guidance. Avoid a blind `git add .ua/` until caches and local scripts are excluded.

A repository-local `.ua/README.md` can explain how to reproduce the graph and use `$understand-dashboard`. That file was subsequently added to [this project's GitHub repository](https://github.com/zacw-243L/LocalPCSteamAchievementManager/tree/master/.ua). Merely committing the JSON does **not** create a hosted interactive GitHub Pages dashboard; the reader needs a compatible local viewer.

## 11. Troubleshooting, condensed from the chat

| What happened | Meaning / response |
|---|---|
| `& $i ...` returned `BadExpression` | `$i` had not been assigned in that PowerShell session yet. Define it before using `& $i`, or run the installer normally. |
| `Move-Item` failed on `.git` with insufficient rights | The cross-drive move hit an access problem. Fresh install directly in D: worked better; avoid force-changing permissions unnecessarily. |
| Codex opened in `C:\Windows\System32` | Exit and reopen in the target repository, or use `codex -C 'path'`. |
| Skill recognized but core was unbuilt | Install pnpm 10+, then build `@understand-anything/core` in the plugin workspace. |
| `Wait for user confirmation before proceeding` | This is the intentional `.understandignore` review checkpoint. Confirm after inspecting exclusions. |
| Mermaid diagram too large for terminal | Only the terminal diagram rendering exceeded its limits. The graph could still be generated. |
| C# resolver found no internal edges | The tool tried source-based analysis. Check runtime/inferred connections carefully. |
| Codex asked to run a generated Node script outside the sandbox | Review the requested script/action, and grant one-time permission if appropriate. |
| npm returned `EALLOWREMOTE` | Packaged viewer download was blocked by npm policy. The local Vite dashboard fallback worked. |
| Dashboard URL stopped responding after exiting the server | The local Vite process must be running to serve the viewer; the saved JSON is unaffected. |

## 12. The experiment in one paragraph

The project started with the question of whether Understand Anything could work inside ChatGPT. The workable route was Codex CLI, followed by the Understand Anything Windows installer and Codex's `$understand` skill. Installing the tool on D: while keeping Git projects on C: was valid. A first attempt to relocate an installed checkout failed, and the first graph run hit a missing pnpm/core-build prerequisite. After resolving that, Codex scanned 84 files and generated a full C# knowledge graph, pausing for `.understandignore` review and later requesting permission to read Git metadata. The graph was saved with 265 nodes, 662 edges, seven architectural layers, and 13 tour steps. The packaged viewer download was blocked, so Codex launched a local Vite dashboard on the user's PC instead. The graph and documentation were then added to GitHub. The broader lesson was that agentic analysis produces far more low-level terminal output than a person can comfortably digest, so the graph should be paired with a concise, source-linked written report.

## References

- [Understand Anything official repository and instructions](https://github.com/Egonex-AI/Understand-Anything)
- [Understand Anything Windows installer](https://github.com/Egonex-AI/Understand-Anything/blob/main/install.ps1)
- [Codex CLI](https://github.com/openai/codex)
- [Local PC Steam Achievement Manager repository](https://github.com/zacw-243L/LocalPCSteamAchievementManager)
- [This repository's `.ua` documentation](https://github.com/zacw-243L/LocalPCSteamAchievementManager/tree/master/.ua)

---

*Document status: post-session learning guide, not a verbatim transcript. Specific outputs, errors, and decisions above come from the recorded conversation and the repository documentation available at the time. No local files or GitHub repository contents were changed to produce this standalone guide.*
