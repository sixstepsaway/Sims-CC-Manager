using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SimsCCManager.OptionLists
{
    public enum SimsGames
    {
        [Description("Null")]
        Null,
        [Description("The Sims")]
        Sims1,
        [Description("The Sims 2")]
        Sims2,
        [Description("The Sims 3")]
        Sims3,
        [Description("The Sims 4")]
        Sims4,
        [Description("The Sims Medieval")]
        SimsMedieval,
        [Description("Sim City 4")]
        SimCity4,
        [Description("Sim City 5")]
        SimCity5,
        [Description("Spore")]
        Spore
    }

    public static class Extensions
    {
        /// <summary>
        /// Extensions for classes and enums.
        /// </summary>
        public static string GetDescription(this Enum e)
        {
            /// <summary>
            /// Credit: https://stackoverflow.com/questions/7966102/how-to-assign-string-values-to-enums-and-use-that-value-in-a-switch
            /// </summary>
            
            var attribute =
                e.GetType()
                    .GetTypeInfo()
                    .GetMember(e.ToString())
                    .FirstOrDefault(member => member.MemberType == MemberTypes.Field)
                    .GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .SingleOrDefault()
                    as DescriptionAttribute;

            return attribute?.Description ?? e.ToString();
        }
    }
}

