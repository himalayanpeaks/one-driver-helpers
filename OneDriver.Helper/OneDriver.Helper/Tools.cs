using System;
using System.ComponentModel;
using System.Reflection;

namespace OneDriver.Toolbox
{

    public static class Tools
    {
        public static string GetDescription(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();

            return attribute == null ? value.ToString() : attribute.Description;
        }
        public static void Wait(uint aWaitTimeInMs)
        {

            uint elapsedTimeInMs = 0;
            DateTime initialTime = DateTime.Now;
            for (ushort i = 0; ; i++)
            {
                elapsedTimeInMs = (uint)(DateTime.Now - initialTime).TotalMilliseconds;
                if (elapsedTimeInMs > aWaitTimeInMs)
                    break;
                if (i >= ushort.MaxValue)
                    i = 0;
            }
        }
    }
}
