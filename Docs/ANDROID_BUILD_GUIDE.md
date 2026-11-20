# Android build and APK delivery

## Goal
Produce a real Android APK from this Unity project and test it on the reference phone: Samsung Galaxy A07, 4 GB physical RAM.

## Current state
An APK is not available yet. The Unity project must compile and the build must complete successfully before an installable file can be shared. A successful CI build is not by itself proof that the game runs smoothly on the phone.

## Unity version
The project is pinned to Unity **6000.0.23f1**. Use this exact editor version. On a Windows PC, install Unity Hub, then install this Unity Editor version with **Android Build Support**, **Android SDK & NDK Tools**, and **OpenJDK** modules.

## Important: current Unity Personal activation method
The old GitHub workflow that tried to generate a manual .alf activation file has been removed. GameCI now recommends activating Unity Personal locally with Unity Hub, then storing the generated license and Unity account credentials as GitHub Actions secrets. Do not run an old “Request Unity Personal License File” workflow.

### 1. Generate the Unity license on your Windows PC
1. Install and open Unity Hub: https://unity.com/download
2. Sign in to your Unity account.
3. Open **Unity Hub → Preferences → Licenses**.
4. Click **Add**, then choose **Get a free Personal license** and complete activation.
5. Confirm that this file exists on Windows: `C:\ProgramData\Unity\Unity_lic.ulf`. The ProgramData folder may be hidden; paste the path into File Explorer's address bar.
6. Open the .ulf file with Notepad and copy its complete contents. Do not send this file or its contents in chat.

### 2. Add three private GitHub Actions secrets
Open the repository:
https://github.com/andoufelixnguessan-cmyk/bus-coach-simulator-africa

Go to **Settings → Secrets and variables → Actions → New repository secret**. Create these three secrets exactly:

- `UNITY_LICENSE` — complete contents of `Unity_lic.ulf`
- `UNITY_EMAIL` — email address used to sign in to Unity
- `UNITY_PASSWORD` — password used to sign in to Unity

Save each secret privately in GitHub. Never put these values in source files, commits, screenshots, or chat messages.

### 3. Build the APK
1. Open the repository's **Actions** tab.
2. Select **Build BUS & COACH SIMULATOR AFRICA for Android**.
3. Press **Run workflow**.
4. If the build succeeds, open that run and download **BUS-COACH-SIMULATOR-AFRICA-Android-APK**.
5. Extract the ZIP and install the APK on the Samsung Galaxy A07.

If the Unity Personal license is already activated on the PC, GameCI's current instructions say to sign out of Unity Hub after copying the license file, so the Personal seat is released for CI use.

## Device testing
Test launch, touch controls, a complete driving session, frame rate, heat, memory use and crashes on the Galaxy A07. Record results before calling the game optimized or ready for release.

## Important
The build workflow only automates compilation. It does not create missing 3D coach assets, realistic roads, traffic, passengers, or finished gameplay. Keep the project in development until a complete playable milestone has been built and tested.


## Orientation de jeu — paysage uniquement

BUS & COACH SIMULATOR AFRICA est conçu pour jouer en format paysage sur téléphone. Le pipeline Android fixe LandscapeLeft comme orientation de départ, autorise les deux orientations horizontales (gauche et droite) et désactive les deux orientations portrait. GameBootstrap réapplique aussi ces règles au démarrage pour éviter qu'une rotation du téléphone bascule l'interface en portrait. Tous les futurs écrans (menu, garage, itinéraires, gestion et conduite) doivent conserver une mise en page paysage et respecter MobileSafeAreaFitter. Le réglage est écrit dans le projet, mais doit encore être confirmé après compilation et installation sur Android.
