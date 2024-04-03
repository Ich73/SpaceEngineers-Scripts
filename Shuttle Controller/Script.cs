/*  Author: Ich_73
>>> Shuttle Controller [v1.0]
        Script to operate a shuttle that transports one resource on the outward journey and another resource on the return journey
        by monitoring free container space and (de-)activating sorters to load the ship with the required quantity.
        Designed to work with the Shuttle Mode of '[PAM] Path Auto Miner' (https://steamcommunity.com/sharedfiles/filedetails/?id=1507646929) by Keks.
        
        Prerequisites:
        * Experimental mode must be enabled!
        * Build a Programmable Block on your shuttle Ship and load this script
        * (optional) Configure 'PAM' by Keks on your Ship in Shuttle Mode
        
        Example:
        On Station A you mine Ore, but you have no access to refineries and Ice.
        On Station B you have Ice and refineries, but you have no access to Ore.
        You want to use a single Ship to transport Ore from Station A to Station B,
          and on the return journey you want use the same Ship to transport Ice from Station B to Station A, and so on.
        Normally you run into problems when the Ship is full of Ice but your containers on Station A have no more free space,
          because you cannot unload the Ship and therefore you cannot load any more Ore to transport it to Station B.
        This is where Shuttle Controller comes into play. It measures the free container space on each station
          and only loads as much goods as it can unload later.
        
        You build the following connections:
        * On Station A build Ice Containers and a conveyor system with a Conveyor Sorter that fetches all Ice from the Ship.
          This Container is monitored by the script; these Sorters are always enabled and not used by the script.
        * On Station A build Ore Containers and a conveyor system with a Conveyor Sorter that loads all Ore onto the Ship.
          This Container is not used by the script; these Sorters are (de-)activated by the script.
        * On Station B build Ore Containers and a conveyor system with a Conveyor Sorter that fetches all Ore from the Ship.
          This Container is monitored by the script; these Sorters are always enabled and not used by the script.
        * On Station B build Ice Containers and a conveyor system with a Conveyor Sorter that loads all Ice onto the Ship.
          This Container is not used by the script; these Sorters are (de-)activated by the script.
        
        Configure the script in the Programmable Block of your Ship:
        * Enter the block names (required)
            + for connectorName enter the name of the Connector of your Ship
            + for containerName enter the name of the Container(s) of your Ship (all containers must have the same name)
            + for pamBlockName enter the name of the Programmable Block running 'PAM' in Shuttle Mode
        * Configure the jobs (required)
            + Note: To implement the above example, we have two entries for each list, but theoretically more entries are possible.
            + for identifiers enter arbitrary nicknames for the individual docking stations, e.g. the names of the stations
            + for connectorNames enter the name of the Connector to which your Ship docks at each station
            + for containerGroupNames enter the name of the Group of Containers to monitor on each station
                - Note: These are the Containers in which the goods are stored that you want to load onto your Ship and take elsewhere, not the ones with the goods you want to import.
            + for sorterNames enter the name of the Sorter(s) that load goods onto your Ship for each station
                - Note: These are the Sorters that handle the goods you want to load onto your Ship and take elsewhere, not the ones that handle the goods you want to unload.
                - Note: With two stations, as in the example, you only change the strings and leave the null entries untouched.
        * Adjust the behavior (modify as needed)
            + for largeContainerSize change the value at the end, i.e. the 3L, to the multiplier value you are playing with
                - Note: This is purely cosmetic. The script shows you container capacities in the output of the programmable block as multiples of this value.
            + for tankMinimumFill enter the required filling level of gas tanks before departure
            + for batteryMinimumStoredPower enter the required charging level of batteries before departure
        
        Instructions:
        * Simply run the script
        
        Note:
        * Run the script with the RESTART argument to fully restart / factory reset it.
<<<
*/

/* === CONFIGURATION === */

// Blocks on this Ship
const string connectorName = "My Ship's Connector";
const string containerName = "My Ship's Container";
const string pamBlockName = "My Ship's Programmable Block with PAM"; // optional

