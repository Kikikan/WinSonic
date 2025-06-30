# WinSonic

A modern music streaming client for Windows, compatible with the Subsonic API and built with WinUI3. Enjoy and manage your music collection from your Subsonic server seamlessly on your desktop.

---

## 🚀 Features

* **Subsonic API Compatibility**: Stream music from any Subsonic-compatible server.
* **WinUI3 Interface**: Crisp, native Windows 11 look-and-feel with Fluent design.
* **Playlists & Queue Management**: Listen to playlists and queue tracks on the fly.
* **Gapless Playback**: Experience uninterrupted listening between tracks.
* **Media Controls**: System media transport controls, global hotkeys, and taskbar integration.
* **Themes**: Light and dark themes based on system preference.

---

## 🎨 Screenshots

### Add new server connection
![image](https://github.com/user-attachments/assets/8a81a0ab-79c6-4b47-b982-b108f61db683)

### Albums view
![image](https://github.com/user-attachments/assets/03f9d11b-c2a5-4fb6-92b3-83becdd380a3)

### Songs view
![image](https://github.com/user-attachments/assets/5f80ebe0-8084-4c12-942d-aaf839f04e8c)

### Now playing queue
![image](https://github.com/user-attachments/assets/c38a2c68-765d-4c56-83c5-3c4e47a0c007)

---

## 📋 Requirements

* **Windows 10 or 11**
* **.NET 8 Desktop Runtime**
* **Access to a Subsonic-compatible server**
* [Segoe Fluent Icons](https://learn.microsoft.com/en-us/windows/apps/design/downloads/#fonts) for Windows 10

---

## 💾 Installation

1. Download the latest release from the [Releases](https://github.com/Kikikan/WinSonic/releases).
2. Extract the appropriate `.zip` archive for your architecture (WinSonic_x.y.z.w_ARM64.zip or WinSonic_x.y.z.w_x64.zip).
3. Inside the extracted folder, right-click `Install.ps1` and select Run with PowerShell to install the app.
  - If not working, use these PowerShell commands:
    - `Import-Certificate -FilePath .\WinSonic_x.y.z.w_arc.cer -CertStoreLocation Cert:\LocalMachine\TrustedPeople`
    - `Add-AppPackage .\WinSonic_x.y.z.w_arc.msix -DependencyPath .\Dependencies\arc\Microsoft.WindowsAppRuntime.1.7.msix`
4. Follow any prompts to allow the script to run and confirm certificate installation.
5. Launch WinSonic from the Start menu.

Alternatively, clone and build from source. Use Visual Studio for development.

---

## ▶️ Usage

1. **Add Server**: Go to **Servers** → **Add**. Enter your Subsonic server URL, username, and password.
2. **Browse Library**: Navigate your music by Albums, Artists, or Genres.
3. **Play Music**: Double-click on track or right-click then use the ▶️ button to start playback.

---

## 🤝 Contributing

We welcome all contributions! Please follow these steps:

1. Fork the repository.
2. Create a feature branch: `git checkout -b feature/YourFeature`.
3. Commit your changes: `git commit -m "Add YourFeature"`.
4. Push to the branch: `git push origin feature/YourFeature`.
5. Open a Pull Request.

If you're new to open source, check out the issues labeled `good first issue` to get started.

---

## 🛡️ License

This project is licensed under the GNU General Public License v3.0. Please take a look at the [LICENSE](LICENSE.txt) file for details.

---

## 📬 Contact

Issues & Discussions: Please use the [GitHub Issues](https://github.com/Kikikan/WinSonic/issues) or [Discussions](https://github.com/Kikikan/WinSonic/discussions) tabs to report bugs, request features, or ask questions.

Thank you for trying **WinSonic**! 🎶 Contributions, feedback, and stars are greatly appreciated.

