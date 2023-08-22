/* v:2.0186 (Updated item names)
* Automatic LCDs 2 - In-game script by MMaster
*
* Thank all of you for making amazing creations with this script, using it and helping each other use it.
* Its 2023 - it's been 8 years already since I uploaded first Automatic LCDs script and you are still using it (in "a bit" upgraded form).
* That's just amazing! I hope you will have many more years of fun with it :)
*
* LATEST UPDATE: 
*  Updated items to match recent changes in the game (item list updated in the guide - Inventory command)
*  Items that changed name can now have 2 short names to ensure compatibility with old blueprints
*   eg: old "Inventory * +200mmmissile" and new "Inventory * +rockets" will both work
*  
* Previous notable updates:
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
void Add(string sT, string mT, int q = 0, string dN = "", string sN = "", string sN2 = "", bool u = true) { Ʀ.Ë(sT, mT, q, dN, sN, sN2, u); }
Ï Ʀ;Ț ƣ;þ Ϣ;ɬ à=null;void ϡ(string Ɣ){}bool Ϡ(string ϝ){return ϝ.Ƿ("true")?true:false;}void ϟ(string Ϟ,string ϝ){string
Ȁ=Ϟ.ToLower();switch(Ȁ){case"lcd_tag":LCD_TAG=ϝ;break;case"slowmode":SlowMode=Ϡ(ϝ);break;case"enable_boot":ENABLE_BOOT=Ϡ(
ϝ);break;case"skip_content_type":SKIP_CONTENT_TYPE=Ϡ(ϝ);break;case"scroll_lines":int Ϝ=0;if(int.TryParse(ϝ,out Ϝ)){
SCROLL_LINES=Ϝ;}break;}}void ϛ(){string[]ŧ=Me.CustomData.Split('\n');for(int X=0;X<ŧ.Length;X++){string Ť=ŧ[X];int ļ=Ť.IndexOf('=');
if(ļ<0){ϡ(Ť);continue;}string Ϛ=Ť.Substring(0,ļ).Trim();string Ƕ=Ť.Substring(ļ+1).Trim();ϟ(Ϛ,Ƕ);}}void ϙ(Ț ƣ){Ʀ=new Ï();
ItemsConf();ϛ();à=new ɬ(this,DebugLevel,ƣ){Ʀ=Ʀ,ɦ=LCD_TAG,ɯ=SCROLL_LINES,ɥ=ENABLE_BOOT,ɣ=BOOT_FRAMES,ɢ=!MDK_IS_GREAT,ɠ=HEIGHT_MOD,
ɡ=WIDTH_MOD};à.ǥ();}void Ϙ(){ƣ.Ǒ=this;à.Ǒ=this;}Program(){Runtime.UpdateFrequency=UpdateFrequency.Update1;}void Main(
string Ă,UpdateType τ){try{if(ƣ==null){ƣ=new Ț(this,DebugLevel,SlowMode);ϙ(ƣ);Ϣ=new þ(à);ƣ.Ȳ(Ϣ,0);}else{Ϙ();à.ŋ.Ѝ();}if(Ă.
Length==0&&(τ&(UpdateType.Update1|UpdateType.Update10|UpdateType.Update100))==0){ƣ.ȩ();return;}if(Ă!=""){if(Ϣ.ă(Ă)){ƣ.ȩ();
return;}}Ϣ.ý=0;ƣ.Ȩ();}catch(Exception ex){Echo("ERROR DESCRIPTION:\n"+ex.ToString());Me.Enabled=false;}}class σ:ɔ{þ č;ɬ à;
string Ă="";public σ(ɬ V,þ ē,string Ŕ){ɐ=-1;ɓ="ArgScroll";Ă=Ŕ;č=ē;à=V;}int ŗ;ο ς;public override void ɰ(){ς=new ο(ƫ,à.ŋ);}int
ρ=0;int ĕ=0;Ͱ Ɣ;public override bool ɮ(bool ò){if(!ò){ĕ=0;ς.Ů();Ɣ=new Ͱ(ƫ);ρ=0;}if(ĕ==0){if(!Ɣ.ʟ(Ă,ò))return false;if(Ɣ.ˑ
.Count>0){if(!int.TryParse(Ɣ.ˑ[0].Ŕ,out ŗ))ŗ=1;else if(ŗ<1)ŗ=1;}if(Ɣ.ˮ.EndsWith("up"))ŗ=-ŗ;else if(!Ɣ.ˮ.EndsWith("down"))
ŗ=0;ĕ++;ò=false;}if(ĕ==1){if(!ς.Ͻ("textpanel",Ɣ.ˬ,ò))return false;ĕ++;ò=false;}á î;for(;ρ<ς.Г();ρ++){if(!ƫ.ʕ(20))return
false;IMyTextPanel π=ς.υ[ρ]as IMyTextPanel;if(!č.ù.TryGetValue(π,out î))continue;if(î==null||î.Ý!=π)continue;if(î.Ö)î.Þ.ķ=10;
if(ŗ>0)î.Þ.ĺ(ŗ);else if(ŗ<0)î.Þ.ł(-ŗ);else î.Þ.Ķ();î.E();}return true;}}class ο{Ț ƫ;В ξ;IMyCubeGrid ν{get{return ƫ.Ǒ.Me.
CubeGrid;}}IMyGridTerminalSystem Ǉ{get{return ƫ.Ǒ.GridTerminalSystem;}}public List<IMyTerminalBlock>υ=new List<IMyTerminalBlock>
();public ο(Ț ƣ,В ϗ){ƫ=ƣ;ξ=ϗ;}int ϖ=0;public double ϕ(ref double ϔ,ref double ϓ,bool ò){if(!ò)ϖ=0;for(;ϖ<υ.Count;ϖ++){if(
!ƫ.ʕ(4))return Double.NaN;IMyInventory ώ=υ[ϖ].GetInventory(0);if(ώ==null)continue;ϔ+=(double)ώ.CurrentVolume;ϓ+=(double)ώ
.MaxVolume;}ϔ*=1000;ϓ*=1000;return(ϓ>0?ϔ/ϓ*100:100);}int ϒ=0;double ϑ=0;public double ϐ(bool ò){if(!ò){ϒ=0;ϑ=0;}for(;ϒ<υ.
Count;ϒ++){if(!ƫ.ʕ(6))return Double.NaN;for(int Ϗ=0;Ϗ<2;Ϗ++){IMyInventory ώ=υ[ϒ].GetInventory(Ϗ);if(ώ==null)continue;ϑ+=(
double)ώ.CurrentMass;}}return ϑ*1000;}int ύ=0;private bool ό(bool ò=false){if(!ò)ύ=0;while(ύ<υ.Count){if(!ƫ.ʕ(4))return false;
if(υ[ύ].CubeGrid!=ν){υ.RemoveAt(ύ);continue;}ύ++;}return true;}int ϋ=0;private bool ϊ(bool ò=false){if(!ò)ϋ=0;var ω=ƫ.Ǒ.Me
;while(ϋ<υ.Count){if(!ƫ.ʕ(4))return false;if(!υ[ϋ].IsSameConstructAs(ω)){υ.RemoveAt(ϋ);continue;}ϋ++;}return true;}List<
IMyBlockGroup>ψ=new List<IMyBlockGroup>();List<IMyTerminalBlock>χ=new List<IMyTerminalBlock>();int ϣ=0;public bool Ϥ(string ˬ,bool ò)
{int Ѓ=ˬ.IndexOf(':');string ϼ=(Ѓ>=1&&Ѓ<=2?ˬ.Substring(0,Ѓ):"");bool Ё=ϼ.Contains("T");bool Ј=ϼ.Contains("C");if(ϼ!="")ˬ=
ˬ.Substring(Ѓ+1);if(ˬ==""||ˬ=="*"){if(!ò){χ.Clear();Ǉ.GetBlocks(χ);υ.AddList(χ);}if(Ё){if(!ό(ò))return false;}else if(Ј){
if(!ϊ(ò))return false;}return true;}string Ђ=(ϼ.Contains("G")?ˬ.Trim():"");if(Ђ!=""){if(!ò){ψ.Clear();Ǉ.GetBlockGroups(ψ);
ϣ=0;}for(;ϣ<ψ.Count;ϣ++){IMyBlockGroup Ѐ=ψ[ϣ];if(string.Compare(Ѐ.Name,Ђ,true)==0){if(!ò){χ.Clear();Ѐ.GetBlocks(χ);υ.
AddList(χ);}if(Ё){if(!ό(ò))return false;}else if(Ј){if(!ϊ(ò))return false;}return true;}}return true;}if(!ò){χ.Clear();Ǉ.
SearchBlocksOfName(ˬ,χ);υ.AddList(χ);}if(Ё){if(!ό(ò))return false;}else if(Ј){if(!ϊ(ò))return false;}return true;}List<IMyBlockGroup>Ї=new
List<IMyBlockGroup>();List<IMyTerminalBlock>І=new List<IMyTerminalBlock>();int Ѕ=0;int Є=0;public bool Љ(string ʘ,string Ђ,
bool Ё,bool ò){if(!ò){Ї.Clear();Ǉ.GetBlockGroups(Ї);Ѕ=0;}for(;Ѕ<Ї.Count;Ѕ++){IMyBlockGroup Ѐ=Ї[Ѕ];if(string.Compare(Ѐ.Name,Ђ
,true)==0){if(!ò){Є=0;І.Clear();Ѐ.GetBlocks(І);}else ò=false;for(;Є<І.Count;Є++){if(!ƫ.ʕ(6))return false;if(Ё&&І[Є].
CubeGrid!=ν)continue;if(ξ.ϯ(І[Є],ʘ))υ.Add(І[Є]);}return true;}}return true;}List<IMyTerminalBlock>Ͽ=new List<IMyTerminalBlock>()
;int Ͼ=0;public bool Ͻ(string ʘ,string ˬ,bool ò){int Ѓ=ˬ.IndexOf(':');string ϼ=(Ѓ>=1&&Ѓ<=2?ˬ.Substring(0,Ѓ):"");bool Ё=ϼ.
Contains("T");bool Ј=ϼ.Contains("C");if(ϼ!="")ˬ=ˬ.Substring(Ѓ+1);if(!ò){Ͽ.Clear();Ͼ=0;}string Ђ=(ϼ.Contains("G")?ˬ.Trim():"");if
(Ђ!=""){if(!Љ(ʘ,Ђ,Ё,ò))return false;return true;}if(!ò)ξ.ϰ(ref Ͽ,ʘ);if(ˬ==""||ˬ=="*"){if(!ò)υ.AddList(Ͽ);if(Ё){if(!ό(ò))
return false;}else if(Ј){if(!ϊ(ò))return false;}return true;}for(;Ͼ<Ͽ.Count;Ͼ++){if(!ƫ.ʕ(4))return false;if(Ё&&Ͽ[Ͼ].CubeGrid!=
ν)continue;if(Ͽ[Ͼ].CustomName.Contains(ˬ))υ.Add(Ͽ[Ͼ]);}return true;}public void Е(ο Д){υ.AddList(Д.υ);}public void Ů(){υ.
Clear();}public int Г(){return υ.Count;}}class В{Ț ƫ;ɬ à;public MyGridProgram Ǒ{get{return ƫ.Ǒ;}}public IMyGridTerminalSystem
Ǉ{get{return ƫ.Ǒ.GridTerminalSystem;}}public В(Ț ƣ,ɬ V){ƫ=ƣ;à=V;}void Б<ǳ>(List<IMyTerminalBlock>А,Func<IMyTerminalBlock,
bool>Џ=null)where ǳ:class,IMyTerminalBlock{Ǉ.GetBlocksOfType<ǳ>(А,Џ);}public Dictionary<string,Action<List<IMyTerminalBlock>
,Func<IMyTerminalBlock,bool>>>Ў;public void Ѝ(){if(Ў!=null)return;Ў=new Dictionary<string,Action<List<IMyTerminalBlock>,
Func<IMyTerminalBlock,bool>>>(){{"CargoContainer",Б<IMyCargoContainer>},{"TextPanel",Б<IMyTextPanel>},{"Assembler",Б<
IMyAssembler>},{"Refinery",Б<IMyRefinery>},{"Reactor",Б<IMyReactor>},{"SolarPanel",Б<IMySolarPanel>},{"BatteryBlock",Б<
IMyBatteryBlock>},{"Beacon",Б<IMyBeacon>},{"RadioAntenna",Б<IMyRadioAntenna>},{"AirVent",Б<IMyAirVent>},{"ConveyorSorter",Б<
IMyConveyorSorter>},{"OxygenTank",Б<IMyGasTank>},{"OxygenGenerator",Б<IMyGasGenerator>},{"OxygenFarm",Б<IMyOxygenFarm>},{"LaserAntenna",Б
<IMyLaserAntenna>},{"Thrust",Б<IMyThrust>},{"Gyro",Б<IMyGyro>},{"SensorBlock",Б<IMySensorBlock>},{"ShipConnector",Б<
IMyShipConnector>},{"ReflectorLight",Б<IMyReflectorLight>},{"InteriorLight",Б<IMyInteriorLight>},{"LandingGear",Б<IMyLandingGear>},{
"ProgrammableBlock",Б<IMyProgrammableBlock>},{"TimerBlock",Б<IMyTimerBlock>},{"MotorStator",Б<IMyMotorStator>},{"PistonBase",Б<
IMyPistonBase>},{"Projector",Б<IMyProjector>},{"ShipMergeBlock",Б<IMyShipMergeBlock>},{"SoundBlock",Б<IMySoundBlock>},{"Collector",Б<
IMyCollector>},{"JumpDrive",Б<IMyJumpDrive>},{"Door",Б<IMyDoor>},{"GravityGeneratorSphere",Б<IMyGravityGeneratorSphere>},{
"GravityGenerator",Б<IMyGravityGenerator>},{"ShipDrill",Б<IMyShipDrill>},{"ShipGrinder",Б<IMyShipGrinder>},{"ShipWelder",Б<IMyShipWelder>}
,{"Parachute",Б<IMyParachute>},{"LargeGatlingTurret",Б<IMyLargeGatlingTurret>},{"LargeInteriorTurret",Б<
IMyLargeInteriorTurret>},{"LargeMissileTurret",Б<IMyLargeMissileTurret>},{"SmallGatlingGun",Б<IMySmallGatlingGun>},{
"SmallMissileLauncherReload",Б<IMySmallMissileLauncherReload>},{"SmallMissileLauncher",Б<IMySmallMissileLauncher>},{"VirtualMass",Б<IMyVirtualMass>}
,{"Warhead",Б<IMyWarhead>},{"FunctionalBlock",Б<IMyFunctionalBlock>},{"LightingBlock",Б<IMyLightingBlock>},{
"ControlPanel",Б<IMyControlPanel>},{"Cockpit",Б<IMyCockpit>},{"CryoChamber",Б<IMyCryoChamber>},{"MedicalRoom",Б<IMyMedicalRoom>},{
"RemoteControl",Б<IMyRemoteControl>},{"ButtonPanel",Б<IMyButtonPanel>},{"CameraBlock",Б<IMyCameraBlock>},{"OreDetector",Б<
IMyOreDetector>},{"ShipController",Б<IMyShipController>},{"SafeZoneBlock",Б<IMySafeZoneBlock>},{"Decoy",Б<IMyDecoy>}};}public void Ќ(
ref List<IMyTerminalBlock>Ñ,string Ћ){Action<List<IMyTerminalBlock>,Func<IMyTerminalBlock,bool>>Њ;if(Ћ=="SurfaceProvider"){
Ǉ.GetBlocksOfType<IMyTextSurfaceProvider>(Ñ);return;}if(Ў.TryGetValue(Ћ,out Њ))Њ(Ñ,null);else{if(Ћ=="WindTurbine"){Ǉ.
GetBlocksOfType<IMyPowerProducer>(Ñ,(ϥ)=>ϥ.BlockDefinition.TypeIdString.EndsWith("WindTurbine"));return;}if(Ћ=="HydrogenEngine"){Ǉ.
GetBlocksOfType<IMyPowerProducer>(Ñ,(ϥ)=>ϥ.BlockDefinition.TypeIdString.EndsWith("HydrogenEngine"));return;}if(Ћ=="StoreBlock"){Ǉ.
GetBlocksOfType<IMyFunctionalBlock>(Ñ,(ϥ)=>ϥ.BlockDefinition.TypeIdString.EndsWith("StoreBlock"));return;}if(Ћ=="ContractBlock"){Ǉ.
GetBlocksOfType<IMyFunctionalBlock>(Ñ,(ϥ)=>ϥ.BlockDefinition.TypeIdString.EndsWith("ContractBlock"));return;}if(Ћ=="VendingMachine"){Ǉ.
GetBlocksOfType<IMyFunctionalBlock>(Ñ,(ϥ)=>ϥ.BlockDefinition.TypeIdString.EndsWith("VendingMachine"));return;}}}public void ϰ(ref List<
IMyTerminalBlock>Ñ,string Ϯ){Ќ(ref Ñ,Ϭ(Ϯ.Trim()));}public bool ϯ(IMyTerminalBlock Ý,string Ϯ){string ϭ=Ϭ(Ϯ);switch(ϭ){case
"FunctionalBlock":return true;case"ShipController":return(Ý as IMyShipController!=null);default:return Ý.BlockDefinition.TypeIdString.
Contains(Ϭ(Ϯ));}}public string Ϭ(string ϫ){if(ϫ=="surfaceprovider")return"SurfaceProvider";if(ϫ.Ǹ("carg")||ϫ.Ǹ("conta"))return
"CargoContainer";if(ϫ.Ǹ("text")||ϫ.Ǹ("lcd"))return"TextPanel";if(ϫ.Ǹ("ass"))return"Assembler";if(ϫ.Ǹ("refi"))return"Refinery";if(ϫ.Ǹ(
"reac"))return"Reactor";if(ϫ.Ǹ("solar"))return"SolarPanel";if(ϫ.Ǹ("wind"))return"WindTurbine";if(ϫ.Ǹ("hydro")&&ϫ.Contains(
"eng"))return"HydrogenEngine";if(ϫ.Ǹ("bat"))return"BatteryBlock";if(ϫ.Ǹ("bea"))return"Beacon";if(ϫ.Ƿ("vent"))return"AirVent";
if(ϫ.Ƿ("sorter"))return"ConveyorSorter";if(ϫ.Ƿ("tank"))return"OxygenTank";if(ϫ.Ƿ("farm")&&ϫ.Ƿ("oxy"))return"OxygenFarm";if
(ϫ.Ƿ("gene")&&ϫ.Ƿ("oxy"))return"OxygenGenerator";if(ϫ.Ƿ("cryo"))return"CryoChamber";if(string.Compare(ϫ,"laserantenna",
true)==0)return"LaserAntenna";if(ϫ.Ƿ("antenna"))return"RadioAntenna";if(ϫ.Ǹ("thrust"))return"Thrust";if(ϫ.Ǹ("gyro"))return
"Gyro";if(ϫ.Ǹ("sensor"))return"SensorBlock";if(ϫ.Ƿ("connector"))return"ShipConnector";if(ϫ.Ǹ("reflector")||ϫ.Ǹ("spotlight"))
return"ReflectorLight";if((ϫ.Ǹ("inter")&&ϫ.ǵ("light")))return"InteriorLight";if(ϫ.Ǹ("land"))return"LandingGear";if(ϫ.Ǹ(
"program"))return"ProgrammableBlock";if(ϫ.Ǹ("timer"))return"TimerBlock";if(ϫ.Ǹ("motor")||ϫ.Ǹ("rotor"))return"MotorStator";if(ϫ.Ǹ(
"piston"))return"PistonBase";if(ϫ.Ǹ("proj"))return"Projector";if(ϫ.Ƿ("merge"))return"ShipMergeBlock";if(ϫ.Ǹ("sound"))return
"SoundBlock";if(ϫ.Ǹ("col"))return"Collector";if(ϫ.Ƿ("jump"))return"JumpDrive";if(string.Compare(ϫ,"door",true)==0)return"Door";if((ϫ
.Ƿ("grav")&&ϫ.Ƿ("sphe")))return"GravityGeneratorSphere";if(ϫ.Ƿ("grav"))return"GravityGenerator";if(ϫ.ǵ("drill"))return
"ShipDrill";if(ϫ.Ƿ("grind"))return"ShipGrinder";if(ϫ.ǵ("welder"))return"ShipWelder";if(ϫ.Ǹ("parach"))return"Parachute";if((ϫ.Ƿ(
"turret")&&ϫ.Ƿ("gatl")))return"LargeGatlingTurret";if((ϫ.Ƿ("turret")&&ϫ.Ƿ("inter")))return"LargeInteriorTurret";if((ϫ.Ƿ("turret"
)&&ϫ.Ƿ("miss")))return"LargeMissileTurret";if(ϫ.Ƿ("gatl"))return"SmallGatlingGun";if((ϫ.Ƿ("launcher")&&ϫ.Ƿ("reload")))
return"SmallMissileLauncherReload";if((ϫ.Ƿ("launcher")))return"SmallMissileLauncher";if(ϫ.Ƿ("mass"))return"VirtualMass";if(
string.Compare(ϫ,"warhead",true)==0)return"Warhead";if(ϫ.Ǹ("func"))return"FunctionalBlock";if(string.Compare(ϫ,"shipctrl",true
)==0)return"ShipController";if(ϫ.Ǹ("light"))return"LightingBlock";if(ϫ.Ǹ("contr"))return"ControlPanel";if(ϫ.Ǹ("coc"))
return"Cockpit";if(ϫ.Ǹ("medi"))return"MedicalRoom";if(ϫ.Ǹ("remote"))return"RemoteControl";if(ϫ.Ǹ("but"))return"ButtonPanel";if
(ϫ.Ǹ("cam"))return"CameraBlock";if(ϫ.Ƿ("detect"))return"OreDetector";if(ϫ.Ǹ("safe"))return"SafeZoneBlock";if(ϫ.Ǹ("store")
)return"StoreBlock";if(ϫ.Ǹ("contract"))return"ContractBlock";if(ϫ.Ǹ("vending"))return"VendingMachine";if(ϫ.Ǹ("decoy"))
return"Decoy";return"Unknown";}public string ϱ(IMyBatteryBlock Ň){string Ϫ="";if(Ň.ChargeMode==ChargeMode.Recharge)Ϫ="(+) ";
else if(Ň.ChargeMode==ChargeMode.Discharge)Ϫ="(-) ";else Ϫ="(±) ";return Ϫ+à.ȇ((Ň.CurrentStoredPower/Ň.MaxStoredPower)*
100.0f)+"%";}Dictionary<MyLaserAntennaStatus,string>ϩ=new Dictionary<MyLaserAntennaStatus,string>(){{MyLaserAntennaStatus.Idle
,"IDLE"},{MyLaserAntennaStatus.Connecting,"CONNECTING"},{MyLaserAntennaStatus.Connected,"CONNECTED"},{
MyLaserAntennaStatus.OutOfRange,"OUT OF RANGE"},{MyLaserAntennaStatus.RotatingToTarget,"ROTATING"},{MyLaserAntennaStatus.
SearchingTargetForAntenna,"SEARCHING"}};public string Ϩ(IMyLaserAntenna Ņ){return ϩ[Ņ.Status];}public double ϧ(IMyJumpDrive ņ,out double ʞ,out
double ƍ){ʞ=ņ.CurrentStoredPower;ƍ=ņ.MaxStoredPower;return(ƍ>0?ʞ/ƍ*100:0);}public double Ϧ(IMyJumpDrive ņ){double ʞ=ņ.
CurrentStoredPower;double ƍ=ņ.MaxStoredPower;return(ƍ>0?ʞ/ƍ*100:0);}}class Ϲ:ɔ{ɬ à;þ č;public int ϻ=0;public Ϲ(ɬ V,þ Ğ){ɓ="BootPanelsTask"
;ɐ=1;à=V;č=Ğ;if(!à.ɥ){ϻ=int.MaxValue;č.ø=true;}}ǫ ğ;public override void ɰ(){ğ=à.ğ;}public override bool ɮ(bool ò){if(ϻ>à
.ɣ.Count){ɤ();return true;}if(!ò&&ϻ==0){č.ø=false;}if(!Ϸ(ò))return false;ϻ++;return true;}public override void ɭ(){č.ø=
true;}public void Ϻ(){Ⱥ ß=č.ß;for(int X=0;X<ß.j();X++){á î=ß.b(X);î.º();}ϻ=(à.ɥ?0:int.MaxValue);}int X;ş ϸ=null;public bool
Ϸ(bool ò){Ⱥ ß=č.ß;if(!ò)X=0;int ϵ=0;for(;X<ß.j();X++){if(!ƫ.ʕ(40)||ϵ>5)return false;á î=ß.b(X);ϸ=à.Ǘ(ϸ,î);float?ϴ=î.Õ?.
FontSize;if(ϴ!=null&&ϴ>3f)continue;if(ϸ.ų.Count<=0)ϸ.ů(à.Ǚ(null,î));else à.Ǚ(ϸ.ų[0],î);à.Ş();à.ƶ(ğ.ǳ("B1"));double ʚ=(double)ϻ/à
.ɣ.Count*100;à.Ƹ(ʚ);if(ϻ==à.ɣ.Count){à.ǖ("");à.ƶ("Version 2.0186");à.ƶ("by MMaster");à.ƶ("");à.ƶ("übersetzt von Ich_73");}else à.Ǖ(à.ɣ[ϻ]);bool Ö=î.Ö;î.Ö=
false;à.Ȃ(î,ϸ);î.Ö=Ö;ϵ++;}return true;}public bool ϳ(){return ϻ<=à.ɣ.Count;}}public enum ϲ{φ=0,μ=1,ʥ=2,ˉ=3,ˈ=4,ˇ=5,ˆ=6,ˁ=7,ˀ=
8,ʿ=9,ʾ=10,ʽ=11,ʼ=12,ˊ=13,ʻ=14,ʹ=15,ʸ=16,ʷ=17,ʶ=18,ʵ=19,ʴ=20,ʳ=21,ʲ=22,ʱ=23,ʰ=24,ʺ=25,ʯ=26,ˋ=27,ʹ=28,ͳ=29,Ͳ=30,ͱ=31,}
class Ͱ{Ț ƫ;public string ˮ="";public string ˬ="";public string ˤ="";public string ˣ="";public ϲ ˢ=ϲ.φ;public Ͱ(Ț ƣ){ƫ=ƣ;}ϲ ˡ
(){if(ˮ=="echo"||ˮ=="center"||ˮ=="right")return ϲ.μ;if(ˮ.StartsWith("hscroll"))return ϲ.Ͳ;if(ˮ.StartsWith("inventory")||ˮ
.StartsWith("missing")||ˮ.StartsWith("invlist"))return ϲ.ʥ;if(ˮ.StartsWith("working"))return ϲ.ʶ;if(ˮ.StartsWith("cargo")
)return ϲ.ˉ;if(ˮ.StartsWith("mass"))return ϲ.ˈ;if(ˮ.StartsWith("shipmass"))return ϲ.ʱ;if(ˮ=="oxygen")return ϲ.ˇ;if(ˮ.
StartsWith("tanks"))return ϲ.ˆ;if(ˮ.StartsWith("powertime"))return ϲ.ˁ;if(ˮ.StartsWith("powerused"))return ϲ.ˀ;if(ˮ.StartsWith(
"power"))return ϲ.ʿ;if(ˮ.StartsWith("speed"))return ϲ.ʾ;if(ˮ.StartsWith("accel"))return ϲ.ʽ;if(ˮ.StartsWith("alti"))return ϲ.ʺ;
if(ˮ.StartsWith("charge"))return ϲ.ʼ;if(ˮ.StartsWith("docked"))return ϲ.ͱ;if(ˮ.StartsWith("time")||ˮ.StartsWith("date"))
return ϲ.ˊ;if(ˮ.StartsWith("countdown"))return ϲ.ʻ;if(ˮ.StartsWith("textlcd"))return ϲ.ʹ;if(ˮ.EndsWith("count"))return ϲ.ʸ;if(
ˮ.StartsWith("dampeners")||ˮ.StartsWith("occupied"))return ϲ.ʷ;if(ˮ.StartsWith("damage"))return ϲ.ʵ;if(ˮ.StartsWith(
"amount"))return ϲ.ʴ;if(ˮ.StartsWith("pos"))return ϲ.ʳ;if(ˮ.StartsWith("distance"))return ϲ.ʰ;if(ˮ.StartsWith("details"))return
ϲ.ʲ;if(ˮ.StartsWith("stop"))return ϲ.ʯ;if(ˮ.StartsWith("gravity"))return ϲ.ˋ;if(ˮ.StartsWith("customdata"))return ϲ.ʹ;if(
ˮ.StartsWith("prop"))return ϲ.ͳ;return ϲ.φ;}public Ɯ ˠ(){switch(ˢ){case ϲ.μ:return new Ҵ();case ϲ.ʥ:return new ѿ();case ϲ
.ˉ:return new κ();case ϲ.ˈ:return new Һ();case ϲ.ˇ:return new ҹ();case ϲ.ˆ:return new Ѫ();case ϲ.ˁ:return new ь();case ϲ.
ˀ:return new Э();case ϲ.ʿ:return new Ӄ();case ϲ.ʾ:return new ћ();case ϲ.ʽ:return new ʢ();case ϲ.ʼ:return new Ϋ();case ϲ.ˊ
:return new Θ();case ϲ.ʻ:return new ΐ();case ϲ.ʹ:return new ĵ();case ϲ.ʸ:return new ʗ();case ϲ.ʷ:return new ѡ();case ϲ.ʶ:
return new Ļ();case ϲ.ʵ:return new ͺ();case ϲ.ʴ:return new Ә();case ϲ.ʳ:return new Ӆ();case ϲ.ʲ:return new Δ();case ϲ.ʱ:return
new џ();case ϲ.ʰ:return new ҧ();case ϲ.ʺ:return new ʙ();case ϲ.ʯ:return new ј();case ϲ.ˋ:return new ҳ();case ϲ.ʹ:return new
ͽ();case ϲ.ͳ:return new Ѽ();case ϲ.Ͳ:return new Ҳ();case ϲ.ͱ:return new Ү();default:return new Ɯ();}}public List<ʮ>ˑ=new
List<ʮ>();string[]ː=null;string ˏ="";bool ˎ=false;int Ŗ=1;public bool ʟ(string ˍ,bool ò){if(!ò){ˢ=ϲ.φ;ˬ="";ˮ="";ˤ=ˍ.
TrimStart(' ');ˑ.Clear();if(ˤ=="")return true;int ˌ=ˤ.IndexOf(' ');if(ˌ<0||ˌ>=ˤ.Length-1)ˣ="";else ˣ=ˤ.Substring(ˌ+1);ː=ˤ.Split(
' ');ˏ="";ˎ=false;ˮ=ː[0].ToLower();Ŗ=1;}for(;Ŗ<ː.Length;Ŗ++){if(!ƫ.ʕ(40))return false;string Ŕ=ː[Ŗ];if(Ŕ=="")continue;if(Ŕ[
0]=='{'&&Ŕ[Ŕ.Length-1]=='}'){Ŕ=Ŕ.Substring(1,Ŕ.Length-2);if(Ŕ=="")continue;if(ˬ=="")ˬ=Ŕ;else ˑ.Add(new ʮ(Ŕ));continue;}if
(Ŕ[0]=='{'){ˎ=true;ˏ=Ŕ.Substring(1);continue;}if(Ŕ[Ŕ.Length-1]=='}'){ˎ=false;ˏ+=' '+Ŕ.Substring(0,Ŕ.Length-1);if(ˬ=="")ˬ=
ˏ;else ˑ.Add(new ʮ(ˏ));continue;}if(ˎ){if(ˏ.Length!=0)ˏ+=' ';ˏ+=Ŕ;continue;}if(ˬ=="")ˬ=Ŕ;else ˑ.Add(new ʮ(Ŕ));}ˢ=ˡ();
return true;}}class ʮ{public string ʣ="";public string ʖ="";public string Ŕ="";public List<string>ʡ=new List<string>();public
ʮ(string ʠ){Ŕ=ʠ;}public void ʟ(){if(Ŕ==""||ʣ!=""||ʖ!=""||ʡ.Count>0)return;string ʞ=Ŕ.Trim();if(ʞ[0]=='+'||ʞ[0]=='-'){ʣ+=ʞ
[0];ʞ=Ŕ.Substring(1);}string[]Ơ=ʞ.Split('/');string ʝ=Ơ[0];if(Ơ.Length>1){ʖ=Ơ[0];ʝ=Ơ[1];}else ʖ="";if(ʝ.Length>0){string[
]Ą=ʝ.Split(',');for(int X=0;X<Ą.Length;X++)if(Ą[X]!="")ʡ.Add(Ą[X]);}}}class ʢ:Ɯ{public ʢ(){ɐ=0.5;ɓ="CmdAccel";}public
override bool Ƒ(bool ò){double ʛ=0;if(Ɣ.ˬ!="")double.TryParse(Ɣ.ˬ.Trim(),out ʛ);à.Ë(ğ.ǳ("AC1")+" ");à.Ƶ(à.ǈ.ʀ.ToString("F1")+
" m/s²");if(ʛ>0){double ʚ=à.ǈ.ʀ/ʛ*100;à.Ƹ(ʚ);}return true;}}class ʙ:Ɯ{public ʙ(){ɐ=1;ɓ="CmdAltitude";}public override bool Ƒ(
bool ò){string ʘ=(Ɣ.ˮ.EndsWith("sea")?"sea":"ground");switch(ʘ){case"sea":à.Ë(ğ.ǳ("ALT1"));à.Ƶ(à.ǈ.ɶ.ToString("F0")+" m");
break;default:à.Ë(ğ.ǳ("ALT2"));à.Ƶ(à.ǈ.ɴ.ToString("F0")+" m");break;}return true;}}class ʗ:Ɯ{public ʗ(){ɐ=15;ɓ=
"CmdBlockCount";}ο ō;public override void ɰ(){ō=new ο(ƫ,à.ŋ);}bool ʜ;bool ʤ;int Ŗ=0;int ĕ=0;public override bool Ƒ(bool ò){if(!ò){ʜ=(Ɣ.
ˮ=="enabledcount");ʤ=(Ɣ.ˮ=="prodcount");Ŗ=0;ĕ=0;}if(Ɣ.ˑ.Count==0){if(ĕ==0){if(!ò)ō.Ů();if(!ō.Ϥ(Ɣ.ˬ,ò))return false;ĕ++;ò=
false;}if(!ʧ(ō,"blocks",ʜ,ʤ,ò))return false;return true;}for(;Ŗ<Ɣ.ˑ.Count;Ŗ++){ʮ Ŕ=Ɣ.ˑ[Ŗ];if(!ò)Ŕ.ʟ();if(!Ŏ(Ŕ,ò))return false
;ò=false;}return true;}int Œ=0;int œ=0;bool Ŏ(ʮ Ŕ,bool ò){if(!ò){Œ=0;œ=0;}for(;Œ<Ŕ.ʡ.Count;Œ++){if(œ==0){if(!ò)ō.Ů();if(!
ō.Ͻ(Ŕ.ʡ[Œ],Ɣ.ˬ,ò))return false;œ++;ò=false;}if(!ʧ(ō,Ŕ.ʡ[Œ],ʜ,ʤ,ò))return false;œ=0;ò=false;}return true;}Dictionary<
string,int>ʭ=new Dictionary<string,int>();Dictionary<string,int>ʬ=new Dictionary<string,int>();List<string>ʫ=new List<string>(
);int ć=0;int ʪ=0;int ʩ=0;ʐ ʨ=new ʐ();bool ʧ(ο Ñ,string ʘ,bool ʜ,bool ʤ,bool ò){if(Ñ.Г()==0){ʨ.Ů().ɱ(char.ToUpper(ʘ[0])).
ɱ(ʘ.ToLower(),1,ʘ.Length-1);à.Ë(ʨ.ɱ(" ").ɱ(ğ.ǳ("C1")).ɱ(" "));string ʦ=(ʜ||ʤ?"0 / 0":"0");à.Ƶ(ʦ);return true;}if(!ò){ʭ.
Clear();ʬ.Clear();ʫ.Clear();ć=0;ʪ=0;ʩ=0;}if(ʩ==0){for(;ć<Ñ.Г();ć++){if(!ƫ.ʕ(15))return false;IMyProductionBlock Ŋ=Ñ.υ[ć]as
IMyProductionBlock;ʨ.Ů().ɱ(Ñ.υ[ć].DefinitionDisplayNameText);string Ȁ=ʨ.ɖ();if(ʫ.Contains(Ȁ)){ʭ[Ȁ]++;if((ʜ&&Ñ.υ[ć].IsWorking)||(ʤ&&Ŋ!=null
&&Ŋ.IsProducing))ʬ[Ȁ]++;}else{ʭ.Add(Ȁ,1);ʫ.Add(Ȁ);if(ʜ||ʤ)if((ʜ&&Ñ.υ[ć].IsWorking)||(ʤ&&Ŋ!=null&&Ŋ.IsProducing))ʬ.Add(Ȁ,1)
;else ʬ.Add(Ȁ,0);}}ʩ++;ò=false;}for(;ʪ<ʭ.Count;ʪ++){if(!ƫ.ʕ(8))return false;à.Ë(ʫ[ʪ]+" "+ğ.ǳ("C1")+" ");string ʦ=(ʜ||ʤ?ʬ[
ʫ[ʪ]]+" / ":"")+ʭ[ʫ[ʪ]];à.Ƶ(ʦ);}return true;}}class κ:Ɯ{ο ō;public κ(){ɐ=2;ɓ="CmdCargo";}public override void ɰ(){ō=new ο
(ƫ,à.ŋ);}bool Ϊ=true;bool ͷ=false;bool Ω=false;bool Σ=false;double Ψ=0;double Χ=0;int ĕ=0;public override bool Ƒ(bool ò){
if(!ò){ō.Ů();Ϊ=Ɣ.ˮ.Contains("all");Σ=Ɣ.ˮ.EndsWith("bar");ͷ=(Ɣ.ˮ[Ɣ.ˮ.Length-1]=='x');Ω=(Ɣ.ˮ[Ɣ.ˮ.Length-1]=='p');Ψ=0;Χ=0;ĕ=0
;}if(ĕ==0){if(Ϊ){if(!ō.Ϥ(Ɣ.ˬ,ò))return false;}else{if(!ō.Ͻ("cargocontainer",Ɣ.ˬ,ò))return false;}ĕ++;ò=false;}double Φ=ō.
ϕ(ref Ψ,ref Χ,ò);if(Double.IsNaN(Φ))return false;if(Σ){à.Ƹ(Φ);return true;}à.Ë(ğ.ǳ("C2")+" ");if(!ͷ&&!Ω){à.Ƶ(à.Ȑ(Ψ)+
"L / "+à.Ȑ(Χ)+"L");à.ƿ(Φ,1.0f,à.Ʒ);à.ǖ(' '+à.ȇ(Φ)+"%");}else if(Ω){à.Ƶ(à.ȇ(Φ)+"%");à.Ƹ(Φ);}else à.Ƶ(à.ȇ(Φ)+"%");return true;}}
class Ϋ:Ɯ{public Ϋ(){ɐ=3;ɓ="CmdCharge";}ο ō;bool ͷ=false;bool Τ=false;bool Σ=false;bool Ρ=false;public override void ɰ(){ō=
new ο(ƫ,à.ŋ);if(Ɣ.ˑ.Count>0)Υ=Ɣ.ˑ[0].Ŕ;Σ=Ɣ.ˮ.EndsWith("bar");ͷ=Ɣ.ˮ.Contains("x");Τ=Ɣ.ˮ.Contains("time");Ρ=Ɣ.ˮ.Contains(
"sum");}int ĕ=0;int ć=0;double Π=0;double Ο=0;TimeSpan Ξ=TimeSpan.Zero;string Υ="";Dictionary<long,double>ě=new Dictionary<
long,double>();Dictionary<long,double>ά=new Dictionary<long,double>();Dictionary<long,double>λ=new Dictionary<long,double>()
;Dictionary<long,double>ι=new Dictionary<long,double>();Dictionary<long,double>θ=new Dictionary<long,double>();double η(
long ζ,double ʞ,double ƍ){double ε=0;double δ=0;double γ=0;double β=0;if(ά.TryGetValue(ζ,out γ)){β=ι[ζ];}if(ě.TryGetValue(ζ,
out ε)){δ=λ[ζ];}double α=(ƫ.Ȗ-γ);double ΰ=0;if(α>0)ΰ=(ʞ-β)/α;if(ΰ<0){if(!θ.TryGetValue(ζ,out ΰ))ΰ=0;}else θ[ζ]=ΰ;if(ε>0){ά[
ζ]=ě[ζ];ι[ζ]=λ[ζ];}ě[ζ]=ƫ.Ȗ;λ[ζ]=ʞ;return(ΰ>0?(ƍ-ʞ)/ΰ:0);}private void ί(string Ȁ,double ʚ,double ʞ,double ƍ,TimeSpan ή){
if(Σ){à.Ƹ(ʚ);}else{à.Ë(Ȁ+" ");if(Τ){à.Ƶ(à.ǉ.ȣ(ή));if(!ͷ){à.ƿ(ʚ,1.0f,à.Ʒ);à.Ƶ(' '+ʚ.ToString("0.0")+"%");}}else{if(!ͷ){à.Ƶ(
à.Ȑ(ʞ)+"Wh / "+à.Ȑ(ƍ)+"Wh");à.ƿ(ʚ,1.0f,à.Ʒ);}à.Ƶ(' '+ʚ.ToString("0.0")+"%");}}}public override bool Ƒ(bool ò){if(!ò){ō.Ů(
);ć=0;ĕ=0;Π=0;Ο=0;Ξ=TimeSpan.Zero;}if(ĕ==0){if(!ō.Ͻ("jumpdrive",Ɣ.ˬ,ò))return false;if(ō.Г()<=0){à.ǖ("Charge: "+ğ.ǳ("D2")
);return true;}ĕ++;ò=false;}for(;ć<ō.Г();ć++){if(!ƫ.ʕ(25))return false;IMyJumpDrive ņ=ō.υ[ć]as IMyJumpDrive;double ʞ,ƍ,ʚ;
ʚ=à.ŋ.ϧ(ņ,out ʞ,out ƍ);TimeSpan έ;if(Τ)έ=TimeSpan.FromSeconds(η(ņ.EntityId,ʞ,ƍ));else έ=TimeSpan.Zero;if(!Ρ){ί(ņ.
CustomName,ʚ,ʞ,ƍ,έ);}else{Π+=ʞ;Ο+=ƍ;if(Ξ<έ)Ξ=έ;}}if(Ρ){double Ν=(Ο>0?Π/Ο*100:0);ί(Υ,Ν,Π,Ο,Ξ);}return true;}}class ΐ:Ɯ{public ΐ(){ɐ
=1;ɓ="CmdCountdown";}public override bool Ƒ(bool ò){bool Ώ=Ɣ.ˮ.EndsWith("c");bool Ύ=Ɣ.ˮ.EndsWith("r");string Ό="";int Ί=Ɣ
.ˤ.IndexOf(' ');if(Ί>=0)Ό=Ɣ.ˤ.Substring(Ί+1).Trim();DateTime Ή=DateTime.Now;DateTime Έ;if(!DateTime.TryParseExact(Ό,
"H:mm d.M.yyyy",System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None,out Έ)){à.ǖ(ğ.ǳ("C3"));à.ǖ(
"  Countdown 19:02 28.2.2015");return true;}TimeSpan Ά=Έ-Ή;string Ĵ="";if(Ά.Ticks<=0)Ĵ=ğ.ǳ("C4");else{if((int)Ά.TotalDays>0)Ĵ+=(int)Ά.TotalDays+" "+ğ
.ǳ("C5")+" ";if(Ά.Hours>0||Ĵ!="")Ĵ+=Ά.Hours+"h ";if(Ά.Minutes>0||Ĵ!="")Ĵ+=Ά.Minutes+"m ";Ĵ+=Ά.Seconds+"s";}if(Ώ)à.ƶ(Ĵ);
else if(Ύ)à.Ƶ(Ĵ);else à.ǖ(Ĵ);return true;}}class ͽ:Ɯ{public ͽ(){ɐ=1;ɓ="CmdCustomData";}public override bool Ƒ(bool ò){string
Ĵ="";if(Ɣ.ˬ!=""&&Ɣ.ˬ!="*"){IMyTerminalBlock ͻ=à.Ǉ.GetBlockWithName(Ɣ.ˬ)as IMyTerminalBlock;if(ͻ==null){à.ǖ("CustomData: "
+ğ.ǳ("CD1")+Ɣ.ˬ);return true;}Ĵ=ͻ.CustomData;}else{à.ǖ("CustomData:"+ğ.ǳ("CD2"));return true;}if(Ĵ.Length==0)return true;
à.Ǖ(Ĵ);return true;}}class ͺ:Ɯ{public ͺ(){ɐ=5;ɓ="CmdDamage";}ο ō;public override void ɰ(){ō=new ο(ƫ,à.ŋ);}bool ƃ=false;
int ć=0;public override bool Ƒ(bool ò){bool ͷ=Ɣ.ˮ.StartsWith("damagex");bool Ͷ=Ɣ.ˮ.EndsWith("noc");bool ͼ=(!Ͷ&&Ɣ.ˮ.EndsWith
("c"));float Α=100;if(!ò){ō.Ů();ƃ=false;ć=0;}if(!ō.Ϥ(Ɣ.ˬ,ò))return false;if(Ɣ.ˑ.Count>0){if(!float.TryParse(Ɣ.ˑ[0].Ŕ,out
Α))Α=100;}Α-=0.00001f;for(;ć<ō.Г();ć++){if(!ƫ.ʕ(30))return false;IMyTerminalBlock Ý=ō.υ[ć];IMySlimBlock Μ=Ý.CubeGrid.
GetCubeBlock(Ý.Position);if(Μ==null)continue;float Κ=(Ͷ?Μ.MaxIntegrity:Μ.BuildIntegrity);if(!ͼ)Κ-=Μ.CurrentDamage;float ʚ=100*(Κ/Μ.
MaxIntegrity);if(ʚ>=Α)continue;ƃ=true;string Ι=à.Ǧ(Μ.FatBlock.DisplayNameText,à.ɞ*0.69f-à.Ʒ);à.Ë(Ι+' ');if(!ͷ){à.Ʋ(à.Ȑ(Κ)+" / ",
0.69f);à.Ë(à.Ȑ(Μ.MaxIntegrity));}à.Ƶ(' '+ʚ.ToString("0.0")+'%');à.Ƹ(ʚ);}if(!ƃ)à.ǖ(ğ.ǳ("D3"));return true;}}class Θ:Ɯ{public Θ
(){ɐ=1;ɓ="CmdDateTime";}public override bool Ƒ(bool ò){bool Η=(Ɣ.ˮ.StartsWith("datetime"));bool Ζ=(Ɣ.ˮ.StartsWith("date")
);bool Ώ=Ɣ.ˮ.Contains("c");int Ε=Ɣ.ˮ.IndexOf('+');if(Ε<0)Ε=Ɣ.ˮ.IndexOf('-');float Λ=0;if(Ε>=0)float.TryParse(Ɣ.ˮ.
Substring(Ε),out Λ);DateTime Ά=DateTime.Now.AddHours(Λ);string Ĵ="";int Ί=Ɣ.ˤ.IndexOf(' ');if(Ί>=0)Ĵ=Ɣ.ˤ.Substring(Ί+1);if(!Η){if
(!Ζ)Ĵ+=Ά.ToShortTimeString();else Ĵ+=Ά.ToShortDateString();}else{if(Ĵ=="")Ĵ=String.Format("{0:d} {0:t}",Ά);else{Ĵ=Ĵ.
Replace("/","\\/");Ĵ=Ĵ.Replace(":","\\:");Ĵ=Ĵ.Replace("\"","\\\"");Ĵ=Ĵ.Replace("'","\\'");Ĵ=Ά.ToString(Ĵ+' ');Ĵ=Ĵ.Substring(0,Ĵ
.Length-1);}}if(Ώ)à.ƶ(Ĵ);else à.ǖ(Ĵ);return true;}}class Δ:Ɯ{public Δ(){ɐ=5;ɓ="CmdDetails";}string Γ="";string Β="";int ř
=0;ο ō;public override void ɰ(){ō=new ο(ƫ,à.ŋ);if(Ɣ.ˑ.Count>0)Γ=Ɣ.ˑ[0].Ŕ.Trim();if(Ɣ.ˑ.Count>1){string Ŕ=Ɣ.ˑ[1].Ŕ.Trim();
if(!int.TryParse(Ŕ,out ř)){ř=0;Β=Ŕ;}}}int ĕ=0;int ć=1;bool Ж=false;IMyTerminalBlock Ý;public override bool Ƒ(bool ò){if(Ɣ.
ˬ==""||Ɣ.ˬ=="*"){à.ǖ("Details: "+ğ.ǳ("D1"));return true;}if(!ò){ō.Ů();Ж=Ɣ.ˮ.Contains("non");ĕ=0;ć=1;}if(ĕ==0){if(!ō.Ϥ(Ɣ.ˬ
,ò))return true;if(ō.Г()<=0){à.ǖ("Details: "+ğ.ǳ("D2"));return true;}ĕ++;ò=false;}int ҭ=(Ɣ.ˮ.EndsWith("x")?1:0);if(ĕ==1){
if(!ò){Ý=ō.υ[0];if(!Ж)à.ǖ(Ý.CustomName);}if(!ҩ(Ý,ҭ,ř,ò))return false;ĕ++;ò=false;}for(;ć<ō.Г();ć++){if(!ò){Ý=ō.υ[ć];if(!Ж)
{à.ǖ("");à.ǖ(Ý.CustomName);}}if(!ҩ(Ý,ҭ,ř,ò))return false;ò=false;}return true;}string[]ŧ;int Ҭ=0;int ҫ=0;bool Ҫ=false;ʐ ƴ
=new ʐ();bool ҩ(IMyTerminalBlock Ý,int Ҩ,int Ĺ,bool ò){if(!ò){ŧ=ƴ.Ů().ɱ(Ý.DetailedInfo).ɱ('\n').ɱ(Ý.CustomInfo).ɖ().Split
('\n');Ҭ=Ҩ;Ҫ=(Γ.Length==0);ҫ=0;}for(;Ҭ<ŧ.Length;Ҭ++){if(!ƫ.ʕ(5))return false;if(ŧ[Ҭ].Length==0)continue;if(!Ҫ){if(!ŧ[Ҭ].
Contains(Γ))continue;Ҫ=true;}if(Β.Length>0&&ŧ[Ҭ].Contains(Β))return true;à.ǖ(ƴ.Ů().ɱ("  ").ɱ(ŧ[Ҭ]));ҫ++;if(Ĺ>0&&ҫ>=Ĺ)return true
;}return true;}}class ҧ:Ɯ{public ҧ(){ɐ=1;ɓ="CmdDistance";}string Ҧ="";string[]ҥ;Vector3D Ҥ;string ң="";bool Ң=false;
public override void ɰ(){Ң=false;if(Ɣ.ˑ.Count<=0)return;Ҧ=Ɣ.ˑ[0].Ŕ.Trim();ҥ=Ҧ.Split(':');if(ҥ.Length<5||ҥ[0]!="GPS")return;
double ҡ,Ҡ,ҟ;if(!double.TryParse(ҥ[2],out ҡ))return;if(!double.TryParse(ҥ[3],out Ҡ))return;if(!double.TryParse(ҥ[4],out ҟ))
return;Ҥ=new Vector3D(ҡ,Ҡ,ҟ);ң=ҥ[1];Ң=true;}public override bool Ƒ(bool ò){if(!Ң){à.ǖ("Distance: "+ğ.ǳ("DTU")+" '"+Ҧ+"'.");
return true;}IMyTerminalBlock Ý=Ğ.o.Ý;if(Ɣ.ˬ!=""&&Ɣ.ˬ!="*"){Ý=à.Ǉ.GetBlockWithName(Ɣ.ˬ);if(Ý==null){à.ǖ("Distance: "+ğ.ǳ("P1")
+": "+Ɣ.ˬ);return true;}}double Ѧ=Vector3D.Distance(Ý.GetPosition(),Ҥ);à.Ë(ң+": ");à.Ƶ(à.Ȑ(Ѧ)+"m ");return true;}}class Ү
:Ɯ{ο ō;public Ү(){ɐ=2;ɓ="CmdDocked";}public override void ɰ(){ō=new ο(ƫ,à.ŋ);}int ĕ=0;int ҷ=0;bool Ҷ=false;bool ҵ=false;
IMyShipConnector ŷ;public override bool Ƒ(bool ò){if(!ò){if(Ɣ.ˮ.EndsWith("e"))Ҷ=true;if(Ɣ.ˮ.Contains("cn"))ҵ=true;ō.Ů();ĕ=0;}if(ĕ==0){if
(!ō.Ͻ("connector",Ɣ.ˬ,ò))return false;ĕ++;ҷ=0;ò=false;}if(ō.Г()<=0){à.ǖ("Docked: "+ğ.ǳ("DO1"));return true;}for(;ҷ<ō.Г();
ҷ++){ŷ=ō.υ[ҷ]as IMyShipConnector;if(ŷ.Status==MyShipConnectorStatus.Connected){if(ҵ){à.Ë(ŷ.CustomName+":");à.Ƶ(ŷ.
OtherConnector.CubeGrid.CustomName);}else{à.ǖ(ŷ.OtherConnector.CubeGrid.CustomName);}}else{if(Ҷ){if(ҵ){à.Ë(ŷ.CustomName+":");à.Ƶ("-");
}else à.ǖ("-");}}}return true;}}class Ҵ:Ɯ{public Ҵ(){ɐ=30;ɓ="CmdEcho";}public override bool Ƒ(bool ò){string ʘ=(Ɣ.ˮ==
"center"?"c":(Ɣ.ˮ=="right"?"r":"n"));switch(ʘ){case"c":à.ƶ(Ɣ.ˣ);break;case"r":à.Ƶ(Ɣ.ˣ);break;default:à.ǖ(Ɣ.ˣ);break;}return true
;}}class ҳ:Ɯ{public ҳ(){ɐ=1;ɓ="CmdGravity";}public override bool Ƒ(bool ò){string ʘ=(Ɣ.ˮ.Contains("nat")?"n":(Ɣ.ˮ.
Contains("art")?"a":(Ɣ.ˮ.Contains("tot")?"t":"s")));Vector3D Ѐ;if(à.ǈ.ɲ==null){à.ǖ("Gravity: "+ğ.ǳ("GNC"));return true;}switch(ʘ
){case"n":à.Ë(ğ.ǳ("G2")+" ");Ѐ=à.ǈ.ɲ.GetNaturalGravity();à.Ƶ(Ѐ.Length().ToString("F1")+" m/s²");break;case"a":à.Ë(ğ.ǳ(
"G3")+" ");Ѐ=à.ǈ.ɲ.GetArtificialGravity();à.Ƶ(Ѐ.Length().ToString("F1")+" m/s²");break;case"t":à.Ë(ğ.ǳ("G1")+" ");Ѐ=à.ǈ.ɲ.
GetTotalGravity();à.Ƶ(Ѐ.Length().ToString("F1")+" m/s²");break;default:à.Ë(ğ.ǳ("GN"));à.Ʋ(" | ",0.33f);à.Ʋ(ğ.ǳ("GA")+" | ",0.66f);à.Ƶ(ğ
.ǳ("GT"),1.0f);à.Ë("");Ѐ=à.ǈ.ɲ.GetNaturalGravity();à.Ʋ(Ѐ.Length().ToString("F1")+" | ",0.33f);Ѐ=à.ǈ.ɲ.
GetArtificialGravity();à.Ʋ(Ѐ.Length().ToString("F1")+" | ",0.66f);Ѐ=à.ǈ.ɲ.GetTotalGravity();à.Ƶ(Ѐ.Length().ToString("F1")+" ");break;}return
true;}}class Ҳ:Ɯ{public Ҳ(){ɐ=0.5;ɓ="CmdHScroll";}ʐ ұ=new ʐ();int Ұ=1;public override bool Ƒ(bool ò){if(ұ.ʎ==0){string Ĵ=Ɣ.ˣ
+"  ";if(Ĵ.Length==0)return true;float ү=à.ɞ;float Ư=à.Ǩ(Ĵ,à.Ǔ);float њ=ү/Ư;if(њ>1)ұ.ɱ(string.Join("",Enumerable.Repeat(Ĵ
,(int)Math.Ceiling(њ))));else ұ.ɱ(Ĵ);if(Ĵ.Length>40)Ұ=3;else if(Ĵ.Length>5)Ұ=2;else Ұ=1;à.ǖ(ұ);return true;}bool Ύ=Ɣ.ˮ.
EndsWith("r");if(Ύ){ұ.ƴ.Insert(0,ұ.ɖ(ұ.ʎ-Ұ,Ұ));ұ.ɗ(ұ.ʎ-Ұ,Ұ);}else{ұ.ɱ(ұ.ɖ(0,Ұ));ұ.ɗ(0,Ұ);}à.ǖ(ұ);return true;}}class ѿ:Ɯ{public
ѿ(){ɐ=7;ɓ="CmdInvList";}float Ғ=-1;float ґ=-1;public override void ɰ(){ō=new ο(ƫ,à.ŋ);ҙ=new Ɛ(ƫ,à);}ʐ ƴ=new ʐ(100);
Dictionary<string,string>Ґ=new Dictionary<string,string>();void ҏ(string ȭ,double ҍ,int Ê){if(Ê>0){if(!Ҹ)à.ƿ(Math.Min(100,100*ҍ/Ê)
,0.3f);string Ι;if(Ґ.ContainsKey(ȭ)){Ι=Ґ[ȭ];}else{if(!ҕ)Ι=à.Ǧ(ȭ,à.ɞ*0.5f-ҋ-ґ);else{if(!Ҹ)Ι=à.Ǧ(ȭ,à.ɞ*0.5f);else Ι=à.Ǧ(ȭ,à
.ɞ*0.9f);}Ґ[ȭ]=Ι;}ƴ.Ů();if(!Ҹ)ƴ.ɱ(' ');if(!ҕ){à.Ë(ƴ.ɱ(Ι).ɱ(' '));à.Ʋ(à.Ȑ(ҍ),1.0f,ҋ+ґ);à.ǖ(ƴ.Ů().ɱ(" / ").ɱ(à.Ȑ(Ê)));}else
{à.ǖ(ƴ.ɱ(Ι));}}else{if(!ҕ){à.Ë(ƴ.Ů().ɱ(ȭ).ɱ(':'));à.Ƶ(à.Ȑ(ҍ),1.0f,Ғ);}else à.ǖ(ƴ.Ů().ɱ(ȭ));}}void ғ(string ȭ,double ҍ,
double Ҍ,int Ê){if(Ê>0){if(!ҕ){à.Ë(ƴ.Ů().ɱ(ȭ).ɱ(' '));à.Ʋ(à.Ȑ(ҍ),0.51f);à.Ë(ƴ.Ů().ɱ(" / ").ɱ(à.Ȑ(Ê)));à.Ƶ(ƴ.Ů().ɱ(" +").ɱ(à.Ȑ(
Ҍ)).ɱ(" ").ɱ(ğ.ǳ("I1")),1.0f);}else à.ǖ(ƴ.Ů().ɱ(ȭ));if(!Ҹ)à.Ƹ(Math.Min(100,100*ҍ/Ê));}else{if(!ҕ){à.Ë(ƴ.Ů().ɱ(ȭ).ɱ(':'));
à.Ʋ(à.Ȑ(ҍ),0.51f);à.Ƶ(ƴ.Ů().ɱ(" +").ɱ(à.Ȑ(Ҍ)).ɱ(" ").ɱ(ğ.ǳ("I1")),1.0f);}else{à.ǖ(ƴ.Ů().ɱ(ȭ));}}}float ҋ=0;bool Ҋ(Ə ƀ){
int Ê=(җ?ƀ.Ǝ:ƀ.ƍ);if(Ê<0)return true;float ƻ=à.Ǩ(à.Ȑ(Ê),à.Ǔ);if(ƻ>ҋ)ҋ=ƻ;return true;}List<Ə>ҁ;int Ҁ=0;int Ҏ=0;bool Ҕ(bool ò
,bool Ҟ,string Â,string Ь){if(!ò){Ҏ=0;Ҁ=0;}if(Ҏ==0){if(ӌ){if((ҁ=ҙ.ż(Â,ò,Ҋ))==null)return false;}else{if((ҁ=ҙ.ż(Â,ò))==
null)return false;}Ҏ++;ò=false;}if(ҁ.Count>0){if(!Ҟ&&!ò){if(!à.Ǜ)à.ǖ();à.ƶ(ƴ.Ů().ɱ("<< ").ɱ(Ь).ɱ(" ").ɱ(ğ.ǳ("I2")).ɱ(" >>"))
;}for(;Ҁ<ҁ.Count;Ҁ++){if(!ƫ.ʕ(30))return false;double ҍ=ҁ[Ҁ].Ƌ;if(җ&&ҍ>=ҁ[Ҁ].Ǝ)continue;int Ê=ҁ[Ҁ].ƍ;if(җ)Ê=ҁ[Ҁ].Ǝ;string
ȭ=à.ǽ(ҁ[Ҁ].Ã,ҁ[Ҁ].Â);ҏ(ȭ,ҍ,Ê);}}return true;}List<Ə>ҝ;int Ҝ=0;int қ=0;bool Қ(bool ò){if(!ò){Ҝ=0;қ=0;}if(қ==0){if((ҝ=ҙ.ż(
"Ingot",ò))==null)return false;қ++;ò=false;}if(ҝ.Count>0){if(!Җ&&!ò){if(!à.Ǜ)à.ǖ();à.ƶ(ƴ.Ů().ɱ("<< ").ɱ(ğ.ǳ("I4")).ɱ(" ").ɱ(ğ.ǳ
("I2")).ɱ(" >>"));}for(;Ҝ<ҝ.Count;Ҝ++){if(!ƫ.ʕ(40))return false;double ҍ=ҝ[Ҝ].Ƌ;if(җ&&ҍ>=ҝ[Ҝ].Ǝ)continue;int Ê=ҝ[Ҝ].ƍ;if(
җ)Ê=ҝ[Ҝ].Ǝ;string ȭ=à.ǽ(ҝ[Ҝ].Ã,ҝ[Ҝ].Â);if(ҝ[Ҝ].Ã!="Scrap"){double Ҍ=ҙ.Ɓ(ҝ[Ҝ].Ã+" Ore",ҝ[Ҝ].Ã,"Ore").Ƌ;ғ(ȭ,ҍ,Ҍ,Ê);}else ҏ(
ȭ,ҍ,Ê);}}return true;}ο ō=null;Ɛ ҙ;List<ʮ>ˑ;bool Ҙ,ͷ,җ,Җ,ҕ,Ҹ;int Ŗ,Œ;string ӎ="";float Ӎ=0;bool ӌ=true;void Ӌ(){if(à.Ǔ!=ӎ
||Ӎ!=à.ɞ){Ґ.Clear();Ӎ=à.ɞ;}if(à.Ǔ!=ӎ){ґ=à.Ǩ(" / ",à.Ǔ);Ғ=à.ǻ(' ',à.Ǔ);ӎ=à.Ǔ;}ō.Ů();Ҙ=Ɣ.ˮ.EndsWith("x")||Ɣ.ˮ.EndsWith("xs")
;ͷ=Ɣ.ˮ.EndsWith("s")||Ɣ.ˮ.EndsWith("sx");җ=Ɣ.ˮ.StartsWith("missing");Җ=Ɣ.ˮ.Contains("list");Ҹ=Ɣ.ˮ.Contains("nb");ҕ=Ɣ.ˮ.
Contains("nn");ҙ.Ů();ˑ=Ɣ.ˑ;if(ˑ.Count==0)ˑ.Add(new ʮ("all"));}bool ӊ(bool ò){if(!ò)Ŗ=0;for(;Ŗ<ˑ.Count;Ŗ++){ʮ Ŕ=ˑ[Ŗ];Ŕ.ʟ();string
Â=Ŕ.ʖ;if(!ò)Œ=0;else ò=false;for(;Œ<Ŕ.ʡ.Count;Œ++){if(!ƫ.ʕ(30))return false;string[]Ą=Ŕ.ʡ[Œ].Split(':');double Ȏ;if(
string.Compare(Ą[0],"all",true)==0)Ą[0]="";int Ǝ=1;int ƍ=-1;if(Ą.Length>1){if(Double.TryParse(Ą[1],out Ȏ)){if(җ)Ǝ=(int)Math.
Ceiling(Ȏ);else ƍ=(int)Math.Ceiling(Ȏ);}}string ơ=Ą[0];if(!string.IsNullOrEmpty(Â))ơ+=' '+Â;ҙ.Ƣ(ơ,Ŕ.ʣ=="-",Ǝ,ƍ);}}return true;}
int Ѳ=0;int Ϗ=0;int Ӊ=0;List<MyInventoryItem>Î=new List<MyInventoryItem>();bool ӈ(bool ò){ο Д=ō;if(!ò)Ѳ=0;for(;Ѳ<Д.υ.Count;
Ѳ++){if(!ò)Ϗ=0;for(;Ϗ<Д.υ[Ѳ].InventoryCount;Ϗ++){IMyInventory ώ=Д.υ[Ѳ].GetInventory(Ϗ);if(!ò){Ӊ=0;Î.Clear();ώ.GetItems(Î)
;}else ò=false;for(;Ӊ<Î.Count;Ӊ++){if(!ƫ.ʕ(40))return false;MyInventoryItem k=Î[Ӊ];string Å=à.ǿ(k);string Ã,Â;à.ȃ(Å,out Ã
,out Â);if(string.Compare(Â,"ore",true)==0){if(ҙ.ź(Ã+" ingot",Ã,"Ingot")&&ҙ.ź(Å,Ã,Â))continue;}else{if(ҙ.ź(Å,Ã,Â))
continue;}à.ȃ(Å,out Ã,out Â);Ə ſ=ҙ.Ɓ(Å,Ã,Â);ſ.Ƌ+=(double)k.Amount;}}}return true;}int ĕ=0;public override bool Ƒ(bool ò){if(!ò){
Ӌ();ĕ=0;}for(;ĕ<=13;ĕ++){switch(ĕ){case 0:if(!ō.Ϥ(Ɣ.ˬ,ò))return false;break;case 1:if(!ӊ(ò))return false;if(Ҙ)ĕ++;break;
case 2:if(!ҙ.Ɖ(ò))return false;break;case 3:if(!ӈ(ò))return false;break;case 4:if(!Ҕ(ò,Җ,"Ore",ğ.ǳ("I3")))return false;break
;case 5:if(ͷ){if(!Ҕ(ò,Җ,"Ingot",ğ.ǳ("I4")))return false;}else{if(!Қ(ò))return false;}break;case 6:if(!Ҕ(ò,Җ,"Component",ğ
.ǳ("I5")))return false;break;case 7:if(!Ҕ(ò,Җ,"OxygenContainerObject",ğ.ǳ("I6")))return false;break;case 8:if(!Ҕ(ò,true,
"GasContainerObject",""))return false;break;case 9:if(!Ҕ(ò,Җ,"AmmoMagazine",ğ.ǳ("I7")))return false;break;case 10:if(!Ҕ(ò,Җ,
"PhysicalGunObject",ğ.ǳ("I8")))return false;break;case 11:if(!Ҕ(ò,true,"Datapad",""))return false;break;case 12:if(!Ҕ(ò,true,
"ConsumableItem",""))return false;break;case 13:if(!Ҕ(ò,true,"PhysicalObject",""))return false;break;}ò=false;}ӌ=false;return true;}}
class Ә:Ɯ{public Ә(){ɐ=2;ɓ="CmdAmount";}ο ō;public override void ɰ(){ō=new ο(ƫ,à.ŋ);}bool ӗ;bool Ӗ=false;int œ=0;int Ŗ=0;int
Œ=0;public override bool Ƒ(bool ò){if(!ò){ӗ=!Ɣ.ˮ.EndsWith("x");Ӗ=Ɣ.ˮ.EndsWith("bar");if(Ӗ)ӗ=true;if(Ɣ.ˑ.Count==0)Ɣ.ˑ.Add(
new ʮ("reactor,gatlingturret,missileturret,interiorturret,gatlinggun,launcherreload,launcher,oxygenerator"));Ŗ=0;}for(;Ŗ<Ɣ.
ˑ.Count;Ŗ++){ʮ Ŕ=Ɣ.ˑ[Ŗ];if(!ò){Ŕ.ʟ();œ=0;Œ=0;}for(;Œ<Ŕ.ʡ.Count;Œ++){if(œ==0){if(!ò){if(Ŕ.ʡ[Œ]=="")continue;ō.Ů();}string
Ő=Ŕ.ʡ[Œ];if(!ō.Ͻ(Ő,Ɣ.ˬ,ò))return false;œ++;ò=false;}if(!ӏ(ò))return false;ò=false;œ=0;}}return true;}int ӕ=0;int ĩ=0;
double ſ=0;double Ӕ=0;double ӓ=0;int Ӊ=0;IMyTerminalBlock Ӓ;IMyInventory ӑ;List<MyInventoryItem>Î=new List<MyInventoryItem>();
string Ӑ="";bool ӏ(bool ò){if(!ò){ӕ=0;ĩ=0;}for(;ӕ<ō.Г();ӕ++){if(ĩ==0){if(!ƫ.ʕ(50))return false;Ӓ=ō.υ[ӕ];ӑ=Ӓ.GetInventory(0);if
(ӑ==null)continue;ĩ++;ò=false;}if(!ò){Î.Clear();ӑ.GetItems(Î);Ӑ=(Î.Count>0?Î[0].Type.ToString():"");Ӊ=0;ſ=0;Ӕ=0;ӓ=0;}for(
;Ӊ<Î.Count;Ӊ++){if(!ƫ.ʕ(30))return false;MyInventoryItem k=Î[Ӊ];if(k.Type.ToString()!=Ӑ)ӓ+=(double)k.Amount;else ſ+=(
double)k.Amount;}string Ҽ=ğ.ǳ("A1");string ƕ=Ӓ.CustomName;if(ſ>0&&(double)ӑ.CurrentVolume>0){double һ=ӓ*(double)ӑ.
CurrentVolume/(ſ+ӓ);Ӕ=Math.Floor(ſ*((double)ӑ.MaxVolume-һ)/((double)ӑ.CurrentVolume-һ));Ҽ=à.Ȑ(ſ)+" / "+(ӓ>0?"~":"")+à.Ȑ(Ӕ);}if(!Ӗ||Ӕ
<=0){ƕ=à.Ǧ(ƕ,à.ɞ*0.8f);à.Ë(ƕ);à.Ƶ(Ҽ);}if(ӗ&&Ӕ>0){double ʚ=100*ſ/Ӕ;à.Ƹ(ʚ);}ĩ=0;ò=false;}return true;}}class Һ:Ɯ{ο ō;public
Һ(){ɐ=2;ɓ="CmdMass";}public override void ɰ(){ō=new ο(ƫ,à.ŋ);}bool ͷ=false;bool Ω=false;int ĕ=0;public override bool Ƒ(
bool ò){if(!ò){ō.Ů();ͷ=(Ɣ.ˮ[Ɣ.ˮ.Length-1]=='x');Ω=(Ɣ.ˮ[Ɣ.ˮ.Length-1]=='p');ĕ=0;}if(ĕ==0){if(!ō.Ϥ(Ɣ.ˬ,ò))return false;ĕ++;ò=
false;}double Æ=ō.ϐ(ò);if(Double.IsNaN(Æ))return false;double ʛ=0;int ѝ=Ɣ.ˑ.Count;if(ѝ>0){double.TryParse(Ɣ.ˑ[0].Ŕ.Trim(),out
ʛ);if(ѝ>1){string Ѡ=Ɣ.ˑ[1].Ŕ.Trim();char љ=' ';if(Ѡ.Length>0)љ=Char.ToLower(Ѡ[0]);int ќ="kmgtpezy".IndexOf(љ);if(ќ>=0)ʛ*=
Math.Pow(1000.0,ќ);}ʛ*=1000.0;}à.Ë(ğ.ǳ("M1")+" ");if(ʛ<=0){à.Ƶ(à.ȏ(Æ,false));return true;}double ʚ=Æ/ʛ*100;if(!ͷ&&!Ω){à.Ƶ(à.
ȏ(Æ)+" / "+à.ȏ(ʛ));à.ƿ(ʚ,1.0f,à.Ʒ);à.ǖ(' '+à.ȇ(ʚ)+"%");}else if(Ω){à.Ƶ(à.ȇ(ʚ)+"%");à.Ƹ(ʚ);}else à.Ƶ(à.ȇ(ʚ)+"%");return
true;}}class ҹ:Ɯ{ȸ ǉ;ο ō;public ҹ(){ɐ=3;ɓ="CmdOxygen";}public override void ɰ(){ǉ=à.ǉ;ō=new ο(ƫ,à.ŋ);}int ĕ=0;int ć=0;bool ƃ
=false;int ҽ=0;double Ƞ=0;double ȡ=0;double ƾ;public override bool Ƒ(bool ò){if(!ò){ō.Ů();ĕ=0;ć=0;ƾ=0;}if(ĕ==0){if(!ō.Ͻ(
"airvent",Ɣ.ˬ,ò))return false;ƃ=(ō.Г()>0);ĕ++;ò=false;}if(ĕ==1){for(;ć<ō.Г();ć++){if(!ƫ.ʕ(8))return false;IMyAirVent ŉ=ō.υ[ć]as
IMyAirVent;ƾ=Math.Max(ŉ.GetOxygenLevel()*100,0f);à.Ë(ŉ.CustomName);if(ŉ.CanPressurize)à.Ƶ(à.ȇ(ƾ)+"%");else à.Ƶ(ğ.ǳ("O1"));à.Ƹ(ƾ);}
ĕ++;ò=false;}if(ĕ==2){if(!ò)ō.Ů();if(!ō.Ͻ("oxyfarm",Ɣ.ˬ,ò))return false;ҽ=ō.Г();ĕ++;ò=false;}if(ĕ==3){if(ҽ>0){if(!ò)ć=0;
double Ӈ=0;for(;ć<ҽ;ć++){if(!ƫ.ʕ(4))return false;IMyOxygenFarm ӆ=ō.υ[ć]as IMyOxygenFarm;Ӈ+=ӆ.GetOutput()*100;}ƾ=Ӈ/ҽ;if(ƃ)à.ǖ(
"");ƃ|=(ҽ>0);à.Ë(ğ.ǳ("O2"));à.Ƶ(à.ȇ(ƾ)+"%");à.Ƹ(ƾ);}ĕ++;ò=false;}if(ĕ==4){if(!ò)ō.Ů();if(!ō.Ͻ("oxytank",Ɣ.ˬ,ò))return
false;ҽ=ō.Г();if(ҽ==0){if(!ƃ)à.ǖ(ğ.ǳ("O3"));return true;}ĕ++;ò=false;}if(ĕ==5){if(!ò){Ƞ=0;ȡ=0;ć=0;}if(!ǉ.Ȓ(ō.υ,"oxygen",ref ȡ
,ref Ƞ,ò))return false;if(Ƞ==0){if(!ƃ)à.ǖ(ğ.ǳ("O3"));return true;}ƾ=ȡ/Ƞ*100;if(ƃ)à.ǖ("");à.Ë(ğ.ǳ("O4"));à.Ƶ(à.ȇ(ƾ)+"%");à
.Ƹ(ƾ);ĕ++;}return true;}}class Ӆ:Ɯ{public Ӆ(){ɐ=1;ɓ="CmdPosition";}public override bool Ƒ(bool ò){bool ӄ=(Ɣ.ˮ=="posxyz");
bool Ҧ=(Ɣ.ˮ=="posgps");IMyTerminalBlock Ý=Ğ.o.Ý;if(Ɣ.ˬ!=""&&Ɣ.ˬ!="*"){Ý=à.Ǉ.GetBlockWithName(Ɣ.ˬ);if(Ý==null){à.ǖ("Pos: "+ğ.
ǳ("P1")+": "+Ɣ.ˬ);return true;}}if(Ҧ){Vector3D Ł=Ý.GetPosition();à.ǖ("GPS:"+ğ.ǳ("P2")+":"+Ł.GetDim(0).ToString("F2")+":"+
Ł.GetDim(1).ToString("F2")+":"+Ł.GetDim(2).ToString("F2")+":");return true;}à.Ë(ğ.ǳ("P2")+": ");if(!ӄ){à.Ƶ(Ý.GetPosition(
).ToString("F0"));return true;}à.ǖ("");à.Ë(" X: ");à.Ƶ(Ý.GetPosition().GetDim(0).ToString("F0"));à.Ë(" Y: ");à.Ƶ(Ý.
GetPosition().GetDim(1).ToString("F0"));à.Ë(" Z: ");à.Ƶ(Ý.GetPosition().GetDim(2).ToString("F0"));return true;}}class Ӄ:Ɯ{public Ӄ(
){ɐ=3;ɓ="CmdPower";}ȸ ǉ;ο ӂ;ο Ӂ;ο Ӏ;ο ђ;ο ҿ;ο ō;public override void ɰ(){ӂ=new ο(ƫ,à.ŋ);Ӂ=new ο(ƫ,à.ŋ);Ӏ=new ο(ƫ,à.ŋ);ђ=
new ο(ƫ,à.ŋ);ҿ=new ο(ƫ,à.ŋ);ō=new ο(ƫ,à.ŋ);ǉ=à.ǉ;}string Ь;bool Ҿ;string ё;string Ѿ;int Ъ;int ĕ=0;public override bool Ƒ(
bool ò){if(!ò){Ь=(Ɣ.ˮ.EndsWith("x")?"s":(Ɣ.ˮ.EndsWith("p")?"p":(Ɣ.ˮ.EndsWith("v")?"v":"n")));Ҿ=(Ɣ.ˮ.StartsWith(
"powersummary"));ё="a";Ѿ="";if(Ɣ.ˮ.Contains("stored"))ё="s";else if(Ɣ.ˮ.Contains("in"))ё="i";else if(Ɣ.ˮ.Contains("out"))ё="o";ĕ=0;ӂ.Ů
();Ӂ.Ů();Ӏ.Ů();ђ.Ů();ҿ.Ů();}if(ё=="a"){if(ĕ==0){if(!ӂ.Ͻ("reactor",Ɣ.ˬ,ò))return false;ò=false;ĕ++;}if(ĕ==1){if(!Ӂ.Ͻ(
"hydrogenengine",Ɣ.ˬ,ò))return false;ò=false;ĕ++;}if(ĕ==2){if(!Ӏ.Ͻ("solarpanel",Ɣ.ˬ,ò))return false;ò=false;ĕ++;}if(ĕ==3){if(!ҿ.Ͻ(
"windturbine",Ɣ.ˬ,ò))return false;ò=false;ĕ++;}}else if(ĕ==0)ĕ=4;if(ĕ==4){if(!ђ.Ͻ("battery",Ɣ.ˬ,ò))return false;ò=false;ĕ++;}int ф=ӂ.
Г();int у=Ӂ.Г();int т=Ӏ.Г();int с=ђ.Г();int р=ҿ.Г();if(ĕ==5){Ъ=0;if(ф>0)Ъ++;if(у>0)Ъ++;if(т>0)Ъ++;if(р>0)Ъ++;if(с>0)Ъ++;
if(Ъ<1){à.ǖ(ğ.ǳ("P6"));return true;}if(Ɣ.ˑ.Count>0){if(Ɣ.ˑ[0].Ŕ.Length>0)Ѿ=Ɣ.ˑ[0].Ŕ;}ĕ++;ò=false;}if(ё!="a"){if(!ѓ(ђ,(Ѿ==
""?ğ.ǳ("P7"):Ѿ),ё,Ь,ò))return false;return true;}string и=ğ.ǳ("P8");if(!Ҿ){if(ĕ==6){if(ф>0)if(!к(ӂ,(Ѿ==""?ğ.ǳ("P9"):Ѿ),Ь,ò
))return false;ĕ++;ò=false;}if(ĕ==7){if(у>0)if(!к(Ӂ,(Ѿ==""?ğ.ǳ("P12"):Ѿ),Ь,ò))return false;ĕ++;ò=false;}if(ĕ==8){if(т>0)
if(!к(Ӏ,(Ѿ==""?ğ.ǳ("P10"):Ѿ),Ь,ò))return false;ĕ++;ò=false;}if(ĕ==9){if(р>0)if(!к(ҿ,(Ѿ==""?ğ.ǳ("P13"):Ѿ),Ь,ò))return false
;ĕ++;ò=false;}if(ĕ==10){if(с>0)if(!ѓ(ђ,(Ѿ==""?ğ.ǳ("P7"):Ѿ),ё,Ь,ò))return false;ĕ++;ò=false;}}else{и=ğ.ǳ("P11");Ъ=10;if(ĕ
==6)ĕ=11;}if(Ъ==1)return true;if(!ò){ō.Ů();ō.Е(ӂ);ō.Е(Ӂ);ō.Е(Ӏ);ō.Е(ҿ);ō.Е(ђ);}if(!к(ō,и,Ь,ò))return false;return true;}
void п(double ʞ,double ƍ){double з=(ƍ>0?ʞ/ƍ*100:0);switch(Ь){case"s":à.Ƶ(ƴ.Ů().ɱ(' ').ɱ(з.ToString("F1")).ɱ("%"));break;case
"v":à.Ƶ(ƴ.Ů().ɱ(à.Ȑ(ʞ)).ɱ("W / ").ɱ(à.Ȑ(ƍ)).ɱ("W"));break;case"c":à.Ƶ(ƴ.Ů().ɱ(à.Ȑ(ʞ)).ɱ("W"));break;case"p":à.Ƶ(ƴ.Ů().ɱ(' '
).ɱ(з.ToString("F1")).ɱ("%"));à.Ƹ(з);break;default:à.Ƶ(ƴ.Ů().ɱ(à.Ȑ(ʞ)).ɱ("W / ").ɱ(à.Ȑ(ƍ)).ɱ("W"));à.ƿ(з,1.0f,à.Ʒ);à.Ƶ(ƴ.
Ů().ɱ(' ').ɱ(з.ToString("F1")).ɱ("%"));break;}}double н=0;double Л=0,м=0;int л=0;bool к(ο й,string и,string ʘ,bool ò){if(
!ò){Л=0;м=0;л=0;}if(л==0){if(!ǉ.Ɂ(й.υ,ǉ.ȷ,ref н,ref н,ref Л,ref м,ò))return false;л++;ò=false;}if(!ƫ.ʕ(50))return false;
double з=(м>0?Л/м*100:0);à.Ë(и+": ");п(Л*1000000,м*1000000);return true;}double о=0,ж=0,х=0,ї=0;double і=0,ѕ=0;int є=0;ʐ ƴ=new
ʐ(100);bool ѓ(ο ђ,string и,string ё,string ʘ,bool ò){if(!ò){о=ж=0;х=ї=0;і=ѕ=0;є=0;}if(є==0){if(!ǉ.Ɇ(ђ.υ,ref х,ref ї,ref о
,ref ж,ref і,ref ѕ,ò))return false;х*=1000000;ї*=1000000;о*=1000000;ж*=1000000;і*=1000000;ѕ*=1000000;є++;ò=false;}double
ѐ=(ѕ>0?і/ѕ*100:0);double я=(ж>0?о/ж*100:0);double ю=(ї>0?х/ї*100:0);bool э=ё=="a";if(є==1){if(!ƫ.ʕ(50))return false;if(э)
{if(ʘ!="p"){à.Ë(ƴ.Ů().ɱ(и).ɱ(": "));à.Ƶ(ƴ.Ů().ɱ("(IN ").ɱ(à.Ȑ(х)).ɱ("W / OUT ").ɱ(à.Ȑ(о)).ɱ("W)"));}else à.ǖ(ƴ.Ů().ɱ(и).ɱ
(": "));à.Ë(ƴ.Ů().ɱ("  ").ɱ(ğ.ǳ("P3")).ɱ(": "));}else à.Ë(ƴ.Ů().ɱ(и).ɱ(": "));if(э||ё=="s")switch(ʘ){case"s":à.Ƶ(ƴ.Ů().ɱ(
' ').ɱ(ѐ.ToString("F1")).ɱ("%"));break;case"v":à.Ƶ(ƴ.Ů().ɱ(à.Ȑ(і)).ɱ("Wh / ").ɱ(à.Ȑ(ѕ)).ɱ("Wh"));break;case"p":à.Ƶ(ƴ.Ů().ɱ(
' ').ɱ(ѐ.ToString("F1")).ɱ("%"));à.Ƹ(ѐ);break;default:à.Ƶ(ƴ.Ů().ɱ(à.Ȑ(і)).ɱ("Wh / ").ɱ(à.Ȑ(ѕ)).ɱ("Wh"));à.ƿ(ѐ,1.0f,à.Ʒ);à.Ƶ
(ƴ.Ů().ɱ(' ').ɱ(ѐ.ToString("F1")).ɱ("%"));break;}if(ё=="s")return true;є++;ò=false;}if(є==2){if(!ƫ.ʕ(50))return false;if(
э)à.Ë(ƴ.Ů().ɱ("  ").ɱ(ğ.ǳ("P4")).ɱ(": "));if(э||ё=="o")switch(ʘ){case"s":à.Ƶ(ƴ.Ů().ɱ(' ').ɱ(я.ToString("F1")).ɱ("%"));
break;case"v":à.Ƶ(ƴ.Ů().ɱ(à.Ȑ(о)).ɱ("W / ").ɱ(à.Ȑ(ж)).ɱ("W"));break;case"p":à.Ƶ(ƴ.Ů().ɱ(' ').ɱ(я.ToString("F1")).ɱ("%"));à.Ƹ(
я);break;default:à.Ƶ(ƴ.Ů().ɱ(à.Ȑ(о)).ɱ("W / ").ɱ(à.Ȑ(ж)).ɱ("W"));à.ƿ(я,1.0f,à.Ʒ);à.Ƶ(ƴ.Ů().ɱ(' ').ɱ(я.ToString("F1")).ɱ(
"%"));break;}if(ё=="o")return true;є++;ò=false;}if(!ƫ.ʕ(50))return false;if(э)à.Ë(ƴ.Ů().ɱ("  ").ɱ(ğ.ǳ("P5")).ɱ(": "));if(э
||ё=="i")switch(ʘ){case"s":à.Ƶ(ƴ.Ů().ɱ(' ').ɱ(ю.ToString("F1")).ɱ("%"));break;case"v":à.Ƶ(ƴ.Ů().ɱ(à.Ȑ(х)).ɱ("W / ").ɱ(à.Ȑ(
ї)).ɱ("W"));break;case"p":à.Ƶ(ƴ.Ů().ɱ(' ').ɱ(ю.ToString("F1")).ɱ("%"));à.Ƹ(ю);break;default:à.Ƶ(ƴ.Ů().ɱ(à.Ȑ(х)).ɱ("W / ")
.ɱ(à.Ȑ(ї)).ɱ("W"));à.ƿ(ю,1.0f,à.Ʒ);à.Ƶ(ƴ.Ů().ɱ(' ').ɱ(ю.ToString("F1")).ɱ("%"));break;}return true;}}class ь:Ɯ{public ь()
{ɐ=7;ɓ="CmdPowerTime";}class ы{public TimeSpan Ĥ=new TimeSpan(-1);public double д=-1;public double ъ=0;}ы щ=new ы();ο ш;ο
ч;public override void ɰ(){ш=new ο(ƫ,à.ŋ);ч=new ο(ƫ,à.ŋ);}int ц=0;double е=0;double Ш=0,З=0;double Ч=0,Ц=0,Х=0;double Ф=0
,У=0;int Т=0;private bool С(string ˬ,out TimeSpan Р,out double ή,bool ò){MyResourceSourceComponent ȵ;
MyResourceSinkComponent ȟ;double О=ɑ;ы Н=щ;Р=Н.Ĥ;ή=Н.д;if(!ò){ш.Ů();ч.Ů();Н.д=0;ц=0;е=0;Ш=З=0;Ч=0;Ц=Х=0;Ф=У=0;Т=0;}if(ц==0){if(!ш.Ͻ("reactor",ˬ
,ò))return false;ò=false;ц++;}if(ц==1){for(;Т<ш.υ.Count;Т++){if(!ƫ.ʕ(6))return false;IMyReactor Ý=ш.υ[Т]as IMyReactor;if(
Ý==null||!Ý.IsWorking)continue;if(Ý.Components.TryGet<MyResourceSourceComponent>(out ȵ)){Ш+=ȵ.CurrentOutputByType(à.ǉ.ȷ);
З+=ȵ.MaxOutputByType(à.ǉ.ȷ);}е+=(double)Ý.GetInventory(0).CurrentMass;}ò=false;ц++;}if(ц==2){if(!ч.Ͻ("battery",ˬ,ò))
return false;ò=false;ц++;}if(ц==3){if(!ò)Т=0;for(;Т<ч.υ.Count;Т++){if(!ƫ.ʕ(15))return false;IMyBatteryBlock Ý=ч.υ[Т]as
IMyBatteryBlock;if(Ý==null||!Ý.IsWorking)continue;if(Ý.Components.TryGet<MyResourceSourceComponent>(out ȵ)){Ц=ȵ.CurrentOutputByType(à.ǉ
.ȷ);Х=ȵ.MaxOutputByType(à.ǉ.ȷ);}if(Ý.Components.TryGet<MyResourceSinkComponent>(out ȟ)){Ц-=ȟ.CurrentInputByType(à.ǉ.ȷ);}
double М=(Ц<0?(Ý.MaxStoredPower-Ý.CurrentStoredPower)/(-Ц/3600):0);if(М>Н.д)Н.д=М;if(Ý.ChargeMode==ChargeMode.Recharge)
continue;Ф+=Ц;У+=Х;Ч+=Ý.CurrentStoredPower;}ò=false;ц++;}double Л=Ш+Ф;if(Л<=0)Н.Ĥ=TimeSpan.FromSeconds(-1);else{double К=Н.Ĥ.
TotalSeconds;double Й;double И=(Н.ъ-е)/О;if(Ш<=0)И=Math.Min(Л,З)/3600000;double П=0;if(У>0)П=Math.Min(Л,У)/3600;if(И<=0&&П<=0)Й=-1;
else if(И<=0)Й=Ч/П;else if(П<=0)Й=е/И;else{double Щ=П;double Я=(Ш<=0?Л/3600:И*Л/Ш);Й=Ч/Щ+е/Я;}if(К<=0||Й<0)К=Й;else К=(К+Й)/
2;try{Н.Ĥ=TimeSpan.FromSeconds(К);}catch{Н.Ĥ=TimeSpan.FromSeconds(-1);}}Н.ъ=е;ή=Н.д;Р=Н.Ĥ;return true;}int ĕ=0;bool Σ=
false;bool ͷ=false;bool Ω=false;double д=0;TimeSpan Ȝ;int г=0,в=0,б=0;int Ǫ=0;int а=0;public override bool Ƒ(bool ò){if(!ò){Σ
=Ɣ.ˮ.EndsWith("bar");ͷ=(Ɣ.ˮ[Ɣ.ˮ.Length-1]=='x');Ω=(Ɣ.ˮ[Ɣ.ˮ.Length-1]=='p');ĕ=0;г=в=б=Ǫ=0;а=0;д=0;}if(ĕ==0){if(Ɣ.ˑ.Count>0
){for(;а<Ɣ.ˑ.Count;а++){if(!ƫ.ʕ(100))return false;Ɣ.ˑ[а].ʟ();if(Ɣ.ˑ[а].ʡ.Count<=0)continue;string Ŕ=Ɣ.ˑ[а].ʡ[0];int.
TryParse(Ŕ,out Ǫ);if(а==0)г=Ǫ;else if(а==1)в=Ǫ;else if(а==2)б=Ǫ;}}ĕ++;ò=false;}if(ĕ==1){if(!С(Ɣ.ˬ,out Ȝ,out д,ò))return false;ĕ
++;ò=false;}if(!ƫ.ʕ(30))return false;double Ĥ=0;TimeSpan Ю;try{Ю=new TimeSpan(г,в,б);}catch{Ю=TimeSpan.FromSeconds(-1);}
string Ĵ;if(Ȝ.TotalSeconds>0||д<=0){if(!Σ)à.Ë(ğ.ǳ("PT1")+" ");Ĵ=à.ǉ.ȣ(Ȝ);Ĥ=Ȝ.TotalSeconds;}else{if(!Σ)à.Ë(ğ.ǳ("PT2")+" ");Ĵ=à.
ǉ.ȣ(TimeSpan.FromSeconds(д));if(Ю.TotalSeconds>=д)Ĥ=Ю.TotalSeconds-д;else Ĥ=0;}if(Ю.Ticks<=0){à.Ƶ(Ĵ);return true;}double
ʚ=Ĥ/Ю.TotalSeconds*100;if(ʚ>100)ʚ=100;if(Σ){à.Ƹ(ʚ);return true;}if(!ͷ&&!Ω){à.Ƶ(Ĵ);à.ƿ(ʚ,1.0f,à.Ʒ);à.ǖ(' '+ʚ.ToString(
"0.0")+"%");}else if(Ω){à.Ƶ(ʚ.ToString("0.0")+"%");à.Ƹ(ʚ);}else à.Ƶ(ʚ.ToString("0.0")+"%");return true;}}class Э:Ɯ{public Э()
{ɐ=7;ɓ="CmdPowerUsed";}ȸ ǉ;ο ō;public override void ɰ(){ō=new ο(ƫ,à.ŋ);ǉ=à.ǉ;}string Ь;string Ы;string Ϫ;void п(double ʞ,
double ƍ){double з=(ƍ>0?ʞ/ƍ*100:0);switch(Ь){case"s":à.Ƶ(з.ToString("0.0")+"%",1.0f);break;case"v":à.Ƶ(à.Ȑ(ʞ)+"W / "+à.Ȑ(ƍ)+
"W",1.0f);break;case"c":à.Ƶ(à.Ȑ(ʞ)+"W",1.0f);break;case"p":à.Ƶ(з.ToString("0.0")+"%",1.0f);à.Ƹ(з);break;default:à.Ƶ(à.Ȑ(ʞ)+
"W / "+à.Ȑ(ƍ)+"W");à.ƿ(з,1.0f,à.Ʒ);à.Ƶ(' '+з.ToString("0.0")+"%");break;}}double ȿ=0,Ⱦ=0;int Ѳ=0;int ĕ=0;ѭ ѱ=new ѭ();public
override bool Ƒ(bool ò){if(!ò){Ь=(Ɣ.ˮ.EndsWith("x")?"s":(Ɣ.ˮ.EndsWith("usedp")||Ɣ.ˮ.EndsWith("topp")?"p":(Ɣ.ˮ.EndsWith("v")?"v":
(Ɣ.ˮ.EndsWith("c")?"c":"n"))));Ы=(Ɣ.ˮ.Contains("top")?"top":"");Ϫ=(Ɣ.ˑ.Count>0?Ɣ.ˑ[0].Ŕ:ğ.ǳ("PU1"));ȿ=Ⱦ=0;ĕ=0;Ѳ=0;ō.Ů();ѱ
.Y();}if(ĕ==0){if(!ō.Ϥ(Ɣ.ˬ,ò))return false;ò=false;ĕ++;}MyResourceSinkComponent ȟ;MyResourceSourceComponent ȵ;switch(Ы){
case"top":if(ĕ==1){for(;Ѳ<ō.υ.Count;Ѳ++){if(!ƫ.ʕ(20))return false;IMyTerminalBlock Ý=ō.υ[Ѳ];if(Ý.Components.TryGet<
MyResourceSinkComponent>(out ȟ)){ListReader<MyDefinitionId>ȝ=ȟ.AcceptedResources;if(ȝ.IndexOf(ǉ.ȷ)<0)continue;ȿ=ȟ.CurrentInputByType(ǉ.ȷ)*
1000000;}else continue;ѱ.v(ȿ,Ý);}ò=false;ĕ++;}if(ѱ.j()<=0){à.ǖ("PowerUsedTop: "+ğ.ǳ("D2"));return true;}int Ĺ=10;if(Ɣ.ˑ.Count>0
)if(!int.TryParse(Ϫ,out Ĺ)){Ĺ=10;}if(Ĺ>ѱ.j())Ĺ=ѱ.j();if(ĕ==2){if(!ò){Ѳ=ѱ.j()-1;ѱ.Z();}for(;Ѳ>=ѱ.j()-Ĺ;Ѳ--){if(!ƫ.ʕ(30))
return false;IMyTerminalBlock Ý=ѱ.b(Ѳ);string ƕ=à.Ǧ(Ý.CustomName,à.ɞ*0.4f);if(Ý.Components.TryGet<MyResourceSinkComponent>(out
ȟ)){ȿ=ȟ.CurrentInputByType(ǉ.ȷ)*1000000;Ⱦ=ȟ.MaxRequiredInputByType(ǉ.ȷ)*1000000;var Ѯ=(Ý as IMyRadioAntenna);if(Ѯ!=null)Ⱦ
*=Ѯ.Radius/500;}à.Ë(ƕ+" ");п(ȿ,Ⱦ);}}break;default:for(;Ѳ<ō.υ.Count;Ѳ++){if(!ƫ.ʕ(10))return false;double ѯ;IMyTerminalBlock
Ý=ō.υ[Ѳ];if(Ý.Components.TryGet<MyResourceSinkComponent>(out ȟ)){ListReader<MyDefinitionId>ȝ=ȟ.AcceptedResources;if(ȝ.
IndexOf(ǉ.ȷ)<0)continue;ѯ=ȟ.CurrentInputByType(ǉ.ȷ);Ⱦ+=ȟ.MaxRequiredInputByType(ǉ.ȷ);var Ѯ=(Ý as IMyRadioAntenna);if(Ѯ!=null){Ⱦ
*=Ѯ.Radius/500;}}else continue;if(Ý.Components.TryGet<MyResourceSourceComponent>(out ȵ)&&(Ý as IMyBatteryBlock!=null)){ѯ-=
ȵ.CurrentOutputByType(ǉ.ȷ);if(ѯ<=0)continue;}ȿ+=ѯ;}à.Ë(Ϫ);п(ȿ*1000000,Ⱦ*1000000);break;}return true;}public class ѭ{List<
KeyValuePair<double,IMyTerminalBlock>>Ѭ=new List<KeyValuePair<double,IMyTerminalBlock>>();public void v(double Ѱ,IMyTerminalBlock Ý)
{Ѭ.Add(new KeyValuePair<double,IMyTerminalBlock>(Ѱ,Ý));}public int j(){return Ѭ.Count;}public IMyTerminalBlock b(int a){
return Ѭ[a].Value;}public void Y(){Ѭ.Clear();}public void Z(){Ѭ.Sort((ϥ,ѽ)=>(ϥ.Key.CompareTo(ѽ.Key)));}}}class Ѽ:Ɯ{ο ō;public
Ѽ(){ɐ=1;ɓ="CmdProp";}public override void ɰ(){ō=new ο(ƫ,à.ŋ);}int ĕ=0;int Ѳ=0;bool ѻ=false;string Ѻ=null;string ѹ=null;
string Ѹ=null;string ѷ=null;public override bool Ƒ(bool ò){if(!ò){ѻ=Ɣ.ˮ.StartsWith("props");Ѻ=ѹ=Ѹ=ѷ=null;Ѳ=0;ĕ=0;}if(Ɣ.ˑ.Count
<1){à.ǖ(Ɣ.ˮ+": "+"Missing property name.");return true;}if(ĕ==0){if(!ò)ō.Ů();if(!ō.Ϥ(Ɣ.ˬ,ò))return false;Ѷ();ĕ++;ò=false;
}if(ĕ==1){int Ĺ=ō.Г();if(Ĺ==0){à.ǖ(Ɣ.ˮ+": "+"No blocks found.");return true;}for(;Ѳ<Ĺ;Ѳ++){if(!ƫ.ʕ(50))return false;
IMyTerminalBlock Ý=ō.υ[Ѳ];if(Ý.GetProperty(Ѻ)!=null){if(ѹ==null){string Ϫ=à.Ǧ(Ý.CustomName,à.ɞ*0.7f);à.Ë(Ϫ);}else à.Ë(ѹ);à.Ƶ(ѵ(Ý,Ѻ,Ѹ,ѷ))
;if(!ѻ)return true;}}}return true;}void Ѷ(){Ѻ=Ɣ.ˑ[0].Ŕ;if(Ɣ.ˑ.Count>1){if(!ѻ)ѹ=Ɣ.ˑ[1].Ŕ;else Ѹ=Ɣ.ˑ[1].Ŕ;if(Ɣ.ˑ.Count>2){
if(!ѻ)Ѹ=Ɣ.ˑ[2].Ŕ;else ѷ=Ɣ.ˑ[2].Ŕ;if(Ɣ.ˑ.Count>3&&!ѻ)ѷ=Ɣ.ˑ[3].Ŕ;}}}string ѵ(IMyTerminalBlock Ý,string Ѵ,string ѳ=null,
string ѫ=null){return(Ý.GetValue<bool>(Ѵ)?(ѳ!=null?ѳ:ğ.ǳ("W9")):(ѫ!=null?ѫ:ğ.ǳ("W1")));}}class ѡ:Ɯ{public ѡ(){ɐ=5;ɓ=
"CmdShipCtrl";}ο ō;public override void ɰ(){ō=new ο(ƫ,à.ŋ);}public override bool Ƒ(bool ò){if(!ò)ō.Ů();if(!ō.Ͻ("shipctrl",Ɣ.ˬ,ò))
return false;if(ō.Г()<=0){if(Ɣ.ˬ!=""&&Ɣ.ˬ!="*")à.ǖ(Ɣ.ˮ+": "+ğ.ǳ("SC1")+" ("+Ɣ.ˬ+")");else à.ǖ(Ɣ.ˮ+": "+ğ.ǳ("SC1"));return true
;}if(Ɣ.ˮ.StartsWith("damp")){bool б=(ō.υ[0]as IMyShipController).DampenersOverride;à.Ë(ğ.ǳ("SCD"));à.Ƶ(б?"ON":"OFF");}
else{bool б=(ō.υ[0]as IMyShipController).IsUnderControl;à.Ë(ğ.ǳ("SCO"));à.Ƶ(б?"YES":"NO");}return true;}}class џ:Ɯ{public џ(
){ɐ=1;ɓ="CmdShipMass";}public override bool Ƒ(bool ò){bool ў=Ɣ.ˮ.EndsWith("base");double ʛ=0;if(Ɣ.ˬ!="")double.TryParse(Ɣ
.ˬ.Trim(),out ʛ);int ѝ=Ɣ.ˑ.Count;if(ѝ>0){string Ѡ=Ɣ.ˑ[0].Ŕ.Trim();char љ=' ';if(Ѡ.Length>0)љ=Char.ToLower(Ѡ[0]);int ќ=
"kmgtpezy".IndexOf(љ);if(ќ>=0)ʛ*=Math.Pow(1000.0,ќ);}double ɺ=(ў?à.ǈ.ɸ:à.ǈ.ɹ);if(!ў)à.Ë(ğ.ǳ("SM1")+" ");else à.Ë(ğ.ǳ("SM2")+" ");à
.Ƶ(à.ȏ(ɺ,true,'k')+" ");if(ʛ>0)à.Ƹ(ɺ/ʛ*100);return true;}}class ћ:Ɯ{public ћ(){ɐ=0.5;ɓ="CmdSpeed";}public override bool Ƒ
(bool ò){double ʛ=0;double њ=1;string љ="m/s";if(Ɣ.ˮ.Contains("kmh")){њ=3.6;љ="km/h";}else if(Ɣ.ˮ.Contains("mph")){њ=
2.23694;љ="mph";}if(Ɣ.ˬ!="")double.TryParse(Ɣ.ˬ.Trim(),out ʛ);à.Ë(ğ.ǳ("S1")+" ");à.Ƶ((à.ǈ.ʂ*њ).ToString("F1")+" "+љ+" ");if(ʛ>0
)à.Ƹ(à.ǈ.ʂ/ʛ*100);return true;}}class ј:Ɯ{public ј(){ɐ=1;ɓ="CmdStopTask";}public override bool Ƒ(bool ò){double Ѣ=0;if(Ɣ.
ˮ.Contains("best"))Ѣ=à.ǈ.ʂ/à.ǈ.ʉ;else Ѣ=à.ǈ.ʂ/à.ǈ.ɻ;double Ѧ=à.ǈ.ʂ/2*Ѣ;if(Ɣ.ˮ.Contains("time")){à.Ë(ğ.ǳ("ST"));if(double.
IsNaN(Ѣ)){à.Ƶ("N/A");return true;}string Ĵ="";try{TimeSpan Ά=TimeSpan.FromSeconds(Ѣ);if((int)Ά.TotalDays>0)Ĵ=" > 24h";else{if
(Ά.Hours>0)Ĵ=Ά.Hours+"h ";if(Ά.Minutes>0||Ĵ!="")Ĵ+=Ά.Minutes+"m ";Ĵ+=Ά.Seconds+"s";}}catch{Ĵ="N/A";}à.Ƶ(Ĵ);return true;}à
.Ë(ğ.ǳ("SD"));if(!double.IsNaN(Ѧ)&&!double.IsInfinity(Ѧ))à.Ƶ(à.Ȑ(Ѧ)+"m ");else à.Ƶ("N/A");return true;}}class Ѫ:Ɯ{ȸ ǉ;ο ō
;public Ѫ(){ɐ=2;ɓ="CmdTanks";}public override void ɰ(){ǉ=à.ǉ;ō=new ο(ƫ,à.ŋ);}int ĕ=0;char Ь='n';string ѩ;double Ѩ=0;
double ѧ=0;double ƾ;bool Σ=false;public override bool Ƒ(bool ò){List<ʮ>ˑ=Ɣ.ˑ;if(ˑ.Count==0){à.ǖ(ğ.ǳ("T4"));return true;}if(!ò)
{Ь=(Ɣ.ˮ.EndsWith("x")?'s':(Ɣ.ˮ.EndsWith("p")?'p':(Ɣ.ˮ.EndsWith("v")?'v':'n')));Σ=Ɣ.ˮ.EndsWith("bar");ĕ=0;if(ѩ==null){ѩ=ˑ[
0].Ŕ.Trim();ѩ=char.ToUpper(ѩ[0])+ѩ.Substring(1).ToLower();}ō.Ů();Ѩ=0;ѧ=0;}if(ĕ==0){if(!ō.Ͻ("oxytank",Ɣ.ˬ,ò))return false;
ò=false;ĕ++;}if(ĕ==1){if(!ō.Ͻ("hydrogenengine",Ɣ.ˬ,ò))return false;ò=false;ĕ++;}if(ĕ==2){if(!ǉ.Ȓ(ō.υ,ѩ,ref Ѩ,ref ѧ,ò))
return false;ò=false;ĕ++;}if(ѧ==0){à.ǖ(String.Format(ğ.ǳ("T5"),ѩ));return true;}ƾ=Ѩ/ѧ*100;if(Σ){à.Ƹ(ƾ);return true;}à.Ë(ѩ);
switch(Ь){case's':à.Ƶ(' '+à.ȇ(ƾ)+"%");break;case'v':à.Ƶ(à.Ȑ(Ѩ)+"L / "+à.Ȑ(ѧ)+"L");break;case'p':à.Ƶ(' '+à.ȇ(ƾ)+"%");à.Ƹ(ƾ);
break;default:à.Ƶ(à.Ȑ(Ѩ)+"L / "+à.Ȑ(ѧ)+"L");à.ƿ(ƾ,1.0f,à.Ʒ);à.Ƶ(' '+ƾ.ToString("0.0")+"%");break;}return true;}}class ѥ{ɬ à=
null;public string L="Debug";public float Ѥ=1.0f;public List<ʐ>ŧ=new List<ʐ>();public int Ų=0;public float ѣ=0;public ѥ(ɬ V)
{à=V;ŧ.Add(new ʐ());}public void ÿ(string Ĵ){ŧ[Ų].ɱ(Ĵ);}public void ÿ(ʐ Ŧ){ŧ[Ų].ɱ(Ŧ);}public void ť(){ŧ.Add(new ʐ());Ų++;
ѣ=0;}public void ť(string Ť){ŧ[Ų].ɱ(Ť);ť();}public void ţ(List<ʐ>Ţ){if(ŧ[Ų].ʎ==0)ŧ.RemoveAt(Ų);else Ų++;ŧ.AddList(Ţ);Ų+=Ţ
.Count-1;ť();}public List<ʐ>ś(){if(ŧ[Ų].ʎ==0)return ŧ.GetRange(0,Ų);else return ŧ;}public void š(string Š,string G=""){
string[]ŧ=Š.Split('\n');for(int X=0;X<ŧ.Length;X++)ť(G+ŧ[X]);}public void Ş(){ŧ.Clear();ť();Ų=0;}public int ŝ(){return Ų+(ŧ[Ų]
.ʎ>0?1:0);}public string Ŝ(){return String.Join("\n",ŧ);}public void ś(List<ʐ>Ś,int ļ,int ř){int Ř=ļ+ř;int ľ=ŝ();if(Ř>ľ)Ř
=ľ;for(int X=ļ;X<Ř;X++)Ś.Add(ŧ[X]);}}class ş{ɬ à=null;public float Ũ=1.0f;public int Ÿ=17;public int Ŷ=0;int ŵ=1;int Ŵ=1;
public List<ѥ>ų=new List<ѥ>();public int Ų=0;public ş(ɬ V){à=V;}public void ű(int Ĺ){Ŵ=Ĺ;}public void Ű(){Ÿ=(int)Math.Floor(ɬ.
ɩ*Ũ*Ŵ/ɬ.ɧ);}public void ů(ѥ Ĵ){ų.Add(Ĵ);}public void Ů(){ų.Clear();}public int ŝ(){int Ĺ=0;foreach(var Ĵ in ų){Ĺ+=Ĵ.ŝ();}
return Ĺ;}ʐ ŭ=new ʐ(256);public ʐ Ŝ(){ŭ.Ů();int Ĺ=ų.Count;for(int X=0;X<Ĺ-1;X++){ŭ.ɱ(ų[X].Ŝ());ŭ.ɱ("\n");}if(Ĺ>0)ŭ.ɱ(ų[Ĺ-1].Ŝ(
));return ŭ;}List<ʐ>Ŭ=new List<ʐ>(20);public ʐ ū(int Ū=0){ŭ.Ů();Ŭ.Clear();if(Ŵ<=0)return ŭ;int ũ=ų.Count;int ŗ=0;int Ń=(Ÿ
/Ŵ);int Ĳ=(Ū*Ń);int Ł=Ŷ+Ĳ;int ŀ=Ł+Ń;bool Ŀ=false;for(int X=0;X<ũ;X++){ѥ Ĵ=ų[X];int ľ=Ĵ.ŝ();int Ľ=ŗ;ŗ+=ľ;if(!Ŀ&&ŗ>Ł){int ļ
=Ł-Ľ;if(ŗ>=ŀ){Ĵ.ś(Ŭ,ļ,ŀ-Ľ-ļ);break;}Ŀ=true;Ĵ.ś(Ŭ,ļ,ľ);continue;}if(Ŀ){if(ŗ>=ŀ){Ĵ.ś(Ŭ,0,ŀ-Ľ);break;}Ĵ.ś(Ŭ,0,ľ);}}int Ĺ=Ŭ.
Count;for(int X=0;X<Ĺ-1;X++){ŭ.ɱ(Ŭ[X]);ŭ.ɱ("\n");}if(Ĺ>0)ŭ.ɱ(Ŭ[Ĺ-1]);return ŭ;}public bool ł(int Ĺ=-1){if(Ĺ<=0)Ĺ=à.ɯ;if(Ŷ-Ĺ<=
0){Ŷ=0;return true;}Ŷ-=Ĺ;return false;}public bool ĺ(int Ĺ=-1){if(Ĺ<=0)Ĺ=à.ɯ;int ĸ=ŝ();if(Ŷ+Ĺ+Ÿ>=ĸ){Ŷ=Math.Max(ĸ-Ÿ,0);
return true;}Ŷ+=Ĺ;return false;}public int ķ=0;public void Ķ(){if(ķ>0){ķ--;return;}if(ŝ()<=Ÿ){Ŷ=0;ŵ=1;return;}if(ŵ>0){if(ĺ()){
ŵ=-1;ķ=2;}}else{if(ł()){ŵ=1;ķ=2;}}}}class ĵ:Ɯ{public ĵ(){ɐ=1;ɓ="CmdTextLCD";}public override bool Ƒ(bool ò){string Ĵ="";
if(Ɣ.ˬ!=""&&Ɣ.ˬ!="*"){IMyTextPanel ĳ=à.Ǉ.GetBlockWithName(Ɣ.ˬ)as IMyTextPanel;if(ĳ==null){à.ǖ("TextLCD: "+ğ.ǳ("T1")+Ɣ.ˬ);
return true;}Ĵ=ĳ.GetText();}else{à.ǖ("TextLCD:"+ğ.ǳ("T2"));return true;}if(Ĵ.Length==0)return true;à.Ǖ(Ĵ);return true;}}class
Ļ:Ɯ{public Ļ(){ɐ=5;ɓ="CmdWorking";}ο ō;public override void ɰ(){ō=new ο(ƫ,à.ŋ);}int ĕ=0;int Ŗ=0;bool ŕ;public override
bool Ƒ(bool ò){if(!ò){ĕ=0;ŕ=(Ɣ.ˮ=="workingx");Ŗ=0;}if(Ɣ.ˑ.Count==0){if(ĕ==0){if(!ò)ō.Ů();if(!ō.Ϥ(Ɣ.ˬ,ò))return false;ĕ++;ò=
false;}if(!ƙ(ō,ŕ,"",ò))return false;return true;}for(;Ŗ<Ɣ.ˑ.Count;Ŗ++){ʮ Ŕ=Ɣ.ˑ[Ŗ];if(!ò)Ŕ.ʟ();if(!Ŏ(Ŕ,ò))return false;ò=false
;}return true;}int œ=0;int Œ=0;string[]ő;string Ő;string ŏ;bool Ŏ(ʮ Ŕ,bool ò){if(!ò){œ=0;Œ=0;}for(;Œ<Ŕ.ʡ.Count;Œ++){if(œ
==0){if(!ò){if(string.IsNullOrEmpty(Ŕ.ʡ[Œ]))continue;ō.Ů();ő=Ŕ.ʡ[Œ].Split(':');Ő=ő[0];ŏ=(ő.Length>1?ő[1]:"");}if(!string.
IsNullOrEmpty(Ő)){if(!ō.Ͻ(Ő,Ɣ.ˬ,ò))return false;}else{if(!ō.Ϥ(Ɣ.ˬ,ò))return false;}œ++;ò=false;}if(!ƙ(ō,ŕ,ŏ,ò))return false;œ=0;ò=
false;}return true;}string Ō(IMyTerminalBlock Ý){В ŋ=à.ŋ;if(!Ý.IsWorking)return ğ.ǳ("W1");IMyProductionBlock Ŋ=Ý as
IMyProductionBlock;if(Ŋ!=null)if(Ŋ.IsProducing)return ğ.ǳ("W2");else return ğ.ǳ("W3");IMyAirVent ŉ=Ý as IMyAirVent;if(ŉ!=null){if(ŉ.
CanPressurize)return(ŉ.GetOxygenLevel()*100).ToString("F1")+"%";else return ğ.ǳ("W4");}IMyGasTank ň=Ý as IMyGasTank;if(ň!=null)return
(ň.FilledRatio*100).ToString("F1")+"%";IMyBatteryBlock Ň=Ý as IMyBatteryBlock;if(Ň!=null)return ŋ.ϱ(Ň);IMyJumpDrive ņ=Ý
as IMyJumpDrive;if(ņ!=null)return ŋ.Ϧ(ņ).ToString("0.0")+"%";IMyLandingGear Ņ=Ý as IMyLandingGear;if(Ņ!=null){switch((int)
Ņ.LockMode){case 0:return ğ.ǳ("W8");case 1:return ğ.ǳ("W10");case 2:return ğ.ǳ("W7");}}IMyDoor ń=Ý as IMyDoor;if(ń!=null)
{if(ń.Status==DoorStatus.Open)return ğ.ǳ("W5");return ğ.ǳ("W6");}IMyShipConnector ŷ=Ý as IMyShipConnector;if(ŷ!=null){if(
ŷ.Status==MyShipConnectorStatus.Unconnected)return ğ.ǳ("W8");if(ŷ.Status==MyShipConnectorStatus.Connected)return ğ.ǳ("W7"
);else return ğ.ǳ("W10");}IMyLaserAntenna Ź=Ý as IMyLaserAntenna;if(Ź!=null)return ŋ.Ϩ(Ź);IMyRadioAntenna ƪ=Ý as
IMyRadioAntenna;if(ƪ!=null)return à.Ȑ(ƪ.Radius)+"m";IMyBeacon Ɲ=Ý as IMyBeacon;if(Ɲ!=null)return à.Ȑ(Ɲ.Radius)+"m";IMyThrust ƛ=Ý as
IMyThrust;if(ƛ!=null&&ƛ.ThrustOverride>0)return à.Ȑ(ƛ.ThrustOverride)+"N";return ğ.ǳ("W9");}int ƚ=0;bool ƙ(ο Ñ,bool Ƙ,string Ɨ,
bool ò){if(!ò)ƚ=0;for(;ƚ<Ñ.Г();ƚ++){if(!ƫ.ʕ(20))return false;IMyTerminalBlock Ý=Ñ.υ[ƚ];string Ɩ=(Ƙ?(Ý.IsWorking?ğ.ǳ("W9"):ğ.
ǳ("W1")):Ō(Ý));if(!string.IsNullOrEmpty(Ɨ)&&String.Compare(Ɩ,Ɨ,true)!=0)continue;if(Ƙ)Ɩ=Ō(Ý);string ƕ=Ý.CustomName;ƕ=à.Ǧ(
ƕ,à.ɞ*0.7f);à.Ë(ƕ);à.Ƶ(Ɩ);}return true;}}class Ɯ:ɔ{public ѥ Ĵ=null;protected Ͱ Ɣ;protected ɬ à;protected Ē Ğ;protected ǫ
ğ;public Ɯ(){ɐ=3600;ɓ="CommandTask";}public void Ɠ(Ē Ĝ,Ͱ ƒ){Ğ=Ĝ;à=Ğ.à;Ɣ=ƒ;ğ=à.ğ;}public virtual bool Ƒ(bool ò){à.ǖ(ğ.ǳ(
"UC")+": '"+Ɣ.ˤ+"'");return true;}public override bool ɮ(bool ò){Ĵ=à.Ǚ(Ĵ,Ğ.o);if(!ò)à.Ş();return Ƒ(ò);}}class Ɛ{Dictionary<
string,string>ƞ=new Dictionary<string,string>(StringComparer.InvariantCultureIgnoreCase){{"ingot","ingot"},{"ore","ore"},{
"component","component"},{"tool","physicalgunobject"},{"ammo","ammomagazine"},{"oxygen","oxygencontainerobject"},{"gas",
"gascontainerobject"}};Ț ƫ;ɬ à;Ɔ Ʃ;Ɔ ƨ;Ɔ Ƨ;Ï Ʀ;bool ƥ;public Ɔ Ƥ;public Ɛ(Ț ƣ,ɬ V,int B=20){Ʃ=new Ɔ();ƨ=new Ɔ();Ƨ=new Ɔ();ƥ=false;Ƥ=new Ɔ();
ƫ=ƣ;à=V;Ʀ=à.Ʀ;}public void Ů(){Ƨ.Y();ƨ.Y();Ʃ.Y();ƥ=false;Ƥ.Y();}public void Ƣ(string ơ,bool ƌ=false,int Ǝ=1,int ƍ=-1){if(
string.IsNullOrEmpty(ơ)){ƥ=true;return;}string[]Ơ=ơ.Split(' ');string Â="";Ə ƀ=new Ə(ƌ,Ǝ,ƍ);if(Ơ.Length==2){if(!ƞ.TryGetValue(
Ơ[1],out Â))Â=Ơ[1];}string Ã=Ơ[0];if(ƞ.TryGetValue(Ã,out ƀ.Â)){ƨ.v(ƀ.Â,ƀ);return;}à.Ǽ(ref Ã,ref Â);if(string.
IsNullOrEmpty(Â)){ƀ.Ã=Ã;Ʃ.v(ƀ.Ã,ƀ);return;}ƀ.Ã=Ã;ƀ.Â=Â;Ƨ.v(Ã+' '+Â,ƀ);}public Ə Ɵ(string Å,string Ã,string Â){Ə ƀ;ƀ=Ƨ.f(Å);if(ƀ!=null
)return ƀ;ƀ=Ʃ.f(Ã);if(ƀ!=null)return ƀ;ƀ=ƨ.f(Â);if(ƀ!=null)return ƀ;return null;}public bool ź(string Å,string Ã,string Â
){Ə ƀ;bool ƃ=false;ƀ=ƨ.f(Â);if(ƀ!=null){if(ƀ.ƌ)return true;ƃ=true;}ƀ=Ʃ.f(Ã);if(ƀ!=null){if(ƀ.ƌ)return true;ƃ=true;}ƀ=Ƨ.f(
Å);if(ƀ!=null){if(ƀ.ƌ)return true;ƃ=true;}return!(ƥ||ƃ);}public Ə Ƃ(string Å,string Ã,string Â){Ə ſ=new Ə();Ə ƀ=Ɵ(Å,Ã,Â);
if(ƀ!=null){ſ.Ǝ=ƀ.Ǝ;ſ.ƍ=ƀ.ƍ;}ſ.Ã=Ã;ſ.Â=Â;Ƥ.v(Å,ſ);return ſ;}public Ə Ɓ(string Å,string Ã,string Â){Ə ſ=Ƥ.f(Å);if(ſ==null)ſ
=Ƃ(Å,Ã,Â);return ſ;}int ž=0;List<Ə>Ž;public List<Ə>ż(string Â,bool ò,Func<Ə,bool>Ż=null){if(!ò){Ž=new List<Ə>();ž=0;}for(
;ž<Ƥ.j();ž++){if(!ƫ.ʕ(5))return null;Ə ƀ=Ƥ.b(ž);if(ź(ƀ.Ã+' '+ƀ.Â,ƀ.Ã,ƀ.Â))continue;if((string.Compare(ƀ.Â,Â,true)==0)&&(Ż
==null||Ż(ƀ)))Ž.Add(ƀ);}return Ž;}int Ƅ=0;public bool Ɖ(bool ò){if(!ò){Ƅ=0;}for(;Ƅ<Ʀ.w.Count;Ƅ++){if(!ƫ.ʕ(10))return false
;È k=Ʀ.Î[Ʀ.w[Ƅ]];if(!k.Æ)continue;string Å=k.Á+' '+k.Ò;if(ź(Å,k.Á,k.Ò))continue;Ə ſ=Ɓ(Å,k.Á,k.Ò);if(ſ.ƍ==-1)ſ.ƍ=k.ë;}
return true;}}class Ə{public int Ǝ;public int ƍ;public string Ã="";public string Â="";public bool ƌ;public double Ƌ;public Ə(
bool Ɗ=false,int ƈ=1,int Ƈ=-1){Ǝ=ƈ;ƌ=Ɗ;ƍ=Ƈ;}}class Ɔ{Dictionary<string,Ə>ƅ=new Dictionary<string,Ə>(StringComparer.
InvariantCultureIgnoreCase);List<string>w=new List<string>();public void v(string e,Ə k){if(!ƅ.ContainsKey(e)){w.Add(e);ƅ.Add(e,k);}}public int j(
){return ƅ.Count;}public Ə f(string e){if(ƅ.ContainsKey(e))return ƅ[e];return null;}public Ə b(int a){return ƅ[w[a]];}
public void Y(){w.Clear();ƅ.Clear();}public void Z(){w.Sort();}}class Ï{public Dictionary<string,È>Î=new Dictionary<string,È>(
StringComparer.InvariantCultureIgnoreCase);Dictionary<string,È>Í=new Dictionary<string,È>(StringComparer.InvariantCultureIgnoreCase);
public List<string>w=new List<string>();public Dictionary<string,È>Ì=new Dictionary<string,È>(StringComparer.
InvariantCultureIgnoreCase);public void Ë(string Ã,string Â,int Ê,string Ð,string É,string Ç,bool Æ){if(Â=="Ammo")Â="AmmoMagazine";else if(Â==
"Tool")Â="PhysicalGunObject";string Å=Ã+' '+Â;È k=new È(Ã,Â,Ê,Ð,É,Æ);Î.Add(Å,k);if(!Í.ContainsKey(Ã))Í.Add(Ã,k);if(É!="")Ì.Add
(É,k);if(Ç!="")Ì.Add(Ç,k);w.Add(Å);}public È Ä(string Ã="",string Â=""){if(Î.ContainsKey(Ã+" "+Â))return Î[Ã+" "+Â];if(
string.IsNullOrEmpty(Â)){È k=null;Í.TryGetValue(Ã,out k);return k;}if(string.IsNullOrEmpty(Ã))for(int X=0;X<Î.Count;X++){È k=Î
[w[X]];if(string.Compare(Â,k.Ò,true)==0)return k;}return null;}}class È{public string Á;public string Ò;public int ë;
public string é;public string è;public bool Æ;public È(string ç,string æ,int å=0,string ä="",string ã="",bool â=true){Á=ç;Ò=æ;
ë=å;é=ä;è=ã;Æ=â;}}class á{ɬ à=null;public µ ß=new µ();public ş Þ;public IMyTerminalBlock Ý;public IMyTextSurface Ü;public
int Û=0;public int Ú=0;public string Ù="";public string Ø="";public bool Ö=true;public IMyTextSurface Õ=>(Ó?Ü:Ý as
IMyTextSurface);public int Ô=>(Ó?(à.ǚ(Ý)?0:1):ß.j());public bool Ó=false;public á(ɬ V,string A){à=V;Ø=A;}public á(ɬ V,string A,
IMyTerminalBlock U,IMyTextSurface C,int S){à=V;Ø=A;Ý=U;Ü=C;Û=S;Ó=true;}public bool R(){return Þ.ŝ()>Þ.Ÿ||Þ.Ŷ!=0;}float Q=1.0f;bool P=
false;public float O(){if(P)return Q;P=true;if(Ý.BlockDefinition.SubtypeId.Contains("PanelWide")){if(Õ.SurfaceSize.X<Õ.
SurfaceSize.Y)Q=2.0f;}return Q;}float W=1.0f;bool N=false;public float K(){if(N)return W;N=true;if(Ý.BlockDefinition.SubtypeId.
Contains("PanelWide")){if(Õ.SurfaceSize.X<Õ.SurfaceSize.Y)W=2.0f;}return W;}bool J=false;public void I(){if(J)return;if(!Ó){ß.Z(
);Ý=ß.b(0);}int H=Ý.CustomName.IndexOf("!MARGIN:");if(H<0||H+8>=Ý.CustomName.Length){Ú=1;Ù=" ";}else{string G=Ý.
CustomName.Substring(H+8);int F=G.IndexOf(" ");if(F>=0)G=G.Substring(0,F);if(!int.TryParse(G,out Ú))Ú=1;Ù=new String(' ',Ú);}if(Ý.
CustomName.Contains("!NOSCROLL"))Ö=false;else Ö=true;J=true;}public void E(ş D=null){if(Þ==null||Ý==null)return;if(D==null)D=Þ;if(
!Ó){IMyTextSurface C=Ý as IMyTextSurface;if(C!=null){float B=C.FontSize;string L=C.Font;for(int X=0;X<ß.j();X++){
IMyTextSurface o=ß.b(X)as IMyTextSurface;if(o==null)continue;o.Alignment=VRage.Game.GUI.TextPanel.TextAlignment.LEFT;o.FontSize=B;o.
Font=L;string À=D.ū(X).ɖ();if(!à.Ǒ.SKIP_CONTENT_TYPE)o.ContentType=VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;o.
WriteText(À);}}}else{Ü.Alignment=VRage.Game.GUI.TextPanel.TextAlignment.LEFT;if(!à.Ǒ.SKIP_CONTENT_TYPE)Ü.ContentType=VRage.Game.
GUI.TextPanel.ContentType.TEXT_AND_IMAGE;Ü.WriteText(D.ū().ɖ());}J=false;}public void º(){if(Ý==null)return;if(Ó){Ü.
WriteText("");return;}IMyTextSurface C=Ý as IMyTextSurface;if(C==null)return;for(int X=0;X<ß.j();X++){IMyTextSurface o=ß.b(X)as
IMyTextSurface;if(o==null)continue;o.WriteText("");}}}class µ{Dictionary<string,IMyTerminalBlock>ª=new Dictionary<string,
IMyTerminalBlock>();Dictionary<IMyTerminalBlock,string>z=new Dictionary<IMyTerminalBlock,string>();List<string>w=new List<string>();
public void v(string e,IMyTerminalBlock k){if(!w.Contains(e)){w.Add(e);ª.Add(e,k);z.Add(k,e);}}public void r(string e){if(w.
Contains(e)){w.Remove(e);z.Remove(ª[e]);ª.Remove(e);}}public void n(IMyTerminalBlock k){if(z.ContainsKey(k)){w.Remove(z[k]);ª.
Remove(z[k]);z.Remove(k);}}public int j(){return ª.Count;}public IMyTerminalBlock f(string e){if(w.Contains(e))return ª[e];
return null;}public IMyTerminalBlock b(int a){return ª[w[a]];}public void Y(){w.Clear();ª.Clear();z.Clear();}public void Z(){w
.Sort();}}class ê:ɔ{public ɬ à;public á î;Ē Ğ;public ê(Ē Ĝ){Ğ=Ĝ;à=Ğ.à;î=Ğ.o;ɐ=0.5;ɓ="PanelDisplay";}double ě=0;public
void Ě(){ě=0;}int ę=0;int Ę=0;bool ė=true;double Ė=double.MaxValue;int ĕ=0;public override bool ɮ(bool ò){Ɯ ĝ;if(!ò&&(Ğ.Č==
false||Ğ.Ď==null||Ğ.Ď.Count<=0))return true;if(Ğ.č.ý>3)return Ɋ(0);if(!ò){Ę=0;ė=false;Ė=double.MaxValue;ĕ=0;}if(ĕ==0){while(Ę
<Ğ.Ď.Count){if(!ƫ.ʕ(5))return false;if(Ğ.ď.TryGetValue(Ğ.Ď[Ę],out ĝ)){if(!ĝ.ɍ)return Ɋ(ĝ.ɘ-ƫ.Ȗ+0.001);if(ĝ.ɒ>ě)ė=true;if(
ĝ.ɘ<Ė)Ė=ĝ.ɘ;}Ę++;}ĕ++;ò=false;}double Ĕ=Ė-ƫ.Ȗ+0.001;if(!ė&&!î.R())return Ɋ(Ĕ);à.Ǘ(î.Þ,î);if(ė){if(!ò){ě=ƫ.Ȗ;î.Þ.Ů();ę=0;}
while(ę<Ğ.Ď.Count){if(!ƫ.ʕ(7))return false;if(!Ğ.ď.TryGetValue(Ğ.Ď[ę],out ĝ)){î.Þ.ų.Add(à.Ǚ(null,î));à.Ş();à.ǖ(
"ERR: No cmd task ("+Ğ.Ď[ę]+")");ę++;continue;}î.Þ.ů(ĝ.Ĵ);ę++;}}à.Ȃ(î);Ğ.č.ý++;if(ɐ<Ĕ&&!î.R())return Ɋ(Ĕ);return true;}}class Ē:ɔ{public ɬ à
;public á o;public ê đ=null;string Đ="N/A";public Dictionary<string,Ɯ>ď=new Dictionary<string,Ɯ>();public List<string>Ď=
null;public þ č;public bool Č{get{return č.ø;}}public Ē(þ ē,á ċ){ɐ=5;o=ċ;č=ē;à=ē.à;ɓ="PanelProcess";}ǫ ğ;public override
void ɰ(){ğ=à.ğ;}Ͱ ı=null;Ɯ İ(string į,bool ò){if(!ò)ı=new Ͱ(ƫ);if(!ı.ʟ(į,ò))return null;Ɯ Ĥ=ı.ˠ();Ĥ.Ɠ(this,ı);ƫ.Ȳ(Ĥ,0);
return Ĥ;}string Į="";void ĭ(){try{Į=o.Ý.Ǳ(o.Û,à.ɦ);}catch{Į="";return;}Į=Į?.Replace("\\\n","");}int ę=0;int Ĭ=0;List<string>ī
=null;HashSet<string>Ī=new HashSet<string>();int ĩ=0;bool Ĩ(bool ò){if(!ò){char[]ħ={';','\n'};string Ħ=Į.Replace("\\;",
"\f");if(Ħ.StartsWith("@")){int ĥ=Ħ.IndexOf("\n");if(ĥ<0){Ħ="";}else{Ħ=Ħ.Substring(ĥ+1);}}ī=new List<string>(Ħ.Split(ħ,
StringSplitOptions.RemoveEmptyEntries));Ī.Clear();ę=0;Ĭ=0;ĩ=0;}while(ę<ī.Count){if(!ƫ.ʕ(500))return false;if(ī[ę].StartsWith("//")){ī.
RemoveAt(ę);continue;}ī[ę]=ī[ę].Replace('\f',';');if(!ď.ContainsKey(ī[ę])){if(ĩ!=1)ò=false;ĩ=1;Ɯ ĝ=İ(ī[ę],ò);if(ĝ==null)return
false;ò=false;ď.Add(ī[ę],ĝ);ĩ=0;}if(!Ī.Contains(ī[ę]))Ī.Add(ī[ę]);ę++;}if(Ď!=null){Ɯ Ĥ;while(Ĭ<Ď.Count){if(!ƫ.ʕ(7))return
false;if(!Ī.Contains(Ď[Ĭ]))if(ď.TryGetValue(Ď[Ĭ],out Ĥ)){Ĥ.ɤ();ď.Remove(Ď[Ĭ]);}Ĭ++;}}Ď=ī;return true;}public override void ɭ(
){if(Ď!=null){Ɯ Ĥ;for(int ģ=0;ģ<Ď.Count;ģ++){if(ď.TryGetValue(Ď[ģ],out Ĥ))Ĥ.ɤ();}Ď=null;}if(đ!=null){đ.ɤ();đ=null;}else{}
}ş Ģ=null;string ġ="";bool Ġ=false;public override bool ɮ(bool ò){if(o.Ô<=0){ɤ();return true;}if(!ò){o.Þ=à.Ǘ(o.Þ,o);Ģ=à.Ǘ
(Ģ,o);ĭ();if(Į==null){if(o.Ó){č.ï(o.Ü,o.Ý as IMyTextPanel);}else{ɤ();}return true;}if(o.Ý.CustomName!=ġ){Ġ=true;}else{Ġ=
false;}ġ=o.Ý.CustomName;}if(Į!=Đ){if(!Ĩ(ò))return false;if(Į==""){Đ="";if(č.ø){if(Ģ.ų.Count<=0)Ģ.ų.Add(à.Ǚ(null,o));else à.Ǚ(
Ģ.ų[0],o);à.Ş();à.ǖ(ğ.ǳ("H1"));bool Ċ=o.Ö;o.Ö=false;à.Ȃ(o,Ģ);o.Ö=Ċ;return true;}return this.Ɋ(2);}Ġ=true;}Đ=Į;if(đ!=null
&&Ġ){ƫ.ȯ(đ);đ.Ě();ƫ.Ȳ(đ,0);}else if(đ==null){đ=new ê(this);ƫ.Ȳ(đ,0);}return true;}}class þ:ɔ{const string ì="T:!LCD!";
public int ý=0;public ɬ à;public Ⱥ ß=new Ⱥ();ο ü;ο û;Dictionary<á,Ē>ú=new Dictionary<á,Ē>();public Dictionary<IMyTextSurface,á
>ù=new Dictionary<IMyTextSurface,á>();public bool ø=false;Ϲ ö=null;public þ(ɬ V){ɐ=5;à=V;ɓ="ProcessPanels";}public
override void ɰ(){ü=new ο(ƫ,à.ŋ);û=new ο(ƫ,à.ŋ);ö=new Ϲ(à,this);}int õ=0;bool ó(bool ò){if(!ò)õ=0;if(õ==0){if(!ü.Ϥ(à.ɦ,ò))return
false;õ++;ò=false;}if(õ==1){if(à.ɦ=="T:[LCD]"&&ì!="")if(!ü.Ϥ(ì,ò))return false;õ++;ò=false;}return true;}string ñ(
IMyTerminalBlock Ý){int ð=Ý.CustomName.IndexOf("!LINK:");if(ð>=0&&Ý.CustomName.Length>ð+6){return Ý.CustomName.Substring(ð+6)+' '+Ý.
Position.ToString();}return Ý.EntityId.ToString();}public void ï(IMyTextSurface C,IMyTextPanel o){á î;if(C==null)return;if(!ù.
TryGetValue(C,out î))return;if(o!=null){î.ß.n(o);}ù.Remove(C);if(î.Ô<=0||î.Ó){Ē í;if(ú.TryGetValue(î,out í)){ß.n(î.Ø);ú.Remove(î);í
.ɤ();}}}void ô(IMyTerminalBlock Ý){IMyTextSurfaceProvider ą=Ý as IMyTextSurfaceProvider;IMyTextSurface C=Ý as
IMyTextSurface;if(C!=null){ï(C,Ý as IMyTextPanel);return;}if(ą==null)return;for(int X=0;X<ą.SurfaceCount;X++){C=ą.GetSurface(X);ï(C,
null);}}string A;string ĉ;bool Ĉ;int ć=0;int Ć=0;public override bool ɮ(bool ò){if(!ò){ü.Ů();ć=0;Ć=0;}if(!ó(ò))return false;
while(ć<ü.Г()){if(!ƫ.ʕ(20))return false;IMyTerminalBlock Ý=(ü.υ[ć]as IMyTerminalBlock);if(Ý==null||!Ý.IsWorking){ü.υ.RemoveAt
(ć);continue;}IMyTextSurfaceProvider ą=Ý as IMyTextSurfaceProvider;IMyTextSurface C=Ý as IMyTextSurface;IMyTextPanel o=Ý
as IMyTextPanel;á î;A=ñ(Ý);string[]Ą=A.Split(' ');ĉ=Ą[0];Ĉ=Ą.Length>1;if(o!=null){if(ù.ContainsKey(C)){î=ù[C];if(î.Ø==A+
"@0"||(Ĉ&&î.Ø==ĉ)){ć++;continue;}ô(Ý);}if(!Ĉ){î=new á(à,A+"@0",Ý,C,0);Ē í=new Ē(this,î);ƫ.Ȳ(í,0);ú.Add(î,í);ß.v(î.Ø,î);ù.Add
(C,î);ć++;continue;}î=ß.f(ĉ);if(î==null){î=new á(à,ĉ);ß.v(ĉ,î);Ē í=new Ē(this,î);ƫ.Ȳ(í,0);ú.Add(î,í);}î.ß.v(A,Ý);ù.Add(C,
î);}else{if(ą==null){ć++;continue;}for(int X=0;X<ą.SurfaceCount;X++){C=ą.GetSurface(X);if(ù.ContainsKey(C)){î=ù[C];if(î.Ø
==A+'@'+X.ToString()){continue;}ï(C,null);}if(Ý.Ǳ(X,à.ɦ)==null)continue;î=new á(à,A+"@"+X.ToString(),Ý,C,X);Ē í=new Ē(this
,î);ƫ.Ȳ(í,0);ú.Add(î,í);ß.v(î.Ø,î);ù.Add(C,î);}}ć++;}while(Ć<û.Г()){if(!ƫ.ʕ(300))return false;IMyTerminalBlock Ý=û.υ[Ć];
if(Ý==null)continue;if(!ü.υ.Contains(Ý)){ô(Ý);}Ć++;}û.Ů();û.Е(ü);if(!ö.Ɏ&&ö.ϳ())ƫ.Ȳ(ö,0);return true;}public bool ă(string
Ă){if(string.Compare(Ă,"clear",true)==0){ö.Ϻ();if(!ö.Ɏ)ƫ.Ȳ(ö,0);return true;}if(string.Compare(Ă,"boot",true)==0){ö.ϻ=0;
if(!ö.Ɏ)ƫ.Ȳ(ö,0);return true;}if(Ă.Ǹ("scroll")){σ ā=new σ(à,this,Ă);ƫ.Ȳ(ā,0);return true;}if(string.Compare(Ă,"props",true
)==0){В Ā=à.ŋ;List<IMyTerminalBlock>Ñ=new List<IMyTerminalBlock>();List<ITerminalAction>Ƭ=new List<ITerminalAction>();
List<ITerminalProperty>ǒ=new List<ITerminalProperty>();IMyTextPanel ĳ=ƫ.Ǒ.GridTerminalSystem.GetBlockWithName("DEBUG")as
IMyTextPanel;if(ĳ==null){return true;}ĳ.WriteText("Properties: ");foreach(var k in Ā.Ў){ĳ.WriteText(k.Key+" =============="+"\n",
true);k.Value(Ñ,null);if(Ñ.Count<=0){ĳ.WriteText("No blocks\n",true);continue;}Ñ[0].GetProperties(ǒ,(î)=>{return î.Id!=
"Name"&&î.Id!="OnOff"&&!î.Id.StartsWith("Show");});foreach(var Ȼ in ǒ){ĳ.WriteText("P "+Ȼ.Id+" "+Ȼ.TypeName+"\n",true);}ǒ.
Clear();Ñ.Clear();}}return false;}}class Ⱥ{Dictionary<string,á>ƅ=new Dictionary<string,á>();List<string>w=new List<string>();
public void v(string e,á k){if(!ƅ.ContainsKey(e)){w.Add(e);ƅ.Add(e,k);}}public int j(){return ƅ.Count;}public á f(string e){if
(ƅ.ContainsKey(e))return ƅ[e];return null;}public á b(int a){return ƅ[w[a]];}public void n(string e){ƅ.Remove(e);w.Remove
(e);}public void Y(){w.Clear();ƅ.Clear();}public void Z(){w.Sort();}}class ȸ{Ț ƫ;ɬ à;public MyDefinitionId ȷ=new
MyDefinitionId(typeof(VRage.Game.ObjectBuilders.Definitions.MyObjectBuilder_GasProperties),"Electricity");public MyDefinitionId ȹ=new
MyDefinitionId(typeof(VRage.Game.ObjectBuilders.Definitions.MyObjectBuilder_GasProperties),"Oxygen");public MyDefinitionId ȶ=new
MyDefinitionId(typeof(VRage.Game.ObjectBuilders.Definitions.MyObjectBuilder_GasProperties),"Hydrogen");public ȸ(Ț ƣ,ɬ V){ƫ=ƣ;à=V;}int
ɇ=0;public bool Ɇ(List<IMyTerminalBlock>Ñ,ref double ȿ,ref double Ⱦ,ref double Ƚ,ref double ȼ,ref double Ʌ,ref double Ʉ,
bool ò){if(!ò)ɇ=0;MyResourceSinkComponent ȟ;MyResourceSourceComponent ȵ;for(;ɇ<Ñ.Count;ɇ++){if(!ƫ.ʕ(8))return false;if(Ñ[ɇ].
Components.TryGet<MyResourceSinkComponent>(out ȟ)){ȿ+=ȟ.CurrentInputByType(ȷ);Ⱦ+=ȟ.MaxRequiredInputByType(ȷ);}if(Ñ[ɇ].Components.
TryGet<MyResourceSourceComponent>(out ȵ)){Ƚ+=ȵ.CurrentOutputByType(ȷ);ȼ+=ȵ.MaxOutputByType(ȷ);}IMyBatteryBlock Ƀ=(Ñ[ɇ]as
IMyBatteryBlock);Ʌ+=Ƀ.CurrentStoredPower;Ʉ+=Ƀ.MaxStoredPower;}return true;}int ɂ=0;public bool Ɂ(List<IMyTerminalBlock>Ñ,MyDefinitionId
ɀ,ref double ȿ,ref double Ⱦ,ref double Ƚ,ref double ȼ,bool ò){if(!ò)ɂ=0;MyResourceSinkComponent ȟ;
MyResourceSourceComponent ȵ;for(;ɂ<Ñ.Count;ɂ++){if(!ƫ.ʕ(6))return false;if(Ñ[ɂ].Components.TryGet<MyResourceSinkComponent>(out ȟ)){ȿ+=ȟ.
CurrentInputByType(ɀ);Ⱦ+=ȟ.MaxRequiredInputByType(ɀ);}if(Ñ[ɂ].Components.TryGet<MyResourceSourceComponent>(out ȵ)){Ƚ+=ȵ.
CurrentOutputByType(ɀ);ȼ+=ȵ.MaxOutputByType(ɀ);}}return true;}int Ȥ=0;public bool Ȓ(List<IMyTerminalBlock>Ñ,string Ȣ,ref double ȡ,ref
double Ƞ,bool ò){if(!ò){Ȥ=0;Ƞ=0;ȡ=0;}MyResourceSinkComponent ȟ;for(;Ȥ<Ñ.Count;Ȥ++){if(!ƫ.ʕ(30))return false;IMyGasTank ň=Ñ[Ȥ]
as IMyGasTank;if(ň==null)continue;double Ȟ=0;if(ň.Components.TryGet<MyResourceSinkComponent>(out ȟ)){ListReader<
MyDefinitionId>ȝ=ȟ.AcceptedResources;int X=0;for(;X<ȝ.Count;X++){if(string.Compare(ȝ[X].SubtypeId.ToString(),Ȣ,true)==0){Ȟ=ň.Capacity;
Ƞ+=Ȟ;ȡ+=Ȟ*ň.FilledRatio;break;}}}}return true;}public string ȣ(TimeSpan Ȝ){string Ĵ="";if(Ȝ.Ticks<=0)return"-";if((int)Ȝ.
TotalDays>0)Ĵ+=(long)Ȝ.TotalDays+" "+à.ğ.ǳ("C5")+" ";if(Ȝ.Hours>0||Ĵ!="")Ĵ+=Ȝ.Hours+"h ";if(Ȝ.Minutes>0||Ĵ!="")Ĵ+=Ȝ.Minutes+"m ";
return Ĵ+Ȝ.Seconds+"s";}}class Ț{public const double ș=0.05;public const int Ș=1000;public const int ȗ=10000;public double Ȗ{
get{return Ȕ;}}int ȕ=Ș;double Ȕ=0;List<ɔ>ȓ=new List<ɔ>(100);public MyGridProgram Ǒ;public bool ț=false;int ȥ=0;public Ț(
MyGridProgram Ǆ,int ǃ=1,bool ȳ=false){Ǒ=Ǆ;ȥ=ǃ;ț=ȳ;}public void Ȳ(ɔ í,double ȱ,bool Ȱ=false){í.Ɏ=true;í.ɋ(this);if(Ȱ){í.ɘ=Ȗ;ȓ.Insert(0
,í);return;}if(ȱ<=0)ȱ=0.001;í.ɘ=Ȗ+ȱ;for(int X=0;X<ȓ.Count;X++){if(ȓ[X].ɘ>í.ɘ){ȓ.Insert(X,í);return;}if(í.ɘ-ȓ[X].ɘ<ș)í.ɘ=ȓ
[X].ɘ+ș;}ȓ.Add(í);}public void ȯ(ɔ í){if(ȓ.Contains(í)){ȓ.Remove(í);í.Ɏ=false;}}public void Ȯ(ʐ ȴ,int Ȭ=1){if(ȥ==Ȭ)Ǒ.Echo
(ȴ.ɖ());}public void Ȯ(string ȭ,int Ȭ=1){if(ȥ==Ȭ)Ǒ.Echo(ȭ);}const double ȫ=(16.66666666/16);double Ȫ=0;public void ȩ(){Ȫ
+=Ǒ.Runtime.TimeSinceLastRun.TotalSeconds*ȫ;}ʐ ƴ=new ʐ();public void Ȩ(){double ȧ=Ǒ.Runtime.TimeSinceLastRun.TotalSeconds*
ȫ+Ȫ;Ȫ=0;Ȕ+=ȧ;ȕ=(int)Math.Min((ȧ*60)*Ș/(ț?5:1),ȗ-1000);while(ȓ.Count>=1){ɔ í=ȓ[0];if(ȕ-Ǒ.Runtime.CurrentInstructionCount<=
0)break;if(í.ɘ>Ȕ){int Ɉ=(int)(60*(í.ɘ-Ȕ));if(Ɉ>=100){Ǒ.Runtime.UpdateFrequency=UpdateFrequency.Update100;}else{if(Ɉ>=10||
ț)Ǒ.Runtime.UpdateFrequency=UpdateFrequency.Update10;else Ǒ.Runtime.UpdateFrequency=UpdateFrequency.Update1;}break;}ȓ.
Remove(í);if(!í.ə())break;}}public int ɉ(){return(ȗ-Ǒ.Runtime.CurrentInstructionCount);}public bool ʕ(int ʊ){return((ȕ-Ǒ.
Runtime.CurrentInstructionCount)>=ʊ);}public void ʈ(){Ȯ(ƴ.Ů().ɱ("Remaining Instr: ").ɱ(ɉ()));}}class ʇ:ɔ{MyShipVelocities ʆ;
public Vector3D ʅ{get{return ʆ.LinearVelocity;}}public Vector3D ʄ{get{return ʆ.AngularVelocity;}}double ʃ=0;public double ʂ{
get{if(ɳ!=null)return ɳ.GetShipSpeed();else return ʃ;}}double ʁ=0;public double ʀ{get{return ʁ;}}double ɿ=0;public double ʉ
{get{return ɿ;}}double ɾ=0;double ɼ=0;public double ɻ{get{return ɾ;}}MyShipMass ɺ;public double ɹ{get{return ɺ.TotalMass;
}}public double ɸ{get{return ɺ.BaseMass;}}double ɷ=double.NaN;public double ɶ{get{return ɷ;}}double ɵ=double.NaN;public
double ɴ{get{return ɵ;}}IMyShipController ɳ=null;IMySlimBlock ɽ=null;public IMyShipController ɲ{get{return ɳ;}}Vector3D ʋ;
public ʇ(Ț ƣ){ɓ="ShipMgr";ƫ=ƣ;ʋ=ƫ.Ǒ.Me.GetPosition();ɐ=0.5;}List<IMyTerminalBlock>ʔ=new List<IMyTerminalBlock>();int ʓ=0;
public override bool ɮ(bool ò){if(!ò){ʔ.Clear();ƫ.Ǒ.GridTerminalSystem.GetBlocksOfType<IMyShipController>(ʔ);ʓ=0;if(ɳ!=null&&ɳ
.CubeGrid.GetCubeBlock(ɳ.Position)!=ɽ)ɳ=null;}if(ʔ.Count>0){for(;ʓ<ʔ.Count;ʓ++){if(!ƫ.ʕ(20))return false;
IMyShipController ʒ=ʔ[ʓ]as IMyShipController;if(ʒ.IsMainCockpit||ʒ.IsUnderControl){ɳ=ʒ;ɽ=ʒ.CubeGrid.GetCubeBlock(ʒ.Position);if(ʒ.
IsMainCockpit){ʓ=ʔ.Count;break;}}}if(ɳ==null){ɳ=ʔ[0]as IMyShipController;ɽ=ɳ.CubeGrid.GetCubeBlock(ɳ.Position);}ɺ=ɳ.CalculateShipMass
();if(!ɳ.TryGetPlanetElevation(MyPlanetElevation.Sealevel,out ɷ))ɷ=double.NaN;if(!ɳ.TryGetPlanetElevation(
MyPlanetElevation.Surface,out ɵ))ɵ=double.NaN;ʆ=ɳ.GetShipVelocities();}double ʑ=ʃ;ʃ=ʅ.Length();ʁ=(ʃ-ʑ)/ɑ;if(-ʁ>ɿ)ɿ=-ʁ;if(-ʁ>ɾ){ɾ=-ʁ;ɼ=ƫ.Ȗ
;}if(ƫ.Ȗ-ɼ>5&&-ʁ>0.1)ɾ-=(ɾ+ʁ)*0.3f;return true;}}class ʐ{public StringBuilder ƴ;public ʐ(int ʏ=0){ƴ=new StringBuilder(ʏ);
}public int ʎ{get{return ƴ.Length;}}public ʐ Ů(){ƴ.Clear();return this;}public ʐ ɱ(string Ħ){ƴ.Append(Ħ);return this;}
public ʐ ɱ(double ʍ){ƴ.Append(ʍ);return this;}public ʐ ɱ(char Ǫ){ƴ.Append(Ǫ);return this;}public ʐ ɱ(ʐ ʌ){ƴ.Append(ʌ.ƴ);return
this;}public ʐ ɱ(string Ħ,int ȉ,int ɕ){ƴ.Append(Ħ,ȉ,ɕ);return this;}public ʐ ɱ(char Ǫ,int ř){ƴ.Append(Ǫ,ř);return this;}
public ʐ ɗ(int ȉ,int ɕ){ƴ.Remove(ȉ,ɕ);return this;}public string ɖ(){return ƴ.ToString();}public string ɖ(int ȉ,int ɕ){return
ƴ.ToString(ȉ,ɕ);}public char this[int e]{get{return ƴ[e];}}}class ɔ{public string ɓ="MMTask";public double ɘ=0;public
double ɒ=0;public double ɑ=0;public double ɐ=-1;double ɏ=-1;public bool Ɏ=false;public bool ɍ=false;double Ɍ=0;protected Ț ƫ;
public void ɋ(Ț ƣ){ƫ=ƣ;if(ƫ.ț){if(ɏ==-1){ɏ=ɐ;ɐ*=2;}else{ɐ=ɏ*2;}}else{if(ɏ!=-1){ɐ=ɏ;ɏ=-1;}}}protected bool Ɋ(double ȱ){Ɍ=Math.
Max(ȱ,0.0001);return true;}public bool ə(){if(ɒ>0){ɑ=ƫ.Ȗ-ɒ;ƫ.Ȯ((ɍ?"Running":"Resuming")+" task: "+ɓ);ɍ=ɮ(!ɍ);}else{ɑ=0;ƫ.Ȯ(
"Init task: "+ɓ);ɰ();ƫ.Ȯ("Running..");ɍ=ɮ(false);if(!ɍ)ɒ=0.001;}if(ɍ){ɒ=ƫ.Ȗ;if((ɐ>=0||Ɍ>0)&&Ɏ)ƫ.Ȳ(this,(Ɍ>0?Ɍ:ɐ));else{Ɏ=false;ɒ=0;}}
else{if(Ɏ)ƫ.Ȳ(this,0,true);}ƫ.Ȯ("Task "+(ɍ?"":"NOT ")+"finished. "+(Ɏ?(Ɍ>0?"Postponed by "+Ɍ.ToString("F1")+"s":
"Scheduled after "+ɐ.ToString("F1")+"s"):"Stopped."));Ɍ=0;return ɍ;}public void ɤ(){ƫ.ȯ(this);ɭ();Ɏ=false;ɍ=false;ɒ=0;}public virtual void
ɰ(){}public virtual bool ɮ(bool ò){return true;}public virtual void ɭ(){}}class ɬ{public const float ɫ=512;public const
float ɪ=ɫ/0.7783784f;public const float ɩ=ɫ/0.7783784f;public const float ɨ=ɪ;public const float ɧ=37;public string ɦ=
"T:[LCD]";public int ɯ=1;public bool ɥ=true;public List<string>ɣ=null;public bool ɢ=true;public int ȥ=0;public float ɡ=1.0f;
public float ɠ=1.0f;public float ɟ{get{return ɨ*ǂ.Ѥ;}}public float ɞ{get{return(float)ɟ-2*ǐ[Ǔ]*Ú;}}string ɝ;string ɜ;float ɛ=-
1;Dictionary<string,float>ɚ=new Dictionary<string,float>(2);Dictionary<string,float>Ȧ=new Dictionary<string,float>(2);
Dictionary<string,float>ȑ=new Dictionary<string,float>(2);public float Ʒ{get{return ȑ[Ǔ];}}Dictionary<string,float>ǐ=new
Dictionary<string,float>(2);Dictionary<string,float>Ǐ=new Dictionary<string,float>(2);Dictionary<string,float>ǎ=new Dictionary<
string,float>(2);int Ú=0;string Ù="";Dictionary<string,char>Ǎ=new Dictionary<string,char>(2);Dictionary<string,char>ǌ=new
Dictionary<string,char>(2);Dictionary<string,char>ǋ=new Dictionary<string,char>(2);Dictionary<string,char>Ǌ=new Dictionary<string,
char>(2);public Ț ƫ;public Program Ǒ;public ȸ ǉ;public В ŋ;public ʇ ǈ;public Ï Ʀ;public ǫ ğ;public IMyGridTerminalSystem Ǉ{
get{return Ǒ.GridTerminalSystem;}}public IMyProgrammableBlock ǆ{get{return Ǒ.Me;}}public Action<string>ǅ{get{return Ǒ.Echo;
}}public ɬ(Program Ǆ,int ǃ,Ț ƣ){ƫ=ƣ;ȥ=ǃ;Ǒ=Ǆ;ğ=new ǫ();ǉ=new ȸ(ƣ,this);ŋ=new В(ƣ,this);ŋ.Ѝ();ǈ=new ʇ(ƫ);ƫ.Ȳ(ǈ,0);}ѥ ǂ=null
;public string Ǔ{get{return ǂ.L;}}public bool Ǜ{get{return(ǂ.ŝ()==0);}}public bool ǚ(IMyTerminalBlock Ý){if(Ý==null||Ý.
WorldMatrix==MatrixD.Identity)return true;return Ǉ.GetBlockWithId(Ý.EntityId)==null;}public ѥ Ǚ(ѥ ǘ,á î){î.I();IMyTextSurface C=î.Õ
;if(ǘ==null)ǘ=new ѥ(this);ǘ.L=C.Font;if(!ǐ.ContainsKey(ǘ.L))ǘ.L=ɝ;ǘ.Ѥ=(C.SurfaceSize.X/C.TextureSize.X)*(C.TextureSize.X/
C.TextureSize.Y)*ɡ/C.FontSize*(100f-C.TextPadding*2)/100*î.K();Ù=î.Ù;Ú=î.Ú;ǂ=ǘ;return ǘ;}public ş Ǘ(ş Þ,á î){î.I();
IMyTextSurface C=î.Õ;if(Þ==null)Þ=new ş(this);Þ.ű(î.Ô);Þ.Ũ=î.O()*(C.SurfaceSize.Y/C.TextureSize.Y)*ɠ/C.FontSize*(100f-C.TextPadding*2)
/100;Þ.Ű();Ù=î.Ù;Ú=î.Ú;return Þ;}public void ǖ(){ǂ.ť();}public void ǖ(ʐ Ť){if(ǂ.ѣ<=0)ǂ.ÿ(Ù);ǂ.ÿ(Ť);ǂ.ť();}public void ǖ(
string Ť){if(ǂ.ѣ<=0)ǂ.ÿ(Ù);ǂ.ť(Ť);}public void Ǖ(string Š){ǂ.š(Š,Ù);}public void ǔ(List<ʐ>ŧ){ǂ.ţ(ŧ);}public void Ë(ʐ Ŧ,bool Ƽ=
true){if(ǂ.ѣ<=0)ǂ.ÿ(Ù);ǂ.ÿ(Ŧ);if(Ƽ)ǂ.ѣ+=Ǩ(Ŧ,ǂ.L);}public void Ë(string Ĵ,bool Ƽ=true){if(ǂ.ѣ<=0)ǂ.ÿ(Ù);ǂ.ÿ(Ĵ);if(Ƽ)ǂ.ѣ+=Ǩ(Ĵ,
ǂ.L);}public void Ƶ(ʐ Ŧ,float Ʊ=1.0f,float ư=0f){Ʋ(Ŧ,Ʊ,ư);ǂ.ť();}public void Ƶ(string Ĵ,float Ʊ=1.0f,float ư=0f){Ʋ(Ĵ,Ʊ,ư)
;ǂ.ť();}ʐ ƴ=new ʐ();public void Ʋ(ʐ Ŧ,float Ʊ=1.0f,float ư=0f){float Ư=Ǩ(Ŧ,ǂ.L);float Ʈ=Ʊ*ɨ*ǂ.Ѥ-ǂ.ѣ-ư;if(Ú>0)Ʈ-=2*ǐ[ǂ.L]*
Ú;if(Ʈ<Ư){ǂ.ÿ(Ŧ);ǂ.ѣ+=Ư;return;}Ʈ-=Ư;int ƭ=(int)Math.Floor(Ʈ/ǐ[ǂ.L]);float Ƴ=ƭ*ǐ[ǂ.L];ƴ.Ů().ɱ(' ',ƭ).ɱ(Ŧ);ǂ.ÿ(ƴ);ǂ.ѣ+=Ƴ+Ư
;}public void Ʋ(string Ĵ,float Ʊ=1.0f,float ư=0f){float Ư=Ǩ(Ĵ,ǂ.L);float Ʈ=Ʊ*ɨ*ǂ.Ѥ-ǂ.ѣ-ư;if(Ú>0)Ʈ-=2*ǐ[ǂ.L]*Ú;if(Ʈ<Ư){ǂ.ÿ
(Ĵ);ǂ.ѣ+=Ư;return;}Ʈ-=Ư;int ƭ=(int)Math.Floor(Ʈ/ǐ[ǂ.L]);float Ƴ=ƭ*ǐ[ǂ.L];ƴ.Ů().ɱ(' ',ƭ).ɱ(Ĵ);ǂ.ÿ(ƴ);ǂ.ѣ+=Ƴ+Ư;}public void
ƶ(ʐ Ŧ){ǀ(Ŧ);ǂ.ť();}public void ƶ(string Ĵ){ǀ(Ĵ);ǂ.ť();}public void ǀ(ʐ Ŧ){float Ư=Ǩ(Ŧ,ǂ.L);float ǁ=ɨ/2*ǂ.Ѥ-ǂ.ѣ;if(ǁ<Ư/2){
ǂ.ÿ(Ŧ);ǂ.ѣ+=Ư;return;}ǁ-=Ư/2;int ƭ=(int)Math.Round(ǁ/ǐ[ǂ.L],MidpointRounding.AwayFromZero);float Ƴ=ƭ*ǐ[ǂ.L];ƴ.Ů().ɱ(' ',ƭ
).ɱ(Ŧ);ǂ.ÿ(ƴ);ǂ.ѣ+=Ƴ+Ư;}public void ǀ(string Ĵ){float Ư=Ǩ(Ĵ,ǂ.L);float ǁ=ɨ/2*ǂ.Ѥ-ǂ.ѣ;if(ǁ<Ư/2){ǂ.ÿ(Ĵ);ǂ.ѣ+=Ư;return;}ǁ-=Ư
/2;int ƭ=(int)Math.Round(ǁ/ǐ[ǂ.L],MidpointRounding.AwayFromZero);float Ƴ=ƭ*ǐ[ǂ.L];ƴ.Ů().ɱ(' ',ƭ).ɱ(Ĵ);ǂ.ÿ(ƴ);ǂ.ѣ+=Ƴ+Ư;}
public void ƿ(double ƾ,float ƽ=1.0f,float ư=0f,bool Ƽ=true){if(Ú>0)ư+=2*Ú*ǐ[ǂ.L];float ƻ=ɨ*ƽ*ǂ.Ѥ-ǂ.ѣ-ư;if(Double.IsNaN(ƾ))ƾ=0;
int ƺ=(int)(ƻ/Ǐ[ǂ.L])-2;if(ƺ<=0)ƺ=2;int ƹ=Math.Min((int)(ƾ*ƺ)/100,ƺ);if(ƹ<0)ƹ=0;if(ǂ.ѣ<=0)ǂ.ÿ(Ù);ƴ.Ů().ɱ(Ǎ[ǂ.L]).ɱ(Ǌ[ǂ.L],ƹ
).ɱ(ǋ[ǂ.L],ƺ-ƹ).ɱ(ǌ[ǂ.L]);ǂ.ÿ(ƴ);if(Ƽ)ǂ.ѣ+=Ǐ[ǂ.L]*ƺ+2*ǎ[ǂ.L];}public void Ƹ(double ƾ,float ƽ=1.0f,float ư=0f){ƿ(ƾ,ƽ,ư,
false);ǂ.ť();}public void Ş(){ǂ.Ş();}public void Ȃ(á o,ş D=null){o.E(D);if(o.Ö)o.Þ.Ķ();}public void ȁ(string Ȁ,string Ĵ){
IMyTextPanel o=Ǒ.GridTerminalSystem.GetBlockWithName(Ȁ)as IMyTextPanel;if(o==null)return;o.WriteText(Ĵ+"\n",true);}public string ǿ(
MyInventoryItem k){string Ǿ=k.Type.TypeId.ToString();Ǿ=Ǿ.Substring(Ǿ.LastIndexOf('_')+1);return k.Type.SubtypeId+" "+Ǿ;}public void ȃ(
string Å,out string Ã,out string Â){int ļ=Å.LastIndexOf(' ');if(ļ>=0){Ã=Å.Substring(0,ļ);Â=Å.Substring(ļ+1);return;}Ã=Å;Â="";}
public string ǽ(string Å){string Ã,Â;ȃ(Å,out Ã,out Â);return ǽ(Ã,Â);}public string ǽ(string Ã,string Â){È k=Ʀ.Ä(Ã,Â);if(k!=
null){if(k.é.Length>0)return k.é;return k.Á;}return System.Text.RegularExpressions.Regex.Replace(Ã,"([a-z])([A-Z])","$1 $2")
;}public void Ǽ(ref string Ã,ref string Â){È k;if(Ʀ.Ì.TryGetValue(Ã,out k)){Ã=k.Á;Â=k.Ò;return;}k=Ʀ.Ä(Ã,Â);if(k!=null){Ã=
k.Á;if((string.Compare(Â,"Ore",true)==0)||(string.Compare(Â,"Ingot",true)==0))return;Â=k.Ò;}}public string Ȑ(double Ȏ,
bool ȍ=true,char Ȍ=' '){if(!ȍ)return Ȏ.ToString("#,###,###,###,###,###,###,###,###,###");string ȋ=" kMGTPEZY";double Ȋ=Ȏ;int
ȉ=ȋ.IndexOf(Ȍ);var Ȉ=(ȉ<0?0:ȉ);while(Ȋ>=1000&&Ȉ+1<ȋ.Length){Ȋ/=1000;Ȉ++;}ƴ.Ů().ɱ(Math.Round(Ȋ,1,MidpointRounding.
AwayFromZero));if(Ȉ>0)ƴ.ɱ(" ").ɱ(ȋ[Ȉ]);return ƴ.ɖ();}public string ȏ(double Ȏ,bool ȍ=true,char Ȍ=' '){if(!ȍ)return Ȏ.ToString(
"#,###,###,###,###,###,###,###,###,###");string ȋ=" ktkMGTPEZY";double Ȋ=Ȏ;int ȉ=ȋ.IndexOf(Ȍ);var Ȉ=(ȉ<0?0:ȉ);while(Ȋ>=1000&&Ȉ+1<ȋ.Length){Ȋ/=1000;Ȉ++;}ƴ.Ů().ɱ
(Math.Round(Ȋ,1,MidpointRounding.AwayFromZero));if(Ȉ==1)ƴ.ɱ(" kg");else if(Ȉ==2)ƴ.ɱ(" t");else if(Ȉ>2)ƴ.ɱ(" ").ɱ(ȋ[Ȉ]).ɱ(
"t");return ƴ.ɖ();}public string ȇ(double ƾ){return(Math.Floor(ƾ*10)/10).ToString("F1");}Dictionary<char,float>Ȇ=new
Dictionary<char,float>();void ȅ(string Ȅ,float B){B+=1;for(int X=0;X<Ȅ.Length;X++){if(B>ɚ[ɝ])ɚ[ɝ]=B;Ȇ.Add(Ȅ[X],B);}}public float ǻ
(char Ǫ,string L){float ƻ;if(L==ɜ||!Ȇ.TryGetValue(Ǫ,out ƻ))return ɚ[L];return ƻ;}public float Ǩ(ʐ ǩ,string L){if(L==ɜ)
return ǩ.ʎ*ɚ[L];float ǧ=0;for(int X=0;X<ǩ.ʎ;X++)ǧ+=ǻ(ǩ[X],L);return ǧ;}public float Ǩ(string Ħ,string L){if(L==ɜ)return Ħ.
Length*ɚ[L];float ǧ=0;for(int X=0;X<Ħ.Length;X++)ǧ+=ǻ(Ħ[X],L);return ǧ;}public string Ǧ(string Ĵ,float Ǥ){if(Ǥ/ɚ[ǂ.L]>=Ĵ.
Length)return Ĵ;float ǣ=Ǩ(Ĵ,ǂ.L);if(ǣ<=Ǥ)return Ĵ;float Ǣ=ǣ/Ĵ.Length;Ǥ-=Ȧ[ǂ.L];int ǡ=(int)Math.Max(Ǥ/Ǣ,1);if(ǡ<Ĵ.Length/2){ƴ.Ů
().ɱ(Ĵ,0,ǡ);ǣ=Ǩ(ƴ,ǂ.L);}else{ƴ.Ů().ɱ(Ĵ);ǡ=Ĵ.Length;}while(ǣ>Ǥ&&ǡ>1){ǡ--;ǣ-=ǻ(Ĵ[ǡ],ǂ.L);}if(ƴ.ʎ>ǡ)ƴ.ɗ(ǡ,ƴ.ʎ-ǡ);return ƴ.ɱ(
"..").ɖ();}void Ǡ(string ǟ){ɝ=ǟ;Ǎ[ɝ]=MMStyle.BAR_START;ǌ[ɝ]=MMStyle.BAR_END;ǋ[ɝ]=MMStyle.BAR_EMPTY;Ǌ[ɝ]=MMStyle.BAR_FILL;ɚ[ɝ
]=0f;}void Ǟ(string ǝ,float ǜ){ɜ=ǝ;ɛ=ǜ;ɚ[ɜ]=ɛ+1;Ȧ[ɜ]=2*(ɛ+1);Ǎ[ɜ]=MMStyle.BAR_MONO_START;ǌ[ɜ]=MMStyle.BAR_MONO_END;ǋ[ɜ]=
MMStyle.BAR_MONO_EMPTY;Ǌ[ɜ]=MMStyle.BAR_MONO_FILL;ǐ[ɜ]=ǻ(' ',ɜ);Ǐ[ɜ]=ǻ(ǋ[ɜ],ɜ);ǎ[ɜ]=ǻ(Ǎ[ɜ],ɜ);ȑ[ɜ]=Ǩ(" 100.0%",ɜ);}public void
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
        ǐ[ɝ]=ǻ(' ',ɝ);Ǐ[ɝ]=ǻ(ǋ[ɝ],ɝ);ǎ[ɝ]=ǻ(Ǎ[ɝ],ɝ);ȑ[ɝ]=Ǩ(" 100.0%",ɝ);Ȧ[ɝ]=ǻ('.',ɝ)*2;}}class ǫ{public string ǳ(string
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
}static class ǹ{public static bool Ǹ(this string Ħ,string Ƕ){return Ħ.StartsWith(Ƕ,StringComparison.
InvariantCultureIgnoreCase);}public static bool Ƿ(this string Ħ,string Ƕ){if(Ħ==null)return false;return Ħ.IndexOf(Ƕ,StringComparison.
InvariantCultureIgnoreCase)>=0;}public static bool ǵ(this string Ħ,string Ƕ){return Ħ.EndsWith(Ƕ,StringComparison.InvariantCultureIgnoreCase);}}
static class Ǵ{public static string ǲ(this IMyTerminalBlock Ý){int Ł=Ý.CustomData.IndexOf("\n---\n");if(Ł<0){if(Ý.CustomData.
StartsWith("---\n"))return Ý.CustomData.Substring(4);return Ý.CustomData;}return Ý.CustomData.Substring(Ł+5);}public static string
Ǳ(this IMyTerminalBlock Ý,int ļ,string ǰ){string ǯ=Ý.ǲ();string Ǯ="@"+ļ.ToString()+" AutoLCD";string ǭ='\n'+Ǯ;int Ł=0;if(
!ǯ.StartsWith(Ǯ,StringComparison.InvariantCultureIgnoreCase)){Ł=ǯ.IndexOf(ǭ,StringComparison.InvariantCultureIgnoreCase);
}if(Ł<0){if(ļ==0){if(ǯ.Length==0)return"";if(ǯ[0]=='@')return null;Ł=ǯ.IndexOf("\n@");if(Ł<0)return ǯ;return ǯ.Substring(
0,Ł);}else return null;}int Ǭ=ǯ.IndexOf("\n@",Ł+1);if(Ǭ<0){if(Ł==0)return ǯ;return ǯ.Substring(Ł+1);}if(Ł==0)return ǯ.
Substring(0,Ǭ);return ǯ.Substring(Ł+1,Ǭ-Ł);}