# Android build and APK delivery

## Goal
Produce a real Android APK from this Unity project and test it on the reference phone: Samsung Galaxy A07, 4 GB physical RAM.

## Current state
The repository is being converted into a Unity project. An APK is not available until the Unity project opens, the game scene and required assets are complete, and a Unity Android build succeeds. A successful CI build is not by itself proof that the game runs smoothly on the phone.

## Unity version
The project is pinned to Unity **6000.0.23f1**. Use this exact editor version to avoid project-version drift. Install Android Build Support, Android SDK & NDK Tools, and OpenJDK through Unity Hub.

## One-time GitHub build setup

A Unity Personal license is required for the automated GitHub build. Never commit a license file or password to the repository.

1. Open the repository's **Actions** tab.
2. Run **Request Unity Personal License File** using **Run workflow**.
3. When it finishes, download the **Unity-Activation-Request** artifact and extract the .alf file.
4. Submit that .alf file at the Unity manual activation page: https://license.unity3d.com/
5. Download the resulting .ulf license file and open it with a text editor.
6. In the repository, open **Settings → Secrets and variables → Actions → New repository secret**.
7. Set the secret name to exactly UNITY_LICENSE and paste the complete contents of the .ulf file as its value. Save it. Do not send the license contents in chat or commit them.

## Build an APK

1. Open **Actions** in the repository.
2. Select **Build BUS & COACH SIMULATOR AFRICA for Android**.
3. Press **Run workflow**.
4. Wait for the job to finish successfully.
5. Open the completed workflow run and download the artifact **BUS-COACH-SIMULATOR-AFRICA-Android-APK**.
6. Extract the ZIP; the APK inside can be transferred to the Samsung Galaxy A07 and installed.

The build workflow is manual so ordinary code commits do not trigger a build before the Unity license is configured.

## Device testing
Install the APK on the Galaxy A07 and test launch, touch controls, a complete driving session, frame rate, heat, memory use and crashes. Record the results before calling the game optimized or ready for release.

## Important
The GitHub workflows only automate building. They do not create missing 3D coach assets, realistic roads, passenger systems, or finished gameplay. The APK must contain a complete playable milestone before it is shared as a game download. Keep the project in development until those requirements are met.
