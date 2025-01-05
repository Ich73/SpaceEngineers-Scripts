/* v:2.0200 (Performance optimizations)
* Automatic LCDs 2 - In-game script by MMaster
*
* Thank all of you for making amazing creations with this script, using it and helping each other use it.
* Its 2024 - it's been 9 years already since I uploaded first Automatic LCDs script and you are still using it (in "a bit" upgraded form).
* That's just amazing! I hope you will have many more years of fun with it :)
*
* LATEST UPDATE:
*  Added PropFloat command to display values of float properties of blocks (more info in guide)
*  Many changes to improve performance of the script
*  You may notice the script is now more responsive eg on scrolling 
*  (there may be hickups in scrolling as the script never updates more than 1 LCD in single tick)
*  Improved startup performance & internal scheduling
*  Startup may take longer on larger grids than before  
*  Updated text override functionality of Power commands (PowerSummary now allows override of text, PowerStoredBar also allows for custom text)
*  
*  Technical Note: 
*    The script is just too large and it may take long time (~6ms per tick) to compile parts of it as they are used.
*    Once all the parts of the script had been executed at least once:
*    - the max execution time on grid with ~200 commands on around 40 LCDs: ~200us, average: ~69us (should still be smooth at around 200 such grids)
*    
*  Bugfixes: Fix same construct filtering for functionality relying on block types
*    Fix Tanks command using different percent rounding than TanksP
*    Add support for clang cola, cosmic coffee, flare gun, flare clip
*    Hopefully fixed weird InvListX value text misalignment
*  
* Previous notable updates:
*  Added C: modifier to name filter to filter on same construct as programmable block (on rotors & pistons, but not connectors)
*    Note: C: modifier works in the same way as T: modifier used for same grid filtering - check guide section 'Same construct blocks filtering'!
*  Cockpit (and other blocks) panels support - read guide section 'How to use with cockpits?'
*  Optimizations for servers running script limiter - use SlowMode!
*  Added SlowMode setting to considerably slow down the script (4-5 times less processing per second)
* 
* Previous updates: Look at Change notes tab on Steam workshop page. */

/* Customize these: */

// Use this tag to identify LCDs managed by this script
// Name filtering rules can be used here so you can use even G:Group or T:[My LCD]
public string LCD_TAG = "T:[LCD]";

// Set to true if you want to slow down the script
public bool SlowMode = false;

// How many lines to scroll per step
public int SCROLL_LINES = 1;

// Script automatically figures if LCD is using monospace font
// if you use custom font scroll down to the bottom, then scroll a bit up until you find AddCharsSize lines
// monospace font name and size definition is above those

// Enable initial boot sequence (after compile / world load)
public bool ENABLE_BOOT = true;

// Set to true to stop the script from changing the content type of the screens
public bool SKIP_CONTENT_TYPE = false;

/* READ THIS FULL GUIDE
http://steamcommunity.com/sharedfiles/filedetails/?id=407158161

Basic video guide
Please watch the video guide even if you don't understand my English. You can see how things are done there.

https://youtu.be/vqpPQ_20Xso


Please carefully read the FULL GUIDE before asking questions I had to remove guide from here to add more features :(
Please DO NOT publish this script or its derivations without my permission! Feel free to use it in blueprints!

Special Thanks
Keen Software House for awesome Space Engineers game
Malware for contributing to programmable blocks game code and MDK!

Watch Twitter: https://twitter.com/MattsPlayCorner
and Facebook: https://www.facebook.com/MattsPlayCorner1080p
for more crazy stuff from me in the future :)

If you want to make scripts for Space Engineers check out MDK by Malware:
https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts
*/
bool MDK_IS_GREAT = true;
/* Customize characters used by script */
class MMStyle {
    // Monospace font characters (\uXXXX is special character code)
    public const char BAR_MONO_START = '[';
    public const char BAR_MONO_END = ']';
    public const char BAR_MONO_EMPTY = '\u2591'; // 25% rect
    public const char BAR_MONO_FILL = '\u2588'; // full rect

    // Classic (Debug) font characters
    // Start and end characters of progress bar need to be the same width!
    public const char BAR_START = '[';
    public const char BAR_END = ']';
    // Empty and fill characters of progress bar need to be the same width!
    public const char BAR_EMPTY = '\'';
    public const char BAR_FILL = '|';
}
// (for developer) Debug level to show
public const int DebugLevel = 0;

// (for modded lcds) Affects all LCDs managed by this programmable block
/* LCD height modifier
0.5f makes the LCD have only 1/2 the lines of normal LCD
2.0f makes it fit 2x more lines on LCD */
public const float HEIGHT_MOD = 1.0f;

/* line width modifier
0.5f moves the right edge to 50% of normal LCD width
2.0f makes it fit 200% more text on line */
public const float WIDTH_MOD = 1.0f;

List<string> BOOT_FRAMES = new List<string>() {
/* BOOT FRAMES
* Each @"<text>" marks single frame, add as many as you want each will be displayed for one second
* @"" is multiline string so you can write multiple lines */
@"
Wird gestartet..."
};

