/*  Author: Ich_73
>>> Connector Guard
        Watches connectors that are on the same construction as this programmable block.
        The custom data of each connector specifies which timer blocks to activate on connecting/disconnecting/approaching.
        
        Setup:
        * Experimental mode must be enabled!
        * Build a programmable block and load this script
        
        Usage:
        * Build a connector and 1-3 timer blocks
        * In the custom data of the connector...
            + ... write "@ConnectorGuard" in the first line
            + (optional) ... add a line with "onConnect: <timer block>" to specify the timer block to run when the connector connects
            + (optional) ... add a line with "onDisconnect: <timer block>" to specify the timer block to run when the connector disconnects
            + (optional) ... add a line with "onApproach: <timer block>" to specify the timer block to run when the connector can connect but is not yet connected
        * In each timer block...
            + ... set the delay to 00:00:01
            + ... configure the actions to be executed when the timer block is run
        
        Example:
        * The custom data of a connector that runs a timer block on connect and another timer block on disconnect:
            @ConnectorGuard
            onConnect: My Timer Block Connect
            onDisconnect: My Timer Block Disconnect
        * The custom data of a connect that runs the same timer block on connect and approach:
            @ConnectorGuard
            onApproach: Timer Block 3
            onConnect: Timer Block 3
<<<
*/

// Variables
int step = 0;
Dictionary<IMyShipConnector, MyShipConnectorStatus> lastStatus = new Dictionary<IMyShipConnector, MyShipConnectorStatus>();
Dictionary<IMyShipConnector, string> lastUpdate = new Dictionary<IMyShipConnector, string>();
Dictionary<IMyShipConnector, int> lastUpdateStep = new Dictionary<IMyShipConnector, int>();

// Constants
const MyShipConnectorStatus Unconnected = MyShipConnectorStatus.Unconnected;
const MyShipConnectorStatus Connectable = MyShipConnectorStatus.Connectable;
const MyShipConnectorStatus Connected = MyShipConnectorStatus.Connected;

// Initialization
public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
}

// Main Loop
public void Main() {
    // Get connectors on this construction
    List<IMyShipConnector> connectors = new List<IMyShipConnector>();
    GridTerminalSystem.GetBlocksOfType(connectors, block => block.IsSameConstructAs(Me));
    connectors.Sort((a, b) => a.BlockDefinition.ToString().CompareTo(b.BlockDefinition.ToString()));
    
    // Print status
    Echo("== Connector Guard ==");
    Echo("Step: " + step.ToString());
    
    // Loop over connectors
    foreach (var c in connectors) {
        // Check custom data    
        if (!c.CustomData.StartsWith("@ConnectorGuard")) continue;
        Echo("");
        Echo(c.CustomName);
        
        // Get timer blocks
        var lines = c.CustomData.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None).Skip(1);
        var timers = new Dictionary<string, IMyTimerBlock>();
        foreach (var s in lines) {
            if (s.Trim().Length == 0) continue;
            var i = s.IndexOf(":");
            var action = s.Substring(0, i).Trim();
            var name = s.Substring(i+1).Trim();
            var block = GridTerminalSystem.GetBlockWithName(name) as IMyTimerBlock;
            if (block == null) { Echo("[ERROR] Unknown block: " + name); continue; }
            timers[action] = block;
        }
        
        // Print status
        Echo("   Status: " + c.Status.ToString());
        if (lastUpdate.ContainsKey(c)) Echo("   Log: " + lastUpdate[c].ToString() + " at step " + lastUpdateStep[c].ToString());
        
        // Logic
        if (!lastStatus.ContainsKey(c)) { lastStatus[c] = c.Status; continue; }
        string update = null;
        if (lastStatus[c] != Connected && c.Status == Connected && timers.ContainsKey("onConnect")) update = "onConnect";
        if (lastStatus[c] == Connected && c.Status != Connected && timers.ContainsKey("onDisconnect")) update = "onDisconnect";
        if (lastStatus[c] == Unconnected && c.Status == Connectable && timers.ContainsKey("onApproach")) update = "onApproach";
        if (update != null) {
            lastUpdate[c] = update;
            lastUpdateStep[c] = step;
            timers[update].ApplyAction("TriggerNow");
        }
        lastStatus[c] = c.Status;
    }
    step++;
}
