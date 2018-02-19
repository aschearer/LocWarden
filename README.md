# Loc Warden
I think it's important to localize my games. Localizing provides the best experience for my players overseas, and, for a relatively low cost, it makes my work available to a much larger audience. By translating my games into a handful of languages I can easily double my addressable market!

However, it's not all sunshine and lollipops. For a small team, localization poses a number of challenges. You have to add support for your game, find a translation company, verify that the translations are sane, convert the translations into a format suitable for your game, and update each store front with translations. Loc Warden makes the last three much easier.

This project has three goals:

  1. Validate your loc files
  1. Convert your loc files into a game friendly format
  1. Export metadata to localize storefronts

Out of the box you will find tools to faciliate all of the above. But every project has different requirements, so I've designed Loc Warden with plugins in mind. If you're comfortable coding, it should be simple to create your own plugins to tackle loc problems unique to your game.

## Prerequisites
  * You're using Windows.
  * You have a text editor.
  * You have loc files.

## Quick Start

*Note for this tutorial I am going to assume you have loc files titled `English.xlsx` and `German.xlsx` in a folder titled `Loc`.*

Download and unzip the latest build here:

[LocWarden.zip][1]

### Validating Translations
Let's start by using LocWarden to validate our translations. When receiving loc files it's not uncommon to find missing strings or missing format args. Using LocWarden, we can quickly and automatically verify that our translations are sane. If not we can work with the translator to fix things before they reach the game.

Start by navigating to the project folder and creating a new file titled `Settings.yml`. Open the file in your favorite editor and add the following:

    languages:
        English:
            path: Loc/English.xlsx
            isMaster: true
        German:
            path: Loc/German.xlsx
    plugins:
        excelImporter:
            type: LocWarden.Plugins.ExcelImporter
            assembly: Plugins/LocWarden.Plugins/LocWarden.Plugins.dll

*Note: You can call the languages whatever you want. "English", "english", "anglais" are all fine.*

*Note: Only one language can be master. This language is treated as the source of truth during validation.*

Copy the `Loc` folder containing your loc files into the folder.

Next open PowerShell and run:

    .\LocWarden.Console.exe -c Settings.yml

You should see the following output in the terminal:

    Languages validated. No errors encountered!
    Press any key to continue...

If your translation contains errors, you'll see them outputted in the console, too. For example:

    Errors encountered in: German (Loc/German.xlsx)
    Type|Line Number|Message
    KeysMissing|26|Key missing: Settings/Quality Level
    FormatArgMissing|10|Format arg missing: Gameplay/Player Name
    EmptyTerm|33|Key Graphics Card Low Name is not translated.

    Encountered 3 errors across 1 languages.
    Press any key to continue...

Once you, or your translator, have fixed all the errors you can be confident that:

  * No string ids are missing
  * No string ids were added
  * The string ids are in the same order
  * No format args are missing
  * No format args were added
  * No format args were changed
  * There are no blank translations
  * The translations are not mostly the same as the master file

If you want to validate additional languages simply add them to the list of `languages` in the settings file.

### Exporting Translations for the Game
Now that we're confident that our translations are ready to be integrated into the game we need convert the Excel files into a single CSV. The CSV will contain a row for each string id and a column for each language. Open the settings file and add a new plugin:

    languages:
        English:
            path: Loc/English.xlsx
            isMaster: true
        German:
            path: Loc/German.xlsx
    plugins:
        excelImporter:
            type: LocWarden.Plugins.ExcelImporter
            assembly: Plugins/LocWarden.Plugins/LocWarden.Plugins.dll
        gameExporter:
            type: LocWarden.Plugins.CsvExporter
            assembly: Plugins/LocWarden.Plugins/LocWarden.Plugins.dll
            outputFile: GameLoc.csv

*Note: the name for each plugin (e.g. "gameExporter") is not important as long as they are unique.*

Run LocWarden again and this time you'll see:

    Languages validated. No errors encountered!
    Exporters executed. No errors encountered!
    Press any key to continue...

In Explorer, you will find `GameLoc.csv`. Open it and confirm it contains English and German strings. At this point, you can use your game's localization tools to import the csv.

### Exporting Translations for Steamworks
Now that the game is translated it's time to update our storefronts. Each platform has unique requirements and in some cases the best solution may be to write a custom LocWarden plugin. But for this example I will demonstrate how to create a compose a BBCode description suitable for Steam. 