void ItemsConf() {
    // ITEMS AND QUOTAS LIST
    // (subType, mainType, quota, display name, short name)
    // VANILLA ITEMS
    Add("Stone", "Ore", 0, "Stein");
    Add("Iron", "Ore", 0, "Eisen");
    Add("Nickel", "Ore", 0, "Nickel");
    Add("Cobalt", "Ore", 0, "Kobalt");
    Add("Magnesium", "Ore", 0, "Magnesium");
    Add("Silicon", "Ore", 0, "Silizium");
    Add("Silver", "Ore", 0, "Silber");
    Add("Gold", "Ore", 0, "Gold");
    Add("Platinum", "Ore", 0, "Platin");
    Add("Uranium", "Ore", 0, "Uran");
    Add("Ice", "Ore", 0, "Eis");
    Add("Scrap", "Ore", 0, "Metallabfall");
    Add("Stone", "Ingot", 40000, "Kies");
    Add("Iron", "Ingot", 300000, "Eisen");
    Add("Nickel", "Ingot", 200000, "Nickel");
    Add("Cobalt", "Ingot", 120000, "Kobalt");
    Add("Magnesium", "Ingot", 80000, "Magnesium");
    Add("Silicon", "Ingot", 150000, "Silizium");
    Add("Silver", "Ingot", 80000, "Silber");
    Add("Gold", "Ingot", 80000, "Gold");
    Add("Platinum", "Ingot", 45000, "Platin");
    Add("Uranium", "Ingot", 3000, "Uran");
    Add("SemiAutoPistolItem", "Tool", 0, "S-10");
    Add("ElitePistolItem", "Tool", 0, "S-10E");
    Add("FullAutoPistolItem", "Tool", 0, "S-20A");
    Add("AutomaticRifleItem", "Tool", 0, "MR-20");
    Add("PreciseAutomaticRifleItem", "Tool", 0, "MR-8P");
    Add("RapidFireAutomaticRifleItem", "Tool", 0, "MR-50A");
    Add("UltimateAutomaticRifleItem", "Tool", 0, "MR-30E");
    Add("BasicHandHeldLauncherItem", "Tool", 0, "RO-1");
    Add("AdvancedHandHeldLauncherItem", "Tool", 0, "PRO-1");
    Add("WelderItem", "Tool", 0, "Schweißgerät");
    Add("Welder2Item", "Tool", 0, "* Verb. Schweißgerät");
    Add("Welder3Item", "Tool", 0, "** Prof. Schweißgerät");
    Add("Welder4Item", "Tool", 0, "*** Elite-Schweißgerät");
    Add("AngleGrinderItem", "Tool", 0, "Schleifgerät");
    Add("AngleGrinder2Item", "Tool", 0, "* Verb. Schleifgerät");
    Add("AngleGrinder3Item", "Tool", 0, "** Prof. Schleifgerät");
    Add("AngleGrinder4Item", "Tool", 0, "*** Elite-Schleifgerät");
    Add("HandDrillItem", "Tool", 0, "Handbohrer");
    Add("HandDrill2Item", "Tool", 0, "* Verb. Handbohrer");
    Add("HandDrill3Item", "Tool", 0, "** Prof. Handbohrer");
    Add("HandDrill4Item", "Tool", 0, "*** Elite-Handbohrer");
    Add("FlareGunItem", "Tool", 0, "Signalpistole");
    Add("Construction", "Component", 50000, "Herstellungskomponenten");
    Add("MetalGrid", "Component", 15500, "Metallgitter");
    Add("InteriorPlate", "Component", 55000, "Interne Panzerung");
    Add("SteelPlate", "Component", 300000, "Stahlplatte");
    Add("Girder", "Component", 3500, "Träger");
    Add("SmallTube", "Component", 26000, "Kleines Stahlrohr");
    Add("LargeTube", "Component", 6000, "Großes Stahlrohr");
    Add("Motor", "Component", 16000, "Motor");
    Add("Display", "Component", 500, "Anzeige");
    Add("BulletproofGlass", "Component", 12000, "Panzerglas");
    Add("Computer", "Component", 6500, "Computer");
    Add("Reactor", "Component", 10000, "Reaktorkomponenten");
    Add("Thrust", "Component", 16000, "Triebwerk-Komponenten");
    Add("GravityGenerator", "Component", 250, "Schwerkraftgenerator-Komponenten");
    Add("Medical", "Component", 120, "Medizinische Komponenten");
    Add("RadioCommunication", "Component", 250, "Kommunikationssystem-Komponenten");
    Add("Detector", "Component", 400, "Sensorkomponente");
    Add("Explosives", "Component", 500, "Sprengstoff");
    Add("SolarCell", "Component", 2800, "Solarzelle");
    Add("PowerCell", "Component", 2800, "Energiezelle");
    Add("Superconductor", "Component", 3000, "Supraleiter");
    Add("Canvas", "Component", 300, "Fallschirmseide");
    Add("EngineerPlushie", "Component", 0, "Ingenieursplüschtier");
    Add("SabiroidPlushie", "Component", 0, "Saberoid-Plüschtier");
    Add("ZoneChip", "Component", 100, "Zonen-Chip");
    Add("Datapad", "Datapad", 0, "Datapad");
    Add("Package", "Package", 0, "Paket");
    Add("Medkit", "ConsumableItem", 0, "MedKit");
    Add("Powerkit", "ConsumableItem", 0, "Powerkit");
    Add("ClangCola", "ConsumableItem", 0, "Clang Kola");
    Add("CosmicCoffee", "ConsumableItem", 0, "Cosmic Coffee");
    Add("SpaceCredit", "PhysicalObject", 0, "Space Credit");
    Add("NATO_5p56x45mm", "Ammo", 8000, "5.56x45-mm-NATO-Munitionskiste", "5.56x45-mm", "", false);
    Add("SemiAutoPistolMagazine", "Ammo", 500, "S-10-Magazin", "S-10");
    Add("ElitePistolMagazine", "Ammo", 500, "S-10E-Magazin", "S-10E");
    Add("FullAutoPistolMagazine", "Ammo", 500, "S-20A-Magazin", "S-20A");
    Add("AutomaticRifleGun_Mag_20rd", "Ammo", 1000, "MR-20-Magazin", "MR-20");
    Add("PreciseAutomaticRifleGun_Mag_5rd", "Ammo", 1000, "MR-8P-Magazin", "MR-8P");
    Add("RapidFireAutomaticRifleGun_Mag_50rd", "Ammo", 8000, "MR-50A-Magazin", "MR-50A");
    Add("UltimateAutomaticRifleGun_Mag_30rd", "Ammo", 1000, "MR-30E-Magazin", "MR-30E");
    Add("NATO_25x184mm", "Ammo", 2500, "25x184-mm-NATO-Munitionskiste", "25x184-mm");
    Add("Missile200mm", "Ammo", 1600, "200-mm-Raketenbehälter", "Raketen");
    Add("AutocannonClip", "Ammo", 50, "Maschinenkanonenmagazin", "Maschinenkanonen");
    Add("MediumCalibreAmmo", "Ammo", 50, "Sturmgeschützgeschoss", "Sturmgeschütz");
    Add("SmallRailgunAmmo", "Ammo", 50, "Treibspiegel für kleine Schienenkanonen", "Kl. Schienenkanonen");
    Add("LargeRailgunAmmo", "Ammo", 50, "Treibspiegel für große Schienenkanonen", "Gr. Schienenkanonen");
    Add("LargeCalibreAmmo", "Ammo", 50, "Artilleriegeschoss", "Artillerie");
    Add("FlareClip", "Ammo", 0, "Signalpistolenhalter");
    Add("OxygenBottle", "OxygenContainerObject", 5, "Sauerstoffflasche");
    Add("HydrogenBottle", "GasContainerObject", 5, "Wasserstoffflasche");
    // MODDED ITEMS
    // (subType, mainType, quota, display name, short name, alt. short name, used)
    // * if used is true, item will be shown in inventory even for 0 items
    // * if used is false, item will be used only for display name and short name
    // AzimuthSupercharger
    Add("AzimuthSupercharger", "Component", 1600, "Supercharger", "supercharger", "", false);
    // OKI Ammo
    Add("OKI23mmAmmo", "Ammo", 500, "23x180mm", "23x180mm", "", false);
    Add("OKI50mmAmmo", "Ammo", 500, "50x450mm", "50x450mm", "", false);
    Add("OKI122mmAmmo", "Ammo", 200, "122x640mm", "122x640mm", "", false);
    Add("OKI230mmAmmo", "Ammo", 100, "230x920mm", "230x920mm", "", false);

    // REALLY REALLY REALLY
    // DO NOT MODIFY ANYTHING BELOW THIS (TRANSLATION STRINGS ARE AT THE BOTTOM)
}
void Add(string sT, string mT, int q = 0, string dN = "", string sN = "", string sN2 = "", bool u = true) { ƫ.Ô(sT, mT, q, dN, sN, sN2, u); }
ĵ ƫ;Ȥ ƨ;Ĥ ϸ;ɹ e=null;void Ϸ(string ƙ){}bool ϵ(string ϲ){return ϲ.ǽ("true");}void ϴ(string ϳ,string ϲ){string ȇ=ϳ.ToLower
();switch(ȇ){case"lcd_tag":LCD_TAG=ϲ;break;case"slowmode":SlowMode=ϵ(ϲ);break;case"enable_boot":ENABLE_BOOT=ϵ(ϲ);break;
case"skip_content_type":SKIP_CONTENT_TYPE=ϵ(ϲ);break;case"scroll_lines":int ϱ=0;if(int.TryParse(ϲ,out ϱ)){SCROLL_LINES=ϱ;}
break;}}void ϰ(){string[]ţ=Me.CustomData.Split('\n');for(int D=0;D<ţ.Length;D++){string Ũ=ţ[D];int Ľ=Ũ.IndexOf('=');if(Ľ<0){Ϸ
(Ũ);continue;}string ϯ=Ũ.Substring(0,Ľ).Trim();string ǻ=Ũ.Substring(Ľ+1).Trim();ϴ(ϯ,ǻ);}}int Ϯ=0;bool ϭ(){if(Ϯ<6){switch(
Ϯ){case 0:ƫ=new ĵ();ItemsConf();break;case 1:ϰ();e=new ɹ(this,DebugLevel,ƨ){ƫ=ƫ,ɳ=LCD_TAG,ɲ=SCROLL_LINES,ɱ=ENABLE_BOOT,ɰ=
BOOT_FRAMES,ɯ=!MDK_IS_GREAT,ɭ=HEIGHT_MOD,ɮ=WIDTH_MOD};e.Ǖ();break;case 2:e.Ǡ();break;case 3:e.ǡ();e.ǣ();break;case 4:ϸ=new Ĥ(e);ƨ.ș
(ϸ,0);break;case 5:var Ϭ=new List<IMyTerminalBlock>();e.Ŏ.Є(ref Ϭ,"reactor");break;}Ϯ++;return false;}return true;}void Ϫ
(){ƨ.ǋ=this;e.ǋ=this;}Program(){Runtime.UpdateFrequency=UpdateFrequency.Update1;}double Ϩ=0;StringBuilder ϖ=new
StringBuilder(10000);void Main(string ą,UpdateType ϕ){Ϩ+=Runtime.TimeSinceLastRun.TotalSeconds*16.6666666666/16;try{if(ƨ==null){Ϩ=0;ƨ
=new Ȥ(this,DebugLevel,SlowMode);}if(!ϭ())return;Ϫ();e.Ŏ.Р();if(ą.Length==0&&(ϕ&(UpdateType.Update1|UpdateType.Update10|
UpdateType.Update100))==0){ƨ.ȯ();return;}if(ą!=""){if(ϸ.Ć(ą)){ƨ.ȯ();return;}}ϸ.Ģ=0;ƨ.Ȯ();}catch(Exception ex){Echo(
"ERROR DESCRIPTION:\n"+ex.ToString());Me.Enabled=false;}}class ϔ:ɢ{Ĥ Ĕ;ɹ e;string ą="";public ϔ(ɹ Å,Ĥ Ē,string Ő){ɞ=-1;ɡ="ArgScroll";ą=Ő;Ĕ=Ē;e
=Å;}int Ś;Ϗ ϒ;public override void ɼ(){ϒ=new Ϗ(ƣ,e.Ŏ);}int ϑ=0;int Ĝ=0;Ί ƙ;public override bool ɻ(bool ó){if(!ó){Ĝ=0;ϒ.Ų(
);ƙ=new Ί(ƣ);ϑ=0;}if(Ĝ==0){if(!ƙ.ʬ(ą,ó))return false;if(ƙ.ͷ.Count>0){if(!int.TryParse(ƙ.ͷ[0].Ő,out Ś))Ś=1;else if(Ś<1)Ś=1
;}if(ƙ.Ή.EndsWith("up"))Ś=-Ś;else if(!ƙ.Ή.EndsWith("down"))Ś=0;Ĝ++;ó=false;}if(Ĝ==1){if(!ϒ.Б("textpanel",ƙ.Έ,ó))return
false;Ĝ++;ó=false;}æ b;for(;ϑ<ϒ.Ц();ϑ++){if(!ƣ.ɔ(20))return false;var ϐ=ϒ.ϩ[ϑ]as IMyTextPanel;if(!Ĕ.ù.TryGetValue(ϐ,out b))
continue;if(b==null||b.ã!=ϐ)continue;if(b.Ý)b.ä.ĸ=10;if(Ś>0)b.ä.Ļ(Ś);else if(Ś<0)b.ä.Ń(-Ś);else b.ä.ķ();b.H();}return true;}}
class Ϗ{Ȥ ƣ;Х ϓ;IMyCubeGrid ώ{get{return ƣ.ǋ.Me.CubeGrid;}}IMyGridTerminalSystem ǈ{get{return ƣ.ǋ.GridTerminalSystem;}}public
List<IMyTerminalBlock>ϩ=new List<IMyTerminalBlock>(30);public Ϗ(Ȥ ƨ,Х ϧ){ƣ=ƨ;ϓ=ϧ;}int Ϧ=0;public double ϥ(ref double Ϥ,ref
double ϣ,bool ó){if(!ó)Ϧ=0;for(;Ϧ<ϩ.Count;Ϧ++){if(!ƣ.ɔ(4))return Double.NaN;IMyInventory Ϟ=ϩ[Ϧ].GetInventory(0);if(Ϟ==null)
continue;Ϥ+=(double)Ϟ.CurrentVolume;ϣ+=(double)Ϟ.MaxVolume;}Ϥ*=1000;ϣ*=1000;return(ϣ>0?Ϥ/ϣ*100:100);}int Ϣ=0;double ϡ=0;public
double Ϡ(bool ó){if(!ó){Ϣ=0;ϡ=0;}for(;Ϣ<ϩ.Count;Ϣ++){if(!ƣ.ɔ(6))return Double.NaN;for(int ϟ=0;ϟ<2;ϟ++){IMyInventory Ϟ=ϩ[Ϣ].
GetInventory(ϟ);if(Ϟ==null)continue;ϡ+=(double)Ϟ.CurrentMass;}}return ϡ*1000;}int ϝ=0;private bool Ϝ(bool ó=false){if(!ó)ϝ=0;while(ϝ
<ϩ.Count){if(!ƣ.ɔ(400))return false;if(ϩ[ϝ].CubeGrid!=ώ){ϩ.RemoveAt(ϝ);continue;}ϝ++;}return true;}int ϛ=0;private bool Ϛ
(bool ó=false){if(!ó)ϛ=0;var ϙ=ƣ.ǋ.Me;while(ϛ<ϩ.Count){if(!ƣ.ɔ(400))return false;if(!ϩ[ϛ].IsSameConstructAs(ϙ)){ϩ.
RemoveAt(ϛ);continue;}ϛ++;}return true;}List<IMyBlockGroup>Ϙ=new List<IMyBlockGroup>(5);List<IMyTerminalBlock>ϫ=new List<
IMyTerminalBlock>(30);int Ϲ=0;public bool А(string Έ,bool ó){int О=Έ.IndexOf(':');string Н=(О>=1&&О<=2?Έ.Substring(0,О):"");bool Ж=Н.
Contains("T");bool Е=Н.Contains("C");if(Н!="")Έ=Έ.Substring(О+1);if(Έ==""||Έ=="*"){if(!ó){ϫ.Clear();ǈ.GetBlocks(ϫ);ϩ.AddList(ϫ);
ƣ.ȫ(100);}if(Ж){if(!Ϝ(ó))return false;}else if(Е){if(!Ϛ(ó))return false;}return true;}string З=(Н.Contains("G")?Έ.Trim():
"");if(З!=""){if(!ó){Ϙ.Clear();ǈ.GetBlockGroups(Ϙ);Ϲ=0;}for(;Ϲ<Ϙ.Count;Ϲ++){IMyBlockGroup Д=Ϙ[Ϲ];if(string.Compare(Д.Name,
З,true)==0){if(!ó){ϫ.Clear();Д.GetBlocks(ϫ);ϩ.AddList(ϫ);ƣ.ȫ(100);}if(Ж){if(!Ϝ(ó))return false;}else if(Е){if(!Ϛ(ó))
return false;}return true;}}return true;}if(!ó){ϫ.Clear();ǈ.SearchBlocksOfName(Έ,ϫ);ϩ.AddList(ϫ);ƣ.ȫ(100);}if(Ж){if(!Ϝ(ó))
return false;}else if(Е){if(!Ϛ(ó))return false;}return true;}List<IMyBlockGroup>М=new List<IMyBlockGroup>(5);List<
IMyTerminalBlock>Л=new List<IMyTerminalBlock>(10);int К=0;int Й=0;public bool И(string ʥ,string З,bool Ж,bool Е,bool ó){if(!ó){М.Clear()
;ǈ.GetBlockGroups(М);К=0;}var ϙ=ƣ.ǋ.Me;for(;К<М.Count;К++){IMyBlockGroup Д=М[К];if(string.Compare(Д.Name,З,true)==0){if(!
ó){Й=0;Л.Clear();Д.GetBlocks(Л);ƣ.ȫ(100);}else ó=false;for(;Й<Л.Count;Й++){if(!ƣ.ɔ(400))return false;if(Ж&&Л[Й].CubeGrid
!=ώ)continue;if(Е&&!Л[Й].IsSameConstructAs(ϙ))continue;if(ϓ.Ѓ(Л[Й],ʥ))ϩ.Add(Л[Й]);ƣ.ȫ(5);}return true;}}return true;}List<
IMyTerminalBlock>Г=new List<IMyTerminalBlock>(50);int В=0;public bool Б(string ʥ,string Έ,bool ó){int О=Έ.IndexOf(':');string Н=(О>=1&&О
<=2?Έ.Substring(0,О):"");bool Ж=Н.Contains("T");bool Е=Н.Contains("C");if(Н!="")Έ=Έ.Substring(О+1);if(!ó){Г.Clear();В=0;}
string З=(Н.Contains("G")?Έ.Trim():"");if(З!=""){if(!И(ʥ,З,Ж,Е,ó))return false;return true;}if(!ó){ϓ.Є(ref Г,ʥ);ƣ.ȫ(100);}if(Έ
==""||Έ=="*"){if(!ó)ϩ.AddList(Г);if(Ж){if(!Ϝ(ó))return false;}else if(Е){if(!Ϛ(ó))return false;}return true;}var ϙ=ƣ.ǋ.Me;
for(;В<Г.Count;В++){if(!ƣ.ɔ(400))return false;if(Ж&&Г[В].CubeGrid!=ώ)continue;if(Е&&!Г[В].IsSameConstructAs(ϙ))continue;if(
Г[В].CustomName.Contains(Έ))ϩ.Add(Г[В]);ƣ.ȫ(5);}return true;}public void Ш(Ϗ Ч){ϩ.AddList(Ч.ϩ);}public void Ų(){ϩ.Clear()
;}public int Ц(){return ϩ.Count;}}class Х{Ȥ ƣ;ɹ e;public MyGridProgram ǋ{get{return ƣ.ǋ;}}public IMyGridTerminalSystem ǈ{
get{return ƣ.ǋ.GridTerminalSystem;}}public Х(Ȥ ƨ,ɹ Å){ƣ=ƨ;e=Å;}void Ф<Ȃ>(List<IMyTerminalBlock>У,Func<IMyTerminalBlock,bool
>Т=null)where Ȃ:class,IMyTerminalBlock{ǈ.GetBlocksOfType<Ȃ>(У,Т);}public Dictionary<string,Action<List<IMyTerminalBlock>,
Func<IMyTerminalBlock,bool>>>С;public void Р(){if(С!=null)return;С=new Dictionary<string,Action<List<IMyTerminalBlock>,Func<
IMyTerminalBlock,bool>>>(){{"CargoContainer",Ф<IMyCargoContainer>},{"TextPanel",Ф<IMyTextPanel>},{"Assembler",Ф<IMyAssembler>},{
"Refinery",Ф<IMyRefinery>},{"Reactor",Ф<IMyReactor>},{"SolarPanel",Ф<IMySolarPanel>},{"BatteryBlock",Ф<IMyBatteryBlock>},{"Beacon"
,Ф<IMyBeacon>},{"RadioAntenna",Ф<IMyRadioAntenna>},{"AirVent",Ф<IMyAirVent>},{"ConveyorSorter",Ф<IMyConveyorSorter>},{
"OxygenTank",Ф<IMyGasTank>},{"OxygenGenerator",Ф<IMyGasGenerator>},{"OxygenFarm",Ф<IMyOxygenFarm>},{"LaserAntenna",Ф<IMyLaserAntenna
>},{"Thrust",Ф<IMyThrust>},{"Gyro",Ф<IMyGyro>},{"SensorBlock",Ф<IMySensorBlock>},{"ShipConnector",Ф<IMyShipConnector>},{
"ReflectorLight",Ф<IMyReflectorLight>},{"InteriorLight",Ф<IMyInteriorLight>},{"LandingGear",Ф<IMyLandingGear>},{"ProgrammableBlock",Ф<
IMyProgrammableBlock>},{"TimerBlock",Ф<IMyTimerBlock>},{"MotorStator",Ф<IMyMotorStator>},{"PistonBase",Ф<IMyPistonBase>},{"Projector",Ф<
IMyProjector>},{"ShipMergeBlock",Ф<IMyShipMergeBlock>},{"SoundBlock",Ф<IMySoundBlock>},{"Collector",Ф<IMyCollector>},{"JumpDrive",Ф<
IMyJumpDrive>},{"Door",Ф<IMyDoor>},{"GravityGeneratorSphere",Ф<IMyGravityGeneratorSphere>},{"GravityGenerator",Ф<IMyGravityGenerator
>},{"ShipDrill",Ф<IMyShipDrill>},{"ShipGrinder",Ф<IMyShipGrinder>},{"ShipWelder",Ф<IMyShipWelder>},{"Parachute",Ф<
IMyParachute>},{"LargeGatlingTurret",Ф<IMyLargeGatlingTurret>},{"LargeInteriorTurret",Ф<IMyLargeInteriorTurret>},{
"LargeMissileTurret",Ф<IMyLargeMissileTurret>},{"SmallGatlingGun",Ф<IMySmallGatlingGun>},{"SmallMissileLauncherReload",Ф<
IMySmallMissileLauncherReload>},{"SmallMissileLauncher",Ф<IMySmallMissileLauncher>},{"VirtualMass",Ф<IMyVirtualMass>},{"Warhead",Ф<IMyWarhead>},{
"FunctionalBlock",Ф<IMyFunctionalBlock>},{"LightingBlock",Ф<IMyLightingBlock>},{"ControlPanel",Ф<IMyControlPanel>},{"Cockpit",Ф<
IMyCockpit>},{"TransponderBlock",Ф<IMyTransponder>},{"BroadcastController",Ф<IMyBroadcastController>},{"CryoChamber",Ф<
IMyCryoChamber>},{"MedicalRoom",Ф<IMyMedicalRoom>},{"RemoteControl",Ф<IMyRemoteControl>},{"ButtonPanel",Ф<IMyButtonPanel>},{
"CameraBlock",Ф<IMyCameraBlock>},{"OreDetector",Ф<IMyOreDetector>},{"ShipController",Ф<IMyShipController>},{"SafeZoneBlock",Ф<
IMySafeZoneBlock>},{"Decoy",Ф<IMyDecoy>}};}public void П(ref List<IMyTerminalBlock>Ă,string Џ){Action<List<IMyTerminalBlock>,Func<
IMyTerminalBlock,bool>>Ї;if(Џ=="SurfaceProvider"){ǈ.GetBlocksOfType<IMyTextSurfaceProvider>(Ă);return;}if(С.TryGetValue(Џ,out Ї))Ї(Ă,
null);else{if(Џ=="WindTurbine"){ǈ.GetBlocksOfType<IMyPowerProducer>(Ă,(Ѕ)=>Ѕ.BlockDefinition.TypeIdString.EndsWith(
"WindTurbine"));return;}if(Џ=="HydrogenEngine"){ǈ.GetBlocksOfType<IMyPowerProducer>(Ă,(Ѕ)=>Ѕ.BlockDefinition.TypeIdString.EndsWith(
"HydrogenEngine"));return;}if(Џ=="StoreBlock"){ǈ.GetBlocksOfType<IMyFunctionalBlock>(Ă,(Ѕ)=>Ѕ.BlockDefinition.TypeIdString.EndsWith(
"StoreBlock"));return;}if(Џ=="ContractBlock"){ǈ.GetBlocksOfType<IMyFunctionalBlock>(Ă,(Ѕ)=>Ѕ.BlockDefinition.TypeIdString.EndsWith(
"ContractBlock"));return;}if(Џ=="VendingMachine"){ǈ.GetBlocksOfType<IMyFunctionalBlock>(Ă,(Ѕ)=>Ѕ.BlockDefinition.TypeIdString.EndsWith(
"VendingMachine"));return;}}}public void Є(ref List<IMyTerminalBlock>Ă,string Ђ){П(ref Ă,Ё(Ђ.Trim()));}public bool Ѓ(IMyTerminalBlock ã,
string Ђ){string І=Ё(Ђ);switch(І){case"FunctionalBlock":return true;case"ShipController":return(ã as IMyShipController!=null);
default:return ã.BlockDefinition.TypeIdString.Contains(Ё(Ђ));}}public string Ё(string Ѐ){if(Ѐ=="surfaceprovider")return
"SurfaceProvider";if(Ѐ.Ǿ("carg")||Ѐ.Ǿ("conta"))return"CargoContainer";if(Ѐ.Ǿ("text")||Ѐ.Ǿ("lcd"))return"TextPanel";if(Ѐ.Ǿ("coc"))return
"Cockpit";if(Ѐ.Ǿ("ass"))return"Assembler";if(Ѐ.Ǿ("refi"))return"Refinery";if(Ѐ.Ǿ("reac"))return"Reactor";if(Ѐ.Ǿ("solar"))return
"SolarPanel";if(Ѐ.Ǿ("wind"))return"WindTurbine";if(Ѐ.Ǿ("hydro")&&Ѐ.Contains("eng"))return"HydrogenEngine";if(Ѐ.Ǿ("bat"))return
"BatteryBlock";if(Ѐ.Ǿ("bea"))return"Beacon";if(Ѐ.ǽ("vent"))return"AirVent";if(Ѐ.ǽ("sorter"))return"ConveyorSorter";if(Ѐ.ǽ("tank"))
return"OxygenTank";if(Ѐ.ǽ("farm")&&Ѐ.ǽ("oxy"))return"OxygenFarm";if(Ѐ.ǽ("gene")&&Ѐ.ǽ("oxy"))return"OxygenGenerator";if(Ѐ.ǽ(
"cryo"))return"CryoChamber";if(string.Compare(Ѐ,"laserantenna",true)==0)return"LaserAntenna";if(Ѐ.ǽ("antenna"))return
"RadioAntenna";if(Ѐ.Ǿ("thrust"))return"Thrust";if(Ѐ.Ǿ("gyro"))return"Gyro";if(Ѐ.Ǿ("sensor"))return"SensorBlock";if(Ѐ.ǽ("connector"))
return"ShipConnector";if(Ѐ.Ǿ("reflector")||Ѐ.Ǿ("spotlight"))return"ReflectorLight";if((Ѐ.Ǿ("inter")&&Ѐ.Ǽ("light")))return
"InteriorLight";if(Ѐ.Ǿ("land"))return"LandingGear";if(Ѐ.Ǿ("program"))return"ProgrammableBlock";if(Ѐ.Ǿ("timer"))return"TimerBlock";if(Ѐ.
Ǿ("motor")||Ѐ.Ǿ("rotor"))return"MotorStator";if(Ѐ.Ǿ("piston"))return"PistonBase";if(Ѐ.Ǿ("proj"))return"Projector";if(Ѐ.ǽ(
"merge"))return"ShipMergeBlock";if(Ѐ.Ǿ("sound"))return"SoundBlock";if(Ѐ.Ǿ("col"))return"Collector";if(Ѐ.ǽ("jump"))return
"JumpDrive";if(string.Compare(Ѐ,"door",true)==0)return"Door";if((Ѐ.ǽ("grav")&&Ѐ.ǽ("sphe")))return"GravityGeneratorSphere";if(Ѐ.ǽ(
"grav"))return"GravityGenerator";if(Ѐ.Ǽ("drill"))return"ShipDrill";if(Ѐ.ǽ("grind"))return"ShipGrinder";if(Ѐ.Ǽ("welder"))return
"ShipWelder";if(Ѐ.Ǿ("parach"))return"Parachute";if((Ѐ.ǽ("turret")&&Ѐ.ǽ("gatl")))return"LargeGatlingTurret";if((Ѐ.ǽ("turret")&&Ѐ.ǽ(
"inter")))return"LargeInteriorTurret";if((Ѐ.ǽ("turret")&&Ѐ.ǽ("miss")))return"LargeMissileTurret";if(Ѐ.ǽ("gatl"))return
"SmallGatlingGun";if((Ѐ.ǽ("launcher")&&Ѐ.ǽ("reload")))return"SmallMissileLauncherReload";if((Ѐ.ǽ("launcher")))return
"SmallMissileLauncher";if(Ѐ.ǽ("mass"))return"VirtualMass";if(string.Compare(Ѐ,"warhead",true)==0)return"Warhead";if(Ѐ.Ǿ("func"))return
"FunctionalBlock";if(string.Compare(Ѐ,"shipctrl",true)==0)return"ShipController";if(Ѐ.StartsWith("broadcast"))return"BroadcastController"
;if(Ѐ.Contains("transponder")||Ѐ.Contains("relay"))return"TransponderBlock";if(Ѐ.Ǿ("light"))return"LightingBlock";if(Ѐ.Ǿ(
"contr"))return"ControlPanel";if(Ѐ.Ǿ("medi"))return"MedicalRoom";if(Ѐ.Ǿ("remote"))return"RemoteControl";if(Ѐ.Ǿ("but"))return
"ButtonPanel";if(Ѐ.Ǿ("cam"))return"CameraBlock";if(Ѐ.ǽ("detect"))return"OreDetector";if(Ѐ.Ǿ("safe"))return"SafeZoneBlock";if(Ѐ.Ǿ(
"store"))return"StoreBlock";if(Ѐ.Ǿ("contract"))return"ContractBlock";if(Ѐ.Ǿ("vending"))return"VendingMachine";if(Ѐ.Ǿ("decoy"))
return"Decoy";return"Unknown";}public string Ͽ(IMyBatteryBlock Ŋ){string Ͼ="";if(Ŋ.ChargeMode==ChargeMode.Recharge)Ͼ="(+) ";
else if(Ŋ.ChargeMode==ChargeMode.Discharge)Ͼ="(-) ";else Ͼ="(±) ";return Ͼ+e.ȍ((Ŋ.CurrentStoredPower/Ŋ.MaxStoredPower)*
100.0f)+"%";}Dictionary<MyLaserAntennaStatus,string>Ͻ=new Dictionary<MyLaserAntennaStatus,string>(){{MyLaserAntennaStatus.Idle
,"IDLE"},{MyLaserAntennaStatus.Connecting,"CONNECTING"},{MyLaserAntennaStatus.Connected,"CONNECTED"},{
MyLaserAntennaStatus.OutOfRange,"OUT OF RANGE"},{MyLaserAntennaStatus.RotatingToTarget,"ROTATING"},{MyLaserAntennaStatus.
SearchingTargetForAntenna,"SEARCHING"}};public string ϼ(IMyLaserAntenna ň){return Ͻ[ň.Status];}public double ϻ(IMyJumpDrive ŉ,out double ʫ,out
double Ɛ){ʫ=ŉ.CurrentStoredPower;Ɛ=ŉ.MaxStoredPower;return(Ɛ>0?ʫ/Ɛ*100:0);}public double Ϻ(IMyJumpDrive ŉ){double ʫ=ŉ.
CurrentStoredPower;double Ɛ=ŉ.MaxStoredPower;return(Ɛ>0?ʫ/Ɛ*100:0);}}class Ў:ɢ{ɹ e;Ĥ Ĕ;public int Ѝ=0;public Ў(ɹ Å,Ĥ a){ɡ="BootPanelsTask"
;ɞ=1;e=Å;Ĕ=a;if(!e.ɱ){Ѝ=int.MaxValue;Ĕ.ø=true;}}ǲ Đ;public override void ɼ(){Đ=e.Đ;}public override bool ɻ(bool ó){if(Ѝ>e
.ɰ.Count){ɽ();return true;}if(!ó&&Ѝ==0){Ĕ.ø=false;}if(!Њ(ó))return false;Ѝ++;return true;}public override void ɺ(){Ĕ.ø=
true;}public void Ќ(){ǔ å=Ĕ.å;for(int D=0;D<å.w();D++){æ b=å.o(D);b.K();}Ѝ=(e.ɱ?0:int.MaxValue);}int D;Ŭ Ћ=null;public bool
Њ(bool ó){ǔ å=Ĕ.å;if(!ó)D=0;int Љ=0;for(;D<å.w();D++){if(!ƣ.ɔ(40)||Љ>5)return false;æ b=å.o(D);Ћ=e.Ǚ(Ћ,b);float?Ј=b.Ü?.
FontSize;if(Ј!=null&&Ј>3f)continue;if(Ћ.ŷ.Count<=0)Ћ.ų(e.Ǜ(null,b));else e.Ǜ(Ћ.ŷ[0],b);e.Ţ();e.ǂ(Đ.Ȃ("B1"));double ʧ=(double)Ѝ/e
.ɰ.Count*100;e.Ǣ(ʧ);if(Ѝ==e.ɰ.Count){e.ǘ("");e.ǂ("Version 2.0200");e.ǂ("by MMaster");e.ǂ("");e.ǂ("übersetzt von Ich_73");}else e.Ǘ(e.ɰ[Ѝ]);bool Ý=b.Ý;b.Ý=
false;e.ȉ(b,Ћ);b.Ý=Ý;Љ++;}return true;}public bool ϗ(){return Ѝ<=e.ɰ.Count;}}public enum ύ{ʲ=0,ˤ=1,ˣ=2,ˢ=3,ˡ=4,ˠ=5,ˑ=6,ː=7,ˏ=
8,ˎ=9,ˍ=10,ˌ=11,ˋ=12,ˊ=13,ˉ=14,ˈ=15,ˇ=16,ˆ=17,ˁ=18,ˀ=19,ʿ=20,ʾ=21,ʽ=22,ʼ=23,ˬ=24,Ͱ=25,Α=26,Β=27,ΐ=28,Ώ=29,Ύ=30,Ό=31,}
class Ί{Ȥ ƣ;public string Ή="";public string Έ="";public string Ά="";public string ͽ="";public ύ ͼ=ύ.ʲ;public Ί(Ȥ ƨ){ƣ=ƨ;}ύ ͻ
(){if(Ή=="echo"||Ή=="center"||Ή=="right")return ύ.ˤ;if(Ή.StartsWith("hscroll"))return ύ.Ύ;if(Ή.StartsWith("inventory")||Ή
.StartsWith("missing")||Ή.StartsWith("invlist"))return ύ.ˣ;if(Ή.StartsWith("working"))return ύ.ˁ;if(Ή.StartsWith("cargo")
)return ύ.ˢ;if(Ή.StartsWith("mass"))return ύ.ˡ;if(Ή.StartsWith("shipmass"))return ύ.ʼ;if(Ή=="oxygen")return ύ.ˠ;if(Ή.
StartsWith("tanks"))return ύ.ˑ;if(Ή.StartsWith("powertime"))return ύ.ː;if(Ή.StartsWith("powerused"))return ύ.ˏ;if(Ή.StartsWith(
"power"))return ύ.ˎ;if(Ή.StartsWith("speed"))return ύ.ˍ;if(Ή.StartsWith("accel"))return ύ.ˌ;if(Ή.StartsWith("alti"))return ύ.Ͱ;
if(Ή.StartsWith("charge"))return ύ.ˋ;if(Ή.StartsWith("docked"))return ύ.Ό;if(Ή.StartsWith("time")||Ή.StartsWith("date"))
return ύ.ˊ;if(Ή.StartsWith("countdown"))return ύ.ˉ;if(Ή.StartsWith("textlcd"))return ύ.ˈ;if(Ή.EndsWith("count"))return ύ.ˇ;if(
Ή.StartsWith("dampeners")||Ή.StartsWith("occupied"))return ύ.ˆ;if(Ή.StartsWith("damage"))return ύ.ˀ;if(Ή.StartsWith(
"amount"))return ύ.ʿ;if(Ή.StartsWith("pos"))return ύ.ʾ;if(Ή.StartsWith("distance"))return ύ.ˬ;if(Ή.StartsWith("details"))return
ύ.ʽ;if(Ή.StartsWith("stop"))return ύ.Α;if(Ή.StartsWith("gravity"))return ύ.Β;if(Ή.StartsWith("customdata"))return ύ.ΐ;if(
Ή.StartsWith("prop"))return ύ.Ώ;return ύ.ʲ;}public ƚ ͺ(){switch(ͼ){case ύ.ˤ:return new ӌ();case ύ.ˣ:return new ҵ();case ύ
.ˢ:return new ί();case ύ.ˡ:return new ӓ();case ύ.ˠ:return new Ӓ();case ύ.ˑ:return new Ѻ();case ύ.ː:return new ѡ();case ύ.
ˏ:return new н();case ύ.ˎ:return new ӛ();case ύ.ˍ:return new ѫ();case ύ.ˌ:return new ʩ();case ύ.ˋ:return new ζ();case ύ.ˊ
:return new Ϊ();case ύ.ˉ:return new Ρ();case ύ.ˈ:return new Ķ();case ύ.ˇ:return new ʤ();case ύ.ˆ:return new Ѳ();case ύ.ˁ:
return new ļ();case ύ.ˀ:return new Ζ();case ύ.ʿ:return new ӥ();case ύ.ʾ:return new ӝ();case ύ.ʽ:return new Υ();case ύ.ʼ:return
new ѱ();case ύ.ˬ:return new ҿ();case ύ.Ͱ:return new ʦ();case ύ.Α:return new Ѯ();case ύ.Β:return new Ӌ();case ύ.ΐ:return new
Ι();case ύ.Ώ:return new Қ();case ύ.Ύ:return new ӊ();case ύ.Ό:return new Ӆ();default:return new ƚ();}}public List<ʰ>ͷ=new
List<ʰ>();string[]Ͷ=null;string ʹ="";bool ͳ=false;int Ř=1;public bool ʬ(string Ͳ,bool ó){if(!ó){ͼ=ύ.ʲ;Έ="";Ή="";Ά=Ͳ.
TrimStart(' ');ͷ.Clear();if(Ά=="")return true;int ͱ=Ά.IndexOf(' ');if(ͱ<0||ͱ>=Ά.Length-1)ͽ="";else ͽ=Ά.Substring(ͱ+1);Ͷ=Ά.Split(
' ');ʹ="";ͳ=false;Ή=Ͷ[0].ToLower();Ř=1;}for(;Ř<Ͷ.Length;Ř++){if(!ƣ.ɔ(40))return false;string Ő=Ͷ[Ř];if(Ő=="")continue;if(Ő[
0]=='{'&&Ő[Ő.Length-1]=='}'){Ő=Ő.Substring(1,Ő.Length-2);if(Ő=="")continue;if(Έ=="")Έ=Ő;else ͷ.Add(new ʰ(Ő));continue;}if
(Ő[0]=='{'){ͳ=true;ʹ=Ő.Substring(1);continue;}if(Ő[Ő.Length-1]=='}'){ͳ=false;ʹ+=' '+Ő.Substring(0,Ő.Length-1);if(Έ=="")Έ=
ʹ;else ͷ.Add(new ʰ(ʹ));continue;}if(ͳ){if(ʹ.Length!=0)ʹ+=' ';ʹ+=Ő;continue;}if(Έ=="")Έ=Ő;else ͷ.Add(new ʰ(Ő));}ͼ=ͻ();
return true;}}class ʰ{public string ʻ="";public string ʯ="";public string Ő="";public List<string>ʮ=new List<string>();public
ʰ(string ʭ){Ő=ʭ;}public void ʬ(){if(Ő==""||ʻ!=""||ʯ!=""||ʮ.Count>0)return;string ʫ=Ő.Trim();if(ʫ[0]=='+'||ʫ[0]=='-'){ʻ+=ʫ
[0];ʫ=Ő.Substring(1);}string[]ƥ=ʫ.Split('/');string ʪ=ƥ[0];if(ƥ.Length>1){ʯ=ƥ[0];ʪ=ƥ[1];}else ʯ="";if(ʪ.Length>0){string[
]ć=ʪ.Split(',');for(int D=0;D<ć.Length;D++)if(ć[D]!="")ʮ.Add(ć[D]);}}}class ʩ:ƚ{public ʩ(){ɞ=0.5;ɡ="CmdAccel";}public
override bool Ɩ(bool ó){double ʨ=0;if(ƙ.Έ!="")double.TryParse(ƙ.Έ.Trim(),out ʨ);e.Ô(Đ.Ȃ("AC1")+" ");e.Ƹ(e.ǉ.ʏ.ToString("F1")+
" m/s²");if(ʨ>0){double ʧ=e.ǉ.ʏ/ʨ*100;e.Ǣ(ʧ);}return true;}}class ʦ:ƚ{public ʦ(){ɞ=1;ɡ="CmdAltitude";}public override bool Ɩ(
bool ó){string ʥ=(ƙ.Ή.EndsWith("sea")?"sea":"ground");switch(ʥ){case"sea":e.Ô(Đ.Ȃ("ALT1"));e.Ƹ(e.ǉ.ʅ.ToString("F0")+" m");
break;default:e.Ô(Đ.Ȃ("ALT2"));e.Ƹ(e.ǉ.ʃ.ToString("F0")+" m");break;}return true;}}class ʤ:ƚ{public ʤ(){ɞ=15;ɡ=
"CmdBlockCount";}Ϗ Ņ;public override void ɼ(){Ņ=new Ϗ(ƣ,e.Ŏ);}bool ʣ;bool ʱ;int Ř=0;int Ĝ=0;public override bool Ɩ(bool ó){if(!ó){ʣ=(ƙ.
Ή=="enabledcount");ʱ=(ƙ.Ή=="prodcount");Ř=0;Ĝ=0;}if(ƙ.ͷ.Count==0){if(Ĝ==0){if(!ó)Ņ.Ų();if(!Ņ.А(ƙ.Έ,ó))return false;Ĝ++;ó=
false;}if(!ʴ(Ņ,"blocks",ʣ,ʱ,ó))return false;return true;}for(;Ř<ƙ.ͷ.Count;Ř++){ʰ Ő=ƙ.ͷ[Ř];if(!ó)Ő.ʬ();if(!ő(Ő,ó))return false
;ó=false;}return true;}int ŕ=0;int Ŗ=0;bool ő(ʰ Ő,bool ó){if(!ó){ŕ=0;Ŗ=0;}for(;ŕ<Ő.ʮ.Count;ŕ++){if(Ŗ==0){if(!ó)Ņ.Ų();if(!
Ņ.Б(Ő.ʮ[ŕ],ƙ.Έ,ó))return false;Ŗ++;ó=false;}if(!ʴ(Ņ,Ő.ʮ[ŕ],ʣ,ʱ,ó))return false;Ŗ=0;ó=false;}return true;}Dictionary<
string,int>ʺ=new Dictionary<string,int>();Dictionary<string,int>ʹ=new Dictionary<string,int>();List<string>ʸ=new List<string>(
30);int ĉ=0;int ʷ=0;int ʶ=0;ʞ ʵ=new ʞ();bool ʴ(Ϗ Ă,string ʥ,bool ʣ,bool ʱ,bool ó){if(Ă.Ц()==0){ʵ.Ų().ʙ(char.ToUpper(ʥ[0]))
.ʙ(ʥ.ToLower(),1,ʥ.Length-1);e.Ô(ʵ.ʙ(" ").ʙ(Đ.Ȃ("C1")).ʙ(" "));string ʳ=(ʣ||ʱ?"0 / 0":"0");e.Ƹ(ʳ);return true;}if(!ó){ʺ.
Clear();ʹ.Clear();ʸ.Clear();ĉ=0;ʷ=0;ʶ=0;}if(ʶ==0){for(;ĉ<Ă.Ц();ĉ++){if(!ƣ.ɔ(15))return false;var ō=Ă.ϩ[ĉ]as
IMyProductionBlock;ʵ.Ų().ʙ(Ă.ϩ[ĉ].DefinitionDisplayNameText);string ȇ=ʵ.ɤ();if(ʸ.Contains(ȇ)){ʺ[ȇ]++;if((ʣ&&Ă.ϩ[ĉ].IsWorking)||(ʱ&&ō!=null
&&ō.IsProducing))ʹ[ȇ]++;}else{ʺ.Add(ȇ,1);ʸ.Add(ȇ);if(ʣ||ʱ)if((ʣ&&Ă.ϩ[ĉ].IsWorking)||(ʱ&&ō!=null&&ō.IsProducing))ʹ.Add(ȇ,1)
;else ʹ.Add(ȇ,0);}}ʶ++;ó=false;}for(;ʷ<ʺ.Count;ʷ++){if(!ƣ.ɔ(8))return false;e.Ô(ʸ[ʷ]+" "+Đ.Ȃ("C1")+" ");string ʳ=(ʣ||ʱ?ʹ[
ʸ[ʷ]]+" / ":"")+ʺ[ʸ[ʷ]];e.Ƹ(ʳ);}return true;}}class ί:ƚ{Ϗ Ņ;public ί(){ɞ=2;ɡ="CmdCargo";}public override void ɼ(){Ņ=new Ϗ
(ƣ,e.Ŏ);}bool μ=true;bool Ε=false;bool λ=false;bool δ=false;double κ=0;double ι=0;string θ;int Ĝ=0;public override bool Ɩ
(bool ó){if(!ó){Ņ.Ų();μ=ƙ.Ή.Contains("all");δ=ƙ.Ή.EndsWith("bar");Ε=(ƙ.Ή[ƙ.Ή.Length-1]=='x');λ=(ƙ.Ή[ƙ.Ή.Length-1]=='p');κ
=0;ι=0;θ="";Ĝ=0;}if(Ĝ==0){if(μ){if(!Ņ.А(ƙ.Έ,ó))return false;}else{if(!Ņ.Б("cargocontainer",ƙ.Έ,ó))return false;}Ĝ++;ó=
false;}double η=Ņ.ϥ(ref κ,ref ι,ó);if(Double.IsNaN(η))return false;if(δ){e.Ǣ(η);return true;}if(ƙ.ͷ.Count>0){if(ƙ.ͷ[0].Ő.
Length>0)θ=ƙ.ͷ[0].Ő;}e.Ô((θ==""?Đ.Ȃ("C2"):θ)+" ");if(!Ε&&!λ){e.Ƹ(e.Ȗ(κ)+"L / "+e.Ȗ(ι)+"L");e.ƿ(η,1.0f,e.Ǔ);e.ǘ(' '+e.ȍ(η)+"%")
;}else if(λ){e.Ƹ(e.ȍ(η)+"%");e.Ǣ(η);}else e.Ƹ(e.ȍ(η)+"%");return true;}}class ζ:ƚ{public ζ(){ɞ=3;ɡ="CmdCharge";}Ϗ Ņ;bool
Ε=false;bool ε=false;bool δ=false;bool γ=false;public override void ɼ(){Ņ=new Ϗ(ƣ,e.Ŏ);if(ƙ.ͷ.Count>0)ν=ƙ.ͷ[0].Ő;δ=ƙ.Ή.
EndsWith("bar");Ε=ƙ.Ή.Contains("x");ε=ƙ.Ή.Contains("time");γ=ƙ.Ή.Contains("sum");}int Ĝ=0;int ĉ=0;double β=0;double α=0;TimeSpan
ΰ=TimeSpan.Zero;string ν="";Dictionary<long,double>í=new Dictionary<long,double>();Dictionary<long,double>ϋ=new
Dictionary<long,double>();Dictionary<long,double>ό=new Dictionary<long,double>();Dictionary<long,double>ϊ=new Dictionary<long,
double>();Dictionary<long,double>ω=new Dictionary<long,double>();double ψ(long χ,double ʫ,double Ɛ){double φ=0;double υ=0;
double τ=0;double σ=0;if(ϋ.TryGetValue(χ,out τ)){σ=ϊ[χ];}if(í.TryGetValue(χ,out φ)){υ=ό[χ];}double ς=(ƣ.ȡ-τ);double ρ=0;if(ς>0
)ρ=(ʫ-σ)/ς;if(ρ<0){if(!ω.TryGetValue(χ,out ρ))ρ=0;}else ω[χ]=ρ;if(φ>0){ϋ[χ]=í[χ];ϊ[χ]=ό[χ];}í[χ]=ƣ.ȡ;ό[χ]=ʫ;return(ρ>0?(Ɛ
-ʫ)/ρ:0);}private void π(string ȇ,double ʧ,double ʫ,double Ɛ,TimeSpan ο){if(δ){e.Ǣ(ʧ);}else{e.Ô(ȇ+" ");if(ε){e.Ƹ(e.Ǌ.Ȧ(ο)
);if(!Ε){e.ƿ(ʧ,1.0f,e.Ǔ);e.Ƹ(' '+ʧ.ToString("0.0")+"%");}}else{if(!Ε){e.Ƹ(e.Ȗ(ʫ)+"Wh / "+e.Ȗ(Ɛ)+"Wh");e.ƿ(ʧ,1.0f,e.Ǔ);}e.
Ƹ(' '+ʧ.ToString("0.0")+"%");}}}public override bool Ɩ(bool ó){if(!ó){Ņ.Ų();ĉ=0;Ĝ=0;β=0;α=0;ΰ=TimeSpan.Zero;}if(Ĝ==0){if(
!Ņ.Б("jumpdrive",ƙ.Έ,ó))return false;if(Ņ.Ц()<=0){e.ǘ("Charge: "+Đ.Ȃ("D2"));return true;}Ĝ++;ó=false;}for(;ĉ<Ņ.Ц();ĉ++){
if(!ƣ.ɔ(25))return false;var ŉ=Ņ.ϩ[ĉ]as IMyJumpDrive;double ʫ,Ɛ,ʧ;ʧ=e.Ŏ.ϻ(ŉ,out ʫ,out Ɛ);TimeSpan ξ;if(ε)try{ξ=TimeSpan.
FromSeconds(ψ(ŉ.EntityId,ʫ,Ɛ));}catch{ξ=new TimeSpan(-1);}else ξ=TimeSpan.Zero;if(!γ){π(ŉ.CustomName,ʧ,ʫ,Ɛ,ξ);}else{β+=ʫ;α+=Ɛ;if(ΰ<
ξ)ΰ=ξ;}}if(γ){double ή=(α>0?β/α*100:0);π(ν,ή,β,α,ΰ);}return true;}}class Ρ:ƚ{public Ρ(){ɞ=1;ɡ="CmdCountdown";}public
override bool Ɩ(bool ó){bool Π=ƙ.Ή.EndsWith("c");bool Ο=ƙ.Ή.EndsWith("r");string Ξ="";int Ν=ƙ.Ά.IndexOf(' ');if(Ν>=0)Ξ=ƙ.Ά.
Substring(Ν+1).Trim();DateTime Μ=DateTime.Now;DateTime Λ;if(!DateTime.TryParseExact(Ξ,"H:mm d.M.yyyy",System.Globalization.
CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None,out Λ)){e.ǘ(Đ.Ȃ("C3"));e.ǘ("  Countdown 19:02 28.2.2015");
return true;}TimeSpan Κ=Λ-Μ;string þ="";if(Κ.Ticks<=0)þ=Đ.Ȃ("C4");else{if((int)Κ.TotalDays>0)þ+=(int)Κ.TotalDays+" "+Đ.Ȃ("C5")
+" ";if(Κ.Hours>0||þ!="")þ+=Κ.Hours+"h ";if(Κ.Minutes>0||þ!="")þ+=Κ.Minutes+"m ";þ+=Κ.Seconds+"s";}if(Π)e.ǂ(þ);else if(Ο)
e.Ƹ(þ);else e.ǘ(þ);return true;}}class Ι:ƚ{public Ι(){ɞ=1;ɡ="CmdCustomData";}public override bool Ɩ(bool ó){string þ="";
if(ƙ.Έ!=""&&ƙ.Έ!="*"){var Η=e.ǈ.GetBlockWithName(ƙ.Έ)as IMyTerminalBlock;if(Η==null){e.ǘ("CustomData: "+Đ.Ȃ("CD1")+ƙ.Έ);
return true;}þ=Η.CustomData;}else{e.ǘ("CustomData:"+Đ.Ȃ("CD2"));return true;}if(þ.Length==0)return true;e.Ǘ(þ);return true;}}
class Ζ:ƚ{public Ζ(){ɞ=5;ɡ="CmdDamage";}Ϗ Ņ;public override void ɼ(){Ņ=new Ϗ(ƣ,e.Ŏ);}bool ƈ=false;int ĉ=0;public override
bool Ɩ(bool ó){bool Ε=ƙ.Ή.StartsWith("damagex");bool Δ=ƙ.Ή.EndsWith("noc");bool Θ=(!Δ&&ƙ.Ή.EndsWith("c"));float Γ=100;if(!ó)
{Ņ.Ų();ƈ=false;ĉ=0;}if(!Ņ.А(ƙ.Έ,ó))return false;if(ƙ.ͷ.Count>0){if(!float.TryParse(ƙ.ͷ[0].Ő,out Γ))Γ=100;}Γ-=0.00001f;for
(;ĉ<Ņ.Ц();ĉ++){if(!ƣ.ɔ(30))return false;IMyTerminalBlock ã=Ņ.ϩ[ĉ];IMySlimBlock έ=ã.CubeGrid.GetCubeBlock(ã.Position);if(έ
==null)continue;float ά=(Δ?έ.MaxIntegrity:έ.BuildIntegrity);if(!Θ)ά-=έ.CurrentDamage;float ʧ=100*(ά/έ.MaxIntegrity);if(ʧ>=
Γ)continue;ƈ=true;string Ϋ=e.Ǭ(έ.FatBlock.DisplayNameText,e.ɫ*0.69f-e.Ǔ);e.Ô(Ϋ+' ');if(!Ε){e.Ƴ(e.Ȗ(ά)+" / ",0.69f);e.Ô(e.
Ȗ(έ.MaxIntegrity));}e.Ƹ(' '+ʧ.ToString("0.0")+'%');e.Ǣ(ʧ);}if(!ƈ)e.ǘ(Đ.Ȃ("D3"));return true;}}class Ϊ:ƚ{public Ϊ(){ɞ=1;ɡ=
"CmdDateTime";}public override bool Ɩ(bool ó){bool Ω=(ƙ.Ή.StartsWith("datetime"));bool Ψ=(ƙ.Ή.StartsWith("date"));bool Π=ƙ.Ή.Contains
("c");int Χ=ƙ.Ή.IndexOf('+');if(Χ<0)Χ=ƙ.Ή.IndexOf('-');float Φ=0;if(Χ>=0)float.TryParse(ƙ.Ή.Substring(Χ),out Φ);DateTime
Κ=DateTime.Now.AddHours(Φ);string þ="";int Ν=ƙ.Ά.IndexOf(' ');if(Ν>=0)þ=ƙ.Ά.Substring(Ν+1);if(!Ω){if(!Ψ)þ+=Κ.
ToShortTimeString();else þ+=Κ.ToShortDateString();}else{if(þ=="")þ=String.Format("{0:d} {0:t}",Κ);else{þ=þ.Replace("/","\\/");þ=þ.Replace
(":","\\:");þ=þ.Replace("\"","\\\"");þ=þ.Replace("'","\\'");þ=Κ.ToString(þ+' ');þ=þ.Substring(0,þ.Length-1);}}if(Π)e.ǂ(þ)
;else e.ǘ(þ);return true;}}class Υ:ƚ{public Υ(){ɞ=5;ɡ="CmdDetails";}string Τ="";string Σ="";int ŝ=0;Ϗ Ņ;public override
void ɼ(){Ņ=new Ϗ(ƣ,e.Ŏ);if(ƙ.ͷ.Count>0)Τ=ƙ.ͷ[0].Ő.Trim();if(ƙ.ͷ.Count>1){string Ő=ƙ.ͷ[1].Ő.Trim();if(!int.TryParse(Ő,out ŝ))
{ŝ=0;Σ=Ő;}}}int Ĝ=0;int ĉ=1;bool ˮ=false;IMyTerminalBlock ã;public override bool Ɩ(bool ó){if(ƙ.Έ==""||ƙ.Έ=="*"){e.ǘ(
"Details: "+Đ.Ȃ("D1"));return true;}if(!ó){Ņ.Ų();ˮ=ƙ.Ή.Contains("non");Ĝ=0;ĉ=1;}if(Ĝ==0){if(!Ņ.А(ƙ.Έ,ó))return true;if(Ņ.Ц()<=0){e.
ǘ("Details: "+Đ.Ȃ("D2"));return true;}Ĝ++;ó=false;}int ӆ=(ƙ.Ή.EndsWith("x")?1:0);if(Ĝ==1){if(!ó){ã=Ņ.ϩ[0];if(!ˮ)e.ǘ(ã.
CustomName);}if(!Ӂ(ã,ӆ,ŝ,ó))return false;Ĝ++;ó=false;}for(;ĉ<Ņ.Ц();ĉ++){if(!ó){ã=Ņ.ϩ[ĉ];if(!ˮ){e.ǘ("");e.ǘ(ã.CustomName);}}if(!Ӂ(ã
,ӆ,ŝ,ó))return false;ó=false;}return true;}string[]ţ;int ӄ=0;int Ӄ=0;bool ӂ=false;ʞ Ʒ=new ʞ();bool Ӂ(IMyTerminalBlock ã,
int Ӏ,int ĺ,bool ó){if(!ó){ţ=Ʒ.Ų().ʙ(ã.DetailedInfo).ʙ('\n').ʙ(ã.CustomInfo).ɤ().Split('\n');ӄ=Ӏ;ӂ=(Τ.Length==0);Ӄ=0;}for(;
ӄ<ţ.Length;ӄ++){if(!ƣ.ɔ(5))return false;if(ţ[ӄ].Length==0)continue;if(!ӂ){if(!ţ[ӄ].Contains(Τ))continue;ӂ=true;}if(Σ.
Length>0&&ţ[ӄ].Contains(Σ))return true;e.ǘ(Ʒ.Ų().ʙ("  ").ʙ(ţ[ӄ]));Ӄ++;if(ĺ>0&&Ӄ>=ĺ)return true;}return true;}}class ҿ:ƚ{public
ҿ(){ɞ=1;ɡ="CmdDistance";}string Ҿ="";string[]ҽ;Vector3D Ҽ;string һ="";bool Һ=false;public override void ɼ(){Һ=false;if(ƙ.
ͷ.Count<=0)return;Ҿ=ƙ.ͷ[0].Ő.Trim();ҽ=Ҿ.Split(':');if(ҽ.Length<5||ҽ[0]!="GPS")return;double ҹ,Ҹ,ҷ;if(!double.TryParse(ҽ[2
],out ҹ))return;if(!double.TryParse(ҽ[3],out Ҹ))return;if(!double.TryParse(ҽ[4],out ҷ))return;Ҽ=new Vector3D(ҹ,Ҹ,ҷ);һ=ҽ[1
];Һ=true;}public override bool Ɩ(bool ó){if(!Һ){e.ǘ("Distance: "+Đ.Ȃ("DTU")+" '"+Ҿ+"'.");return true;}IMyTerminalBlock ã=
a.C.ã;if(ƙ.Έ!=""&&ƙ.Έ!="*"){ã=e.ǈ.GetBlockWithName(ƙ.Έ);if(ã==null){e.ǘ("Distance: "+Đ.Ȃ("P1")+": "+ƙ.Έ);return true;}}
double ѻ=Vector3D.Distance(ã.GetPosition(),Ҽ);e.Ô(һ+": ");e.Ƹ(e.Ȗ(ѻ)+"m ");return true;}}class Ӆ:ƚ{Ϗ Ņ;public Ӆ(){ɞ=2;ɡ=
"CmdDocked";}public override void ɼ(){Ņ=new Ϗ(ƣ,e.Ŏ);}int Ĝ=0;int ӏ=0;bool ӎ=false;bool Ӎ=false;IMyShipConnector ņ;public override
bool Ɩ(bool ó){if(!ó){if(ƙ.Ή.EndsWith("e"))ӎ=true;if(ƙ.Ή.Contains("cn"))Ӎ=true;Ņ.Ų();Ĝ=0;}if(Ĝ==0){if(!Ņ.Б("connector",ƙ.Έ,ó
))return false;Ĝ++;ӏ=0;ó=false;}if(Ņ.Ц()<=0){e.ǘ("Docked: "+Đ.Ȃ("DO1"));return true;}for(;ӏ<Ņ.Ц();ӏ++){ņ=Ņ.ϩ[ӏ]as
IMyShipConnector;if(ņ.Status==MyShipConnectorStatus.Connected){if(Ӎ){e.Ô(ņ.CustomName+":");e.Ƹ(ņ.OtherConnector.CubeGrid.CustomName);}
else{e.ǘ(ņ.OtherConnector.CubeGrid.CustomName);}}else{if(ӎ){if(Ӎ){e.Ô(ņ.CustomName+":");e.Ƹ("-");}else e.ǘ("-");}}}return
true;}}class ӌ:ƚ{public ӌ(){ɞ=30;ɡ="CmdEcho";}public override bool Ɩ(bool ó){string ʥ=(ƙ.Ή=="center"?"c":(ƙ.Ή=="right"?"r":
"n"));switch(ʥ){case"c":e.ǂ(ƙ.ͽ);break;case"r":e.Ƹ(ƙ.ͽ);break;default:e.ǘ(ƙ.ͽ);break;}return true;}}class Ӌ:ƚ{public Ӌ(){ɞ=
1;ɡ="CmdGravity";}public override bool Ɩ(bool ó){string ʥ=(ƙ.Ή.Contains("nat")?"n":(ƙ.Ή.Contains("art")?"a":(ƙ.Ή.Contains
("tot")?"t":"s")));Vector3D Д;if(e.ǉ.ʀ==null){e.ǘ("Gravity: "+Đ.Ȃ("GNC"));return true;}switch(ʥ){case"n":e.Ô(Đ.Ȃ("G2")+
" ");Д=e.ǉ.ʀ.GetNaturalGravity();e.Ƹ(Д.Length().ToString("F1")+" m/s²");break;case"a":e.Ô(Đ.Ȃ("G3")+" ");Д=e.ǉ.ʀ.
GetArtificialGravity();e.Ƹ(Д.Length().ToString("F1")+" m/s²");break;case"t":e.Ô(Đ.Ȃ("G1")+" ");Д=e.ǉ.ʀ.GetTotalGravity();e.Ƹ(Д.Length().
ToString("F1")+" m/s²");break;default:e.Ô(Đ.Ȃ("GN"));e.Ƴ(" | ",0.33f);e.Ƴ(Đ.Ȃ("GA")+" | ",0.66f);e.Ƹ(Đ.Ȃ("GT"),1.0f);e.Ô("");Д=e
.ǉ.ʀ.GetNaturalGravity();e.Ƴ(Д.Length().ToString("F1")+" | ",0.33f);Д=e.ǉ.ʀ.GetArtificialGravity();e.Ƴ(Д.Length().
ToString("F1")+" | ",0.66f);Д=e.ǉ.ʀ.GetTotalGravity();e.Ƹ(Д.Length().ToString("F1")+" ");break;}return true;}}class ӊ:ƚ{public ӊ
(){ɞ=0.5;ɡ="CmdHScroll";}ʞ Ӊ=new ʞ();int ӈ=1;public override bool Ɩ(bool ó){if(Ӊ.ʜ==0){string þ=ƙ.ͽ+"  ";if(þ.Length==0)
return true;float Ӈ=e.ɫ;float ƶ=e.Ǯ(þ,e.Ǟ);float Ѫ=Ӈ/ƶ;if(Ѫ>1)Ӊ.ʙ(string.Join("",Enumerable.Repeat(þ,(int)Math.Ceiling(Ѫ))));
else Ӊ.ʙ(þ);if(þ.Length>40)ӈ=3;else if(þ.Length>5)ӈ=2;else ӈ=1;e.ǘ(Ӊ);return true;}bool Ο=ƙ.Ή.EndsWith("r");if(Ο){Ӊ.Ʒ.Insert
(0,Ӊ.ɤ(Ӊ.ʜ-ӈ,ӈ));Ӊ.ɦ(Ӊ.ʜ-ӈ,ӈ);}else{Ӊ.ʙ(Ӊ.ɤ(0,ӈ));Ӊ.ɦ(0,ӈ);}e.ǘ(Ӊ);return true;}}class ҵ:ƚ{public ҵ(){ɞ=7;ɡ="CmdInvList";
}float Ҵ=-1;float ҩ=-1;public override void ɼ(){Ņ=new Ϗ(ƣ,e.Ŏ);ү=new ƕ(ƣ,e);}ʞ Ʒ=new ʞ(100);Dictionary<string,string>Ҩ=
new Dictionary<string,string>();void ҧ(string ȳ,double Ҧ,int Ó){if(Ó>0){if(!Ҫ)e.ƿ(Math.Min(100,100*Ҧ/Ó),0.3f);string Ϋ;if(Ҩ
.ContainsKey(ȳ)){Ϋ=Ҩ[ȳ];}else{if(!ҫ)Ϋ=e.Ǭ(ȳ,e.ɫ*0.5f-ң-ҩ);else{if(!Ҫ)Ϋ=e.Ǭ(ȳ,e.ɫ*0.69f);else Ϋ=e.Ǭ(ȳ,e.ɫ*0.99f);}Ҩ[ȳ]=Ϋ;}
Ʒ.Ų();if(!Ҫ)Ʒ.ʙ(' ');if(!ҫ){e.Ô(Ʒ.ʙ(Ϋ).ʙ(' '));e.Ƴ(e.Ȗ(Ҧ),1.0f,ң+ҩ);e.ǘ(Ʒ.Ų().ʙ(" / ").ʙ(e.Ȗ(Ó)));}else{e.ǘ(Ʒ.ʙ(Ϋ));}}
else{if(!ҫ){e.Ô(Ʒ.Ų().ʙ(ȳ).ʙ(':'));e.Ƹ(e.Ȗ(Ҧ),1.0f,Ҵ);}else e.ǘ(Ʒ.Ų().ʙ(ȳ));}}void ҥ(string ȳ,double Ҧ,double Ҥ,int Ó){if(Ó>
0){if(!ҫ){e.Ô(Ʒ.Ų().ʙ(ȳ).ʙ(' '));e.Ƴ(e.Ȗ(Ҧ),0.51f);e.Ô(Ʒ.Ų().ʙ(" / ").ʙ(e.Ȗ(Ó)));e.Ƹ(Ʒ.Ų().ʙ(" +").ʙ(e.Ȗ(Ҥ)).ʙ(" ").ʙ(Đ.Ȃ
("I1")),1.0f);}else e.ǘ(Ʒ.Ų().ʙ(ȳ));if(!Ҫ)e.Ǣ(Math.Min(100,100*Ҧ/Ó));}else{if(!ҫ){e.Ô(Ʒ.Ų().ʙ(ȳ).ʙ(':'));e.Ƴ(e.Ȗ(Ҧ),0.51f
);e.Ƹ(Ʒ.Ų().ʙ(" +").ʙ(e.Ȗ(Ҥ)).ʙ(" ").ʙ(Đ.Ȃ("I1")),1.0f);}else{e.ǘ(Ʒ.Ų().ʙ(ȳ));}}}float ң=0;bool Ң(ƒ ſ){int Ó=(ҭ?ſ.Ƒ:ſ.Ɛ);
if(Ó<0)return true;float Ƽ=e.Ǯ(e.Ȗ(Ó),e.Ǟ);if(Ƽ>ң)ң=Ƽ;return true;}List<ƒ>ҡ;int Ҡ=0;int ҟ=0;bool Ҟ(bool ó,bool ҝ,string Ë,
string м){if(!ó){ҟ=0;Ҡ=0;}if(ҟ==0){if(Ӡ){if((ҡ=ү.Ɓ(Ë,ó,Ң))==null)return false;}else{if((ҡ=ү.Ɓ(Ë,ó))==null)return false;}ҟ++;ó=
false;}if(ҡ.Count>0){if(!ҝ&&!ó){if(!e.ǝ)e.ǘ();e.ǂ(Ʒ.Ų().ʙ("<< ").ʙ(м).ʙ(" ").ʙ(Đ.Ȃ("I2")).ʙ(" >>"));}for(;Ҡ<ҡ.Count;Ҡ++){if(!
ƣ.ɔ(30))return false;double Ҧ=ҡ[Ҡ].Ǝ;if(ҭ&&Ҧ>=ҡ[Ҡ].Ƒ)continue;int Ó=ҡ[Ҡ].Ɛ;if(ҭ)Ó=ҡ[Ҡ].Ƒ;string ȳ=e.ȃ(ҡ[Ҡ].Ì,ҡ[Ҡ].Ë);ҧ(ȳ,
Ҧ,Ó);}}return true;}List<ƒ>ҳ;int Ҳ=0;int ұ=0;bool Ұ(bool ó){if(!ó){Ҳ=0;ұ=0;}if(ұ==0){if((ҳ=ү.Ɓ("Ingot",ó))==null)return
false;ұ++;ó=false;}if(ҳ.Count>0){if(!Ҭ&&!ó){if(!e.ǝ)e.ǘ();e.ǂ(Ʒ.Ų().ʙ("<< ").ʙ(Đ.Ȃ("I4")).ʙ(" ").ʙ(Đ.Ȃ("I2")).ʙ(" >>"));}for(
;Ҳ<ҳ.Count;Ҳ++){if(!ƣ.ɔ(40))return false;double Ҧ=ҳ[Ҳ].Ǝ;if(ҭ&&Ҧ>=ҳ[Ҳ].Ƒ)continue;int Ó=ҳ[Ҳ].Ɛ;if(ҭ)Ó=ҳ[Ҳ].Ƒ;string ȳ=e.ȃ
(ҳ[Ҳ].Ì,ҳ[Ҳ].Ë);if(ҳ[Ҳ].Ì!="Scrap"){double Ҥ=ү.Ɔ(ҳ[Ҳ].Ì+" Ore",ҳ[Ҳ].Ì,"Ore").Ǝ;ҥ(ȳ,Ҧ,Ҥ,Ó);}else ҧ(ȳ,Ҧ,Ó);}}return true;}Ϗ
Ņ=null;ƕ ү;List<ʰ>ͷ;bool Ү,Ε,ҭ,Ҭ,ҫ,Ҫ;int Ř,ŕ;string Ҷ="";float Ӑ=0;bool Ӡ=true;void Ӥ(){if(e.Ǟ!=Ҷ||Ӑ!=e.ɫ){Ҩ.Clear();Ӑ=e.
ɫ;}if(e.Ǟ!=Ҷ){ҩ=e.Ǯ(" / ",e.Ǟ);Ҵ=e.Ǳ(' ',e.Ǟ);Ҷ=e.Ǟ;}Ņ.Ų();Ү=ƙ.Ή.EndsWith("x")||ƙ.Ή.EndsWith("xs");Ε=ƙ.Ή.EndsWith("s")||ƙ
.Ή.EndsWith("sx");ҭ=ƙ.Ή.StartsWith("missing");Ҭ=ƙ.Ή.Contains("list");Ҫ=ƙ.Ή.Contains("nb");ҫ=ƙ.Ή.Contains("nn");Ӡ=true;ү.Ų
();ͷ=ƙ.ͷ;if(ͷ.Count==0)ͷ.Add(new ʰ("all"));}bool ӣ(bool ó){if(!ó)Ř=0;for(;Ř<ͷ.Count;Ř++){ʰ Ő=ͷ[Ř];Ő.ʬ();string Ë=Ő.ʯ;if(!
ó)ŕ=0;else ó=false;for(;ŕ<Ő.ʮ.Count;ŕ++){if(!ƣ.ɔ(30))return false;string[]ć=Ő.ʮ[ŕ].Split(':');double Ȕ;if(string.Compare(
ć[0],"all",true)==0)ć[0]="";int Ƒ=1;int Ɛ=-1;if(ć.Length>1){if(Double.TryParse(ć[1],out Ȕ)){if(ҭ)Ƒ=(int)Math.Ceiling(Ȕ);
else Ɛ=(int)Math.Ceiling(Ȕ);}}string Ʀ=ć[0];if(!string.IsNullOrEmpty(Ë))Ʀ+=' '+Ë;ү.Ƨ(Ʀ,Ő.ʻ=="-",Ƒ,Ɛ);}}return true;}int ѿ=0;
int ϟ=0;int Ӣ=0;List<MyInventoryItem>Z=new List<MyInventoryItem>(50);bool ӡ(bool ó){Ϗ Ч=Ņ;if(!ó)ѿ=0;for(;ѿ<Ч.ϩ.Count;ѿ++){
if(!ó)ϟ=0;for(;ϟ<Ч.ϩ[ѿ].InventoryCount;ϟ++){IMyInventory Ϟ=Ч.ϩ[ѿ].GetInventory(ϟ);if(!ó){Ӣ=0;Z.Clear();Ϟ.GetItems(Z);}else
ó=false;for(;Ӣ<Z.Count;Ӣ++){if(!ƣ.ɔ(40))return false;MyInventoryItem z=Z[Ӣ];string Î=e.Ȇ(z);string Ì,Ë;e.Ȅ(Î,out Ì,out Ë)
;if(string.Compare(Ë,"ore",true)==0){if(ү.Ɠ(Ì+" ingot",Ì,"Ingot")&&ү.Ɠ(Î,Ì,Ë))continue;}else{if(ү.Ɠ(Î,Ì,Ë))continue;}e.Ȅ(
Î,out Ì,out Ë);ƒ Ƅ=ү.Ɔ(Î,Ì,Ë);Ƅ.Ǝ+=(double)z.Amount;}}}return true;}int Ĝ=0;public override bool Ɩ(bool ó){if(!ó){Ӥ();Ĝ=0
;}for(;Ĝ<=13;Ĝ++){switch(Ĝ){case 0:if(!Ņ.А(ƙ.Έ,ó))return false;break;case 1:if(!ӣ(ó))return false;if(Ү)Ĝ++;break;case 2:
if(!ү.ƅ(ó))return false;break;case 3:if(!ӡ(ó))return false;break;case 4:if(!Ҟ(ó,Ҭ,"Ore",Đ.Ȃ("I3")))return false;break;case
5:if(Ε){if(!Ҟ(ó,Ҭ,"Ingot",Đ.Ȃ("I4")))return false;}else{if(!Ұ(ó))return false;}break;case 6:if(!Ҟ(ó,Ҭ,"Component",Đ.Ȃ(
"I5")))return false;break;case 7:if(!Ҟ(ó,Ҭ,"OxygenContainerObject",Đ.Ȃ("I6")))return false;break;case 8:if(!Ҟ(ó,true,
"GasContainerObject",""))return false;break;case 9:if(!Ҟ(ó,Ҭ,"AmmoMagazine",Đ.Ȃ("I7")))return false;break;case 10:if(!Ҟ(ó,Ҭ,
"PhysicalGunObject",Đ.Ȃ("I8")))return false;break;case 11:if(!Ҟ(ó,true,"Datapad",""))return false;break;case 12:if(!Ҟ(ó,true,
"ConsumableItem",""))return false;break;case 13:if(!Ҟ(ó,true,"PhysicalObject",""))return false;break;}ó=false;}Ӡ=false;return true;}}
class ӥ:ƚ{public ӥ(){ɞ=2;ɡ="CmdAmount";}Ϗ Ņ;public override void ɼ(){Ņ=new Ϗ(ƣ,e.Ŏ);}bool Ӱ;bool ӯ=false;int Ŗ=0;int Ř=0;int
ŕ=0;public override bool Ɩ(bool ó){if(!ó){Ӱ=!ƙ.Ή.EndsWith("x");ӯ=ƙ.Ή.EndsWith("bar");if(ӯ)Ӱ=true;if(ƙ.ͷ.Count==0)ƙ.ͷ.Add(
new ʰ("reactor,gatlingturret,missileturret,interiorturret,gatlinggun,launcherreload,launcher,oxygenerator"));Ř=0;}for(;Ř<ƙ.
ͷ.Count;Ř++){ʰ Ő=ƙ.ͷ[Ř];if(!ó){Ő.ʬ();Ŗ=0;ŕ=0;}for(;ŕ<Ő.ʮ.Count;ŕ++){if(Ŗ==0){if(!ó){if(Ő.ʮ[ŕ]=="")continue;Ņ.Ų();}string
œ=Ő.ʮ[ŕ];if(!Ņ.Б(œ,ƙ.Έ,ó))return false;Ŗ++;ó=false;}if(!Ө(ó))return false;ó=false;Ŗ=0;}}return true;}int Ӯ=0;int į=0;
double Ƅ=0;double ӭ=0;double Ӭ=0;int Ӣ=0;IMyTerminalBlock ӫ;IMyInventory Ӫ;List<MyInventoryItem>Z=new List<MyInventoryItem>(50
);string ө="";bool Ө(bool ó){if(!ó){Ӯ=0;į=0;}for(;Ӯ<Ņ.Ц();Ӯ++){if(į==0){if(!ƣ.ɔ(50))return false;ӫ=Ņ.ϩ[Ӯ];Ӫ=ӫ.
GetInventory(0);if(Ӫ==null)continue;į++;ó=false;}if(!ó){Z.Clear();Ӫ.GetItems(Z);ө=(Z.Count>0?Z[0].Type.ToString():"");Ӣ=0;Ƅ=0;ӭ=0;Ӭ=
0;}for(;Ӣ<Z.Count;Ӣ++){if(!ƣ.ɔ(30))return false;MyInventoryItem z=Z[Ӣ];if(z.Type.ToString()!=ө)Ӭ+=(double)z.Amount;else Ƅ
+=(double)z.Amount;}string ӧ=Đ.Ȃ("A1");string ƛ=ӫ.CustomName;if(Ƅ>0&&(double)Ӫ.CurrentVolume>0){double Ӧ=Ӭ*(double)Ӫ.
CurrentVolume/(Ƅ+Ӭ);ӭ=Math.Floor(Ƅ*((double)Ӫ.MaxVolume-Ӧ)/((double)Ӫ.CurrentVolume-Ӧ));ӧ=e.Ȗ(Ƅ)+" / "+(Ӭ>0?"~":"")+e.Ȗ(ӭ);}if(!ӯ||ӭ
<=0){ƛ=e.Ǭ(ƛ,e.ɫ*0.8f);e.Ô(ƛ);e.Ƹ(ӧ);}if(Ӱ&&ӭ>0){double ʧ=100*Ƅ/ӭ;e.Ǣ(ʧ);}į=0;ó=false;}return true;}}class ӓ:ƚ{Ϗ Ņ;public
ӓ(){ɞ=2;ɡ="CmdMass";}public override void ɼ(){Ņ=new Ϗ(ƣ,e.Ŏ);}bool Ε=false;bool λ=false;int Ĝ=0;public override bool Ɩ(
bool ó){if(!ó){Ņ.Ų();Ε=(ƙ.Ή[ƙ.Ή.Length-1]=='x');λ=(ƙ.Ή[ƙ.Ή.Length-1]=='p');Ĝ=0;}if(Ĝ==0){if(!Ņ.А(ƙ.Έ,ó))return false;Ĝ++;ó=
false;}double Ï=Ņ.Ϡ(ó);if(Double.IsNaN(Ï))return false;double ʨ=0;int ѯ=ƙ.ͷ.Count;if(ѯ>0){double.TryParse(ƙ.ͷ[0].Ő.Trim(),out
ʨ);if(ѯ>1){string ѭ=ƙ.ͷ[1].Ő.Trim();char ѩ=' ';if(ѭ.Length>0)ѩ=Char.ToLower(ѭ[0]);int Ѭ="kmgtpezy".IndexOf(ѩ);if(Ѭ>=0)ʨ*=
Math.Pow(1000.0,Ѭ);}ʨ*=1000.0;}e.Ô(Đ.Ȃ("M1")+" ");if(ʨ<=0){e.Ƹ(e.ȕ(Ï));return true;}double ʧ=Ï/ʨ*100;if(!Ε&&!λ){e.Ƹ(e.ȕ(Ï)+
" / "+e.ȕ(ʨ));e.ƿ(ʧ,1.0f,e.Ǔ);e.ǘ(' '+e.ȍ(ʧ)+"%");}else if(λ){e.Ƹ(e.ȍ(ʧ)+"%");e.Ǣ(ʧ);}else e.Ƹ(e.ȍ(ʧ)+"%");return true;}}
class Ӓ:ƚ{Ƀ Ǌ;Ϗ Ņ;public Ӓ(){ɞ=3;ɡ="CmdOxygen";}public override void ɼ(){Ǌ=e.Ǌ;Ņ=new Ϗ(ƣ,e.Ŏ);}int Ĝ=0;int ĉ=0;bool ƈ=false;
int ӑ=0;double Ƚ=0;double Ʌ=0;double ƾ;public override bool Ɩ(bool ó){if(!ó){Ņ.Ų();Ĝ=0;ĉ=0;ƾ=0;}if(Ĝ==0){if(!Ņ.Б("airvent",
ƙ.Έ,ó))return false;ƈ=(Ņ.Ц()>0);Ĝ++;ó=false;}if(Ĝ==1){for(;ĉ<Ņ.Ц();ĉ++){if(!ƣ.ɔ(8))return false;var Ō=Ņ.ϩ[ĉ]as IMyAirVent
;ƾ=Math.Max(Ō.GetOxygenLevel()*100,0f);e.Ô(Ō.CustomName);if(Ō.CanPressurize)e.Ƹ(e.ȍ(ƾ)+"%");else e.Ƹ(Đ.Ȃ("O1"));e.Ǣ(ƾ);}Ĝ
++;ó=false;}if(Ĝ==2){if(!ó)Ņ.Ų();if(!Ņ.Б("oxyfarm",ƙ.Έ,ó))return false;ӑ=Ņ.Ц();Ĝ++;ó=false;}if(Ĝ==3){if(ӑ>0){if(!ó)ĉ=0;
double ӟ=0;for(;ĉ<ӑ;ĉ++){if(!ƣ.ɔ(4))return false;var Ӟ=Ņ.ϩ[ĉ]as IMyOxygenFarm;ӟ+=Ӟ.GetOutput()*100;}ƾ=ӟ/ӑ;if(ƈ)e.ǘ("");ƈ|=(ӑ>0
);e.Ô(Đ.Ȃ("O2"));e.Ƹ(e.ȍ(ƾ)+"%");e.Ǣ(ƾ);}Ĝ++;ó=false;}if(Ĝ==4){if(!ó)Ņ.Ų();if(!Ņ.Б("oxytank",ƙ.Έ,ó))return false;ӑ=Ņ.Ц();
if(ӑ==0){if(!ƈ)e.ǘ(Đ.Ȃ("O3"));return true;}Ĝ++;ó=false;}if(Ĝ==5){if(!ó){Ƚ=0;Ʌ=0;ĉ=0;}if(!Ǌ.ɇ(Ņ.ϩ,"oxygen",ref Ʌ,ref Ƚ,ó))
return false;if(Ƚ==0){if(!ƈ)e.ǘ(Đ.Ȃ("O3"));return true;}ƾ=Ʌ/Ƚ*100;if(ƈ)e.ǘ("");e.Ô(Đ.Ȃ("O4"));e.Ƹ(e.ȍ(ƾ)+"%");e.Ǣ(ƾ);Ĝ++;}
return true;}}class ӝ:ƚ{public ӝ(){ɞ=1;ɡ="CmdPosition";}public override bool Ɩ(bool ó){bool Ӝ=(ƙ.Ή=="posxyz");bool Ҿ=(ƙ.Ή==
"posgps");IMyTerminalBlock ã=a.C.ã;if(ƙ.Έ!=""&&ƙ.Έ!="*"){ã=e.ǈ.GetBlockWithName(ƙ.Έ);if(ã==null){e.ǘ("Pos: "+Đ.Ȃ("P1")+": "+ƙ.Έ)
;return true;}}if(Ҿ){Vector3D ł=ã.GetPosition();e.ǘ("GPS:"+Đ.Ȃ("P2")+":"+ł.GetDim(0).ToString("F2")+":"+ł.GetDim(1).
ToString("F2")+":"+ł.GetDim(2).ToString("F2")+":");return true;}e.Ô(Đ.Ȃ("P2")+": ");if(!Ӝ){e.Ƹ(ã.GetPosition().ToString("F0"));
return true;}e.ǘ("");e.Ô(" X: ");e.Ƹ(ã.GetPosition().GetDim(0).ToString("F0"));e.Ô(" Y: ");e.Ƹ(ã.GetPosition().GetDim(1).
ToString("F0"));e.Ô(" Z: ");e.Ƹ(ã.GetPosition().GetDim(2).ToString("F0"));return true;}}class ӛ:ƚ{public ӛ(){ɞ=3;ɡ="CmdPower";}Ƀ
Ǌ;Ϗ Ӛ;Ϗ ә;Ϗ Ә;Ϗ ѧ;Ϗ ӗ;Ϗ Ņ;public override void ɼ(){Ӛ=new Ϗ(ƣ,e.Ŏ);ә=new Ϗ(ƣ,e.Ŏ);Ә=new Ϗ(ƣ,e.Ŏ);ѧ=new Ϗ(ƣ,e.Ŏ);ӗ=new Ϗ(ƣ,
e.Ŏ);Ņ=new Ϗ(ƣ,e.Ŏ);Ǌ=e.Ǌ;}string м;bool Ӗ;string Ѧ;string θ;int ӕ;int Ĝ=0;bool Ӕ=true;public override bool Ɩ(bool ó){if(
!ó){м=(ƙ.Ή.EndsWith("x")?"s":(ƙ.Ή.EndsWith("p")?"p":(ƙ.Ή.EndsWith("v")?"v":(ƙ.Ή.EndsWith("bar")?"b":"n"))));Ӗ=(ƙ.Ή.
StartsWith("powersummary"));Ѧ="a";θ="";if(ƙ.Ή.Contains("stored"))Ѧ="s";else if(ƙ.Ή.Contains("in"))Ѧ="i";else if(ƙ.Ή.Contains("out"
))Ѧ="o";Ĝ=0;Ӛ.Ų();ә.Ų();Ә.Ų();ѧ.Ų();ӗ.Ų();if(Ӕ)return false;}else{if(Ӕ){Ӕ=false;ó=false;}}if(Ѧ=="a"){if(Ĝ==0){if(!Ӛ.Б(
"reactor",ƙ.Έ,ó))return false;ó=false;Ĝ++;}if(Ĝ==1){if(!ә.Б("hydrogenengine",ƙ.Έ,ó))return false;ó=false;Ĝ++;}if(Ĝ==2){if(!Ә.Б(
"solarpanel",ƙ.Έ,ó))return false;ó=false;Ĝ++;}if(Ĝ==3){if(!ӗ.Б("windturbine",ƙ.Έ,ó))return false;ó=false;Ĝ++;}}else if(Ĝ==0)Ĝ=4;if(Ĝ
==4){if(!ѧ.Б("battery",ƙ.Έ,ó))return false;ó=false;Ĝ++;}int Ҝ=Ӛ.Ц();int и=ә.Ц();int ѓ=Ә.Ц();int ђ=ѧ.Ц();int ё=ӗ.Ц();if(Ĝ==
5){ӕ=0;if(Ҝ>0)ӕ++;if(и>0)ӕ++;if(ѓ>0)ӕ++;if(ё>0)ӕ++;if(ђ>0)ӕ++;if(ӕ<1){e.ǘ(Đ.Ȃ("P6"));return true;}if(ƙ.ͷ.Count>0){if(ƙ.ͷ[
0].Ő.Length>0)θ=ƙ.ͷ[0].Ő;}Ĝ++;ó=false;}if(Ѧ!="a"){if(!Ѩ(ѧ,(θ==""&&м!="b"?Đ.Ȃ("P7"):θ),Ѧ,м,ó))return false;return true;}
string ы=Đ.Ȃ("P8");if(!Ӗ){if(Ĝ==6){if(Ҝ>0)if(!э(Ӛ,(θ==""?Đ.Ȃ("P9"):θ),м,ó))return false;Ĝ++;ó=false;}if(Ĝ==7){if(и>0)if(!э(ә,(
θ==""?Đ.Ȃ("P12"):θ),м,ó))return false;Ĝ++;ó=false;}if(Ĝ==8){if(ѓ>0)if(!э(Ә,(θ==""?Đ.Ȃ("P10"):θ),м,ó))return false;Ĝ++;ó=
false;}if(Ĝ==9){if(ё>0)if(!э(ӗ,(θ==""?Đ.Ȃ("P13"):θ),м,ó))return false;Ĝ++;ó=false;}if(Ĝ==10){if(ђ>0)if(!Ѩ(ѧ,(θ==""?Đ.Ȃ("P7"):
θ),Ѧ,м,ó))return false;Ĝ++;ó=false;}}else{ы=(θ==""?Đ.Ȃ("P11"):θ);ӕ=10;if(Ĝ==6)Ĝ=11;}if(ӕ==1)return true;if(!ó){Ņ.Ų();Ņ.Ш(
Ӛ);Ņ.Ш(ә);Ņ.Ш(Ә);Ņ.Ш(ӗ);Ņ.Ш(ѧ);}if(!э(Ņ,ы,м,ó))return false;return true;}void к(double ʫ,double Ɛ){double й=(Ɛ>0?ʫ/Ɛ*100:
0);switch(м){case"s":e.Ƹ(Ʒ.Ų().ʙ(' ').ʙ(й.ToString("F1")).ʙ("%"));break;case"v":e.Ƹ(Ʒ.Ų().ʙ(e.Ȗ(ʫ)).ʙ("W / ").ʙ(e.Ȗ(Ɛ)).ʙ
("W"));break;case"c":e.Ƹ(Ʒ.Ų().ʙ(e.Ȗ(ʫ)).ʙ("W"));break;case"p":e.Ƹ(Ʒ.Ų().ʙ(' ').ʙ(й.ToString("F1")).ʙ("%"));e.Ǣ(й);break;
case"b":e.Ǣ(й);break;default:e.Ƹ(Ʒ.Ų().ʙ(e.Ȗ(ʫ)).ʙ("W / ").ʙ(e.Ȗ(Ɛ)).ʙ("W"));e.ƿ(й,1.0f,e.Ǔ);e.Ƹ(Ʒ.Ų().ʙ(' ').ʙ(й.ToString(
"F1")).ʙ("%"));break;}}double ѐ=0;double Я=0,я=0;int ю=0;bool э(Ϗ ь,string ы,string ʥ,bool ó){if(!ó){Я=0;я=0;ю=0;}if(ю==0){
if(!Ǌ.ɏ(ь.ϩ,Ǌ.ɂ,ref ѐ,ref ѐ,ref Я,ref я,ó))return false;ю++;ó=false;}if(!ƣ.ɔ(250))return false;double й=(я>0?Я/я*100:0);if
(ы!="")e.Ô(ы+": ");к(Я*1000000,я*1000000);return true;}double ъ=0,щ=0,ш=0,ч=0;double ц=0,є=0;int ѕ=0;ʞ Ʒ=new ʞ(100);bool
Ѩ(Ϗ ѧ,string ы,string Ѧ,string ʥ,bool ó){if(!ó){ъ=щ=0;ш=ч=0;ц=є=0;ѕ=0;}if(ѕ==0){if(!Ǌ.Ʉ(ѧ.ϩ,ref ш,ref ч,ref ъ,ref щ,ref ц
,ref є,ó))return false;ш*=1000000;ч*=1000000;ъ*=1000000;щ*=1000000;ц*=1000000;є*=1000000;ѕ++;ó=false;}double ѥ=(є>0?ц/є*
100:0);double Ѥ=(щ>0?ъ/щ*100:0);double ѣ=(ч>0?ш/ч*100:0);bool Ѣ=Ѧ=="a";if(ѕ==1){if(!ƣ.ɔ(200))return false;if(Ѣ){if(ʥ!="p"){
if(ы!="")e.Ô(Ʒ.Ų().ʙ(ы).ʙ(": "));e.Ƹ(Ʒ.Ų().ʙ("(IN ").ʙ(e.Ȗ(ш)).ʙ("W / OUT ").ʙ(e.Ȗ(ъ)).ʙ("W)"));}else if(ы!="")e.ǘ(Ʒ.Ų().ʙ
(ы).ʙ(": "));e.Ô(Ʒ.Ų().ʙ("  ").ʙ(Đ.Ȃ("P3")).ʙ(": "));}else if(ы!="")e.Ô(Ʒ.Ų().ʙ(ы).ʙ(": "));if(Ѣ||Ѧ=="s")switch(ʥ){case
"s":e.Ƹ(Ʒ.Ų().ʙ(' ').ʙ(ѥ.ToString("F1")).ʙ("%"));break;case"v":e.Ƹ(Ʒ.Ų().ʙ(e.Ȗ(ц)).ʙ("Wh / ").ʙ(e.Ȗ(є)).ʙ("Wh"));break;case
"p":e.Ƹ(Ʒ.Ų().ʙ(' ').ʙ(ѥ.ToString("F1")).ʙ("%"));e.Ǣ(ѥ);break;case"b":e.Ǣ(ѥ);break;default:e.Ƹ(Ʒ.Ų().ʙ(e.Ȗ(ц)).ʙ("Wh / ").ʙ
(e.Ȗ(є)).ʙ("Wh"));e.ƿ(ѥ,1.0f,e.Ǔ);e.Ƹ(Ʒ.Ų().ʙ(' ').ʙ(ѥ.ToString("F1")).ʙ("%"));break;}if(Ѧ=="s")return true;ѕ++;ó=false;}
if(ѕ==2){if(!ƣ.ɔ(150))return false;if(Ѣ)e.Ô(Ʒ.Ų().ʙ("  ").ʙ(Đ.Ȃ("P4")).ʙ(": "));if(Ѣ||Ѧ=="o")switch(ʥ){case"s":e.Ƹ(Ʒ.Ų().ʙ
(' ').ʙ(Ѥ.ToString("F1")).ʙ("%"));break;case"v":e.Ƹ(Ʒ.Ų().ʙ(e.Ȗ(ъ)).ʙ("W / ").ʙ(e.Ȗ(щ)).ʙ("W"));break;case"p":e.Ƹ(Ʒ.Ų().ʙ
(' ').ʙ(Ѥ.ToString("F1")).ʙ("%"));e.Ǣ(Ѥ);break;case"b":e.Ǣ(Ѥ);break;default:e.Ƹ(Ʒ.Ų().ʙ(e.Ȗ(ъ)).ʙ("W / ").ʙ(e.Ȗ(щ)).ʙ("W"
));e.ƿ(Ѥ,1.0f,e.Ǔ);e.Ƹ(Ʒ.Ų().ʙ(' ').ʙ(Ѥ.ToString("F1")).ʙ("%"));break;}if(Ѧ=="o")return true;ѕ++;ó=false;}if(!ƣ.ɔ(150))
return false;if(Ѣ)e.Ô(Ʒ.Ų().ʙ("  ").ʙ(Đ.Ȃ("P5")).ʙ(": "));if(Ѣ||Ѧ=="i")switch(ʥ){case"s":e.Ƹ(Ʒ.Ų().ʙ(' ').ʙ(ѣ.ToString("F1")).
ʙ("%"));break;case"v":e.Ƹ(Ʒ.Ų().ʙ(e.Ȗ(ш)).ʙ("W / ").ʙ(e.Ȗ(ч)).ʙ("W"));break;case"p":e.Ƹ(Ʒ.Ų().ʙ(' ').ʙ(ѣ.ToString("F1")).
ʙ("%"));e.Ǣ(ѣ);break;case"b":e.Ǣ(ѣ);break;default:e.Ƹ(Ʒ.Ų().ʙ(e.Ȗ(ш)).ʙ("W / ").ʙ(e.Ȗ(ч)).ʙ("W"));e.ƿ(ѣ,1.0f,e.Ǔ);e.Ƹ(Ʒ.Ų
().ʙ(' ').ʙ(ѣ.ToString("F1")).ʙ("%"));break;}return true;}}class ѡ:ƚ{public ѡ(){ɞ=7;ɡ="CmdPowerTime";}class Ѡ{public
TimeSpan Ī=new TimeSpan(-1);public double з=-1;public double џ=0;}Ѡ ў=new Ѡ();Ϗ ѝ;Ϗ ќ;public override void ɼ(){ѝ=new Ϗ(ƣ,e.Ŏ);ќ=
new Ϗ(ƣ,e.Ŏ);}int ћ=0;double њ=0;double љ=0,ј=0;double ї=0,і=0,х=0;double ж=0,ф=0;int е=0;private bool д(string Έ,out
TimeSpan г,out double ο,bool ó){MyResourceSourceComponent ɉ;MyResourceSinkComponent Ȼ;double в=ɥ;Ѡ б=ў;г=б.Ī;ο=б.з;if(!ó){ѝ.Ų();
ќ.Ų();б.з=0;ћ=0;њ=0;љ=ј=0;ї=0;і=х=0;ж=ф=0;е=0;}if(ћ==0){if(!ѝ.Б("reactor",Έ,ó))return false;ó=false;ћ++;}if(ћ==1){for(;е<
ѝ.ϩ.Count;е++){if(!ƣ.ɔ(200))return false;var ã=ѝ.ϩ[е]as IMyReactor;if(ã==null||!ã.IsWorking)continue;if(ã.Components.
TryGet<MyResourceSourceComponent>(out ɉ)){љ+=ɉ.CurrentOutputByType(e.Ǌ.ɂ);ј+=ɉ.MaxOutputByType(e.Ǌ.ɂ);}њ+=(double)ã.
GetInventory(0).CurrentMass;}ó=false;ћ++;}if(ћ==2){if(!ќ.Б("battery",Έ,ó))return false;ó=false;ћ++;}if(ћ==3){if(!ó)е=0;for(;е<ќ.ϩ.
Count;е++){if(!ƣ.ɔ(300))return false;var ã=ќ.ϩ[е]as IMyBatteryBlock;if(ã==null||!ã.IsWorking)continue;if(ã.Components.TryGet<
MyResourceSourceComponent>(out ɉ)){і=ɉ.CurrentOutputByType(e.Ǌ.ɂ);х=ɉ.MaxOutputByType(e.Ǌ.ɂ);}if(ã.Components.TryGet<MyResourceSinkComponent>(out
Ȼ)){і-=Ȼ.CurrentInputByType(e.Ǌ.ɂ);}double а=(і<0?(ã.MaxStoredPower-ã.CurrentStoredPower)/(-і/3600):0);if(а>б.з)б.з=а;if(
ã.ChargeMode==ChargeMode.Recharge)continue;ж+=і;ф+=х;ї+=ã.CurrentStoredPower;}ó=false;ћ++;}double Я=љ+ж;if(Я<=0)б.Ī=
TimeSpan.FromSeconds(-1);else{double Ю=б.Ī.TotalSeconds;double Э;double Ь=(б.џ-њ)/в;if(љ<=0)Ь=Math.Min(Я,ј)/3600000;double Ы=0;
if(ф>0)Ы=Math.Min(Я,ф)/3600;if(Ь<=0&&Ы<=0)Э=-1;else if(Ь<=0)Э=ї/Ы;else if(Ы<=0)Э=њ/Ь;else{double Ъ=Ы;double Щ=(љ<=0?Я/3600
:Ь*Я/љ);Э=ї/Ъ+њ/Щ;}if(Ю<=0||Э<0)Ю=Э;else Ю=(Ю+Э)/2;try{б.Ī=TimeSpan.FromSeconds(Ю);}catch{б.Ī=TimeSpan.FromSeconds(-1);}}
б.џ=њ;ο=б.з;г=б.Ī;return true;}int Ĝ=0;bool δ=false;bool Ε=false;bool λ=false;double з=0;TimeSpan ȥ;int у=0,т=0,с=0;int ȁ
=0;int р=0;public override bool Ɩ(bool ó){if(!ó){δ=ƙ.Ή.EndsWith("bar");Ε=(ƙ.Ή[ƙ.Ή.Length-1]=='x');λ=(ƙ.Ή[ƙ.Ή.Length-1]==
'p');Ĝ=0;у=т=с=ȁ=0;р=0;з=0;}if(Ĝ==0){if(ƙ.ͷ.Count>0){for(;р<ƙ.ͷ.Count;р++){if(!ƣ.ɔ(100))return false;ƙ.ͷ[р].ʬ();if(ƙ.ͷ[р].ʮ
.Count<=0)continue;string Ő=ƙ.ͷ[р].ʮ[0];int.TryParse(Ő,out ȁ);if(р==0)у=ȁ;else if(р==1)т=ȁ;else if(р==2)с=ȁ;}}Ĝ++;ó=false
;}if(Ĝ==1){if(!д(ƙ.Έ,out ȥ,out з,ó))return false;Ĝ++;ó=false;}if(!ƣ.ɔ(150))return false;double Ī=0;TimeSpan п;try{п=new
TimeSpan(у,т,с);}catch{п=TimeSpan.FromSeconds(-1);}string þ;if(ȥ.TotalSeconds>0||з<=0){if(!δ)e.Ô(Đ.Ȃ("PT1")+" ");þ=e.Ǌ.Ȧ(ȥ);Ī=ȥ.
TotalSeconds;}else{if(!δ)e.Ô(Đ.Ȃ("PT2")+" ");TimeSpan о;try{о=TimeSpan.FromSeconds(з);}catch{о=new TimeSpan(-1);}þ=e.Ǌ.Ȧ(о);if(п.
TotalSeconds>=з)Ī=п.TotalSeconds-з;else Ī=0;}if(п.Ticks<=0){e.Ƹ(þ);return true;}double ʧ=Ī/п.TotalSeconds*100;if(ʧ>100)ʧ=100;if(δ){e
.Ǣ(ʧ);return true;}if(!Ε&&!λ){e.Ƹ(þ);e.ƿ(ʧ,1.0f,e.Ǔ);e.ǘ(' '+ʧ.ToString("0.0")+"%");}else if(λ){e.Ƹ(ʧ.ToString("0.0")+"%"
);e.Ǣ(ʧ);}else e.Ƹ(ʧ.ToString("0.0")+"%");return true;}}class н:ƚ{public н(){ɞ=7;ɡ="CmdPowerUsed";}Ƀ Ǌ;Ϗ Ņ;public
override void ɼ(){Ņ=new Ϗ(ƣ,e.Ŏ);Ǌ=e.Ǌ;}string м;string л;string Ͼ;void к(double ʫ,double Ɛ){double й=(Ɛ>0?ʫ/Ɛ*100:0);switch(м){
case"s":e.Ƹ(й.ToString("0.0")+"%",1.0f);break;case"v":e.Ƹ(e.Ȗ(ʫ)+"W / "+e.Ȗ(Ɛ)+"W",1.0f);break;case"c":e.Ƹ(e.Ȗ(ʫ)+"W",1.0f);
break;case"p":e.Ƹ(й.ToString("0.0")+"%",1.0f);e.Ǣ(й);break;default:e.Ƹ(e.Ȗ(ʫ)+"W / "+e.Ȗ(Ɛ)+"W");e.ƿ(й,1.0f,e.Ǔ);e.Ƹ(' '+й.
ToString("0.0")+"%");break;}}double ɍ=0,Ɍ=0;int ѿ=0;int Ĝ=0;Ҋ Ҏ=new Ҋ();public override bool Ɩ(bool ó){if(!ó){м=(ƙ.Ή.EndsWith(
"x")?"s":(ƙ.Ή.EndsWith("usedp")||ƙ.Ή.EndsWith("topp")?"p":(ƙ.Ή.EndsWith("v")?"v":(ƙ.Ή.EndsWith("c")?"c":"n"))));л=(ƙ.Ή.
Contains("top")?"top":"");Ͼ=(ƙ.ͷ.Count>0?ƙ.ͷ[0].Ő:Đ.Ȃ("PU1"));ɍ=Ɍ=0;Ĝ=0;ѿ=0;Ņ.Ų();Ҏ.k();}if(Ĝ==0){if(!Ņ.А(ƙ.Έ,ó))return false;ó=
false;Ĝ++;}MyResourceSinkComponent Ȼ;MyResourceSourceComponent ɉ;switch(л){case"top":if(Ĝ==1){for(;ѿ<Ņ.ϩ.Count;ѿ++){if(!ƣ.ɔ(
200))return false;IMyTerminalBlock ã=Ņ.ϩ[ѿ];if(ã.Components.TryGet<MyResourceSinkComponent>(out Ȼ)){ListReader<
MyDefinitionId>ȧ=Ȼ.AcceptedResources;if(ȧ.IndexOf(Ǌ.ɂ)<0)continue;ɍ=Ȼ.CurrentInputByType(Ǌ.ɂ)*1000000;}else continue;Ҏ.º(ɍ,ã);}ó=false
;Ĝ++;}if(Ҏ.w()<=0){e.ǘ("PowerUsedTop: "+Đ.Ȃ("D2"));return true;}int ĺ=10;if(ƙ.ͷ.Count>0)if(!int.TryParse(Ͼ,out ĺ)){ĺ=10;}
if(ĺ>Ҏ.w())ĺ=Ҏ.w();if(Ĝ==2){if(!ó){ѿ=Ҏ.w()-1;Ҏ.j();}for(;ѿ>=Ҏ.w()-ĺ;ѿ--){if(!ƣ.ɔ(200))return false;IMyTerminalBlock ã=Ҏ.o(
ѿ);string ƛ=e.Ǭ(ã.CustomName,e.ɫ*0.4f);if(ã.Components.TryGet<MyResourceSinkComponent>(out Ȼ)){ɍ=Ȼ.CurrentInputByType(Ǌ.ɂ
)*1000000;Ɍ=Ȼ.MaxRequiredInputByType(Ǌ.ɂ)*1000000;var ҋ=(ã as IMyRadioAntenna);if(ҋ!=null)Ɍ*=ҋ.Radius/500;}e.Ô(ƛ+" ");к(ɍ
,Ɍ);}}break;default:for(;ѿ<Ņ.ϩ.Count;ѿ++){if(!ƣ.ɔ(200))return false;double ҍ;IMyTerminalBlock ã=Ņ.ϩ[ѿ];if(ã.Components.
TryGet<MyResourceSinkComponent>(out Ȼ)){ListReader<MyDefinitionId>ȧ=Ȼ.AcceptedResources;if(ȧ.IndexOf(Ǌ.ɂ)<0)continue;ҍ=Ȼ.
CurrentInputByType(Ǌ.ɂ);double Ҍ=Ȼ.MaxRequiredInputByType(Ǌ.ɂ);var ҋ=(ã as IMyRadioAntenna);if(ҋ!=null){Ҍ*=ҋ.Radius/500;}Ɍ+=Ҍ;}else
continue;if(ã.Components.TryGet<MyResourceSourceComponent>(out ɉ)&&(ã as IMyBatteryBlock!=null)){ҍ-=ɉ.CurrentOutputByType(Ǌ.ɂ);
if(ҍ<=0)continue;}ɍ+=ҍ;}e.Ô(Ͼ);к(ɍ*1000000,Ɍ*1000000);break;}return true;}public class Ҋ{List<KeyValuePair<double,
IMyTerminalBlock>>ҁ=new List<KeyValuePair<double,IMyTerminalBlock>>();public void º(double Ҁ,IMyTerminalBlock ã){ҁ.Add(new KeyValuePair<
double,IMyTerminalBlock>(Ҁ,ã));}public int w(){return ҁ.Count;}public IMyTerminalBlock o(int n){return ҁ[n].Value;}public void
k(){ҁ.Clear();}public void j(){ҁ.Sort((Ѕ,қ)=>(Ѕ.Key.CompareTo(қ.Key)));}}}class Қ:ƚ{Ϗ Ņ;public Қ(){ɞ=1;ɡ="CmdProp";}
public override void ɼ(){Ņ=new Ϗ(ƣ,e.Ŏ);}int Ĝ=0;int ѿ=0;string ґ="b";bool ҙ=false;string Ҙ=null;string җ=null;string Җ=null;
string ҕ=null;string Ҕ=null;public override bool Ɩ(bool ó){if(!ó){ҙ=ƙ.Ή.StartsWith("props");ґ=ƙ.Ή.Contains("float")?"f":"b";Ҙ=
җ=ҕ=Ҕ=null;ѿ=0;Ĝ=0;}if(ƙ.ͷ.Count<1){e.ǘ(ƙ.Ή+": "+"Missing property name.");return true;}if(Ĝ==0){if(!ó)Ņ.Ų();if(!Ņ.А(ƙ.Έ,
ó))return false;Ғ(ґ);Ĝ++;ó=false;}if(Ĝ==1){int ĺ=Ņ.Ц();if(ĺ==0){e.ǘ(ƙ.Ή+": "+"No blocks found.");return true;}for(;ѿ<ĺ;ѿ
++){if(!ƣ.ɔ(50))return false;IMyTerminalBlock ã=Ņ.ϩ[ѿ];if(ã.GetProperty(Ҙ)!=null){if(җ==null){string Ͼ=e.Ǭ(ã.CustomName,e.
ɫ*0.7f);e.Ô(Ͼ);}else e.Ô(җ);string ғ="N/A";if(ґ=="b")ғ=ҏ(ã,Ҙ,ҕ,Ҕ);else if(ґ=="f")ғ=Ґ(ã,Ҙ);if(Җ!=null)ғ+=" "+Җ;e.Ƹ(ғ);if(!
ҙ)return true;}}}return true;}void Ғ(string ґ){Ҙ=ƙ.ͷ[0].Ő;if(ƙ.ͷ.Count>1){if(ґ=="b"){if(!ҙ)җ=ƙ.ͷ[1].Ő;else ҕ=ƙ.ͷ[1].Ő;}
else if(ґ=="f"){if(!ҙ)җ=ƙ.ͷ[1].Ő;else Җ=ƙ.ͷ[1].Ő;}if(ƙ.ͷ.Count>2){if(ґ=="b"){if(!ҙ)ҕ=ƙ.ͷ[2].Ő;else Ҕ=ƙ.ͷ[2].Ő;if(ƙ.ͷ.Count>3
&&!ҙ)Ҕ=ƙ.ͷ[3].Ő;}else if(ґ=="f"){if(!ҙ)Җ=ƙ.ͷ[2].Ő;}}}}string Ґ(IMyTerminalBlock ã,string Ѿ){return e.Ȗ(ã.GetValue<float>(Ѿ
));}string ҏ(IMyTerminalBlock ã,string Ѿ,string ѳ=null,string Ѽ=null){return(ã.GetValue<bool>(Ѿ)?(ѳ!=null?ѳ:Đ.Ȃ("W9")):(Ѽ
!=null?Ѽ:Đ.Ȃ("W1")));}}class Ѳ:ƚ{public Ѳ(){ɞ=5;ɡ="CmdShipCtrl";}Ϗ Ņ;public override void ɼ(){Ņ=new Ϗ(ƣ,e.Ŏ);}public
override bool Ɩ(bool ó){if(!ó)Ņ.Ų();if(!Ņ.Б("shipctrl",ƙ.Έ,ó))return false;if(Ņ.Ц()<=0){if(ƙ.Έ!=""&&ƙ.Έ!="*")e.ǘ(ƙ.Ή+": "+Đ.Ȃ(
"SC1")+" ("+ƙ.Έ+")");else e.ǘ(ƙ.Ή+": "+Đ.Ȃ("SC1"));return true;}if(ƙ.Ή.StartsWith("damp")){bool с=(Ņ.ϩ[0]as IMyShipController
).DampenersOverride;e.Ô(Đ.Ȃ("SCD"));e.Ƹ(с?"ON":"OFF");}else{bool с=(Ņ.ϩ[0]as IMyShipController).IsUnderControl;e.Ô(Đ.Ȃ(
"SCO"));e.Ƹ(с?"YES":"NO");}return true;}}class ѱ:ƚ{public ѱ(){ɞ=1;ɡ="CmdShipMass";}public override bool Ɩ(bool ó){bool Ѱ=ƙ.Ή.
EndsWith("base");double ʨ=0;if(ƙ.Έ!="")double.TryParse(ƙ.Έ.Trim(),out ʨ);int ѯ=ƙ.ͷ.Count;if(ѯ>0){string ѭ=ƙ.ͷ[0].Ő.Trim();char ѩ
=' ';if(ѭ.Length>0)ѩ=Char.ToLower(ѭ[0]);int Ѭ="kmgtpezy".IndexOf(ѩ);if(Ѭ>=0)ʨ*=Math.Pow(1000.0,Ѭ);}double ʉ=(Ѱ?e.ǉ.ʇ:e.ǉ.
ʈ);if(!Ѱ)e.Ô(Đ.Ȃ("SM1")+" ");else e.Ô(Đ.Ȃ("SM2")+" ");e.Ƹ(e.ȕ(ʉ,true,'k')+" ");if(ʨ>0)e.Ǣ(ʉ/ʨ*100);return true;}}class ѫ:
ƚ{public ѫ(){ɞ=0.5;ɡ="CmdSpeed";}public override bool Ɩ(bool ó){double ʨ=0;double Ѫ=1;string ѩ="m/s";if(ƙ.Ή.Contains(
"kmh")){Ѫ=3.6;ѩ="km/h";}else if(ƙ.Ή.Contains("mph")){Ѫ=2.23694;ѩ="mph";}if(ƙ.Έ!="")double.TryParse(ƙ.Έ.Trim(),out ʨ);e.Ô(Đ.Ȃ(
"S1")+" ");e.Ƹ((e.ǉ.ʑ*Ѫ).ToString("F1")+" "+ѩ+" ");if(ʨ>0)e.Ǣ(e.ǉ.ʑ/ʨ*100);return true;}}class Ѯ:ƚ{public Ѯ(){ɞ=1;ɡ=
"CmdStopTask";}public override bool Ɩ(bool ó){double ѽ;if(ƙ.Ή.Contains("best"))ѽ=e.ǉ.ʑ/e.ǉ.ʍ;else ѽ=e.ǉ.ʑ/e.ǉ.ʊ;double ѻ=e.ǉ.ʑ/2*ѽ;if
(ƙ.Ή.Contains("time")){e.Ô(Đ.Ȃ("ST"));if(double.IsNaN(ѽ)){e.Ƹ("N/A");return true;}string þ="";try{var Κ=TimeSpan.
FromSeconds(ѽ);if((int)Κ.TotalDays>0)þ=" > 24h";else{if(Κ.Hours>0)þ=Κ.Hours+"h ";if(Κ.Minutes>0||þ!="")þ+=Κ.Minutes+"m ";þ+=Κ.
Seconds+"s";}}catch{þ="N/A";}e.Ƹ(þ);return true;}e.Ô(Đ.Ȃ("SD"));if(!double.IsNaN(ѻ)&&!double.IsInfinity(ѻ))e.Ƹ(e.Ȗ(ѻ)+"m ");
else e.Ƹ("N/A");return true;}}class Ѻ:ƚ{Ƀ Ǌ;Ϗ Ņ;public Ѻ(){ɞ=2;ɡ="CmdTanks";}public override void ɼ(){Ǌ=e.Ǌ;Ņ=new Ϗ(ƣ,e.Ŏ);}
int Ĝ=0;char м='n';string ѹ;double Ѹ=0;double ѷ=0;double ƾ;bool δ=false;public override bool Ɩ(bool ó){List<ʰ>ͷ=ƙ.ͷ;if(ͷ.
Count==0){e.ǘ(Đ.Ȃ("T4"));return true;}if(!ó){м=(ƙ.Ή.EndsWith("x")?'s':(ƙ.Ή.EndsWith("p")?'p':(ƙ.Ή.EndsWith("v")?'v':'n')));δ=
ƙ.Ή.EndsWith("bar");Ĝ=0;if(ѹ==null){ѹ=ͷ[0].Ő.Trim();ѹ=char.ToUpper(ѹ[0])+ѹ.Substring(1).ToLower();}Ņ.Ų();Ѹ=0;ѷ=0;}if(Ĝ==0
){if(!Ņ.Б("oxytank",ƙ.Έ,ó))return false;ó=false;Ĝ++;}if(Ĝ==1){if(!Ņ.Б("hydrogenengine",ƙ.Έ,ó))return false;ó=false;Ĝ++;}
if(Ĝ==2){if(!Ǌ.ɇ(Ņ.ϩ,ѹ,ref Ѹ,ref ѷ,ó))return false;ó=false;Ĝ++;}if(ѷ==0){e.ǘ(String.Format(Đ.Ȃ("T5"),ѹ));return true;}ƾ=Ѹ/
ѷ*100;if(δ){e.Ǣ(ƾ);return true;}e.Ô(ѹ);switch(м){case's':e.Ƹ(' '+e.ȍ(ƾ)+"%");break;case'v':e.Ƹ(e.Ȗ(Ѹ)+"L / "+e.Ȗ(ѷ)+"L");
break;case'p':e.Ƹ(' '+e.ȍ(ƾ)+"%");e.Ǣ(ƾ);break;default:e.Ƹ(e.Ȗ(Ѹ)+"L / "+e.Ȗ(ѷ)+"L");e.ƿ(ƾ,1.0f,e.Ǔ);e.Ƹ(' '+e.ȍ(ƾ)+"%");
break;}return true;}}class Ѷ{ɹ e=null;public string E="Debug";public float ѵ=1.0f;public List<ʞ>ţ=new List<ʞ>(30);public int
Ŷ=0;public float Ѵ=0;public Ѷ(ɹ Å){e=Å;ţ.Add(new ʞ());}public void ū(string þ){ţ[Ŷ].ʙ(þ);}public void ū(ʞ Ū){ţ[Ŷ].ʙ(Ū);}
public void ũ(){ţ.Add(new ʞ());Ŷ++;Ѵ=0;}public void ũ(string Ũ){ţ[Ŷ].ʙ(Ũ);ũ();}public void ŧ(List<ʞ>Ŧ){if(ţ[Ŷ].ʜ==0)ţ.RemoveAt
(Ŷ);else Ŷ++;ţ.AddList(Ŧ);Ŷ+=Ŧ.Count-1;ũ();}public List<ʞ>ş(){if(ţ[Ŷ].ʜ==0)return ţ.GetRange(0,Ŷ);else return ţ;}public
void ť(string Ť,string J=""){string[]ţ=Ť.Split('\n');for(int D=0;D<ţ.Length;D++)ũ(J+ţ[D]);}public void Ţ(){ţ.Clear();ũ();Ŷ=0
;}public int š(){return Ŷ+(ţ[Ŷ].ʜ>0?1:0);}public string Š(){return String.Join("\n",ţ);}public void ş(List<ʞ>Ş,int Ľ,int
ŝ){int Ŝ=Ľ+ŝ;int Ŀ=š();if(Ŝ>Ŀ)Ŝ=Ŀ;for(int D=Ľ;D<Ŝ;D++)Ş.Add(ţ[D]);}}class Ŭ{ɹ e=null;public float Ż=1.0f;public int ż=17;
public int ź=0;int Ź=1;int Ÿ=1;public List<Ѷ>ŷ=new List<Ѷ>(10);public int Ŷ=0;public Ŭ(ɹ Å){e=Å;}public void ŵ(int ĺ){Ÿ=ĺ;}
public void Ŵ(){ż=(int)Math.Floor(ɹ.ɶ*Ż*Ÿ/ɹ.ɴ);}public void ų(Ѷ þ){ŷ.Add(þ);}public void Ų(){ŷ.Clear();}public int š(){int ĺ=0
;foreach(var þ in ŷ){ĺ+=þ.š();}return ĺ;}ʞ ű=new ʞ(256);public ʞ Š(){ű.Ų();int ĺ=ŷ.Count;for(int D=0;D<ĺ-1;D++){ű.ʙ(ŷ[D].
Š());ű.ʙ("\n");}if(ĺ>0)ű.ʙ(ŷ[ĺ-1].Š());return ű;}List<ʞ>Ű=new List<ʞ>(20);public ʞ ů(int Ů=0){ű.Ų();Ű.Clear();if(Ÿ<=0)
return ű;int ŭ=ŷ.Count;int Ś=0;int ń=(ż/Ÿ);int ř=(Ů*ń);int ł=ź+ř;int Ł=ł+ń;bool ŀ=false;for(int D=0;D<ŭ;D++){Ѷ þ=ŷ[D];int Ŀ=þ.
š();int ľ=Ś;Ś+=Ŀ;if(!ŀ&&Ś>ł){int Ľ=ł-ľ;if(Ś>=Ł){þ.ş(Ű,Ľ,Ł-ľ-Ľ);break;}ŀ=true;þ.ş(Ű,Ľ,Ŀ);continue;}if(ŀ){if(Ś>=Ł){þ.ş(Ű,0,
Ł-ľ);break;}þ.ş(Ű,0,Ŀ);}}int ĺ=Ű.Count;for(int D=0;D<ĺ-1;D++){ű.ʙ(Ű[D]);ű.ʙ("\n");}if(ĺ>0)ű.ʙ(Ű[ĺ-1]);return ű;}public
bool Ń(int ĺ=-1){if(ĺ<=0)ĺ=e.ɲ;if(ź-ĺ<=0){ź=0;return true;}ź-=ĺ;return false;}public bool Ļ(int ĺ=-1){if(ĺ<=0)ĺ=e.ɲ;int Ĺ=š(
);if(ź+ĺ+ż>=Ĺ){ź=Math.Max(Ĺ-ż,0);return true;}ź+=ĺ;return false;}public int ĸ=0;public void ķ(){if(ĸ>0){ĸ--;return;}if(š(
)<=ż){ź=0;Ź=1;return;}if(Ź>0){if(Ļ()){Ź=-1;ĸ=2;}}else{if(Ń()){Ź=1;ĸ=2;}}}}class Ķ:ƚ{public Ķ(){ɞ=1;ɡ="CmdTextLCD";}public
override bool Ɩ(bool ó){string þ="";if(ƙ.Έ!=""&&ƙ.Έ!="*"){var ÿ=e.ǈ.GetBlockWithName(ƙ.Έ)as IMyTextPanel;if(ÿ==null){e.ǘ(
"TextLCD: "+Đ.Ȃ("T1")+ƙ.Έ);return true;}þ=ÿ.GetText();}else{e.ǘ("TextLCD:"+Đ.Ȃ("T2"));return true;}if(þ.Length==0)return true;e.Ǘ(þ
);return true;}}class ļ:ƚ{public ļ(){ɞ=5;ɡ="CmdWorking";}Ϗ Ņ;public override void ɼ(){Ņ=new Ϗ(ƣ,e.Ŏ);}int Ĝ=0;int Ř=0;
bool ŗ;public override bool Ɩ(bool ó){if(!ó){Ĝ=0;ŗ=(ƙ.Ή=="workingx");Ř=0;}if(ƙ.ͷ.Count==0){if(Ĝ==0){if(!ó)Ņ.Ų();if(!Ņ.А(ƙ.Έ,
ó))return false;Ĝ++;ó=false;}if(!Ɵ(Ņ,ŗ,"",ó))return false;return true;}for(;Ř<ƙ.ͷ.Count;Ř++){ʰ Ő=ƙ.ͷ[Ř];if(!ó)Ő.ʬ();if(!ő
(Ő,ó))return false;ó=false;}return true;}int Ŗ=0;int ŕ=0;string[]Ŕ;string œ;string Œ;bool ő(ʰ Ő,bool ó){if(!ó){Ŗ=0;ŕ=0;}
for(;ŕ<Ő.ʮ.Count;ŕ++){if(Ŗ==0){if(!ó){if(string.IsNullOrEmpty(Ő.ʮ[ŕ]))continue;Ņ.Ų();Ŕ=Ő.ʮ[ŕ].Split(':');œ=Ŕ[0];Œ=(Ŕ.Length
>1?Ŕ[1]:"");}if(!string.IsNullOrEmpty(œ)){if(!Ņ.Б(œ,ƙ.Έ,ó))return false;}else{if(!Ņ.А(ƙ.Έ,ó))return false;}Ŗ++;ó=false;}
if(!Ɵ(Ņ,ŗ,Œ,ó))return false;Ŗ=0;ó=false;}return true;}string ŏ(IMyTerminalBlock ã){Х Ŏ=e.Ŏ;if(!ã.IsWorking)return Đ.Ȃ("W1"
);var ō=ã as IMyProductionBlock;if(ō!=null)if(ō.IsProducing)return Đ.Ȃ("W2");else return Đ.Ȃ("W3");var Ō=ã as IMyAirVent;
if(Ō!=null){if(Ō.CanPressurize)return(Ō.GetOxygenLevel()*100).ToString("F1")+"%";else return Đ.Ȃ("W4");}var ŋ=ã as
IMyGasTank;if(ŋ!=null)return(ŋ.FilledRatio*100).ToString("F1")+"%";var Ŋ=ã as IMyBatteryBlock;if(Ŋ!=null)return Ŏ.Ͽ(Ŋ);var ŉ=ã as
IMyJumpDrive;if(ŉ!=null)return Ŏ.Ϻ(ŉ).ToString("0.0")+"%";var ň=ã as IMyLandingGear;if(ň!=null){switch((int)ň.LockMode){case 0:
return Đ.Ȃ("W8");case 1:return Đ.Ȃ("W10");case 2:return Đ.Ȃ("W7");}}var Ň=ã as IMyDoor;if(Ň!=null){if(Ň.Status==DoorStatus.
Open)return Đ.Ȃ("W5");return Đ.Ȃ("W6");}var ņ=ã as IMyShipConnector;if(ņ!=null){if(ņ.Status==MyShipConnectorStatus.
Unconnected)return Đ.Ȃ("W8");if(ņ.Status==MyShipConnectorStatus.Connected)return Đ.Ȃ("W7");else return Đ.Ȃ("W10");}var ś=ã as
IMyLaserAntenna;if(ś!=null)return Ŏ.ϼ(ś);var Ž=ã as IMyRadioAntenna;if(Ž!=null)return e.Ȗ(Ž.Radius)+"m";var Ɣ=ã as IMyBeacon;if(Ɣ!=null
)return e.Ȗ(Ɣ.Radius)+"m";var Ƣ=ã as IMyThrust;if(Ƣ!=null&&Ƣ.ThrustOverride>0)return e.Ȗ(Ƣ.ThrustOverride)+"N";return Đ.Ȃ
("W9");}int Ơ=0;bool Ɵ(Ϗ Ă,bool ƞ,string Ɲ,bool ó){if(!ó)Ơ=0;for(;Ơ<Ă.Ц();Ơ++){if(!ƣ.ɔ(20))return false;IMyTerminalBlock
ã=Ă.ϩ[Ơ];string Ɯ=(ƞ?(ã.IsWorking?Đ.Ȃ("W9"):Đ.Ȃ("W1")):ŏ(ã));if(!string.IsNullOrEmpty(Ɲ)&&String.Compare(Ɯ,Ɲ,true)!=0)
continue;if(ƞ)Ɯ=ŏ(ã);string ƛ=ã.CustomName;ƛ=e.Ǭ(ƛ,e.ɫ*0.7f);e.Ô(ƛ);e.Ƹ(Ɯ);}return true;}}class ƚ:ɢ{public Ѷ þ=null;protected Ί
ƙ;protected ɹ e;protected ę a;protected ǲ Đ;public ƚ(){ɞ=3600;ɡ="CommandTask";}public void Ƙ(ę Æ,Ί Ɨ){a=Æ;e=a.e;ƙ=Ɨ;Đ=e.Đ
;}public virtual bool Ɩ(bool ó){e.ǘ(Đ.Ȃ("UC")+": '"+ƙ.Ά+"'");return true;}public override bool ɻ(bool ó){þ=e.Ǜ(þ,a.C);if(
!ó)e.Ţ();return Ɩ(ó);}}class ƕ{Dictionary<string,string>ơ=new Dictionary<string,string>(StringComparer.
InvariantCultureIgnoreCase){{"ingot","ingot"},{"ore","ore"},{"component","component"},{"tool","physicalgunobject"},{"ammo","ammomagazine"},{
"oxygen","oxygencontainerobject"},{"gas","gascontainerobject"}};Ȥ ƣ;ɹ e;Ɗ Ʈ;Ɗ ƭ;Ɗ Ƭ;ĵ ƫ;bool ƪ;public Ɗ Ʃ;public ƕ(Ȥ ƨ,ɹ Å,int F
=20){Ʈ=new Ɗ();ƭ=new Ɗ();Ƭ=new Ɗ();ƪ=false;Ʃ=new Ɗ();ƣ=ƨ;e=Å;ƫ=e.ƫ;}public void Ų(){Ƭ.k();ƭ.k();Ʈ.k();ƪ=false;Ʃ.k();}
public void Ƨ(string Ʀ,bool Ə=false,int Ƒ=1,int Ɛ=-1){if(string.IsNullOrEmpty(Ʀ)){ƪ=true;return;}string[]ƥ=Ʀ.Split(' ');string
Ë="";var ſ=new ƒ(Ə,Ƒ,Ɛ);if(ƥ.Length==2){if(!ơ.TryGetValue(ƥ[1],out Ë))Ë=ƥ[1];}string Ì=ƥ[0];if(ơ.TryGetValue(Ì,out ſ.Ë)){
ƭ.º(ſ.Ë,ſ);return;}e.ȗ(ref Ì,ref Ë);if(string.IsNullOrEmpty(Ë)){ſ.Ì=Ì;Ʈ.º(ſ.Ì,ſ);return;}ſ.Ì=Ì;ſ.Ë=Ë;Ƭ.º(Ì+' '+Ë,ſ);}
public ƒ Ƥ(string Î,string Ì,string Ë){ƒ ſ;ſ=Ƭ.v(Î);if(ſ!=null)return ſ;ſ=Ʈ.v(Ì);if(ſ!=null)return ſ;ſ=ƭ.v(Ë);if(ſ!=null)
return ſ;return null;}public bool Ɠ(string Î,string Ì,string Ë){ƒ ſ;bool ƈ=false;ſ=ƭ.v(Ë);if(ſ!=null){if(ſ.Ə)return true;ƈ=
true;}ſ=Ʈ.v(Ì);if(ſ!=null){if(ſ.Ə)return true;ƈ=true;}ſ=Ƭ.v(Î);if(ſ!=null){if(ſ.Ə)return true;ƈ=true;}return!(ƪ||ƈ);}public
ƒ Ƈ(string Î,string Ì,string Ë){var Ƅ=new ƒ();ƒ ſ=Ƥ(Î,Ì,Ë);if(ſ!=null){Ƅ.Ƒ=ſ.Ƒ;Ƅ.Ɛ=ſ.Ɛ;}Ƅ.Ì=Ì;Ƅ.Ë=Ë;Ʃ.º(Î,Ƅ);return Ƅ;}
public ƒ Ɔ(string Î,string Ì,string Ë){ƒ Ƅ=Ʃ.v(Î);if(Ƅ==null)Ƅ=Ƈ(Î,Ì,Ë);return Ƅ;}int ƃ=0;List<ƒ>Ƃ;public List<ƒ>Ɓ(string Ë,
bool ó,Func<ƒ,bool>ƀ=null){if(!ó){Ƃ=new List<ƒ>(5);ƃ=0;}for(;ƃ<Ʃ.w();ƃ++){if(!ƣ.ɔ(5))return null;ƒ ſ=Ʃ.o(ƃ);if(Ɠ(ſ.Ì+' '+ſ.Ë
,ſ.Ì,ſ.Ë))continue;if((string.Compare(ſ.Ë,Ë,true)==0)&&(ƀ==null||ƀ(ſ)))Ƃ.Add(ſ);}return Ƃ;}int ž=0;public bool ƅ(bool ó){
if(!ó){ž=0;}for(;ž<ƫ.À.Count;ž++){if(!ƣ.ɔ(10))return false;Ê z=ƫ.Z[ƫ.À[ž]];if(!z.Ï)continue;string Î=z.É+' '+z.È;if(Ɠ(Î,z.
É,z.È))continue;ƒ Ƅ=Ɔ(Î,z.É,z.È);if(Ƅ.Ɛ==-1)Ƅ.Ɛ=z.Ç;}return true;}}class ƒ{public int Ƒ;public int Ɛ;public string Ì="";
public string Ë="";public bool Ə;public double Ǝ;public ƒ(bool ƍ=false,int ƌ=1,int Ƌ=-1){Ƒ=ƌ;Ə=ƍ;Ɛ=Ƌ;}}class Ɗ{Dictionary<
string,ƒ>Ɖ=new Dictionary<string,ƒ>(StringComparer.InvariantCultureIgnoreCase);List<string>À=new List<string>();public void º(
string r,ƒ z){if(!Ɖ.ContainsKey(r)){À.Add(r);Ɖ.Add(r,z);}}public int w(){return Ɖ.Count;}public ƒ v(string r){if(Ɖ.ContainsKey
(r))return Ɖ[r];return null;}public ƒ o(int n){return Ɖ[À[n]];}public void k(){À.Clear();Ɖ.Clear();}public void j(){À.
Sort();}}class ĵ{public Dictionary<string,Ê>Z=new Dictionary<string,Ê>(StringComparer.InvariantCultureIgnoreCase);Dictionary
<string,Ê>Ö=new Dictionary<string,Ê>(StringComparer.InvariantCultureIgnoreCase);public List<string>À=new List<string>(50)
;public Dictionary<string,Ê>Õ=new Dictionary<string,Ê>(StringComparer.InvariantCultureIgnoreCase);public void Ô(string Ì,
string Ë,int Ó,string Ò,string Ñ,string Ð,bool Ï){if(Ë=="Ammo")Ë="AmmoMagazine";else if(Ë=="Tool")Ë="PhysicalGunObject";string
Î=Ì+' '+Ë;var z=new Ê(Ì,Ë,Ó,Ò,Ñ,Ï);Z.Add(Î,z);if(!Ö.ContainsKey(Ì))Ö.Add(Ì,z);if(Ñ.Length>0)Õ.Add(Ñ,z);if(Ð.Length>0)Õ.
Add(Ð,z);À.Add(Î);}public Ê Í(string Ì="",string Ë=""){if(Z.ContainsKey(Ì+" "+Ë))return Z[Ì+" "+Ë];if(string.IsNullOrEmpty(
Ë)){Ê z=null;Ö.TryGetValue(Ì,out z);return z;}if(string.IsNullOrEmpty(Ì))for(int D=0;D<Z.Count;D++){Ê z=Z[À[D]];if(string
.Compare(Ë,z.È,true)==0)return z;}return null;}}class Ê{public string É;public string È;public int Ç;public string Ø;
public string Ù;public bool Ï;public Ê(string ì,string ë,int ê=0,string é="",string è="",bool ç=true){É=ì;È=ë;Ç=ê;Ø=é;Ù=è;Ï=ç;
}}class æ{ɹ e=null;public Ã å=new Ã();public Ŭ ä;public IMyTerminalBlock ã;public IMyTextSurface â;public int á=0;public
int à=0;public string ß="";public string Þ="";public bool Ý=true;public IMyTextSurface Ü=>(Ú?â:ã as IMyTextSurface);public
int Û=>(Ú?(e.ǜ(ã)?0:1):å.w());public bool Ú=false;public æ(ɹ Å,string Y){e=Å;Þ=Y;}public æ(ɹ Å,string Y,IMyTerminalBlock Ä,
IMyTextSurface A,int W){e=Å;Þ=Y;ã=Ä;â=A;á=W;Ú=true;}public bool V(){return ä.š()>ä.ż||ä.ź!=0;}float U=1.0f;bool S=false;public float R
(){if(S)return U;S=true;return U;}float Q=1.0f;bool P=false;public float O(){if(P)return Q;P=true;return Q;}bool N=false;
public void X(){if(N)return;if(!Ú){å.j();ã=å.o(0);}int L=ã.CustomName.IndexOf("!MARGIN:");if(L<0||L+8>=ã.CustomName.Length){à=
1;ß=" ";}else{string J=ã.CustomName.Substring(L+8);int I=J.IndexOf(" ");if(I>=0)J=J.Substring(0,I);if(!int.TryParse(J,out
à))à=1;ß=new String(' ',à);}if(ã.CustomName.Contains("!NOSCROLL"))Ý=false;else Ý=true;N=true;}public void H(Ŭ G=null){if(
ä==null||ã==null)return;if(G==null)G=ä;if(!Ú){var A=ã as IMyTextSurface;if(A!=null){float F=A.FontSize;string E=A.Font;
for(int D=0;D<å.w();D++){var C=å.o(D)as IMyTextSurface;if(C==null)continue;C.Alignment=VRage.Game.GUI.TextPanel.
TextAlignment.LEFT;C.FontSize=F;C.Font=E;string B=G.ů(D).ɤ();if(!e.ǋ.SKIP_CONTENT_TYPE)C.ContentType=VRage.Game.GUI.TextPanel.
ContentType.TEXT_AND_IMAGE;C.WriteText(B);}}}else{â.Alignment=VRage.Game.GUI.TextPanel.TextAlignment.LEFT;if(!e.ǋ.SKIP_CONTENT_TYPE
)â.ContentType=VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;â.WriteText(G.ů().ɤ());}N=false;}public void K(){if(ã
==null)return;if(Ú){â.WriteText("");return;}var A=ã as IMyTextSurface;if(A==null)return;for(int D=0;D<å.w();D++){var C=å.o
(D)as IMyTextSurface;if(C==null)continue;C.WriteText("");}}}class Ã{Dictionary<string,IMyTerminalBlock>Â=new Dictionary<
string,IMyTerminalBlock>();Dictionary<IMyTerminalBlock,string>Á=new Dictionary<IMyTerminalBlock,string>();List<string>À=new
List<string>(10);public void º(string r,IMyTerminalBlock z){if(!À.Contains(r)){À.Add(r);Â.Add(r,z);Á.Add(z,r);}}public void
µ(string r){if(À.Contains(r)){À.Remove(r);Á.Remove(Â[r]);Â.Remove(r);}}public void ª(IMyTerminalBlock z){if(Á.ContainsKey
(z)){À.Remove(Á[z]);Â.Remove(Á[z]);Á.Remove(z);}}public int w(){return Â.Count;}public IMyTerminalBlock v(string r){if(À.
Contains(r))return Â[r];return null;}public IMyTerminalBlock o(int n){return Â[À[n]];}public void k(){À.Clear();Â.Clear();Á.
Clear();}public void j(){À.Sort();}}class f:ɢ{public ɹ e;public æ b;ę a;public f(ę Æ){a=Æ;e=a.e;b=a.C;ɞ=0.5;ɡ="PanelDisplay";
ɘ=true;}double í=0;public void Č(){í=0;}int ġ=0;int ğ=0;bool Ğ=true;double ĝ=double.MaxValue;int Ĝ=0;public override bool
ɻ(bool ó){ƚ ě;if(!ó&&(a.ē==false||a.ĕ==null||a.ĕ.Count<=0))return true;if(a.Ĕ.Ģ>1)return ɖ(0);if(!ó){ğ=0;Ğ=false;ĝ=double
.MaxValue;Ĝ=0;}if(Ĝ==0){while(ğ<a.ĕ.Count){if(!ƣ.ɔ(5))return false;if(a.Ė.TryGetValue(a.ĕ[ğ],out ě)){if(!ě.ɛ)return ɖ(ě.ɠ
-ƣ.ȡ+0.001);if(ě.ɟ>í)Ğ=true;if(ě.ɠ<ĝ)ĝ=ě.ɠ;}ğ++;}Ĝ++;ó=false;}double Ě=ĝ-ƣ.ȡ+0.001;if(!Ğ&&!b.V())return ɖ(Ě);e.Ǚ(b.ä,b);
if(Ğ){if(!ó){í=ƣ.ȡ;b.ä.Ų();ġ=0;}while(ġ<a.ĕ.Count){if(!ƣ.ɔ(7))return false;if(!a.Ė.TryGetValue(a.ĕ[ġ],out ě)){b.ä.ŷ.Add(e.
Ǜ(null,b));e.Ţ();e.ǘ("ERR: No cmd task ("+a.ĕ[ġ]+")");ġ++;continue;}b.ä.ų(ě.þ);ġ++;}}e.ȉ(b);a.Ĕ.Ģ++;if(ɞ<Ě&&!b.V())return
ɖ(Ě);return true;}}class ę:ɢ{public ɹ e;public æ C;public f Ę=null;string ė="N/A";public Dictionary<string,ƚ>Ė=new
Dictionary<string,ƚ>();public List<string>ĕ=null;public Ĥ Ĕ;public bool ē{get{return Ĕ.ø;}}public ę(Ĥ Ē,æ đ){ɞ=5;C=đ;Ĕ=Ē;e=Ē.e;ɡ=
"PanelProcess";}ǲ Đ;public override void ɼ(){Đ=e.Đ;}Random ď=new Random();Ί Ď=null;ƚ č(string Ġ,bool ó){if(!ó)Ď=new Ί(ƣ);if(!Ď.ʬ(Ġ,ó))
return null;ƚ Ī=Ď.ͺ();Ī.Ƙ(this,Ď);ƣ.ș(Ī,0.1+ď.Next(1,20)/10.0);return Ī;}string Ĵ="";void ĳ(){try{Ĵ=C.ã.Ǹ(C.á,e.ɳ);}catch{Ĵ=""
;return;}Ĵ=Ĵ?.Replace("\\\n","");}int ġ=0;int Ĳ=0;List<string>ı=null;HashSet<string>İ=new HashSet<string>();int į=0;bool
Į(bool ó){if(!ó){char[]ĭ={';','\n'};string Ĭ=Ĵ.Replace("\\;","\f");if(Ĭ.StartsWith("@")){int ī=Ĭ.IndexOf("\n");if(ī<0){Ĭ=
"";}else{Ĭ=Ĭ.Substring(ī+1);}}ı=new List<string>(Ĭ.Split(ĭ,StringSplitOptions.RemoveEmptyEntries));İ.Clear();ġ=0;Ĳ=0;į=0;ƣ
.ȫ(50);}while(ġ<ı.Count){if(!ƣ.ɔ(100))return false;if(ı[ġ].StartsWith("//")){ı.RemoveAt(ġ);continue;}ı[ġ]=ı[ġ].Replace(
'\f',';');if(!Ė.ContainsKey(ı[ġ])){if(į!=1)ó=false;į=1;ƚ ě=č(ı[ġ],ó);if(ě==null)return false;ó=false;Ė.Add(ı[ġ],ě);į=0;}if(!
İ.Contains(ı[ġ]))İ.Add(ı[ġ]);ġ++;}if(ĕ!=null){ƚ Ī;while(Ĳ<ĕ.Count){if(!ƣ.ɔ(100))return false;if(!İ.Contains(ĕ[Ĳ]))if(Ė.
TryGetValue(ĕ[Ĳ],out Ī)){Ī.ɽ();Ė.Remove(ĕ[Ĳ]);}Ĳ++;}}ĕ=ı;return true;}public override void ɺ(){if(ĕ!=null){ƚ Ī;for(int ĩ=0;ĩ<ĕ.
Count;ĩ++){if(Ė.TryGetValue(ĕ[ĩ],out Ī))Ī.ɽ();}ĕ=null;}if(Ę!=null){Ę.ɽ();Ę=null;}else{}}Ŭ Ĩ=null;string ħ="";bool Ħ=false;int
Ĝ=0;public override bool ɻ(bool ó){if(C.Û<=0){ɽ();return true;}if(!ó)Ĝ=0;if(Ĝ==0){if(!ó){C.ä=e.Ǚ(C.ä,C);Ĩ=e.Ǚ(Ĩ,C);return
false;}Ĝ++;ó=false;}if(Ĝ==1){if(!ó){ĳ();if(Ĵ==null){if(C.Ú){Ĕ.ñ(C.â,C.ã as IMyTextPanel);}else{ɽ();}return true;}if(C.ã.
CustomName!=ħ){Ħ=true;}else{Ħ=false;}ħ=C.ã.CustomName;return false;}Ĝ++;ó=false;}if(Ĝ==2){if(Ĵ!=ė){if(!Į(ó))return false;if(Ĵ=="")
{ė="";if(Ĕ.ø){if(Ĩ.ŷ.Count<=0)Ĩ.ŷ.Add(e.Ǜ(null,C));else e.Ǜ(Ĩ.ŷ[0],C);e.Ţ();e.ǘ(Đ.Ȃ("H1"));bool ĥ=C.Ý;C.Ý=false;e.ȉ(C,Ĩ);
C.Ý=ĥ;return true;}return this.ɖ(2);}Ħ=true;}ė=Ĵ;Ĝ++;ó=false;}if(Ę!=null&&Ħ){ƣ.ȶ(Ę);Ę.Č();ƣ.ș(Ę,0);}else if(Ę==null){Ę=
new f(this);ƣ.ș(Ę,0);}return true;}}class Ĥ:ɢ{const string ģ="T:!LCD!";public int Ģ=0;public ɹ e;public ǔ å=new ǔ();Ϗ Ċ;Ϗ û
;Dictionary<æ,ę>ú=new Dictionary<æ,ę>();public Dictionary<IMyTextSurface,æ>ù=new Dictionary<IMyTextSurface,æ>();public
bool ø=false;Ў ö=null;public Ĥ(ɹ Å){ɞ=5;e=Å;ɡ="ProcessPanels";ə=true;}public override void ɼ(){Ċ=new Ϗ(ƣ,e.Ŏ);û=new Ϗ(ƣ,e.Ŏ)
;ö=new Ў(e,this);}int õ=0;bool ô(bool ó){if(!ó)õ=0;if(õ==0){if(!Ċ.А(e.ɳ,ó))return false;õ++;ó=false;}if(õ==1){if(e.ɳ==
"T:[LCD]"&&ģ!="")if(!Ċ.А(ģ,ó))return false;õ++;ó=false;}return true;}string ü(IMyTerminalBlock ã){int ò=ã.CustomName.IndexOf(
"!LINK:");if(ò>=0&&ã.CustomName.Length>ò+6){return ã.CustomName.Substring(ò+6)+' '+ã.Position.ToString();}return ã.EntityId.
ToString();}public void ñ(IMyTextSurface A,IMyTextPanel C){æ b;if(A==null)return;if(!ù.TryGetValue(A,out b))return;if(C!=null){b
.å.ª(C);}ù.Remove(A);if(b.Û<=0||b.Ú){ę ð;if(ú.TryGetValue(b,out ð)){å.ª(b.Þ);ú.Remove(b);ð.ɽ();}}}void ï(IMyTerminalBlock
ã){var î=ã as IMyTextSurfaceProvider;var A=ã as IMyTextSurface;if(A!=null){ñ(A,ã as IMyTextPanel);return;}if(î==null)
return;for(int D=0;D<î.SurfaceCount;D++){A=î.GetSurface(D);ñ(A,null);}}string Y;string ý;bool ċ;int ĉ=0;int Ĉ=0;public
override bool ɻ(bool ó){if(!ó){Ċ.Ų();ĉ=0;Ĉ=0;}if(!ô(ó))return false;while(ĉ<Ċ.Ц()){if(!ƣ.ɔ(100))return false;var ã=(Ċ.ϩ[ĉ]as
IMyTerminalBlock);if(ã==null||!ã.IsWorking){Ċ.ϩ.RemoveAt(ĉ);continue;}var î=ã as IMyTextSurfaceProvider;var A=ã as IMyTextSurface;var C=
ã as IMyTextPanel;æ b;Y=ü(ã);string[]ć=Y.Split(' ');ý=ć[0];ċ=ć.Length>1;if(C!=null){if(ù.ContainsKey(A)){b=ù[A];if(b.Þ==Y
+"@0"||(ċ&&b.Þ==ý)){ĉ++;continue;}ï(ã);}if(!ċ){b=new æ(e,Y+"@0",ã,A,0);var ð=new ę(this,b);ƣ.ș(ð,0,false,true);ú.Add(b,ð)
;å.º(b.Þ,b);ù.Add(A,b);ĉ++;continue;}b=å.v(ý);if(b==null){b=new æ(e,ý);å.º(ý,b);var ð=new ę(this,b);ƣ.ș(ð,0,false,true);ú
.Add(b,ð);}b.å.º(Y,ã);ù.Add(A,b);}else{if(î==null){ĉ++;continue;}for(int D=0;D<î.SurfaceCount;D++){A=î.GetSurface(D);if(ù
.ContainsKey(A)){b=ù[A];if(b.Þ==Y+'@'+D.ToString()){continue;}ñ(A,null);}if(ã.Ǹ(D,e.ɳ)==null)continue;b=new æ(e,Y+"@"+D.
ToString(),ã,A,D);var ð=new ę(this,b);ƣ.ș(ð,0,false,true);ú.Add(b,ð);å.º(b.Þ,b);ù.Add(A,b);}}ĉ++;}while(Ĉ<û.Ц()){if(!ƣ.ɔ(100))
return false;IMyTerminalBlock ã=û.ϩ[Ĉ];if(ã==null)continue;if(!Ċ.ϩ.Contains(ã)){ï(ã);}Ĉ++;}û.Ų();û.Ш(Ċ);if(!ö.ɜ&&ö.ϗ())ƣ.ș(ö,0
,false,true);return true;}public bool Ć(string ą){if(string.Compare(ą,"clear",true)==0){ö.Ќ();if(!ö.ɜ)ƣ.ș(ö,0);return
true;}if(string.Compare(ą,"boot",true)==0){ö.Ѝ=0;if(!ö.ɜ)ƣ.ș(ö,0);return true;}if(ą.Ǿ("scroll")){var Ą=new ϔ(e,this,ą);ƣ.ș(Ą
,0);return true;}if(string.Compare(ą,"props",true)==0){Х ă=e.Ŏ;var Ă=new List<IMyTerminalBlock>();var ā=new List<
ITerminalAction>();var Ā=new List<ITerminalProperty>();var ÿ=ƣ.ǋ.GridTerminalSystem.GetBlockWithName("DEBUG")as IMyTextPanel;if(ÿ==null
){return true;}ÿ.WriteText("Properties: ");foreach(var z in ă.С){ÿ.WriteText(z.Key+" =============="+"\n",true);z.Value(Ă
,null);if(Ă.Count<=0){ÿ.WriteText("No blocks\n",true);continue;}Ă[0].GetProperties(Ā,(b)=>{return b.Id!="Name"&&b.Id!=
"OnOff"&&!b.Id.StartsWith("Show");});foreach(var Ư in Ā){ÿ.WriteText("P "+Ư.Id+" "+Ư.TypeName+"\n",true);}Ā.Clear();Ă.Clear();}
}return false;}}class ǔ{Dictionary<string,æ>Ɖ=new Dictionary<string,æ>();List<string>À=new List<string>(30);public void º
(string r,æ z){if(!Ɖ.ContainsKey(r)){À.Add(r);Ɖ.Add(r,z);}}public int w(){return Ɖ.Count;}public æ v(string r){if(Ɖ.
ContainsKey(r))return Ɖ[r];return null;}public æ o(int n){return Ɖ[À[n]];}public void ª(string r){Ɖ.Remove(r);À.Remove(r);}public
void k(){À.Clear();Ɖ.Clear();}public void j(){À.Sort();}}class Ƀ{Ȥ ƣ;ɹ e;public MyDefinitionId ɂ=new MyDefinitionId(typeof(
VRage.Game.ObjectBuilders.Definitions.MyObjectBuilder_GasProperties),"Electricity");public MyDefinitionId Ɂ=new
MyDefinitionId(typeof(VRage.Game.ObjectBuilders.Definitions.MyObjectBuilder_GasProperties),"Oxygen");public MyDefinitionId ɀ=new
MyDefinitionId(typeof(VRage.Game.ObjectBuilders.Definitions.MyObjectBuilder_GasProperties),"Hydrogen");public Ƀ(Ȥ ƨ,ɹ Å){ƣ=ƨ;e=Å;}int
ȿ=0;public bool Ʉ(List<IMyTerminalBlock>Ă,ref double ɍ,ref double Ɍ,ref double ɋ,ref double Ɋ,ref double ɓ,ref double ɒ,
bool ó){if(!ó)ȿ=0;MyResourceSinkComponent Ȼ;MyResourceSourceComponent ɉ;for(;ȿ<Ă.Count;ȿ++){if(!ƣ.ɔ(600))return false;if(Ă[ȿ
].Components.TryGet<MyResourceSinkComponent>(out Ȼ)){ɍ+=Ȼ.CurrentInputByType(ɂ);Ɍ+=Ȼ.MaxRequiredInputByType(ɂ);}if(Ă[ȿ].
Components.TryGet<MyResourceSourceComponent>(out ɉ)){ɋ+=ɉ.CurrentOutputByType(ɂ);Ɋ+=ɉ.MaxOutputByType(ɂ);}var ɑ=(Ă[ȿ]as
IMyBatteryBlock);ɓ+=ɑ.CurrentStoredPower;ɒ+=ɑ.MaxStoredPower;}return true;}int ɐ=0;public bool ɏ(List<IMyTerminalBlock>Ă,MyDefinitionId
Ɏ,ref double ɍ,ref double Ɍ,ref double ɋ,ref double Ɋ,bool ó){if(!ó)ɐ=0;MyResourceSinkComponent Ȼ;
MyResourceSourceComponent ɉ;for(;ɐ<Ă.Count;ɐ++){if(!ƣ.ɔ(600))return false;if(Ă[ɐ].Components.TryGet<MyResourceSinkComponent>(out Ȼ)){ɍ+=Ȼ.
CurrentInputByType(Ɏ);Ɍ+=Ȼ.MaxRequiredInputByType(Ɏ);}if(Ă[ɐ].Components.TryGet<MyResourceSourceComponent>(out ɉ)){ɋ+=ɉ.
CurrentOutputByType(Ɏ);Ɋ+=ɉ.MaxOutputByType(Ɏ);}}return true;}int Ɉ=0;public bool ɇ(List<IMyTerminalBlock>Ă,string Ɇ,ref double Ʌ,ref
double Ƚ,bool ó){if(!ó){Ɉ=0;Ƚ=0;Ʌ=0;}MyResourceSinkComponent Ȼ;for(;Ɉ<Ă.Count;Ɉ++){if(!ƣ.ɔ(600))return false;var ŋ=Ă[Ɉ]as
IMyGasTank;if(ŋ==null)continue;double Ȩ=0;if(ŋ.Components.TryGet<MyResourceSinkComponent>(out Ȼ)){ListReader<MyDefinitionId>ȧ=Ȼ.
AcceptedResources;int D=0;for(;D<ȧ.Count;D++){if(string.Compare(ȧ[D].SubtypeId.ToString(),Ɇ,true)==0){Ȩ=ŋ.Capacity;Ƚ+=Ȩ;Ʌ+=Ȩ*ŋ.
FilledRatio;break;}}}}return true;}public string Ȧ(TimeSpan ȥ){string þ="";if(ȥ.Ticks<=0)return"-";if((int)ȥ.TotalDays>0)þ+=(long)ȥ
.TotalDays+" "+e.Đ.Ȃ("C5")+" ";if(ȥ.Hours>0||þ!="")þ+=ȥ.Hours+"h ";if(ȥ.Minutes>0||þ!="")þ+=ȥ.Minutes+"m ";return þ+ȥ.
Seconds+"s";}}class Ȥ{public const double ȣ=0.005;public const int Ȣ=1000;public const int ȩ=5000;public double ȡ{get{return Ȟ;
}}int ȟ=Ȣ;double Ȟ=0;List<ɢ>ȝ=new List<ɢ>(100);public MyGridProgram ǋ;public bool Ȝ=false;int ț=0;int Ț=0;public Ȥ(
MyGridProgram ǅ,int Ǆ=1,bool Ƞ=false){ǋ=ǅ;ț=Ǆ;Ȝ=Ƞ;}public void ș(ɢ ð,double ȼ,bool Ⱥ=false,bool ȹ=false){ð.ɜ=true;ð.ɗ(this);if(Ⱥ){ð.ɠ
=ȡ;ȝ.Insert(0,ð);return;}var ȸ=ȣ*(Ȝ?5:1);if(ȼ<=0)ȼ=ȸ;ð.ɠ=ȡ+ȼ;for(int D=0;D<ȝ.Count;D++){if(ȝ[D].ɠ>ð.ɠ){ȝ.Insert(D,ð);
return;}var ȷ=ȝ[D].ɠ;if(!ȹ&&ð.ɠ-ȷ<ȸ)ð.ɠ=ȷ+ȸ;}ȝ.Add(ð);}public void ȶ(ɢ ð){if(ȝ.Contains(ð)){ȝ.Remove(ð);ð.ɜ=false;}}public
void ȴ(ʞ ȵ,int Ȳ=1){if(ț==Ȳ)ǋ.Echo(ȵ.ɤ());}public void ȴ(string ȳ,int Ȳ=1){if(ț==Ȳ)ǋ.Echo(ȳ);}const double ȱ=(16.66666666/16
);double Ȱ=0;public void ȯ(){Ȱ+=ǋ.Runtime.TimeSinceLastRun.TotalSeconds*ȱ;}ʞ Ʒ=new ʞ();public void Ȯ(){double ȭ=ǋ.Runtime
.TimeSinceLastRun.TotalSeconds*ȱ+Ȱ;Ȱ=0;Ț=0;Ȟ+=ȭ;ȟ=(int)Math.Min((ȭ*60)*Ȣ,ȩ-1000);if(Ȝ&&ȟ>Ȣ*2)ȟ=Ȣ*2;while(ȝ.Count>=1){ɢ ð=
ȝ[0];if((ȟ-ǋ.Runtime.CurrentInstructionCount<=0)||(ð.ɠ>Ȟ)){int Ȭ=(int)(60*(ð.ɠ-Ȟ));if(Ȭ>=100){ǋ.Runtime.UpdateFrequency=
UpdateFrequency.Update100;}else{if(Ȭ>=10||Ȝ)ǋ.Runtime.UpdateFrequency=UpdateFrequency.Update10;else ǋ.Runtime.UpdateFrequency=
UpdateFrequency.Update1;}return;}ȝ.Remove(ð);if(!ð.ɧ()){ǋ.Runtime.UpdateFrequency=UpdateFrequency.Update1;break;}}}public void ȫ(int ŝ)
{Ț+=ŝ;}public int Ⱦ(){return(ȩ-ǋ.Runtime.CurrentInstructionCount-Ț);}public bool ɔ(int ɿ){return((ȟ-ǋ.Runtime.
CurrentInstructionCount-Ț)>=ɿ);}public void ʘ(){}}class ʖ:ɢ{MyShipVelocities ʕ;public Vector3D ʔ{get{return ʕ.LinearVelocity;}}public Vector3D
ʓ{get{return ʕ.AngularVelocity;}}double ʒ=0;public double ʑ{get{if(ʂ!=null)return ʂ.GetShipSpeed();else return ʒ;}}double
ʐ=0;public double ʏ{get{return ʐ;}}double ʎ=0;public double ʍ{get{return ʎ;}}double ʌ=0;double ʋ=0;public double ʊ{get{
return ʌ;}}MyShipMass ʉ;public double ʈ{get{return ʉ.TotalMass;}}public double ʇ{get{return ʉ.BaseMass;}}double ʆ=double.NaN;
public double ʅ{get{return ʆ;}}double ʄ=double.NaN;public double ʃ{get{return ʄ;}}IMyShipController ʂ=null;IMySlimBlock ʁ=null
;public IMyShipController ʀ{get{return ʂ;}}Vector3D ʗ;public ʖ(Ȥ ƨ){ɡ="ShipMgr";ƣ=ƨ;ʗ=ƣ.ǋ.Me.GetPosition();ɞ=0.5;}List<
IMyTerminalBlock>ʢ=new List<IMyTerminalBlock>(5);int ʡ=0;public override bool ɻ(bool ó){if(!ó){ʢ.Clear();ƣ.ǋ.GridTerminalSystem.
GetBlocksOfType<IMyShipController>(ʢ);ʡ=0;if(ʂ!=null&&ʂ.CubeGrid.GetCubeBlock(ʂ.Position)!=ʁ)ʂ=null;}if(ʢ.Count>0){for(;ʡ<ʢ.Count;ʡ++){
if(!ƣ.ɔ(20))return false;var ʠ=ʢ[ʡ]as IMyShipController;if(ʠ.IsMainCockpit||ʠ.IsUnderControl){ʂ=ʠ;ʁ=ʠ.CubeGrid.
GetCubeBlock(ʠ.Position);if(ʠ.IsMainCockpit){ʡ=ʢ.Count;break;}}}if(ʂ==null){ʂ=ʢ[0]as IMyShipController;ʁ=ʂ.CubeGrid.GetCubeBlock(ʂ.
Position);}ʉ=ʂ.CalculateShipMass();if(!ʂ.TryGetPlanetElevation(MyPlanetElevation.Sealevel,out ʆ))ʆ=double.NaN;if(!ʂ.
TryGetPlanetElevation(MyPlanetElevation.Surface,out ʄ))ʄ=double.NaN;ʕ=ʂ.GetShipVelocities();}double ʟ=ʒ;ʒ=ʔ.Length();ʐ=(ʒ-ʟ)/ɥ;if(-ʐ>ʎ)ʎ=-ʐ;
if(-ʐ>ʌ){ʌ=-ʐ;ʋ=ƣ.ȡ;}if(ƣ.ȡ-ʋ>5&&-ʐ>0.1)ʌ-=(ʌ+ʐ)*0.3f;return true;}}class ʞ{public StringBuilder Ʒ;public ʞ(int ʝ=0){Ʒ=new
StringBuilder(ʝ);}public int ʜ{get{return Ʒ.Length;}}public ʞ Ų(){Ʒ.Clear();return this;}public ʞ ʙ(string Ĭ){Ʒ.Append(Ĭ);return this
;}public ʞ ʙ(double ʛ){Ʒ.Append(ʛ);return this;}public ʞ ʙ(char ȁ){Ʒ.Append(ȁ);return this;}public ʞ ʙ(ʞ ʚ){Ʒ.Append(ʚ.Ʒ)
;return this;}public ʞ ʙ(string Ĭ,int ȏ,int ɣ){Ʒ.Append(Ĭ,ȏ,ɣ);return this;}public ʞ ʙ(char ȁ,int ŝ){Ʒ.Append(ȁ,ŝ);return
this;}public ʞ ɦ(int ȏ,int ɣ){Ʒ.Remove(ȏ,ɣ);return this;}public string ɤ(){return Ʒ.ToString();}public string ɤ(int ȏ,int ɣ)
{return Ʒ.ToString(ȏ,ɣ);}public char this[int r]{get{return Ʒ[r];}}}class ɢ{public string ɡ="MMTask";public double ɠ=0;
public double ɟ=0;public double ɥ=0;public double ɞ=-1;double ɝ=-1;public bool ɜ=false;public bool ɛ=false;double ɚ=0;public
bool ə=false;public bool ɘ=false;protected Ȥ ƣ;public void ɗ(Ȥ ƨ){ƣ=ƨ;if(ƣ.Ȝ){if(ɝ==-1){ɝ=ɞ;ɞ*=2;}else{ɞ=ɝ*2;}}else{if(ɝ!=-1
){ɞ=ɝ;ɝ=-1;}}}protected bool ɖ(double ȼ){ɚ=Math.Max(ȼ,0.0001);return true;}bool ɕ=false;public bool ɧ(){if(ɟ>0){ɥ=ƣ.ȡ-ɟ;ɛ
=ɻ(!ɛ);}else{var ɾ=false;if(!ɕ){ɥ=0;ɼ();ɕ=true;ɾ=true;ɛ=false;}if(!ɾ||!ə){ɛ=ɻ(false);if(!ɛ)ɟ=0.001;}}if(ɛ){ɟ=ƣ.ȡ;if((ɞ>=0
||ɚ>0)&&ɜ)ƣ.ș(this,(ɚ>0?ɚ:ɞ),false,ɘ);else{ɜ=false;ɟ=0;}}else{if(ɜ)ƣ.ș(this,0,true);}ɚ=0;return ɛ;}public void ɽ(){ƣ.ȶ(
this);ɺ();ɜ=false;ɛ=false;ɟ=0;}public virtual void ɼ(){}public virtual bool ɻ(bool ó){return true;}public virtual void ɺ(){}
}class ɹ{public const float ɸ=512;public const float ɷ=ɸ/0.7783784f;public const float ɶ=ɸ/0.7783784f;public const float
ɵ=ɷ;public const float ɴ=37;public string ɳ="T:[LCD]";public int ɲ=1;public bool ɱ=true;public List<string>ɰ=null;public
bool ɯ=true;public int ț=0;public float ɮ=1.0f;public float ɭ=1.0f;public float ɬ{get{return ɵ*ǟ.ѵ;}}public float ɫ{get{
return(float)ɬ-2*ǒ[Ǟ]*à;}}string ɪ;string ɩ;float ɨ=-1;Dictionary<string,float>Ȫ=new Dictionary<string,float>(2);Dictionary<
string,float>Ș=new Dictionary<string,float>(2);Dictionary<string,float>ƺ=new Dictionary<string,float>(2);public float Ǔ{get{
return ƺ[Ǟ];}}Dictionary<string,float>ǒ=new Dictionary<string,float>(2);Dictionary<string,float>Ǒ=new Dictionary<string,float>
(2);Dictionary<string,float>ǐ=new Dictionary<string,float>(2);int à=0;string ß="";Dictionary<string,char>Ǐ=new Dictionary
<string,char>(2);Dictionary<string,char>ǎ=new Dictionary<string,char>(2);Dictionary<string,char>Ǎ=new Dictionary<string,
char>(2);Dictionary<string,char>ǌ=new Dictionary<string,char>(2);public Ȥ ƣ;public Program ǋ;public Ƀ Ǌ;public Х Ŏ;public ʖ
ǉ;public ĵ ƫ;public ǲ Đ;public IMyGridTerminalSystem ǈ{get{return ǋ.GridTerminalSystem;}}public IMyProgrammableBlock Ǉ{
get{return ǋ.Me;}}public Action<string>ǆ{get{return ǋ.Echo;}}public ɹ(Program ǅ,int Ǆ,Ȥ ƨ){ƣ=ƨ;ț=Ǆ;ǋ=ǅ;Đ=new ǲ();}public
void Ǖ(){Ǌ=new Ƀ(ƣ,this);}public void Ǡ(){Ŏ=new Х(ƣ,this);Ŏ.Р();}public void ǡ(){ǉ=new ʖ(ƣ);ƣ.ș(ǉ,0);}Ѷ ǟ=null;public string
Ǟ{get{return ǟ.E;}}public bool ǝ{get{return(ǟ.š()==0);}}public bool ǜ(IMyTerminalBlock ã){if(ã==null||ã.WorldMatrix==
MatrixD.Identity)return true;return ǈ.GetBlockWithId(ã.EntityId)==null;}public Ѷ Ǜ(Ѷ ǚ,æ b){b.X();IMyTextSurface A=b.Ü;if(ǚ==
null)ǚ=new Ѷ(this);ǚ.E=A.Font;if(!ǒ.ContainsKey(ǚ.E))ǚ.E=ɪ;ǚ.ѵ=b.O()*(A.SurfaceSize.X/A.TextureSize.X)*Math.Max(1.0f,A.
TextureSize.X/A.TextureSize.Y)*ɮ/A.FontSize*(100f-A.TextPadding*2)/100;ß=b.ß;à=b.à;ǟ=ǚ;return ǚ;}public Ŭ Ǚ(Ŭ ä,æ b){b.X();
IMyTextSurface A=b.Ü;if(ä==null)ä=new Ŭ(this);ä.ŵ(b.Û);ä.Ż=b.R()*(A.SurfaceSize.Y/A.TextureSize.Y)*Math.Max(1.0f,A.TextureSize.Y/A.
TextureSize.X)*ɭ/A.FontSize*(100f-A.TextPadding*2)/100;ä.Ŵ();ß=b.ß;à=b.à;return ä;}public void ǘ(){ǟ.ũ();}public void ǘ(ʞ Ũ){if(ǟ.Ѵ
<=0)ǟ.ū(ß);ǟ.ū(Ũ);ǟ.ũ();}public void ǘ(string Ũ){if(ǟ.Ѵ<=0)ǟ.ū(ß);ǟ.ũ(Ũ);}public void Ǘ(string Ť){ǟ.ť(Ť,ß);}public void ǖ(
List<ʞ>ţ){ǟ.ŧ(ţ);}public void Ô(ʞ Ū,bool ƹ=true){if(ǟ.Ѵ<=0)ǟ.ū(ß);ǟ.ū(Ū);if(ƹ)ǟ.Ѵ+=Ǯ(Ū,ǟ.E);}public void Ô(string þ,bool ƹ=
true){if(ǟ.Ѵ<=0)ǟ.ū(ß);ǟ.ū(þ);if(ƹ)ǟ.Ѵ+=Ǯ(þ,ǟ.E);}public void Ƹ(ʞ Ū,float Ʋ=1.0f,float Ʊ=0f){Ƴ(Ū,Ʋ,Ʊ);ǟ.ũ();}public void Ƹ(
string þ,float Ʋ=1.0f,float Ʊ=0f){Ƴ(þ,Ʋ,Ʊ);ǟ.ũ();}ʞ Ʒ=new ʞ();public void Ƴ(ʞ Ū,float Ʋ=1.0f,float Ʊ=0f){float ƶ=Ǯ(Ū,ǟ.E);
float ư=Ʋ*ɵ*ǟ.ѵ-ǟ.Ѵ-Ʊ;if(à>0)ư-=2*ǒ[ǟ.E]*à;if(ư<ƶ){ǟ.ū(Ū);ǟ.Ѵ+=ƶ;return;}ư-=ƶ;int Ƶ=(int)Math.Floor(ư/ǒ[ǟ.E]);float ƴ=Ƶ*ǒ[ǟ.E
];Ʒ.Ų().ʙ(' ',Ƶ).ʙ(Ū);ǟ.ū(Ʒ);ǟ.Ѵ+=ƴ+ƶ;}public void Ƴ(string þ,float Ʋ=1.0f,float Ʊ=0f){float ƶ=Ǯ(þ,ǟ.E);float ư=Ʋ*ɵ*ǟ.ѵ-ǟ
.Ѵ-Ʊ;if(à>0)ư-=2*ǒ[ǟ.E]*à;if(ư<ƶ){ǟ.ū(þ);ǟ.Ѵ+=ƶ;return;}ư-=ƶ;int Ƶ=(int)Math.Floor(ư/ǒ[ǟ.E]);float ƴ=Ƶ*ǒ[ǟ.E];Ʒ.Ų().ʙ(' '
,Ƶ).ʙ(þ);ǟ.ū(Ʒ);ǟ.Ѵ+=ƴ+ƶ;}public void ǂ(ʞ Ū){ǁ(Ū);ǟ.ũ();}public void ǂ(string þ){ǁ(þ);ǟ.ũ();}public void ǁ(ʞ Ū){float ƶ=Ǯ
(Ū,ǟ.E);float ǀ=ɵ/2*ǟ.ѵ-ǟ.Ѵ;if(ǀ<ƶ/2){ǟ.ū(Ū);ǟ.Ѵ+=ƶ;return;}ǀ-=ƶ/2;int Ƶ=(int)Math.Round(ǀ/ǒ[ǟ.E],MidpointRounding.
AwayFromZero);float ƴ=Ƶ*ǒ[ǟ.E];Ʒ.Ų().ʙ(' ',Ƶ).ʙ(Ū);ǟ.ū(Ʒ);ǟ.Ѵ+=ƴ+ƶ;}public void ǁ(string þ){float ƶ=Ǯ(þ,ǟ.E);float ǀ=ɵ/2*ǟ.ѵ-ǟ.Ѵ;if(
ǀ<ƶ/2){ǟ.ū(þ);ǟ.Ѵ+=ƶ;return;}ǀ-=ƶ/2;int Ƶ=(int)Math.Round(ǀ/ǒ[ǟ.E],MidpointRounding.AwayFromZero);float ƴ=Ƶ*ǒ[ǟ.E];Ʒ.Ų().
ʙ(' ',Ƶ).ʙ(þ);ǟ.ū(Ʒ);ǟ.Ѵ+=ƴ+ƶ;}public void ƿ(double ƾ,float ƽ=1.0f,float Ʊ=0f,bool ƹ=true){if(à>0)Ʊ+=2*à*ǒ[ǟ.E];float Ƽ=ɵ
*ƽ*ǟ.ѵ-ǟ.Ѵ-Ʊ;if(Double.IsNaN(ƾ))ƾ=0;int ƻ=(int)(Ƽ/Ǒ[ǟ.E])-2;if(ƻ<=0)ƻ=2;int ǃ=Math.Min((int)(ƾ*ƻ)/100,ƻ);if(ǃ<0)ǃ=0;if(ǟ.
Ѵ<=0)ǟ.ū(ß);Ʒ.Ų().ʙ(Ǐ[ǟ.E]).ʙ(ǌ[ǟ.E],ǃ).ʙ(Ǎ[ǟ.E],ƻ-ǃ).ʙ(ǎ[ǟ.E]);ǟ.ū(Ʒ);if(ƹ)ǟ.Ѵ+=Ǒ[ǟ.E]*ƻ+2*ǐ[ǟ.E];}public void Ǣ(double
ƾ,float ƽ=1.0f,float Ʊ=0f){ƿ(ƾ,ƽ,Ʊ,false);ǟ.ũ();}public void Ţ(){ǟ.Ţ();}public void ȉ(æ C,Ŭ G=null){C.H(G);if(C.Ý)C.ä.ķ()
;}public void Ȉ(string ȇ,string þ){var C=ǋ.GridTerminalSystem.GetBlockWithName(ȇ)as IMyTextPanel;if(C==null)return;C.
WriteText(þ+"\n",true);}public string Ȇ(MyInventoryItem z){string ȅ=z.Type.TypeId.ToString();ȅ=ȅ.Substring(ȅ.LastIndexOf('_')+1);
return z.Type.SubtypeId+" "+ȅ;}public void Ȅ(string Î,out string Ì,out string Ë){int Ľ=Î.LastIndexOf(' ');if(Ľ>=0){Ì=Î.
Substring(0,Ľ);Ë=Î.Substring(Ľ+1);return;}Ì=Î;Ë="";}public string ȃ(string Î){string Ì,Ë;Ȅ(Î,out Ì,out Ë);return ȃ(Ì,Ë);}public
string ȃ(string Ì,string Ë){Ê z=ƫ.Í(Ì,Ë);if(z!=null){if(z.Ø.Length>0)return z.Ø;return z.É;}return System.Text.
RegularExpressions.Regex.Replace(Ì,"([a-z])([A-Z])","$1 $2");}public void ȗ(ref string Ì,ref string Ë){Ê z;if(ƫ.Õ.TryGetValue(Ì,out z)){Ì=
z.É;Ë=z.È;return;}z=ƫ.Í(Ì,Ë);if(z!=null){Ì=z.É;if(string.IsNullOrEmpty(Ë)&&(string.Compare(z.È,"Ore",true)==0)||(string.
Compare(z.È,"Ingot",true)==0))return;Ë=z.È;}}public string Ȗ(double Ȕ,bool ȓ=true,char Ȓ=' '){if(!ȓ)return Ȕ.ToString(
"#,###,###,###,###,###,###,###,###,###");string ȑ=" kMGTPEZY";double Ȑ=Ȕ;int ȏ=ȑ.IndexOf(Ȓ);var Ȏ=(ȏ<0?0:ȏ);while(Ȑ>=1000&&Ȏ+1<ȑ.Length){Ȑ/=1000;Ȏ++;}Ʒ.Ų().ʙ(
Math.Round(Ȑ,1,MidpointRounding.AwayFromZero));if(Ȏ>0)Ʒ.ʙ(" ").ʙ(ȑ[Ȏ]);return Ʒ.ɤ();}public string ȕ(double Ȕ,bool ȓ=true,
char Ȓ=' '){if(!ȓ)return Ȕ.ToString("#,###,###,###,###,###,###,###,###,###");string ȑ=" ktkMGTPEZY";double Ȑ=Ȕ;int ȏ=ȑ.
IndexOf(Ȓ);var Ȏ=(ȏ<0?0:ȏ);while(Ȑ>=1000&&Ȏ+1<ȑ.Length){Ȑ/=1000;Ȏ++;}Ʒ.Ų().ʙ(Math.Round(Ȑ,1,MidpointRounding.AwayFromZero));if(
Ȏ==1)Ʒ.ʙ(" kg");else if(Ȏ==2)Ʒ.ʙ(" t");else if(Ȏ>2)Ʒ.ʙ(" ").ʙ(ȑ[Ȏ]).ʙ("t");return Ʒ.ɤ();}public string ȍ(double ƾ){return
(Math.Floor(ƾ*10)/10).ToString("F1");}Dictionary<char,float>Ȍ=new Dictionary<char,float>();void ȋ(string Ȋ,float F){F+=1;
for(int D=0;D<Ȋ.Length;D++){if(F>Ȫ[ɪ])Ȫ[ɪ]=F;Ȍ.Add(Ȋ[D],F);}}public float Ǳ(char ȁ,string E){float Ƽ;if(E==ɩ||!Ȍ.
TryGetValue(ȁ,out Ƽ))return Ȫ[E];return Ƽ;}public float Ǯ(ʞ ǰ,string E){if(E==ɩ)return ǰ.ʜ*Ȫ[E];float ǯ=0;for(int D=0;D<ǰ.ʜ;D++)ǯ+=
Ǳ(ǰ[D],E);return ǯ;}public float Ǯ(string Ĭ,string E){if(E==ɩ)return Ĭ.Length*Ȫ[E];float ǯ=0;for(int D=0;D<Ĭ.Length;D++)ǯ
+=Ǳ(Ĭ[D],E);return ǯ;}public string Ǭ(string þ,float ǫ){if(ǫ/Ȫ[ǟ.E]>=þ.Length)return þ;float Ǫ=Ǯ(þ,ǟ.E);if(Ǫ<=ǫ)return þ;
float ǩ=Ǫ/þ.Length;ǫ-=Ș[ǟ.E];int Ǩ=(int)Math.Max(ǫ/ǩ,1);if(Ǩ<þ.Length/2){Ʒ.Ų().ʙ(þ,0,Ǩ);Ǫ=Ǯ(Ʒ,ǟ.E);}else{Ʒ.Ų().ʙ(þ);Ǩ=þ.
Length;}while(Ǫ>ǫ&&Ǩ>1){Ǩ--;Ǫ-=Ǳ(þ[Ǩ],ǟ.E);}if(Ʒ.ʜ>Ǩ)Ʒ.ɦ(Ǩ,Ʒ.ʜ-Ǩ);return Ʒ.ʙ("..").ɤ();}void ǧ(string Ǧ){ɪ=Ǧ;Ǐ[ɪ]=MMStyle.
BAR_START;ǎ[ɪ]=MMStyle.BAR_END;Ǎ[ɪ]=MMStyle.BAR_EMPTY;ǌ[ɪ]=MMStyle.BAR_FILL;Ȫ[ɪ]=0f;}void ǥ(string Ǥ,float ǭ){ɩ=Ǥ;ɨ=ǭ;Ȫ[ɩ]=ɨ+1;Ș[
ɩ]=2*(ɨ+1);Ǐ[ɩ]=MMStyle.BAR_MONO_START;ǎ[ɩ]=MMStyle.BAR_MONO_END;Ǎ[ɩ]=MMStyle.BAR_MONO_EMPTY;ǌ[ɩ]=MMStyle.BAR_MONO_FILL;ǒ
[ɩ]=Ǳ(' ',ɩ);Ǒ[ɩ]=Ǳ(Ǎ[ɩ],ɩ);ǐ[ɩ]=Ǳ(Ǐ[ɩ],ɩ);ƺ[ɩ]=Ǯ(" 100.0%",ɩ);}public void ǣ(){if(Ȍ.Count>0)return;
// Monospace font name, width of single character
// Change this if you want to use different (modded) monospace font
ǥ("Monospace", 24f);

// Classic/Debug font name (uses widths of characters below)
// Change this if you want to use different font name (non-monospace)
ǧ("Debug");
// Font characters width (font "aw" values here)
ȋ("3FKTabdeghknopqsuy£µÝàáâãäåèéêëðñòóôõöøùúûüýþÿāăąďđēĕėęěĝğġģĥħĶķńņňŉōŏőśŝşšŢŤŦũūŭůűųŶŷŸșȚЎЗКЛбдекруцяёђћўџ", 17f);
ȋ("ABDNOQRSÀÁÂÃÄÅÐÑÒÓÔÕÖØĂĄĎĐŃŅŇŌŎŐŔŖŘŚŜŞŠȘЅЊЖф□", 21f);
ȋ("#0245689CXZ¤¥ÇßĆĈĊČŹŻŽƒЁЌАБВДИЙПРСТУХЬ€", 19f);
ȋ("￥$&GHPUVY§ÙÚÛÜÞĀĜĞĠĢĤĦŨŪŬŮŰŲОФЦЪЯжы†‡", 20f);
ȋ("！ !I`ijl ¡¨¯´¸ÌÍÎÏìíîïĨĩĪīĮįİıĵĺļľłˆˇ˘˙˚˛˜˝ІЇії‹›∙", 8f);
ȋ("？7?Jcz¢¿çćĉċčĴźżžЃЈЧавийнопсъьѓѕќ", 16f);
ȋ("（）：《》，。、；【】(),.1:;[]ft{}·ţťŧț", 9f);
ȋ("+<=>E^~¬±¶ÈÉÊË×÷ĒĔĖĘĚЄЏЕНЭ−", 18f);
ȋ("L_vx«»ĹĻĽĿŁГгзлхчҐ–•", 15f);
ȋ("\"-rª­ºŀŕŗř", 10f);
ȋ("WÆŒŴ—…‰", 31f);
ȋ("'|¦ˉ‘’‚", 6f);
ȋ("@©®мшњ", 25f);
ȋ("mw¼ŵЮщ", 27f);
ȋ("/ĳтэє", 14f);
ȋ("\\°“”„", 12f);
ȋ("*²³¹", 11f);
ȋ("¾æœЉ", 28f);
ȋ("%ĲЫ", 24f);
ȋ("MМШ", 26f);
ȋ("½Щ", 29f);
ȋ("ю", 23f);
ȋ("ј", 7f);
ȋ("љ", 22f);
ȋ("ґ", 13f);
ȋ("™", 30f);
// End of font characters width
        ǒ[ɪ]=Ǳ(' ',ɪ);Ǒ[ɪ]=Ǳ(Ǎ[ɪ],ɪ);ǐ[ɪ]=Ǳ(Ǐ[ɪ],ɪ);ƺ[ɪ]=Ǯ(" 100.0%",ɪ);Ș[ɪ]=Ǳ('.',ɪ)*2;}}class ǲ{public string Ȃ(string
Ȁ){return TT[Ȁ];}
readonly Dictionary<string, string> TT = new Dictionary<string, string>
{
// TRANSLATION STRINGS
// msg id, text
{ "AC1", "Beschleunigung:" },
// amount
{ "A1", "LEER" },
{ "ALT1", "Flughöhe:"},
{ "ALT2", "Flughöhe:"},
{ "B1", "Automatic LCDs 2" },
{ "C1", "Anzahl:" },
{ "C2", "Ladung:" },
{ "C3", "Ungültiges Countdown Format, richtiges Format:" },
{ "C4", "ABGELAUFEN" },
{ "C5", "Tage" },
// customdata
{ "CD1", "Block nicht gefunden: " },
{ "CD2", "Blockname fehlt." },
{ "D1", "Name fehlt." },
{ "D2", "Keine Blöcke gefunden." },
{ "D3", "Keine beschädigten Blöcke gefunden." },
{ "DO1", "Keine Verbinder gefunden." }, // NEW
{ "DTU", "Ungültiges GPS Format" },
{ "GA", "Künst."}, // (not more than 5 characters)
{ "GN", "Natür."}, // (not more than 5 characters)
{ "GT", "Ges."}, // (not more than 5 characters)
{ "G1", "Ges. Gravitation:"},
{ "G2", "Natür. Gravitation:"},
{ "G3", "Künst. Gravitation:"},
{ "GNC", "Kein Cockpit vorhanden!"},
{ "H1", "Schreibe einen Befehle in die Benutzerdefinierten Daten des LCD-Panels." },
// inventory
{ "I1", "Erz" },
{ "I2", "Übersicht" },
{ "I3", "Erze" },
{ "I4", "Barren" },
{ "I5", "Komponenten" },
{ "I6", "Gas" },
{ "I7", "Munition" },
{ "I8", "Werkzeuge" },
{ "M1", "Container Masse:" },
// oxygen
{ "O1", "Sauerstoff-Leck" },
{ "O2", "Sauerstofffarmen" },
{ "O3", "Keine Sauerstoff-Blöcke gefunden." },
{ "O4", "Sauerstofftanks" },
// position
{ "P1", "Block nicht gefunden" },
{ "P2", "Position" },
// power
{ "P3", "Ladung" },
{ "P4", "Abgabe" },
{ "P5", "Zufuhr" },
{ "P6", "Keine Energiequelle gefunden!" },
{ "P7", "Batterien" },
{ "P8", "Gesamt-Abgabe" },
{ "P9", "Reaktoren" },
{ "P10", "Solarkollektoren" },
{ "P11", "Energieabgabe" },
{ "P12", "Motoren" }, // NEW!
{ "P13", "Windturbinen" }, // NEW!
{ "PT1", "Vollständig verbraucht in:" },
{ "PT2", "Vollständig geladen in:" },
{ "PU1", "Energieverbrauch:" },
{ "S1", "Geschwindigkeit:" },
{ "SM1", "Masse des Schiffs:" },
{ "SM2", "Gesamt-Masse des Schiffs:" },
{ "SD", "Bremsweg:" },
{ "ST", "Bremszeit:" },
// text
{ "T1", "Quell-LCD-Display nicht gefunden: " },
{ "T2", "Fehlender Name des Quell-LCD-Displays" },
// tanks
{ "T4", "Art des Tanks fehlt. Bsp: 'Tanks * Hydrogen'" },
{ "T5", "Keine {0}-Tanks gefunden." }, // {0} is tank type
{ "UC", "Unbekannter Befehl" },
// occupied & dampeners
{ "SC1", "Kein Steuerblock gefunden." },
{ "SCD", "Trägheitsdämpfer: " },
{ "SCO", "Belegt: " },
// working
{ "W1", "AUS" },
{ "W2", "ARBEITET" },
{ "W3", "LEERLAUF" },
{ "W4", "DEKOMPRIMIERT" },
{ "W5", "OFFEN" },
{ "W6", "GESCHLOSSEN" },
{ "W7", "VERRIEGELT" },
{ "W8", "ENTRIEGELT" },
{ "W9", "AN" },
{ "W10", "BEREIT" }
};
    }
}static class ǿ{public static bool Ǿ(this string Ĭ,string ǻ){return Ĭ.StartsWith(ǻ,StringComparison.
InvariantCultureIgnoreCase);}public static bool ǽ(this string Ĭ,string ǻ){if(Ĭ==null)return false;return Ĭ.IndexOf(ǻ,StringComparison.
InvariantCultureIgnoreCase)>=0;}public static bool Ǽ(this string Ĭ,string ǻ){return Ĭ.EndsWith(ǻ,StringComparison.InvariantCultureIgnoreCase);}}
static class Ǻ{public static string ǹ(this IMyTerminalBlock ã){int ł=ã.CustomData.IndexOf("\n---\n");if(ł<0){if(ã.CustomData.
StartsWith("---\n"))return ã.CustomData.Substring(4);return ã.CustomData;}return ã.CustomData.Substring(ł+5);}public static string
Ǹ(this IMyTerminalBlock ã,int Ľ,string Ƿ){string Ƕ=ã.ǹ();string ǵ="@"+Ľ.ToString()+" AutoLCD";string Ǵ='\n'+ǵ;int ł=0;if(
!Ƕ.StartsWith(ǵ,StringComparison.InvariantCultureIgnoreCase)){ł=Ƕ.IndexOf(Ǵ,StringComparison.InvariantCultureIgnoreCase);
}if(ł<0){if(Ľ==0){if(Ƕ.Length==0)return"";if(Ƕ[0]=='@')return null;ł=Ƕ.IndexOf("\n@");if(ł<0)return Ƕ;return Ƕ.Substring(
0,ł);}else return null;}int ǳ=Ƕ.IndexOf("\n@",ł+1);if(ǳ<0){if(ł==0)return Ƕ;return Ƕ.Substring(ł+1);}if(ł==0)return Ƕ.
Substring(0,ǳ);return Ƕ.Substring(ł+1,ǳ-ł);}