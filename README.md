# Train Velocity Mod for Big Walk

A light-weight BepInEx mod for [**Big Walk**](https://store.steampowered.com/app/1478500/Big_Walk/) that modifies the movement physics of trains, allowing them to accelerate faster, brake harder, and double their top speed.


## **Features**
* **Double Max Train Speed**
* **Faster Acceleration/Braking:** Basically just to account for a faster max speed.


## **Installation**

### **Step 1: Install BepInEx**
This mod requires **BepInEx 6 (IL2CPP) or later**. 
* Because *Big Walk* uses Unity's IL2CPP backend, you **must** use the BepInEx Bleeding Edge builds at this time; as of August 2026, it hasn't been supported by the stable build (yet).
* Download the latest x64 build from the [BepInEx Bleeding Edge Releases](https://builds.bepinex.dev/projects/bepinex_be).
* If you are new to installing BepInEx, watch this [BepInEx Installation Guide Video](https://www.youtube.com/watch?v=-YUsdD3nlbU) for a step-by-step walkthrough.

### **Step 2: Install the Mod**
1. Download `TrainVelocityLibrary.dll` from the Releases section of this repository.
2. Navigate to your *Big Walk* game directory (e.g., `.../Steam/steamapps/common/Big Walk/`).
3. Place `TrainVelocityLibrary.dll` inside the `BepInEx/plugins/` folder. (This folder only exists after BepinEx has successfully been installed and the game ran at least once).
4. Launch the game!


## **Compiling from Source (Windows)**

If you want to build the `.dll` file yourself using Windows, follow these steps:


### **Build Steps**
1. Install the [.NET 10.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) or newer to use the command-line interface.
2. Ensure your `.csproj` file properly references your local game directory's `BepInEx/core` and `BepInEx/interop` assemblies.
3. Clone this repository do your local machine
4. Open this repository in PowerShell and run the following command to compile the source code:
```
dotnet build
```
5. This should compile the resulting `TrainVelocityPlugin.dll` file under a new subfolder called `bin/Debug/net10.0`, ready for use. Move it into your BepinEx folders inside the Big Walk game folder.
