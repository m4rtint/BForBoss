using System;
using UnityEngine;

namespace BForBoss
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class Resolve : PropertyAttribute
    {
        public bool IncludeInactive = false;
        
        /// <summary>
        /// Add Component Resolver for Field
        /// </summary>
        public Resolve()
        {
        }

        /// <summary>
        /// Add Component Resolver for Field
        /// </summary>
        /// <param name="includeInactive">
        /// Include components of Inactive GameObjects for relevant searches
        /// </param>
        public Resolve(bool includeInactive)
        {
            IncludeInactive = includeInactive;
        }
    }
}
