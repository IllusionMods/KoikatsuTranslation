# Koikatsu Translations
English translation project for Koikatsu

## Installation
1. Install [BepInEx](https://github.com/BepInEx/BepInEx/releases) and [BepisPlugins](https://github.com/bbepis/BepisPlugins/releases).
2. Press the green "Clone or download" button above and click "download zip".
3. Extract the zip and merge the Translation folder with the one in your BepInEx folder.
4. For H translations, [KK_Subtitles](https://github.com/DeathWeasel1337/KK_Plugins#readme) is required to display the text on screen.

## Contribution
There are over 161024 unique lines of text that need translation and any help is appreciated. Regardless of your translation skill and Japanese knowledge you can still help with translations. Even if you have no experience you can help by using Google translate or other translation services and then cleaning up the translation using sanity and a bit of logic. Absolutely no raw machine translations will be accepted, users can use [XUnity.AutoTranslator](https://github.com/bbepis/XUnity.AutoTranslator/releases) for that. These translations serve as a midway point between machine translation and proper, skilled translations.  

The goal of this translation project is to provide passable translations for any text not already translated by the [main translation project](https://github.com/bbepis/KoikatsuTranslation)  

Status:  
Opening - Complete  
c00 (Sexy), c29 (Humble) - Scenario and Communication in progress by ShadowTsuki  
c13 (Gyaru) - Scenario and Communication mostly complete by DarkPulse  

## Tools
[KK_TranslationSync](https://github.com/DeathWeasel1337/KK_Plugins#kk_translationsync) is a plugin for formatting and copy/pasting translations between files when there are duplicate entries.

These tools can help you make sense of the raw Japanese and the machine translations:  

[Yomichan](https://foosoft.net/projects/yomichan/)  
This browser plugin allows you to see the definition of Japanese words by putting your mouse over them in the browser and pressing shift.  

Dictionaries:  
https://jisho.org/  
http://www.romajidesu.com/  

## How-to
When you have translations to submit [fork the repository](https://help.github.com/articles/fork-a-repo/). Upload your changes to your fork and then [submit a pull request](https://help.github.com/articles/about-pull-requests/). Your pull request will be reviewed and accepted after a quality check. Again, no raw machine translations will be accepted. Proper capitalization, punctuation, and spelling is a must.  

Every translation.txt file has the raw Japanese text that needs translations. Each one starts commented out (// at the begining) to avoid DynamicTranslationLoader errors. For your text to display correctly in game, put the translation on the right side of the equal sign and remove the // at the start of the line. Do not edit the Japanese text or the translation will not work.  

Coordinate with other translators on the [Koikatsu Discord](https://discord.gg/urDt8CK) lingual-studies channel. To avoid translation conflicts please ask if anyone is working on a file. If you have any questions about the quality of your translations, ask for advice on the Koikatsu Discord.
