/* v:2.0194 (Cargo title override; Added broadcast controller & action relay block type)
* Automatic LCDs 2 - In-game script by MMaster
*
* Thank all of you for making amazing creations with this script, using it and helping each other use it.
* Its 2024 - it's been 9 years already since I uploaded first Automatic LCDs script and you are still using it (in "a bit" upgraded form).
* That's just amazing! I hope you will have many more years of fun with it :)
*
* LATEST UPDATE: 
*  Added title override option for Cargo commands allowing for custom text before the volume numbers instead of 'Cargo Used'
*  Recognize broadcast controller (broadcast) and action relay (relay) block types added in game Signal update.
*  Fix Antenna influencing total max power in PowerUsed command
*  
* Previous notable updates:
*  Add bar only variants to Power commands (PowerBar, PowerStoredBar, etc)
*  Improve InvListNN item name trimming
*  Fix Inventory commands not considering ingots when using item names that are common for ingots and ores
*  Fix output on some screens not filling whole screen (e.g. vertical Corner LCDs)
*  "Compress" mass value to use kg, t, .. etc even if no maximum mass is provided
*  Items that changed name can now have 2 short names to ensure compatibility with old blueprints
*   eg: old "Inventory * +200mmmissile" and new "Inventory * +rockets" will both work
*  Added C: modifier to name filter to filter on same construct as programmable block (on rotors & pistons, but not connectors)
*    Note: C: modifier works in the same way as T: modifier used for same grid filtering - check guide section 'Same construct blocks filtering'!
*  Cockpit (and other blocks) panels support - read guide section 'How to use with cockpits?'
*  Optimizations for servers running script limiter - use SlowMode!
*  Added SlowMode setting to considerably slow down the script (4-5 times less processing per second)
*  Now using MDK!
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
void Add(string sT, string mT, int q = 0, string dN = "", string sN = "", string sN2 = "", bool u = true) { ƥ.Ê(sT, mT, q, dN, sN, sN2, u); }
Î ƥ;Ț Ƣ;ì ϣ;ɬ ß=null;void Ϣ(string ƒ){}bool ϡ(string Ϟ){return Ϟ.Ƿ("true")?true:false;}void Ϡ(string ϟ,string Ϟ){string
Ȁ=ϟ.ToLower();switch(Ȁ){case"lcd_tag":LCD_TAG=Ϟ;break;case"slowmode":SlowMode=ϡ(Ϟ);break;case"enable_boot":ENABLE_BOOT=ϡ(
Ϟ);break;case"skip_content_type":SKIP_CONTENT_TYPE=ϡ(Ϟ);break;case"scroll_lines":int ϝ=0;if(int.TryParse(Ϟ,out ϝ)){
SCROLL_LINES=ϝ;}break;}}void Ϝ(){string[]ş=Me.CustomData.Split('\n');for(int n=0;n<ş.Length;n++){string Ť=ş[n];int Ļ=Ť.IndexOf('=');
if(Ļ<0){Ϣ(Ť);continue;}string ϛ=Ť.Substring(0,Ļ).Trim();string Ƕ=Ť.Substring(Ļ+1).Trim();Ϡ(ϛ,Ƕ);}}void Ϛ(Ț Ƣ){ƥ=new Î();
ItemsConf();Ϝ();ß=new ɬ(this,DebugLevel,Ƣ){ƥ=ƥ,ɦ=LCD_TAG,ɥ=SCROLL_LINES,ɤ=ENABLE_BOOT,ɣ=BOOT_FRAMES,ɢ=!MDK_IS_GREAT,ɠ=HEIGHT_MOD,
ɡ=WIDTH_MOD};ß.ǥ();}void ϙ(){Ƣ.Ǒ=this;ß.Ǒ=this;}Program(){Runtime.UpdateFrequency=UpdateFrequency.Update1;}void Main(
string Ă,UpdateType υ){try{if(Ƣ==null){Ƣ=new Ț(this,DebugLevel,SlowMode);Ϛ(Ƣ);ϣ=new ì(ß);Ƣ.ȴ(ϣ,0);}else{ϙ();ß.Ŋ.Ў();}if(Ă.
Length==0&&(υ&(UpdateType.Update1|UpdateType.Update10|UpdateType.Update100))==0){Ƣ.ȩ();return;}if(Ă!=""){if(ϣ.ă(Ă)){Ƣ.ȩ();
return;}}ϣ.ü=0;Ƣ.Ȩ();}catch(Exception ex){Echo("ERROR DESCRIPTION:\n"+ex.ToString());Me.Enabled=false;}}class τ:ɔ{ì Č;ɬ ß;
string Ă="";public τ(ɬ A,ì Ċ,string Ō){ɏ=-1;ɘ="ArgScroll";Ă=Ō;Č=Ċ;ß=A;}int ł;π σ;public override void ɯ(){σ=new π(Ʃ,ß.Ŋ);}int
ς=0;int Ĕ=0;ͱ ƒ;public override bool ɮ(bool ñ){if(!ñ){Ĕ=0;σ.Ŭ();ƒ=new ͱ(Ʃ);ς=0;}if(Ĕ==0){if(!ƒ.ʠ(Ă,ñ))return false;if(ƒ.ˠ
.Count>0){if(!int.TryParse(ƒ.ˠ[0].Ō,out ł))ł=1;else if(ł<1)ł=1;}if(ƒ.Ͱ.EndsWith("up"))ł=-ł;else if(!ƒ.Ͱ.EndsWith("down"))
ł=0;Ĕ++;ñ=false;}if(Ĕ==1){if(!σ.Ͼ("textpanel",ƒ.ˮ,ñ))return false;Ĕ++;ñ=false;}à í;for(;ς<σ.Д();ς++){if(!Ʃ.ʊ(20))return
false;IMyTextPanel ρ=σ.φ[ς]as IMyTextPanel;if(!Č.ø.TryGetValue(ρ,out í))continue;if(í==null||í.Ü!=ρ)continue;if(í.Õ)í.Ý.ĵ=10;
if(ł>0)í.Ý.ĸ(ł);else if(ł<0)í.Ý.ĺ(-ł);else í.Ý.Ĵ();í.D();}return true;}}class π{Ț Ʃ;Г ο;IMyCubeGrid ξ{get{return Ʃ.Ǒ.Me.
CubeGrid;}}IMyGridTerminalSystem Ǉ{get{return Ʃ.Ǒ.GridTerminalSystem;}}public List<IMyTerminalBlock>φ=new List<IMyTerminalBlock>
();public π(Ț Ƣ,Г Ϙ){Ʃ=Ƣ;ο=Ϙ;}int ϗ=0;public double ϖ(ref double ϕ,ref double ϔ,bool ñ){if(!ñ)ϗ=0;for(;ϗ<φ.Count;ϗ++){if(
!Ʃ.ʊ(4))return Double.NaN;IMyInventory Ϗ=φ[ϗ].GetInventory(0);if(Ϗ==null)continue;ϕ+=(double)Ϗ.CurrentVolume;ϔ+=(double)Ϗ
.MaxVolume;}ϕ*=1000;ϔ*=1000;return(ϔ>0?ϕ/ϔ*100:100);}int ϓ=0;double ϒ=0;public double ϑ(bool ñ){if(!ñ){ϓ=0;ϒ=0;}for(;ϓ<φ.
Count;ϓ++){if(!Ʃ.ʊ(6))return Double.NaN;for(int ϐ=0;ϐ<2;ϐ++){IMyInventory Ϗ=φ[ϓ].GetInventory(ϐ);if(Ϗ==null)continue;ϒ+=(
double)Ϗ.CurrentMass;}}return ϒ*1000;}int ώ=0;private bool ύ(bool ñ=false){if(!ñ)ώ=0;while(ώ<φ.Count){if(!Ʃ.ʊ(4))return false;
if(φ[ώ].CubeGrid!=ξ){φ.RemoveAt(ώ);continue;}ώ++;}return true;}int ό=0;private bool ϋ(bool ñ=false){if(!ñ)ό=0;var ϊ=Ʃ.Ǒ.Me
;while(ό<φ.Count){if(!Ʃ.ʊ(4))return false;if(!φ[ό].IsSameConstructAs(ϊ)){φ.RemoveAt(ό);continue;}ό++;}return true;}List<
IMyBlockGroup>ω=new List<IMyBlockGroup>();List<IMyTerminalBlock>ψ=new List<IMyTerminalBlock>();int Ϥ=0;public bool χ(string ˮ,bool ñ)
{int Є=ˮ.IndexOf(':');string Ͻ=(Є>=1&&Є<=2?ˮ.Substring(0,Є):"");bool Ђ=Ͻ.Contains("T");bool Љ=Ͻ.Contains("C");if(Ͻ!="")ˮ=
ˮ.Substring(Є+1);if(ˮ==""||ˮ=="*"){if(!ñ){ψ.Clear();Ǉ.GetBlocks(ψ);φ.AddList(ψ);}if(Ђ){if(!ύ(ñ))return false;}else if(Љ){
if(!ϋ(ñ))return false;}return true;}string Ѓ=(Ͻ.Contains("G")?ˮ.Trim():"");if(Ѓ!=""){if(!ñ){ω.Clear();Ǉ.GetBlockGroups(ω);
Ϥ=0;}for(;Ϥ<ω.Count;Ϥ++){IMyBlockGroup Ё=ω[Ϥ];if(string.Compare(Ё.Name,Ѓ,true)==0){if(!ñ){ψ.Clear();Ё.GetBlocks(ψ);φ.
AddList(ψ);}if(Ђ){if(!ύ(ñ))return false;}else if(Љ){if(!ϋ(ñ))return false;}return true;}}return true;}if(!ñ){ψ.Clear();Ǉ.
SearchBlocksOfName(ˮ,ψ);φ.AddList(ψ);}if(Ђ){if(!ύ(ñ))return false;}else if(Љ){if(!ϋ(ñ))return false;}return true;}List<IMyBlockGroup>Ј=new
List<IMyBlockGroup>();List<IMyTerminalBlock>Ї=new List<IMyTerminalBlock>();int І=0;int Ѕ=0;public bool Њ(string ʘ,string Ѓ,
bool Ђ,bool ñ){if(!ñ){Ј.Clear();Ǉ.GetBlockGroups(Ј);І=0;}for(;І<Ј.Count;І++){IMyBlockGroup Ё=Ј[І];if(string.Compare(Ё.Name,Ѓ
,true)==0){if(!ñ){Ѕ=0;Ї.Clear();Ё.GetBlocks(Ї);}else ñ=false;for(;Ѕ<Ї.Count;Ѕ++){if(!Ʃ.ʊ(6))return false;if(Ђ&&Ї[Ѕ].
CubeGrid!=ξ)continue;if(ο.Ϯ(Ї[Ѕ],ʘ))φ.Add(Ї[Ѕ]);}return true;}}return true;}List<IMyTerminalBlock>Ѐ=new List<IMyTerminalBlock>()
;int Ͽ=0;public bool Ͼ(string ʘ,string ˮ,bool ñ){int Є=ˮ.IndexOf(':');string Ͻ=(Є>=1&&Є<=2?ˮ.Substring(0,Є):"");bool Ђ=Ͻ.
Contains("T");bool Љ=Ͻ.Contains("C");if(Ͻ!="")ˮ=ˮ.Substring(Є+1);if(!ñ){Ѐ.Clear();Ͽ=0;}string Ѓ=(Ͻ.Contains("G")?ˮ.Trim():"");if
(Ѓ!=""){if(!Њ(ʘ,Ѓ,Ђ,ñ))return false;return true;}if(!ñ)ο.ϯ(ref Ѐ,ʘ);if(ˮ==""||ˮ=="*"){if(!ñ)φ.AddList(Ѐ);if(Ђ){if(!ύ(ñ))
return false;}else if(Љ){if(!ϋ(ñ))return false;}return true;}for(;Ͽ<Ѐ.Count;Ͽ++){if(!Ʃ.ʊ(4))return false;if(Ђ&&Ѐ[Ͽ].CubeGrid!=
ξ)continue;if(Ѐ[Ͽ].CustomName.Contains(ˮ))φ.Add(Ѐ[Ͽ]);}return true;}public void Ж(π Е){φ.AddList(Е.φ);}public void Ŭ(){φ.
Clear();}public int Д(){return φ.Count;}}class Г{Ț Ʃ;ɬ ß;public MyGridProgram Ǒ{get{return Ʃ.Ǒ;}}public IMyGridTerminalSystem
Ǉ{get{return Ʃ.Ǒ.GridTerminalSystem;}}public Г(Ț Ƣ,ɬ A){Ʃ=Ƣ;ß=A;}void В<ǳ>(List<IMyTerminalBlock>Б,Func<IMyTerminalBlock,
bool>А=null)where ǳ:class,IMyTerminalBlock{Ǉ.GetBlocksOfType<ǳ>(Б,А);}public Dictionary<string,Action<List<IMyTerminalBlock>
,Func<IMyTerminalBlock,bool>>>Џ;public void Ў(){if(Џ!=null)return;Џ=new Dictionary<string,Action<List<IMyTerminalBlock>,
Func<IMyTerminalBlock,bool>>>(){{"CargoContainer",В<IMyCargoContainer>},{"TextPanel",В<IMyTextPanel>},{"Assembler",В<
IMyAssembler>},{"Refinery",В<IMyRefinery>},{"Reactor",В<IMyReactor>},{"SolarPanel",В<IMySolarPanel>},{"BatteryBlock",В<
IMyBatteryBlock>},{"Beacon",В<IMyBeacon>},{"RadioAntenna",В<IMyRadioAntenna>},{"AirVent",В<IMyAirVent>},{"ConveyorSorter",В<
IMyConveyorSorter>},{"OxygenTank",В<IMyGasTank>},{"OxygenGenerator",В<IMyGasGenerator>},{"OxygenFarm",В<IMyOxygenFarm>},{"LaserAntenna",В
<IMyLaserAntenna>},{"Thrust",В<IMyThrust>},{"Gyro",В<IMyGyro>},{"SensorBlock",В<IMySensorBlock>},{"ShipConnector",В<
IMyShipConnector>},{"ReflectorLight",В<IMyReflectorLight>},{"InteriorLight",В<IMyInteriorLight>},{"LandingGear",В<IMyLandingGear>},{
"ProgrammableBlock",В<IMyProgrammableBlock>},{"TimerBlock",В<IMyTimerBlock>},{"MotorStator",В<IMyMotorStator>},{"PistonBase",В<
IMyPistonBase>},{"Projector",В<IMyProjector>},{"ShipMergeBlock",В<IMyShipMergeBlock>},{"SoundBlock",В<IMySoundBlock>},{"Collector",В<
IMyCollector>},{"JumpDrive",В<IMyJumpDrive>},{"Door",В<IMyDoor>},{"GravityGeneratorSphere",В<IMyGravityGeneratorSphere>},{
"GravityGenerator",В<IMyGravityGenerator>},{"ShipDrill",В<IMyShipDrill>},{"ShipGrinder",В<IMyShipGrinder>},{"ShipWelder",В<IMyShipWelder>}
,{"Parachute",В<IMyParachute>},{"LargeGatlingTurret",В<IMyLargeGatlingTurret>},{"LargeInteriorTurret",В<
IMyLargeInteriorTurret>},{"LargeMissileTurret",В<IMyLargeMissileTurret>},{"SmallGatlingGun",В<IMySmallGatlingGun>},{
"SmallMissileLauncherReload",В<IMySmallMissileLauncherReload>},{"SmallMissileLauncher",В<IMySmallMissileLauncher>},{"VirtualMass",В<IMyVirtualMass>}
,{"Warhead",В<IMyWarhead>},{"FunctionalBlock",В<IMyFunctionalBlock>},{"LightingBlock",В<IMyLightingBlock>},{
"ControlPanel",В<IMyControlPanel>},{"Cockpit",В<IMyCockpit>},{"TransponderBlock",В<IMyTransponder>},{"BroadcastController",В<
IMyBroadcastController>},{"CryoChamber",В<IMyCryoChamber>},{"MedicalRoom",В<IMyMedicalRoom>},{"RemoteControl",В<IMyRemoteControl>},{
"ButtonPanel",В<IMyButtonPanel>},{"CameraBlock",В<IMyCameraBlock>},{"OreDetector",В<IMyOreDetector>},{"ShipController",В<
IMyShipController>},{"SafeZoneBlock",В<IMySafeZoneBlock>},{"Decoy",В<IMyDecoy>}};}public void Ѝ(ref List<IMyTerminalBlock>Ā,string Ќ){
Action<List<IMyTerminalBlock>,Func<IMyTerminalBlock,bool>>Ћ;if(Ќ=="SurfaceProvider"){Ǉ.GetBlocksOfType<IMyTextSurfaceProvider>
(Ā);return;}if(Џ.TryGetValue(Ќ,out Ћ))Ћ(Ā,null);else{if(Ќ=="WindTurbine"){Ǉ.GetBlocksOfType<IMyPowerProducer>(Ā,(ϰ)=>ϰ.
BlockDefinition.TypeIdString.EndsWith("WindTurbine"));return;}if(Ќ=="HydrogenEngine"){Ǉ.GetBlocksOfType<IMyPowerProducer>(Ā,(ϰ)=>ϰ.
BlockDefinition.TypeIdString.EndsWith("HydrogenEngine"));return;}if(Ќ=="StoreBlock"){Ǉ.GetBlocksOfType<IMyFunctionalBlock>(Ā,(ϰ)=>ϰ.
BlockDefinition.TypeIdString.EndsWith("StoreBlock"));return;}if(Ќ=="ContractBlock"){Ǉ.GetBlocksOfType<IMyFunctionalBlock>(Ā,(ϰ)=>ϰ.
BlockDefinition.TypeIdString.EndsWith("ContractBlock"));return;}if(Ќ=="VendingMachine"){Ǉ.GetBlocksOfType<IMyFunctionalBlock>(Ā,(ϰ)=>ϰ.
BlockDefinition.TypeIdString.EndsWith("VendingMachine"));return;}}}public void ϯ(ref List<IMyTerminalBlock>Ā,string ϭ){Ѝ(ref Ā,ϫ(ϭ.Trim
()));}public bool Ϯ(IMyTerminalBlock Ü,string ϭ){string Ϭ=ϫ(ϭ);switch(Ϭ){case"FunctionalBlock":return true;case
"ShipController":return(Ü as IMyShipController!=null);default:return Ü.BlockDefinition.TypeIdString.Contains(ϫ(ϭ));}}public string ϫ(
string Ϫ){if(Ϫ=="surfaceprovider")return"SurfaceProvider";if(Ϫ.Ǹ("carg")||Ϫ.Ǹ("conta"))return"CargoContainer";if(Ϫ.Ǹ("text")||
Ϫ.Ǹ("lcd"))return"TextPanel";if(Ϫ.Ǹ("coc"))return"Cockpit";if(Ϫ.Ǹ("ass"))return"Assembler";if(Ϫ.Ǹ("refi"))return
"Refinery";if(Ϫ.Ǹ("reac"))return"Reactor";if(Ϫ.Ǹ("solar"))return"SolarPanel";if(Ϫ.Ǹ("wind"))return"WindTurbine";if(Ϫ.Ǹ("hydro")&&Ϫ
.Contains("eng"))return"HydrogenEngine";if(Ϫ.Ǹ("bat"))return"BatteryBlock";if(Ϫ.Ǹ("bea"))return"Beacon";if(Ϫ.Ƿ("vent"))
return"AirVent";if(Ϫ.Ƿ("sorter"))return"ConveyorSorter";if(Ϫ.Ƿ("tank"))return"OxygenTank";if(Ϫ.Ƿ("farm")&&Ϫ.Ƿ("oxy"))return
"OxygenFarm";if(Ϫ.Ƿ("gene")&&Ϫ.Ƿ("oxy"))return"OxygenGenerator";if(Ϫ.Ƿ("cryo"))return"CryoChamber";if(string.Compare(Ϫ,
"laserantenna",true)==0)return"LaserAntenna";if(Ϫ.Ƿ("antenna"))return"RadioAntenna";if(Ϫ.Ǹ("thrust"))return"Thrust";if(Ϫ.Ǹ("gyro"))
return"Gyro";if(Ϫ.Ǹ("sensor"))return"SensorBlock";if(Ϫ.Ƿ("connector"))return"ShipConnector";if(Ϫ.Ǹ("reflector")||Ϫ.Ǹ(
"spotlight"))return"ReflectorLight";if((Ϫ.Ǹ("inter")&&Ϫ.ǵ("light")))return"InteriorLight";if(Ϫ.Ǹ("land"))return"LandingGear";if(Ϫ.Ǹ
("program"))return"ProgrammableBlock";if(Ϫ.Ǹ("timer"))return"TimerBlock";if(Ϫ.Ǹ("motor")||Ϫ.Ǹ("rotor"))return
"MotorStator";if(Ϫ.Ǹ("piston"))return"PistonBase";if(Ϫ.Ǹ("proj"))return"Projector";if(Ϫ.Ƿ("merge"))return"ShipMergeBlock";if(Ϫ.Ǹ(
"sound"))return"SoundBlock";if(Ϫ.Ǹ("col"))return"Collector";if(Ϫ.Ƿ("jump"))return"JumpDrive";if(string.Compare(Ϫ,"door",true)==
0)return"Door";if((Ϫ.Ƿ("grav")&&Ϫ.Ƿ("sphe")))return"GravityGeneratorSphere";if(Ϫ.Ƿ("grav"))return"GravityGenerator";if(Ϫ.
ǵ("drill"))return"ShipDrill";if(Ϫ.Ƿ("grind"))return"ShipGrinder";if(Ϫ.ǵ("welder"))return"ShipWelder";if(Ϫ.Ǹ("parach"))
return"Parachute";if((Ϫ.Ƿ("turret")&&Ϫ.Ƿ("gatl")))return"LargeGatlingTurret";if((Ϫ.Ƿ("turret")&&Ϫ.Ƿ("inter")))return
"LargeInteriorTurret";if((Ϫ.Ƿ("turret")&&Ϫ.Ƿ("miss")))return"LargeMissileTurret";if(Ϫ.Ƿ("gatl"))return"SmallGatlingGun";if((Ϫ.Ƿ("launcher")&&
Ϫ.Ƿ("reload")))return"SmallMissileLauncherReload";if((Ϫ.Ƿ("launcher")))return"SmallMissileLauncher";if(Ϫ.Ƿ("mass"))return
"VirtualMass";if(string.Compare(Ϫ,"warhead",true)==0)return"Warhead";if(Ϫ.Ǹ("func"))return"FunctionalBlock";if(string.Compare(Ϫ,
"shipctrl",true)==0)return"ShipController";if(Ϫ.StartsWith("broadcast"))return"BroadcastController";if(Ϫ.Contains("transponder")||
Ϫ.Contains("relay"))return"TransponderBlock";if(Ϫ.Ǹ("light"))return"LightingBlock";if(Ϫ.Ǹ("contr"))return"ControlPanel";
if(Ϫ.Ǹ("medi"))return"MedicalRoom";if(Ϫ.Ǹ("remote"))return"RemoteControl";if(Ϫ.Ǹ("but"))return"ButtonPanel";if(Ϫ.Ǹ("cam"))
return"CameraBlock";if(Ϫ.Ƿ("detect"))return"OreDetector";if(Ϫ.Ǹ("safe"))return"SafeZoneBlock";if(Ϫ.Ǹ("store"))return
"StoreBlock";if(Ϫ.Ǹ("contract"))return"ContractBlock";if(Ϫ.Ǹ("vending"))return"VendingMachine";if(Ϫ.Ǹ("decoy"))return"Decoy";return
"Unknown";}public string ϱ(IMyBatteryBlock ņ){string ϩ="";if(ņ.ChargeMode==ChargeMode.Recharge)ϩ="(+) ";else if(ņ.ChargeMode==
ChargeMode.Discharge)ϩ="(-) ";else ϩ="(±) ";return ϩ+ß.ȇ((ņ.CurrentStoredPower/ņ.MaxStoredPower)*100.0f)+"%";}Dictionary<
MyLaserAntennaStatus,string>Ϩ=new Dictionary<MyLaserAntennaStatus,string>(){{MyLaserAntennaStatus.Idle,"IDLE"},{MyLaserAntennaStatus.
Connecting,"CONNECTING"},{MyLaserAntennaStatus.Connected,"CONNECTED"},{MyLaserAntennaStatus.OutOfRange,"OUT OF RANGE"},{
MyLaserAntennaStatus.RotatingToTarget,"ROTATING"},{MyLaserAntennaStatus.SearchingTargetForAntenna,"SEARCHING"}};public string ϧ(
IMyLaserAntenna ń){return Ϩ[ń.Status];}public double Ϧ(IMyJumpDrive Ņ,out double ʟ,out double Ɗ){ʟ=Ņ.CurrentStoredPower;Ɗ=Ņ.
MaxStoredPower;return(Ɗ>0?ʟ/Ɗ*100:0);}public double ϥ(IMyJumpDrive Ņ){double ʟ=Ņ.CurrentStoredPower;double Ɗ=Ņ.MaxStoredPower;return(Ɗ
>0?ʟ/Ɗ*100:0);}}class ϲ:ɔ{ɬ ß;ì Č;public int ϼ=0;public ϲ(ɬ A,ì Ĝ){ɘ="BootPanelsTask";ɏ=1;ß=A;Č=Ĝ;if(!ß.ɤ){ϼ=int.MaxValue
;Č.ö=true;}}ǫ İ;public override void ɯ(){İ=ß.İ;}public override bool ɮ(bool ñ){if(ϼ>ß.ɣ.Count){ɰ();return true;}if(!ñ&&ϼ
==0){Č.ö=false;}if(!Ϲ(ñ))return false;ϼ++;return true;}public override void ɭ(){Č.ö=true;}public void ϻ(){ȹ Þ=Č.Þ;for(int
n=0;n<Þ.e();n++){à í=Þ.Z(n);í.µ();}ϼ=(ß.ɤ?0:int.MaxValue);}int n;ŗ Ϻ=null;public bool Ϲ(bool ñ){ȹ Þ=Č.Þ;if(!ñ)n=0;int ϸ=0
;for(;n<Þ.e();n++){if(!Ʃ.ʊ(40)||ϸ>5)return false;à í=Þ.Z(n);Ϻ=ß.Ǘ(Ϻ,í);float?Ϸ=í.Ô?.FontSize;if(Ϸ!=null&&Ϸ>3f)continue;if
(Ϻ.ű.Count<=0)Ϻ.ŭ(ß.Ǚ(null,í));else ß.Ǚ(Ϻ.ű[0],í);ß.Ş();ß.ƶ(İ.ǳ("B1"));double ʚ=(double)ϼ/ß.ɣ.Count*100;ß.Ƹ(ʚ);if(ϼ==ß.ɣ.
Count){ß.ǖ("");ß.ƶ("Version 2.0194");ß.ƶ("by MMaster");ß.ƶ("");ß.ƶ("übersetzt von Ich_73");}else ß.Ǖ(ß.ɣ[ϼ]);bool Õ=í.Õ;í.Õ=false;ß.Ȃ(í,Ϻ);í.Õ=Õ;ϸ++;}return
true;}public bool ϵ(){return ϼ<=ß.ɣ.Count;}}public enum ϴ{ϳ=0,ν=1,ˊ=2,ʤ=3,ˈ=4,ˇ=5,ˆ=6,ˁ=7,ˀ=8,ʿ=9,ʾ=10,ʽ=11,ʼ=12,ʻ=13,ˉ=14,ʺ
=15,ʸ=16,ʷ=17,ʶ=18,ʵ=19,ʴ=20,ʳ=21,ʲ=22,ʱ=23,ʰ=24,ʯ=25,ʹ=26,ʮ=27,ˋ=28,ʹ=29,ͳ=30,Ͳ=31,}class ͱ{Ț Ʃ;public string Ͱ="";
public string ˮ="";public string ˬ="";public string ˤ="";public ϴ ˣ=ϴ.ϳ;public ͱ(Ț Ƣ){Ʃ=Ƣ;}ϴ ˢ(){if(Ͱ=="echo"||Ͱ=="center"||Ͱ
=="right")return ϴ.ν;if(Ͱ.StartsWith("hscroll"))return ϴ.ͳ;if(Ͱ.StartsWith("inventory")||Ͱ.StartsWith("missing")||Ͱ.
StartsWith("invlist"))return ϴ.ˊ;if(Ͱ.StartsWith("working"))return ϴ.ʶ;if(Ͱ.StartsWith("cargo"))return ϴ.ʤ;if(Ͱ.StartsWith("mass")
)return ϴ.ˈ;if(Ͱ.StartsWith("shipmass"))return ϴ.ʱ;if(Ͱ=="oxygen")return ϴ.ˇ;if(Ͱ.StartsWith("tanks"))return ϴ.ˆ;if(Ͱ.
StartsWith("powertime"))return ϴ.ˁ;if(Ͱ.StartsWith("powerused"))return ϴ.ˀ;if(Ͱ.StartsWith("power"))return ϴ.ʿ;if(Ͱ.StartsWith(
"speed"))return ϴ.ʾ;if(Ͱ.StartsWith("accel"))return ϴ.ʽ;if(Ͱ.StartsWith("alti"))return ϴ.ʯ;if(Ͱ.StartsWith("charge"))return ϴ.ʼ
;if(Ͱ.StartsWith("docked"))return ϴ.Ͳ;if(Ͱ.StartsWith("time")||Ͱ.StartsWith("date"))return ϴ.ʻ;if(Ͱ.StartsWith(
"countdown"))return ϴ.ˉ;if(Ͱ.StartsWith("textlcd"))return ϴ.ʺ;if(Ͱ.EndsWith("count"))return ϴ.ʸ;if(Ͱ.StartsWith("dampeners")||Ͱ.
StartsWith("occupied"))return ϴ.ʷ;if(Ͱ.StartsWith("damage"))return ϴ.ʵ;if(Ͱ.StartsWith("amount"))return ϴ.ʴ;if(Ͱ.StartsWith("pos")
)return ϴ.ʳ;if(Ͱ.StartsWith("distance"))return ϴ.ʰ;if(Ͱ.StartsWith("details"))return ϴ.ʲ;if(Ͱ.StartsWith("stop"))return ϴ
.ʹ;if(Ͱ.StartsWith("gravity"))return ϴ.ʮ;if(Ͱ.StartsWith("customdata"))return ϴ.ˋ;if(Ͱ.StartsWith("prop"))return ϴ.ʹ;
return ϴ.ϳ;}public Ɠ ˡ(){switch(ˣ){case ϴ.ν:return new Ҷ();case ϴ.ˊ:return new ҁ();case ϴ.ʤ:return new Ͷ();case ϴ.ˈ:return new
ҽ();case ϴ.ˇ:return new Ҽ();case ϴ.ˆ:return new ѫ();case ϴ.ˁ:return new я();case ϴ.ˀ:return new Э();case ϴ.ʿ:return new ӄ
();case ϴ.ʾ:return new ў();case ϴ.ʽ:return new ʝ();case ϴ.ʼ:return new έ();case ϴ.ʻ:return new Ι();case ϴ.ˉ:return new Α(
);case ϴ.ʺ:return new ĳ();case ϴ.ʸ:return new ʗ();case ϴ.ʷ:return new њ();case ϴ.ʶ:return new Ń();case ϴ.ʵ:return new ͻ()
;case ϴ.ʴ:return new Ӛ();case ϴ.ʳ:return new ӆ();case ϴ.ʲ:return new Ε();case ϴ.ʱ:return new Ѣ();case ϴ.ʰ:return new ҩ();
case ϴ.ʯ:return new ʙ();case ϴ.ʹ:return new ћ();case ϴ.ʮ:return new ҵ();case ϴ.ˋ:return new Ά();case ϴ.ʹ:return new ѿ();case
ϴ.ͳ:return new Ҵ();case ϴ.Ͳ:return new Ұ();default:return new Ɠ();}}public List<ˌ>ˠ=new List<ˌ>();string[]ˑ=null;string ː
="";bool ˏ=false;int œ=1;public bool ʠ(string ˎ,bool ñ){if(!ñ){ˣ=ϴ.ϳ;ˮ="";Ͱ="";ˬ=ˎ.TrimStart(' ');ˠ.Clear();if(ˬ=="")
return true;int ˍ=ˬ.IndexOf(' ');if(ˍ<0||ˍ>=ˬ.Length-1)ˤ="";else ˤ=ˬ.Substring(ˍ+1);ˑ=ˬ.Split(' ');ː="";ˏ=false;Ͱ=ˑ[0].ToLower
();œ=1;}for(;œ<ˑ.Length;œ++){if(!Ʃ.ʊ(40))return false;string Ō=ˑ[œ];if(Ō=="")continue;if(Ō[0]=='{'&&Ō[Ō.Length-1]=='}'){Ō
=Ō.Substring(1,Ō.Length-2);if(Ō=="")continue;if(ˮ=="")ˮ=Ō;else ˠ.Add(new ˌ(Ō));continue;}if(Ō[0]=='{'){ˏ=true;ː=Ō.
Substring(1);continue;}if(Ō[Ō.Length-1]=='}'){ˏ=false;ː+=' '+Ō.Substring(0,Ō.Length-1);if(ˮ=="")ˮ=ː;else ˠ.Add(new ˌ(ː));continue
;}if(ˏ){if(ː.Length!=0)ː+=' ';ː+=Ō;continue;}if(ˮ=="")ˮ=Ō;else ˠ.Add(new ˌ(Ō));}ˣ=ˢ();return true;}}class ˌ{public string
ʭ="";public string ʣ="";public string Ō="";public List<string>ʢ=new List<string>();public ˌ(string ʡ){Ō=ʡ;}public void ʠ(
){if(Ō==""||ʭ!=""||ʣ!=""||ʢ.Count>0)return;string ʟ=Ō.Trim();if(ʟ[0]=='+'||ʟ[0]=='-'){ʭ+=ʟ[0];ʟ=Ō.Substring(1);}string[]Ɵ
=ʟ.Split('/');string ʞ=Ɵ[0];if(Ɵ.Length>1){ʣ=Ɵ[0];ʞ=Ɵ[1];}else ʣ="";if(ʞ.Length>0){string[]Ą=ʞ.Split(',');for(int n=0;n<Ą
.Length;n++)if(Ą[n]!="")ʢ.Add(Ą[n]);}}}class ʝ:Ɠ{public ʝ(){ɏ=0.5;ɘ="CmdAccel";}public override bool Ə(bool ñ){double ʛ=0
;if(ƒ.ˮ!="")double.TryParse(ƒ.ˮ.Trim(),out ʛ);ß.Ê(İ.ǳ("AC1")+" ");ß.Ƶ(ß.ǈ.ɿ.ToString("F1")+" m/s²");if(ʛ>0){double ʚ=ß.ǈ.
ɿ/ʛ*100;ß.Ƹ(ʚ);}return true;}}class ʙ:Ɠ{public ʙ(){ɏ=1;ɘ="CmdAltitude";}public override bool Ə(bool ñ){string ʘ=(ƒ.Ͱ.
EndsWith("sea")?"sea":"ground");switch(ʘ){case"sea":ß.Ê(İ.ǳ("ALT1"));ß.Ƶ(ß.ǈ.ɵ.ToString("F0")+" m");break;default:ß.Ê(İ.ǳ("ALT2"
));ß.Ƶ(ß.ǈ.ɳ.ToString("F0")+" m");break;}return true;}}class ʗ:Ɠ{public ʗ(){ɏ=15;ɘ="CmdBlockCount";}π ŕ;public override
void ɯ(){ŕ=new π(Ʃ,ß.Ŋ);}bool ʖ;bool ʜ;int œ=0;int Ĕ=0;public override bool Ə(bool ñ){if(!ñ){ʖ=(ƒ.Ͱ=="enabledcount");ʜ=(ƒ.Ͱ
=="prodcount");œ=0;Ĕ=0;}if(ƒ.ˠ.Count==0){if(Ĕ==0){if(!ñ)ŕ.Ŭ();if(!ŕ.χ(ƒ.ˮ,ñ))return false;Ĕ++;ñ=false;}if(!ʦ(ŕ,"blocks",ʖ,
ʜ,ñ))return false;return true;}for(;œ<ƒ.ˠ.Count;œ++){ˌ Ō=ƒ.ˠ[œ];if(!ñ)Ō.ʠ();if(!Ŕ(Ō,ñ))return false;ñ=false;}return true;
}int Ő=0;int ő=0;bool Ŕ(ˌ Ō,bool ñ){if(!ñ){Ő=0;ő=0;}for(;Ő<Ō.ʢ.Count;Ő++){if(ő==0){if(!ñ)ŕ.Ŭ();if(!ŕ.Ͼ(Ō.ʢ[Ő],ƒ.ˮ,ñ))
return false;ő++;ñ=false;}if(!ʦ(ŕ,Ō.ʢ[Ő],ʖ,ʜ,ñ))return false;ő=0;ñ=false;}return true;}Dictionary<string,int>ʫ=new Dictionary<
string,int>();Dictionary<string,int>ʪ=new Dictionary<string,int>();List<string>ʩ=new List<string>();int ć=0;int ʬ=0;int ʨ=0;ʐ
ʧ=new ʐ();bool ʦ(π Ā,string ʘ,bool ʖ,bool ʜ,bool ñ){if(Ā.Д()==0){ʧ.Ŭ().ɱ(char.ToUpper(ʘ[0])).ɱ(ʘ.ToLower(),1,ʘ.Length-1);
ß.Ê(ʧ.ɱ(" ").ɱ(İ.ǳ("C1")).ɱ(" "));string ʥ=(ʖ||ʜ?"0 / 0":"0");ß.Ƶ(ʥ);return true;}if(!ñ){ʫ.Clear();ʪ.Clear();ʩ.Clear();ć=
0;ʬ=0;ʨ=0;}if(ʨ==0){for(;ć<Ā.Д();ć++){if(!Ʃ.ʊ(15))return false;IMyProductionBlock ŉ=Ā.φ[ć]as IMyProductionBlock;ʧ.Ŭ().ɱ(Ā
.φ[ć].DefinitionDisplayNameText);string Ȁ=ʧ.ɖ();if(ʩ.Contains(Ȁ)){ʫ[Ȁ]++;if((ʖ&&Ā.φ[ć].IsWorking)||(ʜ&&ŉ!=null&&ŉ.
IsProducing))ʪ[Ȁ]++;}else{ʫ.Add(Ȁ,1);ʩ.Add(Ȁ);if(ʖ||ʜ)if((ʖ&&Ā.φ[ć].IsWorking)||(ʜ&&ŉ!=null&&ŉ.IsProducing))ʪ.Add(Ȁ,1);else ʪ.Add(Ȁ
,0);}}ʨ++;ñ=false;}for(;ʬ<ʫ.Count;ʬ++){if(!Ʃ.ʊ(8))return false;ß.Ê(ʩ[ʬ]+" "+İ.ǳ("C1")+" ");string ʥ=(ʖ||ʜ?ʪ[ʩ[ʬ]]+" / ":
"")+ʫ[ʩ[ʬ]];ß.Ƶ(ʥ);}return true;}}class Ͷ:Ɠ{π ŕ;public Ͷ(){ɏ=2;ɘ="CmdCargo";}public override void ɯ(){ŕ=new π(Ʃ,ß.Ŋ);}bool
ά=true;bool ͺ=false;bool Ϋ=false;bool Τ=false;double Ϊ=0;double Ω=0;string Ψ;int Ĕ=0;public override bool Ə(bool ñ){if(!ñ
){ŕ.Ŭ();ά=ƒ.Ͱ.Contains("all");Τ=ƒ.Ͱ.EndsWith("bar");ͺ=(ƒ.Ͱ[ƒ.Ͱ.Length-1]=='x');Ϋ=(ƒ.Ͱ[ƒ.Ͱ.Length-1]=='p');Ϊ=0;Ω=0;Ψ="";Ĕ=
0;}if(Ĕ==0){if(ά){if(!ŕ.χ(ƒ.ˮ,ñ))return false;}else{if(!ŕ.Ͼ("cargocontainer",ƒ.ˮ,ñ))return false;}Ĕ++;ñ=false;}double Χ=ŕ
.ϖ(ref Ϊ,ref Ω,ñ);if(Double.IsNaN(Χ))return false;if(Τ){ß.Ƹ(Χ);return true;}if(ƒ.ˠ.Count>0){if(ƒ.ˠ[0].Ō.Length>0)Ψ=ƒ.ˠ[0]
.Ō;}ß.Ê((Ψ==""?İ.ǳ("C2"):Ψ)+" ");if(!ͺ&&!Ϋ){ß.Ƶ(ß.Ȑ(Ϊ)+"L / "+ß.Ȑ(Ω)+"L");ß.ƿ(Χ,1.0f,ß.Ʒ);ß.ǖ(' '+ß.ȇ(Χ)+"%");}else if(Ϋ)
{ß.Ƶ(ß.ȇ(Χ)+"%");ß.Ƹ(Χ);}else ß.Ƶ(ß.ȇ(Χ)+"%");return true;}}class έ:Ɠ{public έ(){ɏ=3;ɘ="CmdCharge";}π ŕ;bool ͺ=false;bool
Υ=false;bool Τ=false;bool Σ=false;public override void ɯ(){ŕ=new π(Ʃ,ß.Ŋ);if(ƒ.ˠ.Count>0)Φ=ƒ.ˠ[0].Ō;Τ=ƒ.Ͱ.EndsWith("bar")
;ͺ=ƒ.Ͱ.Contains("x");Υ=ƒ.Ͱ.Contains("time");Σ=ƒ.Ͱ.Contains("sum");}int Ĕ=0;int ć=0;double Ρ=0;double Π=0;TimeSpan Ο=
TimeSpan.Zero;string Φ="";Dictionary<long,double>Ě=new Dictionary<long,double>();Dictionary<long,double>ή=new Dictionary<long,
double>();Dictionary<long,double>μ=new Dictionary<long,double>();Dictionary<long,double>λ=new Dictionary<long,double>();
Dictionary<long,double>κ=new Dictionary<long,double>();double ι(long θ,double ʟ,double Ɗ){double η=0;double ζ=0;double ε=0;double
δ=0;if(ή.TryGetValue(θ,out ε)){δ=λ[θ];}if(Ě.TryGetValue(θ,out η)){ζ=μ[θ];}double γ=(Ʃ.Ȗ-ε);double β=0;if(γ>0)β=(ʟ-δ)/γ;if
(β<0){if(!κ.TryGetValue(θ,out β))β=0;}else κ[θ]=β;if(η>0){ή[θ]=Ě[θ];λ[θ]=μ[θ];}Ě[θ]=Ʃ.Ȗ;μ[θ]=ʟ;return(β>0?(Ɗ-ʟ)/β:0);}
private void α(string Ȁ,double ʚ,double ʟ,double Ɗ,TimeSpan ΰ){if(Τ){ß.Ƹ(ʚ);}else{ß.Ê(Ȁ+" ");if(Υ){ß.Ƶ(ß.ǉ.Ȝ(ΰ));if(!ͺ){ß.ƿ(ʚ,
1.0f,ß.Ʒ);ß.Ƶ(' '+ʚ.ToString("0.0")+"%");}}else{if(!ͺ){ß.Ƶ(ß.Ȑ(ʟ)+"Wh / "+ß.Ȑ(Ɗ)+"Wh");ß.ƿ(ʚ,1.0f,ß.Ʒ);}ß.Ƶ(' '+ʚ.ToString(
"0.0")+"%");}}}public override bool Ə(bool ñ){if(!ñ){ŕ.Ŭ();ć=0;Ĕ=0;Ρ=0;Π=0;Ο=TimeSpan.Zero;}if(Ĕ==0){if(!ŕ.Ͼ("jumpdrive",ƒ.ˮ,
ñ))return false;if(ŕ.Д()<=0){ß.ǖ("Charge: "+İ.ǳ("D2"));return true;}Ĕ++;ñ=false;}for(;ć<ŕ.Д();ć++){if(!Ʃ.ʊ(25))return
false;IMyJumpDrive Ņ=ŕ.φ[ć]as IMyJumpDrive;double ʟ,Ɗ,ʚ;ʚ=ß.Ŋ.Ϧ(Ņ,out ʟ,out Ɗ);TimeSpan ί;if(Υ)try{ί=TimeSpan.FromSeconds(ι(Ņ
.EntityId,ʟ,Ɗ));}catch{ί=new TimeSpan(-1);}else ί=TimeSpan.Zero;if(!Σ){α(Ņ.CustomName,ʚ,ʟ,Ɗ,ί);}else{Ρ+=ʟ;Π+=Ɗ;if(Ο<ί)Ο=ί
;}}if(Σ){double Ξ=(Π>0?Ρ/Π*100:0);α(Φ,Ξ,Ρ,Π,Ο);}return true;}}class Α:Ɠ{public Α(){ɏ=1;ɘ="CmdCountdown";}public override
bool Ə(bool ñ){bool ΐ=ƒ.Ͱ.EndsWith("c");bool Ώ=ƒ.Ͱ.EndsWith("r");string Ύ="";int Ό=ƒ.ˬ.IndexOf(' ');if(Ό>=0)Ύ=ƒ.ˬ.Substring(
Ό+1).Trim();DateTime Ί=DateTime.Now;DateTime Ή;if(!DateTime.TryParseExact(Ύ,"H:mm d.M.yyyy",System.Globalization.
CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None,out Ή)){ß.ǖ(İ.ǳ("C3"));ß.ǖ("  Countdown 19:02 28.2.2015");
return true;}TimeSpan Έ=Ή-Ί;string Ĳ="";if(Έ.Ticks<=0)Ĳ=İ.ǳ("C4");else{if((int)Έ.TotalDays>0)Ĳ+=(int)Έ.TotalDays+" "+İ.ǳ("C5")
+" ";if(Έ.Hours>0||Ĳ!="")Ĳ+=Έ.Hours+"h ";if(Έ.Minutes>0||Ĳ!="")Ĳ+=Έ.Minutes+"m ";Ĳ+=Έ.Seconds+"s";}if(ΐ)ß.ƶ(Ĳ);else if(Ώ)
ß.Ƶ(Ĳ);else ß.ǖ(Ĳ);return true;}}class Ά:Ɠ{public Ά(){ɏ=1;ɘ="CmdCustomData";}public override bool Ə(bool ñ){string Ĳ="";
if(ƒ.ˮ!=""&&ƒ.ˮ!="*"){IMyTerminalBlock ͼ=ß.Ǉ.GetBlockWithName(ƒ.ˮ)as IMyTerminalBlock;if(ͼ==null){ß.ǖ("CustomData: "+İ.ǳ(
"CD1")+ƒ.ˮ);return true;}Ĳ=ͼ.CustomData;}else{ß.ǖ("CustomData:"+İ.ǳ("CD2"));return true;}if(Ĳ.Length==0)return true;ß.Ǖ(Ĳ);
return true;}}class ͻ:Ɠ{public ͻ(){ɏ=5;ɘ="CmdDamage";}π ŕ;public override void ɯ(){ŕ=new π(Ʃ,ß.Ŋ);}bool Ɓ=false;int ć=0;public
override bool Ə(bool ñ){bool ͺ=ƒ.Ͱ.StartsWith("damagex");bool ͷ=ƒ.Ͱ.EndsWith("noc");bool ͽ=(!ͷ&&ƒ.Ͱ.EndsWith("c"));float Β=100;
if(!ñ){ŕ.Ŭ();Ɓ=false;ć=0;}if(!ŕ.χ(ƒ.ˮ,ñ))return false;if(ƒ.ˠ.Count>0){if(!float.TryParse(ƒ.ˠ[0].Ō,out Β))Β=100;}Β-=
0.00001f;for(;ć<ŕ.Д();ć++){if(!Ʃ.ʊ(30))return false;IMyTerminalBlock Ü=ŕ.φ[ć];IMySlimBlock Ν=Ü.CubeGrid.GetCubeBlock(Ü.Position)
;if(Ν==null)continue;float Λ=(ͷ?Ν.MaxIntegrity:Ν.BuildIntegrity);if(!ͽ)Λ-=Ν.CurrentDamage;float ʚ=100*(Λ/Ν.MaxIntegrity);
if(ʚ>=Β)continue;Ɓ=true;string Κ=ß.Ǧ(Ν.FatBlock.DisplayNameText,ß.ɞ*0.69f-ß.Ʒ);ß.Ê(Κ+' ');if(!ͺ){ß.Ʋ(ß.Ȑ(Λ)+" / ",0.69f);ß
.Ê(ß.Ȑ(Ν.MaxIntegrity));}ß.Ƶ(' '+ʚ.ToString("0.0")+'%');ß.Ƹ(ʚ);}if(!Ɓ)ß.ǖ(İ.ǳ("D3"));return true;}}class Ι:Ɠ{public Ι(){ɏ
=1;ɘ="CmdDateTime";}public override bool Ə(bool ñ){bool Θ=(ƒ.Ͱ.StartsWith("datetime"));bool Η=(ƒ.Ͱ.StartsWith("date"));
bool ΐ=ƒ.Ͱ.Contains("c");int Ζ=ƒ.Ͱ.IndexOf('+');if(Ζ<0)Ζ=ƒ.Ͱ.IndexOf('-');float Μ=0;if(Ζ>=0)float.TryParse(ƒ.Ͱ.Substring(Ζ),
out Μ);DateTime Έ=DateTime.Now.AddHours(Μ);string Ĳ="";int Ό=ƒ.ˬ.IndexOf(' ');if(Ό>=0)Ĳ=ƒ.ˬ.Substring(Ό+1);if(!Θ){if(!Η)Ĳ+=
Έ.ToShortTimeString();else Ĳ+=Έ.ToShortDateString();}else{if(Ĳ=="")Ĳ=String.Format("{0:d} {0:t}",Έ);else{Ĳ=Ĳ.Replace("/",
"\\/");Ĳ=Ĳ.Replace(":","\\:");Ĳ=Ĳ.Replace("\"","\\\"");Ĳ=Ĳ.Replace("'","\\'");Ĳ=Έ.ToString(Ĳ+' ');Ĳ=Ĳ.Substring(0,Ĳ.Length-1)
;}}if(ΐ)ß.ƶ(Ĳ);else ß.ǖ(Ĳ);return true;}}class Ε:Ɠ{public Ε(){ɏ=5;ɘ="CmdDetails";}string Δ="";string Γ="";int ř=0;π ŕ;
public override void ɯ(){ŕ=new π(Ʃ,ß.Ŋ);if(ƒ.ˠ.Count>0)Δ=ƒ.ˠ[0].Ō.Trim();if(ƒ.ˠ.Count>1){string Ō=ƒ.ˠ[1].Ō.Trim();if(!int.
TryParse(Ō,out ř)){ř=0;Γ=Ō;}}}int Ĕ=0;int ć=1;bool З=false;IMyTerminalBlock Ü;public override bool Ə(bool ñ){if(ƒ.ˮ==""||ƒ.ˮ==
"*"){ß.ǖ("Details: "+İ.ǳ("D1"));return true;}if(!ñ){ŕ.Ŭ();З=ƒ.Ͱ.Contains("non");Ĕ=0;ć=1;}if(Ĕ==0){if(!ŕ.χ(ƒ.ˮ,ñ))return
true;if(ŕ.Д()<=0){ß.ǖ("Details: "+İ.ǳ("D2"));return true;}Ĕ++;ñ=false;}int ү=(ƒ.Ͱ.EndsWith("x")?1:0);if(Ĕ==1){if(!ñ){Ü=ŕ.φ[0
];if(!З)ß.ǖ(Ü.CustomName);}if(!ҫ(Ü,ү,ř,ñ))return false;Ĕ++;ñ=false;}for(;ć<ŕ.Д();ć++){if(!ñ){Ü=ŕ.φ[ć];if(!З){ß.ǖ("");ß.ǖ(
Ü.CustomName);}}if(!ҫ(Ü,ү,ř,ñ))return false;ñ=false;}return true;}string[]ş;int Ү=0;int ҭ=0;bool Ҭ=false;ʐ ƴ=new ʐ();bool
ҫ(IMyTerminalBlock Ü,int Ҫ,int ķ,bool ñ){if(!ñ){ş=ƴ.Ŭ().ɱ(Ü.DetailedInfo).ɱ('\n').ɱ(Ü.CustomInfo).ɖ().Split('\n');Ү=Ҫ;Ҭ=(
Δ.Length==0);ҭ=0;}for(;Ү<ş.Length;Ү++){if(!Ʃ.ʊ(5))return false;if(ş[Ү].Length==0)continue;if(!Ҭ){if(!ş[Ү].Contains(Δ))
continue;Ҭ=true;}if(Γ.Length>0&&ş[Ү].Contains(Γ))return true;ß.ǖ(ƴ.Ŭ().ɱ("  ").ɱ(ş[Ү]));ҭ++;if(ķ>0&&ҭ>=ķ)return true;}return
true;}}class ҩ:Ɠ{public ҩ(){ɏ=1;ɘ="CmdDistance";}string Ҩ="";string[]ҧ;Vector3D Ҧ;string ҥ="";bool Ҥ=false;public override
void ɯ(){Ҥ=false;if(ƒ.ˠ.Count<=0)return;Ҩ=ƒ.ˠ[0].Ō.Trim();ҧ=Ҩ.Split(':');if(ҧ.Length<5||ҧ[0]!="GPS")return;double ң,Ң,ҡ;if(!
double.TryParse(ҧ[2],out ң))return;if(!double.TryParse(ҧ[3],out Ң))return;if(!double.TryParse(ҧ[4],out ҡ))return;Ҧ=new
Vector3D(ң,Ң,ҡ);ҥ=ҧ[1];Ҥ=true;}public override bool Ə(bool ñ){if(!Ҥ){ß.ǖ("Distance: "+İ.ǳ("DTU")+" '"+Ҩ+"'.");return true;}
IMyTerminalBlock Ü=Ĝ.ª.Ü;if(ƒ.ˮ!=""&&ƒ.ˮ!="*"){Ü=ß.Ǉ.GetBlockWithName(ƒ.ˮ);if(Ü==null){ß.ǖ("Distance: "+İ.ǳ("P1")+": "+ƒ.ˮ);return true;
}}double ѭ=Vector3D.Distance(Ü.GetPosition(),Ҧ);ß.Ê(ҥ+": ");ß.Ƶ(ß.Ȑ(ѭ)+"m ");return true;}}class Ұ:Ɠ{π ŕ;public Ұ(){ɏ=2;ɘ
="CmdDocked";}public override void ɯ(){ŕ=new π(Ʃ,ß.Ŋ);}int Ĕ=0;int ҹ=0;bool Ҹ=false;bool ҷ=false;IMyShipConnector Ÿ;
public override bool Ə(bool ñ){if(!ñ){if(ƒ.Ͱ.EndsWith("e"))Ҹ=true;if(ƒ.Ͱ.Contains("cn"))ҷ=true;ŕ.Ŭ();Ĕ=0;}if(Ĕ==0){if(!ŕ.Ͼ(
"connector",ƒ.ˮ,ñ))return false;Ĕ++;ҹ=0;ñ=false;}if(ŕ.Д()<=0){ß.ǖ("Docked: "+İ.ǳ("DO1"));return true;}for(;ҹ<ŕ.Д();ҹ++){Ÿ=ŕ.φ[ҹ]as
IMyShipConnector;if(Ÿ.Status==MyShipConnectorStatus.Connected){if(ҷ){ß.Ê(Ÿ.CustomName+":");ß.Ƶ(Ÿ.OtherConnector.CubeGrid.CustomName);}
else{ß.ǖ(Ÿ.OtherConnector.CubeGrid.CustomName);}}else{if(Ҹ){if(ҷ){ß.Ê(Ÿ.CustomName+":");ß.Ƶ("-");}else ß.ǖ("-");}}}return
true;}}class Ҷ:Ɠ{public Ҷ(){ɏ=30;ɘ="CmdEcho";}public override bool Ə(bool ñ){string ʘ=(ƒ.Ͱ=="center"?"c":(ƒ.Ͱ=="right"?"r":
"n"));switch(ʘ){case"c":ß.ƶ(ƒ.ˤ);break;case"r":ß.Ƶ(ƒ.ˤ);break;default:ß.ǖ(ƒ.ˤ);break;}return true;}}class ҵ:Ɠ{public ҵ(){ɏ=
1;ɘ="CmdGravity";}public override bool Ə(bool ñ){string ʘ=(ƒ.Ͱ.Contains("nat")?"n":(ƒ.Ͱ.Contains("art")?"a":(ƒ.Ͱ.Contains
("tot")?"t":"s")));Vector3D Ё;if(ß.ǈ.ʋ==null){ß.ǖ("Gravity: "+İ.ǳ("GNC"));return true;}switch(ʘ){case"n":ß.Ê(İ.ǳ("G2")+
" ");Ё=ß.ǈ.ʋ.GetNaturalGravity();ß.Ƶ(Ё.Length().ToString("F1")+" m/s²");break;case"a":ß.Ê(İ.ǳ("G3")+" ");Ё=ß.ǈ.ʋ.
GetArtificialGravity();ß.Ƶ(Ё.Length().ToString("F1")+" m/s²");break;case"t":ß.Ê(İ.ǳ("G1")+" ");Ё=ß.ǈ.ʋ.GetTotalGravity();ß.Ƶ(Ё.Length().
ToString("F1")+" m/s²");break;default:ß.Ê(İ.ǳ("GN"));ß.Ʋ(" | ",0.33f);ß.Ʋ(İ.ǳ("GA")+" | ",0.66f);ß.Ƶ(İ.ǳ("GT"),1.0f);ß.Ê("");Ё=ß
.ǈ.ʋ.GetNaturalGravity();ß.Ʋ(Ё.Length().ToString("F1")+" | ",0.33f);Ё=ß.ǈ.ʋ.GetArtificialGravity();ß.Ʋ(Ё.Length().
ToString("F1")+" | ",0.66f);Ё=ß.ǈ.ʋ.GetTotalGravity();ß.Ƶ(Ё.Length().ToString("F1")+" ");break;}return true;}}class Ҵ:Ɠ{public Ҵ
(){ɏ=0.5;ɘ="CmdHScroll";}ʐ ҳ=new ʐ();int Ҳ=1;public override bool Ə(bool ñ){if(ҳ.ʎ==0){string Ĳ=ƒ.ˤ+"  ";if(Ĳ.Length==0)
return true;float ұ=ß.ɞ;float Ư=ß.Ǩ(Ĳ,ß.Ǔ);float ѝ=ұ/Ư;if(ѝ>1)ҳ.ɱ(string.Join("",Enumerable.Repeat(Ĳ,(int)Math.Ceiling(ѝ))));
else ҳ.ɱ(Ĳ);if(Ĳ.Length>40)Ҳ=3;else if(Ĳ.Length>5)Ҳ=2;else Ҳ=1;ß.ǖ(ҳ);return true;}bool Ώ=ƒ.Ͱ.EndsWith("r");if(Ώ){ҳ.ƴ.Insert
(0,ҳ.ɖ(ҳ.ʎ-Ҳ,Ҳ));ҳ.ɗ(ҳ.ʎ-Ҳ,Ҳ);}else{ҳ.ɱ(ҳ.ɖ(0,Ҳ));ҳ.ɗ(0,Ҳ);}ß.ǖ(ҳ);return true;}}class ҁ:Ɠ{public ҁ(){ɏ=7;ɘ="CmdInvList";
}float Ҕ=-1;float ғ=-1;public override void ɯ(){ŕ=new π(Ʃ,ß.Ŋ);қ=new Ɲ(Ʃ,ß);}ʐ ƴ=new ʐ(100);Dictionary<string,string>Ғ=
new Dictionary<string,string>();void ґ(string ȭ,double ҏ,int Ð){if(Ð>0){if(!Һ)ß.ƿ(Math.Min(100,100*ҏ/Ð),0.3f);string Κ;if(Ғ
.ContainsKey(ȭ)){Κ=Ғ[ȭ];}else{if(!җ)Κ=ß.Ǧ(ȭ,ß.ɞ*0.5f-ҍ-ғ);else{if(!Һ)Κ=ß.Ǧ(ȭ,ß.ɞ*0.69f);else Κ=ß.Ǧ(ȭ,ß.ɞ*0.99f);}Ғ[ȭ]=Κ;}
ƴ.Ŭ();if(!Һ)ƴ.ɱ(' ');if(!җ){ß.Ê(ƴ.ɱ(Κ).ɱ(' '));ß.Ʋ(ß.Ȑ(ҏ),1.0f,ҍ+ғ);ß.ǖ(ƴ.Ŭ().ɱ(" / ").ɱ(ß.Ȑ(Ð)));}else{ß.ǖ(ƴ.ɱ(Κ));}}
else{if(!җ){ß.Ê(ƴ.Ŭ().ɱ(ȭ).ɱ(':'));ß.Ƶ(ß.Ȑ(ҏ),1.0f,Ҕ);}else ß.ǖ(ƴ.Ŭ().ɱ(ȭ));}}void ҕ(string ȭ,double ҏ,double Ҏ,int Ð){if(Ð>
0){if(!җ){ß.Ê(ƴ.Ŭ().ɱ(ȭ).ɱ(' '));ß.Ʋ(ß.Ȑ(ҏ),0.51f);ß.Ê(ƴ.Ŭ().ɱ(" / ").ɱ(ß.Ȑ(Ð)));ß.Ƶ(ƴ.Ŭ().ɱ(" +").ɱ(ß.Ȑ(Ҏ)).ɱ(" ").ɱ(İ.ǳ
("I1")),1.0f);}else ß.ǖ(ƴ.Ŭ().ɱ(ȭ));if(!Һ)ß.Ƹ(Math.Min(100,100*ҏ/Ð));}else{if(!җ){ß.Ê(ƴ.Ŭ().ɱ(ȭ).ɱ(':'));ß.Ʋ(ß.Ȑ(ҏ),0.51f
);ß.Ƶ(ƴ.Ŭ().ɱ(" +").ɱ(ß.Ȑ(Ҏ)).ɱ(" ").ɱ(İ.ǳ("I1")),1.0f);}else{ß.ǖ(ƴ.Ŭ().ɱ(ȭ));}}}float ҍ=0;bool Ҍ(ƌ Ź){int Ð=(ҙ?Ź.Ƌ:Ź.Ɗ);
if(Ð<0)return true;float ƻ=ß.Ǩ(ß.Ȑ(Ð),ß.Ǔ);if(ƻ>ҍ)ҍ=ƻ;return true;}List<ƌ>ҋ;int Ҋ=0;int Ґ=0;bool Җ(bool ñ,bool Ҡ,string Â,
string Ь){if(!ñ){Ґ=0;Ҋ=0;}if(Ґ==0){if(ӎ){if((ҋ=қ.ź(Â,ñ,Ҍ))==null)return false;}else{if((ҋ=қ.ź(Â,ñ))==null)return false;}Ґ++;ñ=
false;}if(ҋ.Count>0){if(!Ҡ&&!ñ){if(!ß.Ǜ)ß.ǖ();ß.ƶ(ƴ.Ŭ().ɱ("<< ").ɱ(Ь).ɱ(" ").ɱ(İ.ǳ("I2")).ɱ(" >>"));}for(;Ҋ<ҋ.Count;Ҋ++){if(!
Ʃ.ʊ(30))return false;double ҏ=ҋ[Ҋ].ƍ;if(ҙ&&ҏ>=ҋ[Ҋ].Ƌ)continue;int Ð=ҋ[Ҋ].Ɗ;if(ҙ)Ð=ҋ[Ҋ].Ƌ;string ȭ=ß.ǽ(ҋ[Ҋ].Ã,ҋ[Ҋ].Â);ґ(ȭ,
ҏ,Ð);}}return true;}List<ƌ>ҟ;int Ҟ=0;int ҝ=0;bool Ҝ(bool ñ){if(!ñ){Ҟ=0;ҝ=0;}if(ҝ==0){if((ҟ=қ.ź("Ingot",ñ))==null)return
false;ҝ++;ñ=false;}if(ҟ.Count>0){if(!Ҙ&&!ñ){if(!ß.Ǜ)ß.ǖ();ß.ƶ(ƴ.Ŭ().ɱ("<< ").ɱ(İ.ǳ("I4")).ɱ(" ").ɱ(İ.ǳ("I2")).ɱ(" >>"));}for(
;Ҟ<ҟ.Count;Ҟ++){if(!Ʃ.ʊ(40))return false;double ҏ=ҟ[Ҟ].ƍ;if(ҙ&&ҏ>=ҟ[Ҟ].Ƌ)continue;int Ð=ҟ[Ҟ].Ɗ;if(ҙ)Ð=ҟ[Ҟ].Ƌ;string ȭ=ß.ǽ
(ҟ[Ҟ].Ã,ҟ[Ҟ].Â);if(ҟ[Ҟ].Ã!="Scrap"){double Ҏ=қ.ž(ҟ[Ҟ].Ã+" Ore",ҟ[Ҟ].Ã,"Ore").ƍ;ҕ(ȭ,ҏ,Ҏ,Ð);}else ґ(ȭ,ҏ,Ð);}}return true;}π
ŕ=null;Ɲ қ;List<ˌ>ˠ;bool Қ,ͺ,ҙ,Ҙ,җ,Һ;int œ,Ő;string Ӑ="";float ӏ=0;bool ӎ=true;void Ӎ(){if(ß.Ǔ!=Ӑ||ӏ!=ß.ɞ){Ғ.Clear();ӏ=ß.
ɞ;}if(ß.Ǔ!=Ӑ){ғ=ß.Ǩ(" / ",ß.Ǔ);Ҕ=ß.ǻ(' ',ß.Ǔ);Ӑ=ß.Ǔ;}ŕ.Ŭ();Қ=ƒ.Ͱ.EndsWith("x")||ƒ.Ͱ.EndsWith("xs");ͺ=ƒ.Ͱ.EndsWith("s")||ƒ
.Ͱ.EndsWith("sx");ҙ=ƒ.Ͱ.StartsWith("missing");Ҙ=ƒ.Ͱ.Contains("list");Һ=ƒ.Ͱ.Contains("nb");җ=ƒ.Ͱ.Contains("nn");қ.Ŭ();ˠ=ƒ.
ˠ;if(ˠ.Count==0)ˠ.Add(new ˌ("all"));}bool ӌ(bool ñ){if(!ñ)œ=0;for(;œ<ˠ.Count;œ++){ˌ Ō=ˠ[œ];Ō.ʠ();string Â=Ō.ʣ;if(!ñ)Ő=0;
else ñ=false;for(;Ő<Ō.ʢ.Count;Ő++){if(!Ʃ.ʊ(30))return false;string[]Ą=Ō.ʢ[Ő].Split(':');double Ȏ;if(string.Compare(Ą[0],
"all",true)==0)Ą[0]="";int Ƌ=1;int Ɗ=-1;if(Ą.Length>1){if(Double.TryParse(Ą[1],out Ȏ)){if(ҙ)Ƌ=(int)Math.Ceiling(Ȏ);else Ɗ=(
int)Math.Ceiling(Ȏ);}}string Ơ=Ą[0];if(!string.IsNullOrEmpty(Â))Ơ+=' '+Â;қ.ơ(Ơ,Ō.ʭ=="-",Ƌ,Ɗ);}}return true;}int Ѷ=0;int ϐ=0
;int Ӌ=0;List<MyInventoryItem>Í=new List<MyInventoryItem>();bool ӊ(bool ñ){π Е=ŕ;if(!ñ)Ѷ=0;for(;Ѷ<Е.φ.Count;Ѷ++){if(!ñ)ϐ=
0;for(;ϐ<Е.φ[Ѷ].InventoryCount;ϐ++){IMyInventory Ϗ=Е.φ[Ѷ].GetInventory(ϐ);if(!ñ){Ӌ=0;Í.Clear();Ϗ.GetItems(Í);}else ñ=
false;for(;Ӌ<Í.Count;Ӌ++){if(!Ʃ.ʊ(40))return false;MyInventoryItem f=Í[Ӌ];string Å=ß.ǿ(f);string Ã,Â;ß.ȃ(Å,out Ã,out Â);if(
string.Compare(Â,"ore",true)==0){if(қ.Ƃ(Ã+" ingot",Ã,"Ingot")&&қ.Ƃ(Å,Ã,Â))continue;}else{if(қ.Ƃ(Å,Ã,Â))continue;}ß.ȃ(Å,out Ã,
out Â);ƌ Ž=қ.ž(Å,Ã,Â);Ž.ƍ+=(double)f.Amount;}}}return true;}int Ĕ=0;public override bool Ə(bool ñ){if(!ñ){Ӎ();Ĕ=0;}for(;Ĕ<=
13;Ĕ++){switch(Ĕ){case 0:if(!ŕ.χ(ƒ.ˮ,ñ))return false;break;case 1:if(!ӌ(ñ))return false;if(Қ)Ĕ++;break;case 2:if(!қ.Ǝ(ñ))
return false;break;case 3:if(!ӊ(ñ))return false;break;case 4:if(!Җ(ñ,Ҙ,"Ore",İ.ǳ("I3")))return false;break;case 5:if(ͺ){if(!Җ(
ñ,Ҙ,"Ingot",İ.ǳ("I4")))return false;}else{if(!Ҝ(ñ))return false;}break;case 6:if(!Җ(ñ,Ҙ,"Component",İ.ǳ("I5")))return
false;break;case 7:if(!Җ(ñ,Ҙ,"OxygenContainerObject",İ.ǳ("I6")))return false;break;case 8:if(!Җ(ñ,true,"GasContainerObject",
""))return false;break;case 9:if(!Җ(ñ,Ҙ,"AmmoMagazine",İ.ǳ("I7")))return false;break;case 10:if(!Җ(ñ,Ҙ,"PhysicalGunObject"
,İ.ǳ("I8")))return false;break;case 11:if(!Җ(ñ,true,"Datapad",""))return false;break;case 12:if(!Җ(ñ,true,
"ConsumableItem",""))return false;break;case 13:if(!Җ(ñ,true,"PhysicalObject",""))return false;break;}ñ=false;}ӎ=false;return true;}}
class Ӛ:Ɠ{public Ӛ(){ɏ=2;ɘ="CmdAmount";}π ŕ;public override void ɯ(){ŕ=new π(Ʃ,ß.Ŋ);}bool ә;bool Ә=false;int ő=0;int œ=0;int
Ő=0;public override bool Ə(bool ñ){if(!ñ){ә=!ƒ.Ͱ.EndsWith("x");Ә=ƒ.Ͱ.EndsWith("bar");if(Ә)ә=true;if(ƒ.ˠ.Count==0)ƒ.ˠ.Add(
new ˌ("reactor,gatlingturret,missileturret,interiorturret,gatlinggun,launcherreload,launcher,oxygenerator"));œ=0;}for(;œ<ƒ.
ˠ.Count;œ++){ˌ Ō=ƒ.ˠ[œ];if(!ñ){Ō.ʠ();ő=0;Ő=0;}for(;Ő<Ō.ʢ.Count;Ő++){if(ő==0){if(!ñ){if(Ō.ʢ[Ő]=="")continue;ŕ.Ŭ();}string
Ŏ=Ō.ʢ[Ő];if(!ŕ.Ͼ(Ŏ,ƒ.ˮ,ñ))return false;ő++;ñ=false;}if(!ӑ(ñ))return false;ñ=false;ő=0;}}return true;}int ӗ=0;int ħ=0;
double Ž=0;double Ӗ=0;double ӕ=0;int Ӌ=0;IMyTerminalBlock Ӕ;IMyInventory ӓ;List<MyInventoryItem>Í=new List<MyInventoryItem>();
string Ӓ="";bool ӑ(bool ñ){if(!ñ){ӗ=0;ħ=0;}for(;ӗ<ŕ.Д();ӗ++){if(ħ==0){if(!Ʃ.ʊ(50))return false;Ӕ=ŕ.φ[ӗ];ӓ=Ӕ.GetInventory(0);if
(ӓ==null)continue;ħ++;ñ=false;}if(!ñ){Í.Clear();ӓ.GetItems(Í);Ӓ=(Í.Count>0?Í[0].Type.ToString():"");Ӌ=0;Ž=0;Ӗ=0;ӕ=0;}for(
;Ӌ<Í.Count;Ӌ++){if(!Ʃ.ʊ(30))return false;MyInventoryItem f=Í[Ӌ];if(f.Type.ToString()!=Ӓ)ӕ+=(double)f.Amount;else Ž+=(
double)f.Amount;}string Ӊ=İ.ǳ("A1");string ƛ=Ӕ.CustomName;if(Ž>0&&(double)ӓ.CurrentVolume>0){double Ҿ=ӕ*(double)ӓ.
CurrentVolume/(Ž+ӕ);Ӗ=Math.Floor(Ž*((double)ӓ.MaxVolume-Ҿ)/((double)ӓ.CurrentVolume-Ҿ));Ӊ=ß.Ȑ(Ž)+" / "+(ӕ>0?"~":"")+ß.Ȑ(Ӗ);}if(!Ә||Ӗ
<=0){ƛ=ß.Ǧ(ƛ,ß.ɞ*0.8f);ß.Ê(ƛ);ß.Ƶ(Ӊ);}if(ә&&Ӗ>0){double ʚ=100*Ž/Ӗ;ß.Ƹ(ʚ);}ħ=0;ñ=false;}return true;}}class ҽ:Ɠ{π ŕ;public
ҽ(){ɏ=2;ɘ="CmdMass";}public override void ɯ(){ŕ=new π(Ʃ,ß.Ŋ);}bool ͺ=false;bool Ϋ=false;int Ĕ=0;public override bool Ə(
bool ñ){if(!ñ){ŕ.Ŭ();ͺ=(ƒ.Ͱ[ƒ.Ͱ.Length-1]=='x');Ϋ=(ƒ.Ͱ[ƒ.Ͱ.Length-1]=='p');Ĕ=0;}if(Ĕ==0){if(!ŕ.χ(ƒ.ˮ,ñ))return false;Ĕ++;ñ=
false;}double Æ=ŕ.ϑ(ñ);if(Double.IsNaN(Æ))return false;double ʛ=0;int ѣ=ƒ.ˠ.Count;if(ѣ>0){double.TryParse(ƒ.ˠ[0].Ō.Trim(),out
ʛ);if(ѣ>1){string Ѡ=ƒ.ˠ[1].Ō.Trim();char ќ=' ';if(Ѡ.Length>0)ќ=Char.ToLower(Ѡ[0]);int џ="kmgtpezy".IndexOf(ќ);if(џ>=0)ʛ*=
Math.Pow(1000.0,џ);}ʛ*=1000.0;}ß.Ê(İ.ǳ("M1")+" ");if(ʛ<=0){ß.Ƶ(ß.ȏ(Æ));return true;}double ʚ=Æ/ʛ*100;if(!ͺ&&!Ϋ){ß.Ƶ(ß.ȏ(Æ)+
" / "+ß.ȏ(ʛ));ß.ƿ(ʚ,1.0f,ß.Ʒ);ß.ǖ(' '+ß.ȇ(ʚ)+"%");}else if(Ϋ){ß.Ƶ(ß.ȇ(ʚ)+"%");ß.Ƹ(ʚ);}else ß.Ƶ(ß.ȇ(ʚ)+"%");return true;}}
class Ҽ:Ɠ{ȷ ǉ;π ŕ;public Ҽ(){ɏ=3;ɘ="CmdOxygen";}public override void ɯ(){ǉ=ß.ǉ;ŕ=new π(Ʃ,ß.Ŋ);}int Ĕ=0;int ć=0;bool Ɓ=false;
int һ=0;double Ƞ=0;double ȡ=0;double ƾ;public override bool Ə(bool ñ){if(!ñ){ŕ.Ŭ();Ĕ=0;ć=0;ƾ=0;}if(Ĕ==0){if(!ŕ.Ͼ("airvent",
ƒ.ˮ,ñ))return false;Ɓ=(ŕ.Д()>0);Ĕ++;ñ=false;}if(Ĕ==1){for(;ć<ŕ.Д();ć++){if(!Ʃ.ʊ(8))return false;IMyAirVent ň=ŕ.φ[ć]as
IMyAirVent;ƾ=Math.Max(ň.GetOxygenLevel()*100,0f);ß.Ê(ň.CustomName);if(ň.CanPressurize)ß.Ƶ(ß.ȇ(ƾ)+"%");else ß.Ƶ(İ.ǳ("O1"));ß.Ƹ(ƾ);}
Ĕ++;ñ=false;}if(Ĕ==2){if(!ñ)ŕ.Ŭ();if(!ŕ.Ͼ("oxyfarm",ƒ.ˮ,ñ))return false;һ=ŕ.Д();Ĕ++;ñ=false;}if(Ĕ==3){if(һ>0){if(!ñ)ć=0;
double ӈ=0;for(;ć<һ;ć++){if(!Ʃ.ʊ(4))return false;IMyOxygenFarm Ӈ=ŕ.φ[ć]as IMyOxygenFarm;ӈ+=Ӈ.GetOutput()*100;}ƾ=ӈ/һ;if(Ɓ)ß.ǖ(
"");Ɓ|=(һ>0);ß.Ê(İ.ǳ("O2"));ß.Ƶ(ß.ȇ(ƾ)+"%");ß.Ƹ(ƾ);}Ĕ++;ñ=false;}if(Ĕ==4){if(!ñ)ŕ.Ŭ();if(!ŕ.Ͼ("oxytank",ƒ.ˮ,ñ))return
false;һ=ŕ.Д();if(һ==0){if(!Ɓ)ß.ǖ(İ.ǳ("O3"));return true;}Ĕ++;ñ=false;}if(Ĕ==5){if(!ñ){Ƞ=0;ȡ=0;ć=0;}if(!ǉ.ȣ(ŕ.φ,"oxygen",ref ȡ
,ref Ƞ,ñ))return false;if(Ƞ==0){if(!Ɓ)ß.ǖ(İ.ǳ("O3"));return true;}ƾ=ȡ/Ƞ*100;if(Ɓ)ß.ǖ("");ß.Ê(İ.ǳ("O4"));ß.Ƶ(ß.ȇ(ƾ)+"%");ß
.Ƹ(ƾ);Ĕ++;}return true;}}class ӆ:Ɠ{public ӆ(){ɏ=1;ɘ="CmdPosition";}public override bool Ə(bool ñ){bool Ӆ=(ƒ.Ͱ=="posxyz");
bool Ҩ=(ƒ.Ͱ=="posgps");IMyTerminalBlock Ü=Ĝ.ª.Ü;if(ƒ.ˮ!=""&&ƒ.ˮ!="*"){Ü=ß.Ǉ.GetBlockWithName(ƒ.ˮ);if(Ü==null){ß.ǖ("Pos: "+İ.
ǳ("P1")+": "+ƒ.ˮ);return true;}}if(Ҩ){Vector3D ŀ=Ü.GetPosition();ß.ǖ("GPS:"+İ.ǳ("P2")+":"+ŀ.GetDim(0).ToString("F2")+":"+
ŀ.GetDim(1).ToString("F2")+":"+ŀ.GetDim(2).ToString("F2")+":");return true;}ß.Ê(İ.ǳ("P2")+": ");if(!Ӆ){ß.Ƶ(Ü.GetPosition(
).ToString("F0"));return true;}ß.ǖ("");ß.Ê(" X: ");ß.Ƶ(Ü.GetPosition().GetDim(0).ToString("F0"));ß.Ê(" Y: ");ß.Ƶ(Ü.
GetPosition().GetDim(1).ToString("F0"));ß.Ê(" Z: ");ß.Ƶ(Ü.GetPosition().GetDim(2).ToString("F0"));return true;}}class ӄ:Ɠ{public ӄ(
){ɏ=3;ɘ="CmdPower";}ȷ ǉ;π Ӄ;π ӂ;π Ӂ;π ѕ;π Ӏ;π ŕ;public override void ɯ(){Ӄ=new π(Ʃ,ß.Ŋ);ӂ=new π(Ʃ,ß.Ŋ);Ӂ=new π(Ʃ,ß.Ŋ);ѕ=
new π(Ʃ,ß.Ŋ);Ӏ=new π(Ʃ,ß.Ŋ);ŕ=new π(Ʃ,ß.Ŋ);ǉ=ß.ǉ;}string Ь;bool ҿ;string є;string Ψ;int ц;int Ĕ=0;public override bool Ə(
bool ñ){if(!ñ){Ь=(ƒ.Ͱ.EndsWith("x")?"s":(ƒ.Ͱ.EndsWith("p")?"p":(ƒ.Ͱ.EndsWith("v")?"v":(ƒ.Ͱ.EndsWith("bar")?"b":"n"))));ҿ=(ƒ.
Ͱ.StartsWith("powersummary"));є="a";Ψ="";if(ƒ.Ͱ.Contains("stored"))є="s";else if(ƒ.Ͱ.Contains("in"))є="i";else if(ƒ.Ͱ.
Contains("out"))є="o";Ĕ=0;Ӄ.Ŭ();ӂ.Ŭ();Ӂ.Ŭ();ѕ.Ŭ();Ӏ.Ŭ();}if(є=="a"){if(Ĕ==0){if(!Ӄ.Ͼ("reactor",ƒ.ˮ,ñ))return false;ñ=false;Ĕ++;}
if(Ĕ==1){if(!ӂ.Ͼ("hydrogenengine",ƒ.ˮ,ñ))return false;ñ=false;Ĕ++;}if(Ĕ==2){if(!Ӂ.Ͼ("solarpanel",ƒ.ˮ,ñ))return false;ñ=
false;Ĕ++;}if(Ĕ==3){if(!Ӏ.Ͼ("windturbine",ƒ.ˮ,ñ))return false;ñ=false;Ĕ++;}}else if(Ĕ==0)Ĕ=4;if(Ĕ==4){if(!ѕ.Ͼ("battery",ƒ.ˮ,ñ
))return false;ñ=false;Ĕ++;}int х=Ӄ.Д();int ф=ӂ.Д();int у=Ӂ.Д();int т=ѕ.Д();int с=Ӏ.Д();if(Ĕ==5){ц=0;if(х>0)ц++;if(ф>0)ц
++;if(у>0)ц++;if(с>0)ц++;if(т>0)ц++;if(ц<1){ß.ǖ(İ.ǳ("P6"));return true;}if(ƒ.ˠ.Count>0){if(ƒ.ˠ[0].Ō.Length>0)Ψ=ƒ.ˠ[0].Ō;}Ĕ
++;ñ=false;}if(є!="a"){if(!і(ѕ,(Ψ==""?İ.ǳ("P7"):Ψ),є,Ь,ñ))return false;return true;}string й=İ.ǳ("P8");if(!ҿ){if(Ĕ==6){if(
х>0)if(!л(Ӄ,(Ψ==""?İ.ǳ("P9"):Ψ),Ь,ñ))return false;Ĕ++;ñ=false;}if(Ĕ==7){if(ф>0)if(!л(ӂ,(Ψ==""?İ.ǳ("P12"):Ψ),Ь,ñ))return
false;Ĕ++;ñ=false;}if(Ĕ==8){if(у>0)if(!л(Ӂ,(Ψ==""?İ.ǳ("P10"):Ψ),Ь,ñ))return false;Ĕ++;ñ=false;}if(Ĕ==9){if(с>0)if(!л(Ӏ,(Ψ==""
?İ.ǳ("P13"):Ψ),Ь,ñ))return false;Ĕ++;ñ=false;}if(Ĕ==10){if(т>0)if(!і(ѕ,(Ψ==""?İ.ǳ("P7"):Ψ),є,Ь,ñ))return false;Ĕ++;ñ=
false;}}else{й=İ.ǳ("P11");ц=10;if(Ĕ==6)Ĕ=11;}if(ц==1)return true;if(!ñ){ŕ.Ŭ();ŕ.Ж(Ӄ);ŕ.Ж(ӂ);ŕ.Ж(Ӂ);ŕ.Ж(Ӏ);ŕ.Ж(ѕ);}if(!л(ŕ,й,Ь
,ñ))return false;return true;}void р(double ʟ,double Ɗ){double и=(Ɗ>0?ʟ/Ɗ*100:0);switch(Ь){case"s":ß.Ƶ(ƴ.Ŭ().ɱ(' ').ɱ(и.
ToString("F1")).ɱ("%"));break;case"v":ß.Ƶ(ƴ.Ŭ().ɱ(ß.Ȑ(ʟ)).ɱ("W / ").ɱ(ß.Ȑ(Ɗ)).ɱ("W"));break;case"c":ß.Ƶ(ƴ.Ŭ().ɱ(ß.Ȑ(ʟ)).ɱ("W"));
break;case"p":ß.Ƶ(ƴ.Ŭ().ɱ(' ').ɱ(и.ToString("F1")).ɱ("%"));ß.Ƹ(и);break;case"b":ß.Ƹ(и);break;default:ß.Ƶ(ƴ.Ŭ().ɱ(ß.Ȑ(ʟ)).ɱ(
"W / ").ɱ(ß.Ȑ(Ɗ)).ɱ("W"));ß.ƿ(и,1.0f,ß.Ʒ);ß.Ƶ(ƴ.Ŭ().ɱ(' ').ɱ(и.ToString("F1")).ɱ("%"));break;}}double п=0;double Н=0,н=0;int м
=0;bool л(π к,string й,string ʘ,bool ñ){if(!ñ){Н=0;н=0;м=0;}if(м==0){if(!ǉ.ɂ(к.φ,ǉ.ȸ,ref п,ref п,ref Н,ref н,ñ))return
false;м++;ñ=false;}if(!Ʃ.ʊ(50))return false;double и=(н>0?Н/н*100:0);ß.Ê(й+": ");р(Н*1000000,н*1000000);return true;}double з
=0,о=0,ж=0,ч=0;double љ=0,ј=0;int ї=0;ʐ ƴ=new ʐ(100);bool і(π ѕ,string й,string є,string ʘ,bool ñ){if(!ñ){з=о=0;ж=ч=0;љ=ј
=0;ї=0;}if(ї==0){if(!ǉ.ɇ(ѕ.φ,ref ж,ref ч,ref з,ref о,ref љ,ref ј,ñ))return false;ж*=1000000;ч*=1000000;з*=1000000;о*=
1000000;љ*=1000000;ј*=1000000;ї++;ñ=false;}double ѓ=(ј>0?љ/ј*100:0);double ђ=(о>0?з/о*100:0);double ё=(ч>0?ж/ч*100:0);bool ѐ=є
=="a";if(ї==1){if(!Ʃ.ʊ(50))return false;if(ѐ){if(ʘ!="p"){ß.Ê(ƴ.Ŭ().ɱ(й).ɱ(": "));ß.Ƶ(ƴ.Ŭ().ɱ("(IN ").ɱ(ß.Ȑ(ж)).ɱ(
"W / OUT ").ɱ(ß.Ȑ(з)).ɱ("W)"));}else ß.ǖ(ƴ.Ŭ().ɱ(й).ɱ(": "));ß.Ê(ƴ.Ŭ().ɱ("  ").ɱ(İ.ǳ("P3")).ɱ(": "));}else if(ʘ!="b")ß.Ê(ƴ.Ŭ().ɱ(й
).ɱ(": "));if(ѐ||є=="s")switch(ʘ){case"s":ß.Ƶ(ƴ.Ŭ().ɱ(' ').ɱ(ѓ.ToString("F1")).ɱ("%"));break;case"v":ß.Ƶ(ƴ.Ŭ().ɱ(ß.Ȑ(љ)).
ɱ("Wh / ").ɱ(ß.Ȑ(ј)).ɱ("Wh"));break;case"p":ß.Ƶ(ƴ.Ŭ().ɱ(' ').ɱ(ѓ.ToString("F1")).ɱ("%"));ß.Ƹ(ѓ);break;case"b":ß.Ƹ(ѓ);
break;default:ß.Ƶ(ƴ.Ŭ().ɱ(ß.Ȑ(љ)).ɱ("Wh / ").ɱ(ß.Ȑ(ј)).ɱ("Wh"));ß.ƿ(ѓ,1.0f,ß.Ʒ);ß.Ƶ(ƴ.Ŭ().ɱ(' ').ɱ(ѓ.ToString("F1")).ɱ("%"));
break;}if(є=="s")return true;ї++;ñ=false;}if(ї==2){if(!Ʃ.ʊ(50))return false;if(ѐ)ß.Ê(ƴ.Ŭ().ɱ("  ").ɱ(İ.ǳ("P4")).ɱ(": "));if(ѐ
||є=="o")switch(ʘ){case"s":ß.Ƶ(ƴ.Ŭ().ɱ(' ').ɱ(ђ.ToString("F1")).ɱ("%"));break;case"v":ß.Ƶ(ƴ.Ŭ().ɱ(ß.Ȑ(з)).ɱ("W / ").ɱ(ß.Ȑ(
о)).ɱ("W"));break;case"p":ß.Ƶ(ƴ.Ŭ().ɱ(' ').ɱ(ђ.ToString("F1")).ɱ("%"));ß.Ƹ(ђ);break;case"b":ß.Ƹ(ђ);break;default:ß.Ƶ(ƴ.Ŭ(
).ɱ(ß.Ȑ(з)).ɱ("W / ").ɱ(ß.Ȑ(о)).ɱ("W"));ß.ƿ(ђ,1.0f,ß.Ʒ);ß.Ƶ(ƴ.Ŭ().ɱ(' ').ɱ(ђ.ToString("F1")).ɱ("%"));break;}if(є=="o")
return true;ї++;ñ=false;}if(!Ʃ.ʊ(50))return false;if(ѐ)ß.Ê(ƴ.Ŭ().ɱ("  ").ɱ(İ.ǳ("P5")).ɱ(": "));if(ѐ||є=="i")switch(ʘ){case"s":
ß.Ƶ(ƴ.Ŭ().ɱ(' ').ɱ(ё.ToString("F1")).ɱ("%"));break;case"v":ß.Ƶ(ƴ.Ŭ().ɱ(ß.Ȑ(ж)).ɱ("W / ").ɱ(ß.Ȑ(ч)).ɱ("W"));break;case"p":
ß.Ƶ(ƴ.Ŭ().ɱ(' ').ɱ(ё.ToString("F1")).ɱ("%"));ß.Ƹ(ё);break;case"b":ß.Ƹ(ё);break;default:ß.Ƶ(ƴ.Ŭ().ɱ(ß.Ȑ(ж)).ɱ("W / ").ɱ(ß.
Ȑ(ч)).ɱ("W"));ß.ƿ(ё,1.0f,ß.Ʒ);ß.Ƶ(ƴ.Ŭ().ɱ(' ').ɱ(ё.ToString("F1")).ɱ("%"));break;}return true;}}class я:Ɠ{public я(){ɏ=7;
ɘ="CmdPowerTime";}class ю{public TimeSpan Ģ=new TimeSpan(-1);public double д=-1;public double э=0;}ю ь=new ю();π ы;π ъ;
public override void ɯ(){ы=new π(Ʃ,ß.Ŋ);ъ=new π(Ʃ,ß.Ŋ);}int щ=0;double ш=0;double е=0,Щ=0;double И=0,Ш=0,Ч=0;double Ц=0,Х=0;
int Ф=0;private bool У(string ˮ,out TimeSpan Т,out double ΰ,bool ñ){MyResourceSourceComponent Ȥ;MyResourceSinkComponent ȟ;
double Р=ɐ;ю П=ь;Т=П.Ģ;ΰ=П.д;if(!ñ){ы.Ŭ();ъ.Ŭ();П.д=0;щ=0;ш=0;е=Щ=0;И=0;Ш=Ч=0;Ц=Х=0;Ф=0;}if(щ==0){if(!ы.Ͼ("reactor",ˮ,ñ))
return false;ñ=false;щ++;}if(щ==1){for(;Ф<ы.φ.Count;Ф++){if(!Ʃ.ʊ(6))return false;IMyReactor Ü=ы.φ[Ф]as IMyReactor;if(Ü==null||
!Ü.IsWorking)continue;if(Ü.Components.TryGet<MyResourceSourceComponent>(out Ȥ)){е+=Ȥ.CurrentOutputByType(ß.ǉ.ȸ);Щ+=Ȥ.
MaxOutputByType(ß.ǉ.ȸ);}ш+=(double)Ü.GetInventory(0).CurrentMass;}ñ=false;щ++;}if(щ==2){if(!ъ.Ͼ("battery",ˮ,ñ))return false;ñ=false;щ++
;}if(щ==3){if(!ñ)Ф=0;for(;Ф<ъ.φ.Count;Ф++){if(!Ʃ.ʊ(15))return false;IMyBatteryBlock Ü=ъ.φ[Ф]as IMyBatteryBlock;if(Ü==null
||!Ü.IsWorking)continue;if(Ü.Components.TryGet<MyResourceSourceComponent>(out Ȥ)){Ш=Ȥ.CurrentOutputByType(ß.ǉ.ȸ);Ч=Ȥ.
MaxOutputByType(ß.ǉ.ȸ);}if(Ü.Components.TryGet<MyResourceSinkComponent>(out ȟ)){Ш-=ȟ.CurrentInputByType(ß.ǉ.ȸ);}double О=(Ш<0?(Ü.
MaxStoredPower-Ü.CurrentStoredPower)/(-Ш/3600):0);if(О>П.д)П.д=О;if(Ü.ChargeMode==ChargeMode.Recharge)continue;Ц+=Ш;Х+=Ч;И+=Ü.
CurrentStoredPower;}ñ=false;щ++;}double Н=е+Ц;if(Н<=0)П.Ģ=TimeSpan.FromSeconds(-1);else{double М=П.Ģ.TotalSeconds;double Л;double К=(П.э-ш
)/Р;if(е<=0)К=Math.Min(Н,Щ)/3600000;double Й=0;if(Х>0)Й=Math.Min(Н,Х)/3600;if(К<=0&&Й<=0)Л=-1;else if(К<=0)Л=И/Й;else if(
Й<=0)Л=ш/К;else{double С=Й;double Ъ=(е<=0?Н/3600:К*Н/е);Л=И/С+ш/Ъ;}if(М<=0||Л<0)М=Л;else М=(М+Л)/2;try{П.Ģ=TimeSpan.
FromSeconds(М);}catch{П.Ģ=TimeSpan.FromSeconds(-1);}}П.э=ш;ΰ=П.д;Т=П.Ģ;return true;}int Ĕ=0;bool Τ=false;bool ͺ=false;bool Ϋ=false;
double д=0;TimeSpan ț;int г=0,в=0,б=0;int Ǫ=0;int а=0;public override bool Ə(bool ñ){if(!ñ){Τ=ƒ.Ͱ.EndsWith("bar");ͺ=(ƒ.Ͱ[ƒ.Ͱ.
Length-1]=='x');Ϋ=(ƒ.Ͱ[ƒ.Ͱ.Length-1]=='p');Ĕ=0;г=в=б=Ǫ=0;а=0;д=0;}if(Ĕ==0){if(ƒ.ˠ.Count>0){for(;а<ƒ.ˠ.Count;а++){if(!Ʃ.ʊ(100))
return false;ƒ.ˠ[а].ʠ();if(ƒ.ˠ[а].ʢ.Count<=0)continue;string Ō=ƒ.ˠ[а].ʢ[0];int.TryParse(Ō,out Ǫ);if(а==0)г=Ǫ;else if(а==1)в=Ǫ;
else if(а==2)б=Ǫ;}}Ĕ++;ñ=false;}if(Ĕ==1){if(!У(ƒ.ˮ,out ț,out д,ñ))return false;Ĕ++;ñ=false;}if(!Ʃ.ʊ(30))return false;double
Ģ=0;TimeSpan Я;try{Я=new TimeSpan(г,в,б);}catch{Я=TimeSpan.FromSeconds(-1);}string Ĳ;if(ț.TotalSeconds>0||д<=0){if(!Τ)ß.Ê
(İ.ǳ("PT1")+" ");Ĳ=ß.ǉ.Ȝ(ț);Ģ=ț.TotalSeconds;}else{if(!Τ)ß.Ê(İ.ǳ("PT2")+" ");TimeSpan Ю;try{Ю=TimeSpan.FromSeconds(д);}
catch{Ю=new TimeSpan(-1);}Ĳ=ß.ǉ.Ȝ(Ю);if(Я.TotalSeconds>=д)Ģ=Я.TotalSeconds-д;else Ģ=0;}if(Я.Ticks<=0){ß.Ƶ(Ĳ);return true;}
double ʚ=Ģ/Я.TotalSeconds*100;if(ʚ>100)ʚ=100;if(Τ){ß.Ƹ(ʚ);return true;}if(!ͺ&&!Ϋ){ß.Ƶ(Ĳ);ß.ƿ(ʚ,1.0f,ß.Ʒ);ß.ǖ(' '+ʚ.ToString(
"0.0")+"%");}else if(Ϋ){ß.Ƶ(ʚ.ToString("0.0")+"%");ß.Ƹ(ʚ);}else ß.Ƶ(ʚ.ToString("0.0")+"%");return true;}}class Э:Ɠ{public Э()
{ɏ=7;ɘ="CmdPowerUsed";}ȷ ǉ;π ŕ;public override void ɯ(){ŕ=new π(Ʃ,ß.Ŋ);ǉ=ß.ǉ;}string Ь;string Ы;string ϩ;void р(double ʟ,
double Ɗ){double и=(Ɗ>0?ʟ/Ɗ*100:0);switch(Ь){case"s":ß.Ƶ(и.ToString("0.0")+"%",1.0f);break;case"v":ß.Ƶ(ß.Ȑ(ʟ)+"W / "+ß.Ȑ(Ɗ)+
"W",1.0f);break;case"c":ß.Ƶ(ß.Ȑ(ʟ)+"W",1.0f);break;case"p":ß.Ƶ(и.ToString("0.0")+"%",1.0f);ß.Ƹ(и);break;default:ß.Ƶ(ß.Ȑ(ʟ)+
"W / "+ß.Ȑ(Ɗ)+"W");ß.ƿ(и,1.0f,ß.Ʒ);ß.Ƶ(' '+и.ToString("0.0")+"%");break;}}double ɀ=0,ȿ=0;int Ѷ=0;int Ĕ=0;ѱ ѵ=new ѱ();public
override bool Ə(bool ñ){if(!ñ){Ь=(ƒ.Ͱ.EndsWith("x")?"s":(ƒ.Ͱ.EndsWith("usedp")||ƒ.Ͱ.EndsWith("topp")?"p":(ƒ.Ͱ.EndsWith("v")?"v":
(ƒ.Ͱ.EndsWith("c")?"c":"n"))));Ы=(ƒ.Ͱ.Contains("top")?"top":"");ϩ=(ƒ.ˠ.Count>0?ƒ.ˠ[0].Ō:İ.ǳ("PU1"));ɀ=ȿ=0;Ĕ=0;Ѷ=0;ŕ.Ŭ();ѵ
.Y();}if(Ĕ==0){if(!ŕ.χ(ƒ.ˮ,ñ))return false;ñ=false;Ĕ++;}MyResourceSinkComponent ȟ;MyResourceSourceComponent Ȥ;switch(Ы){
case"top":if(Ĕ==1){for(;Ѷ<ŕ.φ.Count;Ѷ++){if(!Ʃ.ʊ(20))return false;IMyTerminalBlock Ü=ŕ.φ[Ѷ];if(Ü.Components.TryGet<
MyResourceSinkComponent>(out ȟ)){ListReader<MyDefinitionId>ȝ=ȟ.AcceptedResources;if(ȝ.IndexOf(ǉ.ȸ)<0)continue;ɀ=ȟ.CurrentInputByType(ǉ.ȸ)*
1000000;}else continue;ѵ.o(ɀ,Ü);}ñ=false;Ĕ++;}if(ѵ.e()<=0){ß.ǖ("PowerUsedTop: "+İ.ǳ("D2"));return true;}int ķ=10;if(ƒ.ˠ.Count>0
)if(!int.TryParse(ϩ,out ķ)){ķ=10;}if(ķ>ѵ.e())ķ=ѵ.e();if(Ĕ==2){if(!ñ){Ѷ=ѵ.e()-1;ѵ.Ï();}for(;Ѷ>=ѵ.e()-ķ;Ѷ--){if(!Ʃ.ʊ(30))
return false;IMyTerminalBlock Ü=ѵ.Z(Ѷ);string ƛ=ß.Ǧ(Ü.CustomName,ß.ɞ*0.4f);if(Ü.Components.TryGet<MyResourceSinkComponent>(out
ȟ)){ɀ=ȟ.CurrentInputByType(ǉ.ȸ)*1000000;ȿ=ȟ.MaxRequiredInputByType(ǉ.ȸ)*1000000;var Ѳ=(Ü as IMyRadioAntenna);if(Ѳ!=null)ȿ
*=Ѳ.Radius/500;}ß.Ê(ƛ+" ");р(ɀ,ȿ);}}break;default:for(;Ѷ<ŕ.φ.Count;Ѷ++){if(!Ʃ.ʊ(10))return false;double Ѵ;IMyTerminalBlock
Ü=ŕ.φ[Ѷ];if(Ü.Components.TryGet<MyResourceSinkComponent>(out ȟ)){ListReader<MyDefinitionId>ȝ=ȟ.AcceptedResources;if(ȝ.
IndexOf(ǉ.ȸ)<0)continue;Ѵ=ȟ.CurrentInputByType(ǉ.ȸ);double ѳ=ȟ.MaxRequiredInputByType(ǉ.ȸ);var Ѳ=(Ü as IMyRadioAntenna);if(Ѳ!=
null){ѳ*=Ѳ.Radius/500;}ȿ+=ѳ;}else continue;if(Ü.Components.TryGet<MyResourceSourceComponent>(out Ȥ)&&(Ü as IMyBatteryBlock!=
null)){Ѵ-=Ȥ.CurrentOutputByType(ǉ.ȸ);if(Ѵ<=0)continue;}ɀ+=Ѵ;}ß.Ê(ϩ);р(ɀ*1000000,ȿ*1000000);break;}return true;}public class
ѱ{List<KeyValuePair<double,IMyTerminalBlock>>Ѱ=new List<KeyValuePair<double,IMyTerminalBlock>>();public void o(double ѯ,
IMyTerminalBlock Ü){Ѱ.Add(new KeyValuePair<double,IMyTerminalBlock>(ѯ,Ü));}public int e(){return Ѱ.Count;}public IMyTerminalBlock Z(int
X){return Ѱ[X].Value;}public void Y(){Ѱ.Clear();}public void Ï(){Ѱ.Sort((ϰ,Ҁ)=>(ϰ.Key.CompareTo(Ҁ.Key)));}}}class ѿ:Ɠ{π ŕ
;public ѿ(){ɏ=1;ɘ="CmdProp";}public override void ɯ(){ŕ=new π(Ʃ,ß.Ŋ);}int Ĕ=0;int Ѷ=0;bool Ѿ=false;string ѽ=null;string Ѽ
=null;string ѻ=null;string Ѻ=null;public override bool Ə(bool ñ){if(!ñ){Ѿ=ƒ.Ͱ.StartsWith("props");ѽ=Ѽ=ѻ=Ѻ=null;Ѷ=0;Ĕ=0;}
if(ƒ.ˠ.Count<1){ß.ǖ(ƒ.Ͱ+": "+"Missing property name.");return true;}if(Ĕ==0){if(!ñ)ŕ.Ŭ();if(!ŕ.χ(ƒ.ˮ,ñ))return false;ѹ();Ĕ
++;ñ=false;}if(Ĕ==1){int ķ=ŕ.Д();if(ķ==0){ß.ǖ(ƒ.Ͱ+": "+"No blocks found.");return true;}for(;Ѷ<ķ;Ѷ++){if(!Ʃ.ʊ(50))return
false;IMyTerminalBlock Ü=ŕ.φ[Ѷ];if(Ü.GetProperty(ѽ)!=null){if(Ѽ==null){string ϩ=ß.Ǧ(Ü.CustomName,ß.ɞ*0.7f);ß.Ê(ϩ);}else ß.Ê(Ѽ
);ß.Ƶ(Ѹ(Ü,ѽ,ѻ,Ѻ));if(!Ѿ)return true;}}}return true;}void ѹ(){ѽ=ƒ.ˠ[0].Ō;if(ƒ.ˠ.Count>1){if(!Ѿ)Ѽ=ƒ.ˠ[1].Ō;else ѻ=ƒ.ˠ[1].Ō;
if(ƒ.ˠ.Count>2){if(!Ѿ)ѻ=ƒ.ˠ[2].Ō;else Ѻ=ƒ.ˠ[2].Ō;if(ƒ.ˠ.Count>3&&!Ѿ)Ѻ=ƒ.ˠ[3].Ō;}}}string Ѹ(IMyTerminalBlock Ü,string ѷ,
string Ѯ=null,string Ѥ=null){return(Ü.GetValue<bool>(ѷ)?(Ѯ!=null?Ѯ:İ.ǳ("W9")):(Ѥ!=null?Ѥ:İ.ǳ("W1")));}}class њ:Ɠ{public њ(){ɏ=
5;ɘ="CmdShipCtrl";}π ŕ;public override void ɯ(){ŕ=new π(Ʃ,ß.Ŋ);}public override bool Ə(bool ñ){if(!ñ)ŕ.Ŭ();if(!ŕ.Ͼ(
"shipctrl",ƒ.ˮ,ñ))return false;if(ŕ.Д()<=0){if(ƒ.ˮ!=""&&ƒ.ˮ!="*")ß.ǖ(ƒ.Ͱ+": "+İ.ǳ("SC1")+" ("+ƒ.ˮ+")");else ß.ǖ(ƒ.Ͱ+": "+İ.ǳ("SC1"
));return true;}if(ƒ.Ͱ.StartsWith("damp")){bool б=(ŕ.φ[0]as IMyShipController).DampenersOverride;ß.Ê(İ.ǳ("SCD"));ß.Ƶ(б?
"ON":"OFF");}else{bool б=(ŕ.φ[0]as IMyShipController).IsUnderControl;ß.Ê(İ.ǳ("SCO"));ß.Ƶ(б?"YES":"NO");}return true;}}class
Ѣ:Ɠ{public Ѣ(){ɏ=1;ɘ="CmdShipMass";}public override bool Ə(bool ñ){bool ѡ=ƒ.Ͱ.EndsWith("base");double ʛ=0;if(ƒ.ˮ!="")
double.TryParse(ƒ.ˮ.Trim(),out ʛ);int ѣ=ƒ.ˠ.Count;if(ѣ>0){string Ѡ=ƒ.ˠ[0].Ō.Trim();char ќ=' ';if(Ѡ.Length>0)ќ=Char.ToLower(Ѡ[0
]);int џ="kmgtpezy".IndexOf(ќ);if(џ>=0)ʛ*=Math.Pow(1000.0,џ);}double ɹ=(ѡ?ß.ǈ.ɷ:ß.ǈ.ɸ);if(!ѡ)ß.Ê(İ.ǳ("SM1")+" ");else ß.Ê
(İ.ǳ("SM2")+" ");ß.Ƶ(ß.ȏ(ɹ,true,'k')+" ");if(ʛ>0)ß.Ƹ(ɹ/ʛ*100);return true;}}class ў:Ɠ{public ў(){ɏ=0.5;ɘ="CmdSpeed";}
public override bool Ə(bool ñ){double ʛ=0;double ѝ=1;string ќ="m/s";if(ƒ.Ͱ.Contains("kmh")){ѝ=3.6;ќ="km/h";}else if(ƒ.Ͱ.
Contains("mph")){ѝ=2.23694;ќ="mph";}if(ƒ.ˮ!="")double.TryParse(ƒ.ˮ.Trim(),out ʛ);ß.Ê(İ.ǳ("S1")+" ");ß.Ƶ((ß.ǈ.ʁ*ѝ).ToString("F1")
+" "+ќ+" ");if(ʛ>0)ß.Ƹ(ß.ǈ.ʁ/ʛ*100);return true;}}class ћ:Ɠ{public ћ(){ɏ=1;ɘ="CmdStopTask";}public override bool Ə(bool ñ
){double Ѩ=0;if(ƒ.Ͱ.Contains("best"))Ѩ=ß.ǈ.ʁ/ß.ǈ.ɾ;else Ѩ=ß.ǈ.ʁ/ß.ǈ.ɺ;double ѭ=ß.ǈ.ʁ/2*Ѩ;if(ƒ.Ͱ.Contains("time")){ß.Ê(İ.ǳ
("ST"));if(double.IsNaN(Ѩ)){ß.Ƶ("N/A");return true;}string Ĳ="";try{TimeSpan Έ=TimeSpan.FromSeconds(Ѩ);if((int)Έ.
TotalDays>0)Ĳ=" > 24h";else{if(Έ.Hours>0)Ĳ=Έ.Hours+"h ";if(Έ.Minutes>0||Ĳ!="")Ĳ+=Έ.Minutes+"m ";Ĳ+=Έ.Seconds+"s";}}catch{Ĳ="N/A";
}ß.Ƶ(Ĳ);return true;}ß.Ê(İ.ǳ("SD"));if(!double.IsNaN(ѭ)&&!double.IsInfinity(ѭ))ß.Ƶ(ß.Ȑ(ѭ)+"m ");else ß.Ƶ("N/A");return
true;}}class ѫ:Ɠ{ȷ ǉ;π ŕ;public ѫ(){ɏ=2;ɘ="CmdTanks";}public override void ɯ(){ǉ=ß.ǉ;ŕ=new π(Ʃ,ß.Ŋ);}int Ĕ=0;char Ь='n';
string Ѫ;double ѩ=0;double Ѭ=0;double ƾ;bool Τ=false;public override bool Ə(bool ñ){List<ˌ>ˠ=ƒ.ˠ;if(ˠ.Count==0){ß.ǖ(İ.ǳ("T4"))
;return true;}if(!ñ){Ь=(ƒ.Ͱ.EndsWith("x")?'s':(ƒ.Ͱ.EndsWith("p")?'p':(ƒ.Ͱ.EndsWith("v")?'v':'n')));Τ=ƒ.Ͱ.EndsWith("bar");
Ĕ=0;if(Ѫ==null){Ѫ=ˠ[0].Ō.Trim();Ѫ=char.ToUpper(Ѫ[0])+Ѫ.Substring(1).ToLower();}ŕ.Ŭ();ѩ=0;Ѭ=0;}if(Ĕ==0){if(!ŕ.Ͼ("oxytank",
ƒ.ˮ,ñ))return false;ñ=false;Ĕ++;}if(Ĕ==1){if(!ŕ.Ͼ("hydrogenengine",ƒ.ˮ,ñ))return false;ñ=false;Ĕ++;}if(Ĕ==2){if(!ǉ.ȣ(ŕ.φ,
Ѫ,ref ѩ,ref Ѭ,ñ))return false;ñ=false;Ĕ++;}if(Ѭ==0){ß.ǖ(String.Format(İ.ǳ("T5"),Ѫ));return true;}ƾ=ѩ/Ѭ*100;if(Τ){ß.Ƹ(ƾ);
return true;}ß.Ê(Ѫ);switch(Ь){case's':ß.Ƶ(' '+ß.ȇ(ƾ)+"%");break;case'v':ß.Ƶ(ß.Ȑ(ѩ)+"L / "+ß.Ȑ(Ѭ)+"L");break;case'p':ß.Ƶ(' '+ß.
ȇ(ƾ)+"%");ß.Ƹ(ƾ);break;default:ß.Ƶ(ß.Ȑ(ѩ)+"L / "+ß.Ȑ(Ѭ)+"L");ß.ƿ(ƾ,1.0f,ß.Ʒ);ß.Ƶ(' '+ƾ.ToString("0.0")+"%");break;}return
true;}}class ѧ{ɬ ß=null;public string W="Debug";public float Ѧ=1.0f;public List<ʐ>ş=new List<ʐ>();public int Ű=0;public
float ѥ=0;public ѧ(ɬ A){ß=A;ş.Add(new ʐ());}public void ŧ(string Ĳ){ş[Ű].ɱ(Ĳ);}public void ŧ(ʐ Ŧ){ş[Ű].ɱ(Ŧ);}public void ť(){
ş.Add(new ʐ());Ű++;ѥ=0;}public void ť(string Ť){ş[Ű].ɱ(Ť);ť();}public void ţ(List<ʐ>Ţ){if(ş[Ű].ʎ==0)ş.RemoveAt(Ű);else Ű
++;ş.AddList(Ţ);Ű+=Ţ.Count-1;ť();}public List<ʐ>ś(){if(ş[Ű].ʎ==0)return ş.GetRange(0,Ű);else return ş;}public void š(
string Š,string F=""){string[]ş=Š.Split('\n');for(int n=0;n<ş.Length;n++)ť(F+ş[n]);}public void Ş(){ş.Clear();ť();Ű=0;}public
int ŝ(){return Ű+(ş[Ű].ʎ>0?1:0);}public string Ŝ(){return String.Join("\n",ş);}public void ś(List<ʐ>Ś,int Ļ,int ř){int Ř=Ļ+
ř;int Ľ=ŝ();if(Ř>Ľ)Ř=Ľ;for(int n=Ļ;n<Ř;n++)Ś.Add(ş[n]);}}class ŗ{ɬ ß=null;public float ŷ=1.0f;public int ŵ=17;public int
Ŵ=0;int ų=1;int Ų=1;public List<ѧ>ű=new List<ѧ>();public int Ű=0;public ŗ(ɬ A){ß=A;}public void ů(int ķ){Ų=ķ;}public void
Ů(){ŵ=(int)Math.Floor(ɬ.ɩ*ŷ*Ų/ɬ.ɧ);}public void ŭ(ѧ Ĳ){ű.Add(Ĳ);}public void Ŭ(){ű.Clear();}public int ŝ(){int ķ=0;
foreach(var Ĳ in ű){ķ+=Ĳ.ŝ();}return ķ;}ʐ ū=new ʐ(256);public ʐ Ŝ(){ū.Ŭ();int ķ=ű.Count;for(int n=0;n<ķ-1;n++){ū.ɱ(ű[n].Ŝ());ū.
ɱ("\n");}if(ķ>0)ū.ɱ(ű[ķ-1].Ŝ());return ū;}List<ʐ>Ū=new List<ʐ>(20);public ʐ ũ(int Ũ=0){ū.Ŭ();Ū.Clear();if(Ų<=0)return ū;
int Ŗ=ű.Count;int ł=0;int ı=(ŵ/Ų);int Ł=(Ũ*ı);int ŀ=Ŵ+Ł;int Ŀ=ŀ+ı;bool ľ=false;for(int n=0;n<Ŗ;n++){ѧ Ĳ=ű[n];int Ľ=Ĳ.ŝ();
int ļ=ł;ł+=Ľ;if(!ľ&&ł>ŀ){int Ļ=ŀ-ļ;if(ł>=Ŀ){Ĳ.ś(Ū,Ļ,Ŀ-ļ-Ļ);break;}ľ=true;Ĳ.ś(Ū,Ļ,Ľ);continue;}if(ľ){if(ł>=Ŀ){Ĳ.ś(Ū,0,Ŀ-ļ);
break;}Ĳ.ś(Ū,0,Ľ);}}int ķ=Ū.Count;for(int n=0;n<ķ-1;n++){ū.ɱ(Ū[n]);ū.ɱ("\n");}if(ķ>0)ū.ɱ(Ū[ķ-1]);return ū;}public bool ĺ(int
ķ=-1){if(ķ<=0)ķ=ß.ɥ;if(Ŵ-ķ<=0){Ŵ=0;return true;}Ŵ-=ķ;return false;}public bool ĸ(int ķ=-1){if(ķ<=0)ķ=ß.ɥ;int Ķ=ŝ();if(Ŵ+ķ
+ŵ>=Ķ){Ŵ=Math.Max(Ķ-ŵ,0);return true;}Ŵ+=ķ;return false;}public int ĵ=0;public void Ĵ(){if(ĵ>0){ĵ--;return;}if(ŝ()<=ŵ){Ŵ=
0;ų=1;return;}if(ų>0){if(ĸ()){ų=-1;ĵ=2;}}else{if(ĺ()){ų=1;ĵ=2;}}}}class ĳ:Ɠ{public ĳ(){ɏ=1;ɘ="CmdTextLCD";}public
override bool Ə(bool ñ){string Ĳ="";if(ƒ.ˮ!=""&&ƒ.ˮ!="*"){IMyTextPanel Ĺ=ß.Ǉ.GetBlockWithName(ƒ.ˮ)as IMyTextPanel;if(Ĺ==null){ß.
ǖ("TextLCD: "+İ.ǳ("T1")+ƒ.ˮ);return true;}Ĳ=Ĺ.GetText();}else{ß.ǖ("TextLCD:"+İ.ǳ("T2"));return true;}if(Ĳ.Length==0)
return true;ß.Ǖ(Ĳ);return true;}}class Ń:Ɠ{public Ń(){ɏ=5;ɘ="CmdWorking";}π ŕ;public override void ɯ(){ŕ=new π(Ʃ,ß.Ŋ);}int Ĕ=0
;int œ=0;bool Œ;public override bool Ə(bool ñ){if(!ñ){Ĕ=0;Œ=(ƒ.Ͱ=="workingx");œ=0;}if(ƒ.ˠ.Count==0){if(Ĕ==0){if(!ñ)ŕ.Ŭ();
if(!ŕ.χ(ƒ.ˮ,ñ))return false;Ĕ++;ñ=false;}if(!Ɨ(ŕ,Œ,"",ñ))return false;return true;}for(;œ<ƒ.ˠ.Count;œ++){ˌ Ō=ƒ.ˠ[œ];if(!ñ)
Ō.ʠ();if(!Ŕ(Ō,ñ))return false;ñ=false;}return true;}int ő=0;int Ő=0;string[]ŏ;string Ŏ;string ō;bool Ŕ(ˌ Ō,bool ñ){if(!ñ)
{ő=0;Ő=0;}for(;Ő<Ō.ʢ.Count;Ő++){if(ő==0){if(!ñ){if(string.IsNullOrEmpty(Ō.ʢ[Ő]))continue;ŕ.Ŭ();ŏ=Ō.ʢ[Ő].Split(':');Ŏ=ŏ[0]
;ō=(ŏ.Length>1?ŏ[1]:"");}if(!string.IsNullOrEmpty(Ŏ)){if(!ŕ.Ͼ(Ŏ,ƒ.ˮ,ñ))return false;}else{if(!ŕ.χ(ƒ.ˮ,ñ))return false;}ő
++;ñ=false;}if(!Ɨ(ŕ,Œ,ō,ñ))return false;ő=0;ñ=false;}return true;}string ŋ(IMyTerminalBlock Ü){Г Ŋ=ß.Ŋ;if(!Ü.IsWorking)
return İ.ǳ("W1");IMyProductionBlock ŉ=Ü as IMyProductionBlock;if(ŉ!=null)if(ŉ.IsProducing)return İ.ǳ("W2");else return İ.ǳ(
"W3");IMyAirVent ň=Ü as IMyAirVent;if(ň!=null){if(ň.CanPressurize)return(ň.GetOxygenLevel()*100).ToString("F1")+"%";else
return İ.ǳ("W4");}IMyGasTank Ň=Ü as IMyGasTank;if(Ň!=null)return(Ň.FilledRatio*100).ToString("F1")+"%";IMyBatteryBlock ņ=Ü as
IMyBatteryBlock;if(ņ!=null)return Ŋ.ϱ(ņ);IMyJumpDrive Ņ=Ü as IMyJumpDrive;if(Ņ!=null)return Ŋ.ϥ(Ņ).ToString("0.0")+"%";IMyLandingGear ń
=Ü as IMyLandingGear;if(ń!=null){switch((int)ń.LockMode){case 0:return İ.ǳ("W8");case 1:return İ.ǳ("W10");case 2:return İ
.ǳ("W7");}}IMyDoor Ŷ=Ü as IMyDoor;if(Ŷ!=null){if(Ŷ.Status==DoorStatus.Open)return İ.ǳ("W5");return İ.ǳ("W6");}
IMyShipConnector Ÿ=Ü as IMyShipConnector;if(Ÿ!=null){if(Ÿ.Status==MyShipConnectorStatus.Unconnected)return İ.ǳ("W8");if(Ÿ.Status==
MyShipConnectorStatus.Connected)return İ.ǳ("W7");else return İ.ǳ("W10");}IMyLaserAntenna ƪ=Ü as IMyLaserAntenna;if(ƪ!=null)return Ŋ.ϧ(ƪ);
IMyRadioAntenna Ɯ=Ü as IMyRadioAntenna;if(Ɯ!=null)return ß.Ȑ(Ɯ.Radius)+"m";IMyBeacon ƚ=Ü as IMyBeacon;if(ƚ!=null)return ß.Ȑ(ƚ.Radius)+
"m";IMyThrust ƙ=Ü as IMyThrust;if(ƙ!=null&&ƙ.ThrustOverride>0)return ß.Ȑ(ƙ.ThrustOverride)+"N";return İ.ǳ("W9");}int Ƙ=0;
bool Ɨ(π Ā,bool Ɩ,string ƕ,bool ñ){if(!ñ)Ƙ=0;for(;Ƙ<Ā.Д();Ƙ++){if(!Ʃ.ʊ(20))return false;IMyTerminalBlock Ü=Ā.φ[Ƙ];string Ɣ=(
Ɩ?(Ü.IsWorking?İ.ǳ("W9"):İ.ǳ("W1")):ŋ(Ü));if(!string.IsNullOrEmpty(ƕ)&&String.Compare(Ɣ,ƕ,true)!=0)continue;if(Ɩ)Ɣ=ŋ(Ü);
string ƛ=Ü.CustomName;ƛ=ß.Ǧ(ƛ,ß.ɞ*0.7f);ß.Ê(ƛ);ß.Ƶ(Ɣ);}return true;}}class Ɠ:ɔ{public ѧ Ĳ=null;protected ͱ ƒ;protected ɬ ß;
protected đ Ĝ;protected ǫ İ;public Ɠ(){ɏ=3600;ɘ="CommandTask";}public void Ƒ(đ ě,ͱ Ɛ){Ĝ=ě;ß=Ĝ.ß;ƒ=Ɛ;İ=ß.İ;}public virtual bool Ə(
bool ñ){ß.ǖ(İ.ǳ("UC")+": '"+ƒ.ˬ+"'");return true;}public override bool ɮ(bool ñ){Ĳ=ß.Ǚ(Ĳ,Ĝ.ª);if(!ñ)ß.Ş();return Ə(ñ);}}
class Ɲ{Dictionary<string,string>ƫ=new Dictionary<string,string>(StringComparer.InvariantCultureIgnoreCase){{"ingot","ingot"}
,{"ore","ore"},{"component","component"},{"tool","physicalgunobject"},{"ammo","ammomagazine"},{"oxygen",
"oxygencontainerobject"},{"gas","gascontainerobject"}};Ț Ʃ;ɬ ß;Ƅ ƨ;Ƅ Ƨ;Ƅ Ʀ;Î ƥ;bool Ƥ;public Ƅ ƣ;public Ɲ(Ț Ƣ,ɬ A,int L=20){ƨ=new Ƅ();Ƨ=new Ƅ()
;Ʀ=new Ƅ();Ƥ=false;ƣ=new Ƅ();Ʃ=Ƣ;ß=A;ƥ=ß.ƥ;}public void Ŭ(){Ʀ.Y();Ƨ.Y();ƨ.Y();Ƥ=false;ƣ.Y();}public void ơ(string Ơ,bool
Ɖ=false,int Ƌ=1,int Ɗ=-1){if(string.IsNullOrEmpty(Ơ)){Ƥ=true;return;}string[]Ɵ=Ơ.Split(' ');string Â="";ƌ Ź=new ƌ(Ɖ,Ƌ,Ɗ);
if(Ɵ.Length==2){if(!ƫ.TryGetValue(Ɵ[1],out Â))Â=Ɵ[1];}string Ã=Ɵ[0];if(ƫ.TryGetValue(Ã,out Ź.Â)){Ƨ.o(Ź.Â,Ź);return;}ß.Ǽ(
ref Ã,ref Â);if(string.IsNullOrEmpty(Â)){Ź.Ã=Ã;ƨ.o(Ź.Ã,Ź);return;}Ź.Ã=Ã;Ź.Â=Â;Ʀ.o(Ã+' '+Â,Ź);}public ƌ ƞ(string Å,string Ã,
string Â){ƌ Ź;Ź=Ʀ.b(Å);if(Ź!=null)return Ź;Ź=ƨ.b(Ã);if(Ź!=null)return Ź;Ź=Ƨ.b(Â);if(Ź!=null)return Ź;return null;}public bool
Ƃ(string Å,string Ã,string Â){ƌ Ź;bool Ɓ=false;Ź=Ƨ.b(Â);if(Ź!=null){if(Ź.Ɖ)return true;Ɓ=true;}Ź=ƨ.b(Ã);if(Ź!=null){if(Ź.
Ɖ)return true;Ɓ=true;}Ź=Ʀ.b(Å);if(Ź!=null){if(Ź.Ɖ)return true;Ɓ=true;}return!(Ƥ||Ɓ);}public ƌ ƀ(string Å,string Ã,string
Â){ƌ Ž=new ƌ();ƌ Ź=ƞ(Å,Ã,Â);if(Ź!=null){Ž.Ƌ=Ź.Ƌ;Ž.Ɗ=Ź.Ɗ;}Ž.Ã=Ã;Ž.Â=Â;ƣ.o(Å,Ž);return Ž;}public ƌ ž(string Å,string Ã,
string Â){ƌ Ž=ƣ.b(Å);if(Ž==null)Ž=ƀ(Å,Ã,Â);return Ž;}int ż=0;List<ƌ>Ż;public List<ƌ>ź(string Â,bool ñ,Func<ƌ,bool>ſ=null){if(!
ñ){Ż=new List<ƌ>();ż=0;}for(;ż<ƣ.e();ż++){if(!Ʃ.ʊ(5))return null;ƌ Ź=ƣ.Z(ż);if(Ƃ(Ź.Ã+' '+Ź.Â,Ź.Ã,Ź.Â))continue;if((string
.Compare(Ź.Â,Â,true)==0)&&(ſ==null||ſ(Ź)))Ż.Add(Ź);}return Ż;}int ƈ=0;public bool Ǝ(bool ñ){if(!ñ){ƈ=0;}for(;ƈ<ƥ.r.Count;
ƈ++){if(!Ʃ.ʊ(10))return false;Á f=ƥ.Í[ƥ.r[ƈ]];if(!f.Æ)continue;string Å=f.Ò+' '+f.ê;if(Ƃ(Å,f.Ò,f.ê))continue;ƌ Ž=ž(Å,f.Ò,
f.ê);if(Ž.Ɗ==-1)Ž.Ɗ=f.é;}return true;}}class ƌ{public int Ƌ;public int Ɗ;public string Ã="";public string Â="";public
bool Ɖ;public double ƍ;public ƌ(bool Ƈ=false,int Ɔ=1,int ƅ=-1){Ƌ=Ɔ;Ɖ=Ƈ;Ɗ=ƅ;}}class Ƅ{Dictionary<string,ƌ>ƃ=new Dictionary<
string,ƌ>(StringComparer.InvariantCultureIgnoreCase);List<string>r=new List<string>();public void o(string a,ƌ f){if(!ƃ.
ContainsKey(a)){r.Add(a);ƃ.Add(a,f);}}public int e(){return ƃ.Count;}public ƌ b(string a){if(ƃ.ContainsKey(a))return ƃ[a];return
null;}public ƌ Z(int X){return ƃ[r[X]];}public void Y(){r.Clear();ƃ.Clear();}public void Ï(){r.Sort();}}class Î{public
Dictionary<string,Á>Í=new Dictionary<string,Á>(StringComparer.InvariantCultureIgnoreCase);Dictionary<string,Á>Ì=new Dictionary<
string,Á>(StringComparer.InvariantCultureIgnoreCase);public List<string>r=new List<string>();public Dictionary<string,Á>Ë=new
Dictionary<string,Á>(StringComparer.InvariantCultureIgnoreCase);public void Ê(string Ã,string Â,int Ð,string É,string È,string Ç,
bool Æ){if(Â=="Ammo")Â="AmmoMagazine";else if(Â=="Tool")Â="PhysicalGunObject";string Å=Ã+' '+Â;Á f=new Á(Ã,Â,Ð,É,È,Æ);Í.Add(
Å,f);if(!Ì.ContainsKey(Ã))Ì.Add(Ã,f);if(È!="")Ë.Add(È,f);if(Ç!="")Ë.Add(Ç,f);r.Add(Å);}public Á Ä(string Ã="",string Â=""
){if(Í.ContainsKey(Ã+" "+Â))return Í[Ã+" "+Â];if(string.IsNullOrEmpty(Â)){Á f=null;Ì.TryGetValue(Ã,out f);return f;}if(
string.IsNullOrEmpty(Ã))for(int n=0;n<Í.Count;n++){Á f=Í[r[n]];if(string.Compare(Â,f.ê,true)==0)return f;}return null;}}class
Á{public string Ò;public string ê;public int é;public string è;public string ç;public bool Æ;public Á(string æ,string å,
int ä=0,string ã="",string â="",bool á=true){Ò=æ;ê=å;é=ä;è=ã;ç=â;Æ=á;}}class à{ɬ ß=null;public z Þ=new z();public ŗ Ý;
public IMyTerminalBlock Ü;public IMyTextSurface Û;public int Ú=0;public int Ù=0;public string Ø="";public string Ö="";public
bool Õ=true;public IMyTextSurface Ô=>(À?Û:Ü as IMyTextSurface);public int Ó=>(À?(ß.ǚ(Ü)?0:1):Þ.e());public bool À=false;
public à(ɬ A,string U){ß=A;Ö=U;}public à(ɬ A,string U,IMyTerminalBlock S,IMyTextSurface B,int R){ß=A;Ö=U;Ü=S;Û=B;Ú=R;À=true;}
public bool Q(){return Ý.ŝ()>Ý.ŵ||Ý.Ŵ!=0;}float P=1.0f;bool O=false;public float V(){if(O)return P;O=true;return P;}float N=
1.0f;bool K=false;public float J(){if(K)return N;K=true;return N;}bool I=false;public void H(){if(I)return;if(!À){Þ.Ï();Ü=Þ.
Z(0);}int G=Ü.CustomName.IndexOf("!MARGIN:");if(G<0||G+8>=Ü.CustomName.Length){Ù=1;Ø=" ";}else{string F=Ü.CustomName.
Substring(G+8);int E=F.IndexOf(" ");if(E>=0)F=F.Substring(0,E);if(!int.TryParse(F,out Ù))Ù=1;Ø=new String(' ',Ù);}if(Ü.CustomName
.Contains("!NOSCROLL"))Õ=false;else Õ=true;I=true;}public void D(ŗ C=null){if(Ý==null||Ü==null)return;if(C==null)C=Ý;if(!
À){IMyTextSurface B=Ü as IMyTextSurface;if(B!=null){float L=B.FontSize;string W=B.Font;for(int n=0;n<Þ.e();n++){
IMyTextSurface ª=Þ.Z(n)as IMyTextSurface;if(ª==null)continue;ª.Alignment=VRage.Game.GUI.TextPanel.TextAlignment.LEFT;ª.FontSize=L;ª.
Font=W;string º=C.ũ(n).ɖ();if(!ß.Ǒ.SKIP_CONTENT_TYPE)ª.ContentType=VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;ª.
WriteText(º);}}}else{Û.Alignment=VRage.Game.GUI.TextPanel.TextAlignment.LEFT;if(!ß.Ǒ.SKIP_CONTENT_TYPE)Û.ContentType=VRage.Game.
GUI.TextPanel.ContentType.TEXT_AND_IMAGE;Û.WriteText(C.ũ().ɖ());}I=false;}public void µ(){if(Ü==null)return;if(À){Û.
WriteText("");return;}IMyTextSurface B=Ü as IMyTextSurface;if(B==null)return;for(int n=0;n<Þ.e();n++){IMyTextSurface ª=Þ.Z(n)as
IMyTextSurface;if(ª==null)continue;ª.WriteText("");}}}class z{Dictionary<string,IMyTerminalBlock>w=new Dictionary<string,
IMyTerminalBlock>();Dictionary<IMyTerminalBlock,string>v=new Dictionary<IMyTerminalBlock,string>();List<string>r=new List<string>();
public void o(string a,IMyTerminalBlock f){if(!r.Contains(a)){r.Add(a);w.Add(a,f);v.Add(f,a);}}public void k(string a){if(r.
Contains(a)){r.Remove(a);v.Remove(w[a]);w.Remove(a);}}public void j(IMyTerminalBlock f){if(v.ContainsKey(f)){r.Remove(v[f]);w.
Remove(v[f]);v.Remove(f);}}public int e(){return w.Count;}public IMyTerminalBlock b(string a){if(r.Contains(a))return w[a];
return null;}public IMyTerminalBlock Z(int X){return w[r[X]];}public void Y(){r.Clear();w.Clear();v.Clear();}public void Ï(){r
.Sort();}}class ë:ɔ{public ɬ ß;public à í;đ Ĝ;public ë(đ ě){Ĝ=ě;ß=Ĝ.ß;í=Ĝ.ª;ɏ=0.5;ɘ="PanelDisplay";}double Ě=0;public
void ę(){Ě=0;}int Ę=0;int ė=0;bool Ė=true;double ĕ=double.MaxValue;int Ĕ=0;public override bool ɮ(bool ñ){Ɠ ē;if(!ñ&&(Ĝ.ċ==
false||Ĝ.č==null||Ĝ.č.Count<=0))return true;if(Ĝ.Č.ü>3)return ɒ(0);if(!ñ){ė=0;Ė=false;ĕ=double.MaxValue;Ĕ=0;}if(Ĕ==0){while(ė
<Ĝ.č.Count){if(!Ʃ.ʊ(5))return false;if(Ĝ.Ď.TryGetValue(Ĝ.č[ė],out ē)){if(!ē.Ɍ)return ɒ(ē.ɓ-Ʃ.Ȗ+0.001);if(ē.ɑ>Ě)Ė=true;if(
ē.ɓ<ĕ)ĕ=ē.ɓ;}ė++;}Ĕ++;ñ=false;}double Ē=ĕ-Ʃ.Ȗ+0.001;if(!Ė&&!í.Q())return ɒ(Ē);ß.Ǘ(í.Ý,í);if(Ė){if(!ñ){Ě=Ʃ.Ȗ;í.Ý.Ŭ();Ę=0;}
while(Ę<Ĝ.č.Count){if(!Ʃ.ʊ(7))return false;if(!Ĝ.Ď.TryGetValue(Ĝ.č[Ę],out ē)){í.Ý.ű.Add(ß.Ǚ(null,í));ß.Ş();ß.ǖ(
"ERR: No cmd task ("+Ĝ.č[Ę]+")");Ę++;continue;}í.Ý.ŭ(ē.Ĳ);Ę++;}}ß.Ȃ(í);Ĝ.Č.ü++;if(ɏ<Ē&&!í.Q())return ɒ(Ē);return true;}}class đ:ɔ{public ɬ ß
;public à ª;public ë Đ=null;string ď="N/A";public Dictionary<string,Ɠ>Ď=new Dictionary<string,Ɠ>();public List<string>č=
null;public ì Č;public bool ċ{get{return Č.ö;}}public đ(ì Ċ,à ĝ){ɏ=5;ª=ĝ;Č=Ċ;ß=Ċ.ß;ɘ="PanelProcess";}ǫ İ;public override
void ɯ(){İ=ß.İ;}ͱ į=null;Ɠ Į(string ĭ,bool ñ){if(!ñ)į=new ͱ(Ʃ);if(!į.ʠ(ĭ,ñ))return null;Ɠ Ģ=į.ˡ();Ģ.Ƒ(this,į);Ʃ.ȴ(Ģ,0);
return Ģ;}string Ĭ="";void ī(){try{Ĭ=ª.Ü.Ǳ(ª.Ú,ß.ɦ);}catch{Ĭ="";return;}Ĭ=Ĭ?.Replace("\\\n","");}int Ę=0;int Ī=0;List<string>ĩ
=null;HashSet<string>Ĩ=new HashSet<string>();int ħ=0;bool Ħ(bool ñ){if(!ñ){char[]ĥ={';','\n'};string Ĥ=Ĭ.Replace("\\;",
"\f");if(Ĥ.StartsWith("@")){int ģ=Ĥ.IndexOf("\n");if(ģ<0){Ĥ="";}else{Ĥ=Ĥ.Substring(ģ+1);}}ĩ=new List<string>(Ĥ.Split(ĥ,
StringSplitOptions.RemoveEmptyEntries));Ĩ.Clear();Ę=0;Ī=0;ħ=0;}while(Ę<ĩ.Count){if(!Ʃ.ʊ(500))return false;if(ĩ[Ę].StartsWith("//")){ĩ.
RemoveAt(Ę);continue;}ĩ[Ę]=ĩ[Ę].Replace('\f',';');if(!Ď.ContainsKey(ĩ[Ę])){if(ħ!=1)ñ=false;ħ=1;Ɠ ē=Į(ĩ[Ę],ñ);if(ē==null)return
false;ñ=false;Ď.Add(ĩ[Ę],ē);ħ=0;}if(!Ĩ.Contains(ĩ[Ę]))Ĩ.Add(ĩ[Ę]);Ę++;}if(č!=null){Ɠ Ģ;while(Ī<č.Count){if(!Ʃ.ʊ(7))return
false;if(!Ĩ.Contains(č[Ī]))if(Ď.TryGetValue(č[Ī],out Ģ)){Ģ.ɰ();Ď.Remove(č[Ī]);}Ī++;}}č=ĩ;return true;}public override void ɭ(
){if(č!=null){Ɠ Ģ;for(int ġ=0;ġ<č.Count;ġ++){if(Ď.TryGetValue(č[ġ],out Ģ))Ģ.ɰ();}č=null;}if(Đ!=null){Đ.ɰ();Đ=null;}else{}
}ŗ Ġ=null;string ğ="";bool Ğ=false;public override bool ɮ(bool ñ){if(ª.Ó<=0){ɰ();return true;}if(!ñ){ª.Ý=ß.Ǘ(ª.Ý,ª);Ġ=ß.Ǘ
(Ġ,ª);ī();if(Ĭ==null){if(ª.À){Č.î(ª.Û,ª.Ü as IMyTextPanel);}else{ɰ();}return true;}if(ª.Ü.CustomName!=ğ){Ğ=true;}else{Ğ=
false;}ğ=ª.Ü.CustomName;}if(Ĭ!=ď){if(!Ħ(ñ))return false;if(Ĭ==""){ď="";if(Č.ö){if(Ġ.ű.Count<=0)Ġ.ű.Add(ß.Ǚ(null,ª));else ß.Ǚ(
Ġ.ű[0],ª);ß.Ş();ß.ǖ(İ.ǳ("H1"));bool þ=ª.Õ;ª.Õ=false;ß.Ȃ(ª,Ġ);ª.Õ=þ;return true;}return this.ɒ(2);}Ğ=true;}ď=Ĭ;if(Đ!=null
&&Ğ){Ʃ.ȱ(Đ);Đ.ę();Ʃ.ȴ(Đ,0);}else if(Đ==null){Đ=new ë(this);Ʃ.ȴ(Đ,0);}return true;}}class ì:ɔ{const string ý="T:!LCD!";
public int ü=0;public ɬ ß;public ȹ Þ=new ȹ();π û;π ú;Dictionary<à,đ>ù=new Dictionary<à,đ>();public Dictionary<IMyTextSurface,à
>ø=new Dictionary<IMyTextSurface,à>();public bool ö=false;ϲ õ=null;public ì(ɬ A){ɏ=5;ß=A;ɘ="ProcessPanels";}public
override void ɯ(){û=new π(Ʃ,ß.Ŋ);ú=new π(Ʃ,ß.Ŋ);õ=new ϲ(ß,this);}int ó=0;bool ò(bool ñ){if(!ñ)ó=0;if(ó==0){if(!û.χ(ß.ɦ,ñ))return
false;ó++;ñ=false;}if(ó==1){if(ß.ɦ=="T:[LCD]"&&ý!="")if(!û.χ(ý,ñ))return false;ó++;ñ=false;}return true;}string ð(
IMyTerminalBlock Ü){int ï=Ü.CustomName.IndexOf("!LINK:");if(ï>=0&&Ü.CustomName.Length>ï+6){return Ü.CustomName.Substring(ï+6)+' '+Ü.
Position.ToString();}return Ü.EntityId.ToString();}public void î(IMyTextSurface B,IMyTextPanel ª){à í;if(B==null)return;if(!ø.
TryGetValue(B,out í))return;if(ª!=null){í.Þ.j(ª);}ø.Remove(B);if(í.Ó<=0||í.À){đ ô;if(ù.TryGetValue(í,out ô)){Þ.j(í.Ö);ù.Remove(í);ô
.ɰ();}}}void ÿ(IMyTerminalBlock Ü){IMyTextSurfaceProvider ą=Ü as IMyTextSurfaceProvider;IMyTextSurface B=Ü as
IMyTextSurface;if(B!=null){î(B,Ü as IMyTextPanel);return;}if(ą==null)return;for(int n=0;n<ą.SurfaceCount;n++){B=ą.GetSurface(n);î(B,
null);}}string U;string ĉ;bool Ĉ;int ć=0;int Ć=0;public override bool ɮ(bool ñ){if(!ñ){û.Ŭ();ć=0;Ć=0;}if(!ò(ñ))return false;
while(ć<û.Д()){if(!Ʃ.ʊ(20))return false;IMyTerminalBlock Ü=(û.φ[ć]as IMyTerminalBlock);if(Ü==null||!Ü.IsWorking){û.φ.RemoveAt
(ć);continue;}IMyTextSurfaceProvider ą=Ü as IMyTextSurfaceProvider;IMyTextSurface B=Ü as IMyTextSurface;IMyTextPanel ª=Ü
as IMyTextPanel;à í;U=ð(Ü);string[]Ą=U.Split(' ');ĉ=Ą[0];Ĉ=Ą.Length>1;if(ª!=null){if(ø.ContainsKey(B)){í=ø[B];if(í.Ö==U+
"@0"||(Ĉ&&í.Ö==ĉ)){ć++;continue;}ÿ(Ü);}if(!Ĉ){í=new à(ß,U+"@0",Ü,B,0);đ ô=new đ(this,í);Ʃ.ȴ(ô,0);ù.Add(í,ô);Þ.o(í.Ö,í);ø.Add
(B,í);ć++;continue;}í=Þ.b(ĉ);if(í==null){í=new à(ß,ĉ);Þ.o(ĉ,í);đ ô=new đ(this,í);Ʃ.ȴ(ô,0);ù.Add(í,ô);}í.Þ.o(U,Ü);ø.Add(B,
í);}else{if(ą==null){ć++;continue;}for(int n=0;n<ą.SurfaceCount;n++){B=ą.GetSurface(n);if(ø.ContainsKey(B)){í=ø[B];if(í.Ö
==U+'@'+n.ToString()){continue;}î(B,null);}if(Ü.Ǳ(n,ß.ɦ)==null)continue;í=new à(ß,U+"@"+n.ToString(),Ü,B,n);đ ô=new đ(this
,í);Ʃ.ȴ(ô,0);ù.Add(í,ô);Þ.o(í.Ö,í);ø.Add(B,í);}}ć++;}while(Ć<ú.Д()){if(!Ʃ.ʊ(300))return false;IMyTerminalBlock Ü=ú.φ[Ć];
if(Ü==null)continue;if(!û.φ.Contains(Ü)){ÿ(Ü);}Ć++;}ú.Ŭ();ú.Ж(û);if(!õ.ɍ&&õ.ϵ())Ʃ.ȴ(õ,0);return true;}public bool ă(string
Ă){if(string.Compare(Ă,"clear",true)==0){õ.ϻ();if(!õ.ɍ)Ʃ.ȴ(õ,0);return true;}if(string.Compare(Ă,"boot",true)==0){õ.ϼ=0;
if(!õ.ɍ)Ʃ.ȴ(õ,0);return true;}if(Ă.Ǹ("scroll")){τ ā=new τ(ß,this,Ă);Ʃ.ȴ(ā,0);return true;}if(string.Compare(Ă,"props",true
)==0){Г Ñ=ß.Ŋ;List<IMyTerminalBlock>Ā=new List<IMyTerminalBlock>();List<ITerminalAction>Ƭ=new List<ITerminalAction>();
List<ITerminalProperty>Ȼ=new List<ITerminalProperty>();IMyTextPanel Ĺ=Ʃ.Ǒ.GridTerminalSystem.GetBlockWithName("DEBUG")as
IMyTextPanel;if(Ĺ==null){return true;}Ĺ.WriteText("Properties: ");foreach(var f in Ñ.Џ){Ĺ.WriteText(f.Key+" =============="+"\n",
true);f.Value(Ā,null);if(Ā.Count<=0){Ĺ.WriteText("No blocks\n",true);continue;}Ā[0].GetProperties(Ȼ,(í)=>{return í.Id!=
"Name"&&í.Id!="OnOff"&&!í.Id.StartsWith("Show");});foreach(var Ⱥ in Ȼ){Ĺ.WriteText("P "+Ⱥ.Id+" "+Ⱥ.TypeName+"\n",true);}Ȼ.
Clear();Ā.Clear();}}return false;}}class ȹ{Dictionary<string,à>ƃ=new Dictionary<string,à>();List<string>r=new List<string>();
public void o(string a,à f){if(!ƃ.ContainsKey(a)){r.Add(a);ƃ.Add(a,f);}}public int e(){return ƃ.Count;}public à b(string a){if
(ƃ.ContainsKey(a))return ƃ[a];return null;}public à Z(int X){return ƃ[r[X]];}public void j(string a){ƃ.Remove(a);r.Remove
(a);}public void Y(){r.Clear();ƃ.Clear();}public void Ï(){r.Sort();}}class ȷ{Ț Ʃ;ɬ ß;public MyDefinitionId ȸ=new
MyDefinitionId(typeof(VRage.Game.ObjectBuilders.Definitions.MyObjectBuilder_GasProperties),"Electricity");public MyDefinitionId ȶ=new
MyDefinitionId(typeof(VRage.Game.ObjectBuilders.Definitions.MyObjectBuilder_GasProperties),"Oxygen");public MyDefinitionId ȼ=new
MyDefinitionId(typeof(VRage.Game.ObjectBuilders.Definitions.MyObjectBuilder_GasProperties),"Hydrogen");public ȷ(Ț Ƣ,ɬ A){Ʃ=Ƣ;ß=A;}int
Ɉ=0;public bool ɇ(List<IMyTerminalBlock>Ā,ref double ɀ,ref double ȿ,ref double Ⱦ,ref double Ƚ,ref double Ɇ,ref double Ʌ,
bool ñ){if(!ñ)Ɉ=0;MyResourceSinkComponent ȟ;MyResourceSourceComponent Ȥ;for(;Ɉ<Ā.Count;Ɉ++){if(!Ʃ.ʊ(8))return false;if(Ā[Ɉ].
Components.TryGet<MyResourceSinkComponent>(out ȟ)){ɀ+=ȟ.CurrentInputByType(ȸ);ȿ+=ȟ.MaxRequiredInputByType(ȸ);}if(Ā[Ɉ].Components.
TryGet<MyResourceSourceComponent>(out Ȥ)){Ⱦ+=Ȥ.CurrentOutputByType(ȸ);Ƚ+=Ȥ.MaxOutputByType(ȸ);}IMyBatteryBlock Ʉ=(Ā[Ɉ]as
IMyBatteryBlock);Ɇ+=Ʉ.CurrentStoredPower;Ʌ+=Ʉ.MaxStoredPower;}return true;}int Ƀ=0;public bool ɂ(List<IMyTerminalBlock>Ā,MyDefinitionId
Ɂ,ref double ɀ,ref double ȿ,ref double Ⱦ,ref double Ƚ,bool ñ){if(!ñ)Ƀ=0;MyResourceSinkComponent ȟ;
MyResourceSourceComponent Ȥ;for(;Ƀ<Ā.Count;Ƀ++){if(!Ʃ.ʊ(6))return false;if(Ā[Ƀ].Components.TryGet<MyResourceSinkComponent>(out ȟ)){ɀ+=ȟ.
CurrentInputByType(Ɂ);ȿ+=ȟ.MaxRequiredInputByType(Ɂ);}if(Ā[Ƀ].Components.TryGet<MyResourceSourceComponent>(out Ȥ)){Ⱦ+=Ȥ.
CurrentOutputByType(Ɂ);Ƚ+=Ȥ.MaxOutputByType(Ɂ);}}return true;}int Ȓ=0;public bool ȣ(List<IMyTerminalBlock>Ā,string Ȣ,ref double ȡ,ref
double Ƞ,bool ñ){if(!ñ){Ȓ=0;Ƞ=0;ȡ=0;}MyResourceSinkComponent ȟ;for(;Ȓ<Ā.Count;Ȓ++){if(!Ʃ.ʊ(30))return false;IMyGasTank Ň=Ā[Ȓ]
as IMyGasTank;if(Ň==null)continue;double Ȟ=0;if(Ň.Components.TryGet<MyResourceSinkComponent>(out ȟ)){ListReader<
MyDefinitionId>ȝ=ȟ.AcceptedResources;int n=0;for(;n<ȝ.Count;n++){if(string.Compare(ȝ[n].SubtypeId.ToString(),Ȣ,true)==0){Ȟ=Ň.Capacity;
Ƞ+=Ȟ;ȡ+=Ȟ*Ň.FilledRatio;break;}}}}return true;}public string Ȝ(TimeSpan ț){string Ĳ="";if(ț.Ticks<=0)return"-";if((int)ț.
TotalDays>0)Ĳ+=(long)ț.TotalDays+" "+ß.İ.ǳ("C5")+" ";if(ț.Hours>0||Ĳ!="")Ĳ+=ț.Hours+"h ";if(ț.Minutes>0||Ĳ!="")Ĳ+=ț.Minutes+"m ";
return Ĳ+ț.Seconds+"s";}}class Ț{public const double ș=0.05;public const int Ș=1000;public const int ȗ=10000;public double Ȗ{
get{return Ȕ;}}int ȕ=Ș;double Ȕ=0;List<ɔ>ȓ=new List<ɔ>(100);public MyGridProgram Ǒ;public bool ȥ=false;int ȯ=0;public Ț(
MyGridProgram Ǆ,int ǃ=1,bool ȵ=false){Ǒ=Ǆ;ȯ=ǃ;ȥ=ȵ;}public void ȴ(ɔ ô,double ȳ,bool Ȳ=false){ô.ɍ=true;ô.Ɋ(this);if(Ȳ){ô.ɓ=Ȗ;ȓ.Insert(0
,ô);return;}if(ȳ<=0)ȳ=0.001;ô.ɓ=Ȗ+ȳ;for(int n=0;n<ȓ.Count;n++){if(ȓ[n].ɓ>ô.ɓ){ȓ.Insert(n,ô);return;}if(ô.ɓ-ȓ[n].ɓ<ș)ô.ɓ=ȓ
[n].ɓ+ș;}ȓ.Add(ô);}public void ȱ(ɔ ô){if(ȓ.Contains(ô)){ȓ.Remove(ô);ô.ɍ=false;}}public void Ȯ(ʐ Ȱ,int Ȭ=1){if(ȯ==Ȭ)Ǒ.Echo
(Ȱ.ɖ());}public void Ȯ(string ȭ,int Ȭ=1){if(ȯ==Ȭ)Ǒ.Echo(ȭ);}const double ȫ=(16.66666666/16);double Ȫ=0;public void ȩ(){Ȫ
+=Ǒ.Runtime.TimeSinceLastRun.TotalSeconds*ȫ;}ʐ ƴ=new ʐ();public void Ȩ(){double ȧ=Ǒ.Runtime.TimeSinceLastRun.TotalSeconds*
ȫ+Ȫ;Ȫ=0;Ȕ+=ȧ;ȕ=(int)Math.Min((ȧ*60)*Ș/(ȥ?5:1),ȗ-1000);while(ȓ.Count>=1){ɔ ô=ȓ[0];if(ȕ-Ǒ.Runtime.CurrentInstructionCount<=
0)break;if(ô.ɓ>Ȕ){int Ȧ=(int)(60*(ô.ɓ-Ȕ));if(Ȧ>=100){Ǒ.Runtime.UpdateFrequency=UpdateFrequency.Update100;}else{if(Ȧ>=10||
ȥ)Ǒ.Runtime.UpdateFrequency=UpdateFrequency.Update10;else Ǒ.Runtime.UpdateFrequency=UpdateFrequency.Update1;}break;}ȓ.
Remove(ô);if(!ô.ə())break;}}public int ɉ(){return(ȗ-Ǒ.Runtime.CurrentInstructionCount);}public bool ʊ(int ʈ){return((ȕ-Ǒ.
Runtime.CurrentInstructionCount)>=ʈ);}public void ʇ(){Ȯ(ƴ.Ŭ().ɱ("Remaining Instr: ").ɱ(ɉ()));}}class ʆ:ɔ{MyShipVelocities ʅ;
public Vector3D ʄ{get{return ʅ.LinearVelocity;}}public Vector3D ʃ{get{return ʅ.AngularVelocity;}}double ʂ=0;public double ʁ{
get{if(ɽ!=null)return ɽ.GetShipSpeed();else return ʂ;}}double ʀ=0;public double ɿ{get{return ʀ;}}double ʉ=0;public double ɾ
{get{return ʉ;}}double ɼ=0;double ɻ=0;public double ɺ{get{return ɼ;}}MyShipMass ɹ;public double ɸ{get{return ɹ.TotalMass;
}}public double ɷ{get{return ɹ.BaseMass;}}double ɶ=double.NaN;public double ɵ{get{return ɶ;}}double ɴ=double.NaN;public
double ɳ{get{return ɴ;}}IMyShipController ɽ=null;IMySlimBlock ɲ=null;public IMyShipController ʋ{get{return ɽ;}}Vector3D ʕ;
public ʆ(Ț Ƣ){ɘ="ShipMgr";Ʃ=Ƣ;ʕ=Ʃ.Ǒ.Me.GetPosition();ɏ=0.5;}List<IMyTerminalBlock>ʔ=new List<IMyTerminalBlock>();int ʓ=0;
public override bool ɮ(bool ñ){if(!ñ){ʔ.Clear();Ʃ.Ǒ.GridTerminalSystem.GetBlocksOfType<IMyShipController>(ʔ);ʓ=0;if(ɽ!=null&&ɽ
.CubeGrid.GetCubeBlock(ɽ.Position)!=ɲ)ɽ=null;}if(ʔ.Count>0){for(;ʓ<ʔ.Count;ʓ++){if(!Ʃ.ʊ(20))return false;
IMyShipController ʒ=ʔ[ʓ]as IMyShipController;if(ʒ.IsMainCockpit||ʒ.IsUnderControl){ɽ=ʒ;ɲ=ʒ.CubeGrid.GetCubeBlock(ʒ.Position);if(ʒ.
IsMainCockpit){ʓ=ʔ.Count;break;}}}if(ɽ==null){ɽ=ʔ[0]as IMyShipController;ɲ=ɽ.CubeGrid.GetCubeBlock(ɽ.Position);}ɹ=ɽ.CalculateShipMass
();if(!ɽ.TryGetPlanetElevation(MyPlanetElevation.Sealevel,out ɶ))ɶ=double.NaN;if(!ɽ.TryGetPlanetElevation(
MyPlanetElevation.Surface,out ɴ))ɴ=double.NaN;ʅ=ɽ.GetShipVelocities();}double ʑ=ʂ;ʂ=ʄ.Length();ʀ=(ʂ-ʑ)/ɐ;if(-ʀ>ʉ)ʉ=-ʀ;if(-ʀ>ɼ){ɼ=-ʀ;ɻ=Ʃ.Ȗ
;}if(Ʃ.Ȗ-ɻ>5&&-ʀ>0.1)ɼ-=(ɼ+ʀ)*0.3f;return true;}}class ʐ{public StringBuilder ƴ;public ʐ(int ʏ=0){ƴ=new StringBuilder(ʏ);
}public int ʎ{get{return ƴ.Length;}}public ʐ Ŭ(){ƴ.Clear();return this;}public ʐ ɱ(string Ĥ){ƴ.Append(Ĥ);return this;}
public ʐ ɱ(double ʍ){ƴ.Append(ʍ);return this;}public ʐ ɱ(char Ǫ){ƴ.Append(Ǫ);return this;}public ʐ ɱ(ʐ ʌ){ƴ.Append(ʌ.ƴ);return
this;}public ʐ ɱ(string Ĥ,int ȉ,int ɕ){ƴ.Append(Ĥ,ȉ,ɕ);return this;}public ʐ ɱ(char Ǫ,int ř){ƴ.Append(Ǫ,ř);return this;}
public ʐ ɗ(int ȉ,int ɕ){ƴ.Remove(ȉ,ɕ);return this;}public string ɖ(){return ƴ.ToString();}public string ɖ(int ȉ,int ɕ){return
ƴ.ToString(ȉ,ɕ);}public char this[int a]{get{return ƴ[a];}}}class ɔ{public string ɘ="MMTask";public double ɓ=0;public
double ɑ=0;public double ɐ=0;public double ɏ=-1;double Ɏ=-1;public bool ɍ=false;public bool Ɍ=false;double ɋ=0;protected Ț Ʃ;
public void Ɋ(Ț Ƣ){Ʃ=Ƣ;if(Ʃ.ȥ){if(Ɏ==-1){Ɏ=ɏ;ɏ*=2;}else{ɏ=Ɏ*2;}}else{if(Ɏ!=-1){ɏ=Ɏ;Ɏ=-1;}}}protected bool ɒ(double ȳ){ɋ=Math.
Max(ȳ,0.0001);return true;}public bool ə(){if(ɑ>0){ɐ=Ʃ.Ȗ-ɑ;Ʃ.Ȯ((Ɍ?"Running":"Resuming")+" task: "+ɘ);Ɍ=ɮ(!Ɍ);}else{ɐ=0;Ʃ.Ȯ(
"Init task: "+ɘ);ɯ();Ʃ.Ȯ("Running..");Ɍ=ɮ(false);if(!Ɍ)ɑ=0.001;}if(Ɍ){ɑ=Ʃ.Ȗ;if((ɏ>=0||ɋ>0)&&ɍ)Ʃ.ȴ(this,(ɋ>0?ɋ:ɏ));else{ɍ=false;ɑ=0;}}
else{if(ɍ)Ʃ.ȴ(this,0,true);}Ʃ.Ȯ("Task "+(Ɍ?"":"NOT ")+"finished. "+(ɍ?(ɋ>0?"Postponed by "+ɋ.ToString("F1")+"s":
"Scheduled after "+ɏ.ToString("F1")+"s"):"Stopped."));ɋ=0;return Ɍ;}public void ɰ(){Ʃ.ȱ(this);ɭ();ɍ=false;Ɍ=false;ɑ=0;}public virtual void
ɯ(){}public virtual bool ɮ(bool ñ){return true;}public virtual void ɭ(){}}class ɬ{public const float ɫ=512;public const
float ɪ=ɫ/0.7783784f;public const float ɩ=ɫ/0.7783784f;public const float ɨ=ɪ;public const float ɧ=37;public string ɦ=
"T:[LCD]";public int ɥ=1;public bool ɤ=true;public List<string>ɣ=null;public bool ɢ=true;public int ȯ=0;public float ɡ=1.0f;
public float ɠ=1.0f;public float ɟ{get{return ɨ*ǂ.Ѧ;}}public float ɞ{get{return(float)ɟ-2*ǐ[Ǔ]*Ù;}}string ɝ;string ɜ;float ɛ=-
1;Dictionary<string,float>ɚ=new Dictionary<string,float>(2);Dictionary<string,float>ȑ=new Dictionary<string,float>(2);
Dictionary<string,float>ǒ=new Dictionary<string,float>(2);public float Ʒ{get{return ǒ[Ǔ];}}Dictionary<string,float>ǐ=new
Dictionary<string,float>(2);Dictionary<string,float>Ǐ=new Dictionary<string,float>(2);Dictionary<string,float>ǎ=new Dictionary<
string,float>(2);int Ù=0;string Ø="";Dictionary<string,char>Ǎ=new Dictionary<string,char>(2);Dictionary<string,char>ǌ=new
Dictionary<string,char>(2);Dictionary<string,char>ǋ=new Dictionary<string,char>(2);Dictionary<string,char>Ǌ=new Dictionary<string,
char>(2);public Ț Ʃ;public Program Ǒ;public ȷ ǉ;public Г Ŋ;public ʆ ǈ;public Î ƥ;public ǫ İ;public IMyGridTerminalSystem Ǉ{
get{return Ǒ.GridTerminalSystem;}}public IMyProgrammableBlock ǆ{get{return Ǒ.Me;}}public Action<string>ǅ{get{return Ǒ.Echo;
}}public ɬ(Program Ǆ,int ǃ,Ț Ƣ){Ʃ=Ƣ;ȯ=ǃ;Ǒ=Ǆ;İ=new ǫ();ǉ=new ȷ(Ƣ,this);Ŋ=new Г(Ƣ,this);Ŋ.Ў();ǈ=new ʆ(Ʃ);Ʃ.ȴ(ǈ,0);}ѧ ǂ=null
;public string Ǔ{get{return ǂ.W;}}public bool Ǜ{get{return(ǂ.ŝ()==0);}}public bool ǚ(IMyTerminalBlock Ü){if(Ü==null||Ü.
WorldMatrix==MatrixD.Identity)return true;return Ǉ.GetBlockWithId(Ü.EntityId)==null;}public ѧ Ǚ(ѧ ǘ,à í){í.H();IMyTextSurface B=í.Ô
;if(ǘ==null)ǘ=new ѧ(this);ǘ.W=B.Font;if(!ǐ.ContainsKey(ǘ.W))ǘ.W=ɝ;ǘ.Ѧ=í.J()*(B.SurfaceSize.X/B.TextureSize.X)*Math.Max(
1.0f,B.TextureSize.X/B.TextureSize.Y)*ɡ/B.FontSize*(100f-B.TextPadding*2)/100;Ø=í.Ø;Ù=í.Ù;ǂ=ǘ;return ǘ;}public ŗ Ǘ(ŗ Ý,à í){
í.H();IMyTextSurface B=í.Ô;if(Ý==null)Ý=new ŗ(this);Ý.ů(í.Ó);Ý.ŷ=í.V()*(B.SurfaceSize.Y/B.TextureSize.Y)*Math.Max(1.0f,B.
TextureSize.Y/B.TextureSize.X)*ɠ/B.FontSize*(100f-B.TextPadding*2)/100;Ý.Ů();Ø=í.Ø;Ù=í.Ù;return Ý;}public void ǖ(){ǂ.ť();}public
void ǖ(ʐ Ť){if(ǂ.ѥ<=0)ǂ.ŧ(Ø);ǂ.ŧ(Ť);ǂ.ť();}public void ǖ(string Ť){if(ǂ.ѥ<=0)ǂ.ŧ(Ø);ǂ.ť(Ť);}public void Ǖ(string Š){ǂ.š(Š,Ø)
;}public void ǔ(List<ʐ>ş){ǂ.ţ(ş);}public void Ê(ʐ Ŧ,bool Ƽ=true){if(ǂ.ѥ<=0)ǂ.ŧ(Ø);ǂ.ŧ(Ŧ);if(Ƽ)ǂ.ѥ+=Ǩ(Ŧ,ǂ.W);}public void
Ê(string Ĳ,bool Ƽ=true){if(ǂ.ѥ<=0)ǂ.ŧ(Ø);ǂ.ŧ(Ĳ);if(Ƽ)ǂ.ѥ+=Ǩ(Ĳ,ǂ.W);}public void Ƶ(ʐ Ŧ,float Ʊ=1.0f,float ư=0f){Ʋ(Ŧ,Ʊ,ư);ǂ
.ť();}public void Ƶ(string Ĳ,float Ʊ=1.0f,float ư=0f){Ʋ(Ĳ,Ʊ,ư);ǂ.ť();}ʐ ƴ=new ʐ();public void Ʋ(ʐ Ŧ,float Ʊ=1.0f,float ư=
0f){float Ư=Ǩ(Ŧ,ǂ.W);float Ʈ=Ʊ*ɨ*ǂ.Ѧ-ǂ.ѥ-ư;if(Ù>0)Ʈ-=2*ǐ[ǂ.W]*Ù;if(Ʈ<Ư){ǂ.ŧ(Ŧ);ǂ.ѥ+=Ư;return;}Ʈ-=Ư;int ƭ=(int)Math.Floor(Ʈ
/ǐ[ǂ.W]);float Ƴ=ƭ*ǐ[ǂ.W];ƴ.Ŭ().ɱ(' ',ƭ).ɱ(Ŧ);ǂ.ŧ(ƴ);ǂ.ѥ+=Ƴ+Ư;}public void Ʋ(string Ĳ,float Ʊ=1.0f,float ư=0f){float Ư=Ǩ(
Ĳ,ǂ.W);float Ʈ=Ʊ*ɨ*ǂ.Ѧ-ǂ.ѥ-ư;if(Ù>0)Ʈ-=2*ǐ[ǂ.W]*Ù;if(Ʈ<Ư){ǂ.ŧ(Ĳ);ǂ.ѥ+=Ư;return;}Ʈ-=Ư;int ƭ=(int)Math.Floor(Ʈ/ǐ[ǂ.W]);
float Ƴ=ƭ*ǐ[ǂ.W];ƴ.Ŭ().ɱ(' ',ƭ).ɱ(Ĳ);ǂ.ŧ(ƴ);ǂ.ѥ+=Ƴ+Ư;}public void ƶ(ʐ Ŧ){ǀ(Ŧ);ǂ.ť();}public void ƶ(string Ĳ){ǀ(Ĳ);ǂ.ť();}
public void ǀ(ʐ Ŧ){float Ư=Ǩ(Ŧ,ǂ.W);float ǁ=ɨ/2*ǂ.Ѧ-ǂ.ѥ;if(ǁ<Ư/2){ǂ.ŧ(Ŧ);ǂ.ѥ+=Ư;return;}ǁ-=Ư/2;int ƭ=(int)Math.Round(ǁ/ǐ[ǂ.W],
MidpointRounding.AwayFromZero);float Ƴ=ƭ*ǐ[ǂ.W];ƴ.Ŭ().ɱ(' ',ƭ).ɱ(Ŧ);ǂ.ŧ(ƴ);ǂ.ѥ+=Ƴ+Ư;}public void ǀ(string Ĳ){float Ư=Ǩ(Ĳ,ǂ.W);float ǁ=ɨ/
2*ǂ.Ѧ-ǂ.ѥ;if(ǁ<Ư/2){ǂ.ŧ(Ĳ);ǂ.ѥ+=Ư;return;}ǁ-=Ư/2;int ƭ=(int)Math.Round(ǁ/ǐ[ǂ.W],MidpointRounding.AwayFromZero);float Ƴ=ƭ*
ǐ[ǂ.W];ƴ.Ŭ().ɱ(' ',ƭ).ɱ(Ĳ);ǂ.ŧ(ƴ);ǂ.ѥ+=Ƴ+Ư;}public void ƿ(double ƾ,float ƽ=1.0f,float ư=0f,bool Ƽ=true){if(Ù>0)ư+=2*Ù*ǐ[ǂ
.W];float ƻ=ɨ*ƽ*ǂ.Ѧ-ǂ.ѥ-ư;if(Double.IsNaN(ƾ))ƾ=0;int ƺ=(int)(ƻ/Ǐ[ǂ.W])-2;if(ƺ<=0)ƺ=2;int ƹ=Math.Min((int)(ƾ*ƺ)/100,ƺ);if(
ƹ<0)ƹ=0;if(ǂ.ѥ<=0)ǂ.ŧ(Ø);ƴ.Ŭ().ɱ(Ǎ[ǂ.W]).ɱ(Ǌ[ǂ.W],ƹ).ɱ(ǋ[ǂ.W],ƺ-ƹ).ɱ(ǌ[ǂ.W]);ǂ.ŧ(ƴ);if(Ƽ)ǂ.ѥ+=Ǐ[ǂ.W]*ƺ+2*ǎ[ǂ.W];}public
void Ƹ(double ƾ,float ƽ=1.0f,float ư=0f){ƿ(ƾ,ƽ,ư,false);ǂ.ť();}public void Ş(){ǂ.Ş();}public void Ȃ(à ª,ŗ C=null){ª.D(C);if(
ª.Õ)ª.Ý.Ĵ();}public void ȁ(string Ȁ,string Ĳ){IMyTextPanel ª=Ǒ.GridTerminalSystem.GetBlockWithName(Ȁ)as IMyTextPanel;if(ª
==null)return;ª.WriteText(Ĳ+"\n",true);}public string ǿ(MyInventoryItem f){string Ǿ=f.Type.TypeId.ToString();Ǿ=Ǿ.Substring
(Ǿ.LastIndexOf('_')+1);return f.Type.SubtypeId+" "+Ǿ;}public void ȃ(string Å,out string Ã,out string Â){int Ļ=Å.
LastIndexOf(' ');if(Ļ>=0){Ã=Å.Substring(0,Ļ);Â=Å.Substring(Ļ+1);return;}Ã=Å;Â="";}public string ǽ(string Å){string Ã,Â;ȃ(Å,out Ã,
out Â);return ǽ(Ã,Â);}public string ǽ(string Ã,string Â){Á f=ƥ.Ä(Ã,Â);if(f!=null){if(f.è.Length>0)return f.è;return f.Ò;}
return System.Text.RegularExpressions.Regex.Replace(Ã,"([a-z])([A-Z])","$1 $2");}public void Ǽ(ref string Ã,ref string Â){Á f;
if(ƥ.Ë.TryGetValue(Ã,out f)){Ã=f.Ò;Â=f.ê;return;}f=ƥ.Ä(Ã,Â);if(f!=null){Ã=f.Ò;if(string.IsNullOrEmpty(Â)&&(string.Compare(
f.ê,"Ore",true)==0)||(string.Compare(f.ê,"Ingot",true)==0))return;Â=f.ê;}}public string Ȑ(double Ȏ,bool ȍ=true,char Ȍ=' '
){if(!ȍ)return Ȏ.ToString("#,###,###,###,###,###,###,###,###,###");string ȋ=" kMGTPEZY";double Ȋ=Ȏ;int ȉ=ȋ.IndexOf(Ȍ);var
Ȉ=(ȉ<0?0:ȉ);while(Ȋ>=1000&&Ȉ+1<ȋ.Length){Ȋ/=1000;Ȉ++;}ƴ.Ŭ().ɱ(Math.Round(Ȋ,1,MidpointRounding.AwayFromZero));if(Ȉ>0)ƴ.ɱ(
" ").ɱ(ȋ[Ȉ]);return ƴ.ɖ();}public string ȏ(double Ȏ,bool ȍ=true,char Ȍ=' '){if(!ȍ)return Ȏ.ToString(
"#,###,###,###,###,###,###,###,###,###");string ȋ=" ktkMGTPEZY";double Ȋ=Ȏ;int ȉ=ȋ.IndexOf(Ȍ);var Ȉ=(ȉ<0?0:ȉ);while(Ȋ>=1000&&Ȉ+1<ȋ.Length){Ȋ/=1000;Ȉ++;}ƴ.Ŭ().ɱ
(Math.Round(Ȋ,1,MidpointRounding.AwayFromZero));if(Ȉ==1)ƴ.ɱ(" kg");else if(Ȉ==2)ƴ.ɱ(" t");else if(Ȉ>2)ƴ.ɱ(" ").ɱ(ȋ[Ȉ]).ɱ(
"t");return ƴ.ɖ();}public string ȇ(double ƾ){return(Math.Floor(ƾ*10)/10).ToString("F1");}Dictionary<char,float>Ȇ=new
Dictionary<char,float>();void ȅ(string Ȅ,float L){L+=1;for(int n=0;n<Ȅ.Length;n++){if(L>ɚ[ɝ])ɚ[ɝ]=L;Ȇ.Add(Ȅ[n],L);}}public float ǻ
(char Ǫ,string W){float ƻ;if(W==ɜ||!Ȇ.TryGetValue(Ǫ,out ƻ))return ɚ[W];return ƻ;}public float Ǩ(ʐ ǩ,string W){if(W==ɜ)
return ǩ.ʎ*ɚ[W];float ǧ=0;for(int n=0;n<ǩ.ʎ;n++)ǧ+=ǻ(ǩ[n],W);return ǧ;}public float Ǩ(string Ĥ,string W){if(W==ɜ)return Ĥ.
Length*ɚ[W];float ǧ=0;for(int n=0;n<Ĥ.Length;n++)ǧ+=ǻ(Ĥ[n],W);return ǧ;}public string Ǧ(string Ĳ,float Ǥ){if(Ǥ/ɚ[ǂ.W]>=Ĳ.
Length)return Ĳ;float ǣ=Ǩ(Ĳ,ǂ.W);if(ǣ<=Ǥ)return Ĳ;float Ǣ=ǣ/Ĳ.Length;Ǥ-=ȑ[ǂ.W];int ǡ=(int)Math.Max(Ǥ/Ǣ,1);if(ǡ<Ĳ.Length/2){ƴ.Ŭ
().ɱ(Ĳ,0,ǡ);ǣ=Ǩ(ƴ,ǂ.W);}else{ƴ.Ŭ().ɱ(Ĳ);ǡ=Ĳ.Length;}while(ǣ>Ǥ&&ǡ>1){ǡ--;ǣ-=ǻ(Ĳ[ǡ],ǂ.W);}if(ƴ.ʎ>ǡ)ƴ.ɗ(ǡ,ƴ.ʎ-ǡ);return ƴ.ɱ(
"..").ɖ();}void Ǡ(string ǟ){ɝ=ǟ;Ǎ[ɝ]=MMStyle.BAR_START;ǌ[ɝ]=MMStyle.BAR_END;ǋ[ɝ]=MMStyle.BAR_EMPTY;Ǌ[ɝ]=MMStyle.BAR_FILL;ɚ[ɝ
]=0f;}void Ǟ(string ǝ,float ǜ){ɜ=ǝ;ɛ=ǜ;ɚ[ɜ]=ɛ+1;ȑ[ɜ]=2*(ɛ+1);Ǎ[ɜ]=MMStyle.BAR_MONO_START;ǌ[ɜ]=MMStyle.BAR_MONO_END;ǋ[ɜ]=
MMStyle.BAR_MONO_EMPTY;Ǌ[ɜ]=MMStyle.BAR_MONO_FILL;ǐ[ɜ]=ǻ(' ',ɜ);Ǐ[ɜ]=ǻ(ǋ[ɜ],ɜ);ǎ[ɜ]=ǻ(Ǎ[ɜ],ɜ);ǒ[ɜ]=Ǩ(" 100.0%",ɜ);}public void
ǥ(){if(Ȇ.Count>0)return;
// Monospace font name, width of single character
// Change this if you want to use different (modded) monospace font
Ǟ("Monospace", 24f);

// Classic/Debug font name (uses widths of characters below)
// Change this if you want to use different font name (non-monospace)
Ǡ("Debug");
// Font characters width (font "aw" values here)
ȅ("3FKTabdeghknopqsuy£µÝàáâãäåèéêëðñòóôõöøùúûüýþÿāăąďđēĕėęěĝğġģĥħĶķńņňŉōŏőśŝşšŢŤŦũūŭůűųŶŷŸșȚЎЗКЛбдекруцяёђћўџ", 17f);
ȅ("ABDNOQRSÀÁÂÃÄÅÐÑÒÓÔÕÖØĂĄĎĐŃŅŇŌŎŐŔŖŘŚŜŞŠȘЅЊЖф□", 21f);
ȅ("#0245689CXZ¤¥ÇßĆĈĊČŹŻŽƒЁЌАБВДИЙПРСТУХЬ€", 19f);
ȅ("￥$&GHPUVY§ÙÚÛÜÞĀĜĞĠĢĤĦŨŪŬŮŰŲОФЦЪЯжы†‡", 20f);
ȅ("！ !I`ijl ¡¨¯´¸ÌÍÎÏìíîïĨĩĪīĮįİıĵĺļľłˆˇ˘˙˚˛˜˝ІЇії‹›∙", 8f);
ȅ("？7?Jcz¢¿çćĉċčĴźżžЃЈЧавийнопсъьѓѕќ", 16f);
ȅ("（）：《》，。、；【】(),.1:;[]ft{}·ţťŧț", 9f);
ȅ("+<=>E^~¬±¶ÈÉÊË×÷ĒĔĖĘĚЄЏЕНЭ−", 18f);
ȅ("L_vx«»ĹĻĽĿŁГгзлхчҐ–•", 15f);
ȅ("\"-rª­ºŀŕŗř", 10f);
ȅ("WÆŒŴ—…‰", 31f);
ȅ("'|¦ˉ‘’‚", 6f);
ȅ("@©®мшњ", 25f);
ȅ("mw¼ŵЮщ", 27f);
ȅ("/ĳтэє", 14f);
ȅ("\\°“”„", 12f);
ȅ("*²³¹", 11f);
ȅ("¾æœЉ", 28f);
ȅ("%ĲЫ", 24f);
ȅ("MМШ", 26f);
ȅ("½Щ", 29f);
ȅ("ю", 23f);
ȅ("ј", 7f);
ȅ("љ", 22f);
ȅ("ґ", 13f);
ȅ("™", 30f);
// End of font characters width
        ǐ[ɝ]=ǻ(' ',ɝ);Ǐ[ɝ]=ǻ(ǋ[ɝ],ɝ);ǎ[ɝ]=ǻ(Ǎ[ɝ],ɝ);ǒ[ɝ]=Ǩ(" 100.0%",ɝ);ȑ[ɝ]=ǻ('.',ɝ)*2;}}class ǫ{public string ǳ(string
Ǻ){return TT[Ǻ];}
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
}static class ǹ{public static bool Ǹ(this string Ĥ,string Ƕ){return Ĥ.StartsWith(Ƕ,StringComparison.
InvariantCultureIgnoreCase);}public static bool Ƿ(this string Ĥ,string Ƕ){if(Ĥ==null)return false;return Ĥ.IndexOf(Ƕ,StringComparison.
InvariantCultureIgnoreCase)>=0;}public static bool ǵ(this string Ĥ,string Ƕ){return Ĥ.EndsWith(Ƕ,StringComparison.InvariantCultureIgnoreCase);}}
static class Ǵ{public static string ǲ(this IMyTerminalBlock Ü){int ŀ=Ü.CustomData.IndexOf("\n---\n");if(ŀ<0){if(Ü.CustomData.
StartsWith("---\n"))return Ü.CustomData.Substring(4);return Ü.CustomData;}return Ü.CustomData.Substring(ŀ+5);}public static string
Ǳ(this IMyTerminalBlock Ü,int Ļ,string ǰ){string ǯ=Ü.ǲ();string Ǯ="@"+Ļ.ToString()+" AutoLCD";string ǭ='\n'+Ǯ;int ŀ=0;if(
!ǯ.StartsWith(Ǯ,StringComparison.InvariantCultureIgnoreCase)){ŀ=ǯ.IndexOf(ǭ,StringComparison.InvariantCultureIgnoreCase);
}if(ŀ<0){if(Ļ==0){if(ǯ.Length==0)return"";if(ǯ[0]=='@')return null;ŀ=ǯ.IndexOf("\n@");if(ŀ<0)return ǯ;return ǯ.Substring(
0,ŀ);}else return null;}int Ǭ=ǯ.IndexOf("\n@",ŀ+1);if(Ǭ<0){if(ŀ==0)return ǯ;return ǯ.Substring(ŀ+1);}if(ŀ==0)return ǯ.
Substring(0,Ǭ);return ǯ.Substring(ŀ+1,Ǭ-ŀ);}