using Mono.Cecil.Cil;
using UnityEngine;

public static class LSystemCreator
{
    public static string[] L_systems;

    public static string create_tree_Lsystem(int depth)
    {
        string l_system = "F[&F][/&F][\\&F]";
        string target = "F";
        string rule = "[&F][/&F][\\&F]";

        char not_inner = '[';

        for (int i = 0; i < depth; i++)
        {
            string next = "";
            for(int t = 0; t < l_system.Length; t++)
            {
                
                if (t < l_system.Length - 1 && l_system[t] == target[0] && l_system[t+1] != not_inner)
                {
                    next += target + rule;
                }
                else { next += l_system[t]; }
            }
            l_system = next;
        }
        
        return l_system;
    }
}
