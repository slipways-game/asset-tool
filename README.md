# asset-tool

This is a Unity project that you can use to create bundles with custom graphics and sound for your Slipways mods.

# How to use

* Download a fresh copy of asset-tool whenever you need to add assets to a mod. You can download it from here by pressing the green "Code" button and selecting "Download ZIP".
* Extract the archive somewhere. Change the name of the folder if you want, so that you know which copy goes with which mod.
* Open the project in Unity. It needs to be specifically **Unity 2020.3 LTS**, and you **need both Windows and Mac build support installed** so that asset bundles can be exported properly.
* Place your custom assets in `Assets/Bundle/Contents`. More information on how to create these can be found in the [modding docs](https://docs.slipways.net). The asset tool ships with examples of each asset type, so you can delete ones that you don't need and adapts those that you do.
* Export your bundle into the folder containing your mod. At the top of the screen in Unity there is an extra menu called "Slipways". Select "Export bundle..." from there. Point the export into the directory containing your mod. You can choose any name for the asset file.
* You should see two new files in your mod, `[bundlename].win` and `[bundlename].mac`. You need to reference these files from your `mod.pkg.py` so that the assets are loaded with your mod. Always reference both of these files - they contain versions of your assets for different platforms supported by Slipways.
