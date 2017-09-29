using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameScript : MonoBehaviour {

    public GameObject gamePanel;
    public RectTransform gameBoard;
    public Image borders;
    public Image sea_brigdes;
    public GameObject region;
    public Color p1_color;
    public Color p2_color;
    public Color neutral_color;
    public Color unknown_color;
    public GameObject fogP1;
    public GameObject fogP2;
    public GameObject fogAll;
    public Text name_P1;
    public Text name_P2;
    public Text armies_P1;
    public Text armies_P2;
    public Text status_text;
    public Text round_num;

    private Timeline timeline;
    private Owner view = Owner.NEUTRAL;
    private int cur_round = 0;
    private int cur_step = 0;
    private float time = 0.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void resetGamePanel()
    {
        view = Owner.NEUTRAL;
        fogP1.SetActive(true);
        fogP2.SetActive(true);
        fogAll.SetActive(false);
        cur_round = 0;
        cur_step = 0;
        time = 0;
    }

    public void StartGame(LoadBots lb)
    {
        string cmd_p1 = lb.GetCurrentOptionB1();
        string cmd_p2 = lb.GetCurrentOptionB2();

        if (cmd_p1.Equals("") || cmd_p2.Equals(""))
        {
            Debug.LogWarning("Start game called with invalid bot command argument!");
            return;
        }

        resetGamePanel();
        gamePanel.SetActive(true);

        //TODO Load map and initialize pipeline
    }

    public void SetAll()
    {
        view = Owner.NEUTRAL;
        updateGame();
    }

    public void SetP1()
    {
        view = Owner.P1;
        updateGame();
    }

    public void SetP2()
    {
        view = Owner.P2;
        updateGame();
    }

    public void nextStep()
    {
        cur_step += 1;
        time = (float)cur_step;
        cur_round = timeline.getRound(time, view);
    }

    public void nextRound()
    {
        cur_round += 1;
        time = timeline.getTimeForRound(cur_round, view);
        cur_step = (int)time;
    }

    public void prevStep()
    {
        if (cur_step == 0)
        {
            time = 0.0f;
        }
        else
        {
            cur_step -= 1;
            time = (float)cur_step;
            cur_round = timeline.getRound(time, view);
        }
        updateGame();
    }

    public void prevRound()
    {
        if (cur_round == 0)
        {
            time = 0.0f;
            cur_step = (int)time;
        }
        else
        {
            cur_round -= 1;
            time = timeline.getTimeForRound(cur_round, view);
            cur_step = (int)time;
        }
        updateGame();
    }

    public void updateGame()
    {

    }
}

public enum Owner {
    P1, P2, NEUTRAL, UNKNOWN
}

public struct RegionState
{
    public Owner owner;
    public int units;
}

public struct State
{
    public int round;
    public int step;
    public string map;
    public string actionString;
    public List<RegionState> regions;
    public List<int> actionStat; //0 = region_idx; 1 = units placed/moved; 2 = end_region_idx; 3 = units_att_lost; 4 = units_def_lost
}

class Timeline
{
    List<State> state_all, state_p1, state_p2;

    public Timeline(string gameString)
    {
        
        state_all = new List<State>();
        state_p1 = new List<State>();
        state_p2 = new List<State>();

        //Parse game string
        string[] lines = gameString.Split("\n".ToCharArray());
        int l_idx = 0;

        while (!lines[l_idx].StartsWith("#PLAYER1"))
        {
            l_idx += 1;
        }
        l_idx += 1;

        int cur_round = 0;
        int cur_step = 0;
        string cur_action_string = "";
        while (!lines[l_idx].StartsWith("#PLAYER2"))
        {
            ProcessLine(lines, ref state_p1, ref l_idx, ref cur_round, ref cur_step, ref cur_action_string);
        }
        cur_round = 0;
        cur_step = 0;
        cur_action_string = "";
        while (!lines[l_idx].StartsWith("#FULL"))
        {
            ProcessLine(lines, ref state_p2, ref l_idx, ref cur_round, ref cur_step, ref cur_action_string);
        }
        cur_round = 0;
        cur_step = 0;
        cur_action_string = "";
        while (l_idx < lines.Length)
        {
            ProcessLine(lines, ref state_all, ref l_idx, ref cur_round, ref cur_step, ref cur_action_string);
        }
    }

