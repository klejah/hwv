using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;

public class LoadBots : MonoBehaviour {

    public Dropdown bot1Select, bot2Select;
    public string folderName;

	void OnEnable()
    {
        bot1Select.options.Clear();
        bot2Select.options.Clear();

        string[] dirs = Directory.GetDirectories(folderName);
        string[] files = Directory.GetFiles(folderName);

        foreach(string f in files)
        {
            Dropdown.OptionData od = new Dropdown.OptionData();
            od.text = f.Substring(folderName.Length+1);

            bot1Select.options.Add(od);
            bot2Select.options.Add(od);
        }

        foreach(string d in dirs)
        {
            string[] file = Directory.GetFiles(d);

            foreach(string f in file)
            {
                Dropdown.OptionData od = new Dropdown.OptionData();
                od.text = f.Substring(folderName.Length + 1);

                bot1Select.options.Add(od);
                bot2Select.options.Add(od);
            }
        }

        bot1Select.value = 0;
        bot2Select.value = 0;
    }

    public string GetCurrentOptionB1()
    {
        if(bot1Select.value >= 0)
        {
            return bot1Select.options[bot1Select.value].text;
        }
        return "";
    }

    public string GetCurrentOptionB2()
    {
        if (bot2Select.value >= 0)
        {
            return bot2Select.options[bot2Select.value].text;
        }
        return "";
    }
}
