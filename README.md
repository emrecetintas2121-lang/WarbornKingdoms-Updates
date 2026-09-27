# Warborn Kingdoms Updates

Signed update feed for the Warborn Kingdoms launcher.

## Europe1100 redesign — development preview

This repository now also holds the new Warborn website and single-page Windows launcher sources. Existing release tags and signed update assets are unchanged. This preview is not a replacement production update feed.

- `website/`: static website, with the user-selected Europe1100 knight image; English by default, Turkish and Russian available.
- `launcher/`: WPF launcher (.NET 10 Windows Desktop). Checks local Steam Workshop modules, opens subscription links, and lists Warborn modules separately. English/Turkish/Russian interface and guarded Bannerlord text-language configuration.
- `launcher-tests/`: localization and game configuration regression tests, plus website translation checks.

### Server endpoint

The current public endpoint is `159.146.11.20:4200` (COOP). The dedicated server's engine/management port is `7210` and must not be exposed to players. Forward TCP 4200 on the router to the server PC's LAN address, currently `192.168.1.7`, and allow the same port in Windows Firewall. A static public IP alone does not create this forwarding rule; verify from a different network after the server is running.

The launcher deliberately disables Install/Update and Join until the signed own-mod updater and verified server connection are implemented. Discord account/game integration is also pending; the current Discord button is an invitation link. Installed modules are not necessarily version-compatible. Both the website and launcher now use the user-selected Europe1100 knight artwork.

### Build and checks (Windows)

Install the .NET 10 SDK and Node.js, then run from this repository:

```powershell
dotnet run --project launcher-tests -c Release
node launcher-tests/site-tests.mjs
dotnet publish launcher -c Release --no-self-contained -o artifacts/launcher
```

The framework-dependent launcher requires the .NET 10 Windows Desktop Runtime. Keep all published files together. Website files can be served by a static web server; this commit does not deploy to warbornkingdoms.com or modify the Discord bot.

No server credentials, signing keys, save files or private mod binaries are included.
