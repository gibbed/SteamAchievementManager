# Learning a Codebase with Codex CLI and Understand Anything

## Quick check: is Codex still installed?

Run these three commands first whenever opening a new PowerShell session or troubleshooting a tool that appears to have disappeared:

```powershell
codex --version
pnpm --version
npm list -g --depth=0
```

`codex --version` checks whether the Codex command is discoverable, `pnpm --version` checks Understand Anything's package manager, and `npm list -g --depth=0` lists globally installed npm packages. If Codex is missing, jump to [How to actually install Codex if your PC forgot Codex was installed](#how-to-actually-install-codex-if-your-pc-forgot-codex-was-installed). After replacing Node.js, follow [Replacing Node.js](#replacing-nodejs). If Codex refuses to display a saved graph, see [When Codex refuses to open your graph](#when-codex-refuses-to-open-your-graph-verified-sql-lab7-recovery).


**A Windows PowerShell walkthrough, troubleshooting record, and project retrospective**  
**Case study:** [Local PC Steam Achievement Manager](https://github.com/zacw-243L/LocalPCSteamAchievementManager)  
**Additional troubleshooting:** SQL Lab7 Python/MySQL project  
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
pnpm --version
codex --version
npm list -g --depth=0
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

### How to actually install Codex if your PC forgot Codex was installed

This happened during the case study. Codex CLI v0.169.0 had already worked, but after Node.js was installed or changed, a fresh PowerShell window reported `codex : The term 'codex' is not recognized`. The npm global directory was on `PATH`, yet `codex.cmd` was absent. That points to a missing global npm command in the *current* Node/npm environment; the earlier Codex session and saved graphs were real.

Start with these checks in a **new, ordinary (non-Administrator) PowerShell window**:

```powershell
npm list -g --depth=0
npm config get prefix
where.exe codex
Test-Path (Join-Path (npm config get prefix) 'codex.cmd')
```

If `@openai/codex` is absent from the global package list, or the command shim is missing, install Codex again:

```powershell
npm.cmd install -g @openai/codex
codex.cmd --version
```

Using `.cmd` avoids PowerShell execution-policy interference with `npm.ps1` or `codex.ps1`. After successful installation, `codex --version` should work too. If `codex.cmd --version` works but `codex --version` does not, inspect the PowerShell command resolution with `Get-Command codex -All`; a `.ps1` policy issue may be selecting the script over the `.cmd` launcher.

If Codex is listed as installed but `where.exe codex` finds nothing, check whether npm's global prefix is part of the current session's PATH:

```powershell
$prefix = npm config get prefix
$env:Path -split ';' | Where-Object { $_ -eq $prefix }
& (Join-Path $prefix 'codex.cmd') --version
```

The direct-path command will only work when `codex.cmd` exists there. If the global prefix is missing from PATH, add that directory through Windows **Environment Variables**, then open a new PowerShell window. Reinstalling Codex is unnecessary when the executable is present and the problem is merely command lookup.

**What remains intact:** reinstalling the npm CLI should leave the Understand Anything checkout (in this case `D:\Utilities\Understand-Anything-Codex`), links under the user profile, Git repositories, and generated `.ua/knowledge-graph.json` files alone. Restart Codex from the repository directory to confirm `$understand` still appears in the available skills.

### Replacing Node.js

A Node.js installation, upgrade, or change of installation method can alter the npm executable, global package prefix, or PATH used by a new terminal. It does **not invariably uninstall** Codex or pnpm. In our session, an earlier terminal could run Codex and the local viewer, while a later PowerShell session initially could not find `node`, `npm`, `pnpm`, or `codex`. After Node.js became available, Codex was still missing from the global command location. That sequence is compatible with differing environments and changed global package availability; we did not prove a single root cause.

Before replacing Node.js, record what the current environment recognizes:

```powershell
node --version
npm --version
npm config get prefix
npm list -g --depth=0
where.exe node
where.exe codex
```

Install or update Node.js from a consistent source such as the [official LTS installer](https://nodejs.org/en/download) or Windows Package Manager (`winget`). Use the same method for future upgrades where practical. After installation, **close old PowerShell windows and open a fresh one**, then verify:

```powershell
node --version
npm --version
npm list -g --depth=0
codex --version
pnpm --version
```

If npm works but Codex and/or pnpm do not, restore only the missing global tools:

```powershell
npm.cmd install -g @openai/codex
npm.cmd install -g pnpm@10
codex.cmd --version
pnpm.cmd --version
```

`pnpm@10` matches the minimum major version used in this case study, and the project requires Node.js 22 or newer for analysis. The working environment later showed Git 2.56.1, Node.js 26.13.1, npm 11.11.0, and pnpm 10.34.9. Codex must be checked separately because `node`, `npm`, and `pnpm` succeeding does not prove it is installed.

For the next project, start a normal PowerShell session in your chosen Git checkout, run the quick checks at the top of this guide, and launch Codex there. This avoids accidentally starting analysis from `C:\Windows\System32`.

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

### When Codex refuses to open your graph: verified SQL Lab7 recovery

**Real case: SQL Lab7, 14 November 2026.** Understand Anything finished analyzing the old Python/MySQL coursework project, but Codex couldn't start the dashboard. The packaged viewer wasn't cached, its sandbox blocked child processes with `spawn EPERM`, and the helper it generated (`.ua/launch-dashboard.ps1`) produced one Vite configuration error after another. **A manual Vite launch with the bundled config loader ultimately worked.** The screenshot showed the actual graph in a browser, with **224 nodes, 858 edges, 21 inventoried functions, 4 architectural layers, and 8 guided tour steps**.

The practical lesson: **graph generation and viewer startup are two separate operations.** Check the saved graph first, then fix the viewer. Avoid rerunning a potentially expensive multi-phase analysis solely because a browser window refuses to open.

#### The errors we encountered

| Symptom | Why it mattered | What we did |
|---|---|---|
| `spawn EPERM` | Codex's restricted environment blocked a Node/esbuild child process. | Switched to a normal Windows PowerShell terminal. |
| `ReferenceError: __dirname is not defined` | A generated launcher used Vite's runner config-loader, which didn't provide the CommonJS-style variable expected in the plugin config. | Stopped using the generated temporary config. |
| `ERR_MODULE_NOT_FOUND .../packages/core/src/staleness` | The non-bundled ESM loading path could not resolve the dashboard's TypeScript source import. | Launched Vite using `--configLoader bundle`. |
| `Knowledge graph freshness could not be verified` | The running dashboard couldn't read Git `HEAD` for the viewed project. | Treated it as an advisory graph-freshness warning, since the graph itself displayed successfully. |

The later `node: module.register() is deprecated` warning was incidental. Neither the Vite errors nor the Git freshness warning demonstrated corruption of the already-generated graph.

#### Step 1: Verify the graph and redacted viewer copy

Open a **separate, ordinary PowerShell window**. Keep Codex's failed session and the SQL program itself out of the repair procedure. This example uses the actual Windows paths from our experiment:

```powershell
$project = 'D:\Utilities\Programing\Python REPOS\SQL Lab7'
$plugin = 'D:\Utilities\Understand-Anything-Codex\understand-anything-plugin'

Test-Path (Join-Path $project '.ua\knowledge-graph.json')
Test-Path (Join-Path $project '.ua\viewer\.ua\knowledge-graph.json')
Test-Path (Join-Path $plugin 'packages\core\src\staleness.ts')
```

The first check is the canonical graph location. The second is the **redacted viewer's nested `.ua` directory**, reported by Codex during recovery. The third confirms that the core TypeScript source file needed by the dashboard exists. The previous session's initial path checks returned `False`, but a later Codex inspection found and validated both graph copies. Don't assume a saved graph is missing until you've checked the exact location and the generated analysis report.

If the canonical graph or viewer copy is absent, ask Codex to inspect `.ua/`, `.ua/tmp/`, `.ua/intermediate/`, and any recovery report before regenerating anything. If `staleness.ts` is missing, repair or update the plugin checkout rather than creating a dummy file.

**Privacy:** SQL Lab7 contains a historical `secrets.txt` and hardcoded MySQL credentials. The successful dashboard was pointed at Codex's **redacted viewer copy** so the browser wasn't serving the original project tree. Preserve that separation. A local dashboard token should also be treated as private.

#### Step 2: Bypass Codex's generated launcher (working fix)

After confirming that the redacted graph and plugin source exist, run Vite directly from the installed dashboard workspace:

```powershell
$project = 'D:\Utilities\Programing\Python REPOS\SQL Lab7'
$plugin = 'D:\Utilities\Understand-Anything-Codex\understand-anything-plugin'

$env:GRAPH_DIR = Join-Path $project '.ua\viewer'
Set-Location (Join-Path $plugin 'packages\dashboard')
pnpm exec vite --config vite.config.ts --configLoader bundle --host 127.0.0.1
```

**This was the successful workaround in the SQL Lab7 experiment.** The important details are: run outside Codex's sandbox, point `GRAPH_DIR` at the **redacted** viewer folder (which contains `.ua/knowledge-graph.json`), use the installed plugin's `vite.config.ts`, and explicitly request the **bundled** config loader. That avoids the failed `.ua/tmp/viewer.config.mjs` path and the unbundled TypeScript-import resolution that produced `ERR_MODULE_NOT_FOUND`.

Do not modify `vite.config.ts`, change machine-wide PowerShell execution policy, or elevate to Administrator as a first response. This workaround started the server using the existing installation and graph files.

Vite prints a local URL, usually resembling:

```text
http://127.0.0.1:5173/?token=<generated-token>
```

Open **the actual full URL printed by Vite**, including its token when supplied. Port 5173 may change if something else is using it. Keep that terminal running while viewing the dashboard, and stop the server with `Ctrl+C` when finished.

#### Step 3: Interpret the Git freshness warning

The working dashboard displayed a yellow warning saying **“Knowledge graph freshness could not be verified”** because it could not read Git `HEAD`. This is a **comparison/metadata warning**, not a launch failure. The graph, four architectural layers, and guided tour were visible.

The analyzed SQL Lab7 folder might not be a conventional Git checkout, or Git metadata might be unavailable to the dashboard. To inspect this separately, open another terminal in the project and run:

```powershell
Set-Location 'D:\Utilities\Programing\Python REPOS\SQL Lab7'
git rev-parse --show-toplevel
git rev-parse HEAD
```

If the commands report that the folder is outside a Git repository, the dashboard cannot compare the saved graph against a current commit there. You can still explore the graph. To obtain reliable freshness checks in future, analyze an actual Git working tree. Do not initialize Git or rewrite graph metadata merely to dismiss this warning.

#### If the direct launch still fails

Inspect rather than repeatedly editing global config files. Run these checks in a separate PowerShell session:

```powershell
$plugin = 'D:\Utilities\Understand-Anything-Codex\understand-anything-plugin'
Test-Path (Join-Path $plugin 'packages\dashboard\vite.config.ts')
Test-Path (Join-Path $plugin 'packages\core\src\staleness.ts')
node --version
pnpm --version
```

If dependencies are missing or the core hasn't been built, use the plugin workspace to restore them, then retry the verified launch command:

```powershell
Set-Location 'D:\Utilities\Understand-Anything-Codex\understand-anything-plugin'
pnpm install
pnpm --filter @understand-anything/core build
```

`pnpm install` installs package dependencies; it does not restore a deleted TypeScript source file. If an agent previously edited the plugin's config, review and preserve those differences before updating or reinstalling the plugin. This install is shared by other projects, so arbitrary config edits can break a dashboard that used to work.

**What the repair did not require:** rerunning `$understand`, executing the old SQL Lab7 application, reinstalling MySQL, exposing the original secret-bearing source files, or altering Windows administrator permissions.

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
| `node`, `npm`, or `pnpm` became unrecognized in another PowerShell window | Open a fresh session, check `where.exe node`, the npm prefix and PATH, and repair missing prerequisites as needed. |
| `codex` disappeared after changing Node.js even though it ran earlier | Check `npm list -g --depth=0`, the npm prefix and `codex.cmd`; reinstall `@openai/codex` globally only if missing. |
| SQL Lab7 dashboard failed with `spawn EPERM` | Codex could not spawn the viewer process. Run Vite directly from normal PowerShell; the generated helper was also broken. |
| Vite reported `__dirname is not defined` | The generated launcher used an incompatible config runner; bypass it and use Vite with `--configLoader bundle`. |
| Vite reported missing `packages/core/src/staleness` | The bundled Vite config loader resolved this issue in the SQL Lab7 experiment; verify the module exists first. |

| Dashboard displays `Knowledge graph freshness could not be verified` | Graph viewer can work even when Git HEAD is unavailable; check whether the analyzed folder is a Git working tree. |

## 12. The experiment in one paragraph

The project started with the question of whether Understand Anything could work inside ChatGPT. The workable route was Codex CLI, followed by the Understand Anything Windows installer and Codex's `$understand` skill. Installing the tool on D: while keeping Git projects on C: was valid. A first attempt to relocate an installed checkout failed, and the first graph run hit a missing pnpm/core-build prerequisite. After resolving that, Codex scanned 84 files and generated a full C# knowledge graph, pausing for `.understandignore` review and later requesting permission to read Git metadata. The graph was saved with 265 nodes, 662 edges, seven architectural layers, and 13 tour steps. The packaged viewer download was blocked, so Codex launched a local Vite dashboard on the user's PC instead. The graph and documentation were then added to GitHub. The broader lesson was that agentic analysis produces far more low-level terminal output than a person can comfortably digest, so the graph should be paired with a concise, source-linked written report. A later check exposed another lesson: the tools that worked during the initial session were not all discoverable from a subsequent PowerShell environment, and the globally installed Codex command had to be restored after Node.js changes. Version checks and an npm global-package inventory now appear at the top of this guide. A subsequent SQL Lab7 experiment encountered Vite dashboard failures (`spawn EPERM`, `__dirname` and missing `staleness` module) after the analysis workflow; the guide now distinguishes saved graph output from viewer startup and records a cautious config/debugging procedure. The subsequent manual PowerShell launch of Vite with `--config vite.config.ts --configLoader bundle` succeeded, opening a redacted SQL Lab7 graph with 224 nodes, 858 edges, four layers, and eight tour steps. A Git HEAD freshness warning remained advisory.

## References

- [Understand Anything official repository and instructions](https://github.com/Egonex-AI/Understand-Anything)
- [Understand Anything Windows installer](https://github.com/Egonex-AI/Understand-Anything/blob/main/install.ps1)
- [Codex CLI](https://github.com/openai/codex)
- [Local PC Steam Achievement Manager repository](https://github.com/zacw-243L/LocalPCSteamAchievementManager)
- [This repository's `.ua` documentation](https://github.com/zacw-243L/LocalPCSteamAchievementManager/tree/master/.ua)

---

*Document status: updated post-session learning guide, not a verbatim transcript. Specific outputs, errors, and decisions above come from the recorded conversation and the repository documentation available at the time. This document revision does not modify the GitHub repository, the local Codex installation, or project source files.*
