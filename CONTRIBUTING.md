# Contributing

Thanks for helping. This is a fork of [gibbed/SteamAchievementManager](https://github.com/gibbed/SteamAchievementManager); report problems with this fork here, not upstream.

## Reporting bugs and ideas

Open an [issue](https://github.com/chavdar12/SteamAchievementManager/issues/new/choose) using a template. Include the app version, your Windows version and the steps to reproduce. Report security problems privately as described in [SECURITY.md](SECURITY.md).

## Making changes

1. Fork the repository and create a branch from `master`.
2. Build with the .NET 10 SDK: `dotnet publish src/SteamAchievementManager -c Release -o publish`
3. Run the tests: `dotnet test tests/SteamAchievementManager.Tests`
4. Keep each pull request to one change, and follow the style of the code around it.
5. Open a pull request against `master`. The build and CodeQL checks must pass before it is merged.

## Releases

Only the maintainer publishes releases, by pushing a version tag (`git tag v7.2.0 && git push origin v7.2.0`). Pushes and pull requests produce a build artifact but no release.

By contributing you agree that your work is released under the project's [zlib License](LICENSE.txt).