    private static void ProcessLine(string[] lines, ref List<State> state_list, ref int l_idx, ref int cur_round, ref int cur_step, ref string cur_action_string)
    {
        if (lines[l_idx].StartsWith("round"))
        {
            cur_round = int.Parse(lines[l_idx].Split(" ".ToCharArray())[1]);
        }
        else if (lines[l_idx].StartsWith("player"))
        {
            cur_action_string = lines[l_idx];
        }
        else if (lines[l_idx].StartsWith("map"))
        {
            State s = new State();
            s.round = cur_round;
            s.step = cur_step;
            s.map = lines[l_idx];
            s.actionString = cur_action_string;

            List<RegionState> regions = new List<RegionState>();

            string[] map_regs = s.map.Split(" ".ToCharArray());
            for (int i = 1; i < map_regs.Length; i++)
            {
                string[] r_string = map_regs[i].Split(";".ToCharArray());
                RegionState rs = new RegionState();
                rs.owner = parse(r_string[1]);
                rs.units = int.Parse(r_string[2]);
                regions.Add(rs);
            }

            List<int> actionStat = new List<int>();

            if (s.actionString.Contains("place_armies"))
            {
                string[] as_parts = s.actionString.Split(" ".ToCharArray());
                actionStat.Add(int.Parse(as_parts[2]));
                actionStat.Add(int.Parse(as_parts[3]));
            }
            else if (s.actionString.Contains("attack"))
            {
                string[] as_parts = s.actionString.Split(" ".ToCharArray());
                actionStat.Add(int.Parse(as_parts[2]));
                actionStat.Add(int.Parse(as_parts[4]));
                actionStat.Add(int.Parse(as_parts[3]));

                if (state_list.Count == 0)
                {
                    Debug.LogWarning("This should not happen while parsing");
                }
                else
                {
                    List<RegionState> old_state = state_list[state_list.Count - 1].regions;
                    if (old_state[actionStat[2]].owner == parse(as_parts[0]))
                    {
                        //Add nothing to list
                    }
                    else
                    {
                        //calculate fallen units
                        if (old_state[actionStat[2]].owner != regions[actionStat[2]].owner)
                        {
                            //attacker lost = sent - on new region
                            actionStat.Add(actionStat[1] - regions[actionStat[2]].units);

                            //attack successfull -> all defender lost
                            actionStat.Add(old_state[actionStat[2]].units);
                        }
                        else
                        {
                            //attack not successfull -> all attacker died
                            actionStat.Add(actionStat[1]);

                            //defender lost = on old region - on new region
                            actionStat.Add(old_state[actionStat[2]].units - regions[actionStat[2]].units);
                        }
                    }
                }
            }

            s.regions = regions;
            s.actionStat = actionStat;

            state_list.Add(s);

            cur_step += 1;
            cur_action_string = "";
            l_idx += 1;
        }
    }

    public string getString(float time, Owner view)
    {
        if (view == Owner.P1)
        {
            return state_p1[(int)time].actionString;
        }
        if (view == Owner.P2)
        {
            return state_p2[(int)time].actionString;
        }
        return state_all[(int)time].actionString;
    }

    public List<RegionState> getRegionState(float time, Owner view)
    {
        if (view == Owner.P1)
        {
            return state_p1[(int)time].regions;
        }
        if (view == Owner.P2)
        {
            return state_p2[(int)time].regions;
        }
        return state_all[(int)time].regions;
    }

    public List<int> getActionStat(float time, Owner view)
    {
        if (view == Owner.P1)
        {
            return state_p1[(int)time].actionStat;
        }
        if (view == Owner.P2)
        {
            return state_p2[(int)time].actionStat;
        }
        return state_all[(int)time].actionStat;
    }

    public static Owner parse(string s)
    {
        if (s.Equals("player1"))
        {
            return Owner.P1;
        }
        if (s.Equals("player2"))
        {
            return Owner.P2;
        }
        if (s.Equals("neutral"))
        {
            return Owner.NEUTRAL;
        }
        if (s.Equals("unknown"))
        {
            return Owner.UNKNOWN;
        }
        Debug.LogWarning("Parse called with " + s);
        return Owner.UNKNOWN;
    }

    public float getTimeForRound(int round, Owner view)
    {
        List<State> states;
        if (view == Owner.P1)
        {
            states = state_p1;
        }
        if (view == Owner.P2)
        {
            states = state_p2;
        }
        else 
        {
            states = state_all;
        }

        int last_round = 0;
        for (int i = 0; i < states.Count; i++ )
        {
            if (states[i].round == round)
            {
                return (float)i;
            }
            else
            {
                last_round = states[i].round;
            }
        }
        return (float)last_round;
    }

    public int getRound(float time, Owner view)
    {
        int t_idx = (int)time;
        if (view == Owner.P1)
        {
            if (t_idx >= state_p1.Count)
            {
                return state_p1[state_p1.Count - 1].round;
            }
            return state_p1[t_idx].round;
        }
        if (view == Owner.P2)
        {
            if (t_idx >= state_p2.Count)
            {
                return state_p2[state_p2.Count - 1].round;
            }
            return state_p2[t_idx].round;
        }
        if (t_idx >= state_all.Count)
        {
            return state_all[state_all.Count - 1].round;
        }
        return state_all[t_idx].round;
    }
}
