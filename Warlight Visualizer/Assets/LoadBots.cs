using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public class LoadBots : MonoBehaviour {

    public Dropdown bot1Select, bot2Select;
    public string folderName;
    public List<string> botRunnerPaths;

	void OnEnable()
    {
        bot1Select.options.Clear();
        bot2Select.options.Clear();
        botRunnerPaths.Clear();

        string[] teams = Directory.GetDirectories(folderName);
        List<string> dropdown_text = new List<string>();
        foreach(string t in teams)
        {
            List<string> teamBots = GetTeamBots(t);
            foreach(string bot in teamBots)
            {
                Dropdown.OptionData od = new Dropdown.OptionData();
                od.text = bot;

                bot1Select.options.Add(od);
                bot2Select.options.Add(od);
            }
        }

        bot1Select.value = 0;
        bot2Select.value = 0;
    }

    public List<string> GetTeamBots(string folderName)
    {
        string teamName = Path.GetFileName(folderName);
        string[] dirs = Directory.GetDirectories(folderName);
        List<string> botStrings = new List<string>();
        
        for (int i = 0; i < dirs.Length; i++)
        {
            // get the files that match the needed pattern
            string[] bot_files = Directory.GetFiles(dirs[i], "run_bot.*");
            botRunnerPaths.Add(bot_files[0]);

            if (bot_files.Length > 1) { 
                print("Warning: '" + dirs[i] + "' contains more than one run_bot file!");
            }

            // add the bot name
            if (bot_files.Length == 1) {
                botStrings.Add(teamName + ": " + Path.GetFileName(dirs[i]));
            }
        }

        return botStrings;
    }

    public string GetRunnerCmd(int selectedBotIndex)
    {
        // TODO: correctly "compile"
        string runFile = botRunnerPaths[selectedBotIndex];
        string fileExt = Path.GetExtension(runFile).ToLower();
        string runCommand = "";

        switch(fileExt)
        {
            case "py":
                runCommand = "\"python " + runFile + "\"";
                print("Python Bot");
                break;
            case "exe":
                runCommand = "\"" + runFile + "\"";
                print("Exe Bot");
                break;
            case "jar":
                runCommand = "\"java -jar " + runFile + "\"";
                print("Java Bot");
                break;
            default:
                print("Sorry, but I don't know this file type :( (" + runFile + ")");
                break;
        }

        return runCommand;
    }

    public string GetCurrentOptionB1()
    {
        if(bot1Select.value >= 0)
        {
            return GetRunnerCmd(bot1Select.value);
        }
        return "";
    }

    public string GetCurrentOptionB2()
    {
        if (bot2Select.value >= 0)
        {
            return GetRunnerCmd(bot2Select.value);
        }
        return "";
    }
}
