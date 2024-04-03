/*  Author: Ich_73
>>> Ship Printer Controller [v1.0]
        Controls the pistons, welders, and the projector of a ship printer.
        Displays the print status and progress on the display of the programmable block.
        
        Prerequisites:
        * Experimental mode must be enabled!
        
        Setup:
        * Build a ship printer with...
            + ... one or more Piston(s) that are connected to a grid (of the size of your choice) where you build a Rod
            + ... a Projector on the grid with the Rod
            + ... many ship Welders that are connected to a cargo container holding enough resources to build the ship
        * Build a Programmable Block and load this script
        * Build a button panel (or a similar block that lets you assign actions) and assign three actions.
          Select the Programmable Block with the Run action and enter the arguments PRINT, PAUSE and RESET, respectively.
        
        Configure the script in the Programmable Block:
        * Enter the block names (required)
            + for pistonName enter the name of your Piston(s) (all pistons must have the same name)
            + for welderName enter the name of your Welders (all welders must have the same name)
            + for projectorName enter the name of your Projector
        * Enter the block names (optional)
            + for speakerName enter the name of your speaker if you want to hear a sound when a print starts and ends
        * Adjust the speeds and wait times (modify as needed)
            + for printSpeed, lower values ensure that all blocks are welded, higher values reduce print times
            + for weldingDelay, higher values ensure that all blocks are welded, lower values reduce print times
        
        Instructions:
        * Configure the Projector to show the desired print and position it such that it touches the Rod
        * Start the print by pressing the PRINT button (or running the script with the PRINT argument)
        * Wait until the print is finished...
            + You can pause the print by pressing the PAUSE button (or running the script with the PAUSE argument)
        * Remove the ship from the Rod and fly it away
        * Reset the printer by pressing the RESET button (or running the script with the RESET argument)
        
        Note:
        * Run the script with the command RESTART argument to fully restart / factory reset it.
<<<
*/

/* === CONFIGURATION === */

// Block Names (required)
const string pistonName = "Piston (Ship Printer)";
const string welderName = "Welder (Ship Printer)";
const string projectorName = "Projector (Ship Printer)";

// Block Names (optional)
const string speakerName = "Speaker (Ship Printer)";

// Speeds & Wait Times (modify as needed)
const float printSpeed = 0.1f; // default: 0.1f
const float resetSpeed = 1.0f; // default: 1.0f
const int weldingDelay = (int) (6.06f * 3); // default: 3 [seconds]

// Programmable Block Display (modify as needed)
const float fontSize = 1.6f;
const float textPadding = 10.0f;
const TextAlignment textAlignment = TextAlignment.CENTER;
const int progressBarWidth = (int) (72f / fontSize);

/* ==================== */


// Variables
int step;
string status;
string task;
bool isWelding;
int stepWelding;
int previousRemainingBlocks;

// Blocks
List<IMyPistonBase> pistons = new List<IMyPistonBase>();
List<IMyShipWelder> welders = new List<IMyShipWelder>();
IMyProjector projector = null;
IMySoundBlock speaker = null;


// Initialization
private void Initialize() {
    step = 0;
    status = null;
    task = null;
    isWelding = true;
    stepWelding = 0;
    previousRemainingBlocks = -1;
}
public Program() {
    Initialize();
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
    string[] storedData = Storage.Split(';');
    if (storedData.Count() != 6) return;
    try {
        int.TryParse(storedData[0], out step);
        status = storedData[1] != "" ? storedData[1] : null;
        task = storedData[2] != "" ? storedData[2] : null;
        bool.TryParse(storedData[3], out isWelding);
        int.TryParse(storedData[4], out stepWelding);
        int.TryParse(storedData[5], out previousRemainingBlocks);
    } catch { Storage = ""; }
}

// Storage
public void Save() {
    Storage = string.Join(";",
        step.ToString(),
        status ?? "",
        task ?? "",
        isWelding.ToString(),
        stepWelding.ToString(),
        previousRemainingBlocks.ToString()
    );
}