// All Constructions that can be approached...
// - Identifiers: nicknames for the configurations
// - Connector Names: name of the connector where this Ship docks
// - Container Group Names: name of the group of containers to watch
// - Sorter Names: names of the sorters to (de-)activate to fill this Ship
//   where the first sorter belong to the first group of containers,
//   diagonal entries are ignored, e.g., sorter #1 for identifier #1
static List<string> identifiers = new List<string> {
    "Station A",
    "Station B",
};
static List<string> connectorNames = new List<string> {
    "Connector on Station A",
    "Connector on Station B",
};
static List<string> containerGroupNames = new List<string> {
    "Group of Ice Containers on Station A",
    "Group of Ore Containers on Station B",
};
static List<List<string>> sorterNames = new List<List<string>> {
    new List<string> { null, "Sorter to load Ore on Station B" },
    new List<string> { "Sorter to load Ice on Station A", null},
};

// Misc (modify as needed)
const long largeContainerSize = 421875L * 1000L * 3L; // size of a large container w/ multiplier 3x
const int stepDelay = 50; // default: 50
const double tankMinimumFill = 0.95d; // default: 0.95d
const float batteryMinimumStoredPower = 0.90f; // default: 0.90f

// Programmable Block Display (modify as needed)
const float fontSize = 1.6f;
const float textPadding = 10.0f;
const TextAlignment textAlignment = TextAlignment.CENTER;
const int progressBarWidth = (int) (72f / fontSize);

/* ==================== */


// Variables
int state;
int step;
int stepIndicator;

long myContainerVolume;
long myContainerVolumeMemory;
long myContainerVolumeIndicator;

List<long> containerFreeSpaces;


// Initialization
private void Initialize() {
    state = -1;
    step = 0;
    stepIndicator = 0;
    
    myContainerVolume = 0;
    myContainerVolumeMemory = 0;
    myContainerVolumeIndicator = 0;
    
    containerFreeSpaces = new List<long>(new long[identifiers.Count]);
}
public Program() {
    Initialize();
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
    string[] storedData = Storage.Split(';');
    if (storedData.Count() != 7) return;
    try {
        int.TryParse(storedData[0], out state);
        int.TryParse(storedData[1], out step);
        int.TryParse(storedData[2], out stepIndicator);
        long.TryParse(storedData[3], out myContainerVolume);
        long.TryParse(storedData[4], out myContainerVolumeMemory);
        long.TryParse(storedData[5], out myContainerVolumeIndicator);
        string[] splits = storedData[6].Split('|');
        for (var i = 0; i < splits.Count(); i++) containerFreeSpaces[i] = long.Parse(splits[i]);
    } catch { Storage = ""; }
}

// Storage
public void Save() {
    Storage = string.Join(";",
        state.ToString(),
        step.ToString(),
        stepIndicator.ToString(),
        myContainerVolume.ToString(),
        myContainerVolumeMemory.ToString(),
        myContainerVolumeIndicator.ToString(),
        string.Join("|", containerFreeSpaces.ConvertAll(v => v.ToString()))
    );
}

