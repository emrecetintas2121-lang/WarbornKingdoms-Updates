# Warborn Kingdoms Updates

Signed update feed for the Warborn Kingdoms launcher.

## Europe1100 launcher and website

This repository now also holds the new Warborn website and single-page Windows launcher sources. Existing release tags and signed update assets are unchanged. This preview is not a replacement production update feed.

- `website/`: static website, with the user-selected Europe1100 knight image; English by default, Turkish and Russian available.
- `launcher/`: WPF launcher (.NET 10 Windows Desktop). Verifies the signed Europe1100 COOP Fixes feed and installs/updates only that Warborn-owned module with hash checks and a retained backup. Steam Workshop mods stay managed by Steam. English/Turkish/Russian interface and guarded Bannerlord text-language configuration.
- `client-mod/`: source for the Warborn fixes module, including the launcher-triggered COOP auto-join flow.
- `launcher-tests/`: localization and game configuration regression tests, plus website translation checks.

### Server endpoint

The current public endpoint is `server.warbornkingdoms.com:4200` (COOP). The dedicated server's engine/management port is `7210` and must not be exposed to players. Forward UDP 4200 (TCP may remain enabled for diagnostics) on the router to the server PC's LAN address and allow it in Windows Firewall. A DNS record and static public IP alone do not create this forwarding rule; verify from a different network after the server is running.

The launcher checks the signed feed on startup and before game launch. It installs or updates only the Warborn-owned Europe1100 COOP fixes package, verifies package and file hashes, and retains the replaced module as a backup. Steam Workshop mods remain under Steam's control. “Start Bannerlord” opens the game executable directly with the enabled module profile saved by the Bannerlord launcher. The client module renames the COOP main-menu entry to “Join Warborn Kingdoms”; selecting it opens the COOP connection flow and connects to `server.warbornkingdoms.com:4200`. The launcher's separate “Join Warborn” shortcut remains available and can send a short-lived local request to the game. Discord is an invitation/help link and does not read private game telemetry. Installed modules are not necessarily version-compatible, so the launcher reports each detected version before launch.

### Build and checks (Windows)

Install the .NET 10 SDK and Node.js, then run from this repository:

```powershell
dotnet run --project launcher-tests -c Release
node launcher-tests/site-tests.mjs
dotnet publish launcher -c Release --no-self-contained -o artifacts/launcher
```

Build the client fixes module on a PC with Bannerlord and the required Workshop DLLs installed:

```powershell
dotnet build client-mod/EuropeCampaignFixes.csproj -c Release
```

The published launcher is self-contained for Windows x64. Website files can be served by a static web server; the production website and launcher download are deployed separately from this source repository.

No server credentials, signing keys, save files or private mod binaries are included.
