/* v:2.0184 (Same construct filtering, Details improvements, ChargeSum variant)
* Automatic LCDs 2 - In-game script by MMaster
*
* Thank all of you for making amazing creations with this script, using it and helping each other use it.
* Its 2022 - it's been 7 years already since I uploaded first Automatic LCDs script and you are still using it (in "a bit" upgraded form).
* That's just amazing! I hope you will have many more years of fun with it :)
*
* LATEST UPDATE: 
*  Added ChargeSum variant showing just single output for all matched jump drives (check Charge and ChargeTime command guide)
*  Added ability to specify text to end the output or number of lines to print with Details command (check Details command guide)
*  Added C: modifier to name filter to filter on same construct as programmable block (on rotors & pistons, but not connectors)
*    Note: C: modifier works in the same way as T: modifier used for same grid filtering - check guide section 'Same construct blocks filtering'!
*  Updated default Inventory ingots quotas to match current survival game better (in my and my friends opinion :) )
*  Updated magazines to match current game version (check guide of Inventory command for list)
*  Workaround for large antenna reporting higher power use than max required
*  
* Previous notable updates:
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
    Add("Canvas", "Component", 300, "Leinwand");
    Add("ZoneChip", "Component", 100, "Zonen-Chip");
    Add("Datapad", "Datapad", 0, "Datapad");
    Add("Medkit", "ConsumableItem", 0, "MedKit");
    Add("Powerkit", "ConsumableItem", 0, "Powerkit");
    Add("SpaceCredit", "PhysicalObject", 0, "Space Credit");
    Add("NATO_5p56x45mm", "Ammo", 8000, "5.56x45mm NATO Munitionskasten", "5.56x45mm", false);
    Add("SemiAutoPistolMagazine", "Ammo", 500, "S-10-Magazin");
    Add("ElitePistolMagazine", "Ammo", 500, "S-10E-Magazin");
    Add("FullAutoPistolMagazine", "Ammo", 500, "S-20A-Magazin");
    Add("AutomaticRifleGun_Mag_20rd", "Ammo", 1000, "MR-20-Magazin");
    Add("PreciseAutomaticRifleGun_Mag_5rd", "Ammo", 1000, "MR-8P-Magazin");
    Add("RapidFireAutomaticRifleGun_Mag_50rd", "Ammo", 8000, "MR-50A-Magazin");
    Add("UltimateAutomaticRifleGun_Mag_30rd", "Ammo", 1000, "MR-30E-Magazin");
    Add("NATO_25x184mm", "Ammo", 2500, "25x184mm NATO Munitionskasten", "25x184mm");
    Add("Missile200mm", "Ammo", 1600, "200-mm-Raketenbehälter");
    Add("AutocannonClip", "Ammo", 50, "AC Mag");
    Add("MediumCalibreAmmo", "Ammo", 50, "AC Shell");
    Add("SmallRailgunAmmo", "Ammo", 50, "Small Sabot");
    Add("LargeRailgunAmmo", "Ammo", 50, "Large Sabot");
    Add("LargeCalibreAmmo", "Ammo", 50, "Arty Shell");
    Add("OxygenBottle", "OxygenContainerObject", 5, "Sauerstoffflasche");
    Add("HydrogenBottle", "GasContainerObject", 5, "Wasserstoffflasche");
    // MODDED ITEMS
    // (subType, mainType, quota, display name, short name, used)
    // * if used is true, item will be shown in inventory even for 0 items
    // * if used is false, item will be used only for display name and short name
    // AzimuthSupercharger
    Add("AzimuthSupercharger", "Component", 1600, "Supercharger", "supercharger", false);
    // OKI Ammo
    Add("OKI23mmAmmo", "Ammo", 500, "23x180mm", "23x180mm", false);
    Add("OKI50mmAmmo", "Ammo", 500, "50x450mm", "50x450mm", false);
    Add("OKI122mmAmmo", "Ammo", 200, "122x640mm", "122x640mm", false);
    Add("OKI230mmAmmo", "Ammo", 100, "230x920mm", "230x920mm", false);

    // REALLY REALLY REALLY
    // DO NOT MODIFY ANYTHING BELOW THIS (TRANSLATION STRINGS ARE AT THE BOTTOM)
}
void Add(string sT, string mT, int q = 0, string dN = "", string sN = "", bool u = true) { ƥ.Ì(sT, mT, q, dN, sN, u); }
Ð ƥ;ș Ƣ;ü Ϣ;ɫ Z=null;void ϡ(string Ɠ){}bool Ϡ(string ϝ){return ϝ.Ƕ("true")?true:false;}void ϟ(string Ϟ,string ϝ){string
ǿ=Ϟ.ToLower();switch(ǿ){case"lcd_tag":LCD_TAG=ϝ;break;case"slowmode":SlowMode=Ϡ(ϝ);break;case"enable_boot":ENABLE_BOOT=Ϡ(
ϝ);break;case"skip_content_type":SKIP_CONTENT_TYPE=Ϡ(ϝ);break;case"scroll_lines":int Ϝ=0;if(int.TryParse(ϝ,out Ϝ)){
SCROLL_LINES=Ϝ;}break;}}void ϛ(){string[]Ŧ=Me.CustomData.Split('\n');for(int Y=0;Y<Ŧ.Length;Y++){string ţ=Ŧ[Y];int ĺ=ţ.IndexOf('=');
if(ĺ<0){ϡ(ţ);continue;}string Ϛ=ţ.Substring(0,ĺ).Trim();string ǵ=ţ.Substring(ĺ+1).Trim();ϟ(Ϛ,ǵ);}}void ϙ(ș Ƣ){ƥ=new Ð();
ItemsConf();ϛ();Z=new ɫ(this,DebugLevel,Ƣ){ƥ=ƥ,ɥ=LCD_TAG,ɮ=SCROLL_LINES,ɤ=ENABLE_BOOT,ɢ=BOOT_FRAMES,ɡ=!MDK_IS_GREAT,ɟ=HEIGHT_MOD,
ɠ=WIDTH_MOD};Z.Ǥ();}void Ϙ(){Ƣ.ǐ=this;Z.ǐ=this;}Program(){Runtime.UpdateFrequency=UpdateFrequency.Update1;}void Main(
string Ā,UpdateType τ){try{if(Ƣ==null){Ƣ=new ș(this,DebugLevel,SlowMode);ϙ(Ƣ);Ϣ=new ü(Z);Ƣ.ȱ(Ϣ,0);}else{Ϙ();Z.Ŋ.Ѝ();}if(Ā.
Length==0&&(τ&(UpdateType.Update1|UpdateType.Update10|UpdateType.Update100))==0){Ƣ.Ȩ();return;}if(Ā!=""){if(Ϣ.ā(Ā)){Ƣ.Ȩ();
return;}}Ϣ.û=0;Ƣ.ȧ();}catch(Exception ex){Echo("ERROR DESCRIPTION:\n"+ex.ToString());Me.Enabled=false;}}class σ:ɓ{ü ċ;ɫ Z;
string Ā="";public σ(ɫ V,ü đ,string œ){ɏ=-1;ɒ="ArgScroll";Ā=œ;ċ=đ;Z=V;}int Ŗ;ο υ;public override void ɯ(){υ=new ο(ƪ,Z.Ŋ);}int
ς=0;int ē=0;ˮ Ɠ;public override bool ɭ(bool ð){if(!ð){ē=0;υ.ŭ();Ɠ=new ˮ(ƪ);ς=0;}if(ē==0){if(!Ɠ.ʞ(Ā,ð))return false;if(Ɠ.ˏ
.Count>0){if(!int.TryParse(Ɠ.ˏ[0].œ,out Ŗ))Ŗ=1;else if(Ŗ<1)Ŗ=1;}if(Ɠ.ˬ.EndsWith("up"))Ŗ=-Ŗ;else if(!Ɠ.ˬ.EndsWith("down"))
Ŗ=0;ē++;ð=false;}if(ē==1){if(!υ.Ͼ("textpanel",Ɠ.ˤ,ð))return false;ē++;ð=false;}á ê;for(;ς<υ.Г();ς++){if(!ƪ.ʔ(20))return
false;IMyTextPanel π=υ.ρ[ς]as IMyTextPanel;if(!ċ.ö.TryGetValue(π,out ê))continue;if(ê==null||ê.Ý!=π)continue;if(ê.Ö)ê.Þ.ĵ=10;
if(Ŗ>0)ê.Þ.ĸ(Ŗ);else if(Ŗ<0)ê.Þ.ŀ(-Ŗ);else ê.Þ.Ĵ();ê.E();}return true;}}class ο{ș ƪ;В ξ;IMyCubeGrid ν{get{return ƪ.ǐ.Me.
CubeGrid;}}IMyGridTerminalSystem ǆ{get{return ƪ.ǐ.GridTerminalSystem;}}public List<IMyTerminalBlock>ρ=new List<IMyTerminalBlock>
();public ο(ș Ƣ,В ϗ){ƪ=Ƣ;ξ=ϗ;}int ϕ=0;public double ϔ(ref double ϓ,ref double ϒ,bool ð){if(!ð)ϕ=0;for(;ϕ<ρ.Count;ϕ++){if(
!ƪ.ʔ(4))return Double.NaN;IMyInventory ώ=ρ[ϕ].GetInventory(0);if(ώ==null)continue;ϓ+=(double)ώ.CurrentVolume;ϒ+=(double)ώ
.MaxVolume;}ϓ*=1000;ϒ*=1000;return(ϒ>0?ϓ/ϒ*100:100);}int ϑ=0;double ϐ=0;public double Ϗ(bool ð){if(!ð){ϑ=0;ϐ=0;}for(;ϑ<ρ.
Count;ϑ++){if(!ƪ.ʔ(6))return Double.NaN;for(int ϖ=0;ϖ<2;ϖ++){IMyInventory ώ=ρ[ϑ].GetInventory(ϖ);if(ώ==null)continue;ϐ+=(
double)ώ.CurrentMass;}}return ϐ*1000;}int ύ=0;private bool ό(bool ð=false){if(!ð)ύ=0;while(ύ<ρ.Count){if(!ƪ.ʔ(4))return false;
if(ρ[ύ].CubeGrid!=ν){ρ.RemoveAt(ύ);continue;}ύ++;}return true;}int ϋ=0;private bool ϊ(bool ð=false){if(!ð)ϋ=0;var ω=ƪ.ǐ.Me
;while(ϋ<ρ.Count){if(!ƪ.ʔ(4))return false;if(!ρ[ϋ].IsSameConstructAs(ω)){ρ.RemoveAt(ϋ);continue;}ϋ++;}return true;}List<
IMyBlockGroup>ψ=new List<IMyBlockGroup>();List<IMyTerminalBlock>χ=new List<IMyTerminalBlock>();int φ=0;public bool ϣ(string ˤ,bool ð)
{int Ͻ=ˤ.IndexOf(':');string Ђ=(Ͻ>=1&&Ͻ<=2?ˤ.Substring(0,Ͻ):"");bool ϼ=Ђ.Contains("T");bool Љ=Ђ.Contains("C");if(Ђ!="")ˤ=
ˤ.Substring(Ͻ+1);if(ˤ==""||ˤ=="*"){if(!ð){χ.Clear();ǆ.GetBlocks(χ);ρ.AddList(χ);}if(ϼ){if(!ό(ð))return false;}else if(Љ){
if(!ϊ(ð))return false;}return true;}string Ѓ=(Ђ.Contains("G")?ˤ.Trim():"");if(Ѓ!=""){if(!ð){ψ.Clear();ǆ.GetBlockGroups(ψ);
φ=0;}for(;φ<ψ.Count;φ++){IMyBlockGroup Ё=ψ[φ];if(string.Compare(Ё.Name,Ѓ,true)==0){if(!ð){χ.Clear();Ё.GetBlocks(χ);ρ.
AddList(χ);}if(ϼ){if(!ό(ð))return false;}else if(Љ){if(!ϊ(ð))return false;}return true;}}return true;}if(!ð){χ.Clear();ǆ.
SearchBlocksOfName(ˤ,χ);ρ.AddList(χ);}if(ϼ){if(!ό(ð))return false;}else if(Љ){if(!ϊ(ð))return false;}return true;}List<IMyBlockGroup>Ј=new
List<IMyBlockGroup>();List<IMyTerminalBlock>Ї=new List<IMyTerminalBlock>();int І=0;int Ѕ=0;public bool Є(string ʗ,string Ѓ,
bool ϼ,bool ð){if(!ð){Ј.Clear();ǆ.GetBlockGroups(Ј);І=0;}for(;І<Ј.Count;І++){IMyBlockGroup Ё=Ј[І];if(string.Compare(Ё.Name,Ѓ
,true)==0){if(!ð){Ѕ=0;Ї.Clear();Ё.GetBlocks(Ї);}else ð=false;for(;Ѕ<Ї.Count;Ѕ++){if(!ƪ.ʔ(6))return false;if(ϼ&&Ї[Ѕ].
CubeGrid!=ν)continue;if(ξ.ϯ(Ї[Ѕ],ʗ))ρ.Add(Ї[Ѕ]);}return true;}}return true;}List<IMyTerminalBlock>Ѐ=new List<IMyTerminalBlock>()
;int Ͽ=0;public bool Ͼ(string ʗ,string ˤ,bool ð){int Ͻ=ˤ.IndexOf(':');string Ђ=(Ͻ>=1&&Ͻ<=2?ˤ.Substring(0,Ͻ):"");bool ϼ=Ђ.
Contains("T");bool Љ=Ђ.Contains("C");if(Ђ!="")ˤ=ˤ.Substring(Ͻ+1);if(!ð){Ѐ.Clear();Ͽ=0;}string Ѓ=(Ђ.Contains("G")?ˤ.Trim():"");if
(Ѓ!=""){if(!Є(ʗ,Ѓ,ϼ,ð))return false;return true;}if(!ð)ξ.ϰ(ref Ѐ,ʗ);if(ˤ==""||ˤ=="*"){if(!ð)ρ.AddList(Ѐ);if(ϼ){if(!ό(ð))
return false;}else if(Љ){if(!ϊ(ð))return false;}return true;}for(;Ͽ<Ѐ.Count;Ͽ++){if(!ƪ.ʔ(4))return false;if(ϼ&&Ѐ[Ͽ].CubeGrid!=
ν)continue;if(Ѐ[Ͽ].CustomName.Contains(ˤ))ρ.Add(Ѐ[Ͽ]);}return true;}public void Е(ο Д){ρ.AddList(Д.ρ);}public void ŭ(){ρ.
Clear();}public int Г(){return ρ.Count;}}class В{ș ƪ;ɫ Z;public MyGridProgram ǐ{get{return ƪ.ǐ;}}public IMyGridTerminalSystem
ǆ{get{return ƪ.ǐ.GridTerminalSystem;}}public В(ș Ƣ,ɫ V){ƪ=Ƣ;Z=V;}void Б<ǲ>(List<IMyTerminalBlock>А,Func<IMyTerminalBlock,
bool>Џ=null)where ǲ:class,IMyTerminalBlock{ǆ.GetBlocksOfType<ǲ>(А,Џ);}public Dictionary<string,Action<List<IMyTerminalBlock>
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
ref List<IMyTerminalBlock>ł,string Ћ){Action<List<IMyTerminalBlock>,Func<IMyTerminalBlock,bool>>Њ;if(Ћ=="SurfaceProvider"){
ǆ.GetBlocksOfType<IMyTextSurfaceProvider>(ł);return;}if(Ў.TryGetValue(Ћ,out Њ))Њ(ł,null);else{if(Ћ=="WindTurbine"){ǆ.
GetBlocksOfType<IMyPowerProducer>(ł,(Ϥ)=>Ϥ.BlockDefinition.TypeIdString.EndsWith("WindTurbine"));return;}if(Ћ=="HydrogenEngine"){ǆ.
GetBlocksOfType<IMyPowerProducer>(ł,(Ϥ)=>Ϥ.BlockDefinition.TypeIdString.EndsWith("HydrogenEngine"));return;}if(Ћ=="StoreBlock"){ǆ.
GetBlocksOfType<IMyFunctionalBlock>(ł,(Ϥ)=>Ϥ.BlockDefinition.TypeIdString.EndsWith("StoreBlock"));return;}if(Ћ=="ContractBlock"){ǆ.
GetBlocksOfType<IMyFunctionalBlock>(ł,(Ϥ)=>Ϥ.BlockDefinition.TypeIdString.EndsWith("ContractBlock"));return;}if(Ћ=="VendingMachine"){ǆ.
GetBlocksOfType<IMyFunctionalBlock>(ł,(Ϥ)=>Ϥ.BlockDefinition.TypeIdString.EndsWith("VendingMachine"));return;}}}public void ϰ(ref List<
IMyTerminalBlock>ł,string Ϯ){Ќ(ref ł,Ϭ(Ϯ.Trim()));}public bool ϯ(IMyTerminalBlock Ý,string Ϯ){string ϭ=Ϭ(Ϯ);switch(ϭ){case
"FunctionalBlock":return true;case"ShipController":return(Ý as IMyShipController!=null);default:return Ý.BlockDefinition.TypeIdString.
Contains(Ϭ(Ϯ));}}public string Ϭ(string ϫ){if(ϫ=="surfaceprovider")return"SurfaceProvider";if(ϫ.Ƿ("carg")||ϫ.Ƿ("conta"))return
"CargoContainer";if(ϫ.Ƿ("text")||ϫ.Ƿ("lcd"))return"TextPanel";if(ϫ.Ƿ("ass"))return"Assembler";if(ϫ.Ƿ("refi"))return"Refinery";if(ϫ.Ƿ(
"reac"))return"Reactor";if(ϫ.Ƿ("solar"))return"SolarPanel";if(ϫ.Ƿ("wind"))return"WindTurbine";if(ϫ.Ƿ("hydro")&&ϫ.Contains(
"eng"))return"HydrogenEngine";if(ϫ.Ƿ("bat"))return"BatteryBlock";if(ϫ.Ƿ("bea"))return"Beacon";if(ϫ.Ƕ("vent"))return"AirVent";
if(ϫ.Ƕ("sorter"))return"ConveyorSorter";if(ϫ.Ƕ("tank"))return"OxygenTank";if(ϫ.Ƕ("farm")&&ϫ.Ƕ("oxy"))return"OxygenFarm";if
(ϫ.Ƕ("gene")&&ϫ.Ƕ("oxy"))return"OxygenGenerator";if(ϫ.Ƕ("cryo"))return"CryoChamber";if(string.Compare(ϫ,"laserantenna",
true)==0)return"LaserAntenna";if(ϫ.Ƕ("antenna"))return"RadioAntenna";if(ϫ.Ƿ("thrust"))return"Thrust";if(ϫ.Ƿ("gyro"))return
"Gyro";if(ϫ.Ƿ("sensor"))return"SensorBlock";if(ϫ.Ƕ("connector"))return"ShipConnector";if(ϫ.Ƿ("reflector")||ϫ.Ƿ("spotlight"))
return"ReflectorLight";if((ϫ.Ƿ("inter")&&ϫ.Ǵ("light")))return"InteriorLight";if(ϫ.Ƿ("land"))return"LandingGear";if(ϫ.Ƿ(
"program"))return"ProgrammableBlock";if(ϫ.Ƿ("timer"))return"TimerBlock";if(ϫ.Ƿ("motor")||ϫ.Ƿ("rotor"))return"MotorStator";if(ϫ.Ƿ(
"piston"))return"PistonBase";if(ϫ.Ƿ("proj"))return"Projector";if(ϫ.Ƕ("merge"))return"ShipMergeBlock";if(ϫ.Ƿ("sound"))return
"SoundBlock";if(ϫ.Ƿ("col"))return"Collector";if(ϫ.Ƕ("jump"))return"JumpDrive";if(string.Compare(ϫ,"door",true)==0)return"Door";if((ϫ
.Ƕ("grav")&&ϫ.Ƕ("sphe")))return"GravityGeneratorSphere";if(ϫ.Ƕ("grav"))return"GravityGenerator";if(ϫ.Ǵ("drill"))return
"ShipDrill";if(ϫ.Ƕ("grind"))return"ShipGrinder";if(ϫ.Ǵ("welder"))return"ShipWelder";if(ϫ.Ƿ("parach"))return"Parachute";if((ϫ.Ƕ(
"turret")&&ϫ.Ƕ("gatl")))return"LargeGatlingTurret";if((ϫ.Ƕ("turret")&&ϫ.Ƕ("inter")))return"LargeInteriorTurret";if((ϫ.Ƕ("turret"
)&&ϫ.Ƕ("miss")))return"LargeMissileTurret";if(ϫ.Ƕ("gatl"))return"SmallGatlingGun";if((ϫ.Ƕ("launcher")&&ϫ.Ƕ("reload")))
return"SmallMissileLauncherReload";if((ϫ.Ƕ("launcher")))return"SmallMissileLauncher";if(ϫ.Ƕ("mass"))return"VirtualMass";if(
string.Compare(ϫ,"warhead",true)==0)return"Warhead";if(ϫ.Ƿ("func"))return"FunctionalBlock";if(string.Compare(ϫ,"shipctrl",true
)==0)return"ShipController";if(ϫ.Ƿ("light"))return"LightingBlock";if(ϫ.Ƿ("contr"))return"ControlPanel";if(ϫ.Ƿ("coc"))
return"Cockpit";if(ϫ.Ƿ("medi"))return"MedicalRoom";if(ϫ.Ƿ("remote"))return"RemoteControl";if(ϫ.Ƿ("but"))return"ButtonPanel";if
(ϫ.Ƿ("cam"))return"CameraBlock";if(ϫ.Ƕ("detect"))return"OreDetector";if(ϫ.Ƿ("safe"))return"SafeZoneBlock";if(ϫ.Ƿ("store")
)return"StoreBlock";if(ϫ.Ƿ("contract"))return"ContractBlock";if(ϫ.Ƿ("vending"))return"VendingMachine";if(ϫ.Ƿ("decoy"))
return"Decoy";return"Unknown";}public string Ϫ(IMyBatteryBlock ņ){string ϩ="";if(ņ.ChargeMode==ChargeMode.Recharge)ϩ="(+) ";
else if(ņ.ChargeMode==ChargeMode.Discharge)ϩ="(-) ";else ϩ="(±) ";return ϩ+Z.Ȇ((ņ.CurrentStoredPower/ņ.MaxStoredPower)*
100.0f)+"%";}Dictionary<MyLaserAntennaStatus,string>Ϩ=new Dictionary<MyLaserAntennaStatus,string>(){{MyLaserAntennaStatus.Idle
,"IDLE"},{MyLaserAntennaStatus.Connecting,"CONNECTING"},{MyLaserAntennaStatus.Connected,"CONNECTED"},{
MyLaserAntennaStatus.OutOfRange,"OUT OF RANGE"},{MyLaserAntennaStatus.RotatingToTarget,"ROTATING"},{MyLaserAntennaStatus.
SearchingTargetForAntenna,"SEARCHING"}};public string ϧ(IMyLaserAntenna ń){return Ϩ[ń.Status];}public double Ϧ(IMyJumpDrive Ņ,out double ʝ,out
double ƌ){ʝ=Ņ.CurrentStoredPower;ƌ=Ņ.MaxStoredPower;return(ƌ>0?ʝ/ƌ*100:0);}public double ϥ(IMyJumpDrive Ņ){double ʝ=Ņ.
CurrentStoredPower;double ƌ=Ņ.MaxStoredPower;return(ƌ>0?ʝ/ƌ*100:0);}}class ϱ:ɓ{ɫ Z;ü ċ;public int ϻ=0;public ϱ(ɫ V,ü Ĝ){ɒ="BootPanelsTask"
;ɏ=1;Z=V;ċ=Ĝ;if(!Z.ɤ){ϻ=int.MaxValue;ċ.õ=true;}}Ǫ ĝ;public override void ɯ(){ĝ=Z.ĝ;}public override bool ɭ(bool ð){if(ϻ>Z
.ɢ.Count){ɣ();return true;}if(!ð&&ϻ==0){ċ.õ=false;}if(!ϸ(ð))return false;ϻ++;return true;}public override void ɬ(){ċ.õ=
true;}public void Ϻ(){ȹ à=ċ.à;for(int Y=0;Y<à.n();Y++){á ê=à.f(Y);ê.Á();}ϻ=(Z.ɤ?0:int.MaxValue);}int Y;Ş Ϲ=null;public bool
ϸ(bool ð){ȹ à=ċ.à;if(!ð)Y=0;int Ϸ=0;for(;Y<à.n();Y++){if(!ƪ.ʔ(40)||Ϸ>5)return false;á ê=à.f(Y);Ϲ=Z.ǖ(Ϲ,ê);float?ϵ=ê.Õ?.
FontSize;if(ϵ!=null&&ϵ>3f)continue;if(Ϲ.Ų.Count<=0)Ϲ.Ů(Z.ǘ(null,ê));else Z.ǘ(Ϲ.Ų[0],ê);Z.ŝ();Z.Ƶ(ĝ.ǲ("B1"));double ʙ=(double)ϻ/Z
.ɢ.Count*100;Z.Ʒ(ʙ);if(ϻ==Z.ɢ.Count){Z.Ǖ("");Z.Ƶ("Version 2.0184");Z.Ƶ("by MMaster");Z.Ƶ("");Z.Ƶ("übersetzt von Ich_73");}else Z.ǔ(Z.ɢ[ϻ]);bool Ö=ê.Ö;ê.Ö=
false;Z.ȁ(ê,Ϲ);ê.Ö=Ö;Ϸ++;}return true;}public bool ϴ(){return ϻ<=Z.ɢ.Count;}}public enum ϳ{ϲ=0,μ=1,ˈ=2,κ=3,ˆ=4,ˁ=5,ˀ=6,ʿ=7,ʾ=
8,ʽ=9,ʼ=10,ʻ=11,ʺ=12,ʹ=13,ˇ=14,ʸ=15,ʶ=16,ʵ=17,ʴ=18,ʳ=19,ʲ=20,ʱ=21,ʰ=22,ʯ=23,ʮ=24,ʭ=25,ʷ=26,ˉ=27,ˑ=28,Ͳ=29,ͱ=30,Ͱ=31,}
class ˮ{ș ƪ;public string ˬ="";public string ˤ="";public string ˣ="";public string ˢ="";public ϳ ˡ=ϳ.ϲ;public ˮ(ș Ƣ){ƪ=Ƣ;}ϳ ˠ
(){if(ˬ=="echo"||ˬ=="center"||ˬ=="right")return ϳ.μ;if(ˬ.StartsWith("hscroll"))return ϳ.ͱ;if(ˬ.StartsWith("inventory")||ˬ
.StartsWith("missing")||ˬ.StartsWith("invlist"))return ϳ.ˈ;if(ˬ.StartsWith("working"))return ϳ.ʴ;if(ˬ.StartsWith("cargo")
)return ϳ.κ;if(ˬ.StartsWith("mass"))return ϳ.ˆ;if(ˬ.StartsWith("shipmass"))return ϳ.ʯ;if(ˬ=="oxygen")return ϳ.ˁ;if(ˬ.
StartsWith("tanks"))return ϳ.ˀ;if(ˬ.StartsWith("powertime"))return ϳ.ʿ;if(ˬ.StartsWith("powerused"))return ϳ.ʾ;if(ˬ.StartsWith(
"power"))return ϳ.ʽ;if(ˬ.StartsWith("speed"))return ϳ.ʼ;if(ˬ.StartsWith("accel"))return ϳ.ʻ;if(ˬ.StartsWith("alti"))return ϳ.ʭ;
if(ˬ.StartsWith("charge"))return ϳ.ʺ;if(ˬ.StartsWith("docked"))return ϳ.Ͱ;if(ˬ.StartsWith("time")||ˬ.StartsWith("date"))
return ϳ.ʹ;if(ˬ.StartsWith("countdown"))return ϳ.ˇ;if(ˬ.StartsWith("textlcd"))return ϳ.ʸ;if(ˬ.EndsWith("count"))return ϳ.ʶ;if(
ˬ.StartsWith("dampeners")||ˬ.StartsWith("occupied"))return ϳ.ʵ;if(ˬ.StartsWith("damage"))return ϳ.ʳ;if(ˬ.StartsWith(
"amount"))return ϳ.ʲ;if(ˬ.StartsWith("pos"))return ϳ.ʱ;if(ˬ.StartsWith("distance"))return ϳ.ʮ;if(ˬ.StartsWith("details"))return
ϳ.ʰ;if(ˬ.StartsWith("stop"))return ϳ.ʷ;if(ˬ.StartsWith("gravity"))return ϳ.ˉ;if(ˬ.StartsWith("customdata"))return ϳ.ˑ;if(
ˬ.StartsWith("prop"))return ϳ.Ͳ;return ϳ.ϲ;}public ƛ ː(){switch(ˡ){case ϳ.μ:return new ҳ();case ϳ.ˈ:return new Ѿ();case ϳ
.κ:return new ͳ();case ϳ.ˆ:return new ҹ();case ϳ.ˁ:return new Ҹ();case ϳ.ˀ:return new ѩ();case ϳ.ʿ:return new ы();case ϳ.
ʾ:return new Ь();case ϳ.ʽ:return new ӂ();case ϳ.ʼ:return new њ();case ϳ.ʻ:return new ʡ();case ϳ.ʺ:return new Ϋ();case ϳ.ʹ
:return new Θ();case ϳ.ˇ:return new Ώ();case ϳ.ʸ:return new ĳ();case ϳ.ʶ:return new ʖ();case ϳ.ʵ:return new Ѡ();case ϳ.ʴ:
return new Ĺ();case ϳ.ʳ:return new ͷ();case ϳ.ʲ:return new ӗ();case ϳ.ʱ:return new ӄ();case ϳ.ʰ:return new Δ();case ϳ.ʯ:return
new ў();case ϳ.ʮ:return new Ҧ();case ϳ.ʭ:return new ʘ();case ϳ.ʷ:return new ї();case ϳ.ˉ:return new Ҳ();case ϳ.ˑ:return new
ͼ();case ϳ.Ͳ:return new ѻ();case ϳ.ͱ:return new ұ();case ϳ.Ͱ:return new ҭ();default:return new ƛ();}}public List<ʬ>ˏ=new
List<ʬ>();string[]ˎ=null;string ˍ="";bool ˌ=false;int ŕ=1;public bool ʞ(string ˋ,bool ð){if(!ð){ˡ=ϳ.ϲ;ˤ="";ˬ="";ˣ=ˋ.
TrimStart(' ');ˏ.Clear();if(ˣ=="")return true;int ˊ=ˣ.IndexOf(' ');if(ˊ<0||ˊ>=ˣ.Length-1)ˢ="";else ˢ=ˣ.Substring(ˊ+1);ˎ=ˣ.Split(
' ');ˍ="";ˌ=false;ˬ=ˎ[0].ToLower();ŕ=1;}for(;ŕ<ˎ.Length;ŕ++){if(!ƪ.ʔ(40))return false;string œ=ˎ[ŕ];if(œ=="")continue;if(œ[
0]=='{'&&œ[œ.Length-1]=='}'){œ=œ.Substring(1,œ.Length-2);if(œ=="")continue;if(ˤ=="")ˤ=œ;else ˏ.Add(new ʬ(œ));continue;}if
(œ[0]=='{'){ˌ=true;ˍ=œ.Substring(1);continue;}if(œ[œ.Length-1]=='}'){ˌ=false;ˍ+=' '+œ.Substring(0,œ.Length-1);if(ˤ=="")ˤ=
ˍ;else ˏ.Add(new ʬ(ˍ));continue;}if(ˌ){if(ˍ.Length!=0)ˍ+=' ';ˍ+=œ;continue;}if(ˤ=="")ˤ=œ;else ˏ.Add(new ʬ(œ));}ˡ=ˠ();
return true;}}class ʬ{public string ʫ="";public string ʕ="";public string œ="";public List<string>ʠ=new List<string>();public
ʬ(string ʟ){œ=ʟ;}public void ʞ(){if(œ==""||ʫ!=""||ʕ!=""||ʠ.Count>0)return;string ʝ=œ.Trim();if(ʝ[0]=='+'||ʝ[0]=='-'){ʫ+=ʝ
[0];ʝ=œ.Substring(1);}string[]Ɵ=ʝ.Split('/');string ʜ=Ɵ[0];if(Ɵ.Length>1){ʕ=Ɵ[0];ʜ=Ɵ[1];}else ʕ="";if(ʜ.Length>0){string[
]Ă=ʜ.Split(',');for(int Y=0;Y<Ă.Length;Y++)if(Ă[Y]!="")ʠ.Add(Ă[Y]);}}}class ʡ:ƛ{public ʡ(){ɏ=0.5;ɒ="CmdAccel";}public
override bool Ɛ(bool ð){double ʚ=0;if(Ɠ.ˤ!="")double.TryParse(Ɠ.ˤ.Trim(),out ʚ);Z.Ì(ĝ.ǲ("AC1")+" ");Z.ƴ(Z.Ǉ.ɿ.ToString("F1")+
" m/s²");if(ʚ>0){double ʙ=Z.Ǉ.ɿ/ʚ*100;Z.Ʒ(ʙ);}return true;}}class ʘ:ƛ{public ʘ(){ɏ=1;ɒ="CmdAltitude";}public override bool Ɛ(
bool ð){string ʗ=(Ɠ.ˬ.EndsWith("sea")?"sea":"ground");switch(ʗ){case"sea":Z.Ì(ĝ.ǲ("ALT1"));Z.ƴ(Z.Ǉ.ɵ.ToString("F0")+" m");
break;default:Z.Ì(ĝ.ǲ("ALT2"));Z.ƴ(Z.Ǉ.ɳ.ToString("F0")+" m");break;}return true;}}class ʖ:ƛ{public ʖ(){ɏ=15;ɒ=
"CmdBlockCount";}ο Ō;public override void ɯ(){Ō=new ο(ƪ,Z.Ŋ);}bool ʛ;bool ʢ;int ŕ=0;int ē=0;public override bool Ɛ(bool ð){if(!ð){ʛ=(Ɠ.
ˬ=="enabledcount");ʢ=(Ɠ.ˬ=="prodcount");ŕ=0;ē=0;}if(Ɠ.ˏ.Count==0){if(ē==0){if(!ð)Ō.ŭ();if(!Ō.ϣ(Ɠ.ˤ,ð))return false;ē++;ð=
false;}if(!ʤ(Ō,"blocks",ʛ,ʢ,ð))return false;return true;}for(;ŕ<Ɠ.ˏ.Count;ŕ++){ʬ œ=Ɠ.ˏ[ŕ];if(!ð)œ.ʞ();if(!ō(œ,ð))return false
;ð=false;}return true;}int ő=0;int Œ=0;bool ō(ʬ œ,bool ð){if(!ð){ő=0;Œ=0;}for(;ő<œ.ʠ.Count;ő++){if(Œ==0){if(!ð)Ō.ŭ();if(!
Ō.Ͼ(œ.ʠ[ő],Ɠ.ˤ,ð))return false;Œ++;ð=false;}if(!ʤ(Ō,œ.ʠ[ő],ʛ,ʢ,ð))return false;Œ=0;ð=false;}return true;}Dictionary<
string,int>ʪ=new Dictionary<string,int>();Dictionary<string,int>ʩ=new Dictionary<string,int>();List<string>ʨ=new List<string>(
);int ą=0;int ʧ=0;int ʦ=0;ʏ ʥ=new ʏ();bool ʤ(ο ł,string ʗ,bool ʛ,bool ʢ,bool ð){if(ł.Г()==0){ʥ.ŭ().ɰ(char.ToUpper(ʗ[0])).
ɰ(ʗ.ToLower(),1,ʗ.Length-1);Z.Ì(ʥ.ɰ(" ").ɰ(ĝ.ǲ("C1")).ɰ(" "));string ʣ=(ʛ||ʢ?"0 / 0":"0");Z.ƴ(ʣ);return true;}if(!ð){ʪ.
Clear();ʩ.Clear();ʨ.Clear();ą=0;ʧ=0;ʦ=0;}if(ʦ==0){for(;ą<ł.Г();ą++){if(!ƪ.ʔ(15))return false;IMyProductionBlock ŉ=ł.ρ[ą]as
IMyProductionBlock;ʥ.ŭ().ɰ(ł.ρ[ą].DefinitionDisplayNameText);string ǿ=ʥ.ɕ();if(ʨ.Contains(ǿ)){ʪ[ǿ]++;if((ʛ&&ł.ρ[ą].IsWorking)||(ʢ&&ŉ!=null
&&ŉ.IsProducing))ʩ[ǿ]++;}else{ʪ.Add(ǿ,1);ʨ.Add(ǿ);if(ʛ||ʢ)if((ʛ&&ł.ρ[ą].IsWorking)||(ʢ&&ŉ!=null&&ŉ.IsProducing))ʩ.Add(ǿ,1)
;else ʩ.Add(ǿ,0);}}ʦ++;ð=false;}for(;ʧ<ʪ.Count;ʧ++){if(!ƪ.ʔ(8))return false;Z.Ì(ʨ[ʧ]+" "+ĝ.ǲ("C1")+" ");string ʣ=(ʛ||ʢ?ʩ[
ʨ[ʧ]]+" / ":"")+ʪ[ʨ[ʧ]];Z.ƴ(ʣ);}return true;}}class ͳ:ƛ{ο Ō;public ͳ(){ɏ=2;ɒ="CmdCargo";}public override void ɯ(){Ō=new ο
(ƪ,Z.Ŋ);}bool Ϊ=true;bool Ͷ=false;bool Ω=false;bool Σ=false;double Ψ=0;double Χ=0;int ē=0;public override bool Ɛ(bool ð){
if(!ð){Ō.ŭ();Ϊ=Ɠ.ˬ.Contains("all");Σ=Ɠ.ˬ.EndsWith("bar");Ͷ=(Ɠ.ˬ[Ɠ.ˬ.Length-1]=='x');Ω=(Ɠ.ˬ[Ɠ.ˬ.Length-1]=='p');Ψ=0;Χ=0;ē=0
;}if(ē==0){if(Ϊ){if(!Ō.ϣ(Ɠ.ˤ,ð))return false;}else{if(!Ō.Ͼ("cargocontainer",Ɠ.ˤ,ð))return false;}ē++;ð=false;}double Φ=Ō.
ϔ(ref Ψ,ref Χ,ð);if(Double.IsNaN(Φ))return false;if(Σ){Z.Ʒ(Φ);return true;}Z.Ì(ĝ.ǲ("C2")+" ");if(!Ͷ&&!Ω){Z.ƴ(Z.ȏ(Ψ)+
"L / "+Z.ȏ(Χ)+"L");Z.ƾ(Φ,1.0f,Z.ƶ);Z.Ǖ(' '+Z.Ȇ(Φ)+"%");}else if(Ω){Z.ƴ(Z.Ȇ(Φ)+"%");Z.Ʒ(Φ);}else Z.ƴ(Z.Ȇ(Φ)+"%");return true;}}
class Ϋ:ƛ{public Ϋ(){ɏ=3;ɒ="CmdCharge";}ο Ō;bool Ͷ=false;bool Τ=false;bool Σ=false;bool Ρ=false;public override void ɯ(){Ō=
new ο(ƪ,Z.Ŋ);if(Ɠ.ˏ.Count>0)Υ=Ɠ.ˏ[0].œ;Σ=Ɠ.ˬ.EndsWith("bar");Ͷ=Ɠ.ˬ.Contains("x");Τ=Ɠ.ˬ.Contains("time");Ρ=Ɠ.ˬ.Contains(
"sum");}int ē=0;int ą=0;double Π=0;double Ο=0;TimeSpan Ξ=TimeSpan.Zero;string Υ="";Dictionary<long,double>ę=new Dictionary<
long,double>();Dictionary<long,double>ά=new Dictionary<long,double>();Dictionary<long,double>λ=new Dictionary<long,double>()
;Dictionary<long,double>ι=new Dictionary<long,double>();Dictionary<long,double>θ=new Dictionary<long,double>();double η(
long ζ,double ʝ,double ƌ){double ε=0;double δ=0;double γ=0;double β=0;if(ά.TryGetValue(ζ,out γ)){β=ι[ζ];}if(ę.TryGetValue(ζ,
out ε)){δ=λ[ζ];}double α=(ƪ.ȕ-γ);double ΰ=0;if(α>0)ΰ=(ʝ-β)/α;if(ΰ<0){if(!θ.TryGetValue(ζ,out ΰ))ΰ=0;}else θ[ζ]=ΰ;if(ε>0){ά[
ζ]=ę[ζ];ι[ζ]=λ[ζ];}ę[ζ]=ƪ.ȕ;λ[ζ]=ʝ;return(ΰ>0?(ƌ-ʝ)/ΰ:0);}private void ί(string ǿ,double ʙ,double ʝ,double ƌ,TimeSpan ή){
if(Σ){Z.Ʒ(ʙ);}else{Z.Ì(ǿ+" ");if(Τ){Z.ƴ(Z.ǈ.Ȣ(ή));if(!Ͷ){Z.ƾ(ʙ,1.0f,Z.ƶ);Z.ƴ(' '+ʙ.ToString("0.0")+"%");}}else{if(!Ͷ){Z.ƴ(
Z.ȏ(ʝ)+"Wh / "+Z.ȏ(ƌ)+"Wh");Z.ƾ(ʙ,1.0f,Z.ƶ);}Z.ƴ(' '+ʙ.ToString("0.0")+"%");}}}public override bool Ɛ(bool ð){if(!ð){Ō.ŭ(
);ą=0;ē=0;Π=0;Ο=0;Ξ=TimeSpan.Zero;}if(ē==0){if(!Ō.Ͼ("jumpdrive",Ɠ.ˤ,ð))return false;if(Ō.Г()<=0){Z.Ǖ("Charge: "+ĝ.ǲ("D2")
);return true;}ē++;ð=false;}for(;ą<Ō.Г();ą++){if(!ƪ.ʔ(25))return false;IMyJumpDrive Ņ=Ō.ρ[ą]as IMyJumpDrive;double ʝ,ƌ,ʙ;
ʙ=Z.Ŋ.Ϧ(Ņ,out ʝ,out ƌ);TimeSpan έ;if(Τ)έ=TimeSpan.FromSeconds(η(Ņ.EntityId,ʝ,ƌ));else έ=TimeSpan.Zero;if(!Ρ){ί(Ņ.
CustomName,ʙ,ʝ,ƌ,έ);}else{Π+=ʝ;Ο+=ƌ;if(Ξ<έ)Ξ=έ;}}if(Ρ){double Ν=(Ο>0?Π/Ο*100:0);ί(Υ,Ν,Π,Ο,Ξ);}return true;}}class Ώ:ƛ{public Ώ(){ɏ
=1;ɒ="CmdCountdown";}public override bool Ɛ(bool ð){bool Ύ=Ɠ.ˬ.EndsWith("c");bool Ό=Ɠ.ˬ.EndsWith("r");string Ί="";int Ή=Ɠ
.ˣ.IndexOf(' ');if(Ή>=0)Ί=Ɠ.ˣ.Substring(Ή+1).Trim();DateTime Έ=DateTime.Now;DateTime Ά;if(!DateTime.TryParseExact(Ί,
"H:mm d.M.yyyy",System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None,out Ά)){Z.Ǖ(ĝ.ǲ("C3"));Z.Ǖ(
"  Countdown 19:02 28.2.2015");return true;}TimeSpan ͽ=Ά-Έ;string Ĳ="";if(ͽ.Ticks<=0)Ĳ=ĝ.ǲ("C4");else{if((int)ͽ.TotalDays>0)Ĳ+=(int)ͽ.TotalDays+" "+ĝ
.ǲ("C5")+" ";if(ͽ.Hours>0||Ĳ!="")Ĳ+=ͽ.Hours+"h ";if(ͽ.Minutes>0||Ĳ!="")Ĳ+=ͽ.Minutes+"m ";Ĳ+=ͽ.Seconds+"s";}if(Ύ)Z.Ƶ(Ĳ);
else if(Ό)Z.ƴ(Ĳ);else Z.Ǖ(Ĳ);return true;}}class ͼ:ƛ{public ͼ(){ɏ=1;ɒ="CmdCustomData";}public override bool Ɛ(bool ð){string
Ĳ="";if(Ɠ.ˤ!=""&&Ɠ.ˤ!="*"){IMyTerminalBlock ͺ=Z.ǆ.GetBlockWithName(Ɠ.ˤ)as IMyTerminalBlock;if(ͺ==null){Z.Ǖ("CustomData: "
+ĝ.ǲ("CD1")+Ɠ.ˤ);return true;}Ĳ=ͺ.CustomData;}else{Z.Ǖ("CustomData:"+ĝ.ǲ("CD2"));return true;}if(Ĳ.Length==0)return true;
Z.ǔ(Ĳ);return true;}}class ͷ:ƛ{public ͷ(){ɏ=5;ɒ="CmdDamage";}ο Ō;public override void ɯ(){Ō=new ο(ƪ,Z.Ŋ);}bool Ƃ=false;
int ą=0;public override bool Ɛ(bool ð){bool Ͷ=Ɠ.ˬ.StartsWith("damagex");bool ʹ=Ɠ.ˬ.EndsWith("noc");bool ͻ=(!ʹ&&Ɠ.ˬ.EndsWith
("c"));float ΐ=100;if(!ð){Ō.ŭ();Ƃ=false;ą=0;}if(!Ō.ϣ(Ɠ.ˤ,ð))return false;if(Ɠ.ˏ.Count>0){if(!float.TryParse(Ɠ.ˏ[0].œ,out
ΐ))ΐ=100;}ΐ-=0.00001f;for(;ą<Ō.Г();ą++){if(!ƪ.ʔ(30))return false;IMyTerminalBlock Ý=Ō.ρ[ą];IMySlimBlock Μ=Ý.CubeGrid.
GetCubeBlock(Ý.Position);if(Μ==null)continue;float Κ=(ʹ?Μ.MaxIntegrity:Μ.BuildIntegrity);if(!ͻ)Κ-=Μ.CurrentDamage;float ʙ=100*(Κ/Μ.
MaxIntegrity);if(ʙ>=ΐ)continue;Ƃ=true;string Ι=Z.ǥ(Μ.FatBlock.DisplayNameText,Z.ɝ*0.69f-Z.ƶ);Z.Ì(Ι+' ');if(!Ͷ){Z.Ʊ(Z.ȏ(Κ)+" / ",
0.69f);Z.Ì(Z.ȏ(Μ.MaxIntegrity));}Z.ƴ(' '+ʙ.ToString("0.0")+'%');Z.Ʒ(ʙ);}if(!Ƃ)Z.Ǖ(ĝ.ǲ("D3"));return true;}}class Θ:ƛ{public Θ
(){ɏ=1;ɒ="CmdDateTime";}public override bool Ɛ(bool ð){bool Η=(Ɠ.ˬ.StartsWith("datetime"));bool Ζ=(Ɠ.ˬ.StartsWith("date")
);bool Ύ=Ɠ.ˬ.Contains("c");int Ε=Ɠ.ˬ.IndexOf('+');if(Ε<0)Ε=Ɠ.ˬ.IndexOf('-');float Λ=0;if(Ε>=0)float.TryParse(Ɠ.ˬ.
Substring(Ε),out Λ);DateTime ͽ=DateTime.Now.AddHours(Λ);string Ĳ="";int Ή=Ɠ.ˣ.IndexOf(' ');if(Ή>=0)Ĳ=Ɠ.ˣ.Substring(Ή+1);if(!Η){if
(!Ζ)Ĳ+=ͽ.ToShortTimeString();else Ĳ+=ͽ.ToShortDateString();}else{if(Ĳ=="")Ĳ=String.Format("{0:d} {0:t}",ͽ);else{Ĳ=Ĳ.
Replace("/","\\/");Ĳ=Ĳ.Replace(":","\\:");Ĳ=Ĳ.Replace("\"","\\\"");Ĳ=Ĳ.Replace("'","\\'");Ĳ=ͽ.ToString(Ĳ+' ');Ĳ=Ĳ.Substring(0,Ĳ
.Length-1);}}if(Ύ)Z.Ƶ(Ĳ);else Z.Ǖ(Ĳ);return true;}}class Δ:ƛ{public Δ(){ɏ=5;ɒ="CmdDetails";}string Γ="";string Β="";int Ř
=0;ο Ō;public override void ɯ(){Ō=new ο(ƪ,Z.Ŋ);if(Ɠ.ˏ.Count>0)Γ=Ɠ.ˏ[0].œ.Trim();if(Ɠ.ˏ.Count>1){string œ=Ɠ.ˏ[1].œ.Trim();
if(!int.TryParse(œ,out Ř)){Ř=0;Β=œ;}}}int ē=0;int ą=1;bool Α=false;IMyTerminalBlock Ý;public override bool Ɛ(bool ð){if(Ɠ.
ˤ==""||Ɠ.ˤ=="*"){Z.Ǖ("Details: "+ĝ.ǲ("D1"));return true;}if(!ð){Ō.ŭ();Α=Ɠ.ˬ.Contains("non");ē=0;ą=1;}if(ē==0){if(!Ō.ϣ(Ɠ.ˤ
,ð))return true;if(Ō.Г()<=0){Z.Ǖ("Details: "+ĝ.ǲ("D2"));return true;}ē++;ð=false;}int Ҭ=(Ɠ.ˬ.EndsWith("x")?1:0);if(ē==1){
if(!ð){Ý=Ō.ρ[0];if(!Α)Z.Ǖ(Ý.CustomName);}if(!Ҩ(Ý,Ҭ,Ř,ð))return false;ē++;ð=false;}for(;ą<Ō.Г();ą++){if(!ð){Ý=Ō.ρ[ą];if(!Α)
{Z.Ǖ("");Z.Ǖ(Ý.CustomName);}}if(!Ҩ(Ý,Ҭ,Ř,ð))return false;ð=false;}return true;}string[]Ŧ;int ҫ=0;int Ҫ=0;bool ҩ=false;ʏ Ƴ
=new ʏ();bool Ҩ(IMyTerminalBlock Ý,int ҧ,int ķ,bool ð){if(!ð){Ŧ=Ƴ.ŭ().ɰ(Ý.DetailedInfo).ɰ('\n').ɰ(Ý.CustomInfo).ɕ().Split
('\n');ҫ=ҧ;ҩ=(Γ.Length==0);Ҫ=0;}for(;ҫ<Ŧ.Length;ҫ++){if(!ƪ.ʔ(5))return false;if(Ŧ[ҫ].Length==0)continue;if(!ҩ){if(!Ŧ[ҫ].
Contains(Γ))continue;ҩ=true;}if(Β.Length>0&&Ŧ[ҫ].Contains(Β))return true;Z.Ǖ(Ƴ.ŭ().ɰ("  ").ɰ(Ŧ[ҫ]));Ҫ++;if(ķ>0&&Ҫ>=ķ)return true
;}return true;}}class Ҧ:ƛ{public Ҧ(){ɏ=1;ɒ="CmdDistance";}string ҥ="";string[]Ҥ;Vector3D ң;string Ң="";bool ҡ=false;
public override void ɯ(){ҡ=false;if(Ɠ.ˏ.Count<=0)return;ҥ=Ɠ.ˏ[0].œ.Trim();Ҥ=ҥ.Split(':');if(Ҥ.Length<5||Ҥ[0]!="GPS")return;
double Ҡ,ҟ,Ҟ;if(!double.TryParse(Ҥ[2],out Ҡ))return;if(!double.TryParse(Ҥ[3],out ҟ))return;if(!double.TryParse(Ҥ[4],out Ҟ))
return;ң=new Vector3D(Ҡ,ҟ,Ҟ);Ң=Ҥ[1];ҡ=true;}public override bool Ɛ(bool ð){if(!ҡ){Z.Ǖ("Distance: "+ĝ.ǲ("DTU")+" '"+ҥ+"'.");
return true;}IMyTerminalBlock Ý=Ĝ.v.Ý;if(Ɠ.ˤ!=""&&Ɠ.ˤ!="*"){Ý=Z.ǆ.GetBlockWithName(Ɠ.ˤ);if(Ý==null){Z.Ǖ("Distance: "+ĝ.ǲ("P1")
+": "+Ɠ.ˤ);return true;}}double ѥ=Vector3D.Distance(Ý.GetPosition(),ң);Z.Ì(Ң+": ");Z.ƴ(Z.ȏ(ѥ)+"m ");return true;}}class ҭ
:ƛ{ο Ō;public ҭ(){ɏ=2;ɒ="CmdDocked";}public override void ɯ(){Ō=new ο(ƪ,Z.Ŋ);}int ē=0;int Ҷ=0;bool ҵ=false;bool Ҵ=false;
IMyShipConnector Ŷ;public override bool Ɛ(bool ð){if(!ð){if(Ɠ.ˬ.EndsWith("e"))ҵ=true;if(Ɠ.ˬ.Contains("cn"))Ҵ=true;Ō.ŭ();ē=0;}if(ē==0){if
(!Ō.Ͼ("connector",Ɠ.ˤ,ð))return false;ē++;Ҷ=0;ð=false;}if(Ō.Г()<=0){Z.Ǖ("Docked: "+ĝ.ǲ("DO1"));return true;}for(;Ҷ<Ō.Г();
Ҷ++){Ŷ=Ō.ρ[Ҷ]as IMyShipConnector;if(Ŷ.Status==MyShipConnectorStatus.Connected){if(Ҵ){Z.Ì(Ŷ.CustomName+":");Z.ƴ(Ŷ.
OtherConnector.CubeGrid.CustomName);}else{Z.Ǖ(Ŷ.OtherConnector.CubeGrid.CustomName);}}else{if(ҵ){if(Ҵ){Z.Ì(Ŷ.CustomName+":");Z.ƴ("-");
}else Z.Ǖ("-");}}}return true;}}class ҳ:ƛ{public ҳ(){ɏ=30;ɒ="CmdEcho";}public override bool Ɛ(bool ð){string ʗ=(Ɠ.ˬ==
"center"?"c":(Ɠ.ˬ=="right"?"r":"n"));switch(ʗ){case"c":Z.Ƶ(Ɠ.ˢ);break;case"r":Z.ƴ(Ɠ.ˢ);break;default:Z.Ǖ(Ɠ.ˢ);break;}return true
;}}class Ҳ:ƛ{public Ҳ(){ɏ=1;ɒ="CmdGravity";}public override bool Ɛ(bool ð){string ʗ=(Ɠ.ˬ.Contains("nat")?"n":(Ɠ.ˬ.
Contains("art")?"a":(Ɠ.ˬ.Contains("tot")?"t":"s")));Vector3D Ё;if(Z.Ǉ.ɱ==null){Z.Ǖ("Gravity: "+ĝ.ǲ("GNC"));return true;}switch(ʗ
){case"n":Z.Ì(ĝ.ǲ("G2")+" ");Ё=Z.Ǉ.ɱ.GetNaturalGravity();Z.ƴ(Ё.Length().ToString("F1")+" m/s²");break;case"a":Z.Ì(ĝ.ǲ(
"G3")+" ");Ё=Z.Ǉ.ɱ.GetArtificialGravity();Z.ƴ(Ё.Length().ToString("F1")+" m/s²");break;case"t":Z.Ì(ĝ.ǲ("G1")+" ");Ё=Z.Ǉ.ɱ.
GetTotalGravity();Z.ƴ(Ё.Length().ToString("F1")+" m/s²");break;default:Z.Ì(ĝ.ǲ("GN"));Z.Ʊ(" | ",0.33f);Z.Ʊ(ĝ.ǲ("GA")+" | ",0.66f);Z.ƴ(ĝ
.ǲ("GT"),1.0f);Z.Ì("");Ё=Z.Ǉ.ɱ.GetNaturalGravity();Z.Ʊ(Ё.Length().ToString("F1")+" | ",0.33f);Ё=Z.Ǉ.ɱ.
GetArtificialGravity();Z.Ʊ(Ё.Length().ToString("F1")+" | ",0.66f);Ё=Z.Ǉ.ɱ.GetTotalGravity();Z.ƴ(Ё.Length().ToString("F1")+" ");break;}return
true;}}class ұ:ƛ{public ұ(){ɏ=0.5;ɒ="CmdHScroll";}ʏ Ұ=new ʏ();int ү=1;public override bool Ɛ(bool ð){if(Ұ.ʍ==0){string Ĳ=Ɠ.ˢ
+"  ";if(Ĳ.Length==0)return true;float Ү=Z.ɝ;float Ʈ=Z.ǧ(Ĳ,Z.ǒ);float љ=Ү/Ʈ;if(љ>1)Ұ.ɰ(string.Join("",Enumerable.Repeat(Ĳ
,(int)Math.Ceiling(љ))));else Ұ.ɰ(Ĳ);if(Ĳ.Length>40)ү=3;else if(Ĳ.Length>5)ү=2;else ү=1;Z.Ǖ(Ұ);return true;}bool Ό=Ɠ.ˬ.
EndsWith("r");if(Ό){Ұ.Ƴ.Insert(0,Ұ.ɕ(Ұ.ʍ-ү,ү));Ұ.ɖ(Ұ.ʍ-ү,ү);}else{Ұ.ɰ(Ұ.ɕ(0,ү));Ұ.ɖ(0,ү);}Z.Ǖ(Ұ);return true;}}class Ѿ:ƛ{public
Ѿ(){ɏ=7;ɒ="CmdInvList";}float ґ=-1;float Ґ=-1;public override void ɯ(){Ō=new ο(ƪ,Z.Ŋ);Ҙ=new Ə(ƪ,Z);}ʏ Ƴ=new ʏ(100);
Dictionary<string,string>ҏ=new Dictionary<string,string>();void Ҏ(string Ȭ,double Ҍ,int Ë){if(Ë>0){if(!ҷ)Z.ƾ(Math.Min(100,100*Ҍ/Ë)
,0.3f);string Ι;if(ҏ.ContainsKey(Ȭ)){Ι=ҏ[Ȭ];}else{if(!Ҕ)Ι=Z.ǥ(Ȭ,Z.ɝ*0.5f-Ҋ-Ґ);else{if(!ҷ)Ι=Z.ǥ(Ȭ,Z.ɝ*0.5f);else Ι=Z.ǥ(Ȭ,Z
.ɝ*0.9f);}ҏ[Ȭ]=Ι;}Ƴ.ŭ();if(!ҷ)Ƴ.ɰ(' ');if(!Ҕ){Z.Ì(Ƴ.ɰ(Ι).ɰ(' '));Z.Ʊ(Z.ȏ(Ҍ),1.0f,Ҋ+Ґ);Z.Ǖ(Ƴ.ŭ().ɰ(" / ").ɰ(Z.ȏ(Ë)));}else
{Z.Ǖ(Ƴ.ɰ(Ι));}}else{if(!Ҕ){Z.Ì(Ƴ.ŭ().ɰ(Ȭ).ɰ(':'));Z.ƴ(Z.ȏ(Ҍ),1.0f,ґ);}else Z.Ǖ(Ƴ.ŭ().ɰ(Ȭ));}}void Ғ(string Ȭ,double Ҍ,
double ҋ,int Ë){if(Ë>0){if(!Ҕ){Z.Ì(Ƴ.ŭ().ɰ(Ȭ).ɰ(' '));Z.Ʊ(Z.ȏ(Ҍ),0.51f);Z.Ì(Ƴ.ŭ().ɰ(" / ").ɰ(Z.ȏ(Ë)));Z.ƴ(Ƴ.ŭ().ɰ(" +").ɰ(Z.ȏ(
ҋ)).ɰ(" ").ɰ(ĝ.ǲ("I1")),1.0f);}else Z.Ǖ(Ƴ.ŭ().ɰ(Ȭ));if(!ҷ)Z.Ʒ(Math.Min(100,100*Ҍ/Ë));}else{if(!Ҕ){Z.Ì(Ƴ.ŭ().ɰ(Ȭ).ɰ(':'));
Z.Ʊ(Z.ȏ(Ҍ),0.51f);Z.ƴ(Ƴ.ŭ().ɰ(" +").ɰ(Z.ȏ(ҋ)).ɰ(" ").ɰ(ĝ.ǲ("I1")),1.0f);}else{Z.Ǖ(Ƴ.ŭ().ɰ(Ȭ));}}}float Ҋ=0;bool ҁ(Ǝ ſ){
int Ë=(Җ?ſ.ƍ:ſ.ƌ);if(Ë<0)return true;float ƺ=Z.ǧ(Z.ȏ(Ë),Z.ǒ);if(ƺ>Ҋ)Ҋ=ƺ;return true;}List<Ǝ>Ҁ;int ѿ=0;int ҍ=0;bool ғ(bool ð
,bool ҝ,string Ä,string Ы){if(!ð){ҍ=0;ѿ=0;}if(ҍ==0){if(Ӌ){if((Ҁ=Ҙ.Ż(Ä,ð,ҁ))==null)return false;}else{if((Ҁ=Ҙ.Ż(Ä,ð))==
null)return false;}ҍ++;ð=false;}if(Ҁ.Count>0){if(!ҝ&&!ð){if(!Z.ǚ)Z.Ǖ();Z.Ƶ(Ƴ.ŭ().ɰ("<< ").ɰ(Ы).ɰ(" ").ɰ(ĝ.ǲ("I2")).ɰ(" >>"))
;}for(;ѿ<Ҁ.Count;ѿ++){if(!ƪ.ʔ(30))return false;double Ҍ=Ҁ[ѿ].Ɗ;if(Җ&&Ҍ>=Ҁ[ѿ].ƍ)continue;int Ë=Ҁ[ѿ].ƌ;if(Җ)Ë=Ҁ[ѿ].ƍ;string
Ȭ=Z.Ǽ(Ҁ[ѿ].Å,Ҁ[ѿ].Ä);Ҏ(Ȭ,Ҍ,Ë);}}return true;}List<Ǝ>Ҝ;int қ=0;int Қ=0;bool ҙ(bool ð){if(!ð){қ=0;Қ=0;}if(Қ==0){if((Ҝ=Ҙ.Ż(
"Ingot",ð))==null)return false;Қ++;ð=false;}if(Ҝ.Count>0){if(!ҕ&&!ð){if(!Z.ǚ)Z.Ǖ();Z.Ƶ(Ƴ.ŭ().ɰ("<< ").ɰ(ĝ.ǲ("I4")).ɰ(" ").ɰ(ĝ.ǲ
("I2")).ɰ(" >>"));}for(;қ<Ҝ.Count;қ++){if(!ƪ.ʔ(40))return false;double Ҍ=Ҝ[қ].Ɗ;if(Җ&&Ҍ>=Ҝ[қ].ƍ)continue;int Ë=Ҝ[қ].ƌ;if(
Җ)Ë=Ҝ[қ].ƍ;string Ȭ=Z.Ǽ(Ҝ[қ].Å,Ҝ[қ].Ä);if(Ҝ[қ].Å!="Scrap"){double ҋ=Ҙ.ƀ(Ҝ[қ].Å+" Ore",Ҝ[қ].Å,"Ore").Ɗ;Ғ(Ȭ,Ҍ,ҋ,Ë);}else Ҏ(
Ȭ,Ҍ,Ë);}}return true;}ο Ō=null;Ə Ҙ;List<ʬ>ˏ;bool җ,Ͷ,Җ,ҕ,Ҕ,ҷ;int ŕ,ő;string Ӎ="";float ӌ=0;bool Ӌ=true;void ӊ(){if(Z.ǒ!=Ӎ
||ӌ!=Z.ɝ){ҏ.Clear();ӌ=Z.ɝ;}if(Z.ǒ!=Ӎ){Ґ=Z.ǧ(" / ",Z.ǒ);ґ=Z.Ǻ(' ',Z.ǒ);Ӎ=Z.ǒ;}Ō.ŭ();җ=Ɠ.ˬ.EndsWith("x")||Ɠ.ˬ.EndsWith("xs")
;Ͷ=Ɠ.ˬ.EndsWith("s")||Ɠ.ˬ.EndsWith("sx");Җ=Ɠ.ˬ.StartsWith("missing");ҕ=Ɠ.ˬ.Contains("list");ҷ=Ɠ.ˬ.Contains("nb");Ҕ=Ɠ.ˬ.
Contains("nn");Ҙ.ŭ();ˏ=Ɠ.ˏ;if(ˏ.Count==0)ˏ.Add(new ʬ("all"));}bool Ӊ(bool ð){if(!ð)ŕ=0;for(;ŕ<ˏ.Count;ŕ++){ʬ œ=ˏ[ŕ];œ.ʞ();string
Ä=œ.ʕ;if(!ð)ő=0;else ð=false;for(;ő<œ.ʠ.Count;ő++){if(!ƪ.ʔ(30))return false;string[]Ă=œ.ʠ[ő].Split(':');double ȍ;if(
string.Compare(Ă[0],"all",true)==0)Ă[0]="";int ƍ=1;int ƌ=-1;if(Ă.Length>1){if(Double.TryParse(Ă[1],out ȍ)){if(Җ)ƍ=(int)Math.
Ceiling(ȍ);else ƌ=(int)Math.Ceiling(ȍ);}}string Ơ=Ă[0];if(!string.IsNullOrEmpty(Ä))Ơ+=' '+Ä;Ҙ.ơ(Ơ,œ.ʫ=="-",ƍ,ƌ);}}return true;}
int ѱ=0;int ϖ=0;int ӈ=0;List<MyInventoryItem>Ï=new List<MyInventoryItem>();bool Ӈ(bool ð){ο Д=Ō;if(!ð)ѱ=0;for(;ѱ<Д.ρ.Count;
ѱ++){if(!ð)ϖ=0;for(;ϖ<Д.ρ[ѱ].InventoryCount;ϖ++){IMyInventory ώ=Д.ρ[ѱ].GetInventory(ϖ);if(!ð){ӈ=0;Ï.Clear();ώ.GetItems(Ï)
;}else ð=false;for(;ӈ<Ï.Count;ӈ++){if(!ƪ.ʔ(40))return false;MyInventoryItem o=Ï[ӈ];string Ç=Z.Ǿ(o);string Å,Ä;Z.Ȃ(Ç,out Å
,out Ä);if(string.Compare(Ä,"ore",true)==0){if(Ҙ.Ź(Å+" ingot",Å,"Ingot")&&Ҙ.Ź(Ç,Å,Ä))continue;}else{if(Ҙ.Ź(Ç,Å,Ä))
continue;}Z.Ȃ(Ç,out Å,out Ä);Ǝ ž=Ҙ.ƀ(Ç,Å,Ä);ž.Ɗ+=(double)o.Amount;}}}return true;}int ē=0;public override bool Ɛ(bool ð){if(!ð){
ӊ();ē=0;}for(;ē<=13;ē++){switch(ē){case 0:if(!Ō.ϣ(Ɠ.ˤ,ð))return false;break;case 1:if(!Ӊ(ð))return false;if(җ)ē++;break;
case 2:if(!Ҙ.ƈ(ð))return false;break;case 3:if(!Ӈ(ð))return false;break;case 4:if(!ғ(ð,ҕ,"Ore",ĝ.ǲ("I3")))return false;break
;case 5:if(Ͷ){if(!ғ(ð,ҕ,"Ingot",ĝ.ǲ("I4")))return false;}else{if(!ҙ(ð))return false;}break;case 6:if(!ғ(ð,ҕ,"Component",ĝ
.ǲ("I5")))return false;break;case 7:if(!ғ(ð,ҕ,"OxygenContainerObject",ĝ.ǲ("I6")))return false;break;case 8:if(!ғ(ð,true,
"GasContainerObject",""))return false;break;case 9:if(!ғ(ð,ҕ,"AmmoMagazine",ĝ.ǲ("I7")))return false;break;case 10:if(!ғ(ð,ҕ,
"PhysicalGunObject",ĝ.ǲ("I8")))return false;break;case 11:if(!ғ(ð,true,"Datapad",""))return false;break;case 12:if(!ғ(ð,true,
"ConsumableItem",""))return false;break;case 13:if(!ғ(ð,true,"PhysicalObject",""))return false;break;}ð=false;}Ӌ=false;return true;}}
class ӗ:ƛ{public ӗ(){ɏ=2;ɒ="CmdAmount";}ο Ō;public override void ɯ(){Ō=new ο(ƪ,Z.Ŋ);}bool Ӗ;bool ӕ=false;int Œ=0;int ŕ=0;int
ő=0;public override bool Ɛ(bool ð){if(!ð){Ӗ=!Ɠ.ˬ.EndsWith("x");ӕ=Ɠ.ˬ.EndsWith("bar");if(ӕ)Ӗ=true;if(Ɠ.ˏ.Count==0)Ɠ.ˏ.Add(
new ʬ("reactor,gatlingturret,missileturret,interiorturret,gatlinggun,launcherreload,launcher,oxygenerator"));ŕ=0;}for(;ŕ<Ɠ.
ˏ.Count;ŕ++){ʬ œ=Ɠ.ˏ[ŕ];if(!ð){œ.ʞ();Œ=0;ő=0;}for(;ő<œ.ʠ.Count;ő++){if(Œ==0){if(!ð){if(œ.ʠ[ő]=="")continue;Ō.ŭ();}string
ŏ=œ.ʠ[ő];if(!Ō.Ͼ(ŏ,Ɠ.ˤ,ð))return false;Œ++;ð=false;}if(!ӎ(ð))return false;ð=false;Œ=0;}}return true;}int Ӕ=0;int ħ=0;
double ž=0;double ӓ=0;double Ӓ=0;int ӈ=0;IMyTerminalBlock ӑ;IMyInventory Ӑ;List<MyInventoryItem>Ï=new List<MyInventoryItem>();
string ӏ="";bool ӎ(bool ð){if(!ð){Ӕ=0;ħ=0;}for(;Ӕ<Ō.Г();Ӕ++){if(ħ==0){if(!ƪ.ʔ(50))return false;ӑ=Ō.ρ[Ӕ];Ӑ=ӑ.GetInventory(0);if
(Ӑ==null)continue;ħ++;ð=false;}if(!ð){Ï.Clear();Ӑ.GetItems(Ï);ӏ=(Ï.Count>0?Ï[0].Type.ToString():"");ӈ=0;ž=0;ӓ=0;Ӓ=0;}for(
;ӈ<Ï.Count;ӈ++){if(!ƪ.ʔ(30))return false;MyInventoryItem o=Ï[ӈ];if(o.Type.ToString()!=ӏ)Ӓ+=(double)o.Amount;else ž+=(
double)o.Amount;}string һ=ĝ.ǲ("A1");string Ɣ=ӑ.CustomName;if(ž>0&&(double)Ӑ.CurrentVolume>0){double Һ=Ӓ*(double)Ӑ.
CurrentVolume/(ž+Ӓ);ӓ=Math.Floor(ž*((double)Ӑ.MaxVolume-Һ)/((double)Ӑ.CurrentVolume-Һ));һ=Z.ȏ(ž)+" / "+(Ӓ>0?"~":"")+Z.ȏ(ӓ);}if(!ӕ||ӓ
<=0){Ɣ=Z.ǥ(Ɣ,Z.ɝ*0.8f);Z.Ì(Ɣ);Z.ƴ(һ);}if(Ӗ&&ӓ>0){double ʙ=100*ž/ӓ;Z.Ʒ(ʙ);}ħ=0;ð=false;}return true;}}class ҹ:ƛ{ο Ō;public
ҹ(){ɏ=2;ɒ="CmdMass";}public override void ɯ(){Ō=new ο(ƪ,Z.Ŋ);}bool Ͷ=false;bool Ω=false;int ē=0;public override bool Ɛ(
bool ð){if(!ð){Ō.ŭ();Ͷ=(Ɠ.ˬ[Ɠ.ˬ.Length-1]=='x');Ω=(Ɠ.ˬ[Ɠ.ˬ.Length-1]=='p');ē=0;}if(ē==0){if(!Ō.ϣ(Ɠ.ˤ,ð))return false;ē++;ð=
false;}double È=Ō.Ϗ(ð);if(Double.IsNaN(È))return false;double ʚ=0;int ќ=Ɠ.ˏ.Count;if(ќ>0){double.TryParse(Ɠ.ˏ[0].œ.Trim(),out
ʚ);if(ќ>1){string џ=Ɠ.ˏ[1].œ.Trim();char ј=' ';if(џ.Length>0)ј=Char.ToLower(џ[0]);int ћ="kmgtpezy".IndexOf(ј);if(ћ>=0)ʚ*=
Math.Pow(1000.0,ћ);}ʚ*=1000.0;}Z.Ì(ĝ.ǲ("M1")+" ");if(ʚ<=0){Z.ƴ(Z.Ȏ(È,false));return true;}double ʙ=È/ʚ*100;if(!Ͷ&&!Ω){Z.ƴ(Z.
Ȏ(È)+" / "+Z.Ȏ(ʚ));Z.ƾ(ʙ,1.0f,Z.ƶ);Z.Ǖ(' '+Z.Ȇ(ʙ)+"%");}else if(Ω){Z.ƴ(Z.Ȇ(ʙ)+"%");Z.Ʒ(ʙ);}else Z.ƴ(Z.Ȇ(ʙ)+"%");return
true;}}class Ҹ:ƛ{ȷ ǈ;ο Ō;public Ҹ(){ɏ=3;ɒ="CmdOxygen";}public override void ɯ(){ǈ=Z.ǈ;Ō=new ο(ƪ,Z.Ŋ);}int ē=0;int ą=0;bool Ƃ
=false;int Ҽ=0;double ȟ=0;double Ƞ=0;double ƽ;public override bool Ɛ(bool ð){if(!ð){Ō.ŭ();ē=0;ą=0;ƽ=0;}if(ē==0){if(!Ō.Ͼ(
"airvent",Ɠ.ˤ,ð))return false;Ƃ=(Ō.Г()>0);ē++;ð=false;}if(ē==1){for(;ą<Ō.Г();ą++){if(!ƪ.ʔ(8))return false;IMyAirVent ň=Ō.ρ[ą]as
IMyAirVent;ƽ=Math.Max(ň.GetOxygenLevel()*100,0f);Z.Ì(ň.CustomName);if(ň.CanPressurize)Z.ƴ(Z.Ȇ(ƽ)+"%");else Z.ƴ(ĝ.ǲ("O1"));Z.Ʒ(ƽ);}
ē++;ð=false;}if(ē==2){if(!ð)Ō.ŭ();if(!Ō.Ͼ("oxyfarm",Ɠ.ˤ,ð))return false;Ҽ=Ō.Г();ē++;ð=false;}if(ē==3){if(Ҽ>0){if(!ð)ą=0;
double ӆ=0;for(;ą<Ҽ;ą++){if(!ƪ.ʔ(4))return false;IMyOxygenFarm Ӆ=Ō.ρ[ą]as IMyOxygenFarm;ӆ+=Ӆ.GetOutput()*100;}ƽ=ӆ/Ҽ;if(Ƃ)Z.Ǖ(
"");Ƃ|=(Ҽ>0);Z.Ì(ĝ.ǲ("O2"));Z.ƴ(Z.Ȇ(ƽ)+"%");Z.Ʒ(ƽ);}ē++;ð=false;}if(ē==4){if(!ð)Ō.ŭ();if(!Ō.Ͼ("oxytank",Ɠ.ˤ,ð))return
false;Ҽ=Ō.Г();if(Ҽ==0){if(!Ƃ)Z.Ǖ(ĝ.ǲ("O3"));return true;}ē++;ð=false;}if(ē==5){if(!ð){ȟ=0;Ƞ=0;ą=0;}if(!ǈ.ȑ(Ō.ρ,"oxygen",ref Ƞ
,ref ȟ,ð))return false;if(ȟ==0){if(!Ƃ)Z.Ǖ(ĝ.ǲ("O3"));return true;}ƽ=Ƞ/ȟ*100;if(Ƃ)Z.Ǖ("");Z.Ì(ĝ.ǲ("O4"));Z.ƴ(Z.Ȇ(ƽ)+"%");Z
.Ʒ(ƽ);ē++;}return true;}}class ӄ:ƛ{public ӄ(){ɏ=1;ɒ="CmdPosition";}public override bool Ɛ(bool ð){bool Ӄ=(Ɠ.ˬ=="posxyz");
bool ҥ=(Ɠ.ˬ=="posgps");IMyTerminalBlock Ý=Ĝ.v.Ý;if(Ɠ.ˤ!=""&&Ɠ.ˤ!="*"){Ý=Z.ǆ.GetBlockWithName(Ɠ.ˤ);if(Ý==null){Z.Ǖ("Pos: "+ĝ.
ǲ("P1")+": "+Ɠ.ˤ);return true;}}if(ҥ){Vector3D Ŀ=Ý.GetPosition();Z.Ǖ("GPS:"+ĝ.ǲ("P2")+":"+Ŀ.GetDim(0).ToString("F2")+":"+
Ŀ.GetDim(1).ToString("F2")+":"+Ŀ.GetDim(2).ToString("F2")+":");return true;}Z.Ì(ĝ.ǲ("P2")+": ");if(!Ӄ){Z.ƴ(Ý.GetPosition(
).ToString("F0"));return true;}Z.Ǖ("");Z.Ì(" X: ");Z.ƴ(Ý.GetPosition().GetDim(0).ToString("F0"));Z.Ì(" Y: ");Z.ƴ(Ý.
GetPosition().GetDim(1).ToString("F0"));Z.Ì(" Z: ");Z.ƴ(Ý.GetPosition().GetDim(2).ToString("F0"));return true;}}class ӂ:ƛ{public ӂ(
){ɏ=3;ɒ="CmdPower";}ȷ ǈ;ο Ӂ;ο Ӏ;ο ҿ;ο ё;ο Ҿ;ο Ō;public override void ɯ(){Ӂ=new ο(ƪ,Z.Ŋ);Ӏ=new ο(ƪ,Z.Ŋ);ҿ=new ο(ƪ,Z.Ŋ);ё=
new ο(ƪ,Z.Ŋ);Ҿ=new ο(ƪ,Z.Ŋ);Ō=new ο(ƪ,Z.Ŋ);ǈ=Z.ǈ;}string Ы;bool ҽ;string ѐ;string ѽ;int Щ;int ē=0;public override bool Ɛ(
bool ð){if(!ð){Ы=(Ɠ.ˬ.EndsWith("x")?"s":(Ɠ.ˬ.EndsWith("p")?"p":(Ɠ.ˬ.EndsWith("v")?"v":"n")));ҽ=(Ɠ.ˬ.StartsWith(
"powersummary"));ѐ="a";ѽ="";if(Ɠ.ˬ.Contains("stored"))ѐ="s";else if(Ɠ.ˬ.Contains("in"))ѐ="i";else if(Ɠ.ˬ.Contains("out"))ѐ="o";ē=0;Ӂ.ŭ
();Ӏ.ŭ();ҿ.ŭ();ё.ŭ();Ҿ.ŭ();}if(ѐ=="a"){if(ē==0){if(!Ӂ.Ͼ("reactor",Ɠ.ˤ,ð))return false;ð=false;ē++;}if(ē==1){if(!Ӏ.Ͼ(
"hydrogenengine",Ɠ.ˤ,ð))return false;ð=false;ē++;}if(ē==2){if(!ҿ.Ͼ("solarpanel",Ɠ.ˤ,ð))return false;ð=false;ē++;}if(ē==3){if(!Ҿ.Ͼ(
"windturbine",Ɠ.ˤ,ð))return false;ð=false;ē++;}}else if(ē==0)ē=4;if(ē==4){if(!ё.Ͼ("battery",Ɠ.ˤ,ð))return false;ð=false;ē++;}int у=Ӂ.
Г();int т=Ӏ.Г();int с=ҿ.Г();int р=ё.Г();int п=Ҿ.Г();if(ē==5){Щ=0;if(у>0)Щ++;if(т>0)Щ++;if(с>0)Щ++;if(п>0)Щ++;if(р>0)Щ++;
if(Щ<1){Z.Ǖ(ĝ.ǲ("P6"));return true;}if(Ɠ.ˏ.Count>0){if(Ɠ.ˏ[0].œ.Length>0)ѽ=Ɠ.ˏ[0].œ;}ē++;ð=false;}if(ѐ!="a"){if(!ђ(ё,(ѽ==
""?ĝ.ǲ("P7"):ѽ),ѐ,Ы,ð))return false;return true;}string з=ĝ.ǲ("P8");if(!ҽ){if(ē==6){if(у>0)if(!й(Ӂ,(ѽ==""?ĝ.ǲ("P9"):ѽ),Ы,ð
))return false;ē++;ð=false;}if(ē==7){if(т>0)if(!й(Ӏ,(ѽ==""?ĝ.ǲ("P12"):ѽ),Ы,ð))return false;ē++;ð=false;}if(ē==8){if(с>0)
if(!й(ҿ,(ѽ==""?ĝ.ǲ("P10"):ѽ),Ы,ð))return false;ē++;ð=false;}if(ē==9){if(п>0)if(!й(Ҿ,(ѽ==""?ĝ.ǲ("P13"):ѽ),Ы,ð))return false
;ē++;ð=false;}if(ē==10){if(р>0)if(!ђ(ё,(ѽ==""?ĝ.ǲ("P7"):ѽ),ѐ,Ы,ð))return false;ē++;ð=false;}}else{з=ĝ.ǲ("P11");Щ=10;if(ē
==6)ē=11;}if(Щ==1)return true;if(!ð){Ō.ŭ();Ō.Е(Ӂ);Ō.Е(Ӏ);Ō.Е(ҿ);Ō.Е(Ҿ);Ō.Е(ё);}if(!й(Ō,з,Ы,ð))return false;return true;}
void о(double ʝ,double ƌ){double ж=(ƌ>0?ʝ/ƌ*100:0);switch(Ы){case"s":Z.ƴ(Ƴ.ŭ().ɰ(' ').ɰ(ж.ToString("F1")).ɰ("%"));break;case
"v":Z.ƴ(Ƴ.ŭ().ɰ(Z.ȏ(ʝ)).ɰ("W / ").ɰ(Z.ȏ(ƌ)).ɰ("W"));break;case"c":Z.ƴ(Ƴ.ŭ().ɰ(Z.ȏ(ʝ)).ɰ("W"));break;case"p":Z.ƴ(Ƴ.ŭ().ɰ(' '
).ɰ(ж.ToString("F1")).ɰ("%"));Z.Ʒ(ж);break;default:Z.ƴ(Ƴ.ŭ().ɰ(Z.ȏ(ʝ)).ɰ("W / ").ɰ(Z.ȏ(ƌ)).ɰ("W"));Z.ƾ(ж,1.0f,Z.ƶ);Z.ƴ(Ƴ.
ŭ().ɰ(' ').ɰ(ж.ToString("F1")).ɰ("%"));break;}}double м=0;double К=0,л=0;int к=0;bool й(ο и,string з,string ʗ,bool ð){if(
!ð){К=0;л=0;к=0;}if(к==0){if(!ǈ.ɀ(и.ρ,ǈ.ȶ,ref м,ref м,ref К,ref л,ð))return false;к++;ð=false;}if(!ƪ.ʔ(50))return false;
double ж=(л>0?К/л*100:0);Z.Ì(з+": ");о(К*1000000,л*1000000);return true;}double н=0,е=0,ф=0,і=0;double ѕ=0,є=0;int ѓ=0;ʏ Ƴ=new
ʏ(100);bool ђ(ο ё,string з,string ѐ,string ʗ,bool ð){if(!ð){н=е=0;ф=і=0;ѕ=є=0;ѓ=0;}if(ѓ==0){if(!ǈ.Ʌ(ё.ρ,ref ф,ref і,ref н
,ref е,ref ѕ,ref є,ð))return false;ф*=1000000;і*=1000000;н*=1000000;е*=1000000;ѕ*=1000000;є*=1000000;ѓ++;ð=false;}double
я=(є>0?ѕ/є*100:0);double ю=(е>0?н/е*100:0);double э=(і>0?ф/і*100:0);bool ь=ѐ=="a";if(ѓ==1){if(!ƪ.ʔ(50))return false;if(ь)
{if(ʗ!="p"){Z.Ì(Ƴ.ŭ().ɰ(з).ɰ(": "));Z.ƴ(Ƴ.ŭ().ɰ("(IN ").ɰ(Z.ȏ(ф)).ɰ("W / OUT ").ɰ(Z.ȏ(н)).ɰ("W)"));}else Z.Ǖ(Ƴ.ŭ().ɰ(з).ɰ
(": "));Z.Ì(Ƴ.ŭ().ɰ("  ").ɰ(ĝ.ǲ("P3")).ɰ(": "));}else Z.Ì(Ƴ.ŭ().ɰ(з).ɰ(": "));if(ь||ѐ=="s")switch(ʗ){case"s":Z.ƴ(Ƴ.ŭ().ɰ(
' ').ɰ(я.ToString("F1")).ɰ("%"));break;case"v":Z.ƴ(Ƴ.ŭ().ɰ(Z.ȏ(ѕ)).ɰ("Wh / ").ɰ(Z.ȏ(є)).ɰ("Wh"));break;case"p":Z.ƴ(Ƴ.ŭ().ɰ(
' ').ɰ(я.ToString("F1")).ɰ("%"));Z.Ʒ(я);break;default:Z.ƴ(Ƴ.ŭ().ɰ(Z.ȏ(ѕ)).ɰ("Wh / ").ɰ(Z.ȏ(є)).ɰ("Wh"));Z.ƾ(я,1.0f,Z.ƶ);Z.ƴ
(Ƴ.ŭ().ɰ(' ').ɰ(я.ToString("F1")).ɰ("%"));break;}if(ѐ=="s")return true;ѓ++;ð=false;}if(ѓ==2){if(!ƪ.ʔ(50))return false;if(
ь)Z.Ì(Ƴ.ŭ().ɰ("  ").ɰ(ĝ.ǲ("P4")).ɰ(": "));if(ь||ѐ=="o")switch(ʗ){case"s":Z.ƴ(Ƴ.ŭ().ɰ(' ').ɰ(ю.ToString("F1")).ɰ("%"));
break;case"v":Z.ƴ(Ƴ.ŭ().ɰ(Z.ȏ(н)).ɰ("W / ").ɰ(Z.ȏ(е)).ɰ("W"));break;case"p":Z.ƴ(Ƴ.ŭ().ɰ(' ').ɰ(ю.ToString("F1")).ɰ("%"));Z.Ʒ(
ю);break;default:Z.ƴ(Ƴ.ŭ().ɰ(Z.ȏ(н)).ɰ("W / ").ɰ(Z.ȏ(е)).ɰ("W"));Z.ƾ(ю,1.0f,Z.ƶ);Z.ƴ(Ƴ.ŭ().ɰ(' ').ɰ(ю.ToString("F1")).ɰ(
"%"));break;}if(ѐ=="o")return true;ѓ++;ð=false;}if(!ƪ.ʔ(50))return false;if(ь)Z.Ì(Ƴ.ŭ().ɰ("  ").ɰ(ĝ.ǲ("P5")).ɰ(": "));if(ь
||ѐ=="i")switch(ʗ){case"s":Z.ƴ(Ƴ.ŭ().ɰ(' ').ɰ(э.ToString("F1")).ɰ("%"));break;case"v":Z.ƴ(Ƴ.ŭ().ɰ(Z.ȏ(ф)).ɰ("W / ").ɰ(Z.ȏ(
і)).ɰ("W"));break;case"p":Z.ƴ(Ƴ.ŭ().ɰ(' ').ɰ(э.ToString("F1")).ɰ("%"));Z.Ʒ(э);break;default:Z.ƴ(Ƴ.ŭ().ɰ(Z.ȏ(ф)).ɰ("W / ")
.ɰ(Z.ȏ(і)).ɰ("W"));Z.ƾ(э,1.0f,Z.ƶ);Z.ƴ(Ƴ.ŭ().ɰ(' ').ɰ(э.ToString("F1")).ɰ("%"));break;}return true;}}class ы:ƛ{public ы()
{ɏ=7;ɒ="CmdPowerTime";}class ъ{public TimeSpan Ģ=new TimeSpan(-1);public double г=-1;public double щ=0;}ъ ш=new ъ();ο ч;ο
ц;public override void ɯ(){ч=new ο(ƪ,Z.Ŋ);ц=new ο(ƪ,Z.Ŋ);}int х=0;double д=0;double Ч=0,Ж=0;double Ц=0,Х=0,Ф=0;double У=0
,Т=0;int С=0;private bool Р(string ˤ,out TimeSpan П,out double ή,bool ð){MyResourceSourceComponent ȴ;
MyResourceSinkComponent Ȟ;double Н=ɐ;ъ М=ш;П=М.Ģ;ή=М.г;if(!ð){ч.ŭ();ц.ŭ();М.г=0;х=0;д=0;Ч=Ж=0;Ц=0;Х=Ф=0;У=Т=0;С=0;}if(х==0){if(!ч.Ͼ("reactor",ˤ
,ð))return false;ð=false;х++;}if(х==1){for(;С<ч.ρ.Count;С++){if(!ƪ.ʔ(6))return false;IMyReactor Ý=ч.ρ[С]as IMyReactor;if(
Ý==null||!Ý.IsWorking)continue;if(Ý.Components.TryGet<MyResourceSourceComponent>(out ȴ)){Ч+=ȴ.CurrentOutputByType(Z.ǈ.ȶ);
Ж+=ȴ.MaxOutputByType(Z.ǈ.ȶ);}д+=(double)Ý.GetInventory(0).CurrentMass;}ð=false;х++;}if(х==2){if(!ц.Ͼ("battery",ˤ,ð))
return false;ð=false;х++;}if(х==3){if(!ð)С=0;for(;С<ц.ρ.Count;С++){if(!ƪ.ʔ(15))return false;IMyBatteryBlock Ý=ц.ρ[С]as
IMyBatteryBlock;if(Ý==null||!Ý.IsWorking)continue;if(Ý.Components.TryGet<MyResourceSourceComponent>(out ȴ)){Х=ȴ.CurrentOutputByType(Z.ǈ
.ȶ);Ф=ȴ.MaxOutputByType(Z.ǈ.ȶ);}if(Ý.Components.TryGet<MyResourceSinkComponent>(out Ȟ)){Х-=Ȟ.CurrentInputByType(Z.ǈ.ȶ);}
double Л=(Х<0?(Ý.MaxStoredPower-Ý.CurrentStoredPower)/(-Х/3600):0);if(Л>М.г)М.г=Л;if(Ý.ChargeMode==ChargeMode.Recharge)
continue;У+=Х;Т+=Ф;Ц+=Ý.CurrentStoredPower;}ð=false;х++;}double К=Ч+У;if(К<=0)М.Ģ=TimeSpan.FromSeconds(-1);else{double Й=М.Ģ.
TotalSeconds;double И;double З=(М.щ-д)/Н;if(Ч<=0)З=Math.Min(К,Ж)/3600000;double О=0;if(Т>0)О=Math.Min(К,Т)/3600;if(З<=0&&О<=0)И=-1;
else if(З<=0)И=Ц/О;else if(О<=0)И=д/З;else{double Ш=О;double Ю=(Ч<=0?К/3600:З*К/Ч);И=Ц/Ш+д/Ю;}if(Й<=0||И<0)Й=И;else Й=(Й+И)/
2;try{М.Ģ=TimeSpan.FromSeconds(Й);}catch{М.Ģ=TimeSpan.FromSeconds(-1);}}М.щ=д;ή=М.г;П=М.Ģ;return true;}int ē=0;bool Σ=
false;bool Ͷ=false;bool Ω=false;double г=0;TimeSpan ț;int в=0,б=0,а=0;int ǩ=0;int Я=0;public override bool Ɛ(bool ð){if(!ð){Σ
=Ɠ.ˬ.EndsWith("bar");Ͷ=(Ɠ.ˬ[Ɠ.ˬ.Length-1]=='x');Ω=(Ɠ.ˬ[Ɠ.ˬ.Length-1]=='p');ē=0;в=б=а=ǩ=0;Я=0;г=0;}if(ē==0){if(Ɠ.ˏ.Count>0
){for(;Я<Ɠ.ˏ.Count;Я++){if(!ƪ.ʔ(100))return false;Ɠ.ˏ[Я].ʞ();if(Ɠ.ˏ[Я].ʠ.Count<=0)continue;string œ=Ɠ.ˏ[Я].ʠ[0];int.
TryParse(œ,out ǩ);if(Я==0)в=ǩ;else if(Я==1)б=ǩ;else if(Я==2)а=ǩ;}}ē++;ð=false;}if(ē==1){if(!Р(Ɠ.ˤ,out ț,out г,ð))return false;ē
++;ð=false;}if(!ƪ.ʔ(30))return false;double Ģ=0;TimeSpan Э;try{Э=new TimeSpan(в,б,а);}catch{Э=TimeSpan.FromSeconds(-1);}
string Ĳ;if(ț.TotalSeconds>0||г<=0){if(!Σ)Z.Ì(ĝ.ǲ("PT1")+" ");Ĳ=Z.ǈ.Ȣ(ț);Ģ=ț.TotalSeconds;}else{if(!Σ)Z.Ì(ĝ.ǲ("PT2")+" ");Ĳ=Z.
ǈ.Ȣ(TimeSpan.FromSeconds(г));if(Э.TotalSeconds>=г)Ģ=Э.TotalSeconds-г;else Ģ=0;}if(Э.Ticks<=0){Z.ƴ(Ĳ);return true;}double
ʙ=Ģ/Э.TotalSeconds*100;if(ʙ>100)ʙ=100;if(Σ){Z.Ʒ(ʙ);return true;}if(!Ͷ&&!Ω){Z.ƴ(Ĳ);Z.ƾ(ʙ,1.0f,Z.ƶ);Z.Ǖ(' '+ʙ.ToString(
"0.0")+"%");}else if(Ω){Z.ƴ(ʙ.ToString("0.0")+"%");Z.Ʒ(ʙ);}else Z.ƴ(ʙ.ToString("0.0")+"%");return true;}}class Ь:ƛ{public Ь()
{ɏ=7;ɒ="CmdPowerUsed";}ȷ ǈ;ο Ō;public override void ɯ(){Ō=new ο(ƪ,Z.Ŋ);ǈ=Z.ǈ;}string Ы;string Ъ;string ϩ;void о(double ʝ,
double ƌ){double ж=(ƌ>0?ʝ/ƌ*100:0);switch(Ы){case"s":Z.ƴ(ж.ToString("0.0")+"%",1.0f);break;case"v":Z.ƴ(Z.ȏ(ʝ)+"W / "+Z.ȏ(ƌ)+
"W",1.0f);break;case"c":Z.ƴ(Z.ȏ(ʝ)+"W",1.0f);break;case"p":Z.ƴ(ж.ToString("0.0")+"%",1.0f);Z.Ʒ(ж);break;default:Z.ƴ(Z.ȏ(ʝ)+
"W / "+Z.ȏ(ƌ)+"W");Z.ƾ(ж,1.0f,Z.ƶ);Z.ƴ(' '+ж.ToString("0.0")+"%");break;}}double Ⱦ=0,Ƚ=0;int ѱ=0;int ē=0;Ѭ Ѱ=new Ѭ();public
override bool Ɛ(bool ð){if(!ð){Ы=(Ɠ.ˬ.EndsWith("x")?"s":(Ɠ.ˬ.EndsWith("usedp")||Ɠ.ˬ.EndsWith("topp")?"p":(Ɠ.ˬ.EndsWith("v")?"v":
(Ɠ.ˬ.EndsWith("c")?"c":"n"))));Ъ=(Ɠ.ˬ.Contains("top")?"top":"");ϩ=(Ɠ.ˏ.Count>0?Ɠ.ˏ[0].œ:ĝ.ǲ("PU1"));Ⱦ=Ƚ=0;ē=0;ѱ=0;Ō.ŭ();Ѱ
.b();}if(ē==0){if(!Ō.ϣ(Ɠ.ˤ,ð))return false;ð=false;ē++;}MyResourceSinkComponent Ȟ;MyResourceSourceComponent ȴ;switch(Ъ){
case"top":if(ē==1){for(;ѱ<Ō.ρ.Count;ѱ++){if(!ƪ.ʔ(20))return false;IMyTerminalBlock Ý=Ō.ρ[ѱ];if(Ý.Components.TryGet<
MyResourceSinkComponent>(out Ȟ)){ListReader<MyDefinitionId>Ȝ=Ȟ.AcceptedResources;if(Ȝ.IndexOf(ǈ.ȶ)<0)continue;Ⱦ=Ȟ.CurrentInputByType(ǈ.ȶ)*
1000000;}else continue;Ѱ.z(Ⱦ,Ý);}ð=false;ē++;}if(Ѱ.n()<=0){Z.Ǖ("PowerUsedTop: "+ĝ.ǲ("D2"));return true;}int ķ=10;if(Ɠ.ˏ.Count>0
)if(!int.TryParse(ϩ,out ķ)){ķ=10;}if(ķ>Ѱ.n())ķ=Ѱ.n();if(ē==2){if(!ð){ѱ=Ѱ.n()-1;Ѱ.a();}for(;ѱ>=Ѱ.n()-ķ;ѱ--){if(!ƪ.ʔ(30))
return false;IMyTerminalBlock Ý=Ѱ.f(ѱ);string Ɣ=Z.ǥ(Ý.CustomName,Z.ɝ*0.4f);if(Ý.Components.TryGet<MyResourceSinkComponent>(out
Ȟ)){Ⱦ=Ȟ.CurrentInputByType(ǈ.ȶ)*1000000;Ƚ=Ȟ.MaxRequiredInputByType(ǈ.ȶ)*1000000;var ѭ=(Ý as IMyRadioAntenna);if(ѭ!=null)Ƚ
*=ѭ.Radius/500;}Z.Ì(Ɣ+" ");о(Ⱦ,Ƚ);}}break;default:for(;ѱ<Ō.ρ.Count;ѱ++){if(!ƪ.ʔ(10))return false;double Ѯ;IMyTerminalBlock
Ý=Ō.ρ[ѱ];if(Ý.Components.TryGet<MyResourceSinkComponent>(out Ȟ)){ListReader<MyDefinitionId>Ȝ=Ȟ.AcceptedResources;if(Ȝ.
IndexOf(ǈ.ȶ)<0)continue;Ѯ=Ȟ.CurrentInputByType(ǈ.ȶ);Ƚ+=Ȟ.MaxRequiredInputByType(ǈ.ȶ);var ѭ=(Ý as IMyRadioAntenna);if(ѭ!=null){Ƚ
*=ѭ.Radius/500;}}else continue;if(Ý.Components.TryGet<MyResourceSourceComponent>(out ȴ)&&(Ý as IMyBatteryBlock!=null)){Ѯ-=
ȴ.CurrentOutputByType(ǈ.ȶ);if(Ѯ<=0)continue;}Ⱦ+=Ѯ;}Z.Ì(ϩ);о(Ⱦ*1000000,Ƚ*1000000);break;}return true;}public class Ѭ{List<
KeyValuePair<double,IMyTerminalBlock>>ѫ=new List<KeyValuePair<double,IMyTerminalBlock>>();public void z(double ѯ,IMyTerminalBlock Ý)
{ѫ.Add(new KeyValuePair<double,IMyTerminalBlock>(ѯ,Ý));}public int n(){return ѫ.Count;}public IMyTerminalBlock f(int e){
return ѫ[e].Value;}public void b(){ѫ.Clear();}public void a(){ѫ.Sort((Ϥ,Ѽ)=>(Ϥ.Key.CompareTo(Ѽ.Key)));}}}class ѻ:ƛ{ο Ō;public
ѻ(){ɏ=1;ɒ="CmdProp";}public override void ɯ(){Ō=new ο(ƪ,Z.Ŋ);}int ē=0;int ѱ=0;bool Ѻ=false;string ѹ=null;string Ѹ=null;
string ѷ=null;string Ѷ=null;public override bool Ɛ(bool ð){if(!ð){Ѻ=Ɠ.ˬ.StartsWith("props");ѹ=Ѹ=ѷ=Ѷ=null;ѱ=0;ē=0;}if(Ɠ.ˏ.Count
<1){Z.Ǖ(Ɠ.ˬ+": "+"Missing property name.");return true;}if(ē==0){if(!ð)Ō.ŭ();if(!Ō.ϣ(Ɠ.ˤ,ð))return false;ѵ();ē++;ð=false;
}if(ē==1){int ķ=Ō.Г();if(ķ==0){Z.Ǖ(Ɠ.ˬ+": "+"No blocks found.");return true;}for(;ѱ<ķ;ѱ++){if(!ƪ.ʔ(50))return false;
IMyTerminalBlock Ý=Ō.ρ[ѱ];if(Ý.GetProperty(ѹ)!=null){if(Ѹ==null){string ϩ=Z.ǥ(Ý.CustomName,Z.ɝ*0.7f);Z.Ì(ϩ);}else Z.Ì(Ѹ);Z.ƴ(Ѵ(Ý,ѹ,ѷ,Ѷ))
;if(!Ѻ)return true;}}}return true;}void ѵ(){ѹ=Ɠ.ˏ[0].œ;if(Ɠ.ˏ.Count>1){if(!Ѻ)Ѹ=Ɠ.ˏ[1].œ;else ѷ=Ɠ.ˏ[1].œ;if(Ɠ.ˏ.Count>2){
if(!Ѻ)ѷ=Ɠ.ˏ[2].œ;else Ѷ=Ɠ.ˏ[2].œ;if(Ɠ.ˏ.Count>3&&!Ѻ)Ѷ=Ɠ.ˏ[3].œ;}}}string Ѵ(IMyTerminalBlock Ý,string ѳ,string Ѳ=null,
string Ѫ=null){return(Ý.GetValue<bool>(ѳ)?(Ѳ!=null?Ѳ:ĝ.ǲ("W9")):(Ѫ!=null?Ѫ:ĝ.ǲ("W1")));}}class Ѡ:ƛ{public Ѡ(){ɏ=5;ɒ=
"CmdShipCtrl";}ο Ō;public override void ɯ(){Ō=new ο(ƪ,Z.Ŋ);}public override bool Ɛ(bool ð){if(!ð)Ō.ŭ();if(!Ō.Ͼ("shipctrl",Ɠ.ˤ,ð))
return false;if(Ō.Г()<=0){if(Ɠ.ˤ!=""&&Ɠ.ˤ!="*")Z.Ǖ(Ɠ.ˬ+": "+ĝ.ǲ("SC1")+" ("+Ɠ.ˤ+")");else Z.Ǖ(Ɠ.ˬ+": "+ĝ.ǲ("SC1"));return true
;}if(Ɠ.ˬ.StartsWith("damp")){bool а=(Ō.ρ[0]as IMyShipController).DampenersOverride;Z.Ì(ĝ.ǲ("SCD"));Z.ƴ(а?"ON":"OFF");}
else{bool а=(Ō.ρ[0]as IMyShipController).IsUnderControl;Z.Ì(ĝ.ǲ("SCO"));Z.ƴ(а?"YES":"NO");}return true;}}class ў:ƛ{public ў(
){ɏ=1;ɒ="CmdShipMass";}public override bool Ɛ(bool ð){bool ѝ=Ɠ.ˬ.EndsWith("base");double ʚ=0;if(Ɠ.ˤ!="")double.TryParse(Ɠ
.ˤ.Trim(),out ʚ);int ќ=Ɠ.ˏ.Count;if(ќ>0){string џ=Ɠ.ˏ[0].œ.Trim();char ј=' ';if(џ.Length>0)ј=Char.ToLower(џ[0]);int ћ=
"kmgtpezy".IndexOf(ј);if(ћ>=0)ʚ*=Math.Pow(1000.0,ћ);}double ɹ=(ѝ?Z.Ǉ.ɷ:Z.Ǉ.ɸ);if(!ѝ)Z.Ì(ĝ.ǲ("SM1")+" ");else Z.Ì(ĝ.ǲ("SM2")+" ");Z
.ƴ(Z.Ȏ(ɹ,true,'k')+" ");if(ʚ>0)Z.Ʒ(ɹ/ʚ*100);return true;}}class њ:ƛ{public њ(){ɏ=0.5;ɒ="CmdSpeed";}public override bool Ɛ
(bool ð){double ʚ=0;double љ=1;string ј="m/s";if(Ɠ.ˬ.Contains("kmh")){љ=3.6;ј="km/h";}else if(Ɠ.ˬ.Contains("mph")){љ=
2.23694;ј="mph";}if(Ɠ.ˤ!="")double.TryParse(Ɠ.ˤ.Trim(),out ʚ);Z.Ì(ĝ.ǲ("S1")+" ");Z.ƴ((Z.Ǉ.ʁ*љ).ToString("F1")+" "+ј+" ");if(ʚ>0
)Z.Ʒ(Z.Ǉ.ʁ/ʚ*100);return true;}}class ї:ƛ{public ї(){ɏ=1;ɒ="CmdStopTask";}public override bool Ɛ(bool ð){double ѡ=0;if(Ɠ.
ˬ.Contains("best"))ѡ=Z.Ǉ.ʁ/Z.Ǉ.ʈ;else ѡ=Z.Ǉ.ʁ/Z.Ǉ.ɺ;double ѥ=Z.Ǉ.ʁ/2*ѡ;if(Ɠ.ˬ.Contains("time")){Z.Ì(ĝ.ǲ("ST"));if(double.
IsNaN(ѡ)){Z.ƴ("N/A");return true;}string Ĳ="";try{TimeSpan ͽ=TimeSpan.FromSeconds(ѡ);if((int)ͽ.TotalDays>0)Ĳ=" > 24h";else{if
(ͽ.Hours>0)Ĳ=ͽ.Hours+"h ";if(ͽ.Minutes>0||Ĳ!="")Ĳ+=ͽ.Minutes+"m ";Ĳ+=ͽ.Seconds+"s";}}catch{Ĳ="N/A";}Z.ƴ(Ĳ);return true;}Z
.Ì(ĝ.ǲ("SD"));if(!double.IsNaN(ѥ)&&!double.IsInfinity(ѥ))Z.ƴ(Z.ȏ(ѥ)+"m ");else Z.ƴ("N/A");return true;}}class ѩ:ƛ{ȷ ǈ;ο Ō
;public ѩ(){ɏ=2;ɒ="CmdTanks";}public override void ɯ(){ǈ=Z.ǈ;Ō=new ο(ƪ,Z.Ŋ);}int ē=0;char Ы='n';string Ѩ;double ѧ=0;
double Ѧ=0;double ƽ;bool Σ=false;public override bool Ɛ(bool ð){List<ʬ>ˏ=Ɠ.ˏ;if(ˏ.Count==0){Z.Ǖ(ĝ.ǲ("T4"));return true;}if(!ð)
{Ы=(Ɠ.ˬ.EndsWith("x")?'s':(Ɠ.ˬ.EndsWith("p")?'p':(Ɠ.ˬ.EndsWith("v")?'v':'n')));Σ=Ɠ.ˬ.EndsWith("bar");ē=0;if(Ѩ==null){Ѩ=ˏ[
0].œ.Trim();Ѩ=char.ToUpper(Ѩ[0])+Ѩ.Substring(1).ToLower();}Ō.ŭ();ѧ=0;Ѧ=0;}if(ē==0){if(!Ō.Ͼ("oxytank",Ɠ.ˤ,ð))return false;
ð=false;ē++;}if(ē==1){if(!Ō.Ͼ("hydrogenengine",Ɠ.ˤ,ð))return false;ð=false;ē++;}if(ē==2){if(!ǈ.ȑ(Ō.ρ,Ѩ,ref ѧ,ref Ѧ,ð))
return false;ð=false;ē++;}if(Ѧ==0){Z.Ǖ(String.Format(ĝ.ǲ("T5"),Ѩ));return true;}ƽ=ѧ/Ѧ*100;if(Σ){Z.Ʒ(ƽ);return true;}Z.Ì(Ѩ);
switch(Ы){case's':Z.ƴ(' '+Z.Ȇ(ƽ)+"%");break;case'v':Z.ƴ(Z.ȏ(ѧ)+"L / "+Z.ȏ(Ѧ)+"L");break;case'p':Z.ƴ(' '+Z.Ȇ(ƽ)+"%");Z.Ʒ(ƽ);
break;default:Z.ƴ(Z.ȏ(ѧ)+"L / "+Z.ȏ(Ѧ)+"L");Z.ƾ(ƽ,1.0f,Z.ƶ);Z.ƴ(' '+ƽ.ToString("0.0")+"%");break;}return true;}}class Ѥ{ɫ Z=
null;public string L="Debug";public float ѣ=1.0f;public List<ʏ>Ŧ=new List<ʏ>();public int ű=0;public float Ѣ=0;public Ѥ(ɫ V)
{Z=V;Ŧ.Add(new ʏ());}public void Ǒ(string Ĳ){Ŧ[ű].ɰ(Ĳ);}public void Ǒ(ʏ ť){Ŧ[ű].ɰ(ť);}public void Ť(){Ŧ.Add(new ʏ());ű++;
Ѣ=0;}public void Ť(string ţ){Ŧ[ű].ɰ(ţ);Ť();}public void Ţ(List<ʏ>š){if(Ŧ[ű].ʍ==0)Ŧ.RemoveAt(ű);else ű++;Ŧ.AddList(š);ű+=š
.Count-1;Ť();}public List<ʏ>Ś(){if(Ŧ[ű].ʍ==0)return Ŧ.GetRange(0,ű);else return Ŧ;}public void Š(string ş,string G=""){
string[]Ŧ=ş.Split('\n');for(int Y=0;Y<Ŧ.Length;Y++)Ť(G+Ŧ[Y]);}public void ŝ(){Ŧ.Clear();Ť();ű=0;}public int Ŝ(){return ű+(Ŧ[ű]
.ʍ>0?1:0);}public string ś(){return String.Join("\n",Ŧ);}public void Ś(List<ʏ>ř,int ĺ,int Ř){int ŗ=ĺ+Ř;int ļ=Ŝ();if(ŗ>ļ)ŗ
=ļ;for(int Y=ĺ;Y<ŗ;Y++)ř.Add(Ŧ[Y]);}}class Ş{ɫ Z=null;public float ŧ=1.0f;public int ŷ=17;public int ŵ=0;int Ŵ=1;int ų=1;
public List<Ѥ>Ų=new List<Ѥ>();public int ű=0;public Ş(ɫ V){Z=V;}public void Ű(int ķ){ų=ķ;}public void ů(){ŷ=(int)Math.Floor(ɫ.
ɨ*ŧ*ų/ɫ.ɦ);}public void Ů(Ѥ Ĳ){Ų.Add(Ĳ);}public void ŭ(){Ų.Clear();}public int Ŝ(){int ķ=0;foreach(var Ĳ in Ų){ķ+=Ĳ.Ŝ();}
return ķ;}ʏ Ŭ=new ʏ(256);public ʏ ś(){Ŭ.ŭ();int ķ=Ų.Count;for(int Y=0;Y<ķ-1;Y++){Ŭ.ɰ(Ų[Y].ś());Ŭ.ɰ("\n");}if(ķ>0)Ŭ.ɰ(Ų[ķ-1].ś(
));return Ŭ;}List<ʏ>ū=new List<ʏ>(20);public ʏ Ū(int ũ=0){Ŭ.ŭ();ū.Clear();if(ų<=0)return Ŭ;int Ũ=Ų.Count;int Ŗ=0;int Ł=(ŷ
/ų);int İ=(ũ*Ł);int Ŀ=ŵ+İ;int ľ=Ŀ+Ł;bool Ľ=false;for(int Y=0;Y<Ũ;Y++){Ѥ Ĳ=Ų[Y];int ļ=Ĳ.Ŝ();int Ļ=Ŗ;Ŗ+=ļ;if(!Ľ&&Ŗ>Ŀ){int ĺ
=Ŀ-Ļ;if(Ŗ>=ľ){Ĳ.Ś(ū,ĺ,ľ-Ļ-ĺ);break;}Ľ=true;Ĳ.Ś(ū,ĺ,ļ);continue;}if(Ľ){if(Ŗ>=ľ){Ĳ.Ś(ū,0,ľ-Ļ);break;}Ĳ.Ś(ū,0,ļ);}}int ķ=ū.
Count;for(int Y=0;Y<ķ-1;Y++){Ŭ.ɰ(ū[Y]);Ŭ.ɰ("\n");}if(ķ>0)Ŭ.ɰ(ū[ķ-1]);return Ŭ;}public bool ŀ(int ķ=-1){if(ķ<=0)ķ=Z.ɮ;if(ŵ-ķ<=
0){ŵ=0;return true;}ŵ-=ķ;return false;}public bool ĸ(int ķ=-1){if(ķ<=0)ķ=Z.ɮ;int Ķ=Ŝ();if(ŵ+ķ+ŷ>=Ķ){ŵ=Math.Max(Ķ-ŷ,0);
return true;}ŵ+=ķ;return false;}public int ĵ=0;public void Ĵ(){if(ĵ>0){ĵ--;return;}if(Ŝ()<=ŷ){ŵ=0;Ŵ=1;return;}if(Ŵ>0){if(ĸ()){
Ŵ=-1;ĵ=2;}}else{if(ŀ()){Ŵ=1;ĵ=2;}}}}class ĳ:ƛ{public ĳ(){ɏ=1;ɒ="CmdTextLCD";}public override bool Ɛ(bool ð){string Ĳ="";
if(Ɠ.ˤ!=""&&Ɠ.ˤ!="*"){IMyTextPanel ı=Z.ǆ.GetBlockWithName(Ɠ.ˤ)as IMyTextPanel;if(ı==null){Z.Ǖ("TextLCD: "+ĝ.ǲ("T1")+Ɠ.ˤ);
return true;}Ĳ=ı.GetText();}else{Z.Ǖ("TextLCD:"+ĝ.ǲ("T2"));return true;}if(Ĳ.Length==0)return true;Z.ǔ(Ĳ);return true;}}class
Ĺ:ƛ{public Ĺ(){ɏ=5;ɒ="CmdWorking";}ο Ō;public override void ɯ(){Ō=new ο(ƪ,Z.Ŋ);}int ē=0;int ŕ=0;bool Ŕ;public override
bool Ɛ(bool ð){if(!ð){ē=0;Ŕ=(Ɠ.ˬ=="workingx");ŕ=0;}if(Ɠ.ˏ.Count==0){if(ē==0){if(!ð)Ō.ŭ();if(!Ō.ϣ(Ɠ.ˤ,ð))return false;ē++;ð=
false;}if(!Ƙ(Ō,Ŕ,"",ð))return false;return true;}for(;ŕ<Ɠ.ˏ.Count;ŕ++){ʬ œ=Ɠ.ˏ[ŕ];if(!ð)œ.ʞ();if(!ō(œ,ð))return false;ð=false
;}return true;}int Œ=0;int ő=0;string[]Ő;string ŏ;string Ŏ;bool ō(ʬ œ,bool ð){if(!ð){Œ=0;ő=0;}for(;ő<œ.ʠ.Count;ő++){if(Œ
==0){if(!ð){if(string.IsNullOrEmpty(œ.ʠ[ő]))continue;Ō.ŭ();Ő=œ.ʠ[ő].Split(':');ŏ=Ő[0];Ŏ=(Ő.Length>1?Ő[1]:"");}if(!string.
IsNullOrEmpty(ŏ)){if(!Ō.Ͼ(ŏ,Ɠ.ˤ,ð))return false;}else{if(!Ō.ϣ(Ɠ.ˤ,ð))return false;}Œ++;ð=false;}if(!Ƙ(Ō,Ŕ,Ŏ,ð))return false;Œ=0;ð=
false;}return true;}string ŋ(IMyTerminalBlock Ý){В Ŋ=Z.Ŋ;if(!Ý.IsWorking)return ĝ.ǲ("W1");IMyProductionBlock ŉ=Ý as
IMyProductionBlock;if(ŉ!=null)if(ŉ.IsProducing)return ĝ.ǲ("W2");else return ĝ.ǲ("W3");IMyAirVent ň=Ý as IMyAirVent;if(ň!=null){if(ň.
CanPressurize)return(ň.GetOxygenLevel()*100).ToString("F1")+"%";else return ĝ.ǲ("W4");}IMyGasTank Ň=Ý as IMyGasTank;if(Ň!=null)return
(Ň.FilledRatio*100).ToString("F1")+"%";IMyBatteryBlock ņ=Ý as IMyBatteryBlock;if(ņ!=null)return Ŋ.Ϫ(ņ);IMyJumpDrive Ņ=Ý
as IMyJumpDrive;if(Ņ!=null)return Ŋ.ϥ(Ņ).ToString("0.0")+"%";IMyLandingGear ń=Ý as IMyLandingGear;if(ń!=null){switch((int)
ń.LockMode){case 0:return ĝ.ǲ("W8");case 1:return ĝ.ǲ("W10");case 2:return ĝ.ǲ("W7");}}IMyDoor Ń=Ý as IMyDoor;if(Ń!=null)
{if(Ń.Status==DoorStatus.Open)return ĝ.ǲ("W5");return ĝ.ǲ("W6");}IMyShipConnector Ŷ=Ý as IMyShipConnector;if(Ŷ!=null){if(
Ŷ.Status==MyShipConnectorStatus.Unconnected)return ĝ.ǲ("W8");if(Ŷ.Status==MyShipConnectorStatus.Connected)return ĝ.ǲ("W7"
);else return ĝ.ǲ("W10");}IMyLaserAntenna Ÿ=Ý as IMyLaserAntenna;if(Ÿ!=null)return Ŋ.ϧ(Ÿ);IMyRadioAntenna Ʃ=Ý as
IMyRadioAntenna;if(Ʃ!=null)return Z.ȏ(Ʃ.Radius)+"m";IMyBeacon Ɯ=Ý as IMyBeacon;if(Ɯ!=null)return Z.ȏ(Ɯ.Radius)+"m";IMyThrust ƚ=Ý as
IMyThrust;if(ƚ!=null&&ƚ.ThrustOverride>0)return Z.ȏ(ƚ.ThrustOverride)+"N";return ĝ.ǲ("W9");}int ƙ=0;bool Ƙ(ο ł,bool Ɨ,string Ɩ,
bool ð){if(!ð)ƙ=0;for(;ƙ<ł.Г();ƙ++){if(!ƪ.ʔ(20))return false;IMyTerminalBlock Ý=ł.ρ[ƙ];string ƕ=(Ɨ?(Ý.IsWorking?ĝ.ǲ("W9"):ĝ.
ǲ("W1")):ŋ(Ý));if(!string.IsNullOrEmpty(Ɩ)&&String.Compare(ƕ,Ɩ,true)!=0)continue;if(Ɨ)ƕ=ŋ(Ý);string Ɣ=Ý.CustomName;Ɣ=Z.ǥ(
Ɣ,Z.ɝ*0.7f);Z.Ì(Ɣ);Z.ƴ(ƕ);}return true;}}class ƛ:ɓ{public Ѥ Ĳ=null;protected ˮ Ɠ;protected ɫ Z;protected Đ Ĝ;protected Ǫ
ĝ;public ƛ(){ɏ=3600;ɒ="CommandTask";}public void ƒ(Đ Ě,ˮ Ƒ){Ĝ=Ě;Z=Ĝ.Z;Ɠ=Ƒ;ĝ=Z.ĝ;}public virtual bool Ɛ(bool ð){Z.Ǖ(ĝ.ǲ(
"UC")+": '"+Ɠ.ˣ+"'");return true;}public override bool ɭ(bool ð){Ĳ=Z.ǘ(Ĳ,Ĝ.v);if(!ð)Z.ŝ();return Ɛ(ð);}}class Ə{Dictionary<
string,string>Ɲ=new Dictionary<string,string>(StringComparer.InvariantCultureIgnoreCase){{"ingot","ingot"},{"ore","ore"},{
"component","component"},{"tool","physicalgunobject"},{"ammo","ammomagazine"},{"oxygen","oxygencontainerobject"},{"gas",
"gascontainerobject"}};ș ƪ;ɫ Z;ƅ ƨ;ƅ Ƨ;ƅ Ʀ;Ð ƥ;bool Ƥ;public ƅ ƣ;public Ə(ș Ƣ,ɫ V,int B=20){ƨ=new ƅ();Ƨ=new ƅ();Ʀ=new ƅ();Ƥ=false;ƣ=new ƅ();
ƪ=Ƣ;Z=V;ƥ=Z.ƥ;}public void ŭ(){Ʀ.b();Ƨ.b();ƨ.b();Ƥ=false;ƣ.b();}public void ơ(string Ơ,bool Ƌ=false,int ƍ=1,int ƌ=-1){if(
string.IsNullOrEmpty(Ơ)){Ƥ=true;return;}string[]Ɵ=Ơ.Split(' ');string Ä="";Ǝ ſ=new Ǝ(Ƌ,ƍ,ƌ);if(Ɵ.Length==2){if(!Ɲ.TryGetValue(
Ɵ[1],out Ä))Ä=Ɵ[1];}string Å=Ɵ[0];if(Ɲ.TryGetValue(Å,out ſ.Ä)){Ƨ.z(ſ.Ä,ſ);return;}Z.ǻ(ref Å,ref Ä);if(string.
IsNullOrEmpty(Ä)){ſ.Å=Å;ƨ.z(ſ.Å,ſ);return;}ſ.Å=Å;ſ.Ä=Ä;Ʀ.z(Å+' '+Ä,ſ);}public Ǝ ƞ(string Ç,string Å,string Ä){Ǝ ſ;ſ=Ʀ.k(Ç);if(ſ!=null
)return ſ;ſ=ƨ.k(Å);if(ſ!=null)return ſ;ſ=Ƨ.k(Ä);if(ſ!=null)return ſ;return null;}public bool Ź(string Ç,string Å,string Ä
){Ǝ ſ;bool Ƃ=false;ſ=Ƨ.k(Ä);if(ſ!=null){if(ſ.Ƌ)return true;Ƃ=true;}ſ=ƨ.k(Å);if(ſ!=null){if(ſ.Ƌ)return true;Ƃ=true;}ſ=Ʀ.k(
Ç);if(ſ!=null){if(ſ.Ƌ)return true;Ƃ=true;}return!(Ƥ||Ƃ);}public Ǝ Ɓ(string Ç,string Å,string Ä){Ǝ ž=new Ǝ();Ǝ ſ=ƞ(Ç,Å,Ä);
if(ſ!=null){ž.ƍ=ſ.ƍ;ž.ƌ=ſ.ƌ;}ž.Å=Å;ž.Ä=Ä;ƣ.z(Ç,ž);return ž;}public Ǝ ƀ(string Ç,string Å,string Ä){Ǝ ž=ƣ.k(Ç);if(ž==null)ž
=Ɓ(Ç,Å,Ä);return ž;}int Ž=0;List<Ǝ>ż;public List<Ǝ>Ż(string Ä,bool ð,Func<Ǝ,bool>ź=null){if(!ð){ż=new List<Ǝ>();Ž=0;}for(
;Ž<ƣ.n();Ž++){if(!ƪ.ʔ(5))return null;Ǝ ſ=ƣ.f(Ž);if(Ź(ſ.Å+' '+ſ.Ä,ſ.Å,ſ.Ä))continue;if((string.Compare(ſ.Ä,Ä,true)==0)&&(ź
==null||ź(ſ)))ż.Add(ſ);}return ż;}int ƃ=0;public bool ƈ(bool ð){if(!ð){ƃ=0;}for(;ƃ<ƥ.ª.Count;ƃ++){if(!ƪ.ʔ(10))return false
;Ã o=ƥ.Ï[ƥ.ª[ƃ]];if(!o.È)continue;string Ç=o.É+' '+o.Ò;if(Ź(Ç,o.É,o.Ò))continue;Ǝ ž=ƀ(Ç,o.É,o.Ò);if(ž.ƌ==-1)ž.ƌ=o.ß;}
return true;}}class Ǝ{public int ƍ;public int ƌ;public string Å="";public string Ä="";public bool Ƌ;public double Ɗ;public Ǝ(
bool Ɖ=false,int Ƈ=1,int Ɔ=-1){ƍ=Ƈ;Ƌ=Ɖ;ƌ=Ɔ;}}class ƅ{Dictionary<string,Ǝ>Ƅ=new Dictionary<string,Ǝ>(StringComparer.
InvariantCultureIgnoreCase);List<string>ª=new List<string>();public void z(string j,Ǝ o){if(!Ƅ.ContainsKey(j)){ª.Add(j);Ƅ.Add(j,o);}}public int n(
){return Ƅ.Count;}public Ǝ k(string j){if(Ƅ.ContainsKey(j))return Ƅ[j];return null;}public Ǝ f(int e){return Ƅ[ª[e]];}
public void b(){ª.Clear();Ƅ.Clear();}public void a(){ª.Sort();}}class Ð{public Dictionary<string,Ã>Ï=new Dictionary<string,Ã>(
StringComparer.InvariantCultureIgnoreCase);Dictionary<string,Ã>Î=new Dictionary<string,Ã>(StringComparer.InvariantCultureIgnoreCase);
public List<string>ª=new List<string>();public Dictionary<string,Ã>Í=new Dictionary<string,Ã>(StringComparer.
InvariantCultureIgnoreCase);public void Ì(string Å,string Ä,int Ë,string Ñ,string Ê,bool È){if(Ä=="Ammo")Ä="AmmoMagazine";else if(Ä=="Tool")Ä=
"PhysicalGunObject";string Ç=Å+' '+Ä;Ã o=new Ã(Å,Ä,Ë,Ñ,Ê,È);Ï.Add(Ç,o);if(!Î.ContainsKey(Å))Î.Add(Å,o);if(Ê!="")Í.Add(Ê,o);ª.Add(Ç);}public
Ã Æ(string Å="",string Ä=""){if(Ï.ContainsKey(Å+" "+Ä))return Ï[Å+" "+Ä];if(string.IsNullOrEmpty(Ä)){Ã o=null;Î.
TryGetValue(Å,out o);return o;}if(string.IsNullOrEmpty(Å))for(int Y=0;Y<Ï.Count;Y++){Ã o=Ï[ª[Y]];if(string.Compare(Ä,o.Ò,true)==0)
return o;}return null;}}class Ã{public string É;public string Ò;public int ß;public string é;public string è;public bool È;
public Ã(string ç,string æ,int å=0,string ä="",string ã="",bool â=true){É=ç;Ò=æ;ß=å;é=ä;è=ã;È=â;}}class á{ɫ Z=null;public À à=
new À();public Ş Þ;public IMyTerminalBlock Ý;public IMyTextSurface Ü;public int Û=0;public int Ú=0;public string Ù="";
public string Ø="";public bool Ö=true;public IMyTextSurface Õ=>(Ó?Ü:Ý as IMyTextSurface);public int Ô=>(Ó?(Z.Ǚ(Ý)?0:1):à.n());
public bool Ó=false;public á(ɫ V,string A){Z=V;Ø=A;}public á(ɫ V,string A,IMyTerminalBlock U,IMyTextSurface C,int S){Z=V;Ø=A;Ý
=U;Ü=C;Û=S;Ó=true;}public bool R(){return Þ.Ŝ()>Þ.ŷ||Þ.ŵ!=0;}float Q=1.0f;bool P=false;public float O(){if(P)return Q;P=
true;if(Ý.BlockDefinition.SubtypeId.Contains("PanelWide")){if(Õ.SurfaceSize.X<Õ.SurfaceSize.Y)Q=2.0f;}return Q;}float W=1.0f
;bool N=false;public float K(){if(N)return W;N=true;if(Ý.BlockDefinition.SubtypeId.Contains("PanelWide")){if(Õ.
SurfaceSize.X<Õ.SurfaceSize.Y)W=2.0f;}return W;}bool J=false;public void I(){if(J)return;if(!Ó){à.a();Ý=à.f(0);}int H=Ý.CustomName.
IndexOf("!MARGIN:");if(H<0||H+8>=Ý.CustomName.Length){Ú=1;Ù=" ";}else{string G=Ý.CustomName.Substring(H+8);int F=G.IndexOf(" ")
;if(F>=0)G=G.Substring(0,F);if(!int.TryParse(G,out Ú))Ú=1;Ù=new String(' ',Ú);}if(Ý.CustomName.Contains("!NOSCROLL"))Ö=
false;else Ö=true;J=true;}public void E(Ş D=null){if(Þ==null||Ý==null)return;if(D==null)D=Þ;if(!Ó){IMyTextSurface C=Ý as
IMyTextSurface;if(C!=null){float B=C.FontSize;string L=C.Font;for(int Y=0;Y<à.n();Y++){IMyTextSurface v=à.f(Y)as IMyTextSurface;if(v==
null)continue;v.Alignment=VRage.Game.GUI.TextPanel.TextAlignment.LEFT;v.FontSize=B;v.Font=L;string Â=D.Ū(Y).ɕ();if(!Z.ǐ.
SKIP_CONTENT_TYPE)v.ContentType=VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;v.WriteText(Â);}}}else{Ü.Alignment=VRage.Game.GUI.
TextPanel.TextAlignment.LEFT;if(!Z.ǐ.SKIP_CONTENT_TYPE)Ü.ContentType=VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;Ü.
WriteText(D.Ū().ɕ());}J=false;}public void Á(){if(Ý==null)return;if(Ó){Ü.WriteText("");return;}IMyTextSurface C=Ý as
IMyTextSurface;if(C==null)return;for(int Y=0;Y<à.n();Y++){IMyTextSurface v=à.f(Y)as IMyTextSurface;if(v==null)continue;v.WriteText("")
;}}}class À{Dictionary<string,IMyTerminalBlock>º=new Dictionary<string,IMyTerminalBlock>();Dictionary<IMyTerminalBlock,
string>µ=new Dictionary<IMyTerminalBlock,string>();List<string>ª=new List<string>();public void z(string j,IMyTerminalBlock o)
{if(!ª.Contains(j)){ª.Add(j);º.Add(j,o);µ.Add(o,j);}}public void w(string j){if(ª.Contains(j)){ª.Remove(j);µ.Remove(º[j])
;º.Remove(j);}}public void r(IMyTerminalBlock o){if(µ.ContainsKey(o)){ª.Remove(µ[o]);º.Remove(µ[o]);µ.Remove(o);}}public
int n(){return º.Count;}public IMyTerminalBlock k(string j){if(ª.Contains(j))return º[j];return null;}public
IMyTerminalBlock f(int e){return º[ª[e]];}public void b(){ª.Clear();º.Clear();µ.Clear();}public void a(){ª.Sort();}}class X:ɓ{public ɫ Z
;public á ê;Đ Ĝ;public X(Đ Ě){Ĝ=Ě;Z=Ĝ.Z;ê=Ĝ.v;ɏ=0.5;ɒ="PanelDisplay";}double ę=0;public void Ę(){ę=0;}int ė=0;int Ė=0;
bool ĕ=true;double Ĕ=double.MaxValue;int ē=0;public override bool ɭ(bool ð){ƛ ě;if(!ð&&(Ĝ.Ċ==false||Ĝ.Č==null||Ĝ.Č.Count<=0)
)return true;if(Ĝ.ċ.û>3)return ɉ(0);if(!ð){Ė=0;ĕ=false;Ĕ=double.MaxValue;ē=0;}if(ē==0){while(Ė<Ĝ.Č.Count){if(!ƪ.ʔ(5))
return false;if(Ĝ.č.TryGetValue(Ĝ.Č[Ė],out ě)){if(!ě.Ɍ)return ɉ(ě.ɗ-ƪ.ȕ+0.001);if(ě.ɑ>ę)ĕ=true;if(ě.ɗ<Ĕ)Ĕ=ě.ɗ;}Ė++;}ē++;ð=
false;}double Ē=Ĕ-ƪ.ȕ+0.001;if(!ĕ&&!ê.R())return ɉ(Ē);Z.ǖ(ê.Þ,ê);if(ĕ){if(!ð){ę=ƪ.ȕ;ê.Þ.ŭ();ė=0;}while(ė<Ĝ.Č.Count){if(!ƪ.ʔ(7
))return false;if(!Ĝ.č.TryGetValue(Ĝ.Č[ė],out ě)){ê.Þ.Ų.Add(Z.ǘ(null,ê));Z.ŝ();Z.Ǖ("ERR: No cmd task ("+Ĝ.Č[ė]+")");ė++;
continue;}ê.Þ.Ů(ě.Ĳ);ė++;}}Z.ȁ(ê);Ĝ.ċ.û++;if(ɏ<Ē&&!ê.R())return ɉ(Ē);return true;}}class Đ:ɓ{public ɫ Z;public á v;public X ď=
null;string Ď="N/A";public Dictionary<string,ƛ>č=new Dictionary<string,ƛ>();public List<string>Č=null;public ü ċ;public bool
Ċ{get{return ċ.õ;}}public Đ(ü đ,á ĉ){ɏ=5;v=ĉ;ċ=đ;Z=đ.Z;ɒ="PanelProcess";}Ǫ ĝ;public override void ɯ(){ĝ=Z.ĝ;}ˮ į=null;ƛ Į
(string ĭ,bool ð){if(!ð)į=new ˮ(ƪ);if(!į.ʞ(ĭ,ð))return null;ƛ Ģ=į.ː();Ģ.ƒ(this,į);ƪ.ȱ(Ģ,0);return Ģ;}string Ĭ="";void ī()
{try{Ĭ=v.Ý.ǰ(v.Û,Z.ɥ);}catch{Ĭ="";return;}Ĭ=Ĭ?.Replace("\\\n","");}int ė=0;int Ī=0;List<string>ĩ=null;HashSet<string>Ĩ=
new HashSet<string>();int ħ=0;bool Ħ(bool ð){if(!ð){char[]ĥ={';','\n'};string Ĥ=Ĭ.Replace("\\;","\f");if(Ĥ.StartsWith("@"))
{int ģ=Ĥ.IndexOf("\n");if(ģ<0){Ĥ="";}else{Ĥ=Ĥ.Substring(ģ+1);}}ĩ=new List<string>(Ĥ.Split(ĥ,StringSplitOptions.
RemoveEmptyEntries));Ĩ.Clear();ė=0;Ī=0;ħ=0;}while(ė<ĩ.Count){if(!ƪ.ʔ(500))return false;if(ĩ[ė].StartsWith("//")){ĩ.RemoveAt(ė);continue;}ĩ
[ė]=ĩ[ė].Replace('\f',';');if(!č.ContainsKey(ĩ[ė])){if(ħ!=1)ð=false;ħ=1;ƛ ě=Į(ĩ[ė],ð);if(ě==null)return false;ð=false;č.
Add(ĩ[ė],ě);ħ=0;}if(!Ĩ.Contains(ĩ[ė]))Ĩ.Add(ĩ[ė]);ė++;}if(Č!=null){ƛ Ģ;while(Ī<Č.Count){if(!ƪ.ʔ(7))return false;if(!Ĩ.
Contains(Č[Ī]))if(č.TryGetValue(Č[Ī],out Ģ)){Ģ.ɣ();č.Remove(Č[Ī]);}Ī++;}}Č=ĩ;return true;}public override void ɬ(){if(Č!=null){ƛ
Ģ;for(int ġ=0;ġ<Č.Count;ġ++){if(č.TryGetValue(Č[ġ],out Ģ))Ģ.ɣ();}Č=null;}if(ď!=null){ď.ɣ();ď=null;}else{}}Ş Ġ=null;string
ğ="";bool Ğ=false;public override bool ɭ(bool ð){if(v.Ô<=0){ɣ();return true;}if(!ð){v.Þ=Z.ǖ(v.Þ,v);Ġ=Z.ǖ(Ġ,v);ī();if(Ĭ==
null){if(v.Ó){ċ.í(v.Ü,v.Ý as IMyTextPanel);}else{ɣ();}return true;}if(v.Ý.CustomName!=ğ){Ğ=true;}else{Ğ=false;}ğ=v.Ý.
CustomName;}if(Ĭ!=Ď){if(!Ħ(ð))return false;if(Ĭ==""){Ď="";if(ċ.õ){if(Ġ.Ų.Count<=0)Ġ.Ų.Add(Z.ǘ(null,v));else Z.ǘ(Ġ.Ų[0],v);Z.ŝ();Z.
Ǖ(ĝ.ǲ("H1"));bool Ĉ=v.Ö;v.Ö=false;Z.ȁ(v,Ġ);v.Ö=Ĉ;return true;}return this.ɉ(2);}Ğ=true;}Ď=Ĭ;if(ď!=null&&Ğ){ƪ.Ȯ(ď);ď.Ę();ƪ
.ȱ(ď,0);}else if(ď==null){ď=new X(this);ƪ.ȱ(ď,0);}return true;}}class ü:ɓ{const string ë="T:!LCD!";public int û=0;public
ɫ Z;public ȹ à=new ȹ();ο ú;ο ù;Dictionary<á,Đ>ø=new Dictionary<á,Đ>();public Dictionary<IMyTextSurface,á>ö=new Dictionary
<IMyTextSurface,á>();public bool õ=false;ϱ ô=null;public ü(ɫ V){ɏ=5;Z=V;ɒ="ProcessPanels";}public override void ɯ(){ú=new
ο(ƪ,Z.Ŋ);ù=new ο(ƪ,Z.Ŋ);ô=new ϱ(Z,this);}int ó=0;bool ñ(bool ð){if(!ð)ó=0;if(ó==0){if(!ú.ϣ(Z.ɥ,ð))return false;ó++;ð=
false;}if(ó==1){if(Z.ɥ=="T:[LCD]"&&ë!="")if(!ú.ϣ(ë,ð))return false;ó++;ð=false;}return true;}string ï(IMyTerminalBlock Ý){int
î=Ý.CustomName.IndexOf("!LINK:");if(î>=0&&Ý.CustomName.Length>î+6){return Ý.CustomName.Substring(î+6)+' '+Ý.Position.
ToString();}return Ý.EntityId.ToString();}public void í(IMyTextSurface C,IMyTextPanel v){á ê;if(C==null)return;if(!ö.TryGetValue
(C,out ê))return;if(v!=null){ê.à.r(v);}ö.Remove(C);if(ê.Ô<=0||ê.Ó){Đ ì;if(ø.TryGetValue(ê,out ì)){à.r(ê.Ø);ø.Remove(ê);ì.
ɣ();}}}void ò(IMyTerminalBlock Ý){IMyTextSurfaceProvider ă=Ý as IMyTextSurfaceProvider;IMyTextSurface C=Ý as
IMyTextSurface;if(C!=null){í(C,Ý as IMyTextPanel);return;}if(ă==null)return;for(int Y=0;Y<ă.SurfaceCount;Y++){C=ă.GetSurface(Y);í(C,
null);}}string A;string ć;bool Ć;int ą=0;int Ą=0;public override bool ɭ(bool ð){if(!ð){ú.ŭ();ą=0;Ą=0;}if(!ñ(ð))return false;
while(ą<ú.Г()){if(!ƪ.ʔ(20))return false;IMyTerminalBlock Ý=(ú.ρ[ą]as IMyTerminalBlock);if(Ý==null||!Ý.IsWorking){ú.ρ.RemoveAt
(ą);continue;}IMyTextSurfaceProvider ă=Ý as IMyTextSurfaceProvider;IMyTextSurface C=Ý as IMyTextSurface;IMyTextPanel v=Ý
as IMyTextPanel;á ê;A=ï(Ý);string[]Ă=A.Split(' ');ć=Ă[0];Ć=Ă.Length>1;if(v!=null){if(ö.ContainsKey(C)){ê=ö[C];if(ê.Ø==A+
"@0"||(Ć&&ê.Ø==ć)){ą++;continue;}ò(Ý);}if(!Ć){ê=new á(Z,A+"@0",Ý,C,0);Đ ì=new Đ(this,ê);ƪ.ȱ(ì,0);ø.Add(ê,ì);à.z(ê.Ø,ê);ö.Add
(C,ê);ą++;continue;}ê=à.k(ć);if(ê==null){ê=new á(Z,ć);à.z(ć,ê);Đ ì=new Đ(this,ê);ƪ.ȱ(ì,0);ø.Add(ê,ì);}ê.à.z(A,Ý);ö.Add(C,
ê);}else{if(ă==null){ą++;continue;}for(int Y=0;Y<ă.SurfaceCount;Y++){C=ă.GetSurface(Y);if(ö.ContainsKey(C)){ê=ö[C];if(ê.Ø
==A+'@'+Y.ToString()){continue;}í(C,null);}if(Ý.ǰ(Y,Z.ɥ)==null)continue;ê=new á(Z,A+"@"+Y.ToString(),Ý,C,Y);Đ ì=new Đ(this
,ê);ƪ.ȱ(ì,0);ø.Add(ê,ì);à.z(ê.Ø,ê);ö.Add(C,ê);}}ą++;}while(Ą<ù.Г()){if(!ƪ.ʔ(300))return false;IMyTerminalBlock Ý=ù.ρ[Ą];
if(Ý==null)continue;if(!ú.ρ.Contains(Ý)){ò(Ý);}Ą++;}ù.ŭ();ù.Е(ú);if(!ô.ɍ&&ô.ϴ())ƪ.ȱ(ô,0);return true;}public bool ā(string
Ā){if(string.Compare(Ā,"clear",true)==0){ô.Ϻ();if(!ô.ɍ)ƪ.ȱ(ô,0);return true;}if(string.Compare(Ā,"boot",true)==0){ô.ϻ=0;
if(!ô.ɍ)ƪ.ȱ(ô,0);return true;}if(Ā.Ƿ("scroll")){σ ÿ=new σ(Z,this,Ā);ƪ.ȱ(ÿ,0);return true;}if(string.Compare(Ā,"props",true
)==0){В þ=Z.Ŋ;List<IMyTerminalBlock>ł=new List<IMyTerminalBlock>();List<ITerminalAction>ý=new List<ITerminalAction>();
List<ITerminalProperty>ƫ=new List<ITerminalProperty>();IMyTextPanel ı=ƪ.ǐ.GridTerminalSystem.GetBlockWithName("DEBUG")as
IMyTextPanel;if(ı==null){return true;}ı.WriteText("Properties: ");foreach(var o in þ.Ў){ı.WriteText(o.Key+" =============="+"\n",
true);o.Value(ł,null);if(ł.Count<=0){ı.WriteText("No blocks\n",true);continue;}ł[0].GetProperties(ƫ,(ê)=>{return ê.Id!=
"Name"&&ê.Id!="OnOff"&&!ê.Id.StartsWith("Show");});foreach(var Ⱥ in ƫ){ı.WriteText("P "+Ⱥ.Id+" "+Ⱥ.TypeName+"\n",true);}ƫ.
Clear();ł.Clear();}}return false;}}class ȹ{Dictionary<string,á>Ƅ=new Dictionary<string,á>();List<string>ª=new List<string>();
public void z(string j,á o){if(!Ƅ.ContainsKey(j)){ª.Add(j);Ƅ.Add(j,o);}}public int n(){return Ƅ.Count;}public á k(string j){if
(Ƅ.ContainsKey(j))return Ƅ[j];return null;}public á f(int e){return Ƅ[ª[e]];}public void r(string j){Ƅ.Remove(j);ª.Remove
(j);}public void b(){ª.Clear();Ƅ.Clear();}public void a(){ª.Sort();}}class ȷ{ș ƪ;ɫ Z;public MyDefinitionId ȶ=new
MyDefinitionId(typeof(VRage.Game.ObjectBuilders.Definitions.MyObjectBuilder_GasProperties),"Electricity");public MyDefinitionId ȸ=new
MyDefinitionId(typeof(VRage.Game.ObjectBuilders.Definitions.MyObjectBuilder_GasProperties),"Oxygen");public MyDefinitionId ȵ=new
MyDefinitionId(typeof(VRage.Game.ObjectBuilders.Definitions.MyObjectBuilder_GasProperties),"Hydrogen");public ȷ(ș Ƣ,ɫ V){ƪ=Ƣ;Z=V;}int
Ɇ=0;public bool Ʌ(List<IMyTerminalBlock>ł,ref double Ⱦ,ref double Ƚ,ref double ȼ,ref double Ȼ,ref double Ʉ,ref double Ƀ,
bool ð){if(!ð)Ɇ=0;MyResourceSinkComponent Ȟ;MyResourceSourceComponent ȴ;for(;Ɇ<ł.Count;Ɇ++){if(!ƪ.ʔ(8))return false;if(ł[Ɇ].
Components.TryGet<MyResourceSinkComponent>(out Ȟ)){Ⱦ+=Ȟ.CurrentInputByType(ȶ);Ƚ+=Ȟ.MaxRequiredInputByType(ȶ);}if(ł[Ɇ].Components.
TryGet<MyResourceSourceComponent>(out ȴ)){ȼ+=ȴ.CurrentOutputByType(ȶ);Ȼ+=ȴ.MaxOutputByType(ȶ);}IMyBatteryBlock ɂ=(ł[Ɇ]as
IMyBatteryBlock);Ʉ+=ɂ.CurrentStoredPower;Ƀ+=ɂ.MaxStoredPower;}return true;}int Ɂ=0;public bool ɀ(List<IMyTerminalBlock>ł,MyDefinitionId
ȿ,ref double Ⱦ,ref double Ƚ,ref double ȼ,ref double Ȼ,bool ð){if(!ð)Ɂ=0;MyResourceSinkComponent Ȟ;
MyResourceSourceComponent ȴ;for(;Ɂ<ł.Count;Ɂ++){if(!ƪ.ʔ(6))return false;if(ł[Ɂ].Components.TryGet<MyResourceSinkComponent>(out Ȟ)){Ⱦ+=Ȟ.
CurrentInputByType(ȿ);Ƚ+=Ȟ.MaxRequiredInputByType(ȿ);}if(ł[Ɂ].Components.TryGet<MyResourceSourceComponent>(out ȴ)){ȼ+=ȴ.
CurrentOutputByType(ȿ);Ȼ+=ȴ.MaxOutputByType(ȿ);}}return true;}int ȣ=0;public bool ȑ(List<IMyTerminalBlock>ł,string ȡ,ref double Ƞ,ref
double ȟ,bool ð){if(!ð){ȣ=0;ȟ=0;Ƞ=0;}MyResourceSinkComponent Ȟ;for(;ȣ<ł.Count;ȣ++){if(!ƪ.ʔ(30))return false;IMyGasTank Ň=ł[ȣ]
as IMyGasTank;if(Ň==null)continue;double ȝ=0;if(Ň.Components.TryGet<MyResourceSinkComponent>(out Ȟ)){ListReader<
MyDefinitionId>Ȝ=Ȟ.AcceptedResources;int Y=0;for(;Y<Ȝ.Count;Y++){if(string.Compare(Ȝ[Y].SubtypeId.ToString(),ȡ,true)==0){ȝ=Ň.Capacity;
ȟ+=ȝ;Ƞ+=ȝ*Ň.FilledRatio;break;}}}}return true;}public string Ȣ(TimeSpan ț){string Ĳ="";if(ț.Ticks<=0)return"-";if((int)ț.
TotalDays>0)Ĳ+=(long)ț.TotalDays+" "+Z.ĝ.ǲ("C5")+" ";if(ț.Hours>0||Ĳ!="")Ĳ+=ț.Hours+"h ";if(ț.Minutes>0||Ĳ!="")Ĳ+=ț.Minutes+"m ";
return Ĳ+ț.Seconds+"s";}}class ș{public const double Ș=0.05;public const int ȗ=1000;public const int Ȗ=10000;public double ȕ{
get{return ȓ;}}int Ȕ=ȗ;double ȓ=0;List<ɓ>Ȓ=new List<ɓ>(100);public MyGridProgram ǐ;public bool Ț=false;int Ȥ=0;public ș(
MyGridProgram ǃ,int ǂ=1,bool Ȳ=false){ǐ=ǃ;Ȥ=ǂ;Ț=Ȳ;}public void ȱ(ɓ ì,double Ȱ,bool ȯ=false){ì.ɍ=true;ì.Ɋ(this);if(ȯ){ì.ɗ=ȕ;Ȓ.Insert(0
,ì);return;}if(Ȱ<=0)Ȱ=0.001;ì.ɗ=ȕ+Ȱ;for(int Y=0;Y<Ȓ.Count;Y++){if(Ȓ[Y].ɗ>ì.ɗ){Ȓ.Insert(Y,ì);return;}if(ì.ɗ-Ȓ[Y].ɗ<Ș)ì.ɗ=Ȓ
[Y].ɗ+Ș;}Ȓ.Add(ì);}public void Ȯ(ɓ ì){if(Ȓ.Contains(ì)){Ȓ.Remove(ì);ì.ɍ=false;}}public void ȭ(ʏ ȳ,int ȫ=1){if(Ȥ==ȫ)ǐ.Echo
(ȳ.ɕ());}public void ȭ(string Ȭ,int ȫ=1){if(Ȥ==ȫ)ǐ.Echo(Ȭ);}const double Ȫ=(16.66666666/16);double ȩ=0;public void Ȩ(){ȩ
+=ǐ.Runtime.TimeSinceLastRun.TotalSeconds*Ȫ;}ʏ Ƴ=new ʏ();public void ȧ(){double Ȧ=ǐ.Runtime.TimeSinceLastRun.TotalSeconds*
Ȫ+ȩ;ȩ=0;ȓ+=Ȧ;Ȕ=(int)Math.Min((Ȧ*60)*ȗ/(Ț?5:1),Ȗ-1000);while(Ȓ.Count>=1){ɓ ì=Ȓ[0];if(Ȕ-ǐ.Runtime.CurrentInstructionCount<=
0)break;if(ì.ɗ>ȓ){int ɇ=(int)(60*(ì.ɗ-ȓ));if(ɇ>=100){ǐ.Runtime.UpdateFrequency=UpdateFrequency.Update100;}else{if(ɇ>=10||
Ț)ǐ.Runtime.UpdateFrequency=UpdateFrequency.Update10;else ǐ.Runtime.UpdateFrequency=UpdateFrequency.Update1;}break;}Ȓ.
Remove(ì);if(!ì.ɘ())break;}}public int Ɉ(){return(Ȗ-ǐ.Runtime.CurrentInstructionCount);}public bool ʔ(int ʉ){return((Ȕ-ǐ.
Runtime.CurrentInstructionCount)>=ʉ);}public void ʇ(){ȭ(Ƴ.ŭ().ɰ("Remaining Instr: ").ɰ(Ɉ()));}}class ʆ:ɓ{MyShipVelocities ʅ;
public Vector3D ʄ{get{return ʅ.LinearVelocity;}}public Vector3D ʃ{get{return ʅ.AngularVelocity;}}double ʂ=0;public double ʁ{
get{if(ɲ!=null)return ɲ.GetShipSpeed();else return ʂ;}}double ʀ=0;public double ɿ{get{return ʀ;}}double ɾ=0;public double ʈ
{get{return ɾ;}}double ɽ=0;double ɻ=0;public double ɺ{get{return ɽ;}}MyShipMass ɹ;public double ɸ{get{return ɹ.TotalMass;
}}public double ɷ{get{return ɹ.BaseMass;}}double ɶ=double.NaN;public double ɵ{get{return ɶ;}}double ɴ=double.NaN;public
double ɳ{get{return ɴ;}}IMyShipController ɲ=null;IMySlimBlock ɼ=null;public IMyShipController ɱ{get{return ɲ;}}Vector3D ʊ;
public ʆ(ș Ƣ){ɒ="ShipMgr";ƪ=Ƣ;ʊ=ƪ.ǐ.Me.GetPosition();ɏ=0.5;}List<IMyTerminalBlock>ʓ=new List<IMyTerminalBlock>();int ʒ=0;
public override bool ɭ(bool ð){if(!ð){ʓ.Clear();ƪ.ǐ.GridTerminalSystem.GetBlocksOfType<IMyShipController>(ʓ);ʒ=0;if(ɲ!=null&&ɲ
.CubeGrid.GetCubeBlock(ɲ.Position)!=ɼ)ɲ=null;}if(ʓ.Count>0){for(;ʒ<ʓ.Count;ʒ++){if(!ƪ.ʔ(20))return false;
IMyShipController ʑ=ʓ[ʒ]as IMyShipController;if(ʑ.IsMainCockpit||ʑ.IsUnderControl){ɲ=ʑ;ɼ=ʑ.CubeGrid.GetCubeBlock(ʑ.Position);if(ʑ.
IsMainCockpit){ʒ=ʓ.Count;break;}}}if(ɲ==null){ɲ=ʓ[0]as IMyShipController;ɼ=ɲ.CubeGrid.GetCubeBlock(ɲ.Position);}ɹ=ɲ.CalculateShipMass
();if(!ɲ.TryGetPlanetElevation(MyPlanetElevation.Sealevel,out ɶ))ɶ=double.NaN;if(!ɲ.TryGetPlanetElevation(
MyPlanetElevation.Surface,out ɴ))ɴ=double.NaN;ʅ=ɲ.GetShipVelocities();}double ʐ=ʂ;ʂ=ʄ.Length();ʀ=(ʂ-ʐ)/ɐ;if(-ʀ>ɾ)ɾ=-ʀ;if(-ʀ>ɽ){ɽ=-ʀ;ɻ=ƪ.ȕ
;}if(ƪ.ȕ-ɻ>5&&-ʀ>0.1)ɽ-=(ɽ+ʀ)*0.3f;return true;}}class ʏ{public StringBuilder Ƴ;public ʏ(int ʎ=0){Ƴ=new StringBuilder(ʎ);
}public int ʍ{get{return Ƴ.Length;}}public ʏ ŭ(){Ƴ.Clear();return this;}public ʏ ɰ(string Ĥ){Ƴ.Append(Ĥ);return this;}
public ʏ ɰ(double ʌ){Ƴ.Append(ʌ);return this;}public ʏ ɰ(char ǩ){Ƴ.Append(ǩ);return this;}public ʏ ɰ(ʏ ʋ){Ƴ.Append(ʋ.Ƴ);return
this;}public ʏ ɰ(string Ĥ,int Ȉ,int ɔ){Ƴ.Append(Ĥ,Ȉ,ɔ);return this;}public ʏ ɰ(char ǩ,int Ř){Ƴ.Append(ǩ,Ř);return this;}
public ʏ ɖ(int Ȉ,int ɔ){Ƴ.Remove(Ȉ,ɔ);return this;}public string ɕ(){return Ƴ.ToString();}public string ɕ(int Ȉ,int ɔ){return
Ƴ.ToString(Ȉ,ɔ);}public char this[int j]{get{return Ƴ[j];}}}class ɓ{public string ɒ="MMTask";public double ɗ=0;public
double ɑ=0;public double ɐ=0;public double ɏ=-1;double Ɏ=-1;public bool ɍ=false;public bool Ɍ=false;double ɋ=0;protected ș ƪ;
public void Ɋ(ș Ƣ){ƪ=Ƣ;if(ƪ.Ț){if(Ɏ==-1){Ɏ=ɏ;ɏ*=2;}else{ɏ=Ɏ*2;}}else{if(Ɏ!=-1){ɏ=Ɏ;Ɏ=-1;}}}protected bool ɉ(double Ȱ){ɋ=Math.
Max(Ȱ,0.0001);return true;}public bool ɘ(){if(ɑ>0){ɐ=ƪ.ȕ-ɑ;ƪ.ȭ((Ɍ?"Running":"Resuming")+" task: "+ɒ);Ɍ=ɭ(!Ɍ);}else{ɐ=0;ƪ.ȭ(
"Init task: "+ɒ);ɯ();ƪ.ȭ("Running..");Ɍ=ɭ(false);if(!Ɍ)ɑ=0.001;}if(Ɍ){ɑ=ƪ.ȕ;if((ɏ>=0||ɋ>0)&&ɍ)ƪ.ȱ(this,(ɋ>0?ɋ:ɏ));else{ɍ=false;ɑ=0;}}
else{if(ɍ)ƪ.ȱ(this,0,true);}ƪ.ȭ("Task "+(Ɍ?"":"NOT ")+"finished. "+(ɍ?(ɋ>0?"Postponed by "+ɋ.ToString("F1")+"s":
"Scheduled after "+ɏ.ToString("F1")+"s"):"Stopped."));ɋ=0;return Ɍ;}public void ɣ(){ƪ.Ȯ(this);ɬ();ɍ=false;Ɍ=false;ɑ=0;}public virtual void
ɯ(){}public virtual bool ɭ(bool ð){return true;}public virtual void ɬ(){}}class ɫ{public const float ɪ=512;public const
float ɩ=ɪ/0.7783784f;public const float ɨ=ɪ/0.7783784f;public const float ɧ=ɩ;public const float ɦ=37;public string ɥ=
"T:[LCD]";public int ɮ=1;public bool ɤ=true;public List<string>ɢ=null;public bool ɡ=true;public int Ȥ=0;public float ɠ=1.0f;
public float ɟ=1.0f;public float ɞ{get{return ɧ*ǁ.ѣ;}}public float ɝ{get{return(float)ɞ-2*Ǐ[ǒ]*Ú;}}string ɜ;string ɛ;float ɚ=-
1;Dictionary<string,float>ə=new Dictionary<string,float>(2);Dictionary<string,float>ȥ=new Dictionary<string,float>(2);
Dictionary<string,float>Ȑ=new Dictionary<string,float>(2);public float ƶ{get{return Ȑ[ǒ];}}Dictionary<string,float>Ǐ=new
Dictionary<string,float>(2);Dictionary<string,float>ǎ=new Dictionary<string,float>(2);Dictionary<string,float>Ǎ=new Dictionary<
string,float>(2);int Ú=0;string Ù="";Dictionary<string,char>ǌ=new Dictionary<string,char>(2);Dictionary<string,char>ǋ=new
Dictionary<string,char>(2);Dictionary<string,char>Ǌ=new Dictionary<string,char>(2);Dictionary<string,char>ǉ=new Dictionary<string,
char>(2);public ș ƪ;public Program ǐ;public ȷ ǈ;public В Ŋ;public ʆ Ǉ;public Ð ƥ;public Ǫ ĝ;public IMyGridTerminalSystem ǆ{
get{return ǐ.GridTerminalSystem;}}public IMyProgrammableBlock ǅ{get{return ǐ.Me;}}public Action<string>Ǆ{get{return ǐ.Echo;
}}public ɫ(Program ǃ,int ǂ,ș Ƣ){ƪ=Ƣ;Ȥ=ǂ;ǐ=ǃ;ĝ=new Ǫ();ǈ=new ȷ(Ƣ,this);Ŋ=new В(Ƣ,this);Ŋ.Ѝ();Ǉ=new ʆ(ƪ);ƪ.ȱ(Ǉ,0);}Ѥ ǁ=null
;public string ǒ{get{return ǁ.L;}}public bool ǚ{get{return(ǁ.Ŝ()==0);}}public bool Ǚ(IMyTerminalBlock Ý){if(Ý==null||Ý.
WorldMatrix==MatrixD.Identity)return true;return ǆ.GetBlockWithId(Ý.EntityId)==null;}public Ѥ ǘ(Ѥ Ǘ,á ê){ê.I();IMyTextSurface C=ê.Õ
;if(Ǘ==null)Ǘ=new Ѥ(this);Ǘ.L=C.Font;if(!Ǐ.ContainsKey(Ǘ.L))Ǘ.L=ɜ;Ǘ.ѣ=(C.SurfaceSize.X/C.TextureSize.X)*(C.TextureSize.X/
C.TextureSize.Y)*ɠ/C.FontSize*(100f-C.TextPadding*2)/100*ê.K();Ù=ê.Ù;Ú=ê.Ú;ǁ=Ǘ;return Ǘ;}public Ş ǖ(Ş Þ,á ê){ê.I();
IMyTextSurface C=ê.Õ;if(Þ==null)Þ=new Ş(this);Þ.Ű(ê.Ô);Þ.ŧ=ê.O()*(C.SurfaceSize.Y/C.TextureSize.Y)*ɟ/C.FontSize*(100f-C.TextPadding*2)
/100;Þ.ů();Ù=ê.Ù;Ú=ê.Ú;return Þ;}public void Ǖ(){ǁ.Ť();}public void Ǖ(ʏ ţ){if(ǁ.Ѣ<=0)ǁ.Ǒ(Ù);ǁ.Ǒ(ţ);ǁ.Ť();}public void Ǖ(
string ţ){if(ǁ.Ѣ<=0)ǁ.Ǒ(Ù);ǁ.Ť(ţ);}public void ǔ(string ş){ǁ.Š(ş,Ù);}public void Ǔ(List<ʏ>Ŧ){ǁ.Ţ(Ŧ);}public void Ì(ʏ ť,bool ƻ=
true){if(ǁ.Ѣ<=0)ǁ.Ǒ(Ù);ǁ.Ǒ(ť);if(ƻ)ǁ.Ѣ+=ǧ(ť,ǁ.L);}public void Ì(string Ĳ,bool ƻ=true){if(ǁ.Ѣ<=0)ǁ.Ǒ(Ù);ǁ.Ǒ(Ĳ);if(ƻ)ǁ.Ѣ+=ǧ(Ĳ,
ǁ.L);}public void ƴ(ʏ ť,float ư=1.0f,float Ư=0f){Ʊ(ť,ư,Ư);ǁ.Ť();}public void ƴ(string Ĳ,float ư=1.0f,float Ư=0f){Ʊ(Ĳ,ư,Ư)
;ǁ.Ť();}ʏ Ƴ=new ʏ();public void Ʊ(ʏ ť,float ư=1.0f,float Ư=0f){float Ʈ=ǧ(ť,ǁ.L);float ƭ=ư*ɧ*ǁ.ѣ-ǁ.Ѣ-Ư;if(Ú>0)ƭ-=2*Ǐ[ǁ.L]*
Ú;if(ƭ<Ʈ){ǁ.Ǒ(ť);ǁ.Ѣ+=Ʈ;return;}ƭ-=Ʈ;int Ƭ=(int)Math.Floor(ƭ/Ǐ[ǁ.L]);float Ʋ=Ƭ*Ǐ[ǁ.L];Ƴ.ŭ().ɰ(' ',Ƭ).ɰ(ť);ǁ.Ǒ(Ƴ);ǁ.Ѣ+=Ʋ+Ʈ
;}public void Ʊ(string Ĳ,float ư=1.0f,float Ư=0f){float Ʈ=ǧ(Ĳ,ǁ.L);float ƭ=ư*ɧ*ǁ.ѣ-ǁ.Ѣ-Ư;if(Ú>0)ƭ-=2*Ǐ[ǁ.L]*Ú;if(ƭ<Ʈ){ǁ.Ǒ
(Ĳ);ǁ.Ѣ+=Ʈ;return;}ƭ-=Ʈ;int Ƭ=(int)Math.Floor(ƭ/Ǐ[ǁ.L]);float Ʋ=Ƭ*Ǐ[ǁ.L];Ƴ.ŭ().ɰ(' ',Ƭ).ɰ(Ĳ);ǁ.Ǒ(Ƴ);ǁ.Ѣ+=Ʋ+Ʈ;}public void
Ƶ(ʏ ť){ƿ(ť);ǁ.Ť();}public void Ƶ(string Ĳ){ƿ(Ĳ);ǁ.Ť();}public void ƿ(ʏ ť){float Ʈ=ǧ(ť,ǁ.L);float ǀ=ɧ/2*ǁ.ѣ-ǁ.Ѣ;if(ǀ<Ʈ/2){
ǁ.Ǒ(ť);ǁ.Ѣ+=Ʈ;return;}ǀ-=Ʈ/2;int Ƭ=(int)Math.Round(ǀ/Ǐ[ǁ.L],MidpointRounding.AwayFromZero);float Ʋ=Ƭ*Ǐ[ǁ.L];Ƴ.ŭ().ɰ(' ',Ƭ
).ɰ(ť);ǁ.Ǒ(Ƴ);ǁ.Ѣ+=Ʋ+Ʈ;}public void ƿ(string Ĳ){float Ʈ=ǧ(Ĳ,ǁ.L);float ǀ=ɧ/2*ǁ.ѣ-ǁ.Ѣ;if(ǀ<Ʈ/2){ǁ.Ǒ(Ĳ);ǁ.Ѣ+=Ʈ;return;}ǀ-=Ʈ
/2;int Ƭ=(int)Math.Round(ǀ/Ǐ[ǁ.L],MidpointRounding.AwayFromZero);float Ʋ=Ƭ*Ǐ[ǁ.L];Ƴ.ŭ().ɰ(' ',Ƭ).ɰ(Ĳ);ǁ.Ǒ(Ƴ);ǁ.Ѣ+=Ʋ+Ʈ;}
public void ƾ(double ƽ,float Ƽ=1.0f,float Ư=0f,bool ƻ=true){if(Ú>0)Ư+=2*Ú*Ǐ[ǁ.L];float ƺ=ɧ*Ƽ*ǁ.ѣ-ǁ.Ѣ-Ư;if(Double.IsNaN(ƽ))ƽ=0;
int ƹ=(int)(ƺ/ǎ[ǁ.L])-2;if(ƹ<=0)ƹ=2;int Ƹ=Math.Min((int)(ƽ*ƹ)/100,ƹ);if(Ƹ<0)Ƹ=0;if(ǁ.Ѣ<=0)ǁ.Ǒ(Ù);Ƴ.ŭ().ɰ(ǌ[ǁ.L]).ɰ(ǉ[ǁ.L],Ƹ
).ɰ(Ǌ[ǁ.L],ƹ-Ƹ).ɰ(ǋ[ǁ.L]);ǁ.Ǒ(Ƴ);if(ƻ)ǁ.Ѣ+=ǎ[ǁ.L]*ƹ+2*Ǎ[ǁ.L];}public void Ʒ(double ƽ,float Ƽ=1.0f,float Ư=0f){ƾ(ƽ,Ƽ,Ư,
false);ǁ.Ť();}public void ŝ(){ǁ.ŝ();}public void ȁ(á v,Ş D=null){v.E(D);if(v.Ö)v.Þ.Ĵ();}public void Ȁ(string ǿ,string Ĳ){
IMyTextPanel v=ǐ.GridTerminalSystem.GetBlockWithName(ǿ)as IMyTextPanel;if(v==null)return;v.WriteText(Ĳ+"\n",true);}public string Ǿ(
MyInventoryItem o){string ǽ=o.Type.TypeId.ToString();ǽ=ǽ.Substring(ǽ.LastIndexOf('_')+1);return o.Type.SubtypeId+" "+ǽ;}public void Ȃ(
string Ç,out string Å,out string Ä){int ĺ=Ç.LastIndexOf(' ');if(ĺ>=0){Å=Ç.Substring(0,ĺ);Ä=Ç.Substring(ĺ+1);return;}Å=Ç;Ä="";}
public string Ǽ(string Ç){string Å,Ä;Ȃ(Ç,out Å,out Ä);return Ǽ(Å,Ä);}public string Ǽ(string Å,string Ä){Ã o=ƥ.Æ(Å,Ä);if(o!=
null){if(o.é.Length>0)return o.é;return o.É;}return System.Text.RegularExpressions.Regex.Replace(Å,"([a-z])([A-Z])","$1 $2")
;}public void ǻ(ref string Å,ref string Ä){Ã o;if(ƥ.Í.TryGetValue(Å,out o)){Å=o.É;Ä=o.Ò;return;}o=ƥ.Æ(Å,Ä);if(o!=null){Å=
o.É;if((string.Compare(Ä,"Ore",true)==0)||(string.Compare(Ä,"Ingot",true)==0))return;Ä=o.Ò;}}public string ȏ(double ȍ,
bool Ȍ=true,char ȋ=' '){if(!Ȍ)return ȍ.ToString("#,###,###,###,###,###,###,###,###,###");string Ȋ=" kMGTPEZY";double ȉ=ȍ;int
Ȉ=Ȋ.IndexOf(ȋ);var ȇ=(Ȉ<0?0:Ȉ);while(ȉ>=1000&&ȇ+1<Ȋ.Length){ȉ/=1000;ȇ++;}Ƴ.ŭ().ɰ(Math.Round(ȉ,1,MidpointRounding.
AwayFromZero));if(ȇ>0)Ƴ.ɰ(" ").ɰ(Ȋ[ȇ]);return Ƴ.ɕ();}public string Ȏ(double ȍ,bool Ȍ=true,char ȋ=' '){if(!Ȍ)return ȍ.ToString(
"#,###,###,###,###,###,###,###,###,###");string Ȋ=" ktkMGTPEZY";double ȉ=ȍ;int Ȉ=Ȋ.IndexOf(ȋ);var ȇ=(Ȉ<0?0:Ȉ);while(ȉ>=1000&&ȇ+1<Ȋ.Length){ȉ/=1000;ȇ++;}Ƴ.ŭ().ɰ
(Math.Round(ȉ,1,MidpointRounding.AwayFromZero));if(ȇ==1)Ƴ.ɰ(" kg");else if(ȇ==2)Ƴ.ɰ(" t");else if(ȇ>2)Ƴ.ɰ(" ").ɰ(Ȋ[ȇ]).ɰ(
"t");return Ƴ.ɕ();}public string Ȇ(double ƽ){return(Math.Floor(ƽ*10)/10).ToString("F1");}Dictionary<char,float>ȅ=new
Dictionary<char,float>();void Ȅ(string ȃ,float B){B+=1;for(int Y=0;Y<ȃ.Length;Y++){if(B>ə[ɜ])ə[ɜ]=B;ȅ.Add(ȃ[Y],B);}}public float Ǻ
(char ǩ,string L){float ƺ;if(L==ɛ||!ȅ.TryGetValue(ǩ,out ƺ))return ə[L];return ƺ;}public float ǧ(ʏ Ǩ,string L){if(L==ɛ)
return Ǩ.ʍ*ə[L];float Ǧ=0;for(int Y=0;Y<Ǩ.ʍ;Y++)Ǧ+=Ǻ(Ǩ[Y],L);return Ǧ;}public float ǧ(string Ĥ,string L){if(L==ɛ)return Ĥ.
Length*ə[L];float Ǧ=0;for(int Y=0;Y<Ĥ.Length;Y++)Ǧ+=Ǻ(Ĥ[Y],L);return Ǧ;}public string ǥ(string Ĳ,float ǣ){if(ǣ/ə[ǁ.L]>=Ĳ.
Length)return Ĳ;float Ǣ=ǧ(Ĳ,ǁ.L);if(Ǣ<=ǣ)return Ĳ;float ǡ=Ǣ/Ĳ.Length;ǣ-=ȥ[ǁ.L];int Ǡ=(int)Math.Max(ǣ/ǡ,1);if(Ǡ<Ĳ.Length/2){Ƴ.ŭ
().ɰ(Ĳ,0,Ǡ);Ǣ=ǧ(Ƴ,ǁ.L);}else{Ƴ.ŭ().ɰ(Ĳ);Ǡ=Ĳ.Length;}while(Ǣ>ǣ&&Ǡ>1){Ǡ--;Ǣ-=Ǻ(Ĳ[Ǡ],ǁ.L);}if(Ƴ.ʍ>Ǡ)Ƴ.ɖ(Ǡ,Ƴ.ʍ-Ǡ);return Ƴ.ɰ(
"..").ɕ();}void ǟ(string Ǟ){ɜ=Ǟ;ǌ[ɜ]=MMStyle.BAR_START;ǋ[ɜ]=MMStyle.BAR_END;Ǌ[ɜ]=MMStyle.BAR_EMPTY;ǉ[ɜ]=MMStyle.BAR_FILL;ə[ɜ
]=0f;}void ǝ(string ǜ,float Ǜ){ɛ=ǜ;ɚ=Ǜ;ə[ɛ]=ɚ+1;ȥ[ɛ]=2*(ɚ+1);ǌ[ɛ]=MMStyle.BAR_MONO_START;ǋ[ɛ]=MMStyle.BAR_MONO_END;Ǌ[ɛ]=
MMStyle.BAR_MONO_EMPTY;ǉ[ɛ]=MMStyle.BAR_MONO_FILL;Ǐ[ɛ]=Ǻ(' ',ɛ);ǎ[ɛ]=Ǻ(Ǌ[ɛ],ɛ);Ǎ[ɛ]=Ǻ(ǌ[ɛ],ɛ);Ȑ[ɛ]=ǧ(" 100.0%",ɛ);}public void
Ǥ(){if(ȅ.Count>0)return;
// Monospace font name, width of single character
// Change this if you want to use different (modded) monospace font
ǝ("Monospace", 24f);

// Classic/Debug font name (uses widths of characters below)
// Change this if you want to use different font name (non-monospace)
ǟ("Debug");
// Font characters width (font "aw" values here)
Ȅ("3FKTabdeghknopqsuy£µÝàáâãäåèéêëðñòóôõöøùúûüýþÿāăąďđēĕėęěĝğġģĥħĶķńņňŉōŏőśŝşšŢŤŦũūŭůűųŶŷŸșȚЎЗКЛбдекруцяёђћўџ", 17f);
Ȅ("ABDNOQRSÀÁÂÃÄÅÐÑÒÓÔÕÖØĂĄĎĐŃŅŇŌŎŐŔŖŘŚŜŞŠȘЅЊЖф□", 21f);
Ȅ("#0245689CXZ¤¥ÇßĆĈĊČŹŻŽƒЁЌАБВДИЙПРСТУХЬ€", 19f);
Ȅ("￥$&GHPUVY§ÙÚÛÜÞĀĜĞĠĢĤĦŨŪŬŮŰŲОФЦЪЯжы†‡", 20f);
Ȅ("！ !I`ijl ¡¨¯´¸ÌÍÎÏìíîïĨĩĪīĮįİıĵĺļľłˆˇ˘˙˚˛˜˝ІЇії‹›∙", 8f);
Ȅ("？7?Jcz¢¿çćĉċčĴźżžЃЈЧавийнопсъьѓѕќ", 16f);
Ȅ("（）：《》，。、；【】(),.1:;[]ft{}·ţťŧț", 9f);
Ȅ("+<=>E^~¬±¶ÈÉÊË×÷ĒĔĖĘĚЄЏЕНЭ−", 18f);
Ȅ("L_vx«»ĹĻĽĿŁГгзлхчҐ–•", 15f);
Ȅ("\"-rª­ºŀŕŗř", 10f);
Ȅ("WÆŒŴ—…‰", 31f);
Ȅ("'|¦ˉ‘’‚", 6f);
Ȅ("@©®мшњ", 25f);
Ȅ("mw¼ŵЮщ", 27f);
Ȅ("/ĳтэє", 14f);
Ȅ("\\°“”„", 12f);
Ȅ("*²³¹", 11f);
Ȅ("¾æœЉ", 28f);
Ȅ("%ĲЫ", 24f);
Ȅ("MМШ", 26f);
Ȅ("½Щ", 29f);
Ȅ("ю", 23f);
Ȅ("ј", 7f);
Ȅ("љ", 22f);
Ȅ("ґ", 13f);
Ȅ("™", 30f);
// End of font characters width
        Ǐ[ɜ]=Ǻ(' ',ɜ);ǎ[ɜ]=Ǻ(Ǌ[ɜ],ɜ);Ǎ[ɜ]=Ǻ(ǌ[ɜ],ɜ);Ȑ[ɜ]=ǧ(" 100.0%",ɜ);ȥ[ɜ]=Ǻ('.',ɜ)*2;}}class Ǫ{public string ǲ(string
ǹ){return TT[ǹ];}
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
}static class Ǹ{public static bool Ƿ(this string Ĥ,string ǵ){return Ĥ.StartsWith(ǵ,StringComparison.
InvariantCultureIgnoreCase);}public static bool Ƕ(this string Ĥ,string ǵ){if(Ĥ==null)return false;return Ĥ.IndexOf(ǵ,StringComparison.
InvariantCultureIgnoreCase)>=0;}public static bool Ǵ(this string Ĥ,string ǵ){return Ĥ.EndsWith(ǵ,StringComparison.InvariantCultureIgnoreCase);}}
static class ǳ{public static string Ǳ(this IMyTerminalBlock Ý){int Ŀ=Ý.CustomData.IndexOf("\n---\n");if(Ŀ<0){if(Ý.CustomData.
StartsWith("---\n"))return Ý.CustomData.Substring(4);return Ý.CustomData;}return Ý.CustomData.Substring(Ŀ+5);}public static string
ǰ(this IMyTerminalBlock Ý,int ĺ,string ǯ){string Ǯ=Ý.Ǳ();string ǭ="@"+ĺ.ToString()+" AutoLCD";string Ǭ='\n'+ǭ;int Ŀ=0;if(
!Ǯ.StartsWith(ǭ,StringComparison.InvariantCultureIgnoreCase)){Ŀ=Ǯ.IndexOf(Ǭ,StringComparison.InvariantCultureIgnoreCase);
}if(Ŀ<0){if(ĺ==0){if(Ǯ.Length==0)return"";if(Ǯ[0]=='@')return null;Ŀ=Ǯ.IndexOf("\n@");if(Ŀ<0)return Ǯ;return Ǯ.Substring(
0,Ŀ);}else return null;}int ǫ=Ǯ.IndexOf("\n@",Ŀ+1);if(ǫ<0){if(Ŀ==0)return Ǯ;return Ǯ.Substring(Ŀ+1);}if(Ŀ==0)return Ǯ.
Substring(0,ǫ);return Ǯ.Substring(Ŀ+1,ǫ-Ŀ);}