// Main Loop
public void Main(string argument) {
    // Print Info
    Echo("== Shuttle Controller ==");
    Echo("Step: " + step.ToString());
    Echo("State: " + (state == -1 ? "Undocked" : (state == 0 ? "Unloading..." : (state >= identifiers.Count + 1 ? "Finished" : "Loading..."))));
    Echo("Ship Container Volume: " + spacify(myContainerVolume));
    Echo("Free Container Space:");
    for (int i = 0; i < identifiers.Count; i++) {
        Echo("  * " + identifiers[i] + ": " + spacify(containerFreeSpaces[i]));
    }
    Echo("");
    
    // Setup Display
    Me.GetSurface(0).ContentType = ContentType.TEXT_AND_IMAGE;
    Me.GetSurface(0).WriteText("Shuttle Controller\n\n");
    Me.GetSurface(0).FontSize = fontSize;
    Me.GetSurface(0).TextPadding = textPadding;
    Me.GetSurface(0).Alignment = textAlignment;
    
    step++;
    
    // === ARGUMENTS === //
    
    // Restart: reset storage and restart
    if (argument == "RESTART") { Storage = ""; Initialize(); return; }
    
    
    // === Measurements === //
    
    // Container: measure space in containers
    List<IMyCargoContainer> myContainers = new List<IMyCargoContainer>();
    GridTerminalSystem.GetBlocksOfType(myContainers, p => p.CustomName == containerName);
    myContainerVolume = 0;
    long myContainerMaxVolume = 0;
    foreach (var container in myContainers) {
        IMyInventory inv = container.GetInventory();
        myContainerVolume += inv.CurrentVolume.RawValue;
        myContainerMaxVolume += inv.MaxVolume.RawValue;
    }
    
    // Connector: check name of connected grid
    IMyShipConnector connector = GridTerminalSystem.GetBlockWithName(connectorName) as IMyShipConnector;
    if (!connector.IsConnected) {
        state = -1;
        Me.GetSurface(0).WriteText("Undocked :-)\n", true);
        return;
    }
    string otherConnectorName = connector.OtherConnector.CustomName;
    Echo("Connected to " + otherConnectorName);
    int index = connectorNames.IndexOf(otherConnectorName);
    if (index == -1) return;
    
    // Container: measure remaining space in containers
    string containerGroupName = containerGroupNames[index];
    IMyBlockGroup containerGroup = GridTerminalSystem.GetBlockGroupWithName(containerGroupName);
    if (containerGroup != null) {
        List<IMyCargoContainer> containers = new List<IMyCargoContainer>();
        containerGroup.GetBlocksOfType(containers);
        Echo("Watching " + containers.Count.ToString() + " containers...");
        long emptySpace = 0;
        foreach (var container in containers) {
            IMyInventory inv = container.GetInventory();
            emptySpace += (inv.MaxVolume - inv.CurrentVolume).RawValue;
        }
        containerFreeSpaces[index] = emptySpace;
    } else containerFreeSpaces[index] = 0L;
    Echo("");
    
    
    // === State Logic === //
    
    // Undocked -> Unloading
    if (state == -1) {
        state = 0;
        stepIndicator = step + stepDelay;
        myContainerVolumeIndicator = myContainerVolume;
    }
    
    // Unloading
    else if (state == 0) {
        foreach (var sorterName in sorterNames[index]) { // disable all sorters
            if (sorterName == null) continue;
            setSortersEnabled(sorterName, false);
        }
        float progress = (float) (myContainerMaxVolume - myContainerVolume) / myContainerMaxVolume;
        Echo("Unloading... " + (progress * 100).ToString("0.0") + "%");
        Me.GetSurface(0).WriteText("Unloading...\n", true);
        MeProgressBar(progress);
        
        if (step >= stepIndicator && myContainerVolume < myContainerVolumeIndicator) {
            stepIndicator = step + stepDelay;
            myContainerVolumeIndicator = myContainerVolume;
        }
        if (step >= stepIndicator && myContainerVolume == myContainerVolumeIndicator || myContainerVolume == 0) {
            state = 1;
            myContainerVolumeMemory = myContainerVolume;
            stepIndicator = step + stepDelay;
            myContainerVolumeIndicator = myContainerVolume;
        }
    }
    
    // Loading
    else if (state > 0 && state <= identifiers.Count) {
        if (index == state-1 || sorterNames[index][state-1] == null || containerFreeSpaces[state-1] == 0) state++; // skip this sorter
        else {
            setSortersEnabled(sorterNames[index][state-1], true);
            float progress = (float) (myContainerVolume - myContainerVolumeMemory) / Math.Min(myContainerMaxVolume - myContainerVolumeMemory, containerFreeSpaces[state-1]);
            if (progress > 1f) progress = 1f;
            int k = state-1 < index ? state : state - 1;
            int n = identifiers.Count - 1;
            Echo("Loading" + (n > 1 ? " " + k.ToString() + "/" + n.ToString() : "") + "... " + (progress * 100).ToString("0.0") + "%");
            Me.GetSurface(0).WriteText("Loading" + (n > 1 ? " " + k.ToString() + "/" + n.ToString() : "") + "...\n", true);
            MeProgressBar(progress);
            
            if (step >= stepIndicator && myContainerVolume > myContainerVolumeIndicator) {
                stepIndicator = step + stepDelay;
                myContainerVolumeIndicator = myContainerVolume;
            }
            if (
                (step >= stepIndicator && myContainerVolume == myContainerVolumeIndicator) // no change, or ship full
                || (myContainerVolume - myContainerVolumeMemory >= containerFreeSpaces[state-1]) // loaded enough
            ) { // loading finished -> Loading next
                setSortersEnabled(sorterNames[index][state-1], false);
                state++;
                myContainerVolumeMemory = myContainerVolume;
                stepIndicator = step + stepDelay;
                myContainerVolumeIndicator = myContainerVolume;
            }
        }
    }
    
    // Finished
    else if (state == identifiers.Count + 1) {
        // check gas tanks
        List<IMyGasTank> tanks = new List<IMyGasTank>();
        GridTerminalSystem.GetBlocksOfType(tanks, b => b.IsSameConstructAs(Me));
        double filledRatio = 0d;
        foreach (var tank in tanks) filledRatio += tank.FilledRatio;
        filledRatio = filledRatio / tanks.Count;
        Echo("Tanks: " + filledRatio.ToString("0.0%") + "/" + tankMinimumFill.ToString("0.0%"));
        
        // check batteries
        List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
        GridTerminalSystem.GetBlocksOfType(batteries, b => b.IsSameConstructAs(Me));
        float current = 0f, max = 0f;
        foreach (var battery in batteries) {
            current += battery.CurrentStoredPower;
            max += battery.MaxStoredPower;
        }
        float storedPower = current / max;
        Echo("Batteries: " + storedPower.ToString("0.0%") + "/" + batteryMinimumStoredPower.ToString("0.0%"));
        
        // evaluate
        if (filledRatio < tankMinimumFill || storedPower < batteryMinimumStoredPower) {
            Echo("Waiting to refuel and charge...");
            Me.GetSurface(0).WriteText("Refuel and Charge\n", true);
            MeProgressBar((float) (filledRatio / tankMinimumFill));
            MeProgressBar(storedPower / batteryMinimumStoredPower);
        } else {
            Echo("Finished");
            Me.GetSurface(0).WriteText("Finished\n", true);
            if (pamBlockName != null) {
                List<IMyProgrammableBlock> pams = new List<IMyProgrammableBlock>();
                GridTerminalSystem.GetBlocksOfType(pams, b => b.IsSameConstructAs(Me) &&  b.CustomName == pamBlockName);
                IMyProgrammableBlock pam = pams[0];
                pam.TryRun("Undock");
            }
        }
    }
}

private void setSortersEnabled(string sorterName, bool enabled) {
    List<IMyConveyorSorter> sorters = new List<IMyConveyorSorter>();
    GridTerminalSystem.GetBlocksOfType(sorters, p => p.CustomName == sorterName);
    foreach (var sorter in sorters) sorter.Enabled = enabled;
}

private string spacify(long space) {
    return space2string(space) + " (" + space2containers(space) + ")";
}

static List<string> indices = new List<string> { "L", "kL", "ML", "GL" };
private string space2string(long space) {
    int thousand = (int) (space % 1000);
    space = space / 1000;
    int i = 0;
    for (; i < indices.Count; i++) {
        if (space < 1000) break;
        thousand = (int) (space % 1000);
        space = space / 1000;
    }
    return space.ToString() + "." + (thousand / 100).ToString() + " " + indices[i];
}

private string space2containers(long space) {
    return ((float) space / largeContainerSize).ToString("0.00") + "x";
}

private void MeProgressBar(float progress) {
    int n = progressBarWidth;
    int k = (int) (progress * n);
    string s = "[" + string.Concat(Enumerable.Repeat("|", k)) + string.Concat(Enumerable.Repeat("'", n - k)) + "]\n";
    Me.GetSurface(0).WriteText(s, true);
}