> **How I Translate Steam**
>
> On my game's Steam page I have several sections ("About the Game", "Controls", "Features"), several blocks of text, and a bulleted feature list. I sent each of these to the translator as a unique string id. I omitted any BBCode tags so that I can re-use my marketing copy on other platforms which may not support BBCode.

Let's get started. First, add a new plugin to the settings:


    languages:
        English:
            path: Loc/English.xlsx
            isMaster: true
        German:
            path: Loc/German.xlsx
    plugins:
        excelImporter:
            type: LocWarden.Plugins.ExcelImporter
            assembly: Plugins/LocWarden.Plugins/LocWarden.Plugins.dll
        gameExporter:
            type: LocWarden.Plugins.CsvExporter
            assembly: Plugins/LocWarden.Plugins/LocWarden.Plugins.dll
            outputFile: GameLoc.csv
        steamPageExporter:
            type: LocWarden.Plugins.TemplatedKeyExporter
            assembly: Plugins/LocWarden.Plugins/LocWarden.Plugins.dll
            templateFile: SteamTemplate.txt
            outputFile: SteamPageLoc.txt

Note that this time a `templateFile` is being defined. Create `SteamTemplate.txt` and open it in your editor. Paste the following then save and close:

    [h2]{[Developer's Note Header]}[/h2]
    {[Developer's Note]}

    [h2]{[Gameplay Header]}[/h2]
    {[Gameplay Note]}

    [h2]{[Controls Header]}[/h2]
    {[Controls Note]}

    [h2]{[Features Header]}[/h2]
    [list]
    [*]{[Feature-1]}[/*]
    [*]{[Feature-2]}[/*]
    [*]{[Feature-3]}[/*]
    [*]{[Feature-4]}[/*]
    [*]{[Feature-5]}[/*]
    [*]{[Feature-6]}[/*]
    [/list]

Every format arg (surrounded by `{[` and `]}`) corresponds with a string id in my loc files.

Run LocWarden and open the newly generated file `SteamPageLoc.txt`. In it you will find the above template with localized text replacing each format arg and repeated once per a language. Using this file, we can quickly cut and paste into Steam to localize our storefront!

### Discovering Plugins
At this point you may have everything you need to localize your game. But perhaps you want to know what other plugins are available? To discover what plugins are present in a DLL is easy. Just run Loc Warden as follows:

    .\LocWarden.Console.exe -d .\Plugins\LocWarden.Plugins\LocWarden.Plugins.dll

If the assembly contains Loc Warden plugins then their documentation will be printed to the console. For example the above yields:

    Documentation for plugins in: .\Plugins\LocWarden.Plugins\LocWarden.Plugins.dll

    Excel Importer
            Imports languages stored as Excel files.
            Parameters:
                    numberOfHeaderRows (number, optional): How many rows should be skipped from the beginning. Default is 1.
                    keyColumn (number, optional): Column index for loc key. Default is 0.
                    descriptionColumn (number, optional): Column index for loc description. Default is 1.
                    valueColumnForMaster (number, optional): Column index for loc value in master language. Default is 2.
                    valueColumnForNormal (number, optional): Column index for loc value in non-master languages. Default is 3.

    Templated Key Exporter
            Takes a template file and exports a copy per language with parameters replaced with localized text.
            Parameters:
                    templateFile (string, required): File with keys in {[format args]}.
                    outputFile (string, required): File to write results to.

    Key Exporter
            Exports designated keys as columns with each language as a row.
            Parameters:
                    keysListFile (string, required): File containing a list of keys, one per a line.
                    outputFile (string, required): File to write results to.

    Csv Exporter
            Exports languages in a single CSV.
            Parameters:
                    outputFile (string, required): File to write CSV data to.

### Writing a Plugin
This section is forthcoming. Thank you for your patience.

## Contributing
The following is intended for those who would like to change the tool. If you simply want to run the tool refer to the Quick Start section above.

### Installation

 * Open `LocWarden.sln` in Visual Studio 2017.
 * Restore Nuget packages.
 * Build the solution.

The generated exe and dependencies will be automatically copied to `Publish/LocWarden`. Use that directory to test changes. It also serves as the basis for creating new release.

[1]: https://github.com/aschearer/locwarden/releases/latest