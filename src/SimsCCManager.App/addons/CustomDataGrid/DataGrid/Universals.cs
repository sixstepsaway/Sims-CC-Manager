using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace CustomDataGridUniversals
{
    public class UniversalMethods
    {
        /// <summary>
        /// Assorted methods used across the entire program, such as "Is even?"
        /// </summary>
        
        public static bool IsEven(int val)
        {
            if ((val & 0x1) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}