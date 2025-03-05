// DebrisExpulsionTest.cs

using UnityEngine;
using TMPro;

/// <summary>
/// Test script to verify debris expulsion mechanics are working correctly.
/// </summary>
public class DebrisExpulsionTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DebrisCollector debrisCollector;
    [SerializeField] private TMP_Text testInfoText;
    
    [Header("Test Settings")]
    [SerializeField] private bool runAutomaticTest = false;
    [SerializeField] private float testDelay = 1.0f;
    
    // Test phases
    private enum TestPhase
    {
        NotRunning,
        CollectingDebris,
        WaitingToExpelInward,
        ExpellingInward,
        WaitingToExpelOutward,
        ExpellingOutward,
        TestComplete
    }
    
    private TestPhase currentPhase = TestPhase.NotRunning;
    private float phaseTimer = 0f;
    
    private void Start()
    {
        // Find the debris collector if not set
        if (debrisCollector == null)
        {
            debrisCollector = FindFirstObjectByType<DebrisCollector>();
        }
        
        UpdateTestInfoText();
    }
    
    private void Update()
    {
        // Start automatic test with F5 key
        if (Input.GetKeyDown(KeyCode.F5))
        {
            StartTest();
        }
        
        // Manual test controls
        if (Input.GetKeyDown(KeyCode.F6))
        {
            LogTestInfo("MANUAL TEST: Press Q to test inward expulsion, E to test outward expulsion");
        }
        
        // Run the automatic test if activated
        if (runAutomaticTest && currentPhase != TestPhase.NotRunning && currentPhase != TestPhase.TestComplete)
        {
            RunTestPhase();
        }
        
        // Monitor actual expulsion to verify all debris is expelled
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E))
        {
            int beforeCount = debrisCollector.CurrentDebrisCount;
            
            // Let the collector update first
            Invoke("CheckExpulsionComplete", 0.1f);
            
            LogTestInfo($"Expulsion initiated. Debris before: {beforeCount}");
        }
    }
    
    /// Starts the automatic test sequence
    private void StartTest()
    {
        if (debrisCollector == null)
        {
            LogTestInfo("ERROR: DebrisCollector not found. Cannot run test.");
            return;
        }
        
        LogTestInfo("Starting Debris Expulsion Test...");
        currentPhase = TestPhase.CollectingDebris;
        phaseTimer = 0f;
        runAutomaticTest = true;
        
        UpdateTestInfoText();
    }
    
    /// Runs the current test phase
    private void RunTestPhase()
    {
        phaseTimer += Time.deltaTime;
        
        switch (currentPhase)
        {
            case TestPhase.CollectingDebris:
                // Wait until we have some debris to expel
                if (debrisCollector.CurrentDebrisCount > 0 || phaseTimer > testDelay * 3)
                {
                    LogTestInfo($"Debris collected: {debrisCollector.CurrentDebrisCount}");
                    currentPhase = TestPhase.WaitingToExpelInward;
                    phaseTimer = 0f;
                }
                break;
                
            case TestPhase.WaitingToExpelInward:
                // Wait a bit before testing inward expulsion
                if (phaseTimer > testDelay)
                {
                    LogTestInfo("Testing inward expulsion (toward black hole)...");
                    currentPhase = TestPhase.ExpellingInward;
                    
                    // Simulate pressing the inward expulsion key (Q)
                    SimulateKeyPress(KeyCode.Q);
                    
                    phaseTimer = 0f;
                }
                break;
                
            case TestPhase.ExpellingInward:
                // Wait for results and verify
                if (phaseTimer > testDelay)
                {
                    // Wait for more debris to collect for the next test
                    LogTestInfo("Waiting for more debris to collect...");
                    currentPhase = TestPhase.WaitingToExpelOutward;
                    phaseTimer = 0f;
                }
                break;
                
            case TestPhase.WaitingToExpelOutward:
                // Wait until we have some debris to expel
                if (debrisCollector.CurrentDebrisCount > 0 || phaseTimer > testDelay * 3)
                {
                    LogTestInfo($"Debris collected for outward test: {debrisCollector.CurrentDebrisCount}");
                    currentPhase = TestPhase.ExpellingOutward;
                    phaseTimer = 0f;
                    
                    // Simulate pressing the outward expulsion key (E)
                    SimulateKeyPress(KeyCode.E);
                }
                break;
                
            case TestPhase.ExpellingOutward:
                // Wait for results and complete test
                if (phaseTimer > testDelay)
                {
                    LogTestInfo("Debris Expulsion Test complete!");
                    currentPhase = TestPhase.TestComplete;
                    runAutomaticTest = false;
                }
                break;
        }
        
        UpdateTestInfoText();
    }
    
    /// Checks if expulsion was complete (all debris expelled)
    private void CheckExpulsionComplete()
    {
        int afterCount = debrisCollector.CurrentDebrisCount;
        LogTestInfo($"Debris after expulsion: {afterCount}");
        
        if (afterCount == 0)
        {
            LogTestInfo("✓ SUCCESS: All debris was expelled correctly.");
        }
        else
        {
            LogTestInfo("⚠ WARNING: Not all debris was expelled!");
        }
    }
    
    /// Simulates a key press for testing
    private void SimulateKeyPress(KeyCode key)
    {
        // Here we can't actually simulate input, but we can directly call the method
        // In a real game, you would use the Input system or implement a way to trigger the expulsion
        
        if (key == KeyCode.Q)
        {
            // For a proper test, we would need access to the ExpelDebris method in DebrisCollector
            // This is just a placeholder - in an actual game, you'd make ExpelDebris public or add a test hook
            LogTestInfo("Simulating inward expulsion key (Q)");
            // Invoke("CheckExpulsionComplete", 0.1f);
        }
        else if (key == KeyCode.E)
        {
            LogTestInfo("Simulating outward expulsion key (E)");
            // Invoke("CheckExpulsionComplete", 0.1f);
        }
    }
    
    /// Logs test information to console and updates UI
    private void LogTestInfo(string message)
    {
        Debug.Log($"[Expulsion Test] {message}");
        
        if (testInfoText != null)
        {
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
            testInfoText.text = $"[{timestamp}] {message}\n" + testInfoText.text;
            
            // Limit the length of the log
            if (testInfoText.text.Length > 1000)
            {
                testInfoText.text = testInfoText.text.Substring(0, 1000) + "...";
            }
        }
    }
    
    /// Updates the test info UI with current phase and status
    private void UpdateTestInfoText()
    {
        if (testInfoText == null)
            return;
            
        string phaseText = "Not Running";
        
        switch (currentPhase)
        {
            case TestPhase.CollectingDebris:
                phaseText = "Collecting Debris";
                break;
            case TestPhase.WaitingToExpelInward:
                phaseText = "Waiting to Test Inward Expulsion";
                break;
            case TestPhase.ExpellingInward:
                phaseText = "Testing Inward Expulsion";
                break;
            case TestPhase.WaitingToExpelOutward:
                phaseText = "Waiting to Test Outward Expulsion";
                break;
            case TestPhase.ExpellingOutward:
                phaseText = "Testing Outward Expulsion";
                break;
            case TestPhase.TestComplete:
                phaseText = "Test Complete";
                break;
        }
        
        // Only update the header part of the text
        string[] lines = testInfoText.text.Split('\n');
        if (lines.Length > 0)
        {
            lines[0] = $"Test Phase: {phaseText} [{phaseTimer:F1}s]";
            testInfoText.text = string.Join("\n", lines);
        }
        else
        {
            testInfoText.text = $"Test Phase: {phaseText} [{phaseTimer:F1}s]";
        }
    }
}