// Main Loop
public void Main(string argument) {
    // Print Info
    Echo("== Ship Printer ==");
    Echo("Task: " + ((task == null) ? "N/A" : task));
    Echo("Status: " + ((status == null) ? "waiting..." : status));
    Echo("Step: " + step.ToString());
    Echo("");
    
    
    // === SETUP === //
    
    // Setup Display
    Me.GetSurface(0).ContentType = ContentType.TEXT_AND_IMAGE;
    Me.GetSurface(0).WriteText("Ship Printer\n\n");
    Me.GetSurface(0).FontSize = fontSize;
    Me.GetSurface(0).TextPadding = textPadding;
    Me.GetSurface(0).Alignment = textAlignment;

    // Get Blocks
    GridTerminalSystem.GetBlocksOfType(pistons, p => p.CustomName == pistonName);
    GridTerminalSystem.GetBlocksOfType(welders, w => w.CustomName == welderName);
    projector = GridTerminalSystem.GetBlockWithName(projectorName) as IMyProjector;
    speaker = GridTerminalSystem.GetBlockWithName(speakerName) as IMySoundBlock;
    
    
    // === ARGUMENTS === //
    
    // Restart: reset storage and restart
    if (argument == "RESTART") { Storage = ""; Initialize(); return; }
    
    // Print: enable welder and projector, disable piston, wait stepOn to enable pistons
    if (argument == "PRINT") {
        task = "PRINT";
        status = "PRINT";
        isWelding = true;
        stepWelding = step + weldingDelay;
        previousRemainingBlocks = int.MaxValue;
        foreach (var welder in welders) welder.Enabled = true;
        foreach (var piston in pistons) piston.Enabled = false;
        projector.Enabled = true;
        projector.ShowOnlyBuildable = true;
        if (speaker != null) speaker.Play();
    }
    
    // Pause: disable welder and pistons
    else if (argument == "PAUSE" &&  task != null) {
        status = "PAUSE";
        foreach (var welder in welders) welder.Enabled = false;
        foreach (var piston in pistons) piston.Enabled = false;
    }
    
    // Reset: disable welder and projector, retract pistons
    else if (argument == "RESET") {
        task = "RESET";
        status = "RESET";
        foreach (var welder in welders) welder.Enabled = false;
        foreach (var piston in pistons) {
            piston.Enabled = true;
            piston.Velocity = resetSpeed;
            piston.Retract();
        }
        projector.Enabled = false;
    }
    
    
    // === LOGIC === //
    
    // Print
    if (status == "PRINT") {
        // Progress
        Me.GetSurface(0).WriteText("Printing...\n", true);
        MePrintProgress(true);
        Echo("Blocks: " + (projector.TotalBlocks - projector.RemainingBlocks).ToString() + "/" + projector.TotalBlocks.ToString());
        Echo("Buildable: " + projector.BuildableBlocksCount.ToString());
        
        // Projector: check welding
        if (!isWelding || step >= stepWelding) {
            isWelding = projector.RemainingBlocks < previousRemainingBlocks;
            previousRemainingBlocks = projector.RemainingBlocks;
            stepWelding = step + weldingDelay;
            if (!isWelding) {
                foreach (var piston in pistons) {
                    piston.Enabled = true;
                    piston.Velocity = printSpeed;
                    piston.Extend();
                }
            } else {
                foreach (var piston in pistons) piston.Enabled = false;
            }
        }
        
        // Finish: after fully built, reset task and show finish
        if (projector.RemainingBlocks == 0) {
            task = null;
            status = "FINISH";
            foreach (var welder in welders) welder.Enabled = false;
            foreach (var piston in pistons) piston.Enabled = false;
            projector.Enabled = false;
            if (speaker != null) speaker.Play();
        }
    }
    
    // Pause
    else if (status == "PAUSE") {
        Me.GetSurface(0).WriteText("Paused\n", true);
        if (task == "PRINT") MePrintProgress(true);
        if (task == "RESET") MeResetProgress();
    }
    
    // Finish
    else if (status == "FINISH") {
        Me.GetSurface(0).WriteText("Finished\n", true);
        MePrintProgress(false);
    }
    
    // Reset
    else if (status == "RESET") {
        // Progress
        Me.GetSurface(0).WriteText("Resetting...\n", true);
        bool fullyRetracted = MeResetProgress();
        
        // Waiting: if pistons fully retracted, disalbe pistons, reset task and status
        if (fullyRetracted) {
            task = null;
            status = null;
            foreach (var piston in pistons) piston.Enabled = false;
        }
    }
    
    // Null
    else if (status == null) {
        Me.GetSurface(0).WriteText("Ready to print :-)\n", true);
    }
    
    step++;
}

private float MePrintProgress(bool showProgressBar) {
    int total = projector.TotalBlocks;
    int welded = total - projector.RemainingBlocks;
    float progress = (float) welded / total;
    Echo("Printing... " + (progress * 100).ToString("0.0") + "%");
    if (showProgressBar) MeProgressBar(progress);
    else Me.GetSurface(0).WriteText(("Result: " + (progress * 100).ToString("0.0") + "%\n"), true);
    Me.GetSurface(0).WriteText("Blocks: " + welded.ToString() + "/" + total.ToString() + "\n", true);
    return progress;
}

private bool MeResetProgress() {
    float min = 0f, max = 0f, current = 0f;
    foreach (var piston in pistons) {
        min += piston.MinLimit;
        max += piston.MaxLimit;
        current += piston.CurrentPosition;
    }
    float progress = ((max - min) - (current - min)) / (max - min);
    Echo("Resetting... " + (progress * 100).ToString("0.0") + "%");
    MeProgressBar(progress);
    return current == min;
}

private void MeProgressBar(float progress) {
    int n = progressBarWidth;
    int k = (int) (progress * n);
    string s = "[" + string.Concat(Enumerable.Repeat("|", k)) + string.Concat(Enumerable.Repeat("'", n - k)) + "]\n";
    Me.GetSurface(0).WriteText(s, true);
